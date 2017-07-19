using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using MegaApp.Models;

namespace MegaApp.Services
{
    static class DebugService
    {
        private static DebugSettingsViewModel _debugSettings;
        public static DebugSettingsViewModel DebugSettings 
        { 
            get
            {
                if (_debugSettings != null) return _debugSettings;
                _debugSettings = new DebugSettingsViewModel();
                return _debugSettings;
            }
        }
    }
}
