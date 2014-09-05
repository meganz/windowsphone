using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class PreviewImagePage : PhoneApplicationPage
    {
        public PreviewImagePage()
        {
            var previewImageViewModel = new PreviewImageViewModel(App.CloudDrive);
            this.DataContext = previewImageViewModel;
            previewImageViewModel.SelectedPreview = App.CloudDrive.FocusedNode;
            InitializeComponent();
        }
    }
}