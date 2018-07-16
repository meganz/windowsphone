using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class SettingsPage : PhoneDrawerLayoutPage
    {
        private readonly SettingsViewModel _settingsViewModel;
        
        public SettingsPage()
        {
            _settingsViewModel = new SettingsViewModel(SdkService.MegaSdk, App.AppInformation);
            this.DataContext = _settingsViewModel;
                        
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
            switch(navParam)
            {
                case NavigationParameter.UriLaunch:
                    if (NavigationContext.QueryString.ContainsKey("backup"))
                        _settingsViewModel.ProcessBackupLink();
                    break;

                case NavigationParameter.AutoCameraUpload:
                    App.AppInformation.IsStartedAsAutoUpload = false;
                    this.MainPivot.SelectedItem = PivotAutoUpload;
                    break;

                case NavigationParameter.SecuritySettings:
                    this.MainPivot.SelectedItem = SecurityPivot;
                    break;

                case NavigationParameter.MFA_Enabled:
                    this.MainPivot.SelectedItem = SecurityPivot;
                    DialogService.ShowMultiFactorAuthEnabledDialog();
                    break;
            }

            DebugPanel.DataContext = DebugService.DebugSettings;

            #if WINDOWS_PHONE_81
            ((SettingsViewModel)this.DataContext).StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, UiResources.DefaultDownloadLocation);
            #endif
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            // Check if can go back in the stack of pages
            e.Cancel = CheckGoBack(e.Cancel);
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

        private void OnSdkVersionTapped(object sender, GestureEventArgs e)
        {
            DebugService.ChangeStatusAction();
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