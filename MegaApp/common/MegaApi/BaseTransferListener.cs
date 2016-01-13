using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
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
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));

            switch(e.getErrorCode())
            {
                case MErrorType.API_OK:
                    break;

                case MErrorType.API_EOVERQUOTA:

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // Stop all upload transfers
                        if (App.MegaTransfers.Count > 0)
                        {
                            foreach (var item in App.MegaTransfers)
                            {
                                var transferItem = (TransferObjectModel)item;
                                if (transferItem == null) continue;

                                if (transferItem.Type == TransferType.Upload)
                                    transferItem.CancelTransfer();
                            }
                        }

                        // Disable the "camera upload" service
                        MediaService.SetAutoCameraUpload(false);
                        SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);

                        DialogService.ShowOverquotaAlert();
                    });
                    break;
            }
        }

        public virtual void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
               

            });
        }

        public virtual void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            if (DebugService.DebugSettings.IsDebugMode || Debugger.IsAttached)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));
            }            
        }

        public virtual void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));
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
