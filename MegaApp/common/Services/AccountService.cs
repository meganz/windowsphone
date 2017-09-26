using System;
using MegaApp.MegaApi;
using MegaApp.Models;

namespace MegaApp.Services
{
    public static class AccountService
    {
        public static event EventHandler GetAccountDetailsFinish;

        private static AccountDetailsViewModel _accountDetails;
        public static AccountDetailsViewModel AccountDetails
        {
            get
            {
                if (_accountDetails != null) return _accountDetails;
                _accountDetails = new AccountDetailsViewModel() { UserEmail = SdkService.MegaSdk.getMyEmail() };
                return _accountDetails;
            }
        }

        private static UpgradeAccountViewModel _upgradeAccount;
        public static UpgradeAccountViewModel UpgradeAccount
        {
            get
            {
                if (_upgradeAccount != null) return _upgradeAccount;
                _upgradeAccount = new UpgradeAccountViewModel();
                return _upgradeAccount;
            }
        }

        public static void ClearAccountDetails()
        {
            _accountDetails = null;
        }

        public static void GetAccountDetails()
        {
            SdkService.MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(GetAccountDetailsFinish));
        }
    }
}
