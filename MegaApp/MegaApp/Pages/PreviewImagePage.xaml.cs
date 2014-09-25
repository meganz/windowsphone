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
using Telerik.Windows.Controls;

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

            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = UiResources.Previous;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).Text = UiResources.GetPreviewLink;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[2]).Text = UiResources.Next;
        }
        
        private void SetMoveButtons(bool isSlideview = true)
        {
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).IsEnabled =
                SlideViewAndFilmStrip.PreviousItem != null && isSlideview;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = isSlideview;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[2]).IsEnabled =
                SlideViewAndFilmStrip.NextItem != null && isSlideview;
        }

        private void OnNextClick(object sender, System.EventArgs e)
        {
            SlideViewAndFilmStrip.MoveToNextItem();
        }

        private void OnGetLinkClick(object sender, System.EventArgs e)
        {
            _previewImageViewModel.GetPreviewLink();
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