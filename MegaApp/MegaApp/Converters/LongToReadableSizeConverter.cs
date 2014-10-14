using System.IO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using mega;
using MegaApp.Extensions;
using MegaApp.Models;

namespace MegaApp.Converters
{
    public class LongToReadableSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return ((ulong) 0).ToStringAndSuffix();

            return ((ulong)value).ToStringAndSuffix();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
