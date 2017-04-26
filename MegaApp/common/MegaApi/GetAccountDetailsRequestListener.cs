using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetAccountDetailsRequestListener : BaseRequestListener
    {
        private readonly AccountDetailsViewModel _accountDetails;
        private event EventHandler _getAccountDetailsFinish;

        public GetAccountDetailsRequestListener(AccountDetailsViewModel accountDetails, 
            EventHandler getAccountDetailsFinish = null)
        {
            _accountDetails = accountDetails;
            _getAccountDetailsFinish = getAccountDetailsFinish;
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
            switch(request.getType())
            {
                case MRequestType.TYPE_ACCOUNT_DETAILS:

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _accountDetails.TotalSpace = request.getMAccountDetails().getStorageMax();
                        _accountDetails.TotalSpaceSize = _accountDetails.TotalSpace.ToReadableSize();
                        _accountDetails.TotalSpaceUnits = _accountDetails.TotalSpace.ToReadableUnits();
                        _accountDetails.UsedSpace = request.getMAccountDetails().getStorageUsed();
                        _accountDetails.CreateDataPoints();
                        _accountDetails.AccountType = request.getMAccountDetails().getProLevel();

                        if (_accountDetails.AccountType == MAccountType.ACCOUNT_TYPE_FREE)
                        {
                            _accountDetails.IsFreeAccount = true;
                            _accountDetails.AccountTypeText = AppResources.AccountTypeFree;
                            _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                        }
                        else
                        {
                            switch (_accountDetails.AccountType)
                            {
                                case MAccountType.ACCOUNT_TYPE_LITE:
                                    _accountDetails.AccountTypeText = AppResources.AccountTypeLite;
                                    //_accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);                        
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROI:
                                    _accountDetails.AccountTypeText = AppResources.AccountTypePro1;
                                    _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro1" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROII:
                                    _accountDetails.AccountTypeText = AppResources.AccountTypePro2;
                                    _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro2" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                                case MAccountType.ACCOUNT_TYPE_PROIII:
                                    _accountDetails.AccountTypeText = AppResources.AccountTypePro3;
                                    _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro3" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                    break;
                            }

                            _accountDetails.IsFreeAccount = false;

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

                                    _accountDetails.SubscriptionRenewDate = date.ToString("dd-MM-yyyy");
                                }
                                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }                                                                
                                
                                _accountDetails.SubscriptionCycle = request.getMAccountDetails().getSubscriptionCycle();
                                _accountDetails.IsValidSubscription = true;
                            }
                            // Else get the expiration time for the current PRO status
                            else
                            {
                                try 
                                {
                                    date = start.AddSeconds(Convert.ToDouble(request.getMAccountDetails().getProExpiration()));                                    
                                    _accountDetails.ProExpirationDate = date.ToString("dd-MM-yyyy");
                                }
                                catch (ArgumentOutOfRangeException) { /* Do nothing*/ }
                                
                                _accountDetails.IsValidSubscription = false;
                            }                            
                        }

                        if (_getAccountDetailsFinish != null)
                            _getAccountDetailsFinish.Invoke(this, EventArgs.Empty);
                    });

                    break;

                case MRequestType.TYPE_CREDIT_CARD_QUERY_SUBSCRIPTIONS:
                    _accountDetails.CreditCardSubscriptions = request.getNumber();
                    break;
            }            
        }

        #endregion
    }
}
