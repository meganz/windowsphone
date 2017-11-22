using System;
using System.Threading;
using mega;
using ScheduledCameraUploadTaskAgent.Services;

namespace ScheduledCameraUploadTaskAgent.MegaApi
{
    class MegaTransferListener: MTransferListenerInterface
    {
        private Timer _timer;

        // Event raised so that the task agent can abort itself when storage quota is exceeded
        public event EventHandler StorageQuotaExceeded;

        // Event raised so that the task agent can finish itself when transfer quota is exceeded
        public event EventHandler TransferQuotaExceeded;

        protected virtual void OnStorageQuotaExceeded(EventArgs e)
        {
            if (StorageQuotaExceeded != null)
                StorageQuotaExceeded(this, e);
        }

        protected virtual void OnTransferQuotaExceeded(EventArgs e)
        {
            if (TransferQuotaExceeded != null)
                TransferQuotaExceeded(this, e);
        }

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            if(_timer != null) 
                _timer.Dispose();

            if (e.getErrorCode() == MErrorType.API_EGOINGOVERQUOTA || e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                //Stop the Camera Upload Service
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, 
                    "Storage quota exceeded ({0}) - Disabling CAMERA UPLOADS service", e.getErrorCode().ToString());
                OnStorageQuotaExceeded(EventArgs.Empty);
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
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.getErrorString());
                            ErrorProcessingService.ProcessFileError(transfer.getFileName());
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
            // Transfer overquota error
            if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Transfer quota exceeded (API_EOVERQUOTA)");
                OnTransferQuotaExceeded(EventArgs.Empty);
            }
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            
        }
    }
}
