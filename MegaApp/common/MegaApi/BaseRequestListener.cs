using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.MegaApi
{
    abstract class BaseRequestListener: MRequestListenerInterface
    {
        public BaseRequestListener()
        {
            // Set the timer to trigger the event after 10 seconds
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                apiErrorTimer = new DispatcherTimer();
                apiErrorTimer.Tick += ApiErrorTimerOnTick;
                apiErrorTimer.Interval = new TimeSpan(0, 0, 10);
            });
        }

        #region Properties

        /// <summary>
        /// Timer to count the time during which the request is waiting/retrying
        /// </summary>
        protected DispatcherTimer apiErrorTimer;

        /// <summary>
        /// Store the current API instance
        /// </summary>
        private MegaSDK api;

        abstract protected string ProgressMessage { get; }
        abstract protected bool ShowProgressMessage { get; }
        abstract protected string ErrorMessage { get; }
        abstract protected string ErrorMessageTitle { get; }
        abstract protected bool ShowErrorMessage { get; }
        abstract protected string SuccessMessage { get; }
        abstract protected string SuccessMessageTitle { get; }
        abstract protected bool ShowSuccesMessage { get; }
        abstract protected bool NavigateOnSucces { get; }
        abstract protected bool ActionOnSucces { get; }
        abstract protected Type NavigateToPage { get; }
        abstract protected NavigationParameter NavigationParameter { get; }
        
        #endregion

        #region Methods

        // Method which is called when the timer event is triggered
        private void ApiErrorTimerOnTick(object sender, object e)
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

        #endregion

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            this.api = api;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            switch(e.getErrorCode())
            {
                case MErrorType.API_OK:
                    if (ShowSuccesMessage)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            new CustomMessageDialog(
                                SuccessMessageTitle,
                                SuccessMessage,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        });
                    }
                    
                    if (ActionOnSucces)
                        OnSuccesAction(api, request);

                    if (NavigateOnSucces)
                        Deployment.Current.Dispatcher.BeginInvoke(() => NavigateService.NavigateTo(NavigateToPage, NavigationParameter));
                    break;

                case MErrorType.API_EGOINGOVERQUOTA: // Not enough quota
                case MErrorType.API_EOVERQUOTA: //Storage overquota error
                    // Stop all upload transfers
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                        string.Format("Storage quota exceeded ({0}) - Canceling uploads", e.getErrorCode().ToString()));
                    api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

                    // Disable the "camera upload" service if is enabled
                    if (MediaService.GetAutoCameraUploadStatus())
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_INFO,
                            string.Format("Storage quota exceeded ({0}) - Disabling CAMERA UPLOADS service", e.getErrorCode().ToString()));
                        MediaService.SetAutoCameraUpload(false);
                        SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);
                    }

                    Deployment.Current.Dispatcher.BeginInvoke(() => DialogService.ShowOverquotaAlert());
                    break;

                default:
                    if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
                    {
                        if (ShowErrorMessage)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                new CustomMessageDialog(
                                    ErrorMessageTitle,
                                    String.Format(ErrorMessage, e.getErrorString()),
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            });
                        }
                    }
                    break;
            }
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            this.api = api;

            if (!ShowProgressMessage) return;
            var autoReset = new AutoResetEvent(true);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.SetProgressIndicator(true, ProgressMessage);
                autoReset.Set();
            });
            autoReset.WaitOne();
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            this.api = api;

            if(DebugService.DebugSettings.IsDebugMode || Debugger.IsAttached)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));
            }            
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            this.api = api;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnSuccesAction(MegaSDK api, MRequest request)
        {
            // No standard succes action
        }

        #endregion
    }
}
