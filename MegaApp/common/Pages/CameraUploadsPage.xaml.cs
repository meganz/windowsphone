using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
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
    public partial class CameraUploadsPage : PhoneDrawerLayoutPage
    {
        private readonly CameraUploadsPageViewModel _cameraUploadsPageViewModel;

        public CameraUploadsPage()
        {
            // Set the main viewmodel of this page
            _cameraUploadsPageViewModel = new CameraUploadsPageViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _cameraUploadsPageViewModel;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.CameraUploads);
            
            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            CameraUploadsBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            CameraUploadsBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);

            BtnCopyOrMoveItem.Content = String.Format("{0}/{1}", 
                UiResources.Copy.ToLower(), UiResources.Move.ToLower());
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

                    _cameraUploadsPageViewModel.CameraUploads.SetEmptyContentTemplate(true);
                    _cameraUploadsPageViewModel.LoadFolders();
                }
                else
                {
                    _cameraUploadsPageViewModel.CameraUploads.ClearChildNodes();
                    _cameraUploadsPageViewModel.CameraUploads.BreadCrumbs.Clear();
                    _cameraUploadsPageViewModel.CameraUploads.SetOfflineContentTemplate();
                }

                SetApplicationBarData(isNetworkConnected);
            });
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CheckAndBrowseToHome((BreadCrumb)sender);
        }

        private void CheckAndBrowseToHome(BreadCrumb breadCrumb)
        {
            ((CameraUploadsPageViewModel)this.DataContext).CameraUploads.BrowseToHome();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CheckAndBrowseToFolder((BreadCrumb) sender, (IMegaNode) e.Item);
        }

        private void CheckAndBrowseToFolder(BreadCrumb breadCrumb, IMegaNode folderNode)
        {
            ((CameraUploadsPageViewModel)this.DataContext).CameraUploads.BrowseToFolder(folderNode);
        }
      
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _cameraUploadsPageViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            if(e.NavigationMode == NavigationMode.Reset) return;

            _cameraUploadsPageViewModel.Initialize(App.GlobalDriveListener);

            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            
            if(PhoneApplicationService.Current.StartupMode == StartupMode.Activate)
            {
                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();

#if WINDOWS_PHONE_81
                // Check to see if any files have been picked
                var app = Application.Current as App;
                if (app != null && app.FilePickerContinuationArgs != null)
                {
                    this.ContinueFileOpenPicker(app.FilePickerContinuationArgs);
                    return;
                }

                if (app != null && app.FolderPickerContinuationArgs != null)
                {
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs,
                        _cameraUploadsPageViewModel.CameraUploads);
                }
#endif
            }

            // Initialize the application bar of this page
            SetApplicationBarData();
                        
            if (e.NavigationMode == NavigationMode.Back)
            {
                navParam = NavigationParameter.Browsing;
            }


            switch (navParam)
            {
                case NavigationParameter.Normal:
                    // Check if nodes has been fetched. Because when starting app from OS photo setting to go to 
                    // Auto Camera Upload settings fetching has been skipped in the mainpage
                    if (Convert.ToBoolean(App.MegaSdk.isLoggedIn()) && !App.AppInformation.HasFetchedNodes)
                        _cameraUploadsPageViewModel.FetchNodes();
                    else
                        _cameraUploadsPageViewModel.LoadFolders();
                    break;

                case NavigationParameter.Login: 
                    break;
                case NavigationParameter.PasswordLogin:
                    break;
                case NavigationParameter.PictureSelected:
                    break;
                case NavigationParameter.AlbumSelected:
                    break;
                case NavigationParameter.SelfieSelected:
                    break;
                case NavigationParameter.UriLaunch:
                    break;
                case NavigationParameter.Browsing:
                    // Check if nodes has been fetched. Because when starting app from OS photo setting to go to 
                    // Auto Camera Upload settings fetching has been skipped in the mainpage
                    if (Convert.ToBoolean(App.MegaSdk.isLoggedIn()) && !App.AppInformation.HasFetchedNodes)
                        _cameraUploadsPageViewModel.FetchNodes();
                    break;
                case NavigationParameter.BreadCrumb:
                    break;
                case NavigationParameter.Uploads:
                    break;
                case NavigationParameter.Downloads:
                    break;
                case NavigationParameter.DisablePassword:
                    break;
                case NavigationParameter.FileLinkLaunch:
                case NavigationParameter.FolderLinkLaunch:
                case NavigationParameter.None:
                {
                    if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                    {
                        NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                        return;
                    }
                    break;
                }
            }
        }
        
