using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;

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
    }
}
