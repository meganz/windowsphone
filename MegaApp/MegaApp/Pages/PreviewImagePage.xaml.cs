using System;
using System.Windows.Navigation;
using Windows.Devices.Sensors;
using MegaApp.Models;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Telerik.Windows.Controls;
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