#if WINDOWS_PHONE_81
        private async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            bool exceptionCatched = false;
            try
            {
                if (args == null || (args.ContinuationData["Operation"] as string) != "SelectedFiles" ||
                args.Files == null || args.Files.Count <= 0)
                {
                    ResetFilePicker();
                    return;
                }

                if (!App.CloudDrive.IsUserOnline()) return;

                ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareUploads);

                // Set upload directory only once for speed improvement and if not exists, create dir
                var uploadDir = AppService.GetUploadDirectoryPath(true);

                // Get picked files only once for speed improvement and to try avoid ArgumentException in the loop
                IReadOnlyList<StorageFile> pickedFiles = args.Files;
                foreach (StorageFile file in pickedFiles)
                {
                    if (file == null) continue; // To avoid null references

                    try
                    {
                        string newFilePath = Path.Combine(uploadDir, file.Name);
                        using (var fs = new FileStream(newFilePath, FileMode.Create))
                        {
                            var stream = await file.OpenStreamForReadAsync();
                            await stream.CopyToAsync(fs);
                            await fs.FlushAsync();
                            fs.Close();
                        }
                        var uploadTransfer = new TransferObjectModel(
                            App.MegaSdk, _cameraUploadsPageViewModel.CameraUploads.FolderRootNode, TransferType.Upload, newFilePath);
                        App.MegaTransfers.Add(uploadTransfer);
                        uploadTransfer.StartTransfer();
                    }
                    catch (Exception)
                    {
                        new CustomMessageDialog(
                            AppMessages.PrepareFileForUploadFailed_Title,
                            String.Format(AppMessages.PrepareFileForUploadFailed, file.Name),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();

                        exceptionCatched = true;
                    }
                }
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                    AppMessages.AM_PrepareFilesForUploadFailed_Title,
                    String.Format(AppMessages.AM_PrepareFilesForUploadFailed),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
                
                exceptionCatched = true;
            }
            finally
            {
                ResetFilePicker();

                ProgressService.SetProgressIndicator(false);

                if (!exceptionCatched)
                    NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
            }
        }

        private static void ResetFilePicker()
        {
            // Reset the picker data
            var app = Application.Current as App;
            if (app != null) app.FilePickerContinuationArgs = null;
        }
