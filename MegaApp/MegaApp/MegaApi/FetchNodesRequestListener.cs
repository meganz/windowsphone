using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class FetchNodesRequestListener: MRequestListenerInterface
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

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    if (_shortCutHandle.HasValue)
                    {
                        MNode shortCutMegaNode = api.getNodeByHandle(_shortCutHandle.Value);
                        if (shortCutMegaNode != null){
                            
                            _cloudDriveViewModel.CurrentRootNode = new NodeViewModel(api, shortCutMegaNode);
                        }
                    }
                    else
                    {
                        _cloudDriveViewModel.CurrentRootNode = this._rootRefreshNode ?? new NodeViewModel(api, api.getRootNode());
                    }

                    _cloudDriveViewModel.LoadNodes();
                }
                else
                {
                    MessageBox.Show(e.getErrorString());
                }

                ProgessService.SetProgressIndicator(false);
            });

        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => ProgessService.SetProgressIndicator(true,
                String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix())));
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(e.getErrorString()));
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
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
