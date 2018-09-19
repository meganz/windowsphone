using System;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    class LoginViewModel : BaseSdkViewModel
    {
        private readonly MegaSDK _megaSdk;
        private readonly LoginPage _loginPage;

        public LoginViewModel(MegaSDK megaSdk, LoginPage loginPage = null)
            :base(megaSdk)
        {
            this._megaSdk = megaSdk;
            this._loginPage = loginPage;
            this.ControlState = true;
        }

        #region Methods

        /// <summary>
        /// Log in to a MEGA account.
        /// </summary>
        public async void Login()
        {
            if (!CheckInputParameters()) return;

            this.ControlState = false;
            this.IsBusy = true;

            var login = new LoginRequestListenerAsync();
            var result = await login.ExecuteAsync(() =>
                this.MegaSdk.login(this.Email, this.Password, login));

            if (result == LoginResult.MultiFactorAuthRequired)
            {
                await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(async (string code) =>
                {
                    result = await login.ExecuteAsync(() =>
                    this.MegaSdk.multiFactorAuthLogin(this.Email, this.Password, code, login));

                    if (result == LoginResult.MultiFactorAuthInvalidCode)
                    {
                        DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                        return false;
                    }

                    return true;
                });
            }

            if (_loginPage != null)
                Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));

            this.ControlState = true;
            this.IsBusy = false;

            // Set default error content
            var errorContent = string.Format(Resources.AppMessages.LoginFailed, login.ErrorString);
            switch (result)
            {
                case LoginResult.Success:
                    SettingsService.SaveMegaLoginData(this.Email, this.MegaSdk.dumpSession());

                    // Validate product subscription license on background thread
                    Task.Run(() => LicenseService.ValidateLicenses());

                    // Navigate to the main page to load the main application for the user
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Login));
                    return;

                case LoginResult.UnassociatedEmailOrWrongPassword:
                    errorContent = Resources.AppMessages.WrongEmailPasswordLogin;
                    break;

                case LoginResult.TooManyLoginAttempts:
                    // Too many failed login attempts. Wait one hour.
                    errorContent = string.Format(Resources.AppMessages.AM_TooManyFailedLoginAttempts,
                        DateTime.Now.AddHours(1).ToString("HH:mm:ss"));
                    break;

                case LoginResult.AccountNotConfirmed:
                    errorContent = Resources.AppMessages.AM_AccountNotConfirmed;
                    break;

                case LoginResult.MultiFactorAuthRequired:
                case LoginResult.MultiFactorAuthInvalidCode:
                case LoginResult.Unknown:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            

            // Show error message
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                new CustomMessageDialog(
                    AppMessages.LoginFailed_Title, errorContent,
                    App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
            });
        }

        private bool CheckInputParameters()
        {
            if (String.IsNullOrEmpty(Email) || String.IsNullOrEmpty(Password))
            {
                new CustomMessageDialog(
                        AppMessages.RequiredFields_Title,
                        AppMessages.RequiredFieldsLogin,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                return false;
            }
            
            if(!ValidationService.IsValidEmail(Email))
            {
                new CustomMessageDialog(
                        AppMessages.LoginFailed_Title,
                        AppMessages.AM_IncorrectEmailFormat,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
               return false;
            }

            return true;
        }

        private static void SaveLoginData(string email, string session)
        {
            SettingsService.SaveMegaLoginData(email, session);
        }
        
        #endregion
        
        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public string SessionKey { get; set; }        

        #endregion
    }
}
