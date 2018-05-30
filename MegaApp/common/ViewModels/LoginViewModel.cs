using System;
using System.Windows;
using mega;
using MegaApp.Classes;
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

        public void DoLogin()
        {
            if (CheckInputParameters())
                this._megaSdk.login(Email, Password, new LoginRequestListener(this, _loginPage));
            else if (_loginPage != null)
                Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));
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
