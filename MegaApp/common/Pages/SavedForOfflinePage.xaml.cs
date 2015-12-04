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
using MegaApp.Interfaces;
using MegaApp.Models;
using MegaApp.Services;
using MegaApp.UserControls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class SavedForOfflinePage : PhoneDrawerLayoutPage
    {
        private readonly SavedForOfflineViewModel _savedForOfflineViewModel;

        public SavedForOfflinePage()
        {
            this.DataContext = _savedForOfflineViewModel = new SavedForOfflineViewModel();

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.SavedForOffline);

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            SavedForOfflineBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            SavedForOfflineBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ((SavedForOfflineViewModel)this.DataContext).SavedForOffline.BrowseToHome();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ((SavedForOfflineViewModel)this.DataContext).SavedForOffline.BrowseToFolder((IOfflineNode)e.Item);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _savedForOfflineViewModel.LoadFolders();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Check if multi select is active on current view and disable it if so
            e.Cancel = CheckMultiSelectActive(e.Cancel);

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);            
        }

        private bool CheckMultiSelectActive(bool isCancel)
        {
            if (isCancel) return true;

            if (!_savedForOfflineViewModel.SavedForOffline.IsMultiSelectActive) return false;            

            ChangeMultiSelectMode();

            return true;
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            if (isCancel) return true;

            return _savedForOfflineViewModel.SavedForOffline.GoFolderUp();
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            LstSavedForOffline.SelectedItem = null;

            _savedForOfflineViewModel.SavedForOffline.OnChildNodeTapped((IOfflineNode)e.Item.DataContext);
        }

        private bool CheckTappedItem(RadDataBoundListBoxItem item)
        {
            if (item == null || item.DataContext == null) return false;
            if (!(item.DataContext is IOfflineNode)) return false;
            return true;
        }

        private void OnGoToTopTap(object sender, GestureEventArgs e)
        {
            if (!_savedForOfflineViewModel.SavedForOffline.HasChildNodes()) return;

            GoToAction(_savedForOfflineViewModel.SavedForOffline.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, GestureEventArgs e)
        {
            if (!_savedForOfflineViewModel.SavedForOffline.HasChildNodes()) return;            

            GoToAction(_savedForOfflineViewModel.SavedForOffline.ChildNodes.Last());
        }

        private void GoToAction(IOfflineNode bringIntoViewNode)
        {
            LstSavedForOffline.BringIntoView(bringIntoViewNode);
        }

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            LstSavedForOffline.IsCheckModeActive = !LstSavedForOffline.IsCheckModeActive;
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