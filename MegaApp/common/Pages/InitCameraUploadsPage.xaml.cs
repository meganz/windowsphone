using System;
using System.ComponentModel;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Pages
{
    public partial class InitCameraUploadsPage
    {
        private readonly CameraUploadsViewModel _cameraUploadsViewModel;

        public InitCameraUploadsPage()
        {
            _cameraUploadsViewModel = new CameraUploadsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _cameraUploadsViewModel;

            InitializeComponent();            

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Set to false the "CameraUploadsFirstInit" setting
            if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
                SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsFirstInit, false);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if(NavigateService.PreviousPage != null)
                NavigateService.GoBack();
            else
                NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);

            e.Cancel = true;
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).IsEnabled = false;

            try
            {
                // Enable or turn on the "Camera Uploads" service
                SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsIsEnabled, true);
                MediaService.SetAutoCameraUpload(true);

                if (NavigateService.PreviousPage != null)
                    NavigateService.GoBack();
                else
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            finally
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            }
           
        }

        private void OnSkipClick(object sender, EventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;

            try
            {
                if (NavigateService.PreviousPage != null)
                    NavigateService.GoBack();
                else
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            finally
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
            }
        }        
    }
}