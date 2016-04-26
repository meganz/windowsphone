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
                //File link - Open file link to import or download
                if (tempUri.Contains("mega:///#!") || tempUri.Contains("mega://!"))
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
                // Confirm account link
                else if (tempUri.Contains("mega:///#confirm"))
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
                //Folder link - Open folder link to import or download
                else if (tempUri.Contains("mega:///#F!"))
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Master Key backup link
                else if (tempUri.Contains("mega:///#backup")) 
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //New sign up link - Incoming share or contact request (no MEGA account)
                else if (tempUri.Contains("mega:///#newsignup")) 
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Confirm cancel a MEGA account
                else if (tempUri.Contains("mega:///#cancel")) 
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Recover link - Recover the password with the master key or park the account
                else if (tempUri.Contains("mega:///#recover")) 
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Verify the change of the email address of the MEGA account
                else if (tempUri.Contains("mega:///#verify"))
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Contact request to an email with an associated account of MEGA
                else if (tempUri.Contains("mega:///#fm/ipc"))
                {
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
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
