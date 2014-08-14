using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;

namespace MegaApp.Models
{
    class LoginViewModel : BaseViewModel, MRequestListenerInterface
    {
        private readonly MegaSDK _megaSdk;

        public LoginViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
            this.LoginCommand = new DelegateCommand(this.DoLogin);
            this.NavigateSignUpCommand = new DelegateCommand(NavigateSignUpPage);
        }

        #region Methods

        private void DoLogin(object obj)
        {
            if (CheckInputParameters())
            {
                this._megaSdk.login(Email, Password, this);
            }
            else
            {
                MessageBox.Show(AppMessages.RequiredFieldsLogin, AppMessages.RequiredFieldsLogin_Title,
                        MessageBoxButton.OK);
            }
        }
        private static void NavigateSignUpPage(object obj)
        {
            ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(NavigationUriBuilder.BuildNavigationUri(typeof(SignUpPage),
                       NavigationParameter.Normal));
        }

        private bool CheckInputParameters()
        {
            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password);
        }

        private static void SaveLoginData(string email, string session)
        {
            SettingsService.SaveMegaLoginData(email, session);
        }


        #endregion

        #region Commands

        public ICommand LoginCommand { get; set; }

        public ICommand NavigateSignUpCommand { get; set; }

        #endregion

        #region Properties

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get { return _rememberMe; }
            set
            {
                _rememberMe = value;
                OnPropertyChanged("RememberMe");
            }
        }

        private bool _controlState;
        public bool ControlState
        {
            get { return _controlState; }
            set
            {
                _controlState = value;
                OnPropertyChanged("ControlState");
            }
        }

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
                    if (RememberMe)
                        SaveLoginData(Email, api.dumpSession());

                    ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(NavigationUriBuilder.BuildNavigationUri(typeof(MainPage),
                        NavigationParameter.Login));
                }
                else
                    MessageBox.Show(String.Format(AppMessages.LoginFailed, e.getErrorString()),
                        AppMessages.LoginFailed_Title, MessageBoxButton.OK);
            });
        }
       
        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgessService.SetProgressIndicator(true, AppMessages.ProgressIndicator_Login);
            });
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
                MessageBox.Show(String.Format(AppMessages.LoginFailed, e.getErrorString()),
                    AppMessages.LoginFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion
    }
}
