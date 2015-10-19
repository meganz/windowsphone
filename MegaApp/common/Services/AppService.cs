using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Resources;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Phone.System.Memory;
using Windows.Storage;
using Windows.Storage.Pickers;
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
            return String.Format("933e457");
        }

        public static string GetAppUserAgent()
        {
            #if WINDOWS_PHONE_80
                return String.Format("MEGAWindowsPhone80/{0}", GetAppVersion());
            #elif WINDOWS_PHONE_81
                return String.Format("MEGAWindowsPhone81/{0}", GetAppVersion());
            #endif
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

        /// <summary>
        /// Create working directories for the app to use if they do not exist yet
        /// </summary>
        public static void InitializeAppFolders()
        {
            string thumbnailDir = GetThumbnailDirectoryPath();
            if (!Directory.Exists(thumbnailDir)) Directory.CreateDirectory(thumbnailDir);

            string previewDir = GetPreviewDirectoryPath();
            if (!Directory.Exists(previewDir)) Directory.CreateDirectory(previewDir);

            string downloadDir =GetDownloadDirectoryPath();
            if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir);

            string uploadDir = GetUploadDirectoryPath();
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
        }

        public static ulong GetAppCacheSize()
        {
            var files = new List<string>();
            //files.AddRange(Directory.GetFiles(ApplicationData.Current.LocalFolder.Path));
            files.AddRange(Directory.GetFiles(GetThumbnailDirectoryPath()));
            files.AddRange(Directory.GetFiles(GetPreviewDirectoryPath()));
            files.AddRange(Directory.GetFiles(GetDownloadDirectoryPath()));
            files.AddRange(Directory.GetFiles(GetUploadDirectoryPath()));

            ulong totalSize = 0;
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalSize += (ulong)fileInfo.Length;
            }

            return totalSize;
        }

        public static void ClearAppCache(bool includeLocalFolder)
        {
            if (includeLocalFolder)
                ClearLocalCache();
            ClearThumbnailCache();
            ClearPreviewCache();
            ClearDownloadCache();
            ClearUploadCache();
        }

        public static void ClearThumbnailCache()
        {
            string thumbnailDir = GetThumbnailDirectoryPath();
            if (Directory.Exists(thumbnailDir))
            {
                FileService.ClearFiles(Directory.GetFiles(thumbnailDir));
            }
        }

        public static void ClearPreviewCache()
        {
            string previewDir = GetPreviewDirectoryPath();
            if (Directory.Exists(previewDir))
            {
                FileService.ClearFiles(Directory.GetFiles(previewDir));
            } 
        }

        public static void ClearDownloadCache()
        {
            string downloadDir = GetDownloadDirectoryPath();
            if (Directory.Exists(downloadDir))
            {
                FileService.ClearFiles(Directory.GetFiles(downloadDir));
            }
        }
        public static void ClearUploadCache()
        {
            string uploadDir = GetUploadDirectoryPath();
            if (Directory.Exists(uploadDir))
            {
                FileService.ClearFiles(Directory.GetFiles(uploadDir));
            }
        }

        public static void ClearLocalCache()
        {
            FileService.ClearFiles(Directory.GetFiles(ApplicationData.Current.LocalFolder.Path));
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
                AppResources.DefaultDownloadLocation));
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
                String.Format(AppMessages.DownloadLimitMessage, downloadCount),
                App.AppInformation,
                MessageDialogButtons.Ok).ShowDialogAsync();

            return result == MessageDialogResult.OkYes;
        }

        public static void LogoutActions()
        {
            // Disable the "camera upload" service
            MediaService.SetAutoCameraUpload(false);
            SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);

            // Clear settings, cache, previews, thumbnails, etc.
            SettingsService.ClearMegaLoginData();
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App.MainPageViewModel.CloudDrive.ChildNodes.Clear();
                App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            });
            AppService.ClearAppCache(false);            
        }
    }
}
