using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Services;

namespace MegaApp.Classes
{
    class AssociationUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            string tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());

            // URI association launch for MEGA.
            if (tempUri.Contains("mega://"))
            {
                if (tempUri.Contains("confirm"))
                {
                    // Go the confirm account page and add the confirm string as parameter

                    var extraParams = new Dictionary<string, string>(1)
                    {
                        {
                            "confirm",
                            System.Net.HttpUtility.UrlEncode(tempUri.Replace(@"/Protocol?encodedLaunchUri=",String.Empty))
                        }
                    };

                    return NavigateService.BuildNavigationUri(typeof(ConfirmAccountPage), NavigationParameter.UriLaunch, extraParams);
                }
                else
                {
                    var extraParams = new Dictionary<string, string>(1)
                    {
                        {
                            "filelink",
                            System.Net.HttpUtility.UrlEncode(tempUri.Replace(@"/Protocol?encodedLaunchUri=", String.Empty))
                        }
                    };

                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.ImportLinkLaunch, extraParams);
                }
            }

            // Users has selected MEGA app for operating system auto upload function 
            if (tempUri.Contains("ConfigurePhotosUploadSettings"))
            {
                // Launch to the auto-upload settings page.
                App.AppInformation.IsStartedAsAutoUpload = true;

                var extraParams = new Dictionary<string, string>(1)
                {
                    {
                        "ConfigurePhotosUploadSettings",
                        System.Net.HttpUtility.UrlEncode(tempUri.Replace(@"?Action=ConfigurePhotosUploadSettings",String.Empty))
                    }
                };

                return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.AutoCameraUpload, extraParams);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
