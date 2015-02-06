using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

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
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _accountDetails.Products.Clear();

                int numberOfProducts = request.getPricing().getNumProducts();

                for (int i = 0; i < numberOfProducts; i++)
                {
                    var accountType = (MAccountType) Enum.Parse(typeof (MAccountType),
                        request.getPricing().getProLevel(i).ToString());

                    if (accountType == _accountDetails.AccountType)
                        continue;

                    var product = new Product
                    {
                        Amount = request.getPricing().getAmount(i),
                        Currency = request.getPricing().getCurrency(i),
                        GbStorage = request.getPricing().getGBStorage(i),
                        GbTransfer = request.getPricing().getGBTransfer(i),
                        Months = request.getPricing().getMonths(i),
                        Handle = request.getPricing().getHandle(i)
                    };

                    switch (accountType)
                    {
                        case MAccountType.ACCOUNT_TYPE_FREE:
                            product.Name = UiResources.AccountTypeFree;
                            product.ProductUri = new Uri("/Assets/Images/pro1" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                            break;
                        case MAccountType.ACCOUNT_TYPE_PROI:
                            product.Name = UiResources.AccountTypePro1;
                            product.ProductUri = new Uri("/Assets/Images/pro1" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                            break;
                        case MAccountType.ACCOUNT_TYPE_PROII:
                            product.Name = UiResources.AccountTypePro2;
                            product.ProductUri = new Uri("/Assets/Images/pro2" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                            break;
                        case MAccountType.ACCOUNT_TYPE_PROIII:
                            product.Name = UiResources.AccountTypePro3;
                            product.ProductUri = new Uri("/Assets/Images/pro3" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                            break;
                    }

                    _accountDetails.Products.Add(product);
                }
            });
        }

        #endregion
    }
}
