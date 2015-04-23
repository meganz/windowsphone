using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;

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
                MessageBox.Show(AppMessages.RequiredFieldsLogin, AppMessages.RequiredFields_Title.ToUpper(),
                        MessageBoxButton.OK);
                return false;
            }
            
            if(!ValidationService.IsValidEmail(Email))
            {
                MessageBox.Show(AppMessages.MalformedEmail, AppMessages.LoginFailed_Title.ToUpper(),
                        MessageBoxButton.OK);
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
            get { return typeof(MainPage); }
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
                    MessageBox.Show(AppMessages.WrongEmailPasswordLogin, ErrorMessageTitle, MessageBoxButton.OK));
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
