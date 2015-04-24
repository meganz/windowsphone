using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class TransferPage : PhoneApplicationPage
    {
        private TransfersViewModel _transfersViewModel;

        public TransferPage()
        {
            _transfersViewModel = new TransfersViewModel(App.MegaSdk, App.AppInformation, App.MegaTransfers);
            this.DataContext = _transfersViewModel;
            InitializeComponent();

            // Initialize the hamburger menu / slide in
            MainDrawerLayout.InitializeDrawerLayout();
            MainDrawerLayout.DrawerOpened += OnDrawerOpened;
            MainDrawerLayout.DrawerClosed += OnDrawerClosed;

            SetApplicationBar();

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));
        }

        private void SetApplicationBar()
        {
            this.ApplicationBar = (ApplicationBar)Resources["TransferMenu"];

            //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.StartResumeAll.ToLower();
            //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.PauseAll.ToLower();
            //((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = UiResources.CancelAll.ToLower();
            //((ApplicationBarIconButton)ApplicationBar.Buttons[3]).Text = UiResources.CleanUpTransfers.ToLower();
                        
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();
            
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.CancelAllTransfers.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.CleanUpTransfers.ToLower();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (navParam == NavigationParameter.Downloads)
                Transfers.SelectedItem = Downloads;

            if (navParam == NavigationParameter.PictureSelected)
                NavigationService.RemoveBackEntry();

            if (navParam == NavigationParameter.AlbumSelected || navParam == NavigationParameter.SelfieSelected)
            {
                NavigationService.RemoveBackEntry();
                NavigationService.RemoveBackEntry();
            }

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            // Check if Hamburger Menu is open in view. If open. First slide out before exit
            e.Cancel = _transfersViewModel.CheckHamburgerMenu(MainDrawerLayout, e.Cancel);

            base.OnBackKeyPress(e);
        }

        private void OnPauseAllClick(object sender, System.EventArgs e)
        {        
	        // Exit if no transfers
            if (App.MegaTransfers.Count < 1) return;

            // Check if there are active transfers
            bool activeTransfers = false;
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                if (transfer == null) continue;

                if ((transfer.Status != TransferStatus.Downloaded) && (transfer.Status != TransferStatus.Uploaded)
                    && (transfer.Status != TransferStatus.Canceled) && (transfer.Status != TransferStatus.Error))
                {
                    activeTransfers = true;
                    break;
                }
            }

            if (activeTransfers) 
            {
                App.MegaSdk.pauseTransfers(true, new PauseTransferRequestListener(true));

                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.play.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Resume.ToLower();
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnPauseAllClick;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnStartResumeAllClick;

                if(LstDownloads.ItemCount > 0)
                {
                    ImgDownloadsPaused.Visibility = Visibility.Visible;
                    LstDownloads.Opacity = 0.3;                    
                }                    

                if (LstUploads.ItemCount > 0)
                {
                    ImgUploadsPaused.Visibility = Visibility.Visible;
                    LstUploads.Opacity = 0.3;
                }                    
            }
        }

        private void OnStartResumeAllClick(object sender, EventArgs e)
        {
            // Exit if no transfers
            if (App.MegaTransfers.Count < 1) return;

            // Check if there are paused transfers
            bool pausedTransfers = false;
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                if (transfer == null) continue;

                if ((transfer.Status == TransferStatus.Paused) || (transfer.Status == TransferStatus.NotStarted))
                {
                    pausedTransfers = true;
                    break;
                }
            }

            if (pausedTransfers)
            {
                App.MegaSdk.pauseTransfers(false, new PauseTransferRequestListener(false));

                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

                ImgDownloadsPaused.Visibility = Visibility.Collapsed;
                ImgUploadsPaused.Visibility = Visibility.Collapsed;
                LstDownloads.Opacity = LstUploads.Opacity = 1;
            }
        }

        private void OnCancelAllClick(object sender, EventArgs e)
        {
            if (App.MegaTransfers.Count < 1) return;
                        
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                if (transfer == null) continue;
                
                transfer.CancelTransfer();
            }

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/Appbar/transport.pause.png", UriKind.Relative);
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Pause.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= OnStartResumeAllClick;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += OnPauseAllClick;

            ImgDownloadsPaused.Visibility = Visibility.Collapsed;
            ImgUploadsPaused.Visibility = Visibility.Collapsed;
            LstDownloads.Opacity = LstUploads.Opacity = 1;
        }

        private void OnCleanUpTransfersClick(object sender, EventArgs e)
        {
            if (App.MegaTransfers.Count < 1) return;

            var transfersToRemove = new List<TransferObjectModel>();
            foreach (var item in App.MegaTransfers)
            {
                var transfer = (TransferObjectModel)item;
                    if (transfer == null) continue;
                if (transfer.Status == TransferStatus.Downloaded || transfer.Status == TransferStatus.Uploaded ||
                    transfer.Status == TransferStatus.Canceled || transfer.Status == TransferStatus.Error)
                {
                    transfersToRemove.Add(transfer);
                    // Clean up: remove the upload copied file from the cache
                    if (transfer.Type == TransferType.Upload)
                        File.Delete(transfer.FilePath);
                }
            }

            foreach (var item in transfersToRemove)
                App.MegaTransfers.Remove(item);
        }

        private void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var hamburgerMenuItem = e.Item.DataContext as HamburgerMenuItem;
            if (hamburgerMenuItem == null) return;

            if (hamburgerMenuItem.Type == HamburgerMenuItemType.Transfers)
                MainDrawerLayout.CloseDrawer();
            else
                hamburgerMenuItem.TapAction.Invoke();
            
            LstHamburgerMenu.SelectedItem = null;
        }

        private void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MainDrawerLayout.OpenDrawer();
        }

        private void OnDrawerClosed(object sender)
        {
            SetApplicationBar();
        }

        private void OnDrawerOpened(object sender)
        {
            // Remove application bar from display when sliding in the hamburger menu
            this.ApplicationBar = null;
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }
        
    }
}