using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MegaApp.Models;
using MegaApp.Services;

namespace MegaApp.Classes
{
    public class AppMemoryController : BaseViewModel, IDisposable
    {
        private Timer _memoryTimer;

        public AppMemoryController(ulong memoryThreshold)
        {
            MemoryThreshold = memoryThreshold;
        }

        ~AppMemoryController()
        {
            Dispose(false); //I am *not* calling you from Dispose, it's *not* safe
        }
       
        #region Methods

        public void StartDiagnostics(TimeSpan dueTime, TimeSpan period)
        {
            _memoryTimer = new Timer(TimerCallback, null, dueTime, period);
            this.IsBusy = true;
        }

        public void StopDiagnostics()
        {
            this.IsBusy = false;
            _memoryTimer.Dispose();
            _memoryTimer = null;
        }

        public MemoryInformation GetCurrentMemoryInformation()
        {
            CurrentMemoryInformation = AppService.GetAppMemoryUsage();
            MemoryExceedsThreshold = CurrentMemoryInformation.AppMemorySpace < MemoryThreshold;
            return CurrentMemoryInformation;
        }

        public static bool IsThresholdExceeded(ulong memoryThreshold)
        {
            MemoryInformation memoryInformation = AppService.GetAppMemoryUsage();
            return memoryInformation.AppMemorySpace < memoryThreshold;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(Boolean itIsSafeToAlsoFreeManagedObjects)
        {
            //Free managed resources too, but only if I'm being called from Dispose
            //(If I'm being called from Finalize then the objects might not exist anymore
            if (!itIsSafeToAlsoFreeManagedObjects) return;
            
            if (_memoryTimer == null) return;
            
            _memoryTimer.Dispose();
            _memoryTimer = null;
        }


        protected virtual void OnDiagnosticUpdate(MemoryInformation e)
        {
            EventHandler<MemoryInformation> handler = DiagnosticUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void TimerCallback(object state)
        {
            OnDiagnosticUpdate(GetCurrentMemoryInformation());
        }

        #endregion

        #region Properties

        public ulong MemoryThreshold { get; private set; }

        private MemoryInformation _currentMemoryInformation;
        public MemoryInformation CurrentMemoryInformation
        {
            get { return _currentMemoryInformation; }
            set
            {
                _currentMemoryInformation = value;
                OnPropertyChanged("CurrentMemoryInformation");
            }
        }

        private bool _memoryExceedsThreshold;
        public bool MemoryExceedsThreshold
        {
            get { return _memoryExceedsThreshold; }
            set
            {
                _memoryExceedsThreshold = value;
                OnPropertyChanged("MemoryExceedsThreshold");
            }
        }

        public event EventHandler<MemoryInformation> DiagnosticUpdate; 

        #endregion
       
    }
}
