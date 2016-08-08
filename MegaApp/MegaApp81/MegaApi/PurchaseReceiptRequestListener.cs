using System;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class PurchaseReceiptRequestListener: BaseRequestListener
    {
        protected override string ProgressMessage
        {
            get { return ProgressMessages.ValidatePurchase; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.PurchaseValidationFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.PurchaseValidationFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.PurchaseSucceeded; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.PurchaseSucceeded_Title; }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return false; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }
    }
}
