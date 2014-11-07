using System.ComponentModel;
using System.ServiceModel.Description;
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
using System.IO;
using System.Linq;
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

            InteractionEffectManager.AllowedTypes.Add(typeof (RadDataBoundListBoxItem));

            // node tap item animation
            var transition = new RadTileTransition();
            this.SetValue(RadTransitionControl.TransitionProperty, transition);
            this.SetValue(RadTileAnimation.ContainerToAnimateProperty, LstCloudDrive);

            BreadCrumbControl.OnBreadCrumbTap += BreadCrumbControlOnOnBreadCrumbTap;
            BreadCrumbControl.OnHomeTap += BreadCrumbControlOnOnHomeTap;
        }

        private void BreadCrumbControlOnOnHomeTap(object sender, EventArgs eventArgs)
        {
            App.CloudDrive.GoToRoot();
        }

        private void BreadCrumbControlOnOnBreadCrumbTap(object sender, BreadCrumbTapEventArgs e)
        {
            App.CloudDrive.GoToFolder(e.Item as NodeViewModel);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (App.CloudDrive.DriveDisplayMode)
            {
                case DriveDisplayMode.MoveItem:
                {
                    this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
                    break;
                }
                default:
                {
                    this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                    App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
                    break;
                }
            }

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
                case NavigationParameter.Unknown:
                {
                    if (!SettingsService.LoadSetting<bool>(SettingsResources.StayLoggedIn))
                    {
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);
                        return;
                    }
                    else
                    {
                        App.MegaSdk.fastLogin(SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession), new FastLoginRequestListener(App.CloudDrive));
                    }
                    break;
                }
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if(!NavigationService.CanGoBack)
            {
                if (App.MegaSdk.getParentNode(App.CloudDrive.CurrentRootNode.GetMegaNode()) != null)
                {
                    App.CloudDrive.GoFolderUp();
                    App.CloudDrive.LoadNodes();
                    e.Cancel = true;
                }
            }
            base.OnBackKeyPress(e);
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if(e.Item == null || e.Item.DataContext == null) return;
            if (e.Item.DataContext as NodeViewModel == null) return;

            this.SetValue(RadTileAnimation.ElementToDelayProperty, e.Item);
            
            App.CloudDrive.OnNodeTap(e.Item.DataContext as NodeViewModel);
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
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
            FileService.ClearFiles(
                NodeService.GetFiles(App.CloudDrive.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory)));
            App.CloudDrive.FetchNodes(App.CloudDrive.CurrentRootNode);
        }
        private void OnAddFolderClick(object sender, EventArgs e)
        {
            App.CloudDrive.AddFolder(App.CloudDrive.CurrentRootNode);
        }

        private void OnOpenLinkClick(object sender, EventArgs e)
        {
            App.CloudDrive.OpenLink();
        }
        private void OnMyAccountClick(object sender, EventArgs e)
        {
            App.CloudDrive.GoToAccountDetails();
        }

        private void OnTransfersClick(object sender, EventArgs e)
        {
            App.CloudDrive.GoToTransfers();
        }

        private void OnCloudUploadClick(object sender, EventArgs e)
        {
            DialogService.ShowUploadOptions(App.CloudDrive);
        }

        private void OnCancelMoveClick(object sender, EventArgs e)
        {
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;

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

            this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
        }
        private void OnAcceptMoveClick(object sender, EventArgs e)
        {
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

            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.CloudDrive;
            this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
        }

        private void OnPreferencesClick(object sender, EventArgs e)
        {
            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }

        private void OnMoveItemTap(object sender, ContextMenuItemSelectedEventArgs e)
        {
            this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.MoveItem;
            App.CloudDrive.FocusedNode.DisplayMode = NodeDisplayMode.SelectedForMove;
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
                        ((NodeViewModel)e.DataItem).SetThumbnailImage();
                    break;
            }
        }

        private void OnScrollStateChanged(object sender, ScrollStateChangedEventArgs e)
        {
           //
        }

        private void OnGoToTopTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.CloudDrive.HasChildNodes()) return;
            
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.First());
        }

        private void OnGoToBottomTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!App.CloudDrive.HasChildNodes()) return;
           
            LstCloudDrive.BringIntoView(App.CloudDrive.ChildNodes.Last());
        }

        private void OnSortTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DialogService.ShowSortDialog(App.CloudDrive);
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            if (e.CheckBoxesVisible)
            {
                LstCloudDrive.CheckedItems.Add(e.TappedItem);
                this.ApplicationBar = (ApplicationBar)Resources["MultiSelectMenu"];
                App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MultiSelectMenu);
            }
            else
            {
                LstCloudDrive.CheckedItems.Clear();
                this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
                App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
            }
        }
        private void OnMultiSelectDownloadClick(object sender, EventArgs e)
        {
            if (LstCloudDrive.CheckedItems.Count < 1) return;

            foreach (var item in LstCloudDrive.CheckedItems)
            {
                var node = (NodeViewModel) item;
                
                if (node == null) continue;
                
                App.MegaTransfers.Add(node.Transfer);
                node.Transfer.StartTransfer();
            }
            LstCloudDrive.IsCheckModeActive = false;
            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }

        private void OnMultiSelectMoveClick(object sender, EventArgs e)
        {
            if (LstCloudDrive.CheckedItems.Count < 1) return;

            App.CloudDrive.SelectedNodes.Clear();

            foreach (var item in LstCloudDrive.CheckedItems)
            {
                var node = (NodeViewModel)item;

                if (node == null) continue;

                node.DisplayMode = NodeDisplayMode.SelectedForMove;
                App.CloudDrive.SelectedNodes.Add(node);
            }

            LstCloudDrive.IsCheckModeActive = false;
            App.CloudDrive.DriveDisplayMode = DriveDisplayMode.MoveItem;
            this.ApplicationBar = (ApplicationBar)Resources["MoveItemMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.MoveMenu);
        }

        private void OnMultiSelectRemoveClick(object sender, EventArgs e)
        {
            if (LstCloudDrive.CheckedItems.Count < 1) return;

            if (MessageBox.Show(String.Format("Are you sure you want to remove {0} items?", LstCloudDrive.CheckedItems.Count),
                "Remove items", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

            foreach (var item in LstCloudDrive.CheckedItems)
            {
                var node = (NodeViewModel)item;

                if (node == null) continue;

                node.Remove();
            }

            LstCloudDrive.IsCheckModeActive = false;
            this.ApplicationBar = (ApplicationBar)Resources["CloudDriveMenu"];
            App.CloudDrive.TranslateAppBar(ApplicationBar.Buttons, ApplicationBar.MenuItems, MenuType.CloudDriveMenu);
        }
    }
    
}