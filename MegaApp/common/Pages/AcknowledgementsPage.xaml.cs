using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Models;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class AcknowledgementsPage : MegaPhoneApplicationPage
    {
        public AcknowledgementsPage()
        {
            var acknowledgementsViewModel = new AcknowledgementsViewModel();
            this.DataContext = acknowledgementsViewModel;
            InitializeComponent();
        }
    }
}