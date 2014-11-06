using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace MegaApp.Classes
{
    public class MemoryInformation
    {
        public ulong AppMemoryUsage { get; set; }
        public ulong AppMemoryLimit { get; set; }
        public ulong AppMemoryPeak { get; set; }
        public ulong DeviceMemory { get; set; }

        public ulong AppMemorySpace
        {
            get { return AppMemoryLimit - AppMemoryUsage; }
        }
    }
}
