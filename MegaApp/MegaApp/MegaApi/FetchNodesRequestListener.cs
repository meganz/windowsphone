using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
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
        public FetchNodesRequestListener(CloudDriveViewModel cloudDriveViewModel, NodeViewModel rootRefreshNode = null)
        {
            this._cloudDriveViewModel = cloudDriveViewModel;
            this._rootRefreshNode = rootRefreshNode;
        }

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    _cloudDriveViewModel.CurrentRootNode = this._rootRefreshNode ?? new NodeViewModel(api, api.getRootNode());
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
        }

        #endregion
    }
}
