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
using MegaApp.MegaApi;
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
                App.CloudDrive.GoFolderUp();
                navParam = NavigationParameter.Browsing;
            }

            switch (navParam)
            {
                case NavigationParameter.Browsing:
                {
                    App.CloudDrive.LoadNodes();
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
                        App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener());
                        App.CloudDrive.FetchNodes();
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

            App.CloudDrive.OnNodeTap(e.Item.DataContext as NodeViewModel);
        }

        private void AddFolderClick(object sender, EventArgs e)
        {
            App.CloudDrive.AddFolder(App.CloudDrive.CurrentRootNode);
        }

        private void OnMenuOpening(object sender, Telerik.Windows.Controls.ContextMenuOpeningEventArgs e)
        {
            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || focusedListBoxItem.DataContext == null || !(focusedListBoxItem.DataContext is NodeViewModel))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                App.CloudDrive.FocusedNode = focusedListBoxItem.DataContext as NodeViewModel;
                var visibility = App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FILE ? Visibility.Visible : Visibility.Collapsed;
                BtnGetPreviewLink.Visibility = visibility;
                BtnDownloadItemCloud.Visibility = visibility;
            }
        }
    }
}