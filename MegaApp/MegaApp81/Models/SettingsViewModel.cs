using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
            this.MegaSDK_Version = AppResources.MegaSDK_Version;
            this.ShareMasterKeyCommand = new DelegateCommand(ShareMasterKey);
            this.CopyMasterKeyCommand = new DelegateCommand(CopyMasterkey);
            this.ChangePasswordCommand = new DelegateCommand(ChangePassword);
            this.ViewMasterKeyCommand = new DelegateCommand(ViewMasterKey);
            this.SelectDownloadLocationCommand = new DelegateCommand(SelectDownloadLocation);

            this.AskDownloadLocationIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.AskDownloadLocationIsEnabled, false);
            this.PasswordIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled, false);
            this.StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, AppResources.DefaultDownloadLocation);
        }

        #region Commands

        public ICommand ShareMasterKeyCommand { get; set; }
        public ICommand CopyMasterKeyCommand { get; set; }
        public ICommand ViewMasterKeyCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand SelectDownloadLocationCommand { get; set; }

        #endregion

        #region Methods

        private void ShareMasterKey(object obj)
        {
            var shareStatusTask = new ShareStatusTask {Status = MegaSdk.exportMasterKey()};
            shareStatusTask.Show();
        }

        private void CopyMasterkey(object obj)
        {
            CopyClipboard();
        }

        private void CopyClipboard()
        {
            try
            {
                Clipboard.SetText(MegaSdk.exportMasterKey());
                MessageBox.Show("Masterkey copied to clipboard", "Masterkey copied", MessageBoxButton.OK);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to copy masterkey to clipboard. Please try again", "Clipboard failed", MessageBoxButton.OK);
            }
        }
        
        private void ViewMasterKey(object obj)
        {
            DialogService.ShowViewMasterKey(MegaSdk.exportMasterKey(), CopyClipboard);
        }

        private void ChangePassword(object obj)
        {
            DialogService.ShowPasswordDialog(true, this);
        }

        private void SelectDownloadLocation(object obj)
        {
            if (App.FileOpenOrFolderPickerOpenend) return;
            FolderService.SelectFolder("SelectDefaultDownloadFolder");
        }

        #endregion

        #region Properties

        public string AppVersion { get; set; }

        public string MegaSDK_Version { get; set; }

        private bool _askDownloadLocationIsEnabled;
        public bool AskDownloadLocationIsEnabled
        {
            get { return _askDownloadLocationIsEnabled; }
            set
            {
                if (_askDownloadLocationIsEnabled != value)
                    SettingsService.SaveSetting(SettingsResources.AskDownloadLocationIsEnabled, value);

                _askDownloadLocationIsEnabled = value;
                DownloadLocationSelectionIsEnabled = !_askDownloadLocationIsEnabled;

                AskDownloadLocationIsEnabledText = _askDownloadLocationIsEnabled ? UiResources.On : UiResources.Off;

                OnPropertyChanged("AskDownloadLocationIsEnabled");
            }
        }

        private bool _downloadLocationSelectionIsEnabled;
        public bool DownloadLocationSelectionIsEnabled
        {
            get { return _downloadLocationSelectionIsEnabled; }
            set
            {
                _downloadLocationSelectionIsEnabled = value;
                OnPropertyChanged("DownloadLocationSelectionIsEnabled");
            }
        }

        private bool _passwordIsEnabled;
        public bool PasswordIsEnabled
        {
            get { return _passwordIsEnabled; }
            set
            {
                if (_passwordIsEnabled && !value)
                {
                    SettingsService.DeleteSetting(SettingsResources.UserPinLockIsEnabled);
                    SettingsService.DeleteSetting(SettingsResources.UserPinLock);
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

        private string _askDownloadLocationIsEnabledText;
        public string AskDownloadLocationIsEnabledText
        {
            get { return _askDownloadLocationIsEnabledText; }
            set
            {
                _askDownloadLocationIsEnabledText = value;
                OnPropertyChanged("AskDownloadLocationIsEnabledText");
            }
        }

        private string _standardDownloadLocation;
        public string StandardDownloadLocation
        {
            get { return _standardDownloadLocation; }
            set
            {
                _standardDownloadLocation = value;
                OnPropertyChanged("StandardDownloadLocation");
            }
        }

        #endregion
    }
}
