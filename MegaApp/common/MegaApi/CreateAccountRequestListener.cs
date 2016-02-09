using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CreateAccountRequestListener : BaseRequestListener
    {
        private readonly CreateAccountViewModel _createAccountViewModel;
        private readonly LoginPage _loginPage;

        public CreateAccountRequestListener(CreateAccountViewModel createAccountViewModel, LoginPage loginPage)
        {
            _createAccountViewModel = createAccountViewModel;
            _loginPage = loginPage;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.CreateAccount; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.CreateAccountFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.CreateAccountFailed_Title.ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.ConfirmNeeded; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.ConfirmNeeded_Title.ToUpper(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; } //Shown when navigates to the "InitTourPage"
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
            get { return typeof(InitTourPage); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.CreateAccount; }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
                
                _createAccountViewModel.ControlState = true;
                _loginPage.SetApplicationBar(true);
            });

            base.onRequestFinish(api, request, e);
        }

        #endregion
    }
}
