using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class GetAccountDetailsRequestListener : BaseRequestListener
    {
        private event EventHandler _getAccountDetailsFinish;

        public GetAccountDetailsRequestListener(EventHandler getAccountDetailsFinish = null)
        {
            _getAccountDetailsFinish = getAccountDetailsFinish;
        }

        #region Override Properties

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

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            switch(request.getType())
            {
                case MRequestType.TYPE_ACCOUNT_DETAILS:

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        accountDetails.TotalSpace = request.getMAccountDetails().getStorageMax();
                        accountDetails.TotalSpaceSize = accountDetails.TotalSpace.ToReadableSize();
                        accountDetails.TotalSpaceUnits = accountDetails.TotalSpace.ToReadableUnits();
                        accountDetails.UsedSpace = request.getMAccountDetails().getStorageUsed();
                        accountDetails.CreateDataPoints();
                        accountDetails.AccountType = request.getMAccountDetails().getProLevel();

                        if (accountDetails.AccountType == MAccountType.ACCOUNT_TYPE_FREE)
                        {
                            accountDetails.IsFreeAccount = true;
                            accountDetails.AccountTypeText = AppResources.AccountTypeFree;
                            accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                        }
                        else
                        {
                            switch (accountDetails.AccountType)
                            {
                                case MAccountType.ACCOUNT_TYPE_LITE:
                                    accountDetails.AccountTypeText = AppResources.AccountTypeLite;
                                    //accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);                        
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROI:
                                    accountDetails.AccountTypeText = AppResources.AccountTypePro1;
                                    accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro1" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROII:
                                    accountDetails.AccountTypeText = AppResources.AccountTypePro2;
                                    accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro2" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROIII:
                                    accountDetails.AccountTypeText = AppResources.AccountTypePro3;
                                    accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro3" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                            }

                            accountDetails.IsFreeAccount = false;

                            DateTime date;
                            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                            // If there is a valid subscription get the renew time
                            if (request.getMAccountDetails().getSubscriptionStatus() == MSubscriptionStatus.SUBSCRIPTION_STATUS_VALID)
                            {
                                try
                                {
                                    if (request.getMAccountDetails().getSubscriptionRenewTime() != 0)
                                        date = start.AddSeconds(Convert.ToDouble(request.getMAccountDetails().getSubscriptionRenewTime()));
                                    else
                                        date = start.AddSeconds(Convert.ToDouble(request.getMAccountDetails().getProExpiration()));

                                    accountDetails.SubscriptionRenewDate = date.ToString("dd-MM-yyyy");
                                }
                                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }

                                accountDetails.SubscriptionCycle = request.getMAccountDetails().getSubscriptionCycle();
                                accountDetails.IsValidSubscription = true;
                            }
                            // Else get the expiration time for the current PRO status
                            else
                            {
                                try 
                                {
                                    date = start.AddSeconds(Convert.ToDouble(request.getMAccountDetails().getProExpiration()));
                                    accountDetails.ProExpirationDate = date.ToString("dd-MM-yyyy");
                                }
                                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }

                                accountDetails.IsValidSubscription = false;
                            }                            
                        }

                        if (_getAccountDetailsFinish != null)
                            _getAccountDetailsFinish.Invoke(this, EventArgs.Empty);
                    });

                    break;

                case MRequestType.TYPE_CREDIT_CARD_QUERY_SUBSCRIPTIONS:
                    accountDetails.CreditCardSubscriptions = request.getNumber();
                    break;
            }            
        }

        #endregion

        private AccountDetailsViewModel accountDetails
        {
            get { return AccountService.AccountDetails; }
        }
    }
}
