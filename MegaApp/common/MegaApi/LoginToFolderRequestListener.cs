using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class LoginToFolderRequestListener : BaseRequestListener
    {
        private readonly FolderLinkViewModel _folderLinkViewModel;

        public LoginToFolderRequestListener(FolderLinkViewModel folderLinkViewModel)
        {
            _folderLinkViewModel = folderLinkViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_Login; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.LoginFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.LoginFailed_Title.ToUpper(); }
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
                OnSuccesAction(api, request);
            }
            else
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_EARGS:
                        break;

                    case MErrorType.API_EINCOMPLETE:
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
