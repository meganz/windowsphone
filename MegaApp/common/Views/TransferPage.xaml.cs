using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using mega;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class TransferPage : PhoneDrawerLayoutPage
    {
        private TransfersViewModel _transfersViewModel;

        public TransferPage()
        {
            _transfersViewModel = new TransfersViewModel(SdkService.MegaSdk, App.AppInformation);
            this.DataContext = _transfersViewModel;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout,LstHamburgerMenu, HamburgerMenuItemType.Transfers);

            SetApplicationBarData();

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);
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
                if (isNetworkConnected)
                {
                    if (!Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
                    {
                        NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.None);
                        return;
                    }

                    _transfersViewModel.Uploads.SetEmptyContentTemplate();
                    _transfersViewModel.Downloads.SetEmptyContentTemplate();
                    LstUploads.ItemsSource = _transfersViewModel.Uploads.Items;
                    LstDownloads.ItemsSource = _transfersViewModel.Downloads.Items;
                }
                else
                {
                    _transfersViewModel.Uploads.SetOfflineContentTemplate();
                    _transfersViewModel.Downloads.SetOfflineContentTemplate();
                    LstUploads.ItemsSource = null;
                    LstDownloads.ItemsSource = null;
                }

                SetApplicationBarData(isNetworkConnected);
            });
        }

        private void SetApplicationBarData(bool isNetworkConnected = true)
        {
            if(TransfersPivot.SelectedItem != null)
            {
                if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                    SetDownloadsPivotGUI(SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_DOWNLOAD));
                else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
                    SetUploadsPivotGUI(SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_UPLOAD));
                else if (TransfersPivot.SelectedItem.Equals(CompletedPivot))
                    SetCompletedPivotGUI();
            }
            else
            {
                this.ApplicationBar = (ApplicationBar)Resources["ActiveTransfersMenu"];
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.CancelAllTransfers.ToLower();
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.CleanUpTransfers.ToLower();
            }

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
        }

        private void SetDownloadsPivotGUI(bool paused)
        {
            this.ApplicationBar = (ApplicationBar)Resources["ActiveTransfersMenu"];

            if (_transfersViewModel == null)
                _transfersViewModel = new TransfersViewModel(SdkService.MegaSdk, App.AppInformation);

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnPauseAllClick;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;

            if (paused)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.play.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Resume.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnStartResumeAllClick;

                ImgDownloadsPaused.Visibility = Visibility.Visible;
                LstDownloads.Opacity = 0.3;
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

                ImgDownloadsPaused.Visibility = Visibility.Collapsed;
                LstDownloads.Opacity = 1;
            }

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.CancelAllTransfers.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.CleanUpTransfers.ToLower();

            _transfersViewModel.Downloads.SetEmptyContentTemplate();
        }

        private void SetUploadsPivotGUI(bool paused)
        {
            this.ApplicationBar = (ApplicationBar)Resources["ActiveTransfersMenu"];

            if(_transfersViewModel == null)
                _transfersViewModel = new TransfersViewModel(SdkService.MegaSdk, App.AppInformation);

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnPauseAllClick;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;

            if (paused)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.play.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Resume.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnStartResumeAllClick;

                ImgUploadsPaused.Visibility = Visibility.Visible;
                LstUploads.Opacity = 0.3;
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

                ImgUploadsPaused.Visibility = Visibility.Collapsed;
                LstUploads.Opacity = 1;
            }

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.CancelAllTransfers.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.CleanUpTransfers.ToLower();

            _transfersViewModel.Uploads.SetEmptyContentTemplate();
        }

        private void SetCompletedPivotGUI()
        {
            this.ApplicationBar = (ApplicationBar)Resources["CompletedTransfersMenu"];

            if (_transfersViewModel == null)
                _transfersViewModel = new TransfersViewModel(SdkService.MegaSdk, App.AppInformation);

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/AppBar/remove.png", UriKind.Relative);
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.CleanUpTransfers.ToLower();

            _transfersViewModel.Completed.SetEmptyContentTemplate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (navParam == NavigationParameter.Downloads)
                TransfersPivot.SelectedItem = DownloadsPivot;
            else if (navParam == NavigationParameter.Uploads)
                TransfersPivot.SelectedItem = UploadsPivot;

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            _transfersViewModel.Update();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            // Check if can go back in the stack of pages
            e.Cancel = CheckGoBack(e.Cancel);
        }

        private void OnPauseAllClick(object sender, EventArgs e)
        {
            if (_transfersViewModel.ActiveViewModel.PauseOrResumeCommand.CanExecute(sender))
                _transfersViewModel.ActiveViewModel.PauseOrResumeCommand.Execute(sender);

            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                SetDownloadsPivotGUI(true);
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
                SetUploadsPivotGUI(true);
        }

        private void OnStartResumeAllClick(object sender, EventArgs e)
        {
            if (_transfersViewModel.ActiveViewModel.PauseOrResumeCommand.CanExecute(sender))
                _transfersViewModel.ActiveViewModel.PauseOrResumeCommand.Execute(sender);

            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                SetDownloadsPivotGUI(false);
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
                SetUploadsPivotGUI(false);
        }

        private void OnCancelAllClick(object sender, EventArgs e)
        {
            if (_transfersViewModel.ActiveViewModel.CancelCommand.CanExecute(sender))
                _transfersViewModel.ActiveViewModel.CancelCommand.Execute(sender);

            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                SetDownloadsPivotGUI(false);
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
                SetUploadsPivotGUI(false);
        }

        private void OnCleanUpTransfersClick(object sender, EventArgs e)
        {
            if (_transfersViewModel.ActiveViewModel.CleanCommand.CanExecute(sender))
                _transfersViewModel.ActiveViewModel.CleanCommand.Execute(sender);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TransfersPivot.SelectedIndex)
            {
                case 0:
                    SetDownloadsPivotGUI(SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_DOWNLOAD));
                    this._transfersViewModel.ActiveViewModel = this._transfersViewModel.Downloads;
                    break;
                case 1:
                    SetUploadsPivotGUI(SdkService.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_UPLOAD));
                    this._transfersViewModel.ActiveViewModel = this._transfersViewModel.Uploads;
                    break;
                case 2:
                    SetCompletedPivotGUI();
                    this._transfersViewModel.ActiveViewModel = this._transfersViewModel.Completed;
                    break;
            }
        }

        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData(NetworkService.IsNetworkAvailable());
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        #region Override Events

        // XAML can not bind them direct from the base class
        // That is why these are dummy event handlers

        protected override void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            base.OnHamburgerTap(sender, e);
        }

        protected override void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            base.OnHamburgerMenuItemTap(sender, e);
        }

        #endregion
    }
}