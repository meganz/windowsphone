using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.UserControls
{
    public partial class MemoryControl : UserControl
    {
        private AppMemoryController _appMemoryController;

        public MemoryControl()
        {
            InitializeComponent();
        }

        public void StartMemoryCounter()
        {
            _appMemoryController = new AppMemoryController(100UL.FromMBToBytes());
            _appMemoryController.DiagnosticUpdate += AppMemoryControllerOnDiagnosticUpdate;
            _appMemoryController.StartDiagnostics(new TimeSpan(0), new TimeSpan(0, 0, 3));
        }

        private void AppMemoryControllerOnDiagnosticUpdate(object sender, MemoryInformation memInfo)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TxtAppMemory.Text = String.Format("RAM: {0}", memInfo.AppMemoryUsage.ToStringAndSuffix(2));
                TxtAppMemoryLimit.Text = String.Format("MAX: {0}", memInfo.AppMemoryLimit.ToStringAndSuffix(2));
                TxtAppMemoryPeak.Text = String.Format("PEAK: {0}", memInfo.AppMemoryPeak.ToStringAndSuffix(2));
                TxtDeviceMemory.Text = String.Format("PHONE: {0}", memInfo.DeviceMemory.ToStringAndSuffix(2));
            });
        }

        public void StopMemoryCounter()
        {
            if (_appMemoryController == null) return;

            _appMemoryController.StopDiagnostics();
            _appMemoryController.Dispose();
            _appMemoryController = null;
        }
    }
}
