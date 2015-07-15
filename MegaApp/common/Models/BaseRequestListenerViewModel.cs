using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    internal abstract class BaseRequestListenerViewModel : BaseViewModel, MRequestListenerInterface
    {
        #region Properties

        protected abstract string ProgressMessage { get; }
        protected abstract string ErrorMessage { get; }
        protected abstract string ErrorMessageTitle { get; }
        protected abstract string SuccessMessage { get; }
        protected abstract string SuccessMessageTitle { get; }
        protected abstract bool ShowSuccesMessage { get; }
        protected abstract bool NavigateOnSucces { get; }
        protected abstract bool ActionOnSucces { get; }
        protected abstract Type NavigateToPage { get; }
        protected abstract NavigationParameter NavigationParameter { get; }

        #endregion

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);                   
                ProgressService.SetProgressIndicator(false);

                this.ControlState = true;

                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    if (ShowSuccesMessage)
                        new CustomMessageDialog(SuccessMessageTitle, SuccessMessage, App.AppInformation).ShowDialog();

                    if (ActionOnSucces)
                        OnSuccesAction(request);

                    if (NavigateOnSucces)
                        NavigateService.NavigateTo(NavigateToPage, NavigationParameter);
                }
                else if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
                {
                    // Stop all upload transfers
                    if (App.MegaTransfers.Count > 0)
                    {
                        foreach (var item in App.MegaTransfers)
                        {
                            var transferItem = (TransferObjectModel) item;
                            if (transferItem == null) continue;

                            if (transferItem.Type == TransferType.Upload)
                                transferItem.CancelTransfer();
                        }
                    }

                    //**************************************************
                    // TODO: Disable the "camera upload" (when availabe)
                    //**************************************************


                    // User notification message.
                    Deployment.Current.Dispatcher.BeginInvoke(() => DialogService.ShowOverquotaAlert());
                }
                else
                {
                    new CustomMessageDialog(
                            ErrorMessageTitle,
                            String.Format(ErrorMessage, e.getErrorString()),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                }
            });
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.ControlState = false;
                ProgressService.SetProgressIndicator(true, ProgressMessage);
            });
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));  
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnSuccesAction(MRequest request)
        {
            // No standard succes action
        }

        #endregion
    }
}
