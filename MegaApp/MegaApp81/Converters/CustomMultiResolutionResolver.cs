using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MegaApp.Converters
{
    public class CustomMultiResolutionResolver : IValueConverter
    {
        /// <summary>
        /// Gets the string included in the names of images that should be used on WVGA Screens.
        /// 
        /// </summary>
        private const string ScreenWvgaName = "Screen-WVGA";

        /// <summary>
        /// Gets the string included in the names of images that should be used on WXGA Screens.
        /// 
        /// </summary>
        private const string ScreenWxgaName = "Screen-WXGA";

        /// <summary>
        /// Gets the string included in the names of images that should be used on 720p Screens.
        /// 
        /// </summary>
        private const string Screen720pName = "Screen-720p";

        /// <summary>
        /// Gets the string included in the names of images that should be used on 1080p Screens.
        /// 
        /// </summary>
        public static string Screen1080pName = "Screen-1080p";

        /// <summary>
        /// Gets the file name separator character.
        /// 
        /// </summary>
        private const char Separator = '.';

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// 
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// 
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num1 = (int)parameter;
            string str1 = string.Empty;
            string str2;
            switch (num1)
            {
                case 100:
                    str2 = ScreenWvgaName;
                    break;
                case 150:
                case 225:
                    str2 = Screen720pName;
                    break;
                case 160:
                    str2 = ScreenWxgaName;
                    break;
                default:
                    str2 = Screen720pName;
                    break;
                //throw new InvalidOperationException("Unknown resolution type");
            }
            BitmapImage bitmapImage = value as BitmapImage;
            if (bitmapImage == null)
                return (object)(value as WriteableBitmap);
            Uri uriSource = bitmapImage.UriSource;
            if (uriSource == (Uri)null)
                return (object)bitmapImage;
            string str3 = uriSource.ToString();
            int num2 = str3.LastIndexOf(Separator);
            string str4 = str3.Substring(0, num2);
            string str5 = str3.Substring(num2, str3.Length - num2);
            return (object)new Uri(string.Concat(new object[4]
      {
        (object) str4,
        (object) Separator,
        (object) str2,
        (object) str5
      }), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// 
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// 
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
