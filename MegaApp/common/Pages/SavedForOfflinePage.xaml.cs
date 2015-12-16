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

            SetApplicationBarData();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            SavedForOfflineBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            SavedForOfflineBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void SetApplicationBarData()
        {
            // Set the Applicatio Bar to one of the available menu resources in this page
            SetAppbarResources(_savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode);

            // Change and translate the current application bar
            _savedForOfflineViewModel.ChangeMenu(_savedForOfflineViewModel.SavedForOffline,
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);
        }

        private void SetAppbarResources(DriveDisplayMode driveDisplayMode)
        {            
            switch (driveDisplayMode)
            {
                case DriveDisplayMode.SavedForOffline:
                    this.ApplicationBar = (ApplicationBar)Resources["SavedForOfflineMenu"];
                    break;                
                case DriveDisplayMode.MultiSelect:
                    this.ApplicationBar = (ApplicationBar)Resources["MultiSelectMenu"];
                    break;                
                default:
                    throw new ArgumentOutOfRangeException("driveDisplayMode");
            }
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            ((SavedForOfflineViewModel)this.DataContext).SavedForOffline.BrowseToHome();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            ((SavedForOfflineViewModel)this.DataContext).SavedForOffline.BrowseToFolder((IOfflineNode)e.Item);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _savedForOfflineViewModel.LoadFolders();
                        
            SetApplicationBarData();
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

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || !(focusedListBoxItem.DataContext is IOfflineNode))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                _savedForOfflineViewModel.SavedForOffline.FocusedNode = (IOfflineNode)focusedListBoxItem.DataContext;
            }
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            _savedForOfflineViewModel.SavedForOffline.Refresh();
        }

        private void OnItemStateChanged(object sender, ItemStateChangedEventArgs e)
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
                    ((IOfflineNode)e.DataItem).SetThumbnailImage();
                    break;
            }
        }

        private void OnScrollStateChanged(object sender, ScrollStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ScrollState.NotScrolling:
                    //foreach (var frameworkElement in LstCloudDrive.ViewportItems)
                    //{
                    //    ((NodeViewModel)frameworkElement.DataContext).SetThumbnailImage();
                    //}
                    break;
                case ScrollState.Scrolling:
                    break;
                case ScrollState.Flicking:
                    break;
                case ScrollState.TopStretch:
                    break;
                case ScrollState.LeftStretch:
                    break;
                case ScrollState.RightStretch:
                    break;
                case ScrollState.BottomStretch:
                    break;
                case ScrollState.ForceStopTopBottomScroll:
                    break;
                case ScrollState.ForceStopBottomTopScroll:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private void OnSortClick(object sender, EventArgs e)
        {
            DialogService.ShowSortDialog(_savedForOfflineViewModel.SavedForOffline);
        }

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            LstSavedForOffline.IsCheckModeActive = !LstSavedForOffline.IsCheckModeActive;
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            ChangeCheckModeAction(e.CheckBoxesVisible, (RadDataBoundListBox)sender, e.TappedItem);

            Dispatcher.BeginInvoke(SetApplicationBarData);
        }

        private void ChangeCheckModeAction(bool onOff, RadDataBoundListBox listBox, object item)
        {
            if (onOff)
            {
                if (item != null)
                    listBox.CheckedItems.Add(item);

                if (_savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode != DriveDisplayMode.MultiSelect)
                    _savedForOfflineViewModel.SavedForOffline.PreviousDisplayMode = _savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode;
                _savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode = DriveDisplayMode.MultiSelect;
            }
            else
            {
                listBox.CheckedItems.Clear();
                _savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode = _savedForOfflineViewModel.SavedForOffline.PreviousDisplayMode;
            }
        }

        private void OnMultiSelectRemoveClick(object sender, EventArgs e)
        {
            MultiSelectRemoveAction();
        }

        private async void MultiSelectRemoveAction()
        {
            if (!await _savedForOfflineViewModel.SavedForOffline.MultipleRemove()) return;

            _savedForOfflineViewModel.SavedForOffline.CurrentDisplayMode = _savedForOfflineViewModel.SavedForOffline.PreviousDisplayMode;

            SetApplicationBarData();
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