using System.Windows.Controls;
using System.Windows.Input;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using mega;

namespace MegaApp.Pages
{
    public partial class LoginPage : PhoneApplicationPage
    {
        private readonly LoginAndCreateAccountViewModelContainer _loginAndCreateAccountViewModelContainer;
        
        public LoginPage()
        {
            _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer();
            
            this.DataContext = _loginAndCreateAccountViewModelContainer;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Accept.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Cancel.ToLower();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("item"))
            {
                var index = NavigationContext.QueryString["item"];
                var indexParsed = int.Parse(index);
                Pivot_LoginAndCreateAccount.SelectedIndex = indexParsed;
            }

            // Remove the main page from the stack. If user presses back button it will then exit the application
            // Also removes the create account page after the user has created the account succesful
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
            e.Cancel = true;
            return;
        }

        private void OnAcceptClick(object sender, System.EventArgs e)        
        {
            if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_Login)                
                _loginAndCreateAccountViewModelContainer._loginViewModel.DoLogin();
            else if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_CreateAccount)                
                _loginAndCreateAccountViewModelContainer._createAccountViewModel.CreateAccount();
        }

        private void OnCancelClick(object sender, System.EventArgs e)
        {
            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
        }        

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var control = sender as Control;
            if (control != null) control.TabToNextControl((Panel)control.Parent, this);
        }
    }
}