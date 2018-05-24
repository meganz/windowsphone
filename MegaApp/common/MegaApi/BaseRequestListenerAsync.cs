using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using mega;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal abstract class BaseRequestListenerAsync<T>: MRequestListenerInterface
    {
        protected TaskCompletionSource<T> Tcs;

        /// <summary>
        /// Timer to count the time during which the request is waiting/retrying
        /// </summary>
        private DispatcherTimer apiErrorTimer;

        /// <summary>
        /// Store the current API instance
        /// </summary>
        private MegaSDK api;

        /// <summary>
        /// Event triggered when the request is waiting/retrying during more than 10 seconds
        /// </summary>
        public EventHandler<MRetryReason> IsWaiting;
        
        public BaseRequestListenerAsync()
        {
            // Set the timer to trigger the event after 10 seconds
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                apiErrorTimer = new DispatcherTimer();
                apiErrorTimer.Tick += ApiErrorTimerOnTick;
                apiErrorTimer.Interval = new TimeSpan(0, 0, 10);
            });
        }

        private void ApiErrorTimerOnTick(object sender, object o)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
            });

            if (IsWaiting != null)
                IsWaiting.Invoke(this, (MRetryReason)api.isWaiting());
        }

        public async Task<T> ExecuteAsync(Action action)
        {
            Tcs = new TaskCompletionSource<T>();

            action.Invoke();

            return await Tcs.Task;
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            this.api = api;
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Do nothing
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // If is the first error/retry (timer is not running) start the timer
                if (apiErrorTimer != null && !apiErrorTimer.IsEnabled)
                    apiErrorTimer.Start();
            });
        }

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
            });

            switch(e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                case MErrorType.API_EOVERQUOTA: // Storage overquota error
                    Deployment.Current.Dispatcher.BeginInvoke(DialogService.ShowOverquotaAlert);

                    // Stop all upload transfers
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Storage quota exceeded ({0}) - Canceling uploads", e.getErrorCode().ToString()));
                    api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

                    // Disable the "Camera Uploads" service if is enabled
                    if (MediaService.GetAutoCameraUploadStatus())
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Storage quota exceeded ({0}) - Disabling CAMERA UPLOADS service", e.getErrorCode().ToString()));
                        MediaService.SetAutoCameraUpload(false);
                        SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);
                    }
                    break;
            }
        }
    }
}
