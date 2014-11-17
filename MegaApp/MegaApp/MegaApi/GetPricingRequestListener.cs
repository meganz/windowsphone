using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class GetPricingRequestListener : BaseRequestListener
    {
        private readonly AccountDetailsViewModel _accountDetails;

        public GetPricingRequestListener(AccountDetailsViewModel accountDetails)
        {
            _accountDetails = accountDetails;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetAccountDetails; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetAccountDetailsFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetAccountDetailsFailed_Title; }
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

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            _accountDetails.Products.Clear();

            int numberOfProducts = request.getPricing().getNumProducts();

            for (int i = 0; i < numberOfProducts; i++)
            {
                var accountType = (MAccountType) Enum.Parse(typeof (MAccountType),
                    request.getPricing().getProLevel(i).ToString());

                if(accountType == _accountDetails.AccountType)
                    continue;

                var product = new Product
                {
                    Name = accountType.ToString(),
                    Amount = request.getPricing().getAmount(i),
                    Currency = request.getPricing().getCurrency(i),
                    GbStorage = request.getPricing().getGBStorage(i),
                    GbTransfer = request.getPricing().getGBTransfer(i),
                    Months = request.getPricing().getMonths(i),
                    Handle = request.getPricing().getHandle(i)
                };

                _accountDetails.Products.Add(product);
            }
        }

        #endregion
    }
}
