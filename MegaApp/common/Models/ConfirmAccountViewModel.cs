using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
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
            if (String.IsNullOrEmpty(ConfirmCode))
                return;
            else
            {
                if (String.IsNullOrEmpty(Password))
                    MessageBox.Show(AppMessages.RequiredFieldsConfirmAccount, AppMessages.RequiredFields_Title,
                        MessageBoxButton.OK);
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
            get { return ProgressMessages.ConfirmAccount; }
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
            MRequestType type = request.getType();
            MErrorType error = e.getErrorCode();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
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
                                if (ShowSuccesMessage)
                                    MessageBox.Show(SuccessMessage, SuccessMessageTitle, MessageBoxButton.OK);

                                OnSuccesAction(request);
                            }
                            break;
                        }

                    case MErrorType.API_ENOENT: //Useful errors for users
                        {
                            //Already confirmed account
                            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
                            {
                                MessageBox.Show(AppMessages.AlreadyConfirmedAccount, AppMessages.AlreadyConfirmedAccount_Title,
                                    MessageBoxButton.OK);

                                NavigateService.NavigateTo(NavigateToPage, NavigationParameter);
                            }                                

                            //Wrong password
                            if (request.getType() == MRequestType.TYPE_CONFIRM_ACCOUNT)
                                MessageBox.Show(AppMessages.WrongPassword, AppMessages.WrongPassword_Title,
                                    MessageBoxButton.OK);
                            
                            break;
                        }
                    
                    case MErrorType.API_EOVERQUOTA:
                        base.onRequestFinish(api, request, e);
                        break;

                    default: //Other error
                        {
                            MessageBox.Show(String.Format(ErrorMessage, e.getErrorString()), ErrorMessageTitle,
                                MessageBoxButton.OK);
                            break;
                        }

                }
            });            
        }

        protected override void OnSuccesAction(MRequest request)
        {
            bool isAlreadyOnline = Convert.ToBoolean(_megaSdk.isLoggedIn());
                        
            if (isAlreadyOnline)
                _megaSdk.logout(this);
            
            _megaSdk.login(request.getEmail(), request.getPassword(), new LoginViewModel(_megaSdk));            
        }

        #endregion
    }
}
