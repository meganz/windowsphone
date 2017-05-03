using System;
using System.Threading;
using mega;
using MegaApp.Services;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaTransferListener: MTransferListenerInterface
    {
        private Timer _timer;

        // Event raised so that the task agent can abort itself when disk quota is exceeded
        public event EventHandler DiskQuotaExceeded;

        // Event raised so that the task agent can finish itself when transfer quota is exceeded
        public event EventHandler TransferQuotaExceeded;

        protected virtual void OnDiskQuotaExceeded(EventArgs e)
        {
            if (DiskQuotaExceeded != null)
                DiskQuotaExceeded(this, e);
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
            
            if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                //Stop the Camera Upload Service
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Disk quota exceeded (API_EOVERQUOTA) - Disabling CAMERA UPLOADS service");
                OnDiskQuotaExceeded(EventArgs.Empty);
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
