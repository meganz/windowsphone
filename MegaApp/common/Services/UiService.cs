using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Converters;
using MegaApp.Enums;
using Telerik.Windows.Controls;

namespace MegaApp.Services
{
    static class UiService
    {
        private static Dictionary<string, int> _folderSorting;
        private static Dictionary<string, int> _folderViewMode;

        public static int GetSortOrder(string folderBase64Handle, string folderName)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                return _folderSorting[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? (int)MSortOrderType.ORDER_MODIFICATION_DESC :
                (int)MSortOrderType.ORDER_DEFAULT_ASC;
        }

        public static void SetSortOrder(string folderBase64Handle, int sortOrder)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<string, int>();

            if (_folderSorting.ContainsKey(folderBase64Handle))
                _folderSorting[folderBase64Handle] = sortOrder;
            else
                _folderSorting.Add(folderBase64Handle, sortOrder);
        }

        public static ViewMode GetViewMode(string folderBase64Handle, string folderName)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                return (ViewMode)_folderViewMode[folderBase64Handle];

            return folderName.Equals("Camera Uploads") ? ViewMode.LargeThumbnails : ViewMode.ListView;
        }

        public static void SetViewMode(string folderBase64Handle, ViewMode viewMode)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<string, int>();

            if (_folderViewMode.ContainsKey(folderBase64Handle))
                _folderViewMode[folderBase64Handle] = (int)viewMode;
            else
                _folderViewMode.Add(folderBase64Handle, (int)viewMode);
        }

        public static RadCustomHubTile CreateHubTile(string title, Uri bitmapUri, Thickness margin)
        {
            var bitmapImage = new BitmapImage()
            {
                DecodePixelHeight = 128,
                DecodePixelWidth = 128,
                DecodePixelType = DecodePixelType.Logical,
                UriSource = bitmapUri
            };
          
            return new RadCustomHubTile()
            {
                Title = title,
                FrontContent = new Grid()
                {
                    //HorizontalAlignment = HorizontalAlignment.Stretch,
                    //VerticalAlignment = VerticalAlignment.Stretch,
                    Background = (SolidColorBrush) Application.Current.Resources["MegaRedSolidColorBrush"],
                    Children =
                    {
                        new Image()
                        {
                            Source = bitmapImage,
                            Width = 128,
                            Height = 128,
                            Stretch = Stretch.UniformToFill
                        }
                    }
                },
                IsFrozen = true,
                Width = 210,
                Height = 210,
                Margin = margin
            };
        }

        public static void ChangeAppBarStatus(IList iconButtons, IList menuItems, bool enable)
        {
            if(iconButtons != null)
            {
                foreach (var button in iconButtons)
                    ((ApplicationBarIconButton)button).IsEnabled = enable;
            }

            if (menuItems != null)
            {
                foreach (var menuItem in menuItems)
                    ((ApplicationBarMenuItem)menuItem).IsEnabled = enable;
            }            
        }

        /// <summary>
        /// Regular expression to check and hexadecimal color string. 
        /// <para>Supports both “argb” and “rgb” with or without “#” in front of it.</para>        
        /// </summary>
        private static Regex _hexColorMatchRegex = 
            new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Get a Color object from an hexadecimal color string (for example for the user avatar color).
        /// <para>Supports both “argb” and “rgb” with or without “#” in front of it.</para>
        /// <para>Meaning: The string “#aabbcc” or “#ffaabbcc” or “aabbcc” or “ffaabbcc” will be converted to a Color object.</para>
        /// </summary>
        /// <param name="hexColorString">Hexadecimal color string.</param>
        /// <returns>
        /// Color object corresponding to the hexadecimal color string.
        /// <para>Default value: MEGA red color (#D90007)</para>
        /// </returns>
        public static Color GetColorFromHex(String hexColorString)
        {
            if (String.IsNullOrWhiteSpace(hexColorString))
                return (Color)Application.Current.Resources["MegaRedColor"];

            // Regex match the string
            var match = _hexColorMatchRegex.Match(hexColorString);

            // If no matches return the MEGA red color.
            if (!match.Success)
                return (Color)Application.Current.Resources["MegaRedColor"];

            byte a = 255, r = 0, b = 0, g = 0;

            // a value is optional            
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            
            // r,g,b values are not optional
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);            
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
