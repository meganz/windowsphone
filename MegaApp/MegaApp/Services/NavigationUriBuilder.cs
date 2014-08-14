using MegaApp.Classes;
using MegaApp.Resources;
using System;
using System.Collections.Generic;

namespace MegaApp.Services
{
    public static class NavigationUriBuilder
    {
        public static Uri BuildNavigationUri(Type navPage, NavigationParameter navParam)
        {
            if (navPage == null)
                throw new ArgumentNullException("navPage");

            var queryString = String.Format("?navparam={0}", Enum.GetName(typeof(NavigationParameter), navParam));

            return new Uri(String.Format("{0}{1}.xaml{2}", AppResources.PagesLocation, navPage.Name, queryString), UriKind.Relative);
        }

        public static NavigationParameter ProcessQueryString(IDictionary<string, string> queryString)
        {
            if (queryString.ContainsKey("navparam"))
                return (NavigationParameter) Enum.Parse(typeof (NavigationParameter), queryString["navparam"]);
            else
                return NavigationParameter.Unknown;
        }
    }
}
