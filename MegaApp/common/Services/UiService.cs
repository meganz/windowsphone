using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using mega;
using MegaApp.Converters;
using MegaApp.Enums;
using Telerik.Windows.Controls;

namespace MegaApp.Services
{
    static class UiService
    {
        private static Dictionary<ulong, int> _folderSorting;
        private static Dictionary<ulong, int> _folderViewMode;

        private static Dictionary<string, int> _offlineFolderViewMode;

        public static int GetSortOrder(ulong folderHandle, string folderName)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<ulong, int>();

            if (_folderSorting.ContainsKey(folderHandle))
                return _folderSorting[folderHandle];
           
            return folderName.Equals("Camera Uploads") ? (int)MSortOrderType.ORDER_MODIFICATION_DESC :
                (int) MSortOrderType.ORDER_DEFAULT_ASC;
        }

        public static void SetSortOrder(ulong folderHandle, int sortOrder)
        {
            if (_folderSorting == null)
                _folderSorting = new Dictionary<ulong, int>();

            if (_folderSorting.ContainsKey(folderHandle))
                _folderSorting[folderHandle] = sortOrder;
            else
                _folderSorting.Add(folderHandle, sortOrder);
            
        }

        public static ViewMode GetViewMode(ulong folderHandle, string folderName)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<ulong, int>();

            if (_folderViewMode.ContainsKey(folderHandle))
                return (ViewMode)_folderViewMode[folderHandle];

            return folderName.Equals("Camera Uploads") ? ViewMode.LargeThumbnails : ViewMode.ListView;
        }

        public static void SetViewMode(ulong folderHandle, ViewMode viewMode)
        {
            if (_folderViewMode == null)
                _folderViewMode = new Dictionary<ulong, int>();

            if (_folderViewMode.ContainsKey(folderHandle))
                _folderViewMode[folderHandle] = (int)viewMode;
            else
                _folderViewMode.Add(folderHandle, (int)viewMode);

        }

        public static ViewMode GetViewMode(string folderHandle, string folderName)
        {
            if (_offlineFolderViewMode == null)
                _offlineFolderViewMode = new Dictionary<string, int>();

            if (_offlineFolderViewMode.ContainsKey(folderHandle))
                return (ViewMode)_offlineFolderViewMode[folderHandle];

            return folderName.Equals("Camera Uploads") ? ViewMode.LargeThumbnails : ViewMode.ListView;
        }

        public static void SetViewMode(string folderHandle, ViewMode viewMode)
        {
            if (_offlineFolderViewMode == null)
                _offlineFolderViewMode = new Dictionary<string, int>();

            if (_offlineFolderViewMode.ContainsKey(folderHandle))
                _offlineFolderViewMode[folderHandle] = (int)viewMode;
            else
                _offlineFolderViewMode.Add(folderHandle, (int)viewMode);
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

    }
}
