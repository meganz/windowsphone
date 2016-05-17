using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using ShakeGestures;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class SettingsPage : PhoneDrawerLayoutPage
    {
        private readonly SettingsViewModel _settingsViewModel;
        
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
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.Settings);
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            
            if (navParam == NavigationParameter.UriLaunch &&
                NavigationContext.QueryString.ContainsKey("backup"))
                    _settingsViewModel.ProcessBackupLink();

            if (navParam == NavigationParameter.AutoCameraUpload)
            {
                App.AppInformation.IsStartedAsAutoUpload = false;
                MainSettingsPivot.SelectedItem = PivotAutoUpload;
            }

            if (NavigateService.PreviousPage == typeof(PasswordPage))
                NavigationService.RemoveBackEntry();

            DebugPanel.DataContext = DebugService.DebugSettings;

            #if WINDOWS_PHONE_81
            ((SettingsViewModel)this.DataContext).StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, UiResources.DefaultDownloadLocation);
            #endif
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Deinitialize ShakeGestures to disable shake detection
            ShakeGesturesHelper.Instance.ShakeGesture -= InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.Active = false;
        }
                
        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            //Dispatcher.BeginInvoke(
            //    () => DebugService.DebugSettings.IsDebugMode = !DebugService.DebugSettings.IsDebugMode);
        }

        private void BtnCameraUploadsSwitch_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
                SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsFirstInit, false);
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