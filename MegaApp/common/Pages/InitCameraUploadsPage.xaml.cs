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
            _cameraUploadsViewModel = new CameraUploadsViewModel(SdkService.MegaSdk, App.AppInformation);
            this.DataContext = _cameraUploadsViewModel;

            InitializeComponent();

            SetApplicationBar();

            // If user skips the initialization of the "Camera Uploads" service, by default 
            // the connection type is 'Wifi only' and the service is disabled or turned off
            SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsIsEnabled, false);
            SettingsService.SaveSetting<int>(SettingsResources.CameraUploadsConnectionType, (int)CameraUploadsConnectionType.WifiOnly);
        }        

        public void SetApplicationBar(bool isEnabled = true)
        {
            this.ApplicationBar = (ApplicationBar)Resources["InitCameraUploadsMenu"];

            // Change and translate the current application bar
            _cameraUploadsViewModel.ChangeMenu(this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isEnabled);
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

            try
            {
                if (NavigateService.PreviousPage != null)
                    NavigateService.GoBack();
                else
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("NavigateService - GoBack"))
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            finally
            {
                e.Cancel = true;
            }            
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            SetApplicationBar(false);

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
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("NavigateService - GoBack"))
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            finally
            {
                SetApplicationBar(true);
            }           
        }

        private void OnSkipClick(object sender, EventArgs e)
        {
            SetApplicationBar(false);

            try
            {
                if (NavigateService.PreviousPage != null)
                    NavigateService.GoBack();
                else
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("NavigateService - GoBack"))
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            finally
            {
                SetApplicationBar(true);
            }
        }        
    }
}