using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Primitives;

namespace MegaApp.Pages
{
    public partial class CreateAccountPage : PhoneApplicationPage
    {
        private readonly CreateAccountViewModel _createAccountViewModel;

        public CreateAccountPage()
        {
            _createAccountViewModel = new CreateAccountViewModel(App.MegaSdk);
            this.DataContext = _createAccountViewModel;

            InitializeComponent();

            SetApplicationBar();
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.CreateAccount.ToLower();            
        }

        private void OnCreateAccountClick(object sender, System.EventArgs e)
        {
            _createAccountViewModel.CreateAccount();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var control = sender as Control;
            if (control != null) control.TabToNextControl((Panel) control.Parent, this);
        }
    }
}