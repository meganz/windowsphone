using System.IO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using mega;
using MegaApp.Models;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a boolean value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert from boolean to a Visibility state
        /// </summary>
        /// <param name="value">Input boolean parameter</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">"True" for true:Visible, false:Collapsed, "False" for true:Collapsed, false:Visible</param>
        /// <param name="culture">Any specific culture information for the current thread</param>
        /// <returns>Visibility display state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool trueIsVisible = true;

            if(parameter != null)
                trueIsVisible = bool.Parse((string)parameter);

            if (value == null && trueIsVisible) return Visibility.Collapsed;
            if (value == null) return Visibility.Visible;

            if ((bool)value && trueIsVisible)
                return Visibility.Visible;

            if ((bool)value && !trueIsVisible)
                return Visibility.Collapsed;

            return trueIsVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not yet needed in this application
            // Throw exception to check in testing if anything uses this method
            throw new NotImplementedException();
        }
    }
}
