using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
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
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
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
        
        private bool ValidActiveAndOnlineSession()
        {
            if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
                return false;

            if (SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                return false;

            bool isAlreadyOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());
            if (!isAlreadyOnline)
            {
                try
                {
                    if (SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession) == null)                        
                        return false;
                }
                catch (ArgumentNullException) { return false; }
            }

            return true;
        }

        private bool OpenShortCut()
        {
            MNode shortCutMegaNode = App.MegaSdk.getNodeByHandle(_mainPageViewModel.ShortCutHandle.Value);
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
                        NodeService.CreateNew(App.MegaSdk, App.AppInformation, shortCutMegaNode));
                }
                else return false;
            }
            else return false;

            return true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _mainPageViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Reset) return;

            _mainPageViewModel.Initialize(App.GlobalDriveListener);
            
            App.CloudDrive.ListBox = LstCloudDrive;

            NavigationParameter navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            if (NavigationContext.QueryString.ContainsKey("ShortCutHandle"))
            {
                _mainPageViewModel.ShortCutHandle = Convert.ToUInt64(NavigationContext.QueryString["ShortCutHandle"]);
            }
            
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
                    FolderService.ContinueFolderOpenPicker(app.FolderPickerContinuationArgs);
                }
#endif

                if (ValidActiveAndOnlineSession() && navParam == NavigationParameter.None)
                {
                    // If the user is trying to open a shortcut
                    if (_mainPageViewModel.ShortCutHandle.HasValue)
                    {
                        if (!OpenShortCut())
                        {
                            new CustomMessageDialog(
                                    AppMessages.ShortCutFailed_Title,
                                    AppMessages.ShortCutFailed,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            
                            _mainPageViewModel.CloudDrive.BrowseToFolder(
                                NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.MegaSdk.getRootNode()));
                        }
                    }                        

                    return;
                }                    
            }

            // Initialize the application bar of this page
            SetApplicationBarData();
                        
            if (e.NavigationMode == NavigationMode.Back)
            {
                navParam = NavigationParameter.Browsing;

                if (NavigateService.PreviousPage == typeof(MyAccountPage))
                    navParam = NavigationParameter.Browsing;
            }


            switch (navParam)
            {
                case NavigationParameter.Normal:
                    _mainPageViewModel.LoadFolders();
                    break;

                case NavigationParameter.Login:                    
                    // Get last page (previous page)            
                    var backStack = ((PhoneApplicationFrame)Application.Current.RootVisual).BackStack;
                    var lastPage = backStack.FirstOrDefault();
                    if (lastPage != null)
                    {
                        String strLastPage = lastPage.Source.ToString();

                        // If navigation is from the ConfirmAccountPage, active the flag indicating that is newly activated account
                        if (lastPage.Source.ToString().Contains("confirm"))
                        {
                            App.IsNewlyActivatedAccount = true;                            
                            NavigationService.Navigate(new Uri("/Pages/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));                            
                        }                            
                    }

                    // Remove the login or confirm account page from the stack. 
                    // If user presses back button it will then exit the application
                    NavigationService.RemoveBackEntry();
                    _mainPageViewModel.GetAccountDetails();
                    _mainPageViewModel.FetchNodes();
                    break;

                case NavigationParameter.PasswordLogin:
                    NavigationService.RemoveBackEntry();
                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession),
                        new FastLoginRequestListener(_mainPageViewModel));
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
                    break;
                case NavigationParameter.BreadCrumb:
                    break;
                case NavigationParameter.Uploads:
                    break;
                case NavigationParameter.Downloads:
                    break;
                case NavigationParameter.DisablePassword:
                    break;
                case NavigationParameter.ImportLinkLaunch:
                case NavigationParameter.None:
                {
                    if (e.NavigationMode != NavigationMode.Back)
                    {
                        if (NavigationContext.QueryString.ContainsKey("filelink"))
                        {
                            _mainPageViewModel.LoadFolders();
                            _mainPageViewModel.ActiveImportLink = NavigationContext.QueryString["filelink"];
                            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.ImportItem;
                            //_mainPageViewModel.ChangeMenu(_mainPageViewModel.ActiveFolderView,
                            //    this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);
                            //SetApplicationBarData();
                        }
                    }

                    if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
                    {
                        //NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                        NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
                        return;
                    }
                    
                    if (SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                    {
                        NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal);
                        return;
                    }

                    bool isAlreadyOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());
                    if (!isAlreadyOnline)
                    {
                        try
                        {
                            if (SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession) != null)
                                App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession),
                                    new FastLoginRequestListener(_mainPageViewModel));
                            else
                            {
                                //NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                                NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
                                return;
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            //NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                            NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.Normal);
                            return;
                        }
                    }
                    
                    break;
                }
                
            }

            //_navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            //if (NavigationContext.QueryString.ContainsKey("ShortCutHandle"))
            //{
            //    App.CloudDrive.ShortCutHandle = Convert.ToUInt64(NavigationContext.QueryString["ShortCutHandle"]);
            //}

            

            //if (e.NavigationMode == NavigationMode.Reset)
            //{
            //    return;
            //}

            //if (e.NavigationMode == NavigationMode.Back)
            //{
            //    if (!App.CloudDrive.NoFolderUpAction)
            //    {
            //        App.CloudDrive.GoFolderUp();                    
            //        _navParam = NavigationParameter.Browsing;
            //    }
            //    else
            //        _navParam = NavigationParameter.Normal;

            //    if(NavigateService.PreviousPage == typeof(MyAccountPage))
            //        _navParam = NavigationParameter.Browsing;
            //}

            //App.CloudDrive.NoFolderUpAction = false;

            //switch (_navParam)
            //{
            //    case NavigationParameter.Login:
            //    {
            //        // Remove the login page from the stack. If user presses back button it will then exit the application
            //        NavigationService.RemoveBackEntry();
                    
            //        App.CloudDrive.FetchNodes();
            //        break;
            //    }
            //    case NavigationParameter.BreadCrumb:
            //    {
            //        int breadCrumbs = App.CloudDrive.CountBreadCrumbs();
            //        for (int x = 0; x <= breadCrumbs; x++)
            //            NavigationService.RemoveBackEntry();
                   
            //        break;
            //    }
            //    case NavigationParameter.PasswordLogin:
            //    {
            //        NavigationService.RemoveBackEntry();
            //        App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
            //        break;
            //    }
            //    case NavigationParameter.ImportLinkLaunch:
            //    case NavigationParameter.Unknown:
            //    {
            //        if (e.NavigationMode != NavigationMode.Back)
            //        {
            //            if (NavigationContext.QueryString.ContainsKey("filelink"))
            //            {
            //                App.CloudDrive.LinkToImport = NavigationContext.QueryString["filelink"];
            //                App.CloudDrive.CurrentDisplayMode = CurrentDisplayMode.ImportItem;
            //                ChangeMenu();
            //            }
            //        }

            //        if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
            //        {
            //            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
            //            return;
            //        }
                    
            //        if (SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
            //        {
            //            NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal);
            //            return;
            //        }

            //        bool isAlreadyOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());

            //        if (!isAlreadyOnline)
            //        {
            //            try
            //            {
            //                if (SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession) != null)
            //                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
            //                else
            //                {
            //                    NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
            //                    return;
            //                }
            //            }
            //            catch (ArgumentNullException)
            //            {
            //                NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
            //                return;
            //            }
                            
            //        }
                    
            //        break;
            //    }
            //}
            
            //base.OnNavigatedTo(e);
            //App.AppEvent = ApplicationEvent.None;
        }
        
#if WINDOWS_PHONE_81
        private async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if ((args.ContinuationData["Operation"] as string) != "SelectedFiles" || args.Files == null ||
                args.Files.Count <= 0)
            {
                ResetFilePicker();
                return;
            }

            if (!App.CloudDrive.IsUserOnline()) return;

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareUploads);

            // Set upload directory only once for speed improvement and if not exists, create dir
            var uploadDir = AppService.GetUploadDirectoryPath(true);

            foreach (var file in args.Files)
            {
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
                }
            }
            ResetFilePicker();

            ProgressService.SetProgressIndicator(false);

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
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

            // Check if we can go a folder up in the selected pivot view
            e.Cancel = CheckAndGoFolderUp(e.Cancel);

            // If no folder up action, but we are not in the cloud drive section
            // first slide to cloud drive before exiting the application
            e.Cancel = CheckPivotInView(e.Cancel);
        }

        private void SetApplicationBarData()
        {
            // Set the Applicatio Bar to one of the available menu resources in this page
            SetAppbarResources(_mainPageViewModel.ActiveFolderView.CurrentDisplayMode);

            // Change and translate the current application bar
            _mainPageViewModel.ChangeMenu(_mainPageViewModel.ActiveFolderView,
                this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);
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
            if (isCancel) return true;

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot))
            {
                return _mainPageViewModel.CloudDrive.GoFolderUp();
            }

            if(MainPivot.SelectedItem.Equals(RubbishBinPivot))
            {
                return _mainPageViewModel.RubbishBin.GoFolderUp();
            }

            return false;
        }

        private bool CheckPivotInView(bool isCancel)
        {
            if (isCancel) return true;

            if (MainPivot.SelectedItem.Equals(CloudDrivePivot)) return false;
            
            MainPivot.SelectedItem = CloudDrivePivot;
            return true;
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
                var visibility = _mainPageViewModel.ActiveFolderView.FocusedNode.Type == MNodeType.TYPE_FILE 
                    ? Visibility.Visible : Visibility.Collapsed;
                BtnCreateShortCutCloudDrive.Visibility = _mainPageViewModel.ActiveFolderView.FocusedNode.Type == MNodeType.TYPE_FOLDER 
                    ? Visibility.Visible : Visibility.Collapsed;
                BtnDownloadItemCloudDrive.Visibility = visibility;
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
            
            if (MainPivot.SelectedItem.Equals(RubbishBinPivot)){

                LstRubbishBin.IsCheckModeActive = false;
                LstRubbishBin.CheckedItems.Clear();
        }

            _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = _mainPageViewModel.ActiveFolderView.PreviousDisplayMode;
            
            if (_mainPageViewModel.ActiveFolderView.FocusedNode != null)
                _mainPageViewModel.ActiveFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
            
            _mainPageViewModel.ActiveFolderView.FocusedNode = null;

            if (_mainPageViewModel.ActiveFolderView.SelectedNodes != null && 
                _mainPageViewModel.ActiveFolderView.SelectedNodes.Count > 0)
            {
                foreach (var node in _mainPageViewModel.ActiveFolderView.SelectedNodes)
                {
                    node.DisplayMode = NodeDisplayMode.Normal;
                }
                _mainPageViewModel.ActiveFolderView.SelectedNodes.Clear();
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
            if (_mainPageViewModel.ActiveFolderView.FocusedNode != null)
            {
                _mainPageViewModel.ActiveFolderView.FocusedNode.Move(_mainPageViewModel.ActiveFolderView.FolderRootNode);
                _mainPageViewModel.ActiveFolderView.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
            }

            if (_mainPageViewModel.ActiveFolderView.SelectedNodes != null && 
                _mainPageViewModel.ActiveFolderView.SelectedNodes.Count > 0)
            {
                foreach (var node in _mainPageViewModel.ActiveFolderView.SelectedNodes)
                {
                    node.Move(_mainPageViewModel.ActiveFolderView.FolderRootNode);
                    node.DisplayMode = NodeDisplayMode.Normal;
                }
                _mainPageViewModel.ActiveFolderView.SelectedNodes.Clear();
            }

            _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = _mainPageViewModel.ActiveFolderView.PreviousDisplayMode;
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
            _mainPageViewModel.ActiveFolderView.SelectedNodes.Add(_mainPageViewModel.ActiveFolderView.FocusedNode);
            _mainPageViewModel.ActiveFolderView.PreviousDisplayMode = _mainPageViewModel.ActiveFolderView.CurrentDisplayMode;
            _mainPageViewModel.ActiveFolderView.CurrentDisplayMode = DriveDisplayMode.MoveItem;
            _mainPageViewModel.ActiveFolderView.FocusedNode.DisplayMode = NodeDisplayMode.SelectedForMove;
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

            Dispatcher.BeginInvoke(SetApplicationBarData);
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
            if (!_mainPageViewModel.ActiveFolderView.SelectMultipleItemsForMove()) return;

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

            SetApplicationBarData();
        }

        private void OnImportLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.MegaSdk.getPublicNode(
                _mainPageViewModel.ActiveImportLink, new GetPublicNodeRequestListener(_mainPageViewModel.CloudDrive));

            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.CloudDrive;

            SetApplicationBarData();
        }

        private void OnCancelImportClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _mainPageViewModel.ActiveImportLink = null;
            _mainPageViewModel.CloudDrive.CurrentDisplayMode = DriveDisplayMode.CloudDrive;

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