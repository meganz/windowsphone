using System.IO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Converters
{
    public class NodeDisplayModeToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Transparent);

            switch ((NodeDisplayMode)value)
            {
                case NodeDisplayMode.Normal:
                    return new SolidColorBrush(Colors.Transparent);
                case NodeDisplayMode.SelectedForMove:
                {
                    var solidColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"])
                    {
                        // Specify culture independent number information
                        // Parameter is always with , (resource)
                        // Else the number will be converted by the current culture on the phone
                        Opacity = System.Convert.ToDouble(parameter, new NumberFormatInfo()
                        {
                            NumberDecimalSeparator = ","
                           
                        })
                    };
                    return solidColor;
                }
                
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
