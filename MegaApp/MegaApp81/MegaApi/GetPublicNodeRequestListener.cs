﻿using System;
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
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public GetPublicNodeRequestListener(CloudDriveViewModel cloudDriveViewModel)
        {
            this._cloudDriveViewModel = cloudDriveViewModel;
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
                // Detect if is an image to allow directly download to camera albums
                bool isImage = false;
                if (publicNode.isFile())
                {
                    FileNodeViewModel node = new FileNodeViewModel(api, publicNode);
                    isImage = node.IsImage;
                }   

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    DialogService.ShowOpenLink(publicNode, request.getLink(), _cloudDriveViewModel, isImage));
            }                
            else
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    MessageBox.Show(ErrorMessage, ErrorMessageTitle, MessageBoxButton.OK));
        }

        #endregion
    }
}