using MegaApp.Resources;
using System.IO;
using System.Xml;
using Windows.Storage;

namespace MegaApp.Services
{
    static class AppService
    {
        public static string GetAppVersion()
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                XmlResolver = new XmlXapResolver()
            };

            using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
            {
                xmlReader.ReadToDescendant("App");

                return xmlReader.GetAttribute("Version");
            }

            // TODO When moving to WP 8.1 use code below

            //return String.Format("{0}.{1}.{2}.{3}",
            //    Package.Current.Id.Version.Major,
            //    Package.Current.Id.Version.Minor,
            //    Package.Current.Id.Version.Build,
            //    Package.Current.Id.Version.Revision);
        }

        /// <summary>
        /// Create working directories for the app to use if they do not exist yet
        /// </summary>
        public static void InitializeAppFolders()
        {
            string thumbnailDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory);
            if (!Directory.Exists(thumbnailDir)) Directory.CreateDirectory(thumbnailDir);

            string previewDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.PreviewsDirectory);
            if (!Directory.Exists(previewDir)) Directory.CreateDirectory(previewDir);

            string downloadDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory);
            if (!Directory.Exists(downloadDir)) Directory.CreateDirectory(downloadDir);
        }
    }
}
