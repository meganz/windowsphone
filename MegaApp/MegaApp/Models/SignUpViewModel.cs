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

namespace MegaApp.Models
{
    class SignUpViewModel : BaseViewModel, MRequestListenerInterface
    {
        private readonly MegaSDK _megaSdk;

        public SignUpViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ControlState = true;
            this.SignUpCommand = new DelegateCommand(this.DoSignUp);
        }

        #region Methods

        private void DoSignUp(object obj)
        {
            if (CheckInputParameters())
            {
                if (CheckPassword())
                {
                    this._megaSdk.createAccount(Email, Password, Name, this);
                }
                else
                {
                    MessageBox.Show(AppMessages.PasswordsDoNotMatch, AppMessages.PasswordsDoNotMatch_Title,
                        MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppMessages.RequiredFieldsSignUp, AppMessages.RequiredFieldsSignUp_Title,
                        MessageBoxButton.OK);
            }
            
        }

        private bool CheckInputParameters()
        {
            return !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(PasswordReType);
        }

        private bool CheckPassword()
        {
            return Password.Equals(PasswordReType, StringComparison.InvariantCulture);
        }

        #endregion

        #region Commands

        public ICommand SignUpCommand { get; set; }

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

        private string _passwordReType;
        public string PasswordReType
        {
            get { return _passwordReType; }
            set
            {
                _passwordReType = value;
                OnPropertyChanged("PasswordReType");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
                    MessageBox.Show(AppMessages.SignUpSend, AppMessages.SignUpSend_Title, MessageBoxButton.OK);
                }
                else
                    MessageBox.Show(String.Format(AppMessages.SignUpFailed, e.getErrorString()),
                        AppMessages.SignUpFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgessService.SetProgressIndicator(true, AppMessages.ProgressIndicator_SendingSignUp);
            });
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);
                MessageBox.Show(String.Format(AppMessages.SignUpFailed, e.getErrorString()),
                    AppMessages.SignUpFailed_Title, MessageBoxButton.OK);
            });
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion
    }
}
