using System;
using System.Collections.Generic;
using System.Globalization;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Extensions;
using MegaApp.Resources;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Storage;
using Microsoft.Phone.Info;

#if WINDOWS_PHONE_81
    using MemoryManager = Windows.System.MemoryManager;
#endif

namespace MegaApp.Services
{
    static class AppService
    {
        private const int DownloadLimit = 100;

        public static Resolutions CurrentResolution;

        public static string GetAppVersion()
        {
            #if WINDOWS_PHONE_80
                var xmlReaderSettings = new XmlReaderSettings
                {
                    XmlResolver = new XmlXapResolver()
                };

                using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
                {
                    xmlReader.ReadToDescendant("App");

                    return xmlReader.GetAttribute("Version");
                }
            #elif WINDOWS_PHONE_81
                return String.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            #endif
        }

        public static string GetMegaSDK_Version()
        {
            return AppResources.AR_SdkVersion;
        }

        public static string GetAppUserAgent()
        {
            #if WINDOWS_PHONE_80
                return String.Format("MEGAWindowsPhone80/{0}", GetAppVersion());
            #elif WINDOWS_PHONE_81
                return String.Format("MEGAWindowsPhone81/{0}", GetAppVersion());
            #endif
        }

        /// <summary>
        /// Get the code of the language used by the app
        /// </summary>
        /// <returns>Code of the language used by the app or NULL if fails</returns>
        public static string GetAppLanguageCode()
        {
            try
            {
                CultureInfo ci = CultureInfo.CurrentUICulture;
                var languageCode = ci.TwoLetterISOLanguageName;

                switch (languageCode)
                {
                    case null:
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting the app language code");
                        return string.Empty;
                    case "pt":
                        return (ci.Name.Equals("pt-BR")) ? ci.Name : languageCode;
                    case "zh":
                        return (ci.Name.Equals("zh-HANS") || ci.Name.Equals("zh-HANT")) ? ci.Name : languageCode;
                    default:
                        return languageCode;
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting the app language code", e);
                return string.Empty;
            }
        }

        /// <summary>
        /// Initialize the DB (create tables if no exist).
        /// </summary>
        public static void InitializeDatabase()
        {
            SavedForOffline.CreateTable();
        }

        public static MemoryInformation GetAppMemoryUsage()
        {
            #if WINDOWS_PHONE_80
                return new MemoryInformation()
                {
                    AppMemoryUsage = (ulong) DeviceStatus.ApplicationCurrentMemoryUsage,
                    AppMemoryLimit = (ulong) DeviceStatus.ApplicationMemoryUsageLimit,
                    AppMemoryPeak = (ulong) DeviceStatus.ApplicationPeakMemoryUsage,
                    DeviceMemory = (ulong) DeviceStatus.DeviceTotalMemory
                };
            #elif WINDOWS_PHONE_81
                return new MemoryInformation()
                {
                    AppMemoryUsage = MemoryManager.AppMemoryUsage,
                    AppMemoryLimit = MemoryManager.AppMemoryUsageLimit,
                    AppMemoryPeak = (ulong)DeviceStatus.ApplicationPeakMemoryUsage,
                    DeviceMemory = (ulong)DeviceStatus.DeviceTotalMemory
                };
            #endif
        }

        public static bool IsLowMemoryDevice()
        {
            #if WINDOWS_PHONE_80
                return (ulong) DeviceStatus.ApplicationMemoryUsageLimit < 200UL.FromMBToBytes();
            #elif WINDOWS_PHONE_81
                return MemoryManager.AppMemoryUsageLimit < 200UL.FromMBToBytes();
            #endif
        }

        public static ulong MaxMemoryUsage()
        {
            #if WINDOWS_PHONE_80
                return (ulong) DeviceStatus.ApplicationMemoryUsageLimit;
            #elif WINDOWS_PHONE_81
                return MemoryManager.AppMemoryUsageLimit;
            #endif
        }

        /// <summary>
        /// Create working directories for the app to use if they do not exist yet
        /// </summary>
        public static void InitializeAppFolders()
        {
            try
            {
                string thumbnailDir = GetThumbnailDirectoryPath();
                if (!Directory.Exists(thumbnailDir)) Directory.CreateDirectory(thumbnailDir);

                string previewDir = GetPreviewDirectoryPath();
                if (!Directory.Exists(previewDir)) Directory.CreateDirectory(previewDir);

                string downloadDir = GetDownloadDirectoryPath();
                if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir);

                string uploadDir = GetUploadDirectoryPath();
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            }
            catch (IOException) { }
        }

        /// <summary>
        /// Get the size of the app cache
        /// </summary>
        /// <returns>App cache size</returns>
        public static async Task<ulong> GetAppCacheSizeAsync()
        {
            ulong totalSize = 0;

            await Task.Run(() =>
            {
                var files = new List<string>();

                try { files.AddRange(Directory.GetFiles(GetThumbnailDirectoryPath())); }
                catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error getting thumbnails cache.", e); }

                try { files.AddRange(Directory.GetFiles(GetPreviewDirectoryPath())); }
                catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error getting previews cache.", e); }

                try { files.AddRange(Directory.GetFiles(GetUploadDirectoryPath())); }
                catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error getting uploads cache.", e); }

                try { files.AddRange(GetDownloadDirectoryFiles(GetDownloadDirectoryPath())); }
                catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error getting downloads cache.", e); }

                foreach (var file in files)
                {
                    if (FileService.FileExists(file))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            totalSize += (ulong)fileInfo.Length;
                        }
                        catch (Exception e)
                        {
                            LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error getting app cache size.", e);
                        }
                    }
                }
            });

