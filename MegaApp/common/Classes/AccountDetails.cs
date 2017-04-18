using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    public class AccountDetailsViewModel : UserDataViewModel
    {
        // Event triggered when changes the subscriptions number with credit card
        public event EventHandler CreditCardSubscriptionsChanged;

        public AccountDetailsViewModel()
        {
            IsFreeAccount = true; // Default value
            IsDataLoaded = false; // Default value

            CacheSize = AppService.GetAppCacheSize();
        }

        private bool _isDataLoaded;
        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set 
            {
                _isDataLoaded = value;
                OnPropertyChanged("IsDataLoaded");
            }
        }

        private ulong _totalSpace;
        public ulong TotalSpace
        {
            get { return _totalSpace; }
            set
            {
                _totalSpace = value;
                CalculateFreeSpace();
                OnPropertyChanged("TotalSpace");
            }
        }

        private ulong _totalSpaceSize;
        public ulong TotalSpaceSize
        {
            get { return _totalSpaceSize; }
            set
            {
                _totalSpaceSize = value;                
                OnPropertyChanged("TotalSpaceSize");
            }
        }

        private string _totalSpaceUnits;
        public string TotalSpaceUnits
        {
            get { return _totalSpaceUnits; }
            set
            {
                _totalSpaceUnits = value;
                OnPropertyChanged("TotalSpaceUnits");
            }
        }

        private ulong _usedSpace;
        public ulong UsedSpace
        {
            get { return _usedSpace; }
            set
            {
                _usedSpace = value;
                CalculateFreeSpace();
                OnPropertyChanged("UsedSpace");
            }
        }

        private ulong _freeSpace;
        public ulong FreeSpace
        {
            get { return _freeSpace; }
            set
            {
                _freeSpace = value;
                OnPropertyChanged("FreeSpace");
            }
        }

        private ulong _cacheSize;
        public ulong CacheSize
        {
            get { return _cacheSize; }
            set
            {
                _cacheSize = value;
                OnPropertyChanged("CacheSize");
            }
        }

        public ulong MaxCache
        {
            //get { return 100UL.FromMBToBytes(); }
            get { return AppService.MaxMemoryUsage(); }
        }

        private MAccountType _accountType;
        public MAccountType AccountType
        {
            get { return _accountType; }
            set
            {
                _accountType = value;
                OnPropertyChanged("AccountType");
            }
        }

        private string _accountTypeText;
        public string AccountTypeText
        {
            get { return _accountTypeText; }
            set
            {
                _accountTypeText = value;                
                OnPropertyChanged("AccountTypeText");

                PurchaseAccountTypeText = String.Format(UiResources.AccountPurchased, 
                    _accountTypeText.ToUpper());
            }
        }

        private Uri _accountTypeUri;
        public Uri AccountTypeUri
        {
            get { return _accountTypeUri; }
            set
            {
                _accountTypeUri = value;
                OnPropertyChanged("AccountTypeUri");
            }
        }

        private String _proExpirationDate;
        public String ProExpirationDate
        {
            get { return _proExpirationDate; }
            set
            {
                _proExpirationDate = value;
                OnPropertyChanged("ProExpirationDate");
            }
        }

        private bool _isFreeAccount;
        public bool IsFreeAccount
        {
            get { return _isFreeAccount; }
            set
            {
                _isFreeAccount = value;
                OnPropertyChanged("IsFreeAccount");
                OnPropertyChanged("IsProAccount");
            }
        }

        public bool IsProAccount
        {
            get { return !_isFreeAccount; }
        }
        
        private bool _isValidSubscription;
        public bool IsValidSubscription
        {
            get { return _isValidSubscription; }
            set
            {
                _isValidSubscription = value;
                OnPropertyChanged("IsValidSubscription");
            }
        }

        private String _subscriptionRenewDate;
        public String SubscriptionRenewDate
        {
            get { return _subscriptionRenewDate; }
            set
            {
                _subscriptionRenewDate = value;
                OnPropertyChanged("SubscriptionRenewDate");
            }
        }

        private String _subscriptionCycle;
        public String SubscriptionCycle
        {
            get { return _subscriptionCycle; }
            set
            {
                _subscriptionCycle = value;                
                OnPropertyChanged("SubscriptionCycle");

                if(String.Compare(_subscriptionCycle,"1 M") == 0)
                {
                    PurchaseAccountRenewalText = String.Format(UiResources.AccountRenewalNotice,
                        UiResources.Monthly.ToLower());
                }
                else if (String.Compare(_subscriptionCycle, "1 Y") == 0)
                {
                    PurchaseAccountRenewalText = String.Format(UiResources.AccountRenewalNotice,
                        UiResources.Annual.ToLower());
                }                
            }
        }

        private ulong _creditCardSubscriptions;
        public ulong CreditCardSubscriptions
        {
            get { return _creditCardSubscriptions; }
            set
            {
                _creditCardSubscriptions = value;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (CreditCardSubscriptionsChanged != null)
                        CreditCardSubscriptionsChanged.Invoke(this, EventArgs.Empty);
                });
                OnPropertyChanged("CreditCardSubscriptions");
            }
        }

        private void CalculateFreeSpace()
        {
            if (TotalSpace < 1) return;

            if (UsedSpace <= TotalSpace)
                FreeSpace = TotalSpace - UsedSpace;                
            else
                FreeSpace = 0;                            
        }

        public void CreateDataPoints()
        {
            var accountDataPoints = new List<AccountDataPoint>
            {
                new AccountDataPoint() {Label = UiResources.UsedSpace, Value = UsedSpace},
                new AccountDataPoint() {Label = UiResources.FreeSpace, Value = FreeSpace}                
            };
            PieChartCollection = accountDataPoints;
        }

        private IEnumerable<AccountDataPoint> _pieChartCollection;
        public IEnumerable<AccountDataPoint> PieChartCollection
        {
            get { return _pieChartCollection; }
            set
            {
                _pieChartCollection = value;
                OnPropertyChanged("PieChartCollection");
            }
        }

        private String _purchaseAccountTypeText;
        public String PurchaseAccountTypeText
        {
            get { return _purchaseAccountTypeText; }
            set
            {
                _purchaseAccountTypeText = value;
                OnPropertyChanged("PurchaseAccountTypeText");
            }
        }

        private String _purchaseAccountRenewalText;
        public String PurchaseAccountRenewalText
        {
            get { return _purchaseAccountRenewalText; }
            set
            {
                _purchaseAccountRenewalText = value;
                OnPropertyChanged("PurchaseAccountRenewalText");
            }
        }
    }
}
