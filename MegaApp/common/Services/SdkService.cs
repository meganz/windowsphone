using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;

namespace MegaApp.Services
{
    public static class SdkService
    {
        #region Events

        /// <summary>
        /// Event triggered when the API URL is changed.
        /// </summary>
        public static event EventHandler ApiUrlChanged;

        /// <summary>
        /// Event invocator method called when the API URL is changed.
        /// </summary>
        private static void OnApiUrlChanged()
        {
            if (ApiUrlChanged != null)
                ApiUrlChanged.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Main MegaSDK instance of the app
        /// </summary>
        private static MegaSDK _megaSdk;
        public static MegaSDK MegaSdk
        {
            get
            {
                if (_megaSdk != null) return _megaSdk;
                _megaSdk = CreateSdk();
                return _megaSdk;
            }
        }

        /// <summary>
        /// MegaSDK instance for the folder links management
        /// </summary>
        private static MegaSDK _megaSdkFolderLinks;
        public static MegaSDK MegaSdkFolderLinks
        {
            get
            {
                if (_megaSdkFolderLinks != null) return _megaSdkFolderLinks;
                _megaSdkFolderLinks = CreateSdk();
                return _megaSdkFolderLinks;
            }
        }

        // Timer to count the actions needed to change the API URL.
        private static DispatcherTimer timerChangeApiUrl;

        #endregion

        #region Methods

        /// <summary>
        /// Initialize all the SDK parameters
        /// </summary>
        public static void InitializeSdkParams()
        {
            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            LogService.AddLoggerObject(LogService.MegaLogger);

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Set the ID for statistics
            try
            {
                MegaSDK.setStatsID(Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId")));
            }
            catch (NotSupportedException e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error setting the device unique ID for statistics", e);
            }

            // Set the language code used by the app
            var appLanguageCode = AppService.GetAppLanguageCode();
            if (!MegaSdk.setLanguage(appLanguageCode) || !MegaSdkFolderLinks.setLanguage(appLanguageCode))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, 
                    string.Format("Invalid app language code '{0}'", appLanguageCode));
            }

            // Change the API URL if required by settings
            if (SettingsService.LoadSetting<bool>(SettingsResources.UseStagingServer, false))
            {
                MegaSdk.changeApiUrl(AppResources.AR_StagingUrl);
                MegaSdkFolderLinks.changeApiUrl(AppResources.AR_StagingUrl);
            }
        }

        /// <summary>
        /// Create a MegaSDK instance
        /// </summary>
        /// <returns>The new MegaSDK instance</returns>
        private static MegaSDK CreateSdk()
        {
            // Initialize a MegaSDK instance
            var newMegaSDK = new MegaSDK(
                "Z5dGhQhL",
                AppService.GetAppUserAgent(),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());

            // Use custom DNS servers in the new SDK instance
            SetDnsServers(newMegaSDK);

            // Enable retrying when public key pinning fails
            newMegaSDK.retrySSLerrors(true);

            return newMegaSDK;
        }

        /// <summary>
        /// Use custom DNS servers in the selected SDK instance.
        /// </summary>
        /// <param name="megaSdk">SDK instance to set the custom DNS servers.</param>
        private static void SetDnsServers(MegaSDK megaSdk)
        {
            var dnsServers = NetworkService.GetMegaDnsServers(true);
            if (!string.IsNullOrWhiteSpace(dnsServers))
                megaSdk.setDnsServers(dnsServers);
        }

        /// <summary>
        /// Use custom DNS servers in all the SDK instances.
        /// </summary>
        public static void SetDnsServers()
        {
            var dnsServers = NetworkService.GetMegaDnsServers(true);
            if (!string.IsNullOrWhiteSpace(dnsServers))
            {
                SdkService.MegaSdk.setDnsServers(dnsServers);
                SdkService.MegaSdkFolderLinks.setDnsServers(dnsServers);
            }
        }

