using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
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
    public partial class MyAccountPage : PhoneDrawerLayoutPage
    {
        private readonly MyAccountPageViewModel _myAccountPageViewModel;

        public MyAccountPage()
        {
            _myAccountPageViewModel = new MyAccountPageViewModel(App.MegaSdk, App.AppInformation, this);
            this.DataContext = _myAccountPageViewModel;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.MyAccount);

            SetApplicationBarData();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        public void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["MyAccountMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.SettingsShort.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Logout.ToLower();

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.ChangePassword.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.ClearCache.ToLower();
            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = UiResources.CloseAllSessions.ToLower();
            
            // Only if is a LITE account show a "cancel subscription" menu option
            if(_myAccountPageViewModel.AccountDetails.AccountType == MAccountType.ACCOUNT_TYPE_LITE &&
                _myAccountPageViewModel.AccountDetails.CreditCardSubscriptions != 0)
            {
                if(ApplicationBar.MenuItems.Count == 3)
                {
                    ApplicationBarMenuItem cancelSubscription = new ApplicationBarMenuItem(UiResources.CancelSubscription.ToLower());
                    ApplicationBar.MenuItems.Add(cancelSubscription);
                    cancelSubscription.Click += new EventHandler(OnCancelSubscriptionClick);
                }                
            }
            // Else remove the "cancel subscription" menu item if exists
            else if(ApplicationBar.MenuItems.Count == 4)
            {
                ApplicationBar.MenuItems.RemoveAt(3);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get last page (previous page)            
            var backStack = ((PhoneApplicationFrame)Application.Current.RootVisual).BackStack;
            var lastPage = backStack.FirstOrDefault();
            if (lastPage != null)
            {
                // If navigation is from the PaymentPage, remove the last entry of the back stack
                if (lastPage.Source.ToString().Contains((typeof(PaymentPage)).Name))
                    ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
            }

            if(NavigateService.ProcessQueryString(NavigationContext.QueryString) == NavigationParameter.AccountUpdate)
            {
                _myAccountPageViewModel.IsAccountUpdate = true;
                PivotAccountInformation.SelectedItem = PivotSubscription;
            }
            // Check if the navigation destiny is a specific pivot item
            else if (NavigationContext.QueryString.ContainsKey("Pivot"))
            {
                var index = NavigationContext.QueryString["Pivot"];
                if(!String.IsNullOrWhiteSpace(index))
                {
                    int indexParsed;
                    if((int.TryParse(index, out indexParsed)) && 
                        (indexParsed < PivotAccountInformation.Items.Count))
                    {
                        PivotAccountInformation.SelectedIndex = indexParsed;
                    }                    
                }                
            }
        }

        private void OnPieDataBindingComplete(object sender, EventArgs e)
        {
            // Focus on the first datapoint (= Used space)
            //((PieSeries) sender).DataPoints[0].OffsetFromCenter = 0.05;
        }

        private async void OnLogoutClick(object sender, EventArgs e)
        {
            int numPendingTransfers = App.MegaTransfers.Count(t => (t.Status == TransferStatus.Queued ||
                t.Status == TransferStatus.Downloading || t.Status == TransferStatus.Uploading ||
                t.Status == TransferStatus.Paused || t.Status == TransferStatus.Pausing));

            if (numPendingTransfers > 0)
            {
                var result = await new CustomMessageDialog(
                    AppMessages.PendingTransfersLogout_Title,
                    String.Format(AppMessages.PendingTransfersLogout, numPendingTransfers),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return;

                foreach (var item in App.MegaTransfers)
                {
                    var transfer = (TransferObjectModel)item;
                    if (transfer == null) continue;

                    transfer.CancelTransfer();
                }
            }

        	_myAccountPageViewModel.Logout();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }

        private void OnClearCacheClick(object sender, EventArgs e)
        {
            App.MainPageViewModel.CloudDrive.ChildNodes.Clear();
            App.MainPageViewModel.RubbishBin.ChildNodes.Clear();
            _myAccountPageViewModel.ClearCache();
        }

        private void OnChangePasswordClick(object sender, EventArgs e)
        {
            _myAccountPageViewModel.ChangePassword();
        }

        private void OnCancelSubscriptionClick(object sender, EventArgs e)
        {
            _myAccountPageViewModel.CancelSubscription();
        }

        private void OnCloseAllSessionsClick(object sender, EventArgs e)
        {
            _myAccountPageViewModel.CloseAllSessions();
        }

        private void OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // In case that it is an account newly activated, the list of available plans shows the "Free" option. 
            // If the user selects it, we only need to redirect it to the cloud drive
            if(((ProductBase)LstPlans.SelectedItem).AccountType == MAccountType.ACCOUNT_TYPE_FREE)
            {
                NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
                return;
            }

            // Else we need to identify the selected plan and send it along the Monthly and Annual plans of this type to the PaymentPage 
            for(int i=0; i < _myAccountPageViewModel.UpgradeAccount.Products.Count; i++)
            {
                if(_myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i).AccountType == ((ProductBase)LstPlans.SelectedItem).AccountType)
                {
                    switch(_myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i).Months)
                    {
                        case 1:
                            PhoneApplicationService.Current.State["SelectedPlanMonthly"] = _myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i);
                            break;
                        case 12:
                            PhoneApplicationService.Current.State["SelectedPlanAnnualy"] = _myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i);
                            break;
                        default:
                            break;
                    }
                }
            }

            PhoneApplicationService.Current.State["SelectedPlan"] = LstPlans.SelectedItem;
            NavigateService.NavigateTo(typeof(PaymentPage), NavigationParameter.Normal);
        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == PivotAccount)
                _myAccountPageViewModel.GetAccountDetails();
            else
                _myAccountPageViewModel.GetPricing();
        }        
                
        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }
        
        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MainDrawerLayout.CloseDrawer();
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