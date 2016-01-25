using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class FetchNodesRequestListener : BaseRequestListener
    {
        private readonly MainPageViewModel _mainPageViewModel;
        private readonly CameraUploadsPageViewModel _cameraUploadsPageViewModel;
        private readonly String _shortCutBase64Handle;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public FetchNodesRequestListener(MainPageViewModel mainPageViewModel, String shortCutBase64Handle = null, 
            CameraUploadsPageViewModel cameraUploadsPageViewModel = null)
        {
            this._mainPageViewModel = mainPageViewModel;
            this._cameraUploadsPageViewModel = cameraUploadsPageViewModel;
            this._shortCutBase64Handle = shortCutBase64Handle;

            timerAPI_EAGAIN = new DispatcherTimer();
            timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
            timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);
        }

        // Method which is call when the timer event is triggered
        private void timerTickAPI_EAGAIN(object sender, object e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                timerAPI_EAGAIN.Stop();
                ProgressService.SetProgressIndicator(true, ProgressMessages.ServersTooBusy);                
            });
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
            App.AppInformation.HasFetchedNodes = true;

            // If the user is trying to open a shortcut
            if (_shortCutBase64Handle != null)
            {
                bool shortCutError = false;

                MNode shortCutMegaNode = api.getNodeByBase64Handle(_shortCutBase64Handle);
                if (_mainPageViewModel != null && shortCutMegaNode != null)
                {
                    // Looking for the absolute parent of the shortcut node to see the type
                    MNode parentNode;
                    MNode absoluteParentNode = shortCutMegaNode;
                    while ((parentNode = api.getParentNode(absoluteParentNode)) != null)
                        absoluteParentNode = parentNode;

                    if (absoluteParentNode.getType() == MNodeType.TYPE_ROOT)
                    {
                        var newRootNode = NodeService.CreateNew(api, _mainPageViewModel.AppInformation, shortCutMegaNode, ContainerType.CloudDrive);
                        var autoResetEvent = new AutoResetEvent(false);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            _mainPageViewModel.ActiveFolderView.FolderRootNode = newRootNode;
                            autoResetEvent.Set();
                        });
                        autoResetEvent.WaitOne();
                    }
                    else shortCutError = true;
                }
                else shortCutError = true;

                if(shortCutError)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                                AppMessages.ShortCutFailed_Title,
                                AppMessages.ShortCutFailed,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                    });
                }
            }
            else
            {
                if(_mainPageViewModel != null)
                {
                    var cloudDriveRootNode = _mainPageViewModel.CloudDrive.FolderRootNode ??
                        NodeService.CreateNew(api, _mainPageViewModel.AppInformation, api.getRootNode(), ContainerType.CloudDrive);
                    var rubbishBinRootNode = _mainPageViewModel.RubbishBin.FolderRootNode ??
                        NodeService.CreateNew(api, _mainPageViewModel.AppInformation, api.getRubbishNode(), ContainerType.RubbishBin);

                    var autoResetEvent = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _mainPageViewModel.CloudDrive.FolderRootNode = cloudDriveRootNode;
                        _mainPageViewModel.RubbishBin.FolderRootNode = rubbishBinRootNode;
                        autoResetEvent.Set();
                    });
                    autoResetEvent.WaitOne();
                }
                else
                {
                    var cameraUploadsRootNode = _cameraUploadsPageViewModel.CameraUploads.FolderRootNode ??
                        NodeService.CreateNew(api, _cameraUploadsPageViewModel.AppInformation,
                        NodeService.FindCameraUploadNode(api, api.getRootNode()), ContainerType.CloudDrive);
                    
                    var autoResetEvent = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _cameraUploadsPageViewModel.CameraUploads.FolderRootNode = cameraUploadsRootNode;
                        autoResetEvent.Set();
                    });
                    autoResetEvent.WaitOne();
                }
            }

            if (_mainPageViewModel != null) _mainPageViewModel.LoadFolders();
            if (_cameraUploadsPageViewModel != null) _cameraUploadsPageViewModel.LoadFolders();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Enable MainPage appbar buttons
                if (_mainPageViewModel != null) _mainPageViewModel.SetCommandStatus(true);
            });

            // KEEP ALWAYS AT THE END OF THE METHOD, AFTER THE "LoadForlders" call
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // If is a newly activated account, navigates to the upgrade account page
                if (App.AppInformation.IsNewlyActivatedAccount)
                    NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, new Dictionary<string, string> { { "Pivot", "1" } });
                // If is the first login, navigates to the camera upload service config page
                else if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
                    NavigateService.NavigateTo(typeof(InitCameraUploadsPage), NavigationParameter.Normal);
                else if (App.AppInformation.IsStartedAsAutoUpload)
                    NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.AutoCameraUpload);
            });
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => timerAPI_EAGAIN.Stop());
            base.onRequestFinish(api, request, e);
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Disable MainPage appbar buttons
                if (_mainPageViewModel != null) _mainPageViewModel.SetCommandStatus(false);                

                ProgressService.SetProgressIndicator(true,
                   String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix()));
            });
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                Deployment.Current.Dispatcher.BeginInvoke(() => timerAPI_EAGAIN.Start());
            }

            base.onRequestTemporaryError(api, request, e);
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(true, String.Format(ProgressMessages.FetchingNodes,
                    request.getTransferredBytes().ToStringAndSuffix()));
            });
            
            if (AppMemoryController.IsThresholdExceeded(75UL.FromMBToBytes()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.MemoryLimitError_Title,
                            AppMessages.MemoryLimitError,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    Application.Current.Terminate();
                });

            }
        }

        #endregion
    }
}
