using System;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{    
    class CreateAccountViewModel : BaseSdkViewModel 
    {
        private readonly MegaSDK _megaSdk;
        private readonly LoginPage _loginPage;

        public CreateAccountViewModel(MegaSDK megaSdk, LoginPage loginPage)
            : base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this._loginPage = loginPage;
            this.ControlState = true;
            this.NavigateTermsOfServiceCommand = new DelegateCommand(NavigateTermsOfService);
        }

        #region Methods

        public void CreateAccount()
        {
            if (CheckInputParameters())
            {
                if (ValidationService.IsValidEmail(Email))
                {
                    if (CheckPassword())
                    {
                        if (TermOfService)
                        {
                            this._megaSdk.createAccount(Email, Password, FirstName, LastName,
                                new CreateAccountRequestListener(this, _loginPage));
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
                            new CustomMessageDialog(
                                    AppMessages.CreateAccountFailed_Title,
                                    AppMessages.AgreeTermsOfService,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                        }
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
                        new CustomMessageDialog(
                                AppMessages.CreateAccountFailed_Title,
                                AppMessages.PasswordsDoNotMatch,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                    }
                }
                else 
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
                    new CustomMessageDialog(
                            AppMessages.CreateAccountFailed_Title,
                            AppMessages.MalformedEmail,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                }
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
                new CustomMessageDialog(
                        AppMessages.CreateAccountFailed_Title,
                        AppMessages.RequiredFieldsCreateAccount,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
            }            
        }

        private static void NavigateTermsOfService(object obj)
        {
            var webBrowserTask = new WebBrowserTask {Uri = new Uri(AppResources.TermsOfServiceUrl)};
            webBrowserTask.Show();
        }

        private bool CheckInputParameters()
        {
            //Because lastname is not an obligatory parameter, if the lastname field is null or empty,
            //force it to be an empty string to avoid "ArgumentNullException" when call the createAccount method.
            if (String.IsNullOrWhiteSpace(LastName))
                LastName = String.Empty;

            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(FirstName) && 
                !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(ConfirmPassword);
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

        public string NewSignUpCode { get; set; }

        private string _email;
        public string Email 
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool TermOfService { get; set; }
        
        #endregion        
    }
}
