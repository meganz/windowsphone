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

        public void Initialize(GlobalListener globalListener)
        {
            
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            
        }

        public void GetAccountDetails()
        {
            if(!AccountDetails.IsDataLoaded)
            {
                AccountService.GetAccountDetails();
                MegaSdk.creditCardQuerySubscriptions(new GetAccountDetailsRequestListener());
                AccountDetails.IsDataLoaded = true;
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

        public UpgradeAccountViewModel UpgradeAccount
        {
            get { return AccountService.UpgradeAccount; }
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