            return totalSize;
        }

        private static List<string> GetDownloadDirectoryFiles(string path)
        {
            var files = new List<string>();

            if(FolderService.FolderExists(path))
            {
                try
                {
                    files.AddRange(Directory.GetFiles(path));

                    var folders = new List<string>();
                    folders.AddRange(Directory.GetDirectories(path));
                    
                    foreach (var folder in folders)
                        files.AddRange(GetDownloadDirectoryFiles(folder));
                }
                catch (Exception e) { throw e.GetBaseException(); }
            }
            
            return files;
        }

        /// <summary>
        /// Clear the app cache
        /// </summary>
        /// <param name="includeLocalFolder">Flag to indicate if clear the app local cache.</param>
        /// <returns>TRUE if the cache was successfully deleted or FALSE otherwise.</returns>
        public static bool ClearAppCache(bool includeLocalFolder = false)
        {
            bool result = true;
            
            result = result & ClearThumbnailCache();
            result = result & ClearPreviewCache();
            result = result & ClearDownloadCache();
            result = result & ClearUploadCache();

            result = result & ClearAppDatabase();

            if (includeLocalFolder)
                result = result & ClearLocalCache();

            return result;
        }

        public static bool ClearAppDatabase()
        {
            return SavedForOffline.DeleteAllNodes();
        }

        public static bool ClearThumbnailCache()
        {
            string thumbnailDir = GetThumbnailDirectoryPath();
            if (String.IsNullOrWhiteSpace(thumbnailDir) || FolderService.HasIllegalChars(thumbnailDir) || 
                !Directory.Exists(thumbnailDir)) return false;
            
            return FileService.ClearFiles(Directory.GetFiles(thumbnailDir));
        }

        public static bool ClearPreviewCache()
        {
            string previewDir = GetPreviewDirectoryPath();
            if (String.IsNullOrWhiteSpace(previewDir) || FolderService.HasIllegalChars(previewDir) ||
                !Directory.Exists(previewDir)) return false;
            
            return FileService.ClearFiles(Directory.GetFiles(previewDir));
        }

        public static bool ClearDownloadCache()
        {
            string downloadDir = GetDownloadDirectoryPath();
            if (String.IsNullOrWhiteSpace(downloadDir) || FolderService.HasIllegalChars(downloadDir) ||
                !Directory.Exists(downloadDir)) return false;
            
            return FolderService.Clear(downloadDir);
        }

        public static bool ClearUploadCache()
        {
            string uploadDir = GetUploadDirectoryPath();
            if (String.IsNullOrWhiteSpace(uploadDir) || FolderService.HasIllegalChars(uploadDir) ||
                !Directory.Exists(uploadDir)) return false;
            
            return FileService.ClearFiles(Directory.GetFiles(uploadDir));
        }

        public static bool ClearLocalCache()
        {
            string localCacheDir = ApplicationData.Current.LocalFolder.Path;
            if (String.IsNullOrWhiteSpace(localCacheDir) || FolderService.HasIllegalChars(localCacheDir) ||
                !Directory.Exists(localCacheDir)) return false;
            
            return FileService.ClearFiles(Directory.GetFiles(localCacheDir));
        }

        public static string GetUploadDirectoryPath(bool checkIfExists = false)
        {
            var uploadDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.UploadsDirectory);

            if (checkIfExists)
            {
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);
            }

            return uploadDir;
        }

        public static string GetDownloadDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory);
        }

        public static string GetPreviewDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.PreviewsDirectory);
        }

        public static string GetThumbnailDirectoryPath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory);
        }

        public static string GetSelectedDownloadDirectoryPath()
        {
            return Path.Combine(SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                UiResources.DefaultDownloadLocation));
        }

        /// <summary>
        /// Gets the log file path created in DEBUG mode.
        /// </summary>
        /// <returns>Log file path.</returns>
        public static string GetFileLogPath()
        {
            return Path.Combine(GetDownloadDirectoryPath(), AppResources.LogFileName);
        }

        public static void ClearObsoleteSettings()
        {
            var lastAppVersion = SettingsService.LoadSetting<string>(SettingsResources.LastAppVersion, null);

            if (lastAppVersion == null)
            {
                SettingsService.DeleteSetting(SettingsResources.QuestionAskedDownloadOption);
            }
        }

        public static void SaveAppInformation()
        {
            SettingsService.SaveSetting(SettingsResources.LastAppVersion, AppService.GetAppVersion());
        }

        public static async Task<bool> DownloadLimitCheck(int downloadCount)
        {
            if (downloadCount <= DownloadLimit) return true;

            var result = await new CustomMessageDialog(
                AppMessages.DownloadLimitMessage_Title,
                String.Format(AppMessages.DownloadLimitMessage, DownloadLimit, downloadCount),
                App.AppInformation,
                MessageDialogButtons.Ok).ShowDialogAsync();

            return result == MessageDialogResult.OkYes;
        }

        public static void LogoutActions()
        {
            // Disable the "camera upload" service if is enabled
            if(MediaService.GetAutoCameraUploadStatus())
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Disabling CAMERA UPLOADS service (LOGOUT)");
                MediaService.SetAutoCameraUpload(false);
            }            
            
            // Clear settings, cache, previews, thumbnails, etc.
            SettingsService.ClearSettings();
            SettingsService.ClearMegaLoginData();
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Added extra checks preventing null reference exceptions
                if (App.MainPageViewModel == null) return;
                
                if (App.MainPageViewModel.CloudDrive != null) 
                    App.MainPageViewModel.CloudDrive.ChildNodes.Clear();

                if (App.MainPageViewModel.RubbishBin != null) 
                    App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            });
            AppService.ClearAppCache(true);  
          
            // Delete Account Details info
            AccountService.ClearAccountDetails();
        }
    }
}
