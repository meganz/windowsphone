using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class TransferPage : PhoneApplicationPage
    {
        public TransferPage()
        {
            this.DataContext = App.MegaTransfers;
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (navParam == NavigationParameter.Downloads)
                Transfers.SelectedItem = Downloads;
        }

    }
}