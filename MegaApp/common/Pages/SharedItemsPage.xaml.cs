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
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Models;
using MegaApp.UserControls;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class SharedItemsPage : PhoneDrawerLayoutPage
    {
        private readonly SharedItemsViewModel _sharedItemsViewModel;

        public SharedItemsPage()
        {
            _sharedItemsViewModel = new SharedItemsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _sharedItemsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.SharedItems);

            SetApplicationBarData();

            _sharedItemsViewModel.IsInSharedItemsRootListView = true;
            _sharedItemsViewModel.IsOutSharedItemsRootListView = true;

            IncomingSharedBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            IncomingSharedBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
            OutgoingSharedBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            OutgoingSharedBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CheckAndBrowseToHome((BreadCrumb)sender);
        }

        private void CheckAndBrowseToHome(BreadCrumb breadCrumb)
        {
            if (breadCrumb.Equals(IncomingSharedBreadCrumb))
            {
                _sharedItemsViewModel.IsInSharedItemsRootListView = true;
                _sharedItemsViewModel.InShares.BreadCrumbs.Clear();
                _sharedItemsViewModel.GetIncomingSharedFolders();
            }

            if (breadCrumb.Equals(OutgoingSharedBreadCrumb))
            {
                _sharedItemsViewModel.IsOutSharedItemsRootListView = true;
                _sharedItemsViewModel.OutShares.BreadCrumbs.Clear();
                _sharedItemsViewModel.GetOutgoingSharedFolders();
            }
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CheckAndBrowseToFolder((BreadCrumb)sender, (IMegaNode)e.Item);
        }

        private void CheckAndBrowseToFolder(BreadCrumb breadCrumb, IMegaNode folderNode)
        {
            if (breadCrumb.Equals(IncomingSharedBreadCrumb))
            {
                ((SharedItemsViewModel)this.DataContext).InShares.BrowseToFolder(folderNode);
                return;
            }

            if (breadCrumb.Equals(OutgoingSharedBreadCrumb))
            {
                ((SharedItemsViewModel)this.DataContext).OutShares.BrowseToFolder(folderNode);
            }
        }

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["SharedItemsMenu"];

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.Refresh.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.Sort.ToLower();            
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);

            if (e.Cancel) return;

            if (SharedItemsPivot.SelectedItem.Equals(IncomingPivotItem))
            {
                if (!_sharedItemsViewModel.IsInSharedItemsRootListView)
                {
                    _sharedItemsViewModel.IsInSharedItemsRootListView = true;
                    _sharedItemsViewModel.InShares.BreadCrumbs.Clear();
                    _sharedItemsViewModel.GetIncomingSharedFolders();
                    e.Cancel = true; return;
                }
            }
            if (SharedItemsPivot.SelectedItem.Equals(OutgoingPivotItem))
            {
                if (!_sharedItemsViewModel.IsOutSharedItemsRootListView)
                {
                    _sharedItemsViewModel.IsOutSharedItemsRootListView = true;
                    _sharedItemsViewModel.OutShares.BreadCrumbs.Clear();
                    _sharedItemsViewModel.GetOutgoingSharedFolders();
                    e.Cancel = true; return;
                }
            }

            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);

            e.Cancel = true;
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            if (isCancel) return true;

            if (SharedItemsPivot.SelectedItem.Equals(IncomingPivotItem))
            {
                return _sharedItemsViewModel.InShares.GoFolderUp();
            }
            if (SharedItemsPivot.SelectedItem.Equals(OutgoingPivotItem))
            {
                return _sharedItemsViewModel.OutShares.GoFolderUp();
            }

            return false;
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (SharedItemsPivot.SelectedItem == IncomingPivotItem)
            {
                if (_sharedItemsViewModel.IsInSharedItemsRootListView)
                    _sharedItemsViewModel.GetIncomingSharedFolders();
                else
                    _sharedItemsViewModel.InShares.Refresh();
            }
            else if (SharedItemsPivot.SelectedItem == OutgoingPivotItem)
            {
                if (_sharedItemsViewModel.IsOutSharedItemsRootListView)
                    _sharedItemsViewModel.GetOutgoingSharedFolders();
                else
                    _sharedItemsViewModel.OutShares.Refresh();
            }                
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            DialogService.ShowSortDialog(_sharedItemsViewModel.ActiveSharedFolderView);
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] == IncomingPivotItem)
                _sharedItemsViewModel.ActiveSharedFolderView = ((SharedItemsViewModel)this.DataContext).InShares;

            if (e.AddedItems[0] == OutgoingPivotItem)
                _sharedItemsViewModel.ActiveSharedFolderView = ((SharedItemsViewModel)this.DataContext).OutShares;                        
        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == IncomingPivotItem)
                _sharedItemsViewModel.GetIncomingSharedFolders();
            if (sender == OutgoingPivotItem)
                _sharedItemsViewModel.GetOutgoingSharedFolders();
        }
                
        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);            
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        private void OnIncomingSharedItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            this.LstIncomingSharedFolders.SelectedItem = null;
            _sharedItemsViewModel.IsInSharedItemsRootListView = false;

            _sharedItemsViewModel.InShares.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }

        private void OnOutgoingSharedItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            this.LstOutgoingSharedFolders.SelectedItem = null;
            _sharedItemsViewModel.IsOutSharedItemsRootListView = false;

            _sharedItemsViewModel.OutShares.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }

        private void OnSharedItemStateChanged(object sender, ItemStateChangedEventArgs e)
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