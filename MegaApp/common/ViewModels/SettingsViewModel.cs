using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class SettingsViewModel : BaseAppInfoAwareViewModel
    {
        public SettingsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            this.ShareRecoveryKeyCommand = new DelegateCommand(ShareRecoveryKey);
            this.CopyRecoveryKeyCommand = new DelegateCommand(CopyRecoveryKey);
            this.ChangePinLockCommand = new DelegateCommand(ChangePinLock);
            this.ViewRecoveryKeyCommand = new DelegateCommand(ViewRecoveryKey);
            this.CloseOtherSessionsCommand = new DelegateCommand(CloseOtherSessions);
            this.ClearCacheCommand = new DelegateCommand(ClearCache);

            #if WINDOWS_PHONE_80
            this.SelectDownloadLocationCommand = null;
            #elif WINDOWS_PHONE_81
            this.SelectDownloadLocationCommand = new DelegateCommand(SelectDownloadLocation);
            #endif

            this.MegaSdkCommand = new DelegateCommand(NavigateToMegaSdk);
            this.GoedWareCommand = new DelegateCommand(NavigateToGoedWare);
            this.TermsOfServiceCommand = new DelegateCommand(NavigateToTermsOfService);
            this.PrivacyPolicyCommand = new DelegateCommand(NavigateToPrivacyPolicy);
            this.CopyrightCommand = new DelegateCommand(NavigateToCopyright);
            this.TakedownGuidanceCommand = new DelegateCommand(NavigateToTakedownGuidance);
            this.GeneralCommand = new DelegateCommand(NavigateToGeneral);
            this.DataProtectionRegulationCommand =
                new DelegateCommand(NavigateToDataProtectionRegulation);

            this.Initialize();

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.Settings);

            AccountDetails.GetAppCacheSize();
        }

        #region Commands

        public ICommand ShareRecoveryKeyCommand { get; set; }
        public ICommand CopyRecoveryKeyCommand { get; private set; }
        public ICommand ViewRecoveryKeyCommand { get; private set; }
        public ICommand ChangePinLockCommand { get; private set; }
        public ICommand SelectDownloadLocationCommand { get; private set; }
        public ICommand MegaSdkCommand { get; private set; }
        public ICommand GoedWareCommand { get; private set; }
        public ICommand TermsOfServiceCommand { get; private set; }
        public ICommand PrivacyPolicyCommand { get; private set; }
        public ICommand CopyrightCommand { get; private set; }
        public ICommand TakedownGuidanceCommand { get; private set; }
        public ICommand GeneralCommand { get; private set; }
        public ICommand DataProtectionRegulationCommand { get; private set; }
        public ICommand CloseOtherSessionsCommand { get; private set; }
        public ICommand ClearCacheCommand { get; private set; }

        #endregion

        #region Methods

        public void ReloadSettings()
        {
            this.Initialize();
        }

        private async void Initialize()
        {
            this.AppVersion = SettingsService.LoadSetting<bool>(SettingsResources.UseStagingServer, false) ?
                string.Format("{0} (staging)", AppService.GetAppVersion()) : AppService.GetAppVersion();

            this.MegaSdkVersion = AppService.GetMegaSDK_Version();

            // Initialize the PIN lock code setting
            SetField(ref this._isPinLockEnabled, 
                SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled, false),
                "IsMultiFactorAuthEnabled");

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

            this.IsMultiFactorAuthAvailable = SdkService.MegaSdk.multiFactorAuthAvailable();
            if (this.IsMultiFactorAuthAvailable)
            {
                var mfaStatus = await AccountService.CheckMultiFactorAuthStatusAsync();
                switch (mfaStatus)
                {
                    case MultiFactorAuthStatus.Enabled:
                        SetField(ref this._isMultiFactorAuthEnabled, true, "IsMultiFactorAuthEnabled");
                        break;

                    case MultiFactorAuthStatus.Disabled:
                        SetField(ref this._isMultiFactorAuthEnabled, false, "IsMultiFactorAuthEnabled");
                        break;

                    case MultiFactorAuthStatus.Unknown:
                        OnUiThread(() =>
                        {
                            new CustomMessageDialog(UiResources.UI_Warning, AppMessages.AM_MFA_CheckStatusFailed,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                        });
                        break;
                }
            }
        }

        private void ShareRecoveryKey(object obj)
        {
            var shareStatusTask = new ShareStatusTask {Status = MegaSdk.exportMasterKey()};
            shareStatusTask.Show();
        }

        private void CopyRecoveryKey(object obj)
        {
            CopyClipboard();
        }

        private void CopyClipboard()
        {
            try
            {
                Clipboard.SetText(MegaSdk.exportMasterKey());
                MegaSdk.masterKeyExported();
                new CustomMessageDialog(
                           AppMessages.AM_RecoveryKeyCopied_Title,
                           AppMessages.AM_RecoveryKeyCopied,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                           AppMessages.AM_RecoveryKeyClipboardFailed_Title,
                           AppMessages.AM_RecoveryKeyClipboardFailed,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
            }
        }
        
        private void ViewRecoveryKey(object obj)
        {
            DialogService.ShowViewRecoveryKey(MegaSdk.exportMasterKey(), CopyClipboard);
        }

        public void ProcessBackupLink()
        {
            DialogService.ShowViewRecoveryKey(MegaSdk.exportMasterKey(), CopyClipboard);
        }

        private async void ChangePinLock(object obj)
        {
            await DialogService.ShowPinLockDialog(true);
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
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_SdkLink) };
            webBrowserTask.Show();
        }

        private void NavigateToGoedWare(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.GoedWareUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToTermsOfService(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_TermsOfServiceUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToPrivacyPolicy(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_PrivacyPolicyUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToCopyright(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_CopyrightUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToTakedownGuidance(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_TakedownGuidanceUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToGeneral(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_GeneralLegalUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToDataProtectionRegulation(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_DataProtectionRegulationUrl) };
            webBrowserTask.Show();
        }

        private async void CloseOtherSessions(object obj)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            var result = await new CustomMessageDialog(
                UiResources.UI_Warning,
                AppMessages.AM_CloseOtherSessionsQuestionMessage,
                App.AppInformation,
                MessageDialogButtons.YesNo).ShowDialogAsync();

            if (result == MessageDialogResult.CancelNo) return;

            this.MegaSdk.killAllSessions(new KillAllSessionsRequestListener());
        }

        /// <summary>
        /// Enable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> EnableMultiFactorAuthAsync()
        {
            return await DialogService.ShowMultiFactorAuthSetupDialogAsync();
        }

        /// <summary>
        /// Show the dialog to disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> ShowDisableMultiFactorAuthDialogAsync()
        {
            var result = await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(
                this.DisableMultiFactorAuthAsync,
                AppMessages.AM_2FA_DisableDialogTitle);

            if (result)
            {
                DialogService.CloseMultiFactorAuthCodeInputDialog();
                DialogService.ShowMultiFactorAuthDisabledDialog();
            }

            return result;
        }

        /// <summary>
        /// Disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> DisableMultiFactorAuthAsync(string code)
        {
            var disableMultiFactorAuth = new MultiFactorAuthDisableRequestListenerAsync();
            var result = await disableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthDisable(code, disableMultiFactorAuth));

            if (!result)
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();

            return result;
        }

        private async void OnIsMultiFactorAuthEnabledValueChanged()
        {
            var value = this.IsMultiFactorAuthEnabled ?
                await this.EnableMultiFactorAuthAsync() :
                !await this.ShowDisableMultiFactorAuthDialogAsync();
            
            SetField(ref this._isMultiFactorAuthEnabled, value, "IsMultiFactorAuthEnabled");
        }

        private async void OnIsPinLockEnabledValueChanged()
        {
            if (!this.IsPinLockEnabled) return;

            SetField(ref this._isPinLockEnabled,
                    await DialogService.ShowPinLockDialog(false),
                    "IsPinLockEnabled");

            OnPropertyChanged("IsPinLockEnabledText");
        }

        /// Clear the app cache
        /// </summary>
        private async void ClearCache(object obj)
        {
            string title, message = string.Empty;
            if (await AppService.ClearAppCacheAsync())
            {
                title = AppMessages.CacheCleared_Title;
                message = AppMessages.CacheCleared;
            }
            else
            {
                title = AppMessages.AM_ClearCacheFailed_Title;
                message = AppMessages.AM_ClearCacheFailed;
            }

            OnUiThread(() =>
            {
                new CustomMessageDialog(title, message, App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            });

            AccountDetails.GetAppCacheSize();
        }

        #endregion

        #region Properties

        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            private set { SetField(ref _appVersion, value); }
        }

        public string MegaSdkVersion { get; private set; }

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

        private bool _isPinLockEnabled;
        public bool IsPinLockEnabled
        {
            get { return _isPinLockEnabled; }
            set
            {
                if (_isPinLockEnabled && !value)
                {
                    SettingsService.DeleteSetting(SettingsResources.UserPinLockIsEnabled);
                    SettingsService.DeleteSetting(SettingsResources.UserPinLock);
                }

                if (!SetField(ref _isPinLockEnabled, value))
                    return;

                OnPropertyChanged("IsPinLockEnabledText");
                OnIsPinLockEnabledValueChanged();                
            }
        }

        public string IsPinLockEnabledText
        {
            get { return IsPinLockEnabled ? UiResources.On : UiResources.Off; }
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

        private bool _isMultiFactorAuthEnabled;
        public bool IsMultiFactorAuthEnabled
        {
            get { return _isMultiFactorAuthEnabled; }
            set
            {
                if (!SetField(ref _isMultiFactorAuthEnabled, value))
                    return;

                OnIsMultiFactorAuthEnabledValueChanged();
            }
        }

        private bool _isMultiFactorAuthAvailable;
        public bool IsMultiFactorAuthAvailable
        {
            get { return _isMultiFactorAuthAvailable; }
            set { SetField(ref _isMultiFactorAuthAvailable, value); }
        }

        #endregion
    }
}
