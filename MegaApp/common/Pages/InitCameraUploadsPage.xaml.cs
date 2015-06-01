using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class InitCameraUploadsPage : PhoneDrawerLayoutPage
    {
        private readonly CameraUploadsViewModel _cameraUploadsViewModel;

        public InitCameraUploadsPage()
        {
            _cameraUploadsViewModel = new CameraUploadsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _cameraUploadsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.CameraUploads);

            SetApplicationBarData();

            // If user skips the initialization of the "Camera Uploads" service, by default 
            // the connection type is 'Wifi only' and the service is disabled or turned off
            SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsIsEnabled, false);
            SettingsService.SaveSetting<int>(SettingsResources.CameraUploadsConnectionType, (int)CameraUploadsConnectionType.WifiOnly);
        }

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["InitCameraUploadsMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Ok.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Skip.ToLower();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            // Establish the connection type selected by the user
            if((bool)RadioButtonWifiOnly.IsChecked)
                SettingsService.SaveSetting<int>(SettingsResources.CameraUploadsConnectionType, (int)CameraUploadsConnectionType.WifiOnly);
            else if((bool)RadioButtonWifiAndData.IsChecked)
                SettingsService.SaveSetting<int>(SettingsResources.CameraUploadsConnectionType, (int)CameraUploadsConnectionType.WifiAndDataPlan);

            // Enable or turn on the "Camera Uploads" service
            SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsIsEnabled, true);
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
        }

        private void OnSkipClick(object sender, EventArgs e)
        {
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
        }

        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
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