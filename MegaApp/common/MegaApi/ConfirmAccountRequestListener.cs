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

                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: //Request finish successfully
                        {
                            //Valid and operative confirmation link
                            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                            {
                                this._confirmAccountViewModel.Email = request.getEmail();
                            }

                            //Successfull confirmation process
                            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                            {
                                var customMessageDialog = new CustomMessageDialog(
                                    SuccessMessageTitle, SuccessMessage,
                                    App.AppInformation, MessageDialogButtons.Ok);

                                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                                    OnSuccesAction(api, request);

                                customMessageDialog.ShowDialog();
                            }
                            break;
                        }

                    case MErrorType.API_ENOENT: //Useful errors for users
                        {
                            //Already confirmed account
                            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                            {
                                var customMessageDialog = new CustomMessageDialog(
                                    AppMessages.AlreadyConfirmedAccount_Title,
                                    AppMessages.AlreadyConfirmedAccount,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok);

                                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                                    NavigateService.NavigateTo(NavigateToPage, NavigationParameter);

                                customMessageDialog.ShowDialog();
                            }

                            //Wrong password
                            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                            {
                                new CustomMessageDialog(
                                    AppMessages.WrongPassword_Title,
                                    AppMessages.WrongPassword,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            }

                            break;
                        }

                    case MErrorType.API_EOVERQUOTA:
                        base.onRequestFinish(api, request, e);
                        break;

                    default: //Other error
                        {
                            new CustomMessageDialog(
                                ErrorMessageTitle,
                                String.Format(ErrorMessage, e.getErrorString()),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
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

        #endregion
    }
}
