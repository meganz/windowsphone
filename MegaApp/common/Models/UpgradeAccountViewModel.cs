using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Classes;

namespace MegaApp.Models
{
    class UpgradeAccountViewModel : BaseViewModel
    {
        public UpgradeAccountViewModel()
        {
            Plans = new ObservableCollection<ProductBase>();
            Products = new ObservableCollection<Product>();
        }

        public ObservableCollection<ProductBase> Plans { get; set; }
        public ObservableCollection<Product> Products { get; set; }

        private Product _productPurchased;
        public Product ProductPurchased
        {
            get { return _productPurchased; }
            set
            {
                _productPurchased = value;
                OnPropertyChanged("ProductPurchased");
            }
        }

        private bool _centiliPaymentMethodAvailable;
        public bool CentiliPaymentMethodAvailable
        {
            get { return _centiliPaymentMethodAvailable; }
            set
            {
                _centiliPaymentMethodAvailable = value;
                OnPropertyChanged("CentiliPaymentMethodAvailable");
            }
        }

        private bool _fortumoPaymentMethodAvailable;
        public bool FortumoPaymentMethodAvailable
        {
            get { return _fortumoPaymentMethodAvailable; }
            set
            {
                _fortumoPaymentMethodAvailable = value;
                OnPropertyChanged("FortumoPaymentMethodAvailable");
            }
        }        

        private bool _creditCardPaymentMethodAvailable;
        public bool CreditCardPaymentMethodAvailable
        {
            get { return _creditCardPaymentMethodAvailable; }
            set
            {
                _creditCardPaymentMethodAvailable = value;
                OnPropertyChanged("CreditCardPaymentMethodAvailable");
            }
        }
    }
}