        /// <summary>
        /// Checks if a node exists by its name.
        /// </summary>
        /// <param name="searchNode">The parent node of the tree to explore.</param>
        /// <param name="name">Name of the node to search.</param>
        /// <param name="isFolder">True if the node to search is a folder or false in other case.</param>
        /// <param name="recursive">True if you want to seach recursively in the node tree.</param>
        /// <returns>True if the node exists or false in other case.</returns>
        public static bool ExistsNodeByName(MNode searchNode, string name, bool isFolder, bool recursive = false)
        {
            var searchResults = MegaSdk.search(searchNode, name, recursive);
            for (var i = 0; i < searchResults.size(); i++)
            {
                var node = searchResults.get(i);
                if (node.isFolder() == isFolder && node.getName().ToLower().Equals(name.ToLower()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Method that should be called when an action required for 
        /// change the API URL is started.
        /// </summary>
        public static void ChangeApiUrlActionStarted()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (timerChangeApiUrl == null)
                {
                    timerChangeApiUrl = new DispatcherTimer();
                    timerChangeApiUrl.Interval = new TimeSpan(0, 0, 5);
                    timerChangeApiUrl.Tick += (obj, args) => ChangeApiUrl();
                }
                timerChangeApiUrl.Start();
            });
        }

        /// <summary>
        /// Method that should be called when an action required for 
        /// change the API URL is finished.
        /// </summary>
        public static void ChangeApiUrlActionFinished()
        {
            StopChangeApiUrlTimer();
        }

        /// <summary>
        /// Change the API URL.
        /// </summary>
        private static async void ChangeApiUrl()
        {
            StopChangeApiUrlTimer();

            var useStagingServer = SettingsService.LoadSetting<bool>(SettingsResources.UseStagingServer, false);
            if (!useStagingServer)
            {
                var confirmDialog = new CustomMessageDialog("Change to a testing server?",
                    "Are you sure you want to change to a testing server? Your account may run irrecoverable problems.",
                    App.AppInformation, MessageDialogButtons.OkCancel);

                var result = await confirmDialog.ShowDialogAsync();
                confirmDialog.CloseDialog();
                if (result != MessageDialogResult.OkYes) return;
            }

            useStagingServer = !useStagingServer;

            var newApiUrl = useStagingServer ?
                AppResources.AR_StagingUrl : AppResources.AR_ApiUrl;

            MegaSdk.changeApiUrl(newApiUrl);
            MegaSdkFolderLinks.changeApiUrl(newApiUrl);

            SettingsService.SaveSetting<bool>(SettingsResources.UseStagingServer, useStagingServer);

            // If the user is logged in, do a new login with the current session
            if (Convert.ToBoolean(MegaSdk.isLoggedIn()))
            {
                bool fastLoginResult;
                try
                {
                    var fastLogin = new FastLoginRequestListenerAsync();
                    fastLoginResult = await fastLogin.ExecuteAsync(() =>
                        MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), fastLogin));
                }
                // Do nothing, app is already logging out
                catch (BadSessionIdException)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Login failed. Bad session ID.");
                    return;
                }

                if (fastLoginResult)
                {
                    // Fetch nodes from MEGA
                    var fetchNodes = new FetchNodesRequestListenerAsync();
                    var fetchNodesResult = await fetchNodes.ExecuteAsync(() => MegaSdk.fetchNodes(fetchNodes));
                    if (fetchNodesResult != FetchNodesResult.Success)
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Fetch nodes failed.");
                        new CustomMessageDialog(AppMessages.FetchingNodesFailed_Title, AppMessages.FetchingNodesFailed,
                            App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                    }
                }
                else
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Resume session failed.");
                    new CustomMessageDialog(UiResources.UI_ResumeSession, AppMessages.AM_ResumeSessionFailed,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                }
            }

            // Reset the "Camera Uploads" service if is enabled
            if (MediaService.GetAutoCameraUploadStatus())
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Resetting CAMERA UPLOADS service (API URL changed)");
                SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled,
                    MediaService.SetAutoCameraUpload(true));
            }

            new CustomMessageDialog(null, "API URL changed",
                App.AppInformation, MessageDialogButtons.Ok).ShowDialog();

            OnApiUrlChanged();
        }

        /// <summary>
        /// Stops the timer to detect an API URL change.
        /// </summary>
        private static void StopChangeApiUrlTimer()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (timerChangeApiUrl != null)
                    timerChangeApiUrl.Stop();
            });
        }

        #endregion
    }
}
