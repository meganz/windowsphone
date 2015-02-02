using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Resources;
using System.IO;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.Phone.Info;

namespace MegaApp.Services
{
    static class AppService
    {
        public static Resolutions CurrentResolution;

        public static string GetAppVersion()
        {
            //var xmlReaderSettings = new XmlReaderSettings
            //{
            //    XmlResolver = new XmlXapResolver()
            //};

            //using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
            //{
            //    xmlReader.ReadToDescendant("App");

            //    return xmlReader.GetAttribute("Version");
            //}

            // TODO When moving to WP 8.1 use code below

            return String.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);
        }

        public static MemoryInformation GetAppMemoryUsage()
        {
            return new MemoryInformation()
            {
                AppMemoryUsage = (ulong) DeviceStatus.ApplicationCurrentMemoryUsage,
                AppMemoryLimit = (ulong) DeviceStatus.ApplicationMemoryUsageLimit,
                AppMemoryPeak = (ulong) DeviceStatus.ApplicationPeakMemoryUsage,
                DeviceMemory = (ulong) DeviceStatus.DeviceTotalMemory
            };
        }

        public static bool IsLowMemoryDevice()
        {
            return (ulong) DeviceStatus.ApplicationMemoryUsageLimit < 200UL.FromMBToBytes();
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
    }
}
