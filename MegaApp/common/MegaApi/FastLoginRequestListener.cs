using System;
using System.Threading.Tasks;
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

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;
        
        public FastLoginRequestListener(MainPageViewModel mainPageViewModel)
        {
            _mainPageViewModel = mainPageViewModel;

            timerAPI_EAGAIN = new DispatcherTimer();
            timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
            timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);            
        }

        // Method which is call when the timer event is triggered
        private void timerTickAPI_EAGAIN(object sender, object e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                timerAPI_EAGAIN.Stop();
                ProgressService.SetProgressIndicator(true, ProgressMessages.ServersTooBusy);
            });
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

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => timerAPI_EAGAIN.Stop());
            base.onRequestFinish(api, request, e);
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;
            base.onRequestStart(api, request);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                Deployment.Current.Dispatcher.BeginInvoke(() => timerAPI_EAGAIN.Start());                
            }

            base.onRequestTemporaryError(api, request, e);
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mainPageViewModel.GetAccountDetails();
                _mainPageViewModel.FetchNodes();

                // Validate product subscription license on background thread
                Task.Run(() => LicenseService.ValidateLicenses());
            });
        }

        #endregion
    }
}
