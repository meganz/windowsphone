using System.Windows;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using System;
using System.Collections.Generic;
using Microsoft.Phone.Controls;

namespace MegaApp.Services
{
    public static class NavigateService
    {
        public static Type PreviousPage { get; private set; }

        public static bool CanGoBack()
        {
            return ((PhoneApplicationFrame)Application.Current.RootVisual).CanGoBack;
        }

        public static void GoBack()
        {            
            try
            {
                if (CanGoBack())
                    ((PhoneApplicationFrame)Application.Current.RootVisual).GoBack();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("NavigateService - GoBack");
            }            
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam, IDictionary<string, string> extraParams)
        {
            try
            {
                PreviousPage = navPage;
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam, extraParams));
            }
            catch (InvalidOperationException) { }
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam, object data)
        {
            try
            {
                PreviousPage = navPage;
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam), data);
            }
            catch (InvalidOperationException) { }
        }

        public static void NavigateTo(Type navPage, NavigationParameter navParam)
        {
            try
            {
                PreviousPage = navPage;
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(BuildNavigationUri(navPage, navParam));
            }
            catch (InvalidOperationException) { }
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
