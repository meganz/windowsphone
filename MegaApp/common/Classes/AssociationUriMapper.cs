using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
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
                App.LinkInformation.Reset();

                // Process the URI
                tempUri = tempUri.Replace(@"/Protocol?encodedLaunchUri=", String.Empty);
                tempUri = UriService.ReformatUri(tempUri);

                App.LinkInformation.ActiveLink = tempUri;

                //File link - Open file link to import or download
                if (tempUri.Contains("https://mega.nz/#!"))
                {
                    var extraParams = new Dictionary<string, string>(1)
                    {
                        {
                            "filelink",
                            System.Net.HttpUtility.UrlEncode(tempUri)
                        }
                    };

                    // Needed to get the file link properly
                    if (tempUri.EndsWith("/"))
                        tempUri = tempUri.Remove(tempUri.Length - 1, 1);

                    App.LinkInformation.ActiveLink = tempUri;
                    App.LinkInformation.UriLink = UriLinkType.File;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.FileLinkLaunch, extraParams);
                }                
                // Confirm account link
                else if (tempUri.Contains("https://mega.nz/#confirm"))
                {
                    // Go the confirm account page and add the confirm string as parameter
                    var extraParams = new Dictionary<string, string>(1)
                    {
                        {
                            "confirm",
                            System.Net.HttpUtility.UrlEncode(tempUri)
                        }
                    };

                    App.LinkInformation.UriLink = UriLinkType.Confirm;                    
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.UriLaunch, extraParams);
                }
                //Folder link - Open folder link to import or download
                else if (tempUri.Contains("https://mega.nz/#F!"))
                {
                    var extraParams = new Dictionary<string, string>(1)
                    {
                        {
                            "folderlink",
                            System.Net.HttpUtility.UrlEncode(tempUri)
                        }
                    };

                    App.LinkInformation.ActiveLink = tempUri;
                    App.LinkInformation.UriLink = UriLinkType.Folder;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.FolderLinkLaunch, extraParams);
                }
                //Recovery Key backup link
                else if (tempUri.Contains("https://mega.nz/#backup")) 
                {
                    App.LinkInformation.UriLink = UriLinkType.Backup;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.UriLaunch,
                        new Dictionary<string, string>(1) { { "backup", String.Empty } });
                }
                //New sign up link - Incoming share or contact request (no MEGA account)
                else if (tempUri.Contains("https://mega.nz/#newsignup")) 
                {
                    App.LinkInformation.UriLink = UriLinkType.NewSignUp;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.UriLaunch,
                        new Dictionary<string, string>(1) { { "newsignup", System.Net.HttpUtility.UrlEncode(tempUri) } });
                }
                //Confirm cancel a MEGA account
                else if (tempUri.Contains("https://mega.nz/#cancel")) 
                {
                    App.LinkInformation.UriLink = UriLinkType.Cancel;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Recover link - Recover the password with the Recovery Key or park the account
                else if (tempUri.Contains("https://mega.nz/#recover")) 
                {
                    App.LinkInformation.UriLink = UriLinkType.Recover;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Verify the change of the email address of the MEGA account
                else if (tempUri.Contains("https://mega.nz/#verify"))
                {
                    App.LinkInformation.UriLink = UriLinkType.Verify;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.Normal);
                }
                //Contact request to an email with an associated account of MEGA
                else if (tempUri.Contains("https://mega.nz/#fm/ipc"))
                {
                    App.LinkInformation.UriLink = UriLinkType.FmIpc;
                    return NavigateService.BuildNavigationUri(typeof(MainPage), NavigationParameter.UriLaunch,
                        new Dictionary<string, string>(1) { { "fm/ipc", String.Empty } });
                }
                //Invalid link
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            AppMessages.AM_LoadFailed_Title,
                            AppMessages.AM_InvalidLink,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });

                    App.LinkInformation.Reset();
                    return new Uri("/Pages/MainPage.xaml", UriKind.Relative);
                }
            }

            // User has selected a folder shortcut
            if (tempUri.Contains("ShortCutBase64Handle"))
            {
                App.ShortCutBase64Handle = tempUri.Replace(@"/Pages/MainPage.xaml?ShortCutBase64Handle=", String.Empty);
            }

            // User has selected MEGA app for operating system auto upload function 
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
