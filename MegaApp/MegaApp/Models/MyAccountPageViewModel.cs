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
            AccountDetails = new AccountDetailsModel {UserName = megaSdk.getMyEmail()};
            MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(AccountDetails));
        }

        #region Methods

        public void Logout()
        {
            MegaSdk.logout(new LogOutRequestListener());
        }

        public void ClearCache()
        {
            AppService.ClearAppCache();
            MessageBox.Show("Cache has been succesfully cleared", "Cache cleared", MessageBoxButton.OK);
        }

        #endregion

        #region Properties

        private AccountDetailsModel _accountDetails;

        public AccountDetailsModel AccountDetails
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
