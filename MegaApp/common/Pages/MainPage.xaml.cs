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
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.DataForm;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class MainPage : PhoneDrawerLayoutPage
    {
        private readonly MainPageViewModel _mainPageViewModel;

        public MainPage()
        {
            // Set the main viewmodel of this page
            App.MainPageViewModel = _mainPageViewModel = new MainPageViewModel(App.MegaSdk, App.AppInformation, this);
            this.DataContext = _mainPageViewModel;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.CloudDrive);
            
            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            CloudDriveBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            CloudDriveBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
            RubbishBinBreadCrumb.BreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            RubbishBinBreadCrumb.HomeTap += BreadCrumbControlOnOnHomeTap;
            
            _mainPageViewModel.CommandStatusChanged += (sender, args) =>
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
                    NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);                    
                    OnlineBehavior(navParam);
                }
                else
                {
                    _mainPageViewModel.CloudDrive.ClearChildNodes();
                    _mainPageViewModel.CloudDrive.BreadCrumbs.Clear();
                    _mainPageViewModel.CloudDrive.SetOfflineContentTemplate();
                    
                    _mainPageViewModel.RubbishBin.ClearChildNodes();
                    _mainPageViewModel.RubbishBin.BreadCrumbs.Clear();
                    _mainPageViewModel.RubbishBin.SetOfflineContentTemplate();                    
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
            if (breadCrumb.Equals(CloudDriveBreadCrumb))
            {
                ((MainPageViewModel)this.DataContext).CloudDrive.BrowseToHome();
                return;
            }

            if (breadCrumb.Equals(RubbishBinBreadCrumb))
            {
                ((MainPageViewModel)this.DataContext).RubbishBin.BrowseToHome();
            }
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CheckAndBrowseToFolder((BreadCrumb) sender, (IMegaNode) e.Item);
        }

        private void CheckAndBrowseToFolder(BreadCrumb breadCrumb, IMegaNode folderNode)
        {
            if (breadCrumb.Equals(CloudDriveBreadCrumb))
            {
                ((MainPageViewModel)this.DataContext).CloudDrive.BrowseToFolder(folderNode);
                return;
            }

            if (breadCrumb.Equals(RubbishBinBreadCrumb))
            {
                ((MainPageViewModel)this.DataContext).RubbishBin.BrowseToFolder(folderNode);
            }
        }

        private bool CheckActiveAndOnlineSession()
        {
            bool isAlreadyOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());
            if (!isAlreadyOnline)
            {
                if (!SettingsService.HasValidSession())
                {
                    if(App.AppInformation.UriLink == UriLinkType.Confirm)
                    {
                        App.AppInformation.UriLink = UriLinkType.None;
                        if(NavigationContext.QueryString.ContainsKey("confirm"))
                        {
                            string tempUri = HttpUtility.UrlDecode(NavigationContext.QueryString["confirm"]);
                            NavigateService.NavigateTo(typeof(ConfirmAccountPage), NavigationParameter.UriLaunch,
                                new Dictionary<string, string> { { "confirm", HttpUtility.UrlEncode(tempUri) } });
                        }
                    }
                    else if (App.AppInformation.UriLink == UriLinkType.NewSignUp)
                    {
                        App.AppInformation.UriLink = UriLinkType.None;
                        if (NavigationContext.QueryString.ContainsKey("newsignup"))
                        {
                            string tempUri = HttpUtility.UrlDecode(NavigationContext.QueryString["newsignup"]);
                            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.UriLaunch,
                                new Dictionary<string, string> { { "Pivot", "1" }, { "newsignup", HttpUtility.UrlEncode(tempUri) } });
                        }
                    }
                    else if(App.AppInformation.UriLink != UriLinkType.None)
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                    }
                    else
                    {                        
                        NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
                    }
                    
                    return false;
                }
            }

            return true;
        }

        private bool CheckPinLock()
        {
            if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
            {
                NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                return false;
            }

            return true;
        }
        
        private bool CheckSessionAndPinLock()
        {
            if (!CheckActiveAndOnlineSession()) return false;
            if (!CheckPinLock()) return false;
            return true;
        }

        private void CheckNavigationParameters()
        {
            // If the user is trying to open a shortcut
            if (App.ShortCutBase64Handle != null)
            {
                if (!OpenShortCut())
                {
                    new CustomMessageDialog(
                            AppMessages.ShortCutFailed_Title,
                            AppMessages.ShortCutFailed,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();

                    _mainPageViewModel.CloudDrive.BrowseToFolder(
                        NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.MegaSdk.getRootNode(), ContainerType.CloudDrive));
                }
            }

            // If the user is trying to open a MEGA link
            if (App.ActiveImportLink != null)
            {
                App.MegaSdk.getPublicNode(App.ActiveImportLink,
                    new GetPublicNodeRequestListener(_mainPageViewModel.CloudDrive));
            }
        }

        private bool OpenShortCut()
        {
            MNode shortCutMegaNode = App.MegaSdk.getNodeByBase64Handle(App.ShortCutBase64Handle);
            App.ShortCutBase64Handle = null;

            if (shortCutMegaNode != null)
            {
                // Looking for the absolute parent of the shortcut node to see the type
                MNode parentNode;
                MNode absoluteParentNode = shortCutMegaNode;
                while ((parentNode = App.MegaSdk.getParentNode(absoluteParentNode)) != null)
                    absoluteParentNode = parentNode;

                if (absoluteParentNode.getType() == MNodeType.TYPE_ROOT)
                {
                    _mainPageViewModel.CloudDrive.BrowseToFolder(
                        NodeService.CreateNew(App.MegaSdk, App.AppInformation, shortCutMegaNode, ContainerType.CloudDrive));
                }
                else return false;
            }
            else return false;

            return true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Un-Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged -= new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);

            _mainPageViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);

            // Need to check it always and no only in StartupMode, 
            // because this is the first page loaded
            if (!CheckSessionAndPinLock()) return;

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            if(e.NavigationMode == NavigationMode.Reset) return;

            if (e.NavigationMode == NavigationMode.New)
            {
                if (SettingsService.LoadSetting(SettingsResources.CameraUploadsIsEnabled, false))
                {
                    if (!MediaService.GetAutoCameraUploadStatus())
                    {
                        MediaService.SetAutoCameraUpload(true);
                    }
                }
            }

            _mainPageViewModel.Initialize(App.GlobalDriveListener);
            
            App.CloudDrive.ListBox = LstCloudDrive;

            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            
            if (App.AppInformation.IsStartupModeActivate)
            {
                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();                

                App.AppInformation.IsStartupModeActivate = false;

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
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs);
                }
