using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Info;
using Windows.Storage;
using mega;
using MegaApp.MegaApi;

namespace ScheduledCameraUploadTaskAgent.Services
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
            MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Set the ID for statistics
            MegaSDK.setStatsID(Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId")));
        }

        /// <summary>
        /// Create a MegaSDK instance
        /// </summary>
        /// <returns>The new MegaSDK instance</returns>
        private static MegaSDK CreateSdk()
        {
            String folderCameraUploadService = Path.Combine(ApplicationData.Current.LocalFolder.Path, "CameraUploadService");
            if (!Directory.Exists(folderCameraUploadService))
                Directory.CreateDirectory(folderCameraUploadService);

            return new MegaSDK(
                "Z5dGhQhL",
                String.Format("{0}/{1}/{2}",
                    ScheduledAgent.GetBackgroundAgentUserAgent(),
                    DeviceStatus.DeviceManufacturer,
                    DeviceStatus.DeviceName),
                folderCameraUploadService,
                new MegaRandomNumberProvider());
        }
    }
}
