using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    abstract class BaseTransferListener: MTransferListenerInterface
    {
        #region Properties

        #endregion

        public virtual void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                   
                }
               
            });
        }

        public virtual void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
               

            });
        }

        public virtual void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            
        }

        public virtual void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // No update status necessary
        }

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public virtual bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        #region Virtual Methods

        protected virtual void OnSuccesAction(MTransfer transfer)
        {
            // No standard succes action
        }

        #endregion
    }
}
