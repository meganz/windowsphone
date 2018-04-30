using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

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
            if (!CheckInputParameters()) return;

            string errorMessage = string.Empty;
            if (CheckPassword())
            {
                if (CheckPasswordStrenght())
                {
                    if (TermOfService)
                    {
                        this._megaSdk.createAccount(Email, Password, FirstName, LastName,
                            new CreateAccountRequestListener(this, _loginPage));
                        return;
                    }
                    else
                    {
                        errorMessage = AppMessages.AgreeTermsOfService;
                    }
                }
                else
                {
                    errorMessage = AppMessages.AM_VeryWeakPassword;
                }
            }
            else
            {
                errorMessage = AppMessages.PasswordsDoNotMatch;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
            new CustomMessageDialog(
                AppMessages.CreateAccountFailed_Title,
                errorMessage,
                App.AppInformation,
                MessageDialogButtons.Ok).ShowDialog();
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
            if (string.IsNullOrWhiteSpace(LastName))
                LastName = string.Empty;

            if (string.IsNullOrWhiteSpace(this.Email) ||
                string.IsNullOrWhiteSpace(this.Password) ||
                string.IsNullOrWhiteSpace(this.FirstName) ||
                string.IsNullOrWhiteSpace(this.ConfirmPassword))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
                new CustomMessageDialog(
                    AppMessages.CreateAccountFailed_Title,
                    AppMessages.RequiredFieldsCreateAccount,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
                return false;
            }

            if (ValidationService.IsValidEmail(this.Email)) return true;

            Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
            new CustomMessageDialog(
                AppMessages.CreateAccountFailed_Title,
                AppMessages.AM_IncorrectEmailFormat,
                App.AppInformation,
                MessageDialogButtons.Ok).ShowDialog();
            return false;
        }

        private bool CheckPassword()
        {
            return Password.Equals(ConfirmPassword, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Calculate the password strenght.
        /// </summary>
        /// <param name="value">Password string</param>
        private void CalculatePasswordStrength(string value)
        {
            this.PasswordStrength = ValidationService.CalculatePasswordStrength(value);
        }

        /// <summary>
        /// Checks the new password strenght.
        /// </summary>
        /// <returns>TRUE if is all right or FALSE in other case.</returns>
        private bool CheckPasswordStrenght()
        {
            return this.PasswordStrength != MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK;
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

        private string _password;
        public string Password
        {
            get { return _password; }
            set 
            { 
                if(SetField(ref _password, value))
                    CalculatePasswordStrength(value);
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetField(ref _confirmPassword, value); }
        }

        private MPasswordStrength _passwordStrength;
        public MPasswordStrength PasswordStrength
        {
            get { return _passwordStrength; }
            set { SetField(ref _passwordStrength, value); }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set { SetField(ref _firstName, value); } 
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set { SetField(ref _lastName, value); }
        }

        private bool _termOfService;
        public bool TermOfService
        {
            get { return _termOfService; }
            set { SetField(ref _termOfService, value); }
        }
        
        #endregion        
    }
}
