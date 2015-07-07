using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class GetPaymentMethodsRequestListener : BaseRequestListener
    {
        private readonly AccountDetailsViewModel _accountDetails;
        private readonly UpgradeAccountViewModel _upgradeAccount;

        public GetPaymentMethodsRequestListener(AccountDetailsViewModel accountDetails, UpgradeAccountViewModel upgradeAccount)
        {
            _accountDetails = accountDetails;
            _upgradeAccount = upgradeAccount;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetPaymentMethods; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetPaymentMethodsFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetPaymentMethodsFailed_Title.ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _upgradeAccount.CreditCardPaymentMethodAvailable = Convert.ToBoolean(request.getNumber() & (1 << (int)MPaymentMethod.PAYMENT_METHOD_CREDIT_CARD));
                //bool balance = Convert.ToBoolean(request.getNumber() & (1 << (int)MPaymentMethod.PAYMENT_METHOD_BALANCE));

                _upgradeAccount.AvailablePurchases = _accountDetails.IsFreeAccount && _upgradeAccount.CreditCardPaymentMethodAvailable;
            });
        }

        #endregion
    }
}
