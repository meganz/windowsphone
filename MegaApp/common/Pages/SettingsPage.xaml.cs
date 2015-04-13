using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ShakeGestures;

namespace MegaApp.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private SettingsViewModel _settingsViewModel;

        public SettingsPage()
        {
            _settingsViewModel = new SettingsViewModel(App.MegaSdk);
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

        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            //Dispatcher.BeginInvoke(
            //    () => DebugService.DebugSettings.IsDebugMode = !DebugService.DebugSettings.IsDebugMode);
        }
    }
}