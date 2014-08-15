using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class CreateAccountViewModel : BaseViewModel, MRequestListenerInterface
    {
        private readonly MegaSDK _megaSdk;

        public CreateAccountViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
            this.CreateAccountCommand = new DelegateCommand(this.CreateAccount);
            this.NavigateTermsOfUseCommand = new DelegateCommand(NavigateTermsOfUse);
        }

        #region Methods

        private void CreateAccount(object obj)
        {
            if (CheckInputParameters())
            {
                if (CheckPassword())
                {
                    if (TermOfUse)
                    {
                        this._megaSdk.createAccount(Email, Password, Name, this);
                    }
                    else
                        MessageBox.Show(AppMessages.AgreeTermsOfUse, AppMessages.AgreeTermsOfUse_Title,
                            MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show(AppMessages.PasswordsDoNotMatch, AppMessages.PasswordsDoNotMatch_Title,
                        MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppMessages.RequiredFieldsCreateAccount, AppMessages.RequiredFields_Title,
                        MessageBoxButton.OK);
            }
            
        }
        private static void NavigateTermsOfUse(object obj)
        {
            var webBrowserTask = new WebBrowserTask {Uri = new Uri(AppResources.TermsOfUseUrl)};
            webBrowserTask.Show();
        }

        private bool CheckInputParameters()
        {
            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(ConfirmPassword);
        }

        private bool CheckPassword()
        {
            return Password.Equals(ConfirmPassword, StringComparison.InvariantCulture);
        }

        #endregion

        #region Commands

        public ICommand CreateAccountCommand { get; set; }

        public ICommand NavigateTermsOfUseCommand { get; set; }

        #endregion

        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public bool TermOfUse { get; set; }

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
                    MessageBox.Show(AppMessages.ConfirmNeeded, AppMessages.ConfirmNeeded_Title, MessageBoxButton.OK);
                    NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                }
                else
                    MessageBox.Show(String.Format(AppMessages.CreateAccountFailed, e.getErrorString()),
                        AppMessages.CreateAccountFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgessService.SetProgressIndicator(true, AppMessages.ProgressIndicator_CreatingAccount);
            });
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
                MessageBox.Show(String.Format(AppMessages.CreateAccountFailed, e.getErrorString()),
                    AppMessages.CreateAccountFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion
    }
}
