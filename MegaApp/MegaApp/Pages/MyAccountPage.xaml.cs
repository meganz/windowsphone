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
    public partial class MyAccountPage : PhoneApplicationPage
    {
        private MyAccountPageViewModel _myAccountPageViewModel;
        public MyAccountPage()
        {
            _myAccountPageViewModel = new MyAccountPageViewModel(App.MegaSdk);
            this.DataContext = _myAccountPageViewModel;
            InitializeComponent();
        }
    }
}