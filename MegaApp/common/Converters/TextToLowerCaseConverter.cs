using System.IO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using mega;
using MegaApp.Models;

namespace MegaApp.Converters
{
    public class TextToLowerCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return String.Empty;

            return ((string)value).ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
