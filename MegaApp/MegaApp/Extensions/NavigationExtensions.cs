using System;
using System.Windows.Controls;

namespace MegaApp.Extensions
{
    static class NavigationExtensions
    {
        private static object _data;

        /// <summary>
        /// Navigates to the content specified by uniform resource identifier (URI).
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="source">The URI of the content to navigate to.</param>
        /// <param name="data">The data that you need to pass to the other page 
        /// specified in URI.</param>
        public static bool Navigate(this Frame navigationService, Uri source, object data)
        {
            _data = data;
            return navigationService.Navigate(source);
        }

        /// <summary>
        /// Gets the navigation data passed from the previous page.
        /// </summary>
        /// <param name="navigationService">The service.</param>
        /// <returns>System.Object.</returns>
        public static object GetNavigationData(this Frame navigationService)
        {
            return _data;
        }
    }
}
