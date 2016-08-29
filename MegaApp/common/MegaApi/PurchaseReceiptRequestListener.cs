using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class PurchaseReceiptRequestListener: BaseRequestListener
    {
        private readonly string _receipt;

        /// <summary>
        /// Create MEGA listener to act on a subscription purchase
        /// </summary>
        /// <param name="receipt">The official Windows Store receipt</param>
        public PurchaseReceiptRequestListener(string receipt)
        {
            _receipt = receipt;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_ValidatePurchase; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.AM_PurchaseValidationFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.AM_PurchaseValidationFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.AM_PurchaseSucceeded; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.AM_PurchaseSucceeded_Title; }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
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

        private string GetUniqueReceiptId(string receipt)
        {
            var xDoc = XDocument.Parse(receipt, LoadOptions.None);
            return xDoc.Root.Descendants().First().Attribute("Id").Value;
        }

        private void SaveUniqueReceiptId(string receiptId)
        {
            var currentIds = SettingsService.LoadSetting(SettingsResources.Receipts, string.Empty);
            currentIds += receiptId + ";";
            SettingsService.SaveSetting(SettingsResources.Receipts, currentIds);
        }
        

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        SuccessMessageTitle,
                        SuccessMessage,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });

                try
                {
                    SaveUniqueReceiptId(GetUniqueReceiptId(_receipt));
                }
                catch
                {
                    // On error do nothing. The app will just retry to validate the receipt on app start and
                    // add it to the settings if succeeded or -12 (already exists) error code returns
                }
            }
            else if (e.getErrorCode() == MErrorType.API_EEXIST)
            {
                // Current receipt is already validate on MEGA license server
                // Add receipt id to the saved list in the settings
                try
                {
                    SaveUniqueReceiptId(GetUniqueReceiptId(_receipt));
                }
                catch
                {
                    // On error do nothing. The app will just retry to validate the receipt on app start and
                    // Add it to settings if succeeded or -12 (already exists) error code returns
                }
            }
            else if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
            {
                if (ShowErrorMessage)
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            String.Format(ErrorMessage, e.getErrorString()),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });
            }
        }
    }
}
