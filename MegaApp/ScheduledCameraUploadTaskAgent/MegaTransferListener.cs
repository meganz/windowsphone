using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaTransferListener: MTransferListenerInterface
    {
        public event EventHandler<SucceededEventArgs> TransferFinished;

        private Timer _timer;

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            _timer.Dispose();

            if (TransferFinished == null) return;

            TransferFinished(this, new SucceededEventArgs(e.getErrorCode() == MErrorType.API_OK));
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            _timer = new Timer(state => api.retryPendingConnections(), 
                null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            api.retryPendingConnections();
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            api.retryPendingConnections();
        }
    }
}
