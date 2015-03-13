using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using mega;
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
    public partial class MainPage : PhoneApplicationPage
    {
        private NavigationParameter _navParam;
        private bool _normalBackAction = false;

        public MainPage()
        {
            this.DataContext = App.CloudDrive;

            InitializeComponent();

            CreateAdvancedMenu();
            
            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            BreadCrumbControl.OnBreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            BreadCrumbControl.OnHomeTap += BreadCrumbControlOnOnHomeTap;
            
            App.CloudDrive.CommandStatusChanged += (sender, args) =>
            {
                if (ApplicationBar == null) return;

                foreach (var button in ApplicationBar.Buttons)
                {
                    ((ApplicationBarIconButton)button).IsEnabled = args.Status;
                }

                foreach (var item in ApplicationBar.MenuItems)
                {
                    ((ApplicationBarMenuItem)item).IsEnabled = args.Status;
                }

                BtnSelectSorting.IsEnabled = args.Status;
               
            };
        }

        private void CreateAdvancedMenu()
        {
            var advancedMenuItems = new List<AdvancedMenuItem>();

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.Transfers.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.GoToTransfers();
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.MyAccount.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.GoToAccountDetails();
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.Preferences.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.NoFolderUpAction = true;
                    NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.About.ToLower().ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.NoFolderUpAction = true;
                    NavigateService.NavigateTo(typeof(AboutPage), NavigationParameter.Normal);
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.Logout.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    if(App.MegaTransfers.Count > 0)
                    {
                        if (MessageBox.Show(String.Format(AppMessages.PendingTransfersLogout, App.MegaTransfers.Count),
                            AppMessages.PendingTransfersLogout_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

                        foreach (var item in App.MegaTransfers)
                        {
                            var transfer = (TransferObjectModel)item;
                            if (transfer == null) continue;

                            transfer.CancelTransfer();
                        }
                    }

                    App.MegaSdk.logout(new LogOutRequestListener());
                }
            });
           

            LstAdvancedMenu.ItemsSource = advancedMenuItems;
        }        

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.GoToRoot();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.GoToFolder(e.Item as NodeViewModel);
        }

        private void ChangeMenu()
        {
            #if WINDOWS_PHONE_81
            BorderLinkText.Visibility = Visibility.Collapsed;
            #endif

            switch (App.CloudDrive.DriveDisplayMode)
            {
                case DriveDisplayMode.RubbishBin:
                    this.ApplicationBar = (ApplicationBar)Resources["RubbishBinMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.RubbishBinMenu);
                    break;

                case DriveDisplayMode.MoveItem:
                    this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
                    break;

                case DriveDisplayMode.MultiSelect:
                    this.ApplicationBar = (ApplicationBar)Resources["MultiSelectMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MultiSelectMenu);
                    break;

                #if WINDOWS_PHONE_81
                case DriveDisplayMode.ImportItem:
                    this.ApplicationBar = (ApplicationBar)Resources["ImportItemMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.ImportMenu);
                    BorderLinkText.Visibility = Visibility.Visible;
                    break;
                #endif

                case DriveDisplayMode.CloudDrive:
                default:
                    this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
                    break;
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.CloudDrive.ListBox = LstCloudDrive;
            
            #if WINDOWS_PHONE_81
            LstAdvancedMenu.SelectedItem = null;
            #endif

            if(App.AppEvent == ApplicationEvent.Activated)
            {                
                App.AppEvent = ApplicationEvent.None;

                #if WINDOWS_PHONE_81
                App.CloudDrive.NoFolderUpAction = false;

                // Needed on every UI interaction
                App.MegaSdk.retryPendingConnections();

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

                return;
            }
            
            ChangeMenu();

            _navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            if (NavigationContext.QueryString.ContainsKey("ShortCutHandle"))
            {
                App.CloudDrive.ShortCutHandle = Convert.ToUInt64(NavigationContext.QueryString["ShortCutHandle"]);
            }            

            if (e.NavigationMode == NavigationMode.Reset)
            {
                return;
            }

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (!App.CloudDrive.NoFolderUpAction)
                {
                    App.CloudDrive.GoFolderUp();                    
                    _navParam = NavigationParameter.Browsing;
                }
                else
                    _navParam = NavigationParameter.Normal;

                if(NavigateService.PreviousPage == typeof(MyAccountPage))
                    _navParam = NavigationParameter.Browsing;
            }

            App.CloudDrive.NoFolderUpAction = false;

            switch (_navParam)
            {
                case NavigationParameter.Login:
                {
                    // Remove the login page from the stack. If user presses back button it will then exit the application
                    NavigationService.RemoveBackEntry();
                    
                    App.CloudDrive.FetchNodes();
                    break;
                }
                case NavigationParameter.BreadCrumb:
                {
                    int breadCrumbs = App.CloudDrive.CountBreadCrumbs();
                    for (int x = 0; x <= breadCrumbs; x++)
                        NavigationService.RemoveBackEntry();
                   
                    break;
                }
                case NavigationParameter.PasswordLogin:
                {
                    NavigationService.RemoveBackEntry();
                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                    break;
                }

                #if WINDOWS_PHONE_80
                case NavigationParameter.ImportLinkLaunch:                
                {
                    if (NavigationContext.QueryString.ContainsKey("filelink"))
                    {
                        if (!Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
                        {
                            try
                            {
                                if (SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession) != null)
                                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                                else
                                {
                                    NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                                    return;
                                }
                            }
                            catch (ArgumentNullException)
                            {
                                NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                                return;
                            }
                        }

                        //App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                        //App.CloudDrive.FetchNodes();

                        string _link = NavigationContext.QueryString["filelink"];                        
                        if (_link.StartsWith("mega://"))
                            _link = _link.Replace("mega://", "https://mega.co.nz/#");

                        App.MegaSdk.getPublicNode(_link, new GetPublicNodeRequestListener(App.CloudDrive));
                    }
                    break;
                }
                #elif WINDOWS_PHONE_81
                case NavigationParameter.ImportLinkLaunch:
                #endif

                case NavigationParameter.Unknown:
                {
                    #if WINDOWS_PHONE_81
                    if (e.NavigationMode != NavigationMode.Back)
                    {
                        if (NavigationContext.QueryString.ContainsKey("filelink"))
                        {
                            App.CloudDrive.LinkToImport = NavigationContext.QueryString["filelink"];
                            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.ImportItem;
                            ChangeMenu();
                        }
                    }
                    #endif

                    if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
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
                                App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                            else
                            {
                                NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                                return;
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                            return;
                        }
                            
                    }
                    
                    break;
                }
            }
            
            base.OnNavigatedTo(e);
            App.AppEvent = ApplicationEvent.None;
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
                    MessageBox.Show(String.Format(AppMessages.PrepareFileForUploadFailed, file.Name),
                        AppMessages.PrepareFileForUploadFailed_Title, MessageBoxButton.OK);
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
            if (!_normalBackAction)
            {
                if (MainPivot != null && MainPivot.SelectedItem == MenuPivot)
                {
                    MainPivot.SelectedItem = DrivePivot;
                    e.Cancel = true;
                    return;
                }

                if (!NavigationService.CanGoBack)
                {
                    if (App.CloudDrive.CurrentRootNode != null &&
                        App.MegaSdk.getParentNode(App.CloudDrive.CurrentRootNode.GetMegaNode()) != null)
                    {
                        App.CloudDrive.GoFolderUp();
                        Task.Run(() => App.CloudDrive.LoadNodes());
                        e.Cancel = true;
                    }
                    else if (App.CloudDrive.CurrentRootNode != null && App.CloudDrive.CurrentRootNode.Type == MNodeType.TYPE_RUBBISH)
                    {
                        this.BreadCrumbControl.RootName = UiResources.CloudDriveName;
                        App.CloudDrive.ChangeDrive(true);
                        ChangeMenu();

                        e.Cancel = true;
                        return;
                    }
                    else if (App.CloudDrive.DriveDisplayMode != DriveDisplayMode.MultiSelect)
                    {
                        if (App.MegaTransfers.Count(t => t.Status != TransferStatus.Finished) > 0)
                        {
                            if (MessageBox.Show(String.Format(AppMessages.PendingTransfersExit, App.MegaTransfers.Count),
                                AppMessages.PendingTransfersExit_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }
            }

            _normalBackAction = false;
            base.OnBackKeyPress(e);
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if(e.Item == null || e.Item.DataContext == null) return;
            if (!(e.Item.DataContext is NodeViewModel)) return;
            
            App.CloudDrive.OnNodeTap((NodeViewModel) e.Item.DataContext);
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || focusedListBoxItem.DataContext == null || !(focusedListBoxItem.DataContext is NodeViewModel))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                _normalBackAction = true;
                App.CloudDrive.FocusedNode = (NodeViewModel) focusedListBoxItem.DataContext;
                var visibility = App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FILE ? Visibility.Visible : Visibility.Collapsed;
                BtnCreateShortCut.Visibility = 
                    App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FOLDER ? Visibility.Visible : Visibility.Collapsed;
                BtnDownloadItemCloud.Visibility = visibility;
            }
        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {
            if (_navParam != NavigationParameter.Browsing && _navParam != NavigationParameter.BreadCrumb)
            {
                // #1870 fix (single column when reactivating app)
                App.CloudDrive.SetView(App.CloudDrive.ViewMode);
                return;
            }

            // Load nodes in the onlistloaded event so that the nodes will display after the back animation and not before
            App.CloudDrive.LoadNodes();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            FileService.ClearFiles(
                NodeService.GetFiles(App.CloudDrive.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory)));

            App.CloudDrive.FetchNodes(App.CloudDrive.CurrentRootNode);

            if (App.CloudDrive.DriveDisplayMode == DriveDisplayMode.MultiSelect)
                App.CloudDrive.DriveDisplayMode = App.CloudDrive.OldDriveDisplayMode;
        }

        private void OnAddFolderClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
         
            App.CloudDrive.AddFolder(App.CloudDrive.CurrentRootNode);
        }

        private void OnOpenLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.OpenLink();
        }
        private void OnMyAccountClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.GoToAccountDetails();
        }

        private void OnTransfersClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.GoToTransfers();
        }

        private void OnCloudUploadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.ShowUploadOptions();
        }

        private void OnCancelMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
            
            App.CloudDrive.DriveDisplayMode = App.CloudDrive.OldDriveDisplayMode;
            
            if(App.CloudDrive.FocusedNode != null)
                App.CloudDrive.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
            App.CloudDrive.FocusedNode = null;

            if (App.CloudDrive.SelectedNodes.Count > 0)
            {
                foreach (var node in App.CloudDrive.SelectedNodes)
                {
                    node.DisplayMode = NodeDisplayMode.Normal;
                }
            }
            App.CloudDrive.SelectedNodes.Clear();

            LstCloudDrive.IsCheckModeActive = false;
            LstCloudDrive.CheckedItems.Clear();

            ChangeMenu();
        }
        private void OnAcceptMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (App.CloudDrive.FocusedNode != null)
            {
                App.CloudDrive.FocusedNode.Move(App.CloudDrive.CurrentRootNode);
                App.CloudDrive.FocusedNode.DisplayMode = NodeDisplayMode.Normal;
            }

            if (App.CloudDrive.SelectedNodes.Count > 0)
            {
                foreach (var node in App.CloudDrive.SelectedNodes)
                {
                    node.Move(App.CloudDrive.CurrentRootNode);
                    node.DisplayMode = NodeDisplayMode.Normal;
                }
                App.CloudDrive.SelectedNodes.Clear();
            }

            App.CloudDrive.DriveDisplayMode = App.CloudDrive.OldDriveDisplayMode;
            ChangeMenu();
        }

        private void OnPreferencesClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }

        private void OnMoveItemTap(object sender, ContextMenuItemSelectedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
                        
            App.CloudDrive.OldDriveDisplayMode = App.CloudDrive.DriveDisplayMode;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.MoveItem;
            App.CloudDrive.FocusedNode.DisplayMode = NodeDisplayMode.SelectedForMove;
            ChangeMenu();
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
                        //if(LstCloudDrive.IsItemInViewport(e.DataItem))
                        ((NodeViewModel)e.DataItem).SetThumbnailImage();
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

            if (!App.CloudDrive.HasChildNodes()) return;
            
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!App.CloudDrive.HasChildNodes()) return;
           
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.Last());
        }

        private void OnSortTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _normalBackAction = true;
            DialogService.ShowSortDialog(App.CloudDrive);
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (e.CheckBoxesVisible)
            {
                if(e.TappedItem != null)
                    LstCloudDrive.CheckedItems.Add(e.TappedItem);
                if(App.CloudDrive.DriveDisplayMode != DriveDisplayMode.MultiSelect)
                    App.CloudDrive.OldDriveDisplayMode = App.CloudDrive.DriveDisplayMode;
                App.CloudDrive.DriveDisplayMode = DriveDisplayMode.MultiSelect;
            }
            else
            {
                LstCloudDrive.CheckedItems.Clear();
                App.CloudDrive.DriveDisplayMode = App.CloudDrive.OldDriveDisplayMode;                
            }

            Dispatcher.BeginInvoke(ChangeMenu);
        }
        
        private void OnMultiSelectDownloadClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
           
            App.CloudDrive.MultipleDownload();
        }

        private void OnMultiSelectMoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!App.CloudDrive.SelectMultipleMove()) return;
            
            this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
        }

        private void OnMultiSelectRemoveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!App.CloudDrive.MultipleRemove()) return;

            App.CloudDrive.DriveDisplayMode = App.CloudDrive.OldDriveDisplayMode;
            ChangeMenu();
        }
        
        private void OnAboutClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(AboutPage), NavigationParameter.Normal);
        }

        private void OnCloudDriveClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var rootNode = App.MegaSdk.getRootNode();

            if (rootNode == null) return;

            var node = NodeService.CreateNew(App.MegaSdk, rootNode);
            App.CloudDrive.CurrentRootNode = node;
            App.CloudDrive.BreadCrumbNode = node;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;

            this.BreadCrumbControl.RootName = UiResources.HomeRoot;

            Task.Run(() => App.CloudDrive.LoadNodes());
            ChangeMenu();
        }

        private void OnRubbishBinClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var rubbishNode = App.MegaSdk.getRubbishNode();

            if (rubbishNode == null) return;

            var node = NodeService.CreateNew(App.MegaSdk, rubbishNode);                        
            App.CloudDrive.CurrentRootNode = node;
            App.CloudDrive.BreadCrumbNode = node;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.RubbishBin;

            this.BreadCrumbControl.RootName = UiResources.RubbishBinRoot;

            Task.Run(() => App.CloudDrive.LoadNodes());
            ChangeMenu();
        }

        private void OnAdvancedMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            LstAdvancedMenu.SelectedItem = null;

            var advancedMenuItem = e.Item.DataContext as AdvancedMenuItem;
            if (advancedMenuItem == null) return;
            advancedMenuItem.TapAction.Invoke();
        }        

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar == null || MainPivot == null) return;
            ApplicationBar.IsVisible = MainPivot.SelectedItem == DrivePivot;
        }

        private void OnImportLinkClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.MegaSdk.getPublicNode(App.CloudDrive.LinkToImport, new GetPublicNodeRequestListener(App.CloudDrive));
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;

            ChangeMenu();
        }

        private void OnCancelImportClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.LinkToImport = null;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;

            ChangeMenu();
        }        
    }    
}
