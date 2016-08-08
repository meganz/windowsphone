//#define DEBUG
//Uncomment this line to show debug logs even on Release builds
//Only debug levels FATAL, ERROR, WARNING and INFO can be shown in Release builds
//DEBUG and MAX are reserved for Debug builds

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using Windows.Storage;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaLogger : MLoggerInterface
    {
        public virtual void log(string time, int loglevel, string source, string message)
        {
            String logLevelString;
            switch((MLogLevel)loglevel)
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

            if(!string.IsNullOrEmpty(source))
            {
                int index = source.LastIndexOf('\\');
                if(index >=0 && source.Length > (index + 1))
                {
                   source = source.Substring(index + 1);
                }
                message += " (" + source + ")"; 
            }

            Debug.WriteLine("{0}{1}CAMERA UPLOADS - {2}", time, logLevelString, message);

            // If the APP log file exists (DEBUG mode enabled) append the "Camera Uploads" log messages.
            String logFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "downloads", "MEGA_WP.log");
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
        }
    }
}
