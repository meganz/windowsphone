using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class MediaAlbumPage : PhoneApplicationPage
    {
       private readonly MediaAlbumViewModel _mediaAlbumViewModel;
       public MediaAlbumPage()
        {
            _mediaAlbumViewModel = new MediaAlbumViewModel(App.MegaSdk, 
                NavigateService.GetNavigationData<BaseMediaViewModel<PictureAlbum>>());
            this.DataContext = _mediaAlbumViewModel;
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private async void OnAcceptClick(object sender, System.EventArgs e)
        {
            if (LstMediaItems.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select minimal 1 picture", "No pictures selected", MessageBoxButton.OK);
                return;
            }

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareUploads);

            foreach (var checkedItem in LstMediaItems.CheckedItems)
            {
                var item = (BaseMediaViewModel<Picture>)checkedItem;

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
            ProgressService.SetProgressIndicator(false);

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.AlbumSelected);
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
            LstMediaItems.BringIntoView(_mediaAlbumViewModel.Pictures.Last());
        }
    }
}