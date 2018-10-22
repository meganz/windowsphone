using System;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class GetPublicNodeRequestListener: PublicLinkRequestListener
    {
        private readonly FolderViewModel _folderViewModel;
        
        public GetPublicNodeRequestListener(FolderViewModel folderViewModel)
        {
            this._folderViewModel = folderViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_OpenFileLink; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.AM_OpenLinkFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.AM_OpenLinkFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                //If getFlag() returns true, the file link key is invalid.
                if (request.getFlag())
                {
                    if (_decryptionAlert)
                        ShowDecryptionKeyNotValidAlert(api, request);
                    else
                        ShowFileLinkNoValidAlert();
                }
                else
                {
                    OnSuccesAction(api, request);
                }    
            }
            else
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_EARGS:
                        if (_decryptionAlert)
                            ShowDecryptionKeyNotValidAlert(api, request);
                        else
                            ShowFileLinkNoValidAlert();
                        break;

                    case MErrorType.API_ETOOMANY:       // Taken down link and the link owner's account is blocked
                        ShowAssociatedUserAccountTerminatedFileLinkAlert();
                        break;

                    case MErrorType.API_ENOENT:         // Link not exists or has been deleted by user
                    case MErrorType.API_EBLOCKED:       // Taken down link
                        ShowUnavailableFileLinkAlert();
                        break;

                    case MErrorType.API_EINCOMPLETE:    // Link has not decryption key
                        ShowDecryptionAlert(api, request);
                        break;

                    default:
                        ShowFileLinkNoValidAlert();
                        break;
                }
            }
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            App.LinkInformation.ActiveLink = request.getLink();
            MNode publicNode = App.LinkInformation.PublicNode = request.getPublicMegaNode();

            if (publicNode != null)
            {
                // Save the handle of the last public node accessed (Task #10800)
                SettingsService.SaveLastPublicNodeHandle(publicNode.getHandle());

                #if WINDOWS_PHONE_80
                // Detect if is an image to allow directly download to camera albums
                bool isImage = false;
                if (publicNode.isFile())
                {
                    FileNodeViewModel node = new FileNodeViewModel(api, null, publicNode, ContainerType.PublicLink);
                    isImage = node.IsImage;
                }
                #endif                
                
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        #if WINDOWS_PHONE_80
                        DialogService.ShowOpenLink(publicNode, _folderViewModel, isImage);
                        #elif WINDOWS_PHONE_81
                        DialogService.ShowOpenLink(publicNode, _folderViewModel);
                        #endif
                    }
                    catch (Exception e)
                    {
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            String.Format(ErrorMessage, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    }
                });
            }                
            else
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        ErrorMessageTitle,
                        AppMessages.AM_ImportFileFailedNoErrorCode,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
        }

        #endregion
    }
}
