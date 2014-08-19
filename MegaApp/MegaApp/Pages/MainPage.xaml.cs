using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            this.DataContext = App.CloudDrive;
            InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationService.RemoveBackEntry();
                
                MNode parent = App.MegaSdk.getParentNode(App.CloudDrive.CurrentRootNode.GetBaseNode());
                if (parent.getType() == MNodeType.TYPE_UNKNOWN) parent = App.MegaSdk.getRootNode();
                App.CloudDrive.CurrentRootNode = new NodeViewModel(App.MegaSdk, parent);
                
                navParam = NavigationParameter.Browsing;
            }

            switch (navParam)
            {
                case NavigationParameter.Browsing:
                {
                    App.CloudDrive.GetNodes();
                    return;
                    break;
                }
                case NavigationParameter.Login:
                {
                    // Remove the login page from the stack. If user presses back button it will then exit the application
                    NavigationService.RemoveBackEntry();
                    
                    App.CloudDrive.FetchNodes();
                    break;
                }
                case NavigationParameter.Unknown:
                {
                    if (!SettingsService.LoadSetting<bool>(SettingsResources.RememberMe))
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                        return;
                    }
                    else
                    {
                        App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession));
                    }
                    break;
                }
            }

            base.OnNavigatedTo(e);
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if(e.Item == null || e.Item.DataContext == null) return;
            if (e.Item.DataContext as NodeViewModel == null) return;

            switch ((e.Item.DataContext as NodeViewModel).Type)
            {
                case MNodeType.TYPE_FOLDER:
                {
                    App.CloudDrive.SelectFolder(e.Item.DataContext as NodeViewModel);
                    break;
                }
            }

            
        }
    }
}