#endif
                if (navParam == NavigationParameter.PasswordLogin || navParam == NavigationParameter.None ||
                        navParam == NavigationParameter.Normal || navParam == NavigationParameter.AutoCameraUpload)
                {
                    if (this.SpecialNavigation()) return;
                    CheckNavigationParameters();                    
                }
            }

            // Initialize the application bar of this page
            SetApplicationBarData();
                        
            if (e.NavigationMode == NavigationMode.Back)
            {
                navParam = NavigationParameter.Browsing;
            }

            // If the previous page is the InitCameraUploadsPage, remove it from the stack.
            if (NavigateService.PreviousPage == typeof(InitCameraUploadsPage))
                NavigationService.RemoveBackEntry();

            OnlineBehavior(navParam);
        }

        private void OnlineBehavior(NavigationParameter navParam)
        {
            bool checkSpecialNavigation = false;

            switch (navParam)
            {
                case NavigationParameter.Login:
                    // Remove the last page from the stack. 
                    // If user presses back button it will then exit the application
                    NavigationService.RemoveBackEntry();
                                        
                    _mainPageViewModel.FetchNodes();
                    break;

                case NavigationParameter.PasswordLogin:
                    NavigationService.RemoveBackEntry();
                    checkSpecialNavigation = Load();
                    break;

                case NavigationParameter.Browsing:
                    // Check if nodes has been fetched. Because when starting app from OS photo setting to go to 
                    // Auto Camera Upload settings fetching has been skipped in the mainpage
                    if (Convert.ToBoolean(App.MegaSdk.isLoggedIn()) && !App.AppInformation.HasFetchedNodes)
                    {
                        _mainPageViewModel.FetchNodes();
                        return;
                    }                        
                    
                    if (this.SpecialNavigation())
                        return;
                    else if (NavigateService.PreviousPage == typeof(NodeDetailsPage))
                        _mainPageViewModel.ActiveFolderView.LoadChildNodes();
                    else
                        _mainPageViewModel.LoadFolders();
                    break;

                case NavigationParameter.PictureSelected:
                    break;
                case NavigationParameter.AlbumSelected:
                    break;
                case NavigationParameter.SelfieSelected:
                    break;
                case NavigationParameter.UriLaunch:
                    checkSpecialNavigation = Load();
                    break;
                case NavigationParameter.BreadCrumb:
                    break;
                case NavigationParameter.Uploads:
                    break;
                case NavigationParameter.Downloads:
                    break;
                case NavigationParameter.DisablePassword:
                    break;
                case NavigationParameter.AutoCameraUpload:
                case NavigationParameter.ImportLinkLaunch:
                case NavigationParameter.Normal:
                case NavigationParameter.None:
                    {
                        if(navParam != NavigationParameter.Normal)
                            if (!CheckPinLock()) return;

                        checkSpecialNavigation = Load();
                        break;
                    }
            }

            if(checkSpecialNavigation)            
                if (this.SpecialNavigation()) return;

            CheckNavigationParameters();
        }

        private bool Load()
        {
            if (CheckActiveAndOnlineSession() && !Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
            {
                App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession),
                    new FastLoginRequestListener(_mainPageViewModel));
                return false;
            }
            // Check if nodes has been fetched. Because when starting app from OS photo setting to go to 
            // Auto Camera Upload settings fetching has been skipped in the mainpage
            else if (!App.AppInformation.HasFetchedNodes)
            {
                _mainPageViewModel.FetchNodes();
                return false;
            }
            
            _mainPageViewModel.LoadFolders();
            return true;
        }
        
        public bool SpecialNavigation()
        {
            // If is a newly activated account, navigates to the upgrade account page
            if (App.AppInformation.IsNewlyActivatedAccount)
            {
                NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, 
                    new Dictionary<string, string> { { "Pivot", "1" } });
                return true;
            }
            // If is the first login, navigates to the camera upload service config page
            else if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
            {
                NavigateService.NavigateTo(typeof(InitCameraUploadsPage), NavigationParameter.Normal);
                return true;
            }                
            else if (App.AppInformation.IsStartedAsAutoUpload)
            {
                // If the previous page is the SettingsPage, no special navigation is needed.
                if (NavigateService.PreviousPage == typeof(SettingsPage))
                {
                    App.AppInformation.IsStartedAsAutoUpload = false;
                    return false;
                }                    

                NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.AutoCameraUpload);
                return true;
            }

            // Manage URI links
            switch(App.AppInformation.UriLink)
            {
                case UriLinkType.NewSignUp:
                case UriLinkType.Confirm:
                    App.AppInformation.UriLink = UriLinkType.None;
                    if (NavigationContext.QueryString.ContainsKey("newsignup") || 
                        NavigationContext.QueryString.ContainsKey("confirm"))
                    {
                        var customMessageDialog = new CustomMessageDialog(
                            AppMessages.AM_AlreadyLoggedInAlert_Title,
                            AppMessages.AM_AlreadyLoggedInAlert,
                            App.AppInformation,
                            MessageDialogButtons.YesNo);

                        customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                        {
                            // First log out of the current account
                            App.MegaSdk.logout(new LogOutRequestListener(false));

                            if (NavigationContext.QueryString.ContainsKey("newsignup"))
                            {
                                // Go to the CreateAccountPage to create a new account
                                string tempUrl = HttpUtility.UrlDecode(NavigationContext.QueryString["newsignup"]);
                                NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.UriLaunch,
                                    new Dictionary<string, string> { { "Pivot", "1" }, { "newsignup", HttpUtility.UrlEncode(tempUrl) } });
                            }

                            if(NavigationContext.QueryString.ContainsKey("confirm"))
                            {
                                // Go to the ConfirmAccountPage to confirm the new account
                                string tempUrl = HttpUtility.UrlDecode(NavigationContext.QueryString["confirm"]);
                                NavigateService.NavigateTo(typeof(ConfirmAccountPage), NavigationParameter.UriLaunch,
                                    new Dictionary<string, string> { { "confirm", HttpUtility.UrlEncode(tempUrl) } });
                            }
                        };

                        customMessageDialog.ShowDialog();
                                                
                        return true;
                    }
                    break;

                case UriLinkType.Backup:
                    App.AppInformation.UriLink = UriLinkType.None;
                    NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.UriLaunch,
                        new Dictionary<string, string> { { "backup", String.Empty } });                        
                    return true;

                case UriLinkType.FmIpc:
                    App.AppInformation.UriLink = UriLinkType.None;
                    NavigateService.NavigateTo(typeof(ContactsPage), NavigationParameter.UriLaunch,
                        new Dictionary<string, string> { { "Pivot", "2" } });
                    return true;
            }

            return false;                
        }        
        
