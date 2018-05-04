using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
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

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);

                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
            });

            if (e.getErrorCode() != MErrorType.API_OK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_ENOENT: // E-mail unassociated with a MEGA account or Wrong password
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle, AppMessages.WrongEmailPasswordLogin,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;

                    case MErrorType.API_ETOOMANY: // Too many failed login attempts
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle, AppMessages.AM_TooManyFailedLoginAttempts,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;

                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle, AppMessages.AM_AccountNotConfirmed,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;
                }
            }            

            base.onRequestFinish(api, request, e);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // If is the first error/retry (timer is not running) start the timer
                if (apiErrorTimer != null && !apiErrorTimer.IsEnabled)
                    apiErrorTimer.Start();
            });

            base.onRequestTemporaryError(api, request, e);
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mainPageViewModel.FetchNodes();

                // Validate product subscription license on background thread
                Task.Run(() => LicenseService.ValidateLicenses());
            });
        }

        #endregion
    }
}
