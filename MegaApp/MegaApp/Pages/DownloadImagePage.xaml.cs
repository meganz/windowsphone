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
            _downloadImageViewModel = new DownloadImageViewModel(App.CloudDrive.FocusedNode);
            this.DataContext = _downloadImageViewModel;

            InitializeComponent();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Save;
        }

        private void OnSaveClick(object sender, System.EventArgs e)
        {
            _downloadImageViewModel.SelectedNode.SaveImageToCameraRoll();
        }
    }
}