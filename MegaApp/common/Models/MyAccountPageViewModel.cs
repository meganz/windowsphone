using System;
using System.IO;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class MyAccountPageViewModel : BaseAppInfoAwareViewModel
    {
        public MyAccountPageViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            InitializeMenu(HamburgerMenuItemType.MyAccount);

            UpdateUserData();

            AccountDetails = new AccountDetailsViewModel() { UserEmail = megaSdk.getMyEmail() };
            UpgradeAccount = new UpgradeAccountViewModel();
            IsAccountUpdate = false;
        }

        #region Methods

        public void SetOfflineContentTemplate()
        {
            OnUiThread(() =>
            {
                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["OfflineEmptyContent"];
                this.EmptyInformationText = UiResources.NoInternetConnection.ToLower();
            });
        }

        public void Initialize(GlobalDriveListener globalDriveListener)
        {
            // Add contacts to global drive listener to receive notifications
            globalDriveListener.Accounts.Add(this);
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Remove contacts of global drive listener
            globalDriveListener.Accounts.Remove(this);
        }

        public void GetAccountDetails()
        {
            if(!_accountDetails.IsDataLoaded)
            {
                MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(AccountDetails));                
                MegaSdk.creditCardQuerySubscriptions(new GetAccountDetailsRequestListener(AccountDetails));

                OnUiThread(() =>
                {
                    AccountDetails.HasAvatarImage = UserData.HasAvatarImage;                    
                    AccountDetails.AvatarUri = UserData.AvatarUri;                    
                    AccountDetails.Firstname = UserData.Firstname;
                    AccountDetails.Lastname = UserData.Lastname;
                });                

                _accountDetails.IsDataLoaded = true;
            }            
        }

        public async void GetPricing()
        {
            this.UpgradeAccount.InAppPaymentMethodAvailable = await LicenseService.IsAvailable();
            MegaSdk.getPaymentMethods(new GetPaymentMethodsRequestListener(UpgradeAccount));
            MegaSdk.getPricing(new GetPricingRequestListener(AccountDetails, UpgradeAccount));            
        }

        public void Logout()
        {
            MegaSdk.logout(new LogOutRequestListener());
        }

        public void ClearCache()
        {
            AppService.ClearAppCache(false);
            new CustomMessageDialog(
                    AppMessages.CacheCleared_Title,
                    AppMessages.CacheCleared,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            AccountDetails.CacheSize = AppService.GetAppCacheSize();
        }

        public void ChangePassword()
        {
            DialogService.ShowChangePasswordDialog();
        }

        public void CancelSubscription()
        {
            DialogService.ShowCancelSubscriptionFeedbackDialog();
        }

        public void CloseAllSessions()
        {
            MegaSdk.killAllSessions(new KillAllSessionsRequestListener());
        }

        #endregion

        #region Properties

        private AccountDetailsViewModel _accountDetails;
        public AccountDetailsViewModel AccountDetails
        {
            get 
            { 
                if(_accountDetails != null) return _accountDetails;
                _accountDetails = new AccountDetailsViewModel() { UserEmail = this.MegaSdk.getMyEmail() };
                return _accountDetails;
            }
            set
            {
                _accountDetails = value;                
                OnPropertyChanged("AccountDetails");
            }
        }

        private DataTemplate _emptyContentTemplate;
        public DataTemplate EmptyContentTemplate
        {
            get { return _emptyContentTemplate; }
            private set { SetField(ref _emptyContentTemplate, value); }
        }

        private String _emptyInformationText;
        public String EmptyInformationText
        {
            get { return _emptyInformationText; }
            private set { SetField(ref _emptyInformationText, value); }
        }

        private UpgradeAccountViewModel _upgradeAccount;
        public UpgradeAccountViewModel UpgradeAccount
        {
            get 
            { 
                if(_upgradeAccount != null) return _upgradeAccount;
                _upgradeAccount = new UpgradeAccountViewModel();
                return _upgradeAccount;
            }
            set
            {
                _upgradeAccount = value;
                OnPropertyChanged("UpgradeAccount");
            }
        }

        private bool _isAccountUpdate;
        public bool IsAccountUpdate
        {
            get { return _isAccountUpdate; }
            set
            {
                _isAccountUpdate = value;
                OnPropertyChanged("IsAccountUpdate");
            }
        }

        #endregion
    }
}
