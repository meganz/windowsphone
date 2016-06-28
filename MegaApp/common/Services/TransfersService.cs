using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using mega;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;

namespace MegaApp.Services
{
    static class TransfersService
    {
        /// <summary>
        /// Update the transfers list.
        /// </summary>
        public static void UpdateMegaTransfersList()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Remove the transfer listeners and clean the transfers list.
                foreach (var megaTransfer in App.MegaTransfers)
                    App.MegaSdk.removeTransferListener(megaTransfer);

                App.MegaTransfers.Clear();

                // Get transfers and fill the transfers list again.
                var transfers = App.MegaSdk.getTransfers();
                for (int i = 0; i < transfers.size(); i++)
                {
                    var transfer = transfers.get(i);
                    TransferObjectModel megaTransfer;
                    if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
                    {
                        megaTransfer = new TransferObjectModel(App.MegaSdk,
                            NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.MegaSdk.getNodeByHandle(transfer.getNodeHandle()), ContainerType.CloudDrive),
                            TransferType.Download, transfer.getPath(), transfer.getAppData());
                    }
                    else
                    {
                        megaTransfer = new TransferObjectModel(App.MegaSdk, App.MainPageViewModel.CloudDrive.FolderRootNode,
                            TransferType.Upload, transfer.getPath(), transfer.getAppData());
                    }

                    App.MegaTransfers.Add(megaTransfer);
                    App.MegaSdk.addTransferListener(megaTransfer);
                }
            });            
        }

        /// <summary>
        /// Cancel all the pending offline transfer of a node and wait until all transfers are canceled.
        /// </summary>
        /// <param name="nodePath">Path of the node.</param>
        public static void CancelPendingNodeOfflineTransfers(String nodePath)
        {
            foreach (var item in App.MegaTransfers.Downloads)
            {
                var transferItem = (TransferObjectModel)item;
                if (transferItem == null || transferItem.Transfer == null) continue;

                WaitHandle waitEventRequestTransfer = new AutoResetEvent(false);
                if (String.Compare(nodePath, transferItem.Transfer.getPath()) == 0 &&
                    transferItem.IsAliveTransfer())
                {
                    App.MegaSdk.cancelTransfer(transferItem.Transfer,
                        new CancelTransferRequestListener((AutoResetEvent)waitEventRequestTransfer));
                    waitEventRequestTransfer.WaitOne();
                }
            }
        }
    }
}
