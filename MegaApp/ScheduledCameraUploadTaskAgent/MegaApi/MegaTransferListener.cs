using System;
using System.Threading;
using mega;
using ScheduledCameraUploadTaskAgent.Services;

namespace ScheduledCameraUploadTaskAgent.MegaApi
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
            switch (e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA:
                case MErrorType.API_EOVERQUOTA:
                    if (e.getValue() != 0) // TRANSFER OVERQUOTA ERROR
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Transfer quota exceeded ({0})", e.getErrorCode().ToString()));
                    }
                    else // STORAGE OVERQUOTA ERROR
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Storage quota exceeded ({0})", e.getErrorCode().ToString()));
                    }
                    break;
            }
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            
        }
    }
}
