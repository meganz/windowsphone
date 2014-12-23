using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Save;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.OpenButton;

            if (_downloadNodeViewModel.SelectedNode is ImageNodeViewModel) return;
            
            ApplicationBar.Buttons.RemoveAt(0);
        }

        private void SetImageSize()
        {
            if (_downloadNodeViewModel.SelectedNode is ImageNodeViewModel) return;
            this.PanAndZoomImage.Width = 100;
            this.PanAndZoomImage.Height = 100;
            this.PanAndZoomImage.HorizontalAlignment = HorizontalAlignment.Center;
            this.PanAndZoomImage.VerticalAlignment = VerticalAlignment.Center;

            var bitmapImage = new BitmapImage(_downloadNodeViewModel.SelectedNode.ThumbnailImageUri);

            this.PanAndZoomImage.Source = bitmapImage;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _downloadNodeViewModel.SelectedNode.Transfer.CancelTransfer();
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            PanAndZoomImage.Source = null; // memory leak TELERIK
        }

        private void OnSaveClick(object sender, System.EventArgs e)
        {
            ((ImageNodeViewModel)_downloadNodeViewModel.SelectedNode).SaveImageToCameraRoll();
        }

        private void OnOpenClick(object sender, System.EventArgs e)
        {
            // Only open it if the transfer have finished or the file is already downloaded previously
            if(_downloadNodeViewModel.SelectedNode.Transfer.Status == TransferStatus.Finished ||                
                _downloadNodeViewModel.SelectedNode.Transfer.Status == TransferStatus.NotStarted)
            {                
                _downloadNodeViewModel.SelectedNode.OpenFile();
            }
        }
    }
}
