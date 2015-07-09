using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Resources;

namespace MegaApp.Models
{
    class CreditCardPaymentViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;

        public CreditCardPaymentViewModel(MegaSDK megaSdk)
        {
            this.ControlState = true;
            this._megaSdk = megaSdk;

            this.ProductSelectionIsEnabled = true;
            this.CreditCardPaymentIsEnabled = false;
            this.BillingDetails = new BillingDetails();
            this.CreditCard = new CreditCard();
        }

        #region Methods

        public void DoPayment()
        {
            if(CheckInputParameters())
            {                
                if (String.IsNullOrWhiteSpace(BillingDetails.Address2))
                {
                    _megaSdk.creditCardStore(BillingDetails.Address1, " ", BillingDetails.City,
                        BillingDetails.Province, BillingDetails.CountryCode, BillingDetails.PostalCode,
                        CreditCard.FirstName, CreditCard.LastName, CreditCard.Number, CreditCard.ExpireMonth,
                        CreditCard.ExpireYear, CreditCard.CV2, new CreditCardStoreRequestListener(SelectedProduct));
                }                        
                else
                {
                    _megaSdk.creditCardStore(BillingDetails.Address1, BillingDetails.Address2, BillingDetails.City,
                        BillingDetails.Province, BillingDetails.CountryCode, BillingDetails.PostalCode,
                        CreditCard.FirstName, CreditCard.LastName, CreditCard.Number, CreditCard.ExpireMonth,
                        CreditCard.ExpireYear, CreditCard.CV2, new CreditCardStoreRequestListener(SelectedProduct));
                }             
            }
        }

        private bool CheckInputParameters()
        {
            // Check if all the needed fields have been filled.
            if (String.IsNullOrWhiteSpace(BillingDetails.Address1) || String.IsNullOrWhiteSpace(BillingDetails.City) ||
                String.IsNullOrWhiteSpace(BillingDetails.Province) || String.IsNullOrWhiteSpace(BillingDetails.CountryCode) ||
                String.IsNullOrWhiteSpace(BillingDetails.PostalCode) || String.IsNullOrWhiteSpace(CreditCard.FirstName) ||
                String.IsNullOrWhiteSpace(CreditCard.LastName) || String.IsNullOrWhiteSpace(CreditCard.Number) ||
                String.IsNullOrWhiteSpace(CreditCard.ExpireMonth) || String.IsNullOrWhiteSpace(CreditCard.ExpireYear) ||
                String.IsNullOrWhiteSpace(CreditCard.CV2))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppMessages.RequiredFieldsCreditCardPayment,
                        AppMessages.RequiredFields_Title.ToUpper(), MessageBoxButton.OK);
                });
                return false;
            }                
            
            // Check if numeric fields have been filled with numbers.
            ulong ulong_temp; int int_temp;
            if (!ulong.TryParse(CreditCard.Number, out ulong_temp) || !int.TryParse(CreditCard.ExpireMonth, out int_temp) ||
                !int.TryParse(CreditCard.ExpireYear, out int_temp) || !int.TryParse(CreditCard.CV2, out int_temp))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppMessages.WrongDataFormatCreditCardPayment,
                        AppMessages.WrongDataFormat_Title.ToUpper(), MessageBoxButton.OK);
                });
                return false;
            }

            return true;
        }

        #endregion

        #region Properties

        private ProductBase _plan;
        public ProductBase Plan
        {
            get { return _plan; }
            set
            {
                _plan = value;
                OnPropertyChanged("Plan");
            }
        }

        private Product _productMonthly;
        public Product ProductMonthly
        {
            get { return _productMonthly; }
            set
            {
                _productMonthly = value;
                OnPropertyChanged("ProductMonthly");
            }
        }

        private Product _productAnnualy;
        public Product ProductAnnualy
        {
            get { return _productAnnualy; }
            set
            {
                _productAnnualy = value;
                OnPropertyChanged("ProductAnnualy");
            }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
            }
        }
                
        private bool _productSelectionIsEnabled;
        public bool ProductSelectionIsEnabled
        {
            get { return _productSelectionIsEnabled; }
            set
            {
                _productSelectionIsEnabled = value;
                OnPropertyChanged("ProductSelectionIsEnabled");
            }
        }

        private bool _paymentMethodSelectionIsEnabled;
        public bool PaymentMethodSelectionIsEnabled
        {
            get { return _paymentMethodSelectionIsEnabled; }
            set
            {
                _paymentMethodSelectionIsEnabled = value;
                OnPropertyChanged("PaymentMethodSelectionIsEnabled");
            }
        }

        private bool _creditCardPaymentIsEnabled;
        public bool CreditCardPaymentIsEnabled
        {
            get { return _creditCardPaymentIsEnabled; }
            set
            {
                _creditCardPaymentIsEnabled = value;
                OnPropertyChanged("CreditCardPaymentIsEnabled");
            }
        }

        private BillingDetails _billingDetails;
        public BillingDetails BillingDetails
        {
            get { return _billingDetails; }
            set
            {
                _billingDetails = value;
                OnPropertyChanged("BillingDetails");
            }
        }

        private CreditCard _creditCard;
        public CreditCard CreditCard
        {
            get { return _creditCard; }
            set
            {
                _creditCard = value;
                OnPropertyChanged("CreditCard");
            }
        }

        #endregion
    }    
}
