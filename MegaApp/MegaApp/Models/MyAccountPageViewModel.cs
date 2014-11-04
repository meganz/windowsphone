using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.Models
{
    class MyAccountPageViewModel : BaseSdkViewModel
    {
        public MyAccountPageViewModel(MegaSDK megaSdk)
            :base(megaSdk)
        {
            AccountDetails = new AccountDetailsViewModel {UserName = megaSdk.getMyEmail()};
        }

        #region Methods

        public void GetAccountDetails()
        {
            MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(AccountDetails));
        }

        public void GetPricing()
        {
            MegaSdk.getPricing(new GetPricingRequestListener(AccountDetails));
        }

        public void Logout()
        {
            MegaSdk.logout(new LogOutRequestListener());
        }

        public void ClearCache()
        {
            AppService.ClearAppCache(false);
            MessageBox.Show("Cache has been succesfully cleared", "Cache cleared", MessageBoxButton.OK);
            AccountDetails.CacheSize = AppService.GetAppCacheSize();
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

        #endregion


    }
}
