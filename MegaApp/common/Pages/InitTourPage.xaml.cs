using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Classes;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.Resources;

namespace MegaApp.Pages
{
    public partial class InitTourPage : MegaPhoneApplicationPage
    {
        /// <summary>
        /// Flag to try to avoid display duplicate alerts
        /// </summary>
        private bool isAlertAlreadyDisplayed;

        public InitTourPage()
        {
            isAlertAlreadyDisplayed = false;    // Default value

            InitializeComponent();

            // Determine the visibility of the dark background.
            if(((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"]) != Visibility.Visible)
            {
                ImgMegaSpace.Source = new BitmapImage(new Uri("/Assets/Images/01B_storage.png", UriKind.Relative));
                ImgMegaSpeed.Source = new BitmapImage(new Uri("/Assets/Images/02B_speed.png", UriKind.Relative));
                ImgMegaPrivacy.Source = new BitmapImage(new Uri("/Assets/Images/03B_security.png", UriKind.Relative));
                ImgMegaAccess.Source = new BitmapImage(new Uri("/Assets/Images/04B_access.png", UriKind.Relative));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            

            // Remove the main page from the stack. If user presses back button it will then exit the application
            // Also removes the login page and the create account page after the user has created the account succesful            
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            // Try to avoid display duplicate alerts
            if (isAlertAlreadyDisplayed) return;
            isAlertAlreadyDisplayed = true;

            switch(navParam)
            {
                case NavigationParameter.CreateAccount:
                    // Show the success message
                    new CustomMessageDialog(
                        AppMessages.ConfirmNeeded_Title,
                        AppMessages.ConfirmNeeded,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                    break;

                case NavigationParameter.API_ESID:
                    // Show a message notifying the error
                    new CustomMessageDialog(
                        AppMessages.SessionIDError_Title,
                        AppMessages.SessionIDError,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                    break;

                case NavigationParameter.API_EBLOCKED:
                    // Show a message notifying the error
                    new CustomMessageDialog(
                        AppMessages.AM_AccountBlocked_Title,
                        AppMessages.AM_AccountBlocked, 
                        App.AppInformation, 
                        MessageDialogButtons.Ok).ShowDialog();
                    break;

                case NavigationParameter.API_ESSL:
                    // Show a message notifying the error
                    new CustomMessageDialog(
                        AppMessages.SSLKeyError_Title,
                        AppMessages.SSLKeyError,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                    break;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {            
            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "0" } });            
        }

        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "1" } });            
        }
    }
}