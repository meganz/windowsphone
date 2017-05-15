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
                        megaTransfer.TransferState = MTransferState.STATE_NONE;
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
        /// Add a <see cref="MTransfer"/> to the corresponding transfers list if it is not already included.
        /// </summary>
        /// <param name="megaTransfers"><see cref="TransferQueue"/> which contains the transfers list(s).</param>
        /// <param name="transfer"><see cref="MTransfer"/> to be added to the corresponding transfer list.</param>
        /// <returns>The <see cref="TransferObjectModel"/> corresponding to the <see cref="MTransfer"/>.</returns>
        public static TransferObjectModel AddTransferToList(TransferQueue megaTransfers, MTransfer transfer)
        {
            // Folder transfers are not included into the transfers list.
            if (transfer != null || transfer.isFolderTransfer() == true) return null;

            // Search if the transfer already exists into the transfers list.
            var megaTransfer = SearchTransfer(megaTransfers.SelectAll(), transfer);
            if (megaTransfer != null) return megaTransfer;

            // If doesn't exist create a new one and add it to the transfers list
            megaTransfer = CreateTransferObjectModel(transfer);            
            if (megaTransfer != null)                
                megaTransfers.Add(megaTransfer);

            return megaTransfer;
        }

        /// <summary>
        /// Search into a transfers list the <see cref="TransferObjectModel"/> corresponding to a <see cref="MTransfer"/>.
        /// </summary>
        /// <param name="transfersList">Transfers list where search the transfer.</param>
        /// <param name="transfer">Transfer to search.</param>
        /// <returns>The transfer object if exists or NULL in other case.</returns>
        public static TransferObjectModel SearchTransfer(IList<TransferObjectModel> transfersList, MTransfer transfer)
        {
            // Folder transfers are not included into the transfers list.
            if (transfer == null || transfer.isFolderTransfer()) return null;

            var megaTransfer = transfersList.FirstOrDefault(
                t => (t.Transfer != null && t.Transfer.getTag() == transfer.getTag()) ||
                t.TransferPath.Equals(transfer.getPath()));

            return megaTransfer;
        }

        /// <summary>
        /// Create a <see cref="TransferObjectModel"/> from a <see cref="MTransfer"/>.
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns>The new <see cref="TransferObjectModel"/></returns>
        public static TransferObjectModel CreateTransferObjectModel(MTransfer transfer)
        {
            if (transfer == null) return null;

            try
            {
                TransferObjectModel megaTransfer = null;

                switch (transfer.getType())
                {
                    case MTransferType.TYPE_DOWNLOAD:
                        MNode node = transfer.getPublicMegaNode() ?? // If is a public node
                            SdkService.MegaSdk.getNodeByHandle(transfer.getNodeHandle()); // If not

                        if (node == null) return null;

                        megaTransfer = new TransferObjectModel(SdkService.MegaSdk,
                            NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, node, ContainerType.CloudDrive),
                            MTransferType.TYPE_DOWNLOAD, transfer.getPath());
                        break;

                    case MTransferType.TYPE_UPLOAD:
                        var parentNode = SdkService.MegaSdk.getNodeByHandle(transfer.getParentHandle());

                        if (parentNode == null) return null;

                        megaTransfer = new TransferObjectModel(SdkService.MegaSdk,
                            NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, parentNode, ContainerType.CloudDrive),
                            MTransferType.TYPE_UPLOAD, transfer.getPath());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (megaTransfer != null)
                {
                    GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.Transfer = transfer;
                    megaTransfer.TransferState = transfer.getState();
                    megaTransfer.TransferPriority = transfer.getPriority();
                    megaTransfer.IsBusy = false;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = string.Empty;
                    megaTransfer.TransferMeanSpeed = 0;

                    megaTransfer.TransferState = !SdkService.MegaSdk.areTransfersPaused((int)transfer.getType())
                        ? MTransferState.STATE_QUEUED : MTransferState.STATE_PAUSED;
                }

                return megaTransfer;
            }
            catch (Exception) { return null; }
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