#endif

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Check if multi select is active on current view and disable it if so
            e.Cancel = CheckMultiSelectActive(e.Cancel);

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);

            // Check if can go back in the stack of pages
            e.Cancel = CheckGoBack(e.Cancel);
        }

        private bool CheckMultiSelectActive(bool isCancel)
        {
            if (isCancel) return true;

            if (!_cameraUploadsPageViewModel.CameraUploads.IsMultiSelectActive) return false;

            ChangeMultiSelectMode();

            return true;
        }

        private void SetApplicationBarData(bool isNetworkConnected = true)
        {
            // Set the Applicatio Bar to one of the available menu resources in this page
            SetAppbarResources(_cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode);

            // Change and translate the current application bar
            _cameraUploadsPageViewModel.ChangeMenu(_cameraUploadsPageViewModel.CameraUploads,
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
        }

        private void SetAppbarResources(DriveDisplayMode driveDisplayMode)
        {
            switch (driveDisplayMode)
                    {
                case DriveDisplayMode.CloudDrive:
                    this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                    break;
                case DriveDisplayMode.CopyOrMoveItem:
                    this.ApplicationBar = (ApplicationBar)Resources["CopyOrMoveItemMenu"];
                    break;
                case DriveDisplayMode.ImportItem:
                    this.ApplicationBar = (ApplicationBar)Resources["ImportItemMenu"];
                    break;
                case DriveDisplayMode.MultiSelect:
                    this.ApplicationBar = (ApplicationBar)Resources["MultiSelectMenu"];
                    break;
                case DriveDisplayMode.RubbishBin:
                    this.ApplicationBar = (ApplicationBar)Resources["RubbishBinMenu"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("driveDisplayMode");
            }
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            if (isCancel) return true;

            return _cameraUploadsPageViewModel.CameraUploads.GoFolderUp();
        }
        
        private void OnCameraUploadsItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if(!CheckTappedItem(e.Item)) return;

            LstCloudDrive.SelectedItem = null;

            _cameraUploadsPageViewModel.CameraUploads.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }

        private bool CheckTappedItem(RadDataBoundListBoxItem item)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (item == null || item.DataContext == null) return false;
            if (!(item.DataContext is IMegaNode)) return false;
            return true;
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // If the user is moving nodes, don't show the contextual menu
            if (_cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem)
                e.Cancel = true;

            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || !(focusedListBoxItem.DataContext is IMegaNode))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                _cameraUploadsPageViewModel.CameraUploads.FocusedNode = (IMegaNode) focusedListBoxItem.DataContext;
            }
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _cameraUploadsPageViewModel.CameraUploads.Refresh();
        }

        private void OnAddFolderClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
         
            _cameraUploadsPageViewModel.CameraUploads.AddFolder();
        }

        private void OnOpenLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

           _cameraUploadsPageViewModel.CameraUploads.OpenLink();
        }

        private void OnCloudUploadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            DialogService.ShowUploadOptions(_cameraUploadsPageViewModel.CameraUploads);
        }

        private void OnCancelCopyOrMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CancelCopyOrMoveAction();

            SetApplicationBarData();
        }

        private void CancelCopyOrMoveAction()
        {
            LstCloudDrive.IsCheckModeActive = false;
            LstCloudDrive.CheckedItems.Clear();

            _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode;

            // Release the focused node
            if (_cameraUploadsPageViewModel.CameraUploads.FocusedNode != null)
            {
                _cameraUploadsPageViewModel.CameraUploads.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                _cameraUploadsPageViewModel.CameraUploads.FocusedNode = null;
            }

            // Clear and release the selected nodes list
            if (_cameraUploadsPageViewModel.CameraUploads.SelectedNodes != null &&
                _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Count > 0)
            {
                foreach (var node in _cameraUploadsPageViewModel.CameraUploads.SelectedNodes)
                {
                    node.DisplayMode = NodeDisplayMode.Normal;
                }
                _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Clear();
            }
        }

        private void OnAcceptCopyClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            AcceptCopyAction();

            SetApplicationBarData();
        }

        private void AcceptCopyAction()
        {
            if (_cameraUploadsPageViewModel.CameraUploads != null)
            {
                try
                {
                    // Copy all the selected nodes and then clear and release the selected nodes list
                    if (_cameraUploadsPageViewModel.CameraUploads.SelectedNodes != null &&
                        _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Count > 0)
                    {
                        foreach (var node in _cameraUploadsPageViewModel.CameraUploads.SelectedNodes)
                        {
                            node.Copy(_cameraUploadsPageViewModel.CameraUploads.FolderRootNode);
                            node.DisplayMode = NodeDisplayMode.Normal;
                        }
                        _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Clear();
                    }

                    // Release the focused node
                    if (_cameraUploadsPageViewModel.CameraUploads.FocusedNode != null)
                    {
                        _cameraUploadsPageViewModel.CameraUploads.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                        _cameraUploadsPageViewModel.CameraUploads.FocusedNode = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    new CustomMessageDialog(AppMessages.AM_CopyFailed_Title, AppMessages.AM_CopyFailed,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                }
                finally
                {
                    _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode;
                }
            }            
        }

        private void OnAcceptMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            AcceptMoveAction();
            
            SetApplicationBarData();
        }

        private void AcceptMoveAction()
        {
            if(_cameraUploadsPageViewModel.CameraUploads != null)
            {
                try
                {
                    // Move all the selected nodes and then clear and release the selected nodes list
                    if (_cameraUploadsPageViewModel.CameraUploads.SelectedNodes != null &&
                        _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Count > 0)
                    {
                        foreach (var node in _cameraUploadsPageViewModel.CameraUploads.SelectedNodes)
                        {
                            node.Move(_cameraUploadsPageViewModel.CameraUploads.FolderRootNode);
                            node.DisplayMode = NodeDisplayMode.Normal;
                        }
                        _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Clear();
                    }

                    // Release the focused node
                    if (_cameraUploadsPageViewModel.CameraUploads.FocusedNode != null)
                    {
                        _cameraUploadsPageViewModel.CameraUploads.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                        _cameraUploadsPageViewModel.CameraUploads.FocusedNode = null;
                    }
                }
                catch(InvalidOperationException)
                {
                    new CustomMessageDialog(AppMessages.MoveFailed_Title, AppMessages.MoveFailed,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                }
                finally
                {
                    _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode;
                }
            }
        }

        private void OnCopyOrMoveItemTap(object sender, ContextMenuItemSelectedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            this.CopyOrMoveItemTapAction();
          
            this.SetApplicationBarData();
        }

        private void CopyOrMoveItemTapAction()
        {
            // Extra null reference exceptions checks
            if (_cameraUploadsPageViewModel == null ||
                _cameraUploadsPageViewModel.CameraUploads == null ||
                _cameraUploadsPageViewModel.CameraUploads.FocusedNode == null) return;

            _cameraUploadsPageViewModel.CameraUploads.SelectedNodes.Add(_cameraUploadsPageViewModel.CameraUploads.FocusedNode);
            _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode = _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode;
            _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = DriveDisplayMode.CopyOrMoveItem;
            _cameraUploadsPageViewModel.CameraUploads.FocusedNode.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
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
            App.MegaSdk.retryPendingConnections();

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
            App.MegaSdk.retryPendingConnections();

            if (!_cameraUploadsPageViewModel.CameraUploads.HasChildNodes()) return;

            GoToAction(_cameraUploadsPageViewModel.CameraUploads.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!_cameraUploadsPageViewModel.CameraUploads.HasChildNodes()) return;

            GoToAction(_cameraUploadsPageViewModel.CameraUploads.ChildNodes.Last());
        }

        private void GoToAction(IMegaNode bringIntoViewNode)
        {
            LstCloudDrive.BringIntoView(bringIntoViewNode);
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            DialogService.ShowSortDialog(_cameraUploadsPageViewModel.CameraUploads);
        }

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            LstCloudDrive.IsCheckModeActive = !LstCloudDrive.IsCheckModeActive;
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ChangeCheckModeAction(e.CheckBoxesVisible, (RadDataBoundListBox) sender, e.TappedItem);

            SetApplicationBarData();
        }

        private void OnCheckModeChanging(object sender, IsCheckModeActiveChangingEventArgs e)
        {
            if(_cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem)
                e.Cancel = true;
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

                if (_cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode != DriveDisplayMode.MultiSelect)
                    _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode = _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode;
                _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = DriveDisplayMode.MultiSelect;
            }
            else
            {
                listBox.CheckedItems.Clear();
                _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode;          
            }
        }
        
        private void OnMultiSelectDownloadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
           
            MultipleDownloadAction();
        }

        private void MultipleDownloadAction()
        {
            _cameraUploadsPageViewModel.CameraUploads.MultipleDownload();
        }

        private void OnMultiSelectCopyOrMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MultiSelectCopyOrMoveAction();
        }

        private void MultiSelectCopyOrMoveAction()
        {
            if (!_cameraUploadsPageViewModel.CameraUploads.SelectMultipleItemsForMove()) return;

            SetApplicationBarData();
        }
        
        private void OnMultiSelectRemoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MultiSelectRemoveAction();
        }

        private async void MultiSelectRemoveAction()
        {
            if (!await _cameraUploadsPageViewModel.CameraUploads.MultipleRemoveItems()) return;

            _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = _cameraUploadsPageViewModel.CameraUploads.PreviousDisplayMode;

            SetApplicationBarData();
        }

        private void OnImportLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            //App.MegaSdk.getPublicNode(
            //    _mainPageViewModel.ActiveImportLink, new GetPublicNodeRequestListener(_cameraUploadsPageViewModel.CameraUploads));

            _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = DriveDisplayMode.CloudDrive;

            SetApplicationBarData();
        }

        private void OnCancelImportClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            //_mainPageViewModel.ActiveImportLink = null;
            _cameraUploadsPageViewModel.CameraUploads.CurrentDisplayMode = DriveDisplayMode.CloudDrive;

            SetApplicationBarData();
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

        private void OnSelectAllClick(object sender, EventArgs e)
        {
            _cameraUploadsPageViewModel.CameraUploads.SelectAll();
        }

        private void OnDeselectAllClick(object sender, EventArgs e)
        {
            _cameraUploadsPageViewModel.CameraUploads.DeselectAll();
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