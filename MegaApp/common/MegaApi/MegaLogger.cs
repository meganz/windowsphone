//#define DEBUG
//Uncomment this line to show debug logs even on Release builds
//Only debug levels FATAL, ERROR, WARNING and INFO can be shown in Release builds
//DEBUG and MAX are reserved for Debug builds

using System;
using System.Diagnostics;
using System.IO;
using mega;

#if CAMERA_UPLOADS_SERVICE
using Windows.Storage;
#else
using MegaApp.Services;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace ScheduledCameraUploadTaskAgent.MegaApi
#else
namespace MegaApp.MegaApi
#endif
{
    class MegaLogger : MLoggerInterface
    {
        public virtual void log(string time, int loglevel, string source, string message)
        {
            string logLevelString;
            switch ((MLogLevel)loglevel)
            {
                case MLogLevel.LOG_LEVEL_DEBUG:
                    logLevelString = " (debug): ";
                    break;
                case MLogLevel.LOG_LEVEL_ERROR:
                    logLevelString = " (error): ";
                    break;
                case MLogLevel.LOG_LEVEL_FATAL:
                    logLevelString = " (fatal): ";
                    break;
                case MLogLevel.LOG_LEVEL_INFO:
                    logLevelString = " (info):  ";
                    break;
                case MLogLevel.LOG_LEVEL_MAX:
                    logLevelString = " (verb):  ";
                    break;
                case MLogLevel.LOG_LEVEL_WARNING:
                    logLevelString = " (warn):  ";
                    break;
                default:
                    logLevelString = " (none):  ";
                    break;
            }

            if (!string.IsNullOrEmpty(source))
            {
                int index = source.LastIndexOf('\\');
                if (index >= 0 && source.Length > (index + 1))
                {
                    source = source.Substring(index + 1);
                }
                message += " (" + source + ")";
            }

#if CAMERA_UPLOADS_SERVICE
            Debug.WriteLine("{0}{1}CAMERA UPLOADS - {2}", time, logLevelString, message);

            // If the APP log file exists (DEBUG mode enabled) append the "CAMERA UPLOADS" log messages.
            string logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "offline", "MEGA_UWP.log");
            if (File.Exists(logFilePath))
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(logFilePath))
                    {
                        sw.WriteLine("{0}{1}CAMERA UPLOADS - {2}", time, logLevelString, message);
                    }
                }
                catch (Exception) { }
            }
#else
            Debug.WriteLine("{0}{1}{2}", time, logLevelString, message);

            if (DebugService.DebugSettings != null && DebugService.DebugSettings.IsDebugMode)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(AppService.GetFileLogPath()))
                    {
                        sw.WriteLine("{0}{1}{2}", time, logLevelString, message);
                        sw.Flush();
                    }
                }
                catch (Exception) { }
            }
#endif
        }
    }
}
