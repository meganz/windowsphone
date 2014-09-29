using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using mega;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class DownloadTransferListener : MTransferListenerInterface
    {
        private readonly NodeViewModel _node;
        public DownloadTransferListener(NodeViewModel node)
        {
            _node = node;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
             Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();
                _node.IsBusy = false;
                _node.LoadImage(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,transfer.getFileName()));
            });
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _node.IsBusy = true;
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            throw new NotImplementedException();
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
             Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();
            });
        }

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public virtual bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }
    }
}
