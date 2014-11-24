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
    class FetchNodesRequestListener : BaseRequestListener
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        private readonly NodeViewModel _rootRefreshNode;
        private readonly ulong? _shortCutHandle;
        public FetchNodesRequestListener(CloudDriveViewModel cloudDriveViewModel, NodeViewModel rootRefreshNode = null,
            ulong? shortCutHandle = null)
        {
            this._cloudDriveViewModel = cloudDriveViewModel;
            this._rootRefreshNode = rootRefreshNode;
            this._shortCutHandle = shortCutHandle;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.FetchingNodes; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.FetchingNodesFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.FetchingNodesFailed_Title; }
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
            if (_shortCutHandle.HasValue)
            {
                MNode shortCutMegaNode = api.getNodeByHandle(_shortCutHandle.Value);
                if (shortCutMegaNode != null)
                {

                    _cloudDriveViewModel.CurrentRootNode = new NodeViewModel(api, shortCutMegaNode);
                }
            }
            else
            {
                _cloudDriveViewModel.CurrentRootNode = this._rootRefreshNode ??
                                                        new NodeViewModel(api, api.getRootNode());
            }
            //_cloudDriveViewModel.LoadNodes();
            _cloudDriveViewModel.LoadNodes();
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => ProgessService.SetProgressIndicator(true,
                String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix())));
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => ProgessService.SetProgressIndicator(true,
                String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix())));

            if (AppMemoryController.IsThresholdExceeded(75UL.FromMBToBytes()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(
                        "Not enough free memory space to complete this operation. The app will shutdown now",
                        "Memory Limit", MessageBoxButton.OK);
                    Application.Current.Terminate();
                });

            }
        }

        #endregion
    }
}
