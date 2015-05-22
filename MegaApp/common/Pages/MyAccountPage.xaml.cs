using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class MyAccountPage : PhoneApplicationPage
    {
        private readonly MyAccountPageViewModel _myAccountPageViewModel;

        public MyAccountPage()
        {
            _myAccountPageViewModel = new MyAccountPageViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _myAccountPageViewModel;
            InitializeComponent();

            // Initialize the hamburger menu / slide in
            MainDrawerLayout.InitializeDrawerLayout();
            MainDrawerLayout.DrawerOpened += OnDrawerOpened;
            MainDrawerLayout.DrawerClosed += OnDrawerClosed;

            SetApplicationBar();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private void SetApplicationBar()
        {
            this.ApplicationBar = (ApplicationBar)Resources["MyAccountMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Settings.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Logout.ToLower();

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.ClearCache.ToLower();

            /*((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.ChangePassword.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.ExportMasterKeyText.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = UiResources.ClearCache.ToLower();*/
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            // Check if Hamburger Menu is open in view. If open. First slide out before exit
            e.Cancel = MainDrawerLayout.CloseIfOpen();

            base.OnBackKeyPress(e);
        }

        private void OnPieDataBindingComplete(object sender, System.EventArgs e)
        {
            // Focus on the first datapoint (= Used space)
            //((PieSeries) sender).DataPoints[0].OffsetFromCenter = 0.05;
        }

        private void OnLogoutClick(object sender, System.EventArgs e)
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

        private void OnSettingsClick(object sender, System.EventArgs e)
        {
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }

        /*private void OnChangePasswordClick(object sender, System.EventArgs e)
        {

        }

        private void OnExportMasterkeyClick(object sender, System.EventArgs e)
        {

        }*/

        private void OnClearCacheClick(object sender, System.EventArgs e)
        {
            App.MainPageViewModel.CloudDrive.ChildNodes.Clear();
            App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            _myAccountPageViewModel.ClearCache();
        }        

        private void OnItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            App.MegaSdk.getPaymentId(((Product)e.Item.DataContext).Handle, new GetPaymentUrlRequestListener());
        }

        private void OnPivotLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == PivotAccount)
                _myAccountPageViewModel.GetAccountDetails();
            else
                _myAccountPageViewModel.GetPricing();
        }

        private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //LstProducts.SelectedItem = null;
        }

        private void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var hamburgerMenuItem = e.Item.DataContext as HamburgerMenuItem;
            if (hamburgerMenuItem == null) return;

            if (hamburgerMenuItem.Type == HamburgerMenuItemType.MyAccount)
                MainDrawerLayout.CloseDrawer();
            else
                hamburgerMenuItem.TapAction.Invoke();
            
            LstHamburgerMenu.SelectedItem = null;
        }

        private void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MainDrawerLayout.OpenDrawer();
        }

        private void OnDrawerClosed(object sender)
        {
            SetApplicationBar();
        }

        private void OnDrawerOpened(object sender)
        {
            // Remove application bar from display when sliding in the hamburger menu
            this.ApplicationBar = null;
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MainDrawerLayout.CloseDrawer();
        }
    }
}