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
            this.ChangePinLockCommand = new DelegateCommand(ChangePinLock);
            this.ViewMasterKeyCommand = new DelegateCommand(ViewMasterKey);

            this.ExportIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.ExportImagesToPhotoAlbum, false);
            this.PinLockIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled, false);
        }

        #region Commands

        public ICommand ShareMasterKeyCommand { get; set; }
        public ICommand CopyMasterKeyCommand { get; set; }
        public ICommand ViewMasterKeyCommand { get; set; }
        public ICommand ChangePinLockCommand { get; set; }

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

        private void ChangePinLock(object obj)
        {
            DialogService.ShowPinLockDialog(true, this);
        }

        #endregion

        #region Properties

        public string AppVersion { get; set; }

        public string MegaSDK_Version { get; set; }

        private bool _exportIsEnabled;
        public bool ExportIsEnabled
        {
            get { return _exportIsEnabled; }
            set
            {
                if(_exportIsEnabled != value)
                    SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, value);
                
                _exportIsEnabled = value;

                ExportIsEnabledText = _exportIsEnabled ? UiResources.On : UiResources.Off;
                
                OnPropertyChanged("ExportIsEnabled");
            }
        }

        private bool _pinLockIsEnabled;
        public bool PinLockIsEnabled
        {
            get { return _pinLockIsEnabled; }
            set
            {
                if (_pinLockIsEnabled && !value)
                {
                    SettingsService.DeleteSetting(SettingsResources.UserPinLockIsEnabled);
                    SettingsService.DeleteSetting(SettingsResources.UserPinLock);
                }

                _pinLockIsEnabled = value;

                PinLockIsEnabledText = _pinLockIsEnabled ? UiResources.On : UiResources.Off;

                if (_pinLockIsEnabled)
                    DialogService.ShowPinLockDialog(false, this);
                
                OnPropertyChanged("PinLockIsEnabled");
            }
        }

        private string _pinLockIsEnabledText;
        public string PinLockIsEnabledText
        {
            get { return _pinLockIsEnabledText; }
            set
            {
                _pinLockIsEnabledText = value;
                OnPropertyChanged("PinLockIsEnabledText");
            }
        }

        private string _exportIsEnabledText;
        public string ExportIsEnabledText
        {
            get { return _exportIsEnabledText; }
            set
            {
                _exportIsEnabledText = value;
                OnPropertyChanged("ExportIsEnabledText");
            }
        }

        #endregion
    }
}
