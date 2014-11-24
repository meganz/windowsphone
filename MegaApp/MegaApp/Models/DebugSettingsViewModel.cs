using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Models;

namespace MegaApp.Classes
{
    class DebugSettingsViewModel: BaseViewModel
    {
        private bool _isDebugMode;
        public bool IsDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                _isDebugMode = value;
                OnPropertyChanged("IsDebugMode");
            }
        }
        private bool _showMemoryInformation;
        public bool ShowMemoryInformation
        {
            get { return _showMemoryInformation; }
            set
            {
                _showMemoryInformation = value;
                OnPropertyChanged("ShowMemoryInformation");
            }
        }
    }
}
