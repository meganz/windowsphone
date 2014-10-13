using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Models;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    class AccountDetailsModel: BaseViewModel
    {
        public AccountDetailsModel()
        {
            PieChartCollection = new ObservableCollection<ulong>();
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

        private void CalculateFreeSpace()
        {
            if (TotalSpace < 1) return;
            FreeSpace = TotalSpace - UsedSpace;
        }

        public void CreateDataPoints()
        {
            PieChartCollection.Clear();
            PieChartCollection.Add(UsedSpace);
            PieChartCollection.Add(FreeSpace);
        }

        private ObservableCollection<ulong> PieChartCollection { get; set; }
       
    }
}
