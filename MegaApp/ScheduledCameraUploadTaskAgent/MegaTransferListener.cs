using System;
using System.Threading;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaTransferListener: MTransferListenerInterface
    {
        private Timer _timer;

        // Event raised so that the task agent can abort itself on Quoata exceeded
        public event EventHandler QuotaExceeded;

        protected virtual void OnQuotaExceeded(EventArgs e)
        {
            if (QuotaExceeded != null)
                QuotaExceeded(this, e);
        }

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            if(_timer != null) 
                _timer.Dispose();
            
            if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                //Stop the Camera Upload Service
                MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Disabling CAMERA UPLOADS service (API_EOVERQUOTA)");
                OnQuotaExceeded(EventArgs.Empty);
                return;
            }

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
                    // An error occured. Log and process it.
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_EFAILED:
                        case MErrorType.API_EEXIST:
                        case MErrorType.API_EARGS:
                        case MErrorType.API_EREAD:
                        case MErrorType.API_EWRITE:
                        {
                            ErrorProcessingService.ProcessFileError(e.getErrorString(), transfer.getFileName());
                            break;
                        }
                    }
                  
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
