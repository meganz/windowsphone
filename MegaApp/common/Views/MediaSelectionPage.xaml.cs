using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class MediaSelectionPage : MegaPhoneApplicationPage
    {
        private readonly MediaSelectionPageModel _mediaSelectionPageModel;
       
        public MediaSelectionPage()
        {
            _mediaSelectionPageModel = new MediaSelectionPageModel(SdkService.MegaSdk);
            this.DataContext = _mediaSelectionPageModel;

            InitializeComponent();

            SetApplicationBar();

            CreateGroupDescriptor();
            CreateGroupPickerItems();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private void SetApplicationBar()
        {
            // Change and translate the current application bar
            _mediaSelectionPageModel.ChangeMenu(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems);
        }

        private void SetControlState(bool state)
        {
            if (this.ApplicationBar == null) return;

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, state);
        }

        private async void OnAcceptClick(object sender, System.EventArgs e)
        {
            if (LstMediaItems.CheckedItems == null || LstMediaItems.CheckedItems.Count < 1)
            {
                new CustomMessageDialog(
                    AppMessages.MinimalPictureSelection_Title,
                    AppMessages.MinimalPictureSelection,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
                return;
            }

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareUploads);
            SetControlState(false);

            // Set upload directory only once for speed improvement and if not exists, create dir
            var uploadDir = AppService.GetUploadDirectoryPath(true);

            foreach (var checkedItem in LstMediaItems.CheckedItems)
            {
                var item = (BaseMediaViewModel<Picture>)checkedItem;
                if (item == null) continue;

                try
                {
                    string fileName = Path.GetFileName(item.Name);
                    if (fileName != null)
                    {
                        string newFilePath = Path.Combine(uploadDir, fileName);
                        using (var fs = new FileStream(newFilePath, FileMode.Create))
                        {
                            await item.BaseObject.GetImage().CopyToAsync(fs);
                            await fs.FlushAsync();
                            fs.Close();
                        }
                        var uploadTransfer = new TransferObjectModel(SdkService.MegaSdk, App.CloudDrive.CurrentRootNode, MTransferType.TYPE_UPLOAD, newFilePath);
                        TransfersService.MegaTransfers.Add(uploadTransfer);
                        uploadTransfer.StartTransfer();
                    }
                }
                catch (Exception)
                {
                    new CustomMessageDialog(
                        AppMessages.PrepareImageForUploadFailed_Title,
                        String.Format(AppMessages.PrepareImageForUploadFailed, item.Name),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }

            }

            ProgressService.SetProgressIndicator(false);
            SetControlState(true);

            App.CloudDrive.NoFolderUpAction = true;

            if (NavigateService.CanGoBack())
                NavigateService.GoBack();
            else
                NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
        }

        private void OnClearSelectionClick(object sender, System.EventArgs e)
        {
            LstMediaItems.CheckedItems.Clear();
        }

        private void OnItemCheckedStateChanged(object sender, Telerik.Windows.Controls.ItemCheckedStateChangedEventArgs e)
        {
            if (LstMediaItems != null && LstMediaItems.CheckedItems != null)
                SetControlState(LstMediaItems.CheckedItems.Count > 0);
            else
                SetControlState(false);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_mediaSelectionPageModel.Pictures == null) return;

            var lastPicture = _mediaSelectionPageModel.Pictures.LastOrDefault();
            if (lastPicture != null)
                LstMediaItems.BringIntoView(lastPicture);
        }

        private void CreateGroupDescriptor()
        {
            var groupByDate =
                new GenericGroupDescriptor<BaseMediaViewModel<Picture>, DateTime>(
                    p => DateTime.Parse(p.BaseObject.Date.ToString("MMMM yyyy")))
                {SortMode = ListSortMode.Ascending, GroupFormatString = "{0:MMMM yyyy}"};

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