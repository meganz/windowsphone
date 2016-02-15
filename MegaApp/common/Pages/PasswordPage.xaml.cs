using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class PasswordPage : MegaPhoneApplicationPage
    {
        private readonly PasswordViewModel _passwordViewModel;
        private Type _originPage;

        public PasswordPage()
        {
            _passwordViewModel = new PasswordViewModel();
            this.DataContext = _passwordViewModel;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Done.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Logout.ToLower();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationService.RemoveBackEntry();

            _originPage = NavigateService.GetNavigationData<Type>();
            if (_originPage == null || _originPage == typeof(NodeDetailsPage) || _originPage == typeof(PreviewImagePage))
                _originPage = typeof(MainPage);
        }

        private void OnPasswordLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtPassword.Focus();
        }

        private void OnDoneClick(object sender, System.EventArgs e)
        {
        	if (!_passwordViewModel.CheckPassword()) return;

            NavigationService.RemoveBackEntry();
            App.AppInformation.HasPinLockIntroduced = true;
            NavigateService.NavigateTo(_originPage, NavigationParameter.PasswordLogin);    
        }

        private void OnLogoutClick(object sender, EventArgs e)
        {
            _passwordViewModel.Logout();            
        }
    }
}