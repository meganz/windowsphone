using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class SettingsViewModel: BaseSdkViewModel
    {
        public SettingsViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this.AppVersion = AppService.GetAppVersion();
            this.ShareMasterKeyCommand = new DelegateCommand(ShareMasterKey);
            this.CopyMasterKeyCommand = new DelegateCommand(CopyMasterkey);
            this.ChangePasswordCommand = new DelegateCommand(ChangePassword);

            PasswordIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.UserPasswordIsEnabled, false);
        }

        #region Commands

        public ICommand ShareMasterKeyCommand { get; set; }
        public ICommand CopyMasterKeyCommand { get; set; }

        public ICommand ChangePasswordCommand { get; set; }

        #endregion

        #region Methods

        private void ShareMasterKey(object obj)
        {
            var shareStatusTask = new ShareStatusTask {Status = MegaSdk.exportMasterKey()};
            shareStatusTask.Show();
        }

        private void CopyMasterkey(object obj)
        {
            Clipboard.SetText(MegaSdk.exportMasterKey());
            MessageBox.Show("Masterkey copied to clipboard", "Masterkey copied", MessageBoxButton.OK);
        }
        private void ChangePassword(object obj)
        {
            DialogService.ShowPasswordDialog(true, this);
        }

        #endregion

        #region Properties

        public string AppVersion { get; set; }

        private bool _passwordIsEnabled;
        public bool PasswordIsEnabled
        {
            get { return _passwordIsEnabled; }
            set
            {
                if (_passwordIsEnabled && !value)
                {
                    SettingsService.DeleteSetting(SettingsResources.UserPasswordIsEnabled);
                    SettingsService.DeleteSetting(SettingsResources.UserPassword);
                }

                _passwordIsEnabled = value;

                PasswordIsEnabledText = _passwordIsEnabled ? UiResources.On : UiResources.Off;
               
                if (_passwordIsEnabled)
                    DialogService.ShowPasswordDialog(false, this);
                
                OnPropertyChanged("PasswordIsEnabled");
            }
        }

        private string _passwordIsEnabledText;
        public string PasswordIsEnabledText
        {
            get { return _passwordIsEnabledText; }
            set
            {
                _passwordIsEnabledText = value;
                OnPropertyChanged("PasswordIsEnabledText");
            }
        }

        #endregion
    }
}
