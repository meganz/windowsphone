using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.Models;

namespace MegaApp.Services
{
    static class TransfersService
    {
        /// <summary>
        /// Global transfers queue
        /// </summary>
        private static TransferQueue _megaTransfers;
        public static TransferQueue MegaTransfers
        {
            get
            {
                if (_megaTransfers != null) return _megaTransfers;
                _megaTransfers = new TransferQueue();
                return _megaTransfers;
            }
        }

        /// <summary>
        /// Global transfer listener
        /// </summary>
        private static GlobalTransferListener _globalTransferListener;
        public static GlobalTransferListener GlobalTransferListener
        {
            get
            {
                if (_globalTransferListener != null) return _globalTransferListener;
                _globalTransferListener = new GlobalTransferListener();
                return _globalTransferListener;
            }
        }

        #region Public Methods

        /// <summary>
        /// Update the transfers list/queue.
        /// </summary>
        /// <param name="MegaTransfers">Transfers list/queue to update.</param>
        public static void UpdateMegaTransfersList(TransferQueue MegaTransfers)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MegaTransfers.Clear();
                MegaTransfers.Downloads.Clear();
                MegaTransfers.Uploads.Clear();
            });

            TransfersService.GlobalTransferListener.Transfers.Clear();
            
            // Get transfers and fill the transfers list again.
            var transfers = SdkService.MegaSdk.getTransfers();
            var numTransfers = transfers.size();
            for (int i = 0; i < numTransfers; i++)
            {
                var transfer = transfers.get(i);

                TransferObjectModel megaTransfer = null;
                if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
                {
                    // If is a public node
                    MNode node = transfer.getPublicMegaNode();
                    if (node == null) // If not
                        node = SdkService.MegaSdk.getNodeByHandle(transfer.getNodeHandle());

                    if (node != null)
                    {
                        megaTransfer = new TransferObjectModel(SdkService.MegaSdk,
                            NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, node, ContainerType.CloudDrive),
                            MTransferType.TYPE_DOWNLOAD, transfer.getPath());
                    }
                }
                else
                {
                    megaTransfer = new TransferObjectModel(SdkService.MegaSdk, App.MainPageViewModel.CloudDrive.FolderRootNode,
                        MTransferType.TYPE_UPLOAD, transfer.getPath());
                }

                if(megaTransfer != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        GetTransferAppData(transfer, megaTransfer);

                        megaTransfer.Transfer = transfer;
                        megaTransfer.Status = TransferStatus.Queued;
                        megaTransfer.CancelButtonState = true;
                        megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                        megaTransfer.TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
                        megaTransfer.IsBusy = true;
                        megaTransfer.TotalBytes = transfer.getTotalBytes();
                        megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                        megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();

                        MegaTransfers.Add(megaTransfer);
                        TransfersService.GlobalTransferListener.Transfers.Add(megaTransfer);
                    });                    
                }
            }
        }

        /// <summary>
        /// Get the transfer "AppData" (substrings separated by '#')
        /// <para>- Substring 1: Boolean value to indicate if the download is for Save For Offline (SFO).</para>
        /// <para>- Substring 2: String which contains the download folder path external to the app sandbox cache.</para>
        /// </summary>
        /// <param name="transfer">MEGA SDK transfer to obtain the "AppData".</param>
        /// <param name="megaTransfer">App transfer object to be displayed.</param>
        /// <returns>Boolean value indicating if all was good.</returns>
        public static bool GetTransferAppData(MTransfer transfer, TransferObjectModel megaTransfer)
        {
            // Default values
            megaTransfer.IsSaveForOfflineTransfer = false;
            megaTransfer.ExternalDownloadPath = null;

            // Only the downloads can contain app data
            if (transfer.getType() != MTransferType.TYPE_DOWNLOAD)
                return false;

            // Get the transfer "AppData"
            String transferAppData = transfer.getAppData();
            if (String.IsNullOrWhiteSpace(transferAppData))
                return false;      

            // Split the string into the substrings separated by '#'
            string[] splittedAppData = transferAppData.Split("#".ToCharArray(), 2);
            if(splittedAppData.Count() < 1)
                return false;

            // Set the corresponding values
            megaTransfer.IsSaveForOfflineTransfer = Convert.ToBoolean(splittedAppData[0]);

            if(splittedAppData.Count() >= 2)
                megaTransfer.ExternalDownloadPath = splittedAppData[1];

            return true;
        }

        /// <summary>
        /// Create the transfer "AppData" string (substrings separated by '#')
        /// - Substring 1: Boolean value to indicate if the download is for Save For Offline (SFO).
        /// - Substring 2: String which contains the download folder path external to the app sandbox cache.
        /// </summary>
        /// <returns>"AppData" string (substrings separated by '#')</returns>
        public static String CreateTransferAppDataString(bool isSaveForOfflineTransfer = false,
            String downloadFolderPath = null)
        {
            return String.Concat(isSaveForOfflineTransfer.ToString(), "#", downloadFolderPath);
        }

        /// <summary>
        /// Cancel all the pending offline transfer of a node and wait until all transfers are canceled.
        /// </summary>
        /// <param name="nodePath">Path of the node.</param>
        /// <param name="isFolder">Boolean value which indicates if the node is a folder or not.</param>
        public static void CancelPendingNodeOfflineTransfers(String nodePath, bool isFolder)
        {
            var megaTransfers = SdkService.MegaSdk.getTransfers(MTransferType.TYPE_DOWNLOAD);
            var numMegaTransfers = megaTransfers.size();

            for (int i = 0; i < numMegaTransfers; i++)
            {
                var transfer = megaTransfers.get(i);
                if (transfer == null) continue;
                
                String transferPathToCompare;
                if (isFolder)
                    transferPathToCompare = transfer.getParentPath();
                else
                    transferPathToCompare = transfer.getPath();
                                
                WaitHandle waitEventRequestTransfer = new AutoResetEvent(false);
                if (String.Compare(nodePath, transferPathToCompare) == 0)
                {
                    SdkService.MegaSdk.cancelTransfer(transfer, 
                        new CancelTransferRequestListener((AutoResetEvent)waitEventRequestTransfer));
                    waitEventRequestTransfer.WaitOne();
                }
            }
        }

        #endregion

    }
}
