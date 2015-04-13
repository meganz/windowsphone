using System;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    ///mobile_terms.html and /mobile_privacy.html
    class CreateAccountViewModel : BaseRequestListenerViewModel 
    {
        private readonly MegaSDK _megaSdk;

        public CreateAccountViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
            this.NavigateTermsOfServiceCommand = new DelegateCommand(NavigateTermsOfService);
        }

        #region Methods

        public void CreateAccount()
        {
            if (CheckInputParameters())
            {
                if (!ValidationService.IsValidEmail(Email))
                {
                    if (CheckPassword())
                    {
                        if (TermOfService)
                        {
                            this._megaSdk.createAccount(Email, Password, Name, this);
                        }
                        else
                            MessageBox.Show(AppMessages.AgreeTermsOfService, AppMessages.CreateAccountFailed_Title.ToUpper(),
                                MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show(AppMessages.PasswordsDoNotMatch, AppMessages.CreateAccountFailed_Title.ToUpper(),
                            MessageBoxButton.OK);
                    }
                }
                else 
                {
                    MessageBox.Show(AppMessages.MalformedEmail, AppMessages.CreateAccountFailed_Title.ToUpper(),
                            MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppMessages.RequiredFieldsCreateAccount, AppMessages.RequiredFields_Title.ToUpper(),
                        MessageBoxButton.OK);
            }
            
        }
        private static void NavigateTermsOfService(object obj)
        {
            var webBrowserTask = new WebBrowserTask {Uri = new Uri(AppResources.TermsOfServiceUrl)};
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

        public ICommand NavigateTermsOfServiceCommand { get; set; }

        #endregion

        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public bool TermOfService { get; set; }
        
        #endregion

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.CreateAccount; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.CreateAccountFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.CreateAccountFailed_Title.ToUpper(); }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.ConfirmNeeded; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.ConfirmNeeded_Title.ToUpper(); }
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
            get { return typeof (InitTourPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Normal; }
        }

        #endregion
        
    }
}
