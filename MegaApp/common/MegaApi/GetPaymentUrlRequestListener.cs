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
using Microsoft.Phone.Tasks;

namespace MegaApp.MegaApi
{
    class GetPaymentUrlRequestListener : BaseRequestListener
    {
        private readonly MPaymentMethod _paymentMethodType;

        public GetPaymentUrlRequestListener(MPaymentMethod paymentMethodType)
        {            
            _paymentMethodType = paymentMethodType;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetPaymentUrl; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetPaymentUrlFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetPaymentUrlFailed_Title; }
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
                var webBrowserTask = new WebBrowserTask();

                switch(_paymentMethodType)
                {
                    case MPaymentMethod.PAYMENT_METHOD_CENTILI:
                        webBrowserTask.Uri = new Uri("https://www.centili.com/widget/WidgetModule?api=9e8eee856f4c048821954052a8d734ac&clientid=" + request.getLink());
                        break;

                    case MPaymentMethod.PAYMENT_METHOD_FORTUMO:
                        webBrowserTask.Uri = new Uri("http://fortumo.com/mobile_payments/f250460ec5d97fd27e361afaa366db0f?cuid=" + request.getLink());
                        break;
                }
                
                webBrowserTask.Show();
            });
        }

        #endregion
    }
}
