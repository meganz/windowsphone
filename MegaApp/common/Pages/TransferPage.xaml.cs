using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using mega;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class TransferPage : PhoneDrawerLayoutPage
    {
        private readonly TransfersViewModel _transfersViewModel;

        public TransferPage()
        {
            _transfersViewModel = new TransfersViewModel(App.MegaSdk, App.AppInformation, App.MegaTransfers);
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
                    if (!Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
                    {
                        NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.None);
                        return;
                    }

                    _transfersViewModel.SetEmptyContentTemplate();
                    LstUploads.ItemsSource = _transfersViewModel.MegaTransfers.Uploads;
                    LstDownloads.ItemsSource = _transfersViewModel.MegaTransfers.Downloads;
                }
                else
                {
                    _transfersViewModel.SetOfflineContentTemplate();
                    LstUploads.ItemsSource = null;
                    LstDownloads.ItemsSource = null;
                }

                SetApplicationBarData(isNetworkConnected);
            });
        }

        private void SetApplicationBarData(bool isNetworkConnected = true)
        {
            this.ApplicationBar = (ApplicationBar)Resources["TransferMenu"];

            if(TransfersPivot.SelectedItem != null)
            {
                if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
                    SetDownloadsPivotGUI(App.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_DOWNLOAD));
                else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
                    SetUploadsPivotGUI(App.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_UPLOAD));
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();
            }
            
            
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.CancelAllTransfers.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.CleanUpTransfers.ToLower();

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
        }

        private void SetDownloadsPivotGUI(bool paused)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnPauseAllClick;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;

            if (paused)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.play.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Resume.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnStartResumeAllClick;

                ImgDownloadsPaused.Visibility = Visibility.Visible;
                LstDownloads.Opacity = 0.3;

                _transfersViewModel.SetEmptyContentTemplate(true, (int)MTransferType.TYPE_DOWNLOAD);
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

                ImgDownloadsPaused.Visibility = Visibility.Collapsed;
                LstDownloads.Opacity = 1;

                _transfersViewModel.SetEmptyContentTemplate(false, (int)MTransferType.TYPE_DOWNLOAD);
            }
        }

        private void SetUploadsPivotGUI(bool paused)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnPauseAllClick;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;

            if (paused)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.play.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Resume.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnStartResumeAllClick;

                ImgUploadsPaused.Visibility = Visibility.Visible;
                LstUploads.Opacity = 0.3;

                _transfersViewModel.SetEmptyContentTemplate(true, (int)MTransferType.TYPE_UPLOAD);
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();                
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

                ImgUploadsPaused.Visibility = Visibility.Collapsed;
                LstUploads.Opacity = 1;

                _transfersViewModel.SetEmptyContentTemplate(false, (int)MTransferType.TYPE_UPLOAD);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (navParam == NavigationParameter.Downloads)
                TransfersPivot.SelectedItem = DownloadsPivot;

            if (navParam == NavigationParameter.PictureSelected)
                NavigationService.RemoveBackEntry();

            if (navParam == NavigationParameter.AlbumSelected || navParam == NavigationParameter.SelfieSelected)
            {
                NavigationService.RemoveBackEntry();
                NavigationService.RemoveBackEntry();
            }

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            TransfersService.UpdateMegaTransfersList(App.MegaTransfers);
            _transfersViewModel.MegaTransfers = App.MegaTransfers;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            // Check if can go back in the stack of pages
            e.Cancel = CheckGoBack(e.Cancel);
        }

        private void OnPauseAllClick(object sender, EventArgs e)
        {        
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
            {
                App.MegaSdk.pauseTransfersDirection(true, (int)MTransferType.TYPE_DOWNLOAD,
                    new PauseTransferRequestListener());
                SetDownloadsPivotGUI(true);
            }                
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
            {
                App.MegaSdk.pauseTransfersDirection(true, (int)MTransferType.TYPE_UPLOAD,
                    new PauseTransferRequestListener());
                SetUploadsPivotGUI(true);
            }
        }

        private void OnStartResumeAllClick(object sender, EventArgs e)
        {
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
            {
                App.MegaSdk.pauseTransfersDirection(false, (int)MTransferType.TYPE_DOWNLOAD,
                    new PauseTransferRequestListener());
                SetDownloadsPivotGUI(false);
            }
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
            {
                App.MegaSdk.pauseTransfersDirection(false, (int)MTransferType.TYPE_UPLOAD,
                    new PauseTransferRequestListener());
                SetUploadsPivotGUI(false);
            }
        }

        private void OnCancelAllClick(object sender, EventArgs e)
        {
            if (TransfersPivot.SelectedItem.Equals(DownloadsPivot))
            {
                App.MegaSdk.cancelTransfers((int)MTransferType.TYPE_DOWNLOAD);
                SetDownloadsPivotGUI(false);
            }                
            else if (TransfersPivot.SelectedItem.Equals(UploadsPivot))
            {
                App.MegaSdk.cancelTransfers((int)MTransferType.TYPE_UPLOAD);
                SetUploadsPivotGUI(false);
            }
        }

        private void OnCleanUpTransfersClick(object sender, EventArgs e)
        {
            TransfersService.UpdateMegaTransfersList(App.MegaTransfers);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] == DownloadsPivot)
                SetDownloadsPivotGUI(App.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_DOWNLOAD));
            else if (e.AddedItems[0] == UploadsPivot)
                SetUploadsPivotGUI(App.MegaSdk.areTransfersPaused((int)MTransferType.TYPE_UPLOAD));
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