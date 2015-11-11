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
            
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.Rename.ToLower();
            
            if(!_nodeViewModel.IsExported)
            {
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.GetLink.ToLower();

                if (ApplicationBar.MenuItems.Count == 3)
                    ApplicationBar.MenuItems.RemoveAt(2);                
            }                
            else
            {
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.ManageLink.ToLower();

                if (ApplicationBar.MenuItems.Count == 2)
                {
                    ApplicationBarMenuItem removeLink = new ApplicationBarMenuItem(UiResources.RemoveLink.ToLower());
                    ApplicationBar.MenuItems.Add(removeLink);
                    removeLink.Click += new EventHandler(OnRemoveLinkClick);
                }
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