using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    abstract class BaseRequestListener: MRequestListenerInterface
    {
        #region Properties

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

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaGrayBackgroundColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                if (ShowSuccesMessage)
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => MessageBox.Show(SuccessMessage, SuccessMessageTitle, MessageBoxButton.OK));

                if (ActionOnSucces)
                    OnSuccesAction(api, request);

                if (NavigateOnSucces)
                    Deployment.Current.Dispatcher.BeginInvoke(() => NavigateService.NavigateTo(NavigateToPage, NavigationParameter));
            }
            else if(e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Stop all upload transfers
                    if (App.MegaTransfers.Count > 0)
                    {
                        foreach (var item in App.MegaTransfers)
                        {
                            var transferItem = (TransferObjectModel)item;
                            if (transferItem == null) continue;

                            if (transferItem.Type == TransferType.Upload)
                                transferItem.CancelTransfer();
                        }
                    }

                    //**************************************************
                    // TODO: Disable the "camera upload" (when availabe)
                    //**************************************************

                    DialogService.ShowOverquotaAlert();
                });
            }
            else if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
            {
                if (ShowErrorMessage)
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        MessageBox.Show(String.Format(ErrorMessage, e.getErrorString()), ErrorMessageTitle, MessageBoxButton.OK));
            }           
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
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
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));

            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    ProgressService.SetProgressIndicator(false);
            //    if (ShowErrorMessage)
            //        MessageBox.Show(String.Format(ErrorMessage, e.getErrorString()), ErrorMessageTitle, MessageBoxButton.OK);
            //});
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {            
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaGrayBackgroundColor"]));
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
