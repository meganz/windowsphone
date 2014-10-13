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
        private MTransfer _currentTransfer;
        private MegaSDK _api;

        public DownloadTransferListener(NodeViewModel node)
        {
            _node = node;
            _currentTransfer = null;
            _api = null;
            _node.CancelingTransfer += NodeOnCancelingTransfer;
        }

        private void NodeOnCancelingTransfer(object sender, EventArgs eventArgs)
        {
            if (_currentTransfer == null || _api == null) return;
            _api.cancelTransfer((_currentTransfer));
                
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();
                _node.IsBusy = false;
                _node.IsNotTransferring = true;
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => _node.LoadImage(_node.ImagePath));
            }
            else if(e.getErrorCode() != MErrorType.API_EINCOMPLETE)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => 
                    MessageBox.Show(String.Format(AppMessages.DownloadNodeFailed, e.getErrorString()),
                    AppMessages.DownloadNodeFailed_Title, MessageBoxButton.OK));
            }
            
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            _currentTransfer = transfer;
            _api = api;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _node.IsBusy = true;
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            //throw new NotImplementedException();
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {

             Deployment.Current.Dispatcher.BeginInvoke(() =>
             {
                _node.TotalBytes = transfer.getTotalBytes();
                _node.TransferedBytes = transfer.getTransferredBytes();

                 if(_node.TransferedBytes > 0)
                     _node.IsNotTransferring = false;
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
