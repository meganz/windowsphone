using System;
using System.Linq;
using mega;
using Microsoft.Xna.Framework.Media;

namespace ScheduledCameraUploadTaskAgent
{
    /// <summary>
    /// Service that processes errors of the Camera Upload Service
    /// </summary>
    public static class ErrorProcessingService
    {
        private const int MaxNumberOfErrors = 10;
        
        /// <summary>
        /// Process an error occurring on service upload
        /// </summary>
        /// <param name="errorString">The error message</param>
        /// <param name="fileName">The file that failed</param>
        /// <param name="dateTime">The datetime of the file that failed, if available</param>
        public static void ProcessFileError(string errorString, string fileName, DateTime? dateTime = null)
        {
            // Process the error of the current file transfer
            var errorCount = LogFileError(errorString, fileName);

            // Skip the current file if it has failed to upload a number of times (MaxNumberOfErrors).
            if (errorCount < MaxNumberOfErrors) return;

            // Skip the file by setting it's date as last upload date
            SkipFile(fileName, dateTime);
        }

        /// <summary>
        /// Log a file upload error to log file and save the filename and error count of that specific file
        /// </summary>
        /// <param name="errorString">The error message</param>
        /// <param name="fileName">The file that failed</param>
        /// <returns></returns>
        private static int LogFileError(string errorString, string fileName)
        {
            try
            {
                // Log the error
                MegaSDK.log(MLogLevel.LOG_LEVEL_ERROR, "Error during the item upload");
                MegaSDK.log(MLogLevel.LOG_LEVEL_ERROR, errorString, fileName);

                // Load filename last error
                var lastErrorFileName = SettingsService.LoadSettingFromFile<string>("ErrorFileName");

                // Check if it is the same file that has an error again
                if (!string.IsNullOrEmpty(lastErrorFileName))
                {
                    if (lastErrorFileName.Equals(fileName))
                    {
                        // If the same file, add to the error count
                        var count = SettingsService.LoadSettingFromFile<int>("FileErrorCount");
                        count++;
                        SettingsService.SaveSettingToFile("FileErrorCount", count);
                        return count;
                    }
                }

                // New file error. Save the name and set the error count to one.
                SettingsService.SaveSettingToFile("ErrorFileName", fileName);
                SettingsService.SaveSettingToFile("FileErrorCount", 1);
                return 1;
            }
            catch (Exception)
            {
                // Do not let the error process cause the main service to generate a fault
                return 0;
            }
        }

        /// <summary>
        /// Skip the file for upload after a number of attempts and failures
        /// </summary>
        /// <param name="fileName">The file that failed and should be skipped</param>
        /// <param name="dateTime">The datetime of the file that should be skipped</param>
        private static void SkipFile(string fileName, DateTime? dateTime = null)
        {
            try
            {
                DateTime lastUploadDate;
                if (dateTime.HasValue)
                {
                    lastUploadDate = dateTime.Value;
                }
                else
                {
                    using (var mediaLibrary = new MediaLibrary())
                    {
                        using (var picture = mediaLibrary.Pictures.FirstOrDefault(p => p.Name.Equals(fileName)))
                        {
                            if (picture == null) return;
                            lastUploadDate = picture.Date;
                        }
                    }
                }

                SettingsService.SaveSettingToFile<DateTime>("LastUploadDate", lastUploadDate);
                Clear();
            }
            catch
            {
                // Do not let the error process cause the main service to generate a fault
            }
            
        }

        /// <summary>
        /// Clear filename and error count for error processing
        /// </summary>
        public static void Clear()
        {
            SettingsService.SaveSettingToFile("ErrorFileName", string.Empty);
            SettingsService.SaveSettingToFile("FileErrorCount", 0);
        }
    }
}
