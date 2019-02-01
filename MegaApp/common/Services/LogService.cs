using System;
using System.IO;
using System.Runtime.CompilerServices;
using mega;

#if CAMERA_UPLOADS_SERVICE
using ScheduledCameraUploadTaskAgent.MegaApi;
#else
using MegaApp.MegaApi;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace ScheduledCameraUploadTaskAgent.Services
#else
namespace MegaApp.Services
#endif
{
    /// <summary>
    /// Helper service to send logs to the loggin system
    /// </summary>
    static class LogService
    {
        /// <summary>
        /// Main MegaLogger instance
        /// </summary>
        private static MegaLogger _megaLogger;
        public static MegaLogger MegaLogger
        {
            get
            {
                if (_megaLogger != null) return _megaLogger;
                _megaLogger = new MegaLogger();
                return _megaLogger;
            }
        }

        /// <summary>
        /// Add a MegaLogger implementation to receive logs
        /// </summary>
        /// <param name="megaLogger">MegaLogger implementation</param>
        public static void AddLoggerObject(MLoggerInterface megaLogger)
        {
            SdkService.MegaSdk.addLoggerObject(megaLogger);
        }

        /// <summary>
        /// Remove a MegaLogger implementation to stop receiving SDK logs
        /// </summary>
        /// <param name="megaLogger">MegaLogger implementation</param>
        public static void RemoveLoggerObject(MLoggerInterface megaLogger)
        {
            SdkService.MegaSdk.removeLoggerObject(megaLogger);
        }

        /// <summary>
        /// Set the active log level
        /// </summary>
        /// <param name="logLevel">Active log level</param>
        public static void SetLogLevel(MLogLevel logLevel)
        {
            MegaSDK.setLogLevel(logLevel);
        }

        /// <summary>
        /// Send a log to the logging system
        /// </summary>
        /// <param name="logLevel">Log level for this message</param>
        /// <param name="message">Message for the logging system</param>
        /// <param name="file">Origin of the log message</param>
        /// <param name="line">Line of code where this message was generated</param>
        public static void Log(MLogLevel logLevel, string message,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            MegaSDK.log(logLevel, String.Format("{0} ({1}:{2})",
                message, Path.GetFileName(file), line));
        }

        /// <summary>
        /// Send a log to the logging system
        /// </summary>
        /// <param name="logLevel">Log level for this message</param>
        /// <param name="message">Message for the logging system</param>
        /// <param name="e">Exception which produced the error</param>
        /// <param name="file">Origin of the log message</param>
        /// <param name="line">Line of code where this message was generated</param>
        public static void Log(MLogLevel logLevel, string message, Exception e,
            [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            if (e == null)
            {
                Log(logLevel, message, file, line);
                return;
            }

            MegaSDK.log(logLevel, String.Format("{0} [{1} - {2}] ({3}:{4})", message,
                e.GetType().ToString(), e.Message.Replace(Environment.NewLine, " "),
                Path.GetFileName(file), line));
        }
    }
}
