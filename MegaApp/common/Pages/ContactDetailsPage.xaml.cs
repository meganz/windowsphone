using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class ContactDetailsPage : PhoneDrawerLayoutPage
    {
        private readonly ContactDetailsViewModel _contactDetailsViewModel;        

        public ContactDetailsPage()
        {
            _contactDetailsViewModel = new ContactDetailsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _contactDetailsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.Contacts);

            SetApplicationBarData();

            _contactDetailsViewModel.IsInSharedItemsRootListView = true;

            InSharesBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            InSharesBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _contactDetailsViewModel.GetContactSharedFolders();
            _contactDetailsViewModel.IsInSharedItemsRootListView = true;            
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
            
            ((ContactDetailsViewModel)this.DataContext).InShares.BrowseToFolder((IMegaNode)e.Item);            
        }        

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["ContactDetailsMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Message.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.SendFile.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = UiResources.ShareFolders.ToLower();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _contactDetailsViewModel.SelectedContact = (Contact)PhoneApplicationService.Current.State["SelectedContact"];
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);
                        
            if (e.Cancel) return;

            if (ContactDetails.SelectedItem.Equals(SharedItems))
            {
                if (!_contactDetailsViewModel.IsInSharedItemsRootListView)
                {
                    _contactDetailsViewModel.GetContactSharedFolders();
                    _contactDetailsViewModel.IsInSharedItemsRootListView = true;
                    e.Cancel = true; return;
                }                    
            }

            NavigateService.NavigateTo(typeof(ContactsPage), NavigationParameter.Normal);

            e.Cancel = true;            
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            if (isCancel) return true;

            if (ContactDetails.SelectedItem.Equals(SharedItems))
            {
                return _contactDetailsViewModel.InShares.GoFolderUp();
            }

            return false;
        }

        private void OnMessageClick(object sender, EventArgs e)
        {

        }

        private void OnSendFilesClick(object sender, EventArgs e)
        {

        }

        private void OnShareFolderClick(object sender, EventArgs e)
        {

        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == SharedItems)
                _contactDetailsViewModel.GetContactSharedFolders();
        }

        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        private void OnInSharedItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            this.LstInSharedFolders.SelectedItem = null;
            _contactDetailsViewModel.IsInSharedItemsRootListView = false;

            _contactDetailsViewModel.InShares.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }

        private void OnInSharedItemStateChanged(object sender, ItemStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ItemState.Recycling:
                    break;
                case ItemState.Recycled:
                    break;
                case ItemState.Realizing:
                    break;
                case ItemState.Realized:
                    ((IMegaNode)e.DataItem).SetThumbnailImage();
                    break;
            }
        }

        private bool CheckTappedItem(RadDataBoundListBoxItem item)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (item == null || item.DataContext == null) return false;
            if (!(item.DataContext is IMegaNode)) return false;
            return true;
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