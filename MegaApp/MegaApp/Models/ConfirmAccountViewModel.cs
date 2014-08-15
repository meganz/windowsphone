using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class ConfirmAccountViewModel: BaseViewModel, MRequestListenerInterface
    {
        private readonly MegaSDK _megaSdk;

        public ConfirmAccountViewModel(MegaSDK megaSdk)
        {
            this.ControlState = true;
            this._megaSdk = megaSdk;
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

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
              
                this.ControlState = true;

                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    MessageBox.Show(AppMessages.ConfirmAccountSucces, AppMessages.ConfirmAccountSucces_Title, MessageBoxButton.OK);
                    NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                }
                else
                    MessageBox.Show(String.Format(AppMessages.ConfirmAccountFailed, e.getErrorString()),
                        AppMessages.ConfirmAccountFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgessService.SetProgressIndicator(true, AppMessages.ProgressIndicator_ConfirmAccount);
            });
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
                MessageBox.Show(String.Format(AppMessages.ConfirmAccountFailed, e.getErrorString()),
                    AppMessages.ConfirmAccountFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion
    }
}
