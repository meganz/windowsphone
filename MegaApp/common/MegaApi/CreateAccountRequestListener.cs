using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Enums;
using MegaApp.Classes;
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
            get { return ProgressMessages.PM_CreateAccount; }
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

        public override void onRequestStart(MegaSDK api, MRequest request)
        {            
            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
                base.onRequestStart(api, request);
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
                
                _createAccountViewModel.ControlState = true;
                _loginPage.SetApplicationBar(true);                
            });
            
            if (request.getType() == MRequestType.TYPE_QUERY_SIGNUP_LINK)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Valid and operative #newsignup link
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            _createAccountViewModel.Email = request.getEmail();

                            if (!String.IsNullOrWhiteSpace(_createAccountViewModel.Email))
                                this._loginPage.txtEmail_CreateAccount.IsReadOnly = true;
                        });
                        break;

                    case MErrorType.API_EARGS: // Invalid #newsignup link
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            new CustomMessageDialog(
                                AppMessages.AM_InvalidLink,
                                AppMessages.AM_NewSignUpInvalidLink,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        });
                        break;

                    default: // Default error processing
                        base.onRequestFinish(api, request, e);
                        break;
                }
            }
            
            if (request.getType() == MRequestType.TYPE_CREATE_ACCOUNT)
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull create account process
                        base.onRequestFinish(api, request, e);
                        break;

                    case MErrorType.API_EEXIST: // Email already registered
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            new CustomMessageDialog(
                                ErrorMessageTitle,
                                AppMessages.AM_EmailAlreadyRegistered,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        });
                        break;

                    default: // Default error processing
                        base.onRequestFinish(api, request, e);
                        break;
                }
            }
        }

        #endregion
    }
}
