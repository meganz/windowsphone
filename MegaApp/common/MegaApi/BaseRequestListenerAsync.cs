using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        /// Error description associated with an error code
        /// </summary>
        public string ErrorString;
        
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

        // Method which is called when the timer event is triggered
        private void ApiErrorTimerOnTick(object sender, object o)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
            });

            if (this.api == null) return;

            string message = string.Empty;
            switch ((MRetryReason)this.api.isWaiting())
            {
                case MRetryReason.RETRY_CONNECTIVITY:
                    message = ProgressMessages.PM_ConnectivityIssue;
                    break;

                case MRetryReason.RETRY_SERVERS_BUSY:
                    message = ProgressMessages.PM_ServersBusy;
                    break;

                case MRetryReason.RETRY_API_LOCK:
                    message = ProgressMessages.PM_ApiLocked;
                    break;

                case MRetryReason.RETRY_RATE_LIMIT:
                    message = ProgressMessages.PM_ApiRateLimit;
                    break;

                default: return;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(true, message));
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
            this.api = api;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            this.api = api;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (DebugService.DebugSettings.IsDebugMode || Debugger.IsAttached)
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]);

                // If is the first error/retry (timer is not running) start the timer
                if (apiErrorTimer != null && !apiErrorTimer.IsEnabled)
                    apiErrorTimer.Start();
            });
        }

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            this.api = api;

            this.ErrorString = e.getErrorString();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);

                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
            });

            switch(e.getErrorCode())
            {
                case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                case MErrorType.API_EOVERQUOTA: // Storage overquota error
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, 
                        string.Format("Storage quota exceeded ({0})", e.getErrorCode().ToString()));
                    Deployment.Current.Dispatcher.BeginInvoke(DialogService.ShowStorageOverquotaAlert);
                    break;
            }
        }
    }
}
