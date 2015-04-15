using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        private readonly MainPageViewModel _mainPageViewModel;
        private readonly ulong? _shortCutHandle;
        public FetchNodesRequestListener(MainPageViewModel mainPageViewModel, ulong? shortCutHandle = null)
        {
            this._mainPageViewModel = mainPageViewModel;
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
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Enable appbar buttons
                //_cloudDriveViewModel.SetCommandStatus(true);
            });

            if (_shortCutHandle.HasValue)
            {
                //MNode shortCutMegaNode = api.getNodeByHandle(_shortCutHandle.Value);
                //if (shortCutMegaNode != null)
                //{
                //     var newRootNode = NodeService.CreateNew(api, _cloudDriveViewModel.AppInformation, shortCutMegaNode);
                //     var autoResetEvent = new AutoResetEvent(false);
                //     Deployment.Current.Dispatcher.BeginInvoke(() =>
                //     {
                //         _cloudDriveViewModel.CurrentRootNode = newRootNode;
                //         autoResetEvent.Set();
                //     });
                //     autoResetEvent.WaitOne();
                //}
            }
            else
            {
                var cloudDriveRootNode = _mainPageViewModel.CloudDrive.FolderRootNode ??
                    NodeService.CreateNew(api, _mainPageViewModel.AppInformation, api.getRootNode());
                var rubbishBinRootNode = _mainPageViewModel.RubbishBin.FolderRootNode ??
                        NodeService.CreateNew(api, _mainPageViewModel.AppInformation, api.getRubbishNode());

                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mainPageViewModel.CloudDrive.FolderRootNode = cloudDriveRootNode;
                    _mainPageViewModel.RubbishBin.FolderRootNode = rubbishBinRootNode;
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            }

            _mainPageViewModel.LoadFolders();
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Disable appbar buttons
                //_cloudDriveViewModel.SetCommandStatus(false);

                ProgressService.SetProgressIndicator(true,
                   String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix()));
            });
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaGrayBackgroundColor"]);
                ProgressService.SetProgressIndicator(true, String.Format(ProgressMessages.FetchingNodes,
                    request.getTransferredBytes().ToStringAndSuffix()));
            });
            
            if (AppMemoryController.IsThresholdExceeded(75UL.FromMBToBytes()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppMessages.MemoryLimitError,
                        AppMessages.MemoryLimitError_Title, MessageBoxButton.OK);
                    Application.Current.Terminate();
                });

            }
        }

        #endregion
    }
}
