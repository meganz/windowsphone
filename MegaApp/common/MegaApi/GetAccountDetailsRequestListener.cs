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

        public GetAccountDetailsRequestListener(AccountDetailsViewModel accountDetails)
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

                        if (_accountDetails.AccountType != MAccountType.ACCOUNT_TYPE_FREE)
                        {
                            // Get the expiration time for the current PRO status
                            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            DateTime date = start.AddSeconds(request.getMAccountDetails().getProExpiration());
                            _accountDetails.ProExpiration = date.ToString("dd-MM-yyyy");
                        }

                        switch (_accountDetails.AccountType)
                        {
                            case MAccountType.ACCOUNT_TYPE_FREE:
                                _accountDetails.IsFreeAccount = true;
                                _accountDetails.AccountTypeText = UiResources.AccountTypeFree;
                                _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                break;
                            case MAccountType.ACCOUNT_TYPE_LITE:
                                _accountDetails.AccountTypeText = UiResources.AccountTypeLite;
                                //_accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_free" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);                        
                                break;
                            case MAccountType.ACCOUNT_TYPE_PROI:
                                _accountDetails.AccountTypeText = UiResources.AccountTypePro1;
                                _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro1" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                break;
                            case MAccountType.ACCOUNT_TYPE_PROII:
                                _accountDetails.AccountTypeText = UiResources.AccountTypePro2;
                                _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro2" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                break;
                            case MAccountType.ACCOUNT_TYPE_PROIII:
                                _accountDetails.AccountTypeText = UiResources.AccountTypePro3;
                                _accountDetails.AccountTypeUri = new Uri("/Assets/Images/small_pro3" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
                                break;
                        }

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
