using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class DownloadImagePage : PhoneApplicationPage
    {
        private readonly DownloadImageViewModel _downloadImageViewModel;
        public DownloadImagePage()
        {
            _downloadImageViewModel = new DownloadImageViewModel(App.CloudDrive);
            this.DataContext = _downloadImageViewModel;
            _downloadImageViewModel.SelectedDownload = App.CloudDrive.FocusedNode;

            InitializeComponent();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Previous;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Save;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = UiResources.Next;
        }

        private void SetMoveButtons()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = SlideViewAndFilmStrip.PreviousItem != null;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = SlideViewAndFilmStrip.SelectedItem != null;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = SlideViewAndFilmStrip.NextItem != null;
        }

        private void OnNextClick(object sender, System.EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToNextItem();
        }

        private void OnPreviousClick(object sender, System.EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToPreviousItem();
        }

        private void OnSaveClick(object sender, System.EventArgs e)
        {
            _downloadImageViewModel.SelectedDownload.SaveImageToCameraRoll();
        }

        private void OnSlideViewLoaded(object sender, RoutedEventArgs e)
        {
            SetMoveButtons();
        }

        private void SlideViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMoveButtons();
        }
    }
}