using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Resources;

namespace MegaApp.Models
{
    class DebugSettingsViewModel: BaseViewModel
    {
        public DebugSettingsViewModel()
        {
            ShowMemoryInformation = false;
        }

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
                ShowMemoryInformationText = _showMemoryInformation ? UiResources.On : UiResources.Off;
                OnPropertyChanged("ShowMemoryInformation");
            }
        }

        private string _showMemoryInformationText;
        public string ShowMemoryInformationText
        {
            get { return _showMemoryInformationText; }
            set
            {
                _showMemoryInformationText = value;
                OnPropertyChanged("ShowMemoryInformationText");
            }
        }
    }
}
