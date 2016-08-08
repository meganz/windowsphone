using System;
using System.Collections.Generic;
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
    class LoginToFolderRequestListener : PublicLinkRequestListener
    {
        public LoginToFolderRequestListener(FolderLinkViewModel folderLinkViewModel)
            : base(folderLinkViewModel)
        {
            
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_OpenFolderLink; }
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
            get { return AppMessages.AM_OpenLinkFailed_Title.ToUpper(); }
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

            // If folder link is well structured
            if (e.getErrorCode() == MErrorType.API_OK)
            {
                OnSuccesAction(api, request);
            }
            else
            {
                // Set the empty state and disable the app bar buttons
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _folderLinkViewModel.FolderLink.SetEmptyContentTemplate(false);
                    _folderLinkViewModel._folderLinkPage.SetApplicationBarData(false);
                });

                switch(e.getErrorCode())
                {
                    case MErrorType.API_EARGS:
                        if (_decryptionAlert)
                            ShowDecryptionKeyNotValidAlert(api, request); //If the user have written the key
                        else
                            ShowFolderLinkNoValidAlert(); //Handle length or Key length no valid
                        break;

                    case MErrorType.API_EINCOMPLETE: //Link has not decryption key
                        ShowDecryptionAlert(api, request);
                        break;
                }
            }
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                _folderLinkViewModel.FetchNodes());
        }

        #endregion
    }
}
