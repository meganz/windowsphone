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
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class MyAccountPageViewModel : BaseSdkViewModel
    {
        public MyAccountPageViewModel(MegaSDK megaSdk)
            :base(megaSdk)
        {
            AccountDetails = new AccountDetailsViewModel {UserName = megaSdk.getMyEmail()};
            if (!File.Exists(AvatarPath)) return;
            AccountDetails.AvatarUri = new Uri(AvatarPath);
        }

        #region Methods

        public void GetAccountDetails()
        {
            MegaSdk.getAccountDetails(new GetAccountDetailsRequestListener(AccountDetails));
            MegaSdk.getUserAvatar(MegaSdk.getContact(MegaSdk.getMyEmail()), AvatarPath, new GetUserAvatarRequestListener(AccountDetails));
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
            MessageBox.Show(AppMessages.CacheCleared, AppMessages.CacheCleared_Title.ToUpper(), MessageBoxButton.OK);
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

        private string AvatarPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory, "UserAvatarImage");
            }
        }

        #endregion


    }
}
