using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
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

        private void OnItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            App.MegaSdk.getPaymentUrl(((Product)e.Item.DataContext).Handle, new GetPaymentUrlRequestListener());;
        }

        private void OnPivotLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == PivotAccount)
                _myAccountPageViewModel.GetAccountDetails();
            else
                _myAccountPageViewModel.GetPricing();
        }
        
    }
}