using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class LogOutRequestListener: BaseRequestListener
    {
        protected override string ProgressMessage
        {
            get { return ProgressMessages.Logout; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.LogoutFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.LogoutFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.LoggedOut; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.LoggedOut_Title; }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return typeof(LoginPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Normal; }
        }

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            SettingsService.ClearMegaLoginData();
            App.CloudDrive.ChildNodes.Clear();
            AppService.ClearAppCache(false);
        }

        #endregion
    }
}
