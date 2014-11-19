using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace MegaApp.Pages
{
    public partial class MediaSelectionPage : PhoneApplicationPage
    {
        private readonly MediaSelectionPageModel _mediaSelectionPageModel;
       
        public MediaSelectionPage()
        {
            _mediaSelectionPageModel = new MediaSelectionPageModel(App.MegaSdk);
            this.DataContext = _mediaSelectionPageModel;

            InitializeComponent();

            CreateGroupDescriptor();
            CreateGroupPickerItems();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private async void OnAcceptClick(object sender, System.EventArgs e)
        {
            if (LstMediaItems.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select minimal 1 picture", "No pictures selected", MessageBoxButton.OK);
                return;
            }

            ProgessService.SetProgressIndicator(true, "Preparing uploads...");

            foreach (var checkedItem in LstMediaItems.CheckedItems)
            {
                var item = (BaseMediaViewModel<Picture>) checkedItem;

                string fileName = Path.GetFileName(item.Name);
                if (fileName != null)
                {
                    string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
                    using (var fs = new FileStream(newFilePath, FileMode.Create))
                    {
                        await item.BaseObject.GetImage().CopyToAsync(fs);
                        await fs.FlushAsync();
                        fs.Close();
                    }
                    var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
                    App.MegaTransfers.Add(uploadTransfer);
                    uploadTransfer.StartTransfer();
                }
            }
            ProgessService.SetProgressIndicator(false);

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.PictureSelected);
        }

        private void OnClearSelectionClick(object sender, System.EventArgs e)
        {
            LstMediaItems.CheckedItems.Clear();
        }

        private void OnItemCheckedStateChanged(object sender, Telerik.Windows.Controls.ItemCheckedStateChangedEventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = LstMediaItems.CheckedItems.Count > 0;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = LstMediaItems.CheckedItems.Count > 0;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LstMediaItems.BringIntoView(_mediaSelectionPageModel.Pictures.Last());
        }

        private void CreateGroupDescriptor()
        {
            var groupByDate = new GenericGroupDescriptor<BaseMediaViewModel<Picture>, DateTime>(p => DateTime.Parse(p.BaseObject.Date.ToString("MMMM yyyy"))) 
                { SortMode = ListSortMode.Ascending };

            groupByDate.GroupFormatString = "{0:MMMM yyyy}";
            LstMediaItems.GroupDescriptors.Add(groupByDate);
        }

        private void CreateGroupPickerItems()
        {
            var monthList = _mediaSelectionPageModel.Pictures
              .GroupBy(revision =>  revision.BaseObject.Date.ToString("MMMM yyyy") )
              .Select(group => new { GroupCriteria = group.Key, Count = group.Count(), GroupDate=DateTime.Parse(group.Key) })
              .OrderBy(x => x.GroupDate);

           var groupPickerItems = monthList.Select(month => month.GroupCriteria).ToList();

            LstMediaItems.GroupPickerItemsSource = groupPickerItems;
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MediaAlbumPage), NavigationParameter.Normal, LstMediaAlbums.SelectedItem);
        }

        private void OnGroupPickerItemTap(object sender, Telerik.Windows.Controls.GroupPickerItemTapEventArgs e)
        {
            foreach (DataGroup group in LstMediaItems.Groups)
            {
                if (object.Equals(DateTime.Parse(Convert.ToString(e.DataItem)), group.Key))
                {
                    e.DataItemToNavigate = group;
                    return;
                }
            }
        }
    }
}