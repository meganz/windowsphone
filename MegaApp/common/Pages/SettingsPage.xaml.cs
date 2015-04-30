using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ShakeGestures;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private SettingsViewModel _settingsViewModel;
        
        public SettingsPage()
        {
            _settingsViewModel = new SettingsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _settingsViewModel;

            // Initialize ShakeGestures to display debug settings
            ShakeGesturesHelper.Instance.ShakeGesture += InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 12;
            ShakeGesturesHelper.Instance.Active = true;
                        
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                #if WINDOWS_PHONE_80
                    TextAskDownloadLocation.Visibility = Visibility.Collapsed;
                    GridAskDownloadLocation.Visibility = Visibility.Collapsed;
                    TextDefaultDownloadLocation.Visibility = Visibility.Collapsed;
                    LinkDefaultDownloadLocation.Visibility = Visibility.Collapsed;
                #elif WINDOWS_PHONE_81
                    TextExportPhotoAlbumSwitch.Visibility = Visibility.Collapsed;
                    GridExportPhotoAlbumSwitch.Visibility = Visibility.Collapsed;
                #endif
                });
            
            InitializeComponent();

            // Initialize the hamburger menu / slide in
            MainDrawerLayout.InitializeDrawerLayout();
            MainDrawerLayout.DrawerOpened += OnDrawerOpened;
            MainDrawerLayout.DrawerClosed += OnDrawerClosed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigateService.PreviousPage == typeof (PasswordPage))
                NavigationService.RemoveBackEntry();

            DebugPanel.DataContext = DebugService.DebugSettings;

            #if WINDOWS_PHONE_81
            ((SettingsViewModel)this.DataContext).StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, AppResources.DefaultDownloadLocation);
            #endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Deinitialize ShakeGestures to disable shake detection
            ShakeGesturesHelper.Instance.ShakeGesture -= InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.Active = false;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            // Check if Hamburger Menu is open in view. If open. First slide out before exit
            e.Cancel = _settingsViewModel.CheckHamburgerMenu(MainDrawerLayout, e.Cancel);
                        
            base.OnBackKeyPress(e);            
        }
                
        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            //Dispatcher.BeginInvoke(
            //    () => DebugService.DebugSettings.IsDebugMode = !DebugService.DebugSettings.IsDebugMode);
        }

        private void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var hamburgerMenuItem = e.Item.DataContext as HamburgerMenuItem;
            if (hamburgerMenuItem == null) return;
            
            if(hamburgerMenuItem.Type == HamburgerMenuItemType.Settings)
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
            
        }

        private void OnDrawerOpened(object sender)
        {
            // Remove application bar from display when sliding in the hamburger menu
            this.ApplicationBar = null;
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }
    }
}