using System;
using System.Windows;
using System.Windows.Threading;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class FastLoginRequestListener: BaseRequestListener
    {
        private readonly MainPageViewModel _mainPageViewModel;
        
        public FastLoginRequestListener(MainPageViewModel mainPageViewModel)
        {
            _mainPageViewModel = mainPageViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.FastLogin; }
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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if (_mainPageViewModel.AppInformation.IsStartedAsAutoUpload)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.AutoCameraUpload);
                });
            }

            _mainPageViewModel.GetAccountDetails();
            _mainPageViewModel.FetchNodes();            
        }

        #endregion
    }
}
