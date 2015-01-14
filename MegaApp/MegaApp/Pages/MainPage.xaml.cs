using System.Collections.Generic;
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
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Windows.Storage;

namespace MegaApp.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        private NavigationParameter _navParam;        

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
                    ((ApplicationBarIconButton) button).IsEnabled = args.Status;
                }

                foreach (var item in ApplicationBar.MenuItems)
                {
                    ((ApplicationBarMenuItem) item).IsEnabled = args.Status;
                }

                BtnSelectSorting.IsEnabled = args.Status;
            };
        }

        private void CreateAdvancedMenu()
        {
            var advancedMenuItems = new List<AdvancedMenuItem>();
            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.Transfers,
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.GoToTransfers();
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.MyAccount,
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    App.CloudDrive.GoToAccountDetails();
                }
            });

            advancedMenuItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.Settings,
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
                Name = UiResources.About,
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
                Name = UiResources.Logout,
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

            if(App.AppEvent == ApplicationEvent.Activated)
            {                
                App.AppEvent = ApplicationEvent.None;
                return;
            }

            ChangeMenu();

            _navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);
            if (NavigationContext.QueryString.ContainsKey("ShortCutHandle"))
            {
                App.CloudDrive.ShortCutHandle = Convert.ToUInt64(NavigationContext.QueryString["ShortCutHandle"]);
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
                case NavigationParameter.ImportLinkLaunch:
                {
                    //App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener());
                    //App.CloudDrive.FetchNodes();

                    //App.CloudDrive.ImportLink(NavigationContext.QueryString["link"]);
                    break;
                }
                case NavigationParameter.PasswordLogin:
                {
                    NavigationService.RemoveBackEntry();
                    App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                    break;
                }
                case NavigationParameter.Unknown:
                {
                    if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                        return;
                    }
                    
                    if (SettingsService.LoadSetting<bool>(SettingsResources.UserPasswordIsEnabled))
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
                        catch (System.ArgumentNullException)
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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (MainPivot != null && MainPivot.SelectedItem == MenuPivot)
            {
                MainPivot.SelectedItem = DrivePivot;
                e.Cancel = true;
                return;
            }

            if(!NavigationService.CanGoBack)
            {
                if (App.CloudDrive.CurrentRootNode != null && 
                    App.MegaSdk.getParentNode(App.CloudDrive.CurrentRootNode.GetMegaNode()) != null)
                {
                    App.CloudDrive.GoFolderUp();
                    Task.Run(() => App.CloudDrive.LoadNodes());
                    e.Cancel = true;
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
            base.OnBackKeyPress(e);
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if(e.Item == null || e.Item.DataContext == null) return;
            if (e.Item.DataContext as NodeViewModel == null) return;
            
            App.CloudDrive.OnNodeTap(e.Item.DataContext as NodeViewModel);
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
                App.CloudDrive.FocusedNode = focusedListBoxItem.DataContext as NodeViewModel;
                var visibility = App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FILE ? Visibility.Visible : Visibility.Collapsed;
                BtnCreateShortCut.Visibility = App.CloudDrive.FocusedNode.Type == MNodeType.TYPE_FOLDER ? Visibility.Visible : Visibility.Collapsed;
                BtnDownloadItemCloud.Visibility = visibility;
            }
        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {
            if (_navParam != NavigationParameter.Browsing && _navParam != NavigationParameter.BreadCrumb) return;

            // Load nodes in the onlistloaded event so that the nodes will display after the back animation and not before
            App.CloudDrive.LoadNodes();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            //MessageBox.Show(LstCloudDrive.RealizedItems.Length.ToString());

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

            DialogService.ShowUploadOptions(App.CloudDrive);
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

        private void OnGoToTopTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!App.CloudDrive.HasChildNodes()) return;
            
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (!App.CloudDrive.HasChildNodes()) return;
           
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.Last());
        }

        private void OnSortTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

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

            ChangeMenu();
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
        
        private void OnAboutClick(object sender, System.EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(AboutPage), NavigationParameter.Normal);
        }

        private void OnCloudDriveClick(object sender, EventArgs e)
        {
            var rootNode = App.MegaSdk.getRootNode();

            if (rootNode == null) return;

            var node = NodeService.CreateNew(App.MegaSdk, rootNode);
            App.CloudDrive.CurrentRootNode = node;
            App.CloudDrive.BreadCrumbNode = node;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;

            this.BreadCrumbControl.RootName = "Cloud Drive";

            Task.Run(() => App.CloudDrive.LoadNodes());
            ChangeMenu();
        }

        private void OnRubbishBinClick(object sender, EventArgs e)
        {
            var rubbishNode = App.MegaSdk.getRubbishNode();

            if (rubbishNode == null) return;

            var node = NodeService.CreateNew(App.MegaSdk, rubbishNode);                        
            App.CloudDrive.CurrentRootNode = node;
            App.CloudDrive.BreadCrumbNode = node;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.RubbishBin;

            this.BreadCrumbControl.RootName = "Rubbish Bin";

            Task.Run(() => App.CloudDrive.LoadNodes());
            ChangeMenu();
        }

        private void OnAdvancedMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var advancedMenuItem = e.Item.DataContext as AdvancedMenuItem;
            if (advancedMenuItem == null) return;
            advancedMenuItem.TapAction.Invoke();
        }        

        private void OnPivotSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ApplicationBar == null || MainPivot == null) return;
            ApplicationBar.IsVisible = MainPivot.SelectedItem == DrivePivot;
        }
    }
    
}