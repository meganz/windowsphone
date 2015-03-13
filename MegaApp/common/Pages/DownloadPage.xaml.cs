using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MegaApp.Converters;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class DownloadPage : PhoneApplicationPage
    {
        private readonly DownloadNodeViewModel _downloadNodeViewModel;
        
        public DownloadPage()
        {
            _downloadNodeViewModel = new DownloadNodeViewModel(NavigateService.GetNavigationData<NodeViewModel>());
            this.DataContext = _downloadNodeViewModel;
            
            InitializeComponent();

            SetApplicationBar();
            SetImageSize();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Save.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Open.ToLower();

            if (_downloadNodeViewModel.SelectedNode == null) return;
            if (_downloadNodeViewModel.SelectedNode is ImageNodeViewModel) return;
            
            ApplicationBar.Buttons.RemoveAt(0);
        }

        private void SetImageSize()
        {
            if (_downloadNodeViewModel.SelectedNode == null) return;
            if (_downloadNodeViewModel.SelectedNode is ImageNodeViewModel) return;
            
            this.PanAndZoomImage.HorizontalAlignment = HorizontalAlignment.Center;
            this.PanAndZoomImage.VerticalAlignment = VerticalAlignment.Center;
            this.PanAndZoomImage.Stretch = Stretch.None;

            var bitmapImage = new BitmapImage(new Uri("/Assets/FileTypes/ThumbView/" + 
                _downloadNodeViewModel.SelectedNode.ThumbnailImageUri, UriKind.Relative))
            {
                DecodePixelHeight = 128,
                DecodePixelWidth = 128,
                DecodePixelType = DecodePixelType.Logical
            };

            this.PanAndZoomImage.Source = bitmapImage;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (_downloadNodeViewModel.SelectedNode != null)
                _downloadNodeViewModel.SelectedNode.Transfer.CancelTransfer();
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            PanAndZoomImage.Source = null; // memory leak TELERIK
        }

        private void OnSaveClick(object sender, System.EventArgs e)
        {
            if (_downloadNodeViewModel.SelectedNode == null) return;
            if (_downloadNodeViewModel.SelectedNode.Transfer.IsBusy) return;
            ((ImageNodeViewModel)_downloadNodeViewModel.SelectedNode).SaveImageToCameraRoll();
        }

        private void OnOpenClick(object sender, System.EventArgs e)
        {
            if (_downloadNodeViewModel.SelectedNode == null) return;
            if (_downloadNodeViewModel.SelectedNode.Transfer.IsBusy) return;

            // Only open it if the transfer have finished or the file is already downloaded previously
            if(_downloadNodeViewModel.SelectedNode.Transfer.Status == TransferStatus.Finished ||                
                _downloadNodeViewModel.SelectedNode.Transfer.Status == TransferStatus.NotStarted)
            {       
               _downloadNodeViewModel.SelectedNode.OpenFile();
            }
        }
    }
}
