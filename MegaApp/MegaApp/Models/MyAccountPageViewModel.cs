using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;

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
