using System.Windows.Controls;
using System.Windows.Input;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Navigation;
using mega;

namespace MegaApp.Pages
{
    public partial class LoginPage : PhoneApplicationPage
    {
        private readonly LoginViewModel _loginViewModel;

        public LoginPage()
        {
            _loginViewModel = new LoginViewModel(App.MegaSdk);
            this.DataContext = _loginViewModel;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.LoginText;
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

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var control = sender as Control;
            if (control != null) control.TabToNextControl((Panel)control.Parent, this);
        }
    }
}