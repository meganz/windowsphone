using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Services;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    class AccountDetailsViewModel: BaseViewModel
    {
        public AccountDetailsViewModel()
        {
            Products = new ObservableCollection<Product>();
            CacheSize = AppService.GetAppCacheSize();
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
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
        

        private void CalculateFreeSpace()
        {
            if (TotalSpace < 1) return;
            FreeSpace = TotalSpace - UsedSpace;
        }

        public void CreateDataPoints()
        {
            var accountDataPoints = new List<AccountDataPoint>
            {
                new AccountDataPoint() {Label = "Free space", Value = FreeSpace},
                new AccountDataPoint() {Label = "Used space", Value = UsedSpace}
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


        public ObservableCollection<Product> Products { get; set; }
       
    }
}
