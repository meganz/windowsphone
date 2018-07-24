using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class MyAccountPage : PhoneDrawerLayoutPage
    {
        private MyAccountPageViewModel _myAccountPageViewModel;
        private MyAccountPageViewModel myAccountPageViewModel
        {
            get
            {
                if (_myAccountPageViewModel != null) return _myAccountPageViewModel;
                _myAccountPageViewModel = new MyAccountPageViewModel(SdkService.MegaSdk, App.AppInformation);
                return _myAccountPageViewModel;
            }
        }

        public MyAccountPage()
        {
            this.DataContext = myAccountPageViewModel;

            myAccountPageViewModel.AccountDetails.CreditCardSubscriptionsChanged += OnCreditCardSubscriptionsChanged;
            
            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.MyAccount);

            SetApplicationBarData();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);
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

        private void OnCreditCardSubscriptionsChanged(object sender, EventArgs e)
        {
            SetApplicationBarData(NetworkService.IsNetworkAvailable());
        }

        public void SetApplicationBarData(bool isNetworkConnected = true)
        {
            this.ApplicationBar = (ApplicationBar)Resources["MyAccountMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.SettingsShort.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.UI_Logout.ToLower();

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.UI_ChangePassword.ToLower();
            
            // Only if is a LITE account show a "cancel subscription" menu option
            if (myAccountPageViewModel.AccountDetails.AccountType == MAccountType.ACCOUNT_TYPE_LITE &&
                myAccountPageViewModel.AccountDetails.CreditCardSubscriptions != 0)
            {
                if(ApplicationBar.MenuItems.Count == 3)
                {
                    ApplicationBarMenuItem cancelSubscription = new ApplicationBarMenuItem(UiResources.UI_CancelSubscription.ToLower());
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

        /// <summary>
        /// Set the user avatar color.
        /// </summary>        
        private void SetAvatarColor()
        {
            myAccountPageViewModel.AccountDetails.AvatarColor = AccountService.AccountDetails.AvatarColor;
        }

        private void UpdateGUI(bool isNetworkConnected = true)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (isNetworkConnected)
                {
                    if(!Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
                    {
                        NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.None);
                        return;
                    }

                    myAccountPageViewModel.GetAccountDetails();
                    myAccountPageViewModel.GetPricing();
                }
                else
                {
                    myAccountPageViewModel.SetOfflineContentTemplate();
                }

                SetAvatarColor();
                SetApplicationBarData(isNetworkConnected);
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            myAccountPageViewModel.Deinitialize(App.GlobalListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            myAccountPageViewModel.Initialize(App.GlobalListener);

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
                myAccountPageViewModel.IsAccountUpdate = true;
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

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }

            SetAvatarColor();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Check if can go back in the stack of pages
            e.Cancel = CheckGoBack(e.Cancel);
        }

        private void OnPieDataBindingComplete(object sender, EventArgs e)
        {
            // Focus on the first datapoint (= Used space)
            //((PieSeries) sender).DataPoints[0].OffsetFromCenter = 0.05;
        }

        private async void OnLogoutClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            var transferData = SdkService.MegaSdk.getTransferData();
            int numPendingTransfers = transferData.getNumDownloads() + transferData.getNumUploads();
            
            if (numPendingTransfers > 0)
            {
                var result = await new CustomMessageDialog(
                    AppMessages.PendingTransfersLogout_Title,
                    String.Format(AppMessages.PendingTransfersLogout, numPendingTransfers),
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return;

                SdkService.MegaSdk.cancelTransfers((int)MTransferType.TYPE_DOWNLOAD);
                SdkService.MegaSdk.cancelTransfers((int)MTransferType.TYPE_UPLOAD);
            }

            myAccountPageViewModel.Logout();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
        }

        private void OnChangePasswordClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            myAccountPageViewModel.ChangePassword();
        }

        private void OnCancelSubscriptionClick(object sender, EventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            myAccountPageViewModel.CancelSubscription();
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
            for (int i = 0; i < myAccountPageViewModel.UpgradeAccount.Products.Count; i++)
            {
                if (myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i).AccountType == ((ProductBase)LstPlans.SelectedItem).AccountType)
                {
                    switch (myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i).Months)
                    {
                        case 1:
                            PhoneApplicationService.Current.State["SelectedPlanMonthly"] = myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i);
                            break;
                        case 12:
                            PhoneApplicationService.Current.State["SelectedPlanAnnualy"] = myAccountPageViewModel.UpgradeAccount.Products.ElementAt(i);
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
            if (!NetworkService.IsNetworkAvailable()) return;

            if (sender == PivotAccount)
                myAccountPageViewModel.GetAccountDetails();
            else
                myAccountPageViewModel.GetPricing();
        }        
                
        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }
        
        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

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