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

namespace MegaApp.MegaApi
{
    class GetUserAvatarRequestListener : BaseRequestListener
    {
        private readonly AccountDetailsViewModel _accountDetails;

        public GetUserAvatarRequestListener(AccountDetailsViewModel accountDetails)
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
            
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if(e.getErrorCode() == MErrorType.API_OK)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _accountDetails.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute);
                });
            }
            else if (e.getErrorCode() == MErrorType.API_ESID)
            {
                base.onRequestFinish(api, request, e);
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _accountDetails.AvatarUri = null;
                });
            }
        }

        #endregion
    }
}
