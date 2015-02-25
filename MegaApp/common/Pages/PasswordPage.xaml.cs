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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class PasswordPage : PhoneApplicationPage
    {
        private readonly PasswordViewModel _passwordViewModel;
        public PasswordPage()
        {
            _passwordViewModel = new PasswordViewModel();
            this.DataContext = _passwordViewModel;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.DoneButton;         
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //if (NavigateService.ProcessQueryString(NavigationContext.QueryString) == NavigationParameter.DisablePassword)
            //{
            //    _passwordViewModel.IsDisablePassword = true;
            //    return;
            //}

            NavigationService.RemoveBackEntry();
        }

        private void OnPasswordLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtPassword.Focus();
        }

        private void OnDoneClick(object sender, System.EventArgs e)
        {
        	_passwordViewModel.CheckPassword();
        }

        
    }
}