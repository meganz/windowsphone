using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Pages
{
    public partial class NodeDetailsPage : MegaPhoneApplicationPage
    {
        private readonly NodeDetailsViewModel _nodeDetailsViewModel;
        private readonly NodeViewModel _nodeViewModel;

        private bool isBtnAvailableOfflineSwitchLoaded = false;

        public NodeDetailsPage()
        {
            _nodeViewModel = NavigateService.GetNavigationData<NodeViewModel>();
            _nodeDetailsViewModel = new NodeDetailsViewModel(this, _nodeViewModel);

            this.DataContext = _nodeDetailsViewModel;

            InitializeComponent();

            SetApplicationBar();            
        }

        public void SetApplicationBar()
        {
            this.ApplicationBar = (ApplicationBar)Resources["NodeDetailsMenu"];
            
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Download.ToLower();            
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Remove.ToLower();
            
            ApplicationBar.MenuItems.Clear();

            ApplicationBarMenuItem rename = new ApplicationBarMenuItem(UiResources.Rename.ToLower());
            ApplicationBar.MenuItems.Add(rename);
            rename.Click += new EventHandler(OnRenameClick);
                        
            if (_nodeViewModel.IsFolder)
            {
                ApplicationBarMenuItem createShortcut = new ApplicationBarMenuItem(UiResources.CreateShortCut.ToLower());
                ApplicationBar.MenuItems.Add(createShortcut);
                createShortcut.Click += new EventHandler(OnCreateShortcutClick);
            }
                        
            if(!_nodeViewModel.IsExported)
            {
                ApplicationBarMenuItem getLink = new ApplicationBarMenuItem(UiResources.GetLink.ToLower());
                ApplicationBar.MenuItems.Add(getLink);
                getLink.Click += new EventHandler(OnGetLinkClick);
            }                
            else
            {
                ApplicationBarMenuItem manageLink = new ApplicationBarMenuItem(UiResources.ManageLink.ToLower());
                ApplicationBar.MenuItems.Add(manageLink);
                manageLink.Click += new EventHandler(OnGetLinkClick);

                ApplicationBarMenuItem removeLink = new ApplicationBarMenuItem(UiResources.RemoveLink.ToLower());
                ApplicationBar.MenuItems.Add(removeLink);
                removeLink.Click += new EventHandler(OnRemoveLinkClick);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _nodeDetailsViewModel.Initialize(App.GlobalDriveListener);

            if (App.AppInformation.IsStartupModeActivate)
            {
                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();

                App.AppInformation.IsStartupModeActivate = false;
                
#if WINDOWS_PHONE_81
                // Check to see if any files have been picked
                var app = Application.Current as App;
                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs);
                }
                return;
#endif
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _nodeDetailsViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        private void OnDownloadClick(object sender, EventArgs e)
        {
            _nodeDetailsViewModel.Download();            
        }

        private async void OnRemoveClick(object sender, EventArgs e)
        {   
            NodeActionResult result = await _nodeDetailsViewModel.Remove();
            if (result == NodeActionResult.Cancelled) return;
            NavigateService.GoBack();            
        }

        private void OnGetLinkClick(object sender, EventArgs e)
        {
            _nodeDetailsViewModel.GetLink();
        }

        private void OnRemoveLinkClick(object sender, EventArgs e)
        {
            _nodeDetailsViewModel.RemoveLink();
        }

        private void OnRenameClick(object sender, EventArgs e)
        {
            _nodeDetailsViewModel.Rename();
        }

        private void OnCreateShortcutClick(object sender, EventArgs e)
        {
            _nodeDetailsViewModel.CreateShortcut();
        }

        private void BtnAvailableOfflineSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            this.isBtnAvailableOfflineSwitchLoaded = true;
        }

        private void BtnAvailableOfflineSwitch_CheckedChanged(object sender, Telerik.Windows.Controls.CheckedChangedEventArgs e)
        {
            if(this.isBtnAvailableOfflineSwitchLoaded)
                _nodeDetailsViewModel.SaveForOffline(e.NewState);
        }
    }
}