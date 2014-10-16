using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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
        }

        private void OnCreateAccountClick(object sender, System.EventArgs e)
        {
            _createAccountViewModel.CreateAccount();
        }
    }
}