using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
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

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);

            LabelSize.Text = String.Concat(UiResources.UI_Size, ":");
            LabelContent.Text = String.Concat(UiResources.UI_Content, ":");
            LabelAdded.Text = String.Concat(UiResources.UI_Added, ":");
            LabelModified.Text = String.Concat(UiResources.UI_Modified, ":");
        }

        // Code to execute when a Network change is detected.
        private void NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NetworkNotificationType.InterfaceConnected:
                    UpdateGUI();
                    break;
                case NetworkNotificationType.InterfaceDisconnected:
                    UpdateGUI(false);
                    break;
                case NetworkNotificationType.CharacteristicUpdate:
                default:
                    break;
            }
        }

        private void UpdateGUI(bool isNetworkConnected = true)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {                
                BtnAvailableOfflineSwitch.IsEnabled = isNetworkConnected;
                SetApplicationBar(isNetworkConnected);
            });
        }

        public void SetApplicationBar(bool isNetworkConnected = true)
        {
            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources();

            // Change and translate the current application bar
            _nodeDetailsViewModel.ChangeMenu(this.ApplicationBar.Buttons, 
                this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
        }

        private void SetAppbarResources()
        {
            if(_nodeViewModel.ParentContainerType == ContainerType.FolderLink)
            {
                this.ApplicationBar = (ApplicationBar)Resources["FolderLinkNodeDetailsMenu"];
            }
            else
            {
                if (_nodeViewModel.IsFolder)
                {
                    if (_nodeViewModel.IsExported)
                        this.ApplicationBar = (ApplicationBar)Resources["ExportedFolderDetailsMenu"];
                    else
                        this.ApplicationBar = (ApplicationBar)Resources["FolderDetailsMenu"];
                }
                else //Node is a File
                {
                    if (_nodeViewModel.IsExported)
                        this.ApplicationBar = (ApplicationBar)Resources["ExportedFileDetailsMenu"];
                    else
                        this.ApplicationBar = (ApplicationBar)Resources["FileDetailsMenu"];
                }
            }            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            _nodeDetailsViewModel.Initialize(App.GlobalListener);

            if (App.AppInformation.IsStartupModeActivate)
            {
                // Needed on every UI interaction
                SdkService.MegaSdk.retryPendingConnections();

                if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                {
                    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                    return;
                }

                App.AppInformation.IsStartupModeActivate = false;
                
#if WINDOWS_PHONE_81
                // Check to see if any files have been picked
                var app = Application.Current as App;
                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs,
                        new FolderViewModel(SdkService.MegaSdk, App.AppInformation, _nodeViewModel.ParentContainerType));
                }
#endif
                return;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _nodeDetailsViewModel.Deinitialize(App.GlobalListener);
            base.OnNavigatedFrom(e);
        }

        private void OnDownloadClick(object sender, EventArgs e)
        {
            if (_nodeViewModel.ParentContainerType == ContainerType.FolderLink)
            {
                App.LinkInformation.SelectedNodes.Add(_nodeViewModel);
                App.LinkInformation.LinkAction = LinkAction.Download;
            }

            _nodeDetailsViewModel.Download();            
        }

        private void OnImportClick(object sender, EventArgs e)
        {
            if (_nodeViewModel.ParentContainerType == ContainerType.FolderLink)
            {
                App.LinkInformation.SelectedNodes.Add(_nodeViewModel);
                App.LinkInformation.LinkAction = LinkAction.Import;

                NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.ImportFolderLink);
            }
        }

        private async void OnRemoveClick(object sender, EventArgs e)
        {   
            NodeActionResult result = await _nodeDetailsViewModel.Remove();
            if (result == NodeActionResult.Cancelled) return;

            try
            {
                if (NavigateService.CanGoBack())
                    NavigateService.GoBack();
                else
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("NavigateService - GoBack"))
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
            }
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