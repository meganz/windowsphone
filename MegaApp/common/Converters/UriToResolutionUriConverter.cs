using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MegaApp.Services;


namespace MegaApp.Converters
{
    public class UriToResolutionUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new Uri("/Assets/FileTypes/ListView/" + ImageService.GetDefaultFileImage(String.Empty));

            if (System.Convert.ToString(value).Contains(@"file:///")) return value;

            switch (System.Convert.ToInt32(parameter))
            {
                case 0:
                {
                    return new Uri("/Assets/FileTypes/ListView/" + value, UriKind.Relative);
                }
                case 1:
                {
                    return new Uri("/Assets/FileTypes/ThumbView/" + value, UriKind.Relative);
                }
                default: return new Uri("/Assets/FileTypes/ListView/" + value, UriKind.Relative);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
