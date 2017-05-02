using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Telerik.Windows.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Pages
{
    public partial class MediaAlbumPage : MegaPhoneApplicationPage
    {
       private MediaAlbumViewModel _mediaAlbumViewModel;

       public MediaAlbumPage()
        {
            _mediaAlbumViewModel = new MediaAlbumViewModel(App.MegaSdk, 
                NavigateService.GetNavigationData<BaseMediaViewModel<PictureAlbum>>());
            this.DataContext = _mediaAlbumViewModel;
            InitializeComponent();

            SetApplicationBar();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }
        
        private void SetApplicationBar()
        {
            if (ApplicationBar == null)
                ApplicationBar = (ApplicationBar)Resources["MediaAlbumMenu"];

            if(_mediaAlbumViewModel == null)
            {
                _mediaAlbumViewModel = new MediaAlbumViewModel(App.MegaSdk,
                    NavigateService.GetNavigationData<BaseMediaViewModel<PictureAlbum>>());
            }

            // Change and translate the current application bar
            _mediaAlbumViewModel.ChangeMenu(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems);
        }

        private void SetControlState(bool state)
        {
            if (ApplicationBar == null)
                ApplicationBar = (ApplicationBar)Resources["MediaAlbumMenu"];

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
                if(item == null) continue;

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
                        var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, MTransferType.TYPE_UPLOAD, newFilePath);
                        App.MegaTransfers.Add(uploadTransfer);
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
            if (_mediaAlbumViewModel == null)
            {
                _mediaAlbumViewModel = new MediaAlbumViewModel(App.MegaSdk,
                    NavigateService.GetNavigationData<BaseMediaViewModel<PictureAlbum>>());
            }

            if (_mediaAlbumViewModel.Pictures == null) return;

            var lastPicture = _mediaAlbumViewModel.Pictures.LastOrDefault();
            if (lastPicture != null) 
                LstMediaItems.BringIntoView(lastPicture);
        }
    }
}