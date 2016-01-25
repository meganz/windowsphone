using System;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class SettingsViewModel : BaseAppInfoAwareViewModel
    {
        public SettingsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            this.AppVersion = AppService.GetAppVersion();
            this.MegaSdkVersion = AppService.GetMegaSDK_Version();
            this.ShareMasterKeyCommand = new DelegateCommand(ShareMasterKey);
            this.CopyMasterKeyCommand = new DelegateCommand(CopyMasterkey);
            this.ChangePinLockCommand = new DelegateCommand(ChangePinLock);
            this.ViewMasterKeyCommand = new DelegateCommand(ViewMasterKey);

#if WINDOWS_PHONE_81
            this.SelectDownloadLocationCommand = new DelegateCommand(SelectDownloadLocation);
#endif
            this.MegaSdkCommand = new DelegateCommand(NavigateToMegaSdk);
            this.GoedWareCommand = new DelegateCommand(NavigateToGoedWare);

            this.PinLockIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled, false);
            
            // Do not set the property on initialize, because it fill fire the SetAutoCameraUploadStatus
            _cameraUploadsIsEnabled = MediaService.GetAutoCameraUploadStatus();
            this.CameraUploadsIsEnabledText = _cameraUploadsIsEnabled ? UiResources.On : UiResources.Off;

#if WINDOWS_PHONE_80
            this.ExportIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.ExportImagesToPhotoAlbum, false);
#elif WINDOWS_PHONE_81
            this.AskDownloadLocationIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.AskDownloadLocationIsEnabled, false);
            this.StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, UiResources.DefaultDownloadLocation);
#endif

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.Settings);
        }

        #region Commands

        public ICommand ShareMasterKeyCommand { get; set; }
        public ICommand CopyMasterKeyCommand { get; private set; }
        public ICommand ViewMasterKeyCommand { get; private set; }
        public ICommand ChangePinLockCommand { get; private set; }

#if WINDOWS_PHONE_81
        public ICommand SelectDownloadLocationCommand { get; private set; }
#endif

        public ICommand MegaSdkCommand { get; private set; }
        public ICommand GoedWareCommand { get; private set; }

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
                new CustomMessageDialog(
                           AppMessages.MasterkeyCopied_Title,
                           AppMessages.MasterkeyCopied,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                           AppMessages.ClipboardFailed_Title,
                           AppMessages.ClipboardFailed,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
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

#if WINDOWS_PHONE_81
        private void SelectDownloadLocation(object obj)
        {
            if (App.FileOpenOrFolderPickerOpenend) return;
            FolderService.SelectFolder("SelectDefaultDownloadFolder");
        }
#endif

        private void NavigateToMegaSdk(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.MegaSdkUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToGoedWare(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.GoedWareUrl) };
            webBrowserTask.Show();
        }

        #endregion

        #region Properties

        public string AppVersion { get; private set; }

        public string MegaSdkVersion { get; private set; }

//        #if WINDOWS_PHONE_80
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
//        #endif

//        #if WINDOWS_PHONE_81
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
//        #endif

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

        private string _cameraUploadsIsEnabledText;
        public string CameraUploadsIsEnabledText
        {
            get { return _cameraUploadsIsEnabledText; }
            set
            {
                _cameraUploadsIsEnabledText = value;
                OnPropertyChanged("CameraUploadsIsEnabledText");
            }
        }

        private bool _cameraUploadsIsEnabled;
        public bool CameraUploadsIsEnabled
        {
            get { return _cameraUploadsIsEnabled; }
            set
            {
                if (_cameraUploadsIsEnabled != value)
                {
                    SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, value);
                    if(!value) SettingsService.DeleteFileSetting("LastUploadDate");
                }

                _cameraUploadsIsEnabled = MediaService.SetAutoCameraUpload(value);
                this.CameraUploadsIsEnabledText = _cameraUploadsIsEnabled ? UiResources.On : UiResources.Off;
                OnPropertyChanged("CameraUploadsIsEnabled");
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

//        #if WINDOWS_PHONE_80
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
  //      #endif

//        #if WINDOWS_PHONE_81
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
//        #endif

        #endregion
    }
}
