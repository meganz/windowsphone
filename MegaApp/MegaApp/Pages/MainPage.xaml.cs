using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        private CloudDriveViewModel cloudDriveViewModel;
        public MainPage()
        {
            cloudDriveViewModel = new CloudDriveViewModel(App.MegaSdk);
            this.DataContext = cloudDriveViewModel;

            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationParameter navParam = NavigationUriBuilder.ProcessQueryString(NavigationContext.QueryString);

            if (navParam != NavigationParameter.Login)
            {
                if (!SettingsService.LoadSetting<bool>(SettingsResources.RememberMe))
                {
                    NavigationService.Navigate(NavigationUriBuilder.BuildNavigationUri(typeof(LoginPage),
                        NavigationParameter.Normal));
                    return;
                }
                else
                {
                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession));
                }
            }

            // Remove the login page from the stack. If user presses back button it will then exit the application
            NavigationService.RemoveBackEntry();

            cloudDriveViewModel.GetNodes();

            base.OnNavigatedTo(e);
        }
    }
}