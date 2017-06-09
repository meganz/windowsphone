using System;
using Microsoft.Phone.Info;
using Windows.Storage;
using mega;
using MegaApp.MegaApi;

namespace MegaApp.Services
{
    public static class SdkService
    {
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

        /// <summary>
        /// Initialize all the SDK parameters
        /// </summary>
        public static void InitializeSdkParams()
        {
            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            MegaSDK.addLoggerObject(new MegaLogger());

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Set the ID for statistics
            MegaSDK.setStatsID(Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId")));

            // Set the language code used by the app
            var appLanguageCode = AppService.GetAppLanguageCode();
            if (!MegaSdk.setLanguage(appLanguageCode))
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, 
                    string.Format("Invalid app language code '{0}'", appLanguageCode));
            }
        }

        /// <summary>
        /// Create a MegaSDK instance
        /// </summary>
        /// <returns>The new MegaSDK instance</returns>
        private static MegaSDK CreateSdk()
        {
            // Initialize a MegaSDK instance
            return new MegaSDK(
                "Z5dGhQhL",
                AppService.GetAppUserAgent(),
                ApplicationData.Current.LocalFolder.Path,
                new MegaRandomNumberProvider());
        }
    }
}
