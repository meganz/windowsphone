using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }
}
