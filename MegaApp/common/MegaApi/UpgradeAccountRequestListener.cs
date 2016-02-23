using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class UpgradeAccountRequestListener : BaseRequestListener
    {
        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_UpgradeAccount; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.UpgradeAccountFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.UpgradeAccountFailed_Title.ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.UpgradeAccountSuccessfully; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.UpgradeAccountSuccessfully_Title.ToUpper(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
        }

        protected override Type NavigateToPage
        {
            get { return typeof(MyAccountPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.AccountUpdate; }
        }

        #endregion
    }
}
