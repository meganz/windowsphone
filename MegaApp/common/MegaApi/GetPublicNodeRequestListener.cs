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
            get { return AppMessages.ImportFileFailed; }
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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            MNode publicNode = request.getPublicNode();
            if (publicNode != null)
            {
                #if WINDOWS_PHONE_80
                // Detect if is an image to allow directly download to camera albums
                bool isImage = false;
                if (publicNode.isFile())
                {
                    FileNodeViewModel node = new FileNodeViewModel(api, null, publicNode);
                    isImage = node.IsImage;
                }   

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    DialogService.ShowOpenLink(publicNode, request.getLink(), _folderViewModel, isImage));
                #elif WINDOWS_PHONE_81
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    DialogService.ShowOpenLink(publicNode, request.getLink(), _folderViewModel));
                #endif
            }                
            else
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            ErrorMessageTitle,
                            ErrorMessage,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
        }

        #endregion
    }
}
