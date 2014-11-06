using System;
using System.Windows.Navigation;
using Windows.Devices.Sensors;
using MegaApp.Classes;
using MegaApp.Models;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
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

        public PreviewImagePage()
        {
            _previewImageViewModel = new PreviewImageViewModel(App.MegaSdk, App.CloudDrive);
            this.DataContext = _previewImageViewModel;
            _previewImageViewModel.SelectedPreview = App.CloudDrive.FocusedNode;
            
            InitializeComponent();

            _previewImageViewModel.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems);

            MemoryControl.StartMemoryCounter();
        }
        

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            MemoryControl.StopMemoryCounter();
        }

        private void SetMoveButtons(bool isSlideview = true)
        {
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
            SetMoveButtons();

            // Bind to item state changed event to explicit release the image when scrolling in filmstrip mode.
            var zoomableListBox = ElementTreeHelper.FindVisualDescendant<ZoomableListBox>(sender as RadSlideView);
            zoomableListBox.ItemStateChanged += ZoomableListBoxOnItemStateChanged;
        }

        private void ZoomableListBoxOnItemStateChanged(object sender, ItemStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ItemState.Recycling:
                    ((NodeViewModel)e.DataItem).PreviewImageUri = null;
                    break;
                case ItemState.Recycled:
                    break;
                case ItemState.Realizing:
                    break;
                case ItemState.Realized:
                    break;
            }
        }

        private void OnSlideViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMoveButtons();
        }

        private void OnSlideViewStateChanged(object sender, SlideViewStateChangedArgs e)
        {
            SetMoveButtons(e.NewState != SlideViewState.Filmstrip);
        }
        
    }
}