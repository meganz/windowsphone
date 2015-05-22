using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class MyAccountPage : PhoneDrawerLayoutPage
    {
        private readonly MyAccountPageViewModel _myAccountPageViewModel;

        public MyAccountPage()
        {
            _myAccountPageViewModel = new MyAccountPageViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _myAccountPageViewModel;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.MyAccount);

            SetApplicationBarData();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["MyAccountMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Settings.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Logout.ToLower();

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.ClearCache.ToLower();
        }


        private void OnPieDataBindingComplete(object sender, EventArgs e)
        {
            // Focus on the first datapoint (= Used space)
            //((PieSeries) sender).DataPoints[0].OffsetFromCenter = 0.05;
        }

        private void OnLogoutClick(object sender, EventArgs e)
        {
            int numPendingTransfers = App.MegaTransfers.Count(t => (t.Status == TransferStatus.Queued ||
                t.Status == TransferStatus.Downloading || t.Status == TransferStatus.Uploading ||
                t.Status == TransferStatus.Paused || t.Status == TransferStatus.Pausing));

            if (numPendingTransfers > 0)
            {
                if (MessageBox.Show(String.Format(AppMessages.PendingTransfersLogout, numPendingTransfers),
                    AppMessages.PendingTransfersLogout_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

                foreach (var item in App.MegaTransfers)
                {
                    var transfer = (TransferObjectModel)item;
                    if (transfer == null) continue;

                    transfer.CancelTransfer();
                }
            }

        	_myAccountPageViewModel.Logout();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }
       

        private void OnClearCacheClick(object sender, EventArgs e)
        {
            App.MainPageViewModel.CloudDrive.ChildNodes.Clear();
            App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            _myAccountPageViewModel.ClearCache();
        }        

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            App.MegaSdk.getPaymentId(((Product)e.Item.DataContext).Handle, new GetPaymentUrlRequestListener());
        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == PivotAccount)
                _myAccountPageViewModel.GetAccountDetails();
            else
                _myAccountPageViewModel.GetPricing();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //LstProducts.SelectedItem = null;
        }


        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }
        
        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MainDrawerLayout.CloseDrawer();
        }

        #region Override Events

        // XAML can not bind them direct from the base class
        // That is why these are dummy event handlers

        protected override void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            base.OnHamburgerTap(sender, e);
        }

        protected override void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            base.OnHamburgerMenuItemTap(sender, e);
        }

        #endregion
    }
}