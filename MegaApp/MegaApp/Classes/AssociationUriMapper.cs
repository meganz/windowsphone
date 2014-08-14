using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
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
            if (tempUri.Contains("mega:"))
            {
                // TODO: Get the information to confirm account 

                // Map the show products request to ShowProducts.xaml
                return NavigateService.BuildNavigationUri(typeof (LoginPage), NavigationParameter.UriLaunch);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
