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
            _timer.Dispose();
            
            try
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    ulong mtime = api.getNodeByHandle(transfer.getNodeHandle()).getModificationTime();
                    DateTime pictureDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(mtime));                    
                    SettingsService.SaveSettingToFile<DateTime>("LastUploadDate", pictureDate);
                }                    

                // Clean up after upload
                File.Delete(transfer.getPath());
            }
            catch (Exception)
            {
                // File could not be found for delete or setting could not be saved
                // Just continue the run
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
