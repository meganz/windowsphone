using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class ConfirmAccountPage : MegaPhoneApplicationPage
    {
        private readonly ConfirmAccountViewModel _confirmAccountViewModel;
        public ConfirmAccountPage()
        {
            _confirmAccountViewModel = new ConfirmAccountViewModel(App.MegaSdk);
            this.DataContext = _confirmAccountViewModel;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
                App.MegaSdk.logout(new LogOutRequestListener(false));

            // Remove all pages from the stack (including the MainPage).
            // If user presses back button should go to InitTourPage or exit the application            
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            if (NavigateService.ProcessQueryString(NavigationContext.QueryString) == NavigationParameter.UriLaunch
                && NavigationContext.QueryString.ContainsKey("confirm"))
            {
                _confirmAccountViewModel.ConfirmCode = HttpUtility.UrlDecode(NavigationContext.QueryString["confirm"]);                
                App.MegaSdk.querySignupLink(_confirmAccountViewModel.ConfirmCode, _confirmAccountViewModel);
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);

            e.Cancel = true;
        }
    }
}