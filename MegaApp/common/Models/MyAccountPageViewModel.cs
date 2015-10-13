using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class MyAccountPageViewModel : BaseAppInfoAwareViewModel
    {
        public MyAccountPageViewModel(MegaSDK megaSdk, AppInformation appInformation, MyAccountPage myAccountPage)
            : base(megaSdk, appInformation)
        {
            InitializeMenu(HamburgerMenuItemType.MyAccount);

            AccountDetails = new AccountDetailsViewModel(myAccountPage) {UserEmail = megaSdk.getMyEmail()};
            UpgradeAccount = new UpgradeAccountViewModel();
            IsAccountUpdate = false;

            if (!File.Exists(AccountDetails.AvatarPath)) return;
            AccountDetails.AvatarUri = new Uri(AccountDetails.AvatarPath);            
        }

        #region Methods

        public void GetAccountDetails()
        {
            MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(AccountDetails));
            MegaSdk.getUserAvatar(MegaSdk.getContact(MegaSdk.getMyEmail()), AccountDetails.AvatarPath, new GetUserAvatarRequestListener(AccountDetails));
            MegaSdk.getOwnUserData(new GetUserDataRequestListener(AccountDetails));
            MegaSdk.creditCardQuerySubscriptions(new GetAccountDetailsRequestListener(AccountDetails));
        }

        public void GetPricing()
        {
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
            DialogService.ShowChangePasswrodDialog();
        }

        public void CancelSubscription()
        {
            DialogService.ShowCancelSubscriptionFeedbackDialog();
        }

        #endregion

        #region Properties

        private AccountDetailsViewModel _accountDetails;
        public AccountDetailsViewModel AccountDetails
        {
            get { return _accountDetails; }
            set
            {
                _accountDetails = value;                
                OnPropertyChanged("AccountDetails");
            }
        }

        private UpgradeAccountViewModel _upgradeAccount;
        public UpgradeAccountViewModel UpgradeAccount
        {
            get { return _upgradeAccount; }
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

        /*private string AvatarPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory, "UserAvatarImage");
            }
        }*/

        #endregion
    }
}
