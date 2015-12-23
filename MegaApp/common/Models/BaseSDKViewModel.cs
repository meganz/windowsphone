using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public abstract class BaseSdkViewModel: BaseViewModel
    {
        protected BaseSdkViewModel(MegaSDK megaSdk)
        {
            this.MegaSdk = megaSdk;
        }

        #region Methods

        public bool IsUserOnline()
        {
            if (!NetworkService.IsNetworkAvailable()) return false;

            bool isOnline = Convert.ToBoolean(this.MegaSdk.isLoggedIn());

            if (!isOnline)
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.UserNotOnline_Title,
                            AppMessages.UserNotOnline,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });

            return isOnline;
        }

        #endregion

        #region Properties

        public MegaSDK MegaSdk { get; private set; }

        #endregion
    }
}
