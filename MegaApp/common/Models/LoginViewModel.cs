using System;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class LoginViewModel : BaseRequestListenerViewModel
    {
        private readonly MegaSDK _megaSdk;

        public LoginViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.StayLoggedIn = SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn, true);
            this.ControlState = true;            
        }

        #region Methods

        public void DoLogin()
        {
            if (CheckInputParameters())
            {
                this._megaSdk.login(Email, Password, this);
            }
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
                        AppMessages.MalformedEmail,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
               return false;
            }

            return true;
        }

        private static void SaveLoginData(string email, string session, bool stayLoggedIn)
        {
            SettingsService.SaveMegaLoginData(email, session, stayLoggedIn);
        }
        
        #endregion
        
        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public bool StayLoggedIn { get; set; }
        public string SessionKey { get; private set; }        

        #endregion

        #region  Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.Login; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.LoginFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.LoginFailed_Title.ToUpper(); }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return (typeof(MainPage)); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Login; }
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
            });            

            if (e.getErrorCode() == MErrorType.API_OK)
                SessionKey = api.dumpSession();

            // E-mail unassociated with a MEGA account or Wrong password
            if (e.getErrorCode() == MErrorType.API_ENOENT)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            ErrorMessageTitle,
                            AppMessages.WrongEmailPasswordLogin,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
                return;
            }

            base.onRequestFinish(api, request, e);
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MRequest request)
        {
            SaveLoginData(Email, SessionKey, StayLoggedIn);
        }

        #endregion
        
        
    }
}
