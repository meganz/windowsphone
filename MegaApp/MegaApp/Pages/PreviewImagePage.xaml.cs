using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Windows.Devices.Sensors;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Primitives;
using Telerik.Windows.Controls.SlideView;
using Telerik.Windows.Data;
using Accelerometer = Microsoft.Devices.Sensors.Accelerometer;
using AccelerometerReading = Microsoft.Devices.Sensors.AccelerometerReading;

namespace MegaApp.Pages
{
  
    public partial class PreviewImagePage : PhoneApplicationPage
    {
        private readonly PreviewImageViewModel _previewImageViewModel;
        private Timer _overlayTimer;

        public PreviewImagePage()
        {
            _previewImageViewModel = new PreviewImageViewModel(App.MegaSdk, App.CloudDrive);
            this.DataContext = _previewImageViewModel;
            _previewImageViewModel.SelectedPreview = App.CloudDrive.FocusedNode;
            
            InitializeComponent();

            ApplicationBar = (ApplicationBar)Resources["PreviewApplicationBar"];

            _previewImageViewModel.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems);
          
            MemoryControl.StartMemoryCounter();

            if(AppService.IsLowMemoryDevice())
                SlideViewAndFilmStrip.ItemRealizationMode = SlideViewItemRealizationMode.ViewportItem;
        }
        

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            MemoryControl.StopMemoryCounter();
        }

        private void SetMoveButtons(bool isSlideview = true)
        {
            if (ApplicationBar == null) return;

            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).IsEnabled =
                SlideViewAndFilmStrip.PreviousItem != null && isSlideview;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = isSlideview;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = isSlideview;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[3]).IsEnabled =
                SlideViewAndFilmStrip.NextItem != null && isSlideview;
        }

        private void OnNextClick(object sender, System.EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToNextItem();
        }

        private void OnViewOriginalClick(object sender, System.EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.ViewOriginal();
        }

        private void OnGetLinkClick(object sender, System.EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.GetPreviewLink();
        }

        private void OnRenameItemClick(object sender, System.EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.Rename();
        }
        private void OnRemoveClick(object sender, System.EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.Remove();
        }

        private void OnPreviousClick(object sender, System.EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToPreviousItem();
        }

        private void OnSlideViewLoaded(object sender, RoutedEventArgs e)
        {
            // Start always with the information visible
            SlideViewAndFilmStrip.ShowOverlayContent();

            // Start the timer. If the user does not do anything in a few seconds then remove the overlay
            //_overlayTimer = new Timer(HideOverlayAndAppbar, null, new TimeSpan(0,0,10), new TimeSpan(0,0,0,0,-1));

            SetMoveButtons();

            // Bind to item state changed event to explicit release the image when scrolling in filmstrip mode.
            var zoomableListBox = ElementTreeHelper.FindVisualDescendant<ZoomableListBox>(sender as RadSlideView);
            zoomableListBox.ItemStateChanged += ZoomableListBoxOnItemStateChanged;
        }

        private void HideOverlayAndAppbar(object p)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SlideViewAndFilmStrip.HideOverlayContent();
                ApplicationBar = null;
            });
            _overlayTimer.Dispose();
            _overlayTimer = null;
        }

        private void ZoomableListBoxOnItemStateChanged(object sender, ItemStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ItemState.Recycling:
                    ((NodeViewModel) e.DataItem).InViewingRange = false;
                    ((NodeViewModel)e.DataItem).PreviewImageUri = null;
                    break;
                case ItemState.Recycled:
                    break;
                case ItemState.Realizing:
                    ((NodeViewModel)e.DataItem).InViewingRange = true;
                    ((NodeViewModel)e.DataItem).SetPreviewImage();
                    break;
                case ItemState.Realized:
                    break;
            }
        }

        private void OnSlideViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMoveButtons();
            if (e.RemovedItems[0] != null)
            {
                int currentIndex = _previewImageViewModel.PreviewItems.IndexOf((NodeViewModel) e.AddedItems[0]);
                int lastIndex = _previewImageViewModel.PreviewItems.IndexOf((NodeViewModel)e.RemovedItems[0]);

                _previewImageViewModel.GalleryDirection = currentIndex > lastIndex
                    ? GalleryDirection.Next
                    : GalleryDirection.Previous;
            }
            else
                _previewImageViewModel.GalleryDirection = GalleryDirection.Next;
        }

        private void OnSlideViewStateChanged(object sender, SlideViewStateChangedArgs e)
        {
            SetMoveButtons(e.NewState != SlideViewState.Filmstrip);
            if (!SlideViewAndFilmStrip.IsOverlayContentDisplayed)
            {
                ApplicationBar = null;
            }
            else
            {
                ApplicationBar = (ApplicationBar)Resources["PreviewApplicationBar"];
            }
        }

        private void OnSlideViewTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (SlideViewAndFilmStrip.IsOverlayContentDisplayed)
            {
                ApplicationBar = null;
            }
            else
            {
                ApplicationBar = (ApplicationBar)Resources["PreviewApplicationBar"];
            }
        }
    }
}