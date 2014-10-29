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
        private Timer _timer;

        public MemoryControl()
        {
            InitializeComponent();
        }

        private void TimerCallback(object state)
        {
            MemoryInformation memInfo = AppService.GetAppMemoryUsage();

            Dispatcher.BeginInvoke(() =>
            {
                TxtAppMemory.Text = String.Format("RAM: {0}", memInfo.AppMemoryUsage.ToStringAndSuffix());
                TxtAppMemoryLimit.Text = String.Format("MAX: {0}", memInfo.AppMemoryLimit.ToStringAndSuffix());
                TxtAppMemoryPeak.Text = String.Format("PEAK: {0}", memInfo.AppMemoryPeak.ToStringAndSuffix());
                TxtDeviceMemory.Text = String.Format("PHONE: {0}", memInfo.DeviceMemory.ToStringAndSuffix());
            });
        }

        public void StartMemoryCounter()
        {
            _timer = new Timer(TimerCallback, null, new TimeSpan(0), new TimeSpan(0, 0, 3));
        }

        public void StopMemoryCounter()
        {
            _timer.Dispose();
            _timer = null;
        }
    }
}
