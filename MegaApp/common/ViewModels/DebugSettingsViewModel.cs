using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    class DebugSettingsViewModel: BaseViewModel
    {
        public DebugSettingsViewModel()
        {
            _isDebugMode = SettingsService.LoadSetting(SettingsResources.DebugModeIsEnabled, false);
            ShowMemoryInformation = false;

            if (_isDebugMode)
            {
                ShowDebugAlert = true;
                LogService.SetLogLevel(MLogLevel.LOG_LEVEL_MAX);
            }
            else
            {
                ShowDebugAlert = false;
                LogService.SetLogLevel(MLogLevel.LOG_LEVEL_DEBUG);
            }
        }

        #region Methods

        /// <summary>
        /// Method to enable the DEBUG mode.
        /// <para>Saves the setting, sets the log level and creates the log file.</para>
        /// </summary>
        public void EnableDebugMode()
        {
            _isDebugMode = true;
            SettingsService.SaveSetting(SettingsResources.DebugModeIsEnabled, true);
            LogService.SetLogLevel(MLogLevel.LOG_LEVEL_MAX);
            try { File.Create(AppService.GetFileLogPath()); }
            catch (IOException)
            { /* No problem, because it will be created when writes the first line if not exists. */ }

            if (App.SavedForOfflineViewModel != null)
                App.SavedForOfflineViewModel.SavedForOffline.Refresh();
        }

        /// <summary>
        /// Method to disable the DEBUG mode.
        /// <para>Saves the setting, sets the log level and creates the log file.</para>
        /// </summary>
#if WINDOWS_PHONE_80
        public void DisableDebugMode()
#elif WINDOWS_PHONE_81
        public async void DisableDebugMode()
#endif
        {
            _isDebugMode = false;
            SettingsService.SaveSetting(SettingsResources.DebugModeIsEnabled, false);
            LogService.SetLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

#if WINDOWS_PHONE_80
            FileService.DeleteFile(AppService.GetFileLogPath());
            if (App.SavedForOfflineViewModel != null)
                App.SavedForOfflineViewModel.SavedForOffline.Refresh();
#elif WINDOWS_PHONE_81
            // Need force to false to show the options dialog
            App.AppInformation.PickerOrAsyncDialogIsOpen = false;
            await DialogService.ShowOptionsDialog(AppMessages.AM_SaveLogFile_Title, AppMessages.AM_SaveLogFile,
                new[]
                {
                    new DialogButton(UiResources.Yes, () =>
                    {
                        // Ask the user a save location
                        FolderService.SelectFolder("SelectLogFileSaveLocation");
                    }),
                    new DialogButton(UiResources.No, () =>
                    {
                        FileService.DeleteFile(AppService.GetFileLogPath());
                        if (App.SavedForOfflineViewModel != null)
                            App.SavedForOfflineViewModel.SavedForOffline.Refresh();
                    })
                });
#endif
        }

        #endregion

        #region Properties

        private bool _isDebugMode;
        public bool IsDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                OnUiThread(() =>
                {
                    if (_isDebugMode == value) return;

                    String message, title = String.Empty;                    
                    if (value)
                    {
                        title = AppMessages.AM_EnableDebugMode_Title;
                        message = String.Format(AppMessages.AM_EnableDebugMode_Message,
                            AppResources.LogFileName);
                    }
                    else
                    {
                        title = AppMessages.AM_DisableDebugMode_Title;
                        message = String.Format(AppMessages.AM_DisableDebugMode_Message,
                            AppResources.LogFileName);
                    }

                    var customMessageDialog = new CustomMessageDialog(title, message, 
                        App.AppInformation, MessageDialogButtons.OkCancel);

                    customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                    {
                        if (value)
                            EnableDebugMode();
                        else
                            DisableDebugMode();

                        OnPropertyChanged("IsDebugMode");
                    };

                    customMessageDialog.ShowDialog();
                });
            }
        }

        /// <summary>
        /// Boolean property to indicate if is necessary to show the DEBUG alert.
        /// <para>Is set to TRUE during the app launching if the DEBUG mode is enabled.
        /// Once the DEBUG alert has been shown, is set to FALSE.</para>
        /// </summary>
        private bool _showDebugAlert;
        public bool ShowDebugAlert
        {
            get { return _showDebugAlert; }
            set
            {
                _showDebugAlert = value;
                OnPropertyChanged("ShowDebugAlert");
            }
        }

        private bool _showMemoryInformation;
        public bool ShowMemoryInformation
        {
            get { return _showMemoryInformation; }
            set
            {
                _showMemoryInformation = value;
                ShowMemoryInformationText = _showMemoryInformation ? UiResources.On : UiResources.Off;
                OnPropertyChanged("ShowMemoryInformation");
            }
        }

        private string _showMemoryInformationText;
        public string ShowMemoryInformationText
        {
            get { return _showMemoryInformationText; }
            set
            {
                _showMemoryInformationText = value;
                OnPropertyChanged("ShowMemoryInformationText");
            }
        }

        #endregion
    }
}
