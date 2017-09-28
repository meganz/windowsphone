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
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class ConfirmAccountRequestListener : BaseRequestListener
    {
        private readonly ConfirmAccountViewModel _confirmAccountViewModel;

        public ConfirmAccountRequestListener(ConfirmAccountViewModel confirmAccountViewModel)
        {
            _confirmAccountViewModel = confirmAccountViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_ConfirmAccount; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.ConfirmAccountFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.ConfirmAccountFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.ConfirmAccountSucces; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.ConfirmAccountSucces_Title; }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
        }

        protected override Type NavigateToPage
        {
            get { return typeof(LoginPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Normal; }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                this._confirmAccountViewModel.ControlState = false);

            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                base.onRequestStart(api, request);
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
                this._confirmAccountViewModel.ControlState = true;

                if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                {
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_OK: // Valid and operative confirmation link
                            this._confirmAccountViewModel.Email = request.getEmail();
                            break;

                        case MErrorType.API_ENOENT: // Already confirmed account
                            ShowErrorMesageAndNavigate(AppMessages.AlreadyConfirmedAccount_Title,
                                AppMessages.AlreadyConfirmedAccount);
                            break;

                        case MErrorType.API_EINCOMPLETE: // Incomplete confirmation link
                            ShowErrorMesageAndNavigate(AppMessages.ConfirmAccountFailed_Title,
                                AppMessages.AM_IncompleteConfirmationLink);
                            break;

                        case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                        case MErrorType.API_EOVERQUOTA: // Storage overquota error
                            base.onRequestFinish(api, request, e);
                            break;

                        default: // Other error
                            ShowDefaultErrorMessage(e);
                            break;
                    }
                }
                else if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                {
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_OK: // Successfull confirmation process
                            var customMessageDialog = new CustomMessageDialog(
                                SuccessMessageTitle, SuccessMessage,
                                App.AppInformation, MessageDialogButtons.Ok);
                            
                            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                                OnSuccesAction(api, request);
                            
                                customMessageDialog.ShowDialog();
                            break;

                        case MErrorType.API_ENOENT: // Wrong password
                            new CustomMessageDialog(
                                AppMessages.WrongPassword_Title,
                                AppMessages.WrongPassword,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                            break;

                        case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                        case MErrorType.API_EOVERQUOTA: // Storage overquota error
                            base.onRequestFinish(api, request, e);
                            break;

                        default: // Other error
                            ShowDefaultErrorMessage(e);
                            break;
                    }
                }
            });
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if (Convert.ToBoolean(api.isLoggedIn()))
                api.logout(new LogOutRequestListener(false));

            App.AppInformation.IsNewlyActivatedAccount = true;

            api.login(request.getEmail(), request.getPassword(),
                new LoginRequestListener(new LoginViewModel(api)));
        }

        private void ShowErrorMesageAndNavigate(String title, String message)
        {
            var customMessageDialog = new CustomMessageDialog(
                title, message, App.AppInformation, MessageDialogButtons.Ok);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                NavigateService.NavigateTo(NavigateToPage, NavigationParameter);

            customMessageDialog.ShowDialog();
        }

        private void ShowDefaultErrorMessage(MError e)
        {
            new CustomMessageDialog(ErrorMessageTitle,
                String.Format(ErrorMessage, e.getErrorString()),
                App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
        }

        #endregion
    }
}
