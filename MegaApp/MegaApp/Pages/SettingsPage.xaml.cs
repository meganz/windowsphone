using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ShakeGestures;

namespace MegaApp.Pages
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private readonly SettingsViewModel _settingsViewModel;

        public SettingsPage()
        {
            _settingsViewModel = new SettingsViewModel(App.MegaSdk);
            this.DataContext = _settingsViewModel;

            // Initialize ShakeGestures to display debug settings
            ShakeGesturesHelper.Instance.ShakeGesture += InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 12;
            ShakeGesturesHelper.Instance.Active = true;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DebugPanel.DataContext = DebugService.DebugSettings;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Deinitialize ShakeGestures to disable shake detection
            ShakeGesturesHelper.Instance.ShakeGesture -= InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.Active = false;
        }

        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            Dispatcher.BeginInvoke(
                () => DebugService.DebugSettings.IsDebugMode = !DebugService.DebugSettings.IsDebugMode);
        }
    }
}