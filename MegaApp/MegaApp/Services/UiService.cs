using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using mega;
using MegaApp.Enums;

namespace MegaApp.Services
{
    static class UiService
    {
        private static Dictionary<ulong, int> _folderSorting;
        private static Dictionary<ulong, int> _folderViewMode;

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

    }
}
