using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaTransferListener: MTransferListenerInterface
    {
        private Timer _timer;

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            if(_timer != null) 
                _timer.Dispose();
            
            try
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    ulong mtime = api.getNodeByHandle(transfer.getNodeHandle()).getModificationTime();
                    DateTime pictureDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(mtime));                    
                    SettingsService.SaveSettingToFile<DateTime>("LastUploadDate", pictureDate);
                    
                    // If file upload succeeded. Clear the error information for a clean sheet.
                    ErrorProcessingService.Clear();
                }
                else
                {
                   // An error occured. Process it.
                   ErrorProcessingService.ProcessFileError(e.getErrorString(), transfer.getFileName());
                }
            }
            catch (Exception)
            {
                // Setting could not be saved. Just continue the run
            }
            finally
            {
                // Start a new upload action
                ScheduledAgent.Upload();
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            _timer = new Timer(state =>
            {
                api.retryPendingConnections();
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            
        }
    }
}
