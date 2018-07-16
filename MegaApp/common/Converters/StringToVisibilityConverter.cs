using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a string value to a Visibility state (Visible/Collapsed)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert from string to a Visibility state
        /// </summary>
        /// <param name="value">Input string parameter</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Any specific culture information for the current thread</param>
        /// <returns>Visibility display state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace((string)value))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not yet needed in this application
            // Throw exception to check in testing if anything uses this method
            throw new NotImplementedException();
        }
    }
}