#if WINDOWS_PHONE_81
        private async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args == null || (args.ContinuationData["Operation"] as string) != "SelectedFiles" || 
                args.Files == null || args.Files.Count <= 0)
            {
                ResetFilePicker();
                return;
            }

            if (!App.CloudDrive.IsUserOnline()) return;

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareUploads);

            bool exceptionCatched = false;
            try
            {
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
                        var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
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

                App.CloudDrive.NoFolderUpAction = true;

                if(!exceptionCatched)
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

            if (CloudDriveMenu.IsOpen || RubbishBinMenu.IsOpen)
                e.Cancel = true;

            // Check if multi select is active on current view and disable it if so
            e.Cancel = CheckMultiSelectActive(e.Cancel);

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);

            // If no folder up action, but we are not in the cloud drive section
            // first slide to cloud drive before exiting the application
            e.Cancel = CheckPivotInView(e.Cancel);
        }

        private bool CheckMultiSelectActive(bool isCancel)
        {
            if (isCancel) return true;

            if (!_mainPageViewModel.ActiveFolderView.IsMultiSelectActive) return false;

            ChangeMultiSelectMode();

            return true;
        }

        private void SetApplicationBarData(bool isNetworkConnected = true)
        {
            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources(_mainPageViewModel.ActiveFolderView.CurrentDisplayMode);

            // Change and translate the current application bar
            _mainPageViewModel.ChangeMenu(_mainPageViewModel.ActiveFolderView,
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
        }

        private void SetAppbarResources(DriveDisplayMode driveDisplayMode)
        {
            BorderLinkText.Visibility = Visibility.Collapsed;

            switch (driveDisplayMode)
                    {
                case DriveDisplayMode.CloudDrive:
                    this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                    break;
                case DriveDisplayMode.MoveItem:
                    this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
                    break;
                case DriveDisplayMode.ImportItem:
                    BorderLinkText.Visibility = Visibility.Visible;
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

        public void SetImportMode()
        {
            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.ImportItem;
            SetApplicationBarData();
        }

        public void ChangeGetProAccountBorderVisibility(Visibility visibility)
        {
            GetProAccountCloudDrive.Visibility = visibility;
            GetProAccountRubbishBin.Visibility = visibility;
        }

        public void ChangeWarningOutOfSpaceBorderVisibility(Visibility visibility)
        {
            WarningOutOfSpaceCloudDrive.Visibility = visibility;
            WarningOutOfSpaceRubbishBin.Visibility = visibility;
        }

        private bool CheckAndGoFolderUp(bool isCancel)
        {
            try 
            {
                if (isCancel) return true;

                if (MainPivot.SelectedItem.Equals(CloudDrivePivot) && _mainPageViewModel.CloudDrive != null)
                {
                    return _mainPageViewModel.CloudDrive.GoFolderUp();
                }

                if (MainPivot.SelectedItem.Equals(RubbishBinPivot) && _mainPageViewModel.RubbishBin != null)
                {
                    return _mainPageViewModel.RubbishBin.GoFolderUp();
                }

                return false;
            }
            catch (Exception) { return false; }
        }

        private bool CheckPivotInView(bool isCancel)
        {
            try
            {
                if (isCancel) return true;

                if (MainPivot.SelectedItem.Equals(CloudDrivePivot)) return false;

                MainPivot.SelectedItem = CloudDrivePivot;
                return true;
            }
            catch (Exception) { return false; }
        }

        private void OnCloudDriveItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if(!CheckTappedItem(e.Item)) return;

            LstCloudDrive.SelectedItem = null;

            _mainPageViewModel.CloudDrive.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
        }

        private void OnRubbishBinItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!CheckTappedItem(e.Item)) return;

            LstRubbishBin.SelectedItem = null;

            _mainPageViewModel.RubbishBin.OnChildNodeTapped((IMegaNode)e.Item.DataContext);
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
            if (_mainPageViewModel.ActiveFolderView.CurrentDisplayMode == DriveDisplayMode.MoveItem) 
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
                _mainPageViewModel.ActiveFolderView.FocusedNode = (IMegaNode) focusedListBoxItem.DataContext;                
            }
        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {
            //if (_navParam != NavigationParameter.Browsing && _navParam != NavigationParameter.BreadCrumb)
            //{
            //    // #1870 fix (single column when reactivating app)
            //    App.CloudDrive.SetView(App.CloudDrive.ViewMode);
            //    return;
            //}

            //// Load nodes in the onlistloaded event so that the nodes will display after the back animation and not before
            //App.CloudDrive.LoadNodes();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _mainPageViewModel.ActiveFolderView.Refresh();
        }

        private void OnAddFolderClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
         
            // Only allow add folder on the Cloud Drive section
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                _mainPageViewModel.CloudDrive.AddFolder();
        }

        private void OnOpenLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // Only allow opening of links on the Cloud Drive section
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                _mainPageViewModel.CloudDrive.OpenLink();
        }

        private void OnCloudUploadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // Only allow uploads on the Cloud Drive section
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
                DialogService.ShowUploadOptions(_mainPageViewModel.CloudDrive);
        }

        private void OnCancelMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            CancelMoveAction();

            SetApplicationBarData();
        }

        private void CancelMoveAction()
        {
            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                LstCloudDrive.IsCheckModeActive = false;
                LstCloudDrive.CheckedItems.Clear();
            }
            
            if (MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                LstRubbishBin.IsCheckModeActive = false;
                LstRubbishBin.CheckedItems.Clear();
            }

            _mainPageViewModel.CloudDrive.CurrentDisplayMode = _mainPageViewModel.CloudDrive.PreviousDisplayMode;
            _mainPageViewModel.RubbishBin.CurrentDisplayMode = _mainPageViewModel.RubbishBin.PreviousDisplayMode;

            if(_mainPageViewModel.SourceFolderView != null)
            {
                // Release the focused node
                if (_mainPageViewModel.SourceFolderView.FocusedNode != null)
                {
                    _mainPageViewModel.SourceFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                    _mainPageViewModel.SourceFolderView.FocusedNode = null;
                }

                // Clear and release the selected nodes list
                if (_mainPageViewModel.SourceFolderView.SelectedNodes != null &&
                    _mainPageViewModel.SourceFolderView.SelectedNodes.Count > 0)
                {
                    foreach (var node in _mainPageViewModel.SourceFolderView.SelectedNodes)
                    {
                        node.DisplayMode = NodeDisplayMode.Normal;
                    }
                    _mainPageViewModel.SourceFolderView.SelectedNodes.Clear();
                }

                _mainPageViewModel.SourceFolderView = null;
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
            if(_mainPageViewModel.SourceFolderView != null)
            {
                // Move the focused node and then release it
                if (_mainPageViewModel.SourceFolderView.FocusedNode != null)
                {
                    _mainPageViewModel.SourceFolderView.FocusedNode.Move(_mainPageViewModel.ActiveFolderView.FolderRootNode);
                    _mainPageViewModel.SourceFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
                    _mainPageViewModel.SourceFolderView.FocusedNode = null;
                }

                // Move all the selected nodes and then clear and release the selected nodes list
                if (_mainPageViewModel.SourceFolderView.SelectedNodes != null &&
                    _mainPageViewModel.SourceFolderView.SelectedNodes.Count > 0)
                {
                    foreach (var node in _mainPageViewModel.SourceFolderView.SelectedNodes)
                    {
                        node.Move(_mainPageViewModel.ActiveFolderView.FolderRootNode);
                        node.DisplayMode = NodeDisplayMode.Normal;
                    }
                    _mainPageViewModel.SourceFolderView.SelectedNodes.Clear();
                }

                _mainPageViewModel.SourceFolderView = null;
            }            
                        
            _mainPageViewModel.CloudDrive.CurrentDisplayMode = _mainPageViewModel.CloudDrive.PreviousDisplayMode;
            _mainPageViewModel.RubbishBin.CurrentDisplayMode = _mainPageViewModel.RubbishBin.PreviousDisplayMode;
        }

        private void OnMoveItemTap(object sender, ContextMenuItemSelectedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            this.MoveItemTapAction();
          
            this.SetApplicationBarData();
        }

        private void MoveItemTapAction()
        {
            // Extra null reference exceptions checks
            if (_mainPageViewModel == null || 
                _mainPageViewModel.ActiveFolderView == null ||
                _mainPageViewModel.ActiveFolderView.FocusedNode == null) return;

            _mainPageViewModel.ActiveFolderView.FocusedNode.DisplayMode = NodeDisplayMode.SelectedForMove;
            _mainPageViewModel.ActiveFolderView.SelectedNodes.Add(_mainPageViewModel.ActiveFolderView.FocusedNode);            

            _mainPageViewModel.CloudDrive.PreviousDisplayMode = _mainPageViewModel.CloudDrive.CurrentDisplayMode;
            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.MoveItem;
            _mainPageViewModel.RubbishBin.PreviousDisplayMode = _mainPageViewModel.RubbishBin.CurrentDisplayMode;
            _mainPageViewModel.RubbishBin.CurrentDisplayMode = DriveDisplayMode.MoveItem;            

            _mainPageViewModel.SourceFolderView = _mainPageViewModel.ActiveFolderView;            
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

            if (!_mainPageViewModel.ActiveFolderView.HasChildNodes()) return;
            
            GoToAction(_mainPageViewModel.ActiveFolderView.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!_mainPageViewModel.ActiveFolderView.HasChildNodes()) return;

            GoToAction(_mainPageViewModel.ActiveFolderView.ChildNodes.Last());
        }

        private void GoToAction(IMegaNode bringIntoViewNode)
        {
            if(MainPivot.SelectedItem == CloudDrivePivot)
                LstCloudDrive.BringIntoView(bringIntoViewNode);

            if (MainPivot.SelectedItem == RubbishBinPivot)
                LstRubbishBin.BringIntoView(bringIntoViewNode);
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
           
            DialogService.ShowSortDialog(_mainPageViewModel.ActiveFolderView);
        }

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            if (MainPivot.SelectedItem == CloudDrivePivot)
                LstCloudDrive.IsCheckModeActive = !LstCloudDrive.IsCheckModeActive;

            if (MainPivot.SelectedItem == RubbishBinPivot)
                LstRubbishBin.IsCheckModeActive = !LstRubbishBin.IsCheckModeActive;
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
            // If the user is moving nodes don't allow change to check mode in the Cloud Drive
            if ((MainPivot.SelectedItem == CloudDrivePivot) && 
                (_mainPageViewModel.CloudDrive.CurrentDisplayMode == DriveDisplayMode.MoveItem))
                e.Cancel = true;

            // If the user is moving nodes don't allow change to check mode in the Rubbis Bin
            if ((MainPivot.SelectedItem == RubbishBinPivot) && 
                (_mainPageViewModel.RubbishBin.CurrentDisplayMode == DriveDisplayMode.MoveItem))
                e.Cancel = true;
        }

        private void ChangeCheckModeAction(bool onOff, RadDataBoundListBox listBox, object item)
        {
            if (onOff)
            {
                if(item != null)
                    listBox.CheckedItems.Add(item);

                if (_mainPageViewModel.ActiveFolderView.CurrentDisplayMode != DriveDisplayMode.MultiSelect)
                    _mainPageViewModel.ActiveFolderView.PreviousDisplayMode = _mainPageViewModel.ActiveFolderView.CurrentDisplayMode;
                _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = DriveDisplayMode.MultiSelect;
            }
            else
            {
                listBox.CheckedItems.Clear();
                _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = _mainPageViewModel.ActiveFolderView.PreviousDisplayMode;          
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
            _mainPageViewModel.ActiveFolderView.MultipleDownload();
        }

        private void OnMultiSelectMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MultiSelectMoveAction();
        }

        private void MultiSelectMoveAction()
        {
            // Set the selected nodes list and change the display mode of the current active folder
            if (!_mainPageViewModel.ActiveFolderView.SelectMultipleItemsForMove()) return;

            // Also change the display mode of the Cloud Drive if is necessary
            if(!_mainPageViewModel.ActiveFolderView.Equals(_mainPageViewModel.CloudDrive))
            {
                _mainPageViewModel.CloudDrive.PreviousDisplayMode = _mainPageViewModel.CloudDrive.CurrentDisplayMode;
                _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.MoveItem;
            }

            // Also change the display mode of the Rubbish Bin if is necessary
            if (!_mainPageViewModel.ActiveFolderView.Equals(_mainPageViewModel.RubbishBin))
            {
                _mainPageViewModel.RubbishBin.PreviousDisplayMode = _mainPageViewModel.RubbishBin.CurrentDisplayMode;
                _mainPageViewModel.RubbishBin.CurrentDisplayMode = DriveDisplayMode.MoveItem;
            }            

            // Set the source folder as the current active folder
            _mainPageViewModel.SourceFolderView = _mainPageViewModel.ActiveFolderView;
            
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
            if (! await _mainPageViewModel.ActiveFolderView.MultipleRemoveItems()) return;

            _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = _mainPageViewModel.ActiveFolderView.PreviousDisplayMode;

            SetApplicationBarData();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] == CloudDrivePivot)
                _mainPageViewModel.ActiveFolderView = ((MainPageViewModel) this.DataContext).CloudDrive;            

            if (e.AddedItems[0] == RubbishBinPivot)
                _mainPageViewModel.ActiveFolderView = ((MainPageViewModel)this.DataContext).RubbishBin;

            SetApplicationBarData(NetworkService.IsNetworkAvailable());
        }

        private void OnImportLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if(App.ActiveImportLink != null)
            {
                _mainPageViewModel.ActiveFolderView.ImportLink(App.ActiveImportLink);                
            }
            else
            {
                new CustomMessageDialog(
                    AppMessages.ImportFileFailed_Title,
                    AppMessages.AM_InvalidLink,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }

            App.ActiveImportLink = null;

            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.CloudDrive;
            SetApplicationBarData();
        }

        private void OnCancelImportClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.ActiveImportLink = null;

            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.CloudDrive;
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
        
        private void OnEmptyRubbishBinClick(object sender, EventArgs e)
        {
            _mainPageViewModel.CleanRubbishBin();
        }

        private void OnSelectAllClick(object sender, EventArgs e)
        {
            _mainPageViewModel.ActiveFolderView.SelectAll();
        }

        private void OnDeselectAllClick(object sender, EventArgs e)
        {
            _mainPageViewModel.ActiveFolderView.DeselectAll();
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