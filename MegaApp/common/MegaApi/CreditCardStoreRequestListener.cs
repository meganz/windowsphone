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
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CreditCardStoreRequestListener : BaseRequestListener
    {
        private readonly Product _selectedProduct;

        #region Base Properties

        public CreditCardStoreRequestListener(Product selectedProduct)
        {
            _selectedProduct = selectedProduct;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.CreditCardStore; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.CreditCardStoreFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.CreditCardStoreFailed_Title.ToUpper(); }
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
                        
            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                    OnSuccesAction(api, request);
                    break;
                case MErrorType.API_EEXIST:
                    OnSuccesAction(api, request);
                    break;
                default:
                    if (ShowErrorMessage)
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => MessageBox.Show(ErrorMessage,ErrorMessageTitle, MessageBoxButton.OK));
                    break;
            }            
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            api.upgradeAccount(_selectedProduct.Handle, (int)MPaymentMethod.PAYMENT_METHOD_CREDIT_CARD);
        }

        #endregion
    }
}
