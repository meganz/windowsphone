using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetPublicNodeRequestListener: BaseRequestListener
    {
        private readonly FolderViewModel _folderViewModel;
        
        public GetPublicNodeRequestListener(FolderViewModel folderViewModel)
        {
            this._folderViewModel = folderViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.ImportFile; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.AM_ImportFileFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.ImportFileFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods
        
        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            App.ActiveImportLink = null;

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                //If getFlag() returns true, the file link key is invalid.
                if (request.getFlag() && ShowErrorMessage)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            AppMessages.AM_InvalidLink,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });
                }                    
            }
            else if (e.getErrorCode() == MErrorType.API_EINCOMPLETE)
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
            
            base.onRequestFinish(api, request, e);            
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            MNode publicNode = App.PublicNode = request.getPublicNode();
            if (publicNode != null)
            {
                #if WINDOWS_PHONE_80
                // Detect if is an image to allow directly download to camera albums
                bool isImage = false;
                if (publicNode.isFile())
                {
                    FileNodeViewModel node = new FileNodeViewModel(api, null, publicNode, ContainerType.PublicLink);
                    isImage = node.IsImage;
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    DialogService.ShowOpenLink(publicNode, request.getLink(), _folderViewModel, isImage));
                #elif WINDOWS_PHONE_81
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        DialogService.ShowOpenLink(publicNode, request.getLink(), _folderViewModel);
                    }
                    catch (Exception e)
                    {
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            String.Format(ErrorMessage, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    }
                });
                #endif
            }                
            else
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        ErrorMessageTitle,
                        AppMessages.AM_ImportFileFailedNoErrorCode,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
        }

        #endregion
    }
}
