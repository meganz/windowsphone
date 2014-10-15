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
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class MyAccountPage : PhoneApplicationPage
    {
        private readonly MyAccountPageViewModel _myAccountPageViewModel;

        public MyAccountPage()
        {
            _myAccountPageViewModel = new MyAccountPageViewModel(App.MegaSdk);
            this.DataContext = _myAccountPageViewModel;
            InitializeComponent();
        }

        private void OnPieDataBindingComplete(object sender, System.EventArgs e)
        {
            // Focus on the first datapoint (= Used space)
            ((PieSeries) sender).DataPoints[0].OffsetFromCenter = 0.05;
        }

        private void OnLogoutClick(object sender, System.EventArgs e)
        {
        	_myAccountPageViewModel.Logout();
        }

        private void OnClearCacheClick(object sender, System.EventArgs e)
        {
            App.CloudDrive.ChildNodes.Clear();
            _myAccountPageViewModel.ClearCache();
        }
    }
}