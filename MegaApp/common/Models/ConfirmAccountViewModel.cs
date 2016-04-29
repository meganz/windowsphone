using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class ConfirmAccountViewModel: BaseRequestListenerViewModel
    {
        private readonly MegaSDK _megaSdk;
        private readonly ConfirmAccountPage _confirmAccountPage;

        public ConfirmAccountViewModel(MegaSDK megaSdk, ConfirmAccountPage confirmAccountPage)
        {
            this.ControlState = true;
            this._megaSdk = megaSdk;
            this._confirmAccountPage = confirmAccountPage;
            this.ConfirmAccountCommand = new DelegateCommand(this.ConfirmAccount);
        }

        #region Methods

        private void ConfirmAccount(object obj)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (String.IsNullOrEmpty(ConfirmCode))
                return;
            else
            {
                if (String.IsNullOrEmpty(Password))
                {
                    new CustomMessageDialog(
                        AppMessages.RequiredFields_Title,
                        AppMessages.RequiredFieldsConfirmAccount,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
                else
                {
                    this._megaSdk.confirmAccount(ConfirmCode, Password, this);
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ConfirmAccountCommand { get; set; }

        #endregion

        #region Properties

        public string ConfirmCode { get; set; }
        public string Password { get; set; }

        #endregion

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_ConfirmAccount; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.ConfirmAccountFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.ConfirmAccountFailed_Title; }
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

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
                this.ControlState = true;
                
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: //Request finish successfully
                        {
                            //Valid and operative confirmation link
                            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                            {
                                this._confirmAccountPage.txtEmail.Text = request.getEmail();                                
                            }

                            //Successfull confirmation process
                            if(request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                            {
                                var customMessageDialog = new CustomMessageDialog(
                                    SuccessMessageTitle, SuccessMessage,
                                    App.AppInformation, MessageDialogButtons.Ok);

                                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                                    OnSuccesAction(request);
                                
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

        protected override void OnSuccesAction(MRequest request)
        {
            if (Convert.ToBoolean(_megaSdk.isLoggedIn()))
                _megaSdk.logout(new LogOutRequestListener(false));

            App.AppInformation.IsNewlyActivatedAccount = true;

            _megaSdk.login(request.getEmail(), request.getPassword(), 
                new LoginRequestListener(new LoginViewModel(_megaSdk)));
        }

        #endregion
    }
}
