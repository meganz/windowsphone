using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;

namespace MegaApp.Models
{
    public abstract class MemDiagnosticViewModel: BaseSdkViewModel, IDisposable
    {
        protected MemDiagnosticViewModel(MegaSDK megaSdk, bool autoStartDiagnostics, TimeSpan diagnosticPeriod)
            : base(megaSdk)
        {
            MemoryController = new AppMemoryController(314572800);
            if(autoStartDiagnostics)
                MemoryController.StartDiagnostics(new TimeSpan(0), diagnosticPeriod);
        }

        ~MemDiagnosticViewModel()
        {
            Dispose(false); //I am *not* calling you from Dispose, it's *not* safe
        }

        #region Methods

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

            if (MemoryController == null) return;

            MemoryController.Dispose();
            MemoryController = null;
        }

        #endregion

        #region Properties

        public AppMemoryController MemoryController { get; private set; }

        #endregion

       
    }
}
