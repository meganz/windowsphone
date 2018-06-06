using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MegaApp.Containers;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Views
{
    public partial class LoginPage : MegaPhoneApplicationPage
    {
        private LoginAndCreateAccountViewModelContainer _loginAndCreateAccountViewModelContainer;
        
        public LoginPage()
        {
            _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);
            
            this.DataContext = _loginAndCreateAccountViewModelContainer;

            InitializeComponent();

            SetApplicationBar(true);
        }

        public void SetApplicationBar(bool isEnabled)
        {
            if (this.ApplicationBar == null) return;

            if (_loginAndCreateAccountViewModelContainer == null)
                _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);
            
            // Change and translate the current application bar
            _loginAndCreateAccountViewModelContainer.ChangeMenu(                
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);            

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isEnabled);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
                SdkService.MegaSdk.logout(new LogOutRequestListener(false));

            if (NavigationContext.QueryString.ContainsKey("Pivot"))
            {
                var index = NavigationContext.QueryString["Pivot"];
                var indexParsed = int.Parse(index);
                Pivot_LoginAndCreateAccount.SelectedIndex = indexParsed;
            }
            
            // Remove the main page from the stack. If user presses back button it will then exit the application
            // Also removes the create account page after the user has created the account succesful
            // Also removes the settings page when the user has selected app in auto upload but was not logged in.
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            if (NavigateService.ProcessQueryString(NavigationContext.QueryString) == NavigationParameter.UriLaunch
                && NavigationContext.QueryString.ContainsKey("newsignup"))
            {
                _loginAndCreateAccountViewModelContainer.CreateAccountViewModel.NewSignUpCode = 
                    HttpUtility.UrlDecode(NavigationContext.QueryString["newsignup"]);

                SdkService.MegaSdk.querySignupLink(_loginAndCreateAccountViewModelContainer.CreateAccountViewModel.NewSignUpCode,
                    new CreateAccountRequestListener(_loginAndCreateAccountViewModelContainer.CreateAccountViewModel, this));
            }
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
            if (!NetworkService.IsNetworkAvailable(true)) return;
            
            // To not allow cancel a request to login or 
            // create account once that is started
            SetApplicationBar(false);

            if (_loginAndCreateAccountViewModelContainer == null)
                _loginAndCreateAccountViewModelContainer = new LoginAndCreateAccountViewModelContainer(this);

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

        private void OnMegaHeaderLogoTapped(object sender, GestureEventArgs e)
        {
            DebugService.ChangeStatusAction();
        }
    }
}