using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SlideView;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
  
    public partial class PreviewImagePage : MegaPhoneApplicationPage
    {
        private readonly PreviewImageViewModel _previewImageViewModel;
        private readonly FolderViewModel _foderViewModel;

        private Timer _overlayTimer;

        public PreviewImagePage()
        {
            _foderViewModel = NavigateService.GetNavigationData<FolderViewModel>();
            _previewImageViewModel = new PreviewImageViewModel(App.MegaSdk, App.AppInformation, _foderViewModel);

            this.DataContext = _previewImageViewModel;

            InitializeComponent();
            SetApplicationBar();

            if(AppService.IsLowMemoryDevice())
                SlideViewAndFilmStrip.ItemRealizationMode = SlideViewItemRealizationMode.ViewportItem;

            if (!DebugService.DebugSettings.IsDebugMode || !DebugService.DebugSettings.ShowMemoryInformation) return;
            
            MemoryControl.Visibility = Visibility.Visible;
            MemoryControl.StartMemoryCounter();
        }

        public void SetApplicationBar(bool isEnabled = true)
        {
            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources();

            // Change and translate the current application bar
            _previewImageViewModel.ChangeMenu(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isEnabled);
        }

        private void SetAppbarResources()
        {
            if (SlideViewAndFilmStrip.IsOverlayContentDisplayed)
            {
                ApplicationBar = null;
            }
            else
            {
                if (_foderViewModel.Type == ContainerType.FolderLink)
                    this.ApplicationBar = (ApplicationBar)Resources["FolderLinkPreviewApplicationBar"];
                else
                    this.ApplicationBar = (ApplicationBar)Resources["PreviewApplicationBar"];
            }            
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            MemoryControl.StopMemoryCounter();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.AppInformation.IsStartupModeActivate)
            {
                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();

                if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                {
                    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                    return;
                }

                App.AppInformation.IsStartupModeActivate = false;

                #if WINDOWS_PHONE_81
                // Check to see if any files have been picked
                var app = Application.Current as App;
                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs);
                }
                return;
                #endif
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
            if (_previewImageViewModel != null && _previewImageViewModel.SelectedPreview != null)
                _previewImageViewModel.SelectedPreview.Download(App.MegaTransfers);
        }

        private void OnImportClick(object sender, EventArgs e)
        {

        }

        private void OnGetLinkClick(object sender, EventArgs e)
        {
            if (_previewImageViewModel != null && _previewImageViewModel.SelectedPreview != null) 
                _previewImageViewModel.SelectedPreview.GetLink();
        }

        private void OnRenameItemClick(object sender, EventArgs e)
        {
            if (_previewImageViewModel != null && _previewImageViewModel.SelectedPreview != null) 
                _previewImageViewModel.SelectedPreview.Rename();
        }

        private async void OnRemoveClick(object sender, EventArgs e)
        {
            if (_previewImageViewModel != null && _previewImageViewModel.SelectedPreview != null)
                await _previewImageViewModel.SelectedPreview.RemoveAsync(false);
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
            SetAppbarResources();
        }
    }
}