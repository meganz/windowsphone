using System.Linq;
using System.Windows;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Pages;
using MegaApp.Resources;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace MegaApp.Services
{
    public static class NavigateService
    {
        public static Type PreviousPage { get; private set; }

        public static void GoBack()
        {
            if(((PhoneApplicationFrame)Application.Current.RootVisual).CanGoBack)
                ((PhoneApplicationFrame)Application.Current.RootVisual).GoBack();
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam, IDictionary<string, string> extraParams)
        {
            PreviousPage = navPage;
            ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam, extraParams));
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam, object data)
        {
            PreviousPage = navPage;
            ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam), data);
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam)
        {
            PreviousPage = navPage;
            ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam));
        }
        
        public static Uri BuildNavigationUri(Type navPage, NavigationParameter navParam, IDictionary<string, string> extraParams)
        {
            var resultUrl = BuildNavigationUri(navPage, navParam).ToString();
            
            foreach (var extraParam in extraParams)
            {
                resultUrl += String.Format(@"&{0}={1}", extraParam.Key, extraParam.Value);
            }

            return new Uri(resultUrl, UriKind.Relative);
        }

        public static T GetNavigationData<T>()
        {
            return (T) ((PhoneApplicationFrame) Application.Current.RootVisual).GetNavigationData();
        }

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
                return NavigationParameter.None;
        }
    }
}
