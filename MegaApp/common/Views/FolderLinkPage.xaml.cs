using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.DataForm;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class FolderLinkPage : MegaPhoneApplicationPage
    {
        private FolderLinkViewModel _folderLinkViewModel;

        public FolderLinkPage()
        {
            _folderLinkViewModel = new FolderLinkViewModel(SdkService.MegaSdkFolderLinks, App.AppInformation, this);
            this.DataContext = _folderLinkViewModel;
            
            InitializeComponent();
            
            FolderLinkBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            FolderLinkBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
            
            _folderLinkViewModel.CommandStatusChanged += (sender, args) =>
            {
                if (ApplicationBar == null) return;
                UiService.ChangeAppBarStatus(ApplicationBar.Buttons,  ApplicationBar.MenuItems, args.Status);
            };
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
                    _folderLinkViewModel.FolderLink.LoadChildNodes();
                }
                else
                {
                    _folderLinkViewModel.FolderLink.ClearChildNodes();
                    _folderLinkViewModel.FolderLink.BreadCrumbs.Clear();
                    _folderLinkViewModel.FolderLink.SetOfflineContentTemplate();
                }

                SetApplicationBarData(isNetworkConnected);
            });
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            CheckAndBrowseToHome((BreadCrumb)sender);
        }

        private void CheckAndBrowseToHome(BreadCrumb breadCrumb)
        {
            ((FolderLinkViewModel)this.DataContext).FolderLink.BrowseToHome();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            CheckAndBrowseToFolder((BreadCrumb)sender, (IMegaNode)e.Item);
        }

        private void CheckAndBrowseToFolder(BreadCrumb breadCrumb, IMegaNode folderNode)
        {
            ((FolderLinkViewModel)this.DataContext).FolderLink.BrowseToFolder(folderNode);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Un-Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged -= 
                new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);

            _folderLinkViewModel.Deinitialize(App.GlobalListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += 
                new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);            

            if (App.AppInformation.IsStartupModeActivate)
            {
                // Needed on every UI interaction
                SdkService.MegaSdkFolderLinks.retryPendingConnections();

                if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                {
                    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                    return;
                }

                App.AppInformation.IsStartupModeActivate = false;

                #if WINDOWS_PHONE_81
                // Check to see if some folder has been picked
                var app = Application.Current as App;
                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs,
                        _folderLinkViewModel.FolderLink);
                }
                #endif                
            }

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            // Initialize the application bar of this page
            SetApplicationBarData();

            if (!App.LinkInformation.HasFetchedNodesFolderLink)
            {
                if (!String.IsNullOrWhiteSpace(App.LinkInformation.ActiveLink))
                {
                    SdkService.MegaSdkFolderLinks.loginToFolder(App.LinkInformation.ActiveLink,
                        new LoginToFolderRequestListener(_folderLinkViewModel));
                }
                else
                {
                    var customMessageDialog = new CustomMessageDialog(
                        AppMessages.AM_OpenLinkFailed_Title, 
                        AppMessages.AM_InvalidLink, 
                        App.AppInformation,
                        MessageDialogButtons.Ok);

                    customMessageDialog.OkOrYesButtonTapped += (sender, args) => CancelAction();

                    customMessageDialog.ShowDialog();
                    return;
                }
            }
            else
            {
                if(e.NavigationMode != NavigationMode.Back)
                {
                    _folderLinkViewModel.LoadFolders();

                    if (App.LinkInformation.LinkAction == LinkAction.Download)
                            _folderLinkViewModel.FolderLink.MultipleDownload(App.LinkInformation.DownloadPath);
                }                
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (FolderLinkMenu.IsOpen)
                e.Cancel = true;

            // Check if multi select is active on current view and disable it if so
            e.Cancel = CheckMultiSelectActive(e.Cancel);

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);

            if(!e.Cancel)
            {
                App.LinkInformation.Reset();

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
                finally
                {
                    e.Cancel = true;
                }
            }
        }

        private bool CheckMultiSelectActive(bool isCancel)
        {
            if (isCancel) return true;

            if (!_folderLinkViewModel.FolderLink.IsMultiSelectActive) return false;

            ChangeMultiSelectMode();

            return true;
        }

        public void SetApplicationBarData(bool isEnabled = true)
        {
            if(_folderLinkViewModel == null)
                _folderLinkViewModel = new FolderLinkViewModel(SdkService.MegaSdkFolderLinks, App.AppInformation, this);

            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources(_folderLinkViewModel.FolderLink.CurrentDisplayMode);

            // Change and translate the current application bar
            _folderLinkViewModel.ChangeMenu(_folderLinkViewModel.FolderLink,
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isEnabled);

            // Button "cancel" should be enabled always
            if(_folderLinkViewModel.FolderLink.CurrentDisplayMode == DriveDisplayMode.FolderLink)
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[2]).IsEnabled = true;
        }

        private void SetAppbarResources(DriveDisplayMode driveDisplayMode)
        {
            switch (driveDisplayMode)
            {
                case DriveDisplayMode.FolderLink:
                    this.ApplicationBar = (ApplicationBar)Resources["FolderLinkMenu"];
                    break;                
                case DriveDisplayMode.MultiSelect:
                    this.ApplicationBar = (ApplicationBar)Resources["MultiSelectMenu"];
                    break;                
                default:
                    throw new ArgumentOutOfRangeException("driveDisplayMode");
            }
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            try
            {
                if (isCancel) return true;

                return _folderLinkViewModel.FolderLink.GoFolderUp();                
            }
            catch (Exception) { return false; }
        }

        private void OnFolderLinkItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            LstCloudDrive.SelectedItem = null;

            _folderLinkViewModel.FolderLink.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }        

        private bool CheckTappedItem(RadDataBoundListBoxItem item)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            if (item == null || item.DataContext == null) return false;
            if (!(item.DataContext is IMegaNode)) return false;
            return true;
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            try
            {
                if (_folderLinkViewModel != null && _folderLinkViewModel.FolderLink != null)
                {
                    var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
                    if (focusedListBoxItem == null || !(focusedListBoxItem.DataContext is IMegaNode))
                    {
                        // We don't want to open the menu if the focused element is not a list box item.
                        // If the list box is empty focusedItem will be null.
                        e.Cancel = true;
                    }
                    else
                    {
                        _folderLinkViewModel.FolderLink.FocusedNode = (IMegaNode)focusedListBoxItem.DataContext;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception) { e.Cancel = true; }
        }        

        private void OnRefreshClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            _folderLinkViewModel.FolderLink.Refresh();
        }

        private void OnDownloadFolderLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            // Extra check to avoid NullReferenceException
            if(_folderLinkViewModel == null || _folderLinkViewModel.FolderLinkRootNode == null)
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.AM_DownloadFailed_Title,
                    AppMessages.AM_DownloadFolderLinkFailed,
                    App.AppInformation,
                    MessageDialogButtons.Ok);

                customMessageDialog.OkOrYesButtonTapped += (new_sender, args) => CancelAction();

                customMessageDialog.ShowDialog();
                return;
            }

            App.LinkInformation.SelectedNodes.Add(_folderLinkViewModel.FolderLinkRootNode);
            App.LinkInformation.LinkAction = LinkAction.Download;

            if (!_folderLinkViewModel.IsUserOnline()) return;

            _folderLinkViewModel.FolderLinkRootNode.Download(TransfersService.MegaTransfers);
        }

        private void OnImportFolderLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            // Extra check to avoid NullReferenceException
            if (_folderLinkViewModel == null || _folderLinkViewModel.FolderLinkRootNode == null)
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.AM_ImportFailed_Title,
                    AppMessages.AM_ImportFolderLinkFailed,
                    App.AppInformation,
                    MessageDialogButtons.Ok);

                customMessageDialog.OkOrYesButtonTapped += (new_sender, args) => CancelAction();

                customMessageDialog.ShowDialog();
                return;
            }

            App.LinkInformation.SelectedNodes.Add(_folderLinkViewModel.FolderLinkRootNode);
            App.LinkInformation.LinkAction = LinkAction.Import;

            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.ImportFolderLink);
        }

        private void OnCancelFolderLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            CancelAction();
        }

        public void CancelAction()
        {
            App.LinkInformation.Reset();

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
                    ((IMegaNode)e.DataItem).SetThumbnailImage();
                    break;
            }
        }

        private void OnScrollStateChanged(object sender, ScrollStateChangedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

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
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            if (!_folderLinkViewModel.FolderLink.HasChildNodes()) return;

            GoToAction(_folderLinkViewModel.FolderLink.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            if (!_folderLinkViewModel.FolderLink.HasChildNodes()) return;

            GoToAction(_folderLinkViewModel.FolderLink.ChildNodes.Last());
        }

        private void GoToAction(IMegaNode bringIntoViewNode)
        {
            LstCloudDrive.BringIntoView(bringIntoViewNode);
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            DialogService.ShowSortDialog(_folderLinkViewModel.FolderLink);
        }

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            LstCloudDrive.IsCheckModeActive = !LstCloudDrive.IsCheckModeActive;
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            ChangeCheckModeAction(e.CheckBoxesVisible, (RadDataBoundListBox)sender, e.TappedItem);

            SetApplicationBarData();
        }

        private void ChangeCheckModeAction(bool onOff, RadDataBoundListBox listBox, object item)
        {
            if (onOff)
            {
                if (item != null && !listBox.CheckedItems.Contains(item))
                {
                    try { listBox.CheckedItems.Add(item); }
                    catch (InvalidOperationException) { /* Item already checked. Do nothing. */ }
                }

                if (_folderLinkViewModel.FolderLink.CurrentDisplayMode != DriveDisplayMode.MultiSelect)
                    _folderLinkViewModel.FolderLink.PreviousDisplayMode = _folderLinkViewModel.FolderLink.CurrentDisplayMode;
                _folderLinkViewModel.FolderLink.CurrentDisplayMode = DriveDisplayMode.MultiSelect;
            }
            else
            {
                listBox.CheckedItems.Clear();
                _folderLinkViewModel.FolderLink.CurrentDisplayMode = _folderLinkViewModel.FolderLink.PreviousDisplayMode;
            }
        }
        
        private void OnMultiSelectDownloadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            App.LinkInformation.SelectedNodes = _folderLinkViewModel.FolderLink.ChildNodes.Where(n => n.IsMultiSelected).ToList();
            App.LinkInformation.LinkAction = LinkAction.Download;

            if (!_folderLinkViewModel.IsUserOnline()) return;
            
            _folderLinkViewModel.FolderLink.MultipleDownload();
        }

        private void OnMultiSelectImportClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdkFolderLinks.retryPendingConnections();

            App.LinkInformation.SelectedNodes = _folderLinkViewModel.FolderLink.ChildNodes.Where(n => n.IsMultiSelected).ToList();
            App.LinkInformation.LinkAction = LinkAction.Import;

            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.ImportFolderLink);
        }

        private void OnSelectAllClick(object sender, EventArgs e)
        {
            _folderLinkViewModel.FolderLink.SelectAll();
        }

        private void OnDeselectAllClick(object sender, EventArgs e)
        {
            _folderLinkViewModel.FolderLink.DeselectAll();
        }
    }
}