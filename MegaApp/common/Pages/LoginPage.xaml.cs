using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MegaApp.Containers;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class LoginPage : MegaPhoneApplicationPage
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
            // Also removes the settings page when the user has selected app in auto upload but was not logged in.
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
            
            e.Cancel = true;
        }

        private void OnAcceptClick(object sender, EventArgs e)
        {
            // To not allow cancel a request to login or 
            // create account once that is started
            this.ApplicationBar.IsVisible = false;            

            if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_Login)                
                _loginAndCreateAccountViewModelContainer.LoginViewModel.DoLogin();
            else if (Pivot_LoginAndCreateAccount.SelectedItem == PivotItem_CreateAccount)                
                _loginAndCreateAccountViewModelContainer.CreateAccountViewModel.CreateAccount();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
        }        

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var control = sender as Control;
            if (control != null) control.TabToNextControl((Panel)control.Parent, this);
        }
    }
}