using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetUserDataRequestListener : BaseRequestListener
    {
        #region Override Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetUserData; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetUserDataFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetUserDataFailed_Title; }
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

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
                {
                    switch (request.getParamType())
                    {
                        case (int)MUserAttrType.USER_ATTR_FIRSTNAME:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                AccountService.AccountDetails.Firstname = request.getText());
                            break;

                        case (int)MUserAttrType.USER_ATTR_LASTNAME:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                AccountService.AccountDetails.Lastname = request.getText());
                            break;
                    }
                }                
            }
            else
            {
                if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
                {
                    if (request.getParamType() == (int)MUserAttrType.USER_ATTR_FIRSTNAME)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            AccountService.AccountDetails.Firstname = UiResources.MyAccount);                            
                    }
                }
            }
        }

        #endregion
    }
}
