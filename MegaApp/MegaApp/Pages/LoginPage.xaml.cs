using System.Windows.Controls;
using MegaApp.Models;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;
using mega;

namespace MegaApp.Pages
{
    public partial class LoginPage : PhoneApplicationPage
    {
        private LoginViewModel _loginViewModel;

        public LoginPage()
        {
            _loginViewModel = new LoginViewModel(App.MegaSdk);
            this.DataContext = _loginViewModel;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Remove the main page from the stack. If user presses back button it will then exit the application
            // Also removes the create account page after the user has created the account succesful
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        private void OnLoginClick(object sender, System.EventArgs e)
        {
            _loginViewModel.DoLogin();
        }
    }
}