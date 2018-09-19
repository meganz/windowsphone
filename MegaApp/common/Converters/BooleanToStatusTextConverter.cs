using System;
using System.Globalization;
using System.Windows.Data;
using MegaApp.Resources;

namespace MegaApp.Converters
{
    /// <summary>
    /// Class to convert from a boolean value to a Status string (On/Off)
    /// </summary>
    public class BooleanToStatusTextConverter : IValueConverter
    {
        /// <summary>
        /// Convert from boolean to a Status string (On/Off)
        /// </summary>
        /// <param name="value">Input boolean parameter</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">"True" for true:On, false:Off, "False" for true:Off, false:On</param>
        /// <param name="culture">Any specific culture information for the current thread</param>
        /// <returns>Status string</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool trueIsVisible = true;

            if (parameter != null)
                trueIsVisible = bool.Parse((string)parameter);

            if (value == null && trueIsVisible) return UiResources.Off;
            if (value == null) return UiResources.On;

            if ((bool)value && trueIsVisible)
                return UiResources.On;

            if ((bool)value && !trueIsVisible)
                return UiResources.Off;

            return trueIsVisible ? UiResources.Off : UiResources.On;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not yet needed in this application
            // Throw exception to check in testing if anything uses this method
            throw new NotImplementedException();
        }
    }
}
