using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SlideView;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

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
            _previewImageViewModel.SelectedPreview = (ImageNodeViewModel) App.CloudDrive.FocusedNode;
            
            InitializeComponent();

            ApplicationBar = (ApplicationBar)Resources["PreviewApplicationBar"];

            _previewImageViewModel.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems);

            if(AppService.IsLowMemoryDevice())
                SlideViewAndFilmStrip.ItemRealizationMode = SlideViewItemRealizationMode.ViewportItem;

            if (!DebugService.DebugSettings.IsDebugMode || !DebugService.DebugSettings.ShowMemoryInformation) return;
            MemoryControl.Visibility = Visibility.Visible;
            MemoryControl.StartMemoryCounter();
        }
        

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            MemoryControl.StopMemoryCounter();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.AppEvent == ApplicationEvent.Activated)
            {
                App.AppEvent = ApplicationEvent.None;
               
                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();

                // Check to see if any files have been picked
                var app = Application.Current as App;
                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs);
                }
                return;
            }
            base.OnNavigatedTo(e);
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

        private void OnNextClick(object sender, EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToNextItem();
        }

        private void OnViewOriginalClick(object sender, EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.Download();
        }

        private void OnGetLinkClick(object sender, EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.GetPreviewLink();
        }

        private void OnRenameItemClick(object sender, EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.Rename();
        }
        private void OnRemoveClick(object sender, EventArgs e)
        {
            _previewImageViewModel.SelectedPreview.Remove(false);
        }

        private void OnPreviousClick(object sender, EventArgs e)
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
                    ((ImageNodeViewModel)e.DataItem).InViewingRange = false;
                    ((ImageNodeViewModel)e.DataItem).PreviewImageUri = null;
                    break;
                case ItemState.Recycled:
                    break;
                case ItemState.Realizing:
                    ((ImageNodeViewModel)e.DataItem).InViewingRange = true;
                    ((ImageNodeViewModel)e.DataItem).SetPreviewImage();
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
                int currentIndex = _previewImageViewModel.PreviewItems.IndexOf((ImageNodeViewModel)e.AddedItems[0]);
                int lastIndex = _previewImageViewModel.PreviewItems.IndexOf((ImageNodeViewModel)e.RemovedItems[0]);

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
        }

        private void OnSlideViewTap(object sender, GestureEventArgs e)
        {
            SetApplicationBar();
        }

        private void SetApplicationBar()
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