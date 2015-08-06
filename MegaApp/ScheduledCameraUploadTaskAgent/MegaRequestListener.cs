using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaRequestListener : MRequestListenerInterface
    {
        public event EventHandler<SucceededEventArgs> RequestFinished;

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (RequestFinished == null) return;

            RequestFinished(this, new SucceededEventArgs(e.getErrorCode() == MErrorType.API_OK));
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            // Do nothing
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Do nothing
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Do nothing
        }
    }

    public class SucceededEventArgs : EventArgs
    {
        public SucceededEventArgs(bool succeeded)
        {
            this.Succeeded = succeeded;
        }

        public bool Succeeded { get; private set; }
    }
}
