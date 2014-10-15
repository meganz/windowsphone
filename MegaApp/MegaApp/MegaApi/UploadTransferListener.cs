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
    class UploadTransferListener : MTransferListenerInterface
    {
        private readonly TransferObjectModel _transferObject;
        private MTransfer _currentTransfer;
        private MegaSDK _api;

        public UploadTransferListener(TransferObjectModel transferObject)
        {
            _transferObject = transferObject;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _transferObject.TotalBytes = transfer.getTotalBytes();
                _transferObject.TransferedBytes = transfer.getTransferredBytes();
                _transferObject.IsBusy = false;
                //_transferObject.IsNotTransferring = true;
            });
            
            if (e.getErrorCode() == MErrorType.API_OK)
            {
                
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
                _transferObject.IsBusy = true;
                _transferObject.TotalBytes = transfer.getTotalBytes();
                _transferObject.TransferedBytes = transfer.getTransferredBytes();
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
                 _transferObject.TotalBytes = transfer.getTotalBytes();
                 _transferObject.TransferedBytes = transfer.getTransferredBytes();

                 if (_transferObject.TransferedBytes > 0)
                     _transferObject.IsNotTransferring = false;
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
