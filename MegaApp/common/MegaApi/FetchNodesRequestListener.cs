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
    class FetchNodesRequestListener : PublicLinkRequestListener
    {
        private readonly MainPageViewModel _mainPageViewModel;
        private readonly CameraUploadsPageViewModel _cameraUploadsPageViewModel;
        private readonly FolderLinkViewModel _folderLinkViewModel;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public FetchNodesRequestListener(MainPageViewModel mainPageViewModel,
            CameraUploadsPageViewModel cameraUploadsPageViewModel = null)
        {
            this._mainPageViewModel = mainPageViewModel;
            this._cameraUploadsPageViewModel = cameraUploadsPageViewModel;
            this._folderLinkViewModel = null;

            createTimer();
        }

        public FetchNodesRequestListener(CameraUploadsPageViewModel cameraUploadsPageViewModel)
        {
            this._mainPageViewModel = null;
            this._cameraUploadsPageViewModel = cameraUploadsPageViewModel;
            this._folderLinkViewModel = null;

            createTimer();
        }

        public FetchNodesRequestListener(FolderLinkViewModel folderLinkViewModel)
        {
            this._mainPageViewModel = null;
            this._cameraUploadsPageViewModel = null;
            this._folderLinkViewModel = folderLinkViewModel;

            createTimer();
        }

        private void createTimer()
        {
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
            if (_mainPageViewModel != null)
                FetchNodesMainPage(api, request);
            else if (_cameraUploadsPageViewModel != null)
                FetchNodesCameraUploadsPage(api, request);
            else if (_folderLinkViewModel != null)
                FetchNodesFolderLink(api, request);
        }

        private void FetchNodesMainPage(MegaSDK api, MRequest request)
        {
            App.AppInformation.HasFetchedNodes = true;

            // If the user is trying to open a shortcut
            if (App.ShortCutBase64Handle != null)
            {
                bool shortCutError = false;

                MNode shortCutMegaNode = api.getNodeByBase64Handle(App.ShortCutBase64Handle);
                App.ShortCutBase64Handle = null;

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

                if (shortCutError)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(AppMessages.ShortCutFailed_Title,
                            AppMessages.ShortCutFailed, App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });
                }
            }
            else
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

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mainPageViewModel.LoadFolders();
                _mainPageViewModel.GetAccountDetails();

                // Enable MainPage appbar buttons
                _mainPageViewModel.SetCommandStatus(true);

                if (_mainPageViewModel.SpecialNavigation()) return;                
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

        private void FetchNodesCameraUploadsPage(MegaSDK api, MRequest request)
        {
            App.AppInformation.HasFetchedNodes = true;            

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

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                _cameraUploadsPageViewModel.LoadFolders());
        }

        private void FetchNodesFolderLink(MegaSDK api, MRequest request)
        {
            var folderLinkRootNode = _folderLinkViewModel.FolderLink.FolderRootNode ??
                NodeService.CreateNew(api, App.AppInformation, api.getRootNode(), ContainerType.FolderLink);

            var autoResetEvent = new AutoResetEvent(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _folderLinkViewModel.FolderLink.FolderRootNode = folderLinkRootNode;
                autoResetEvent.Set();
            });
            autoResetEvent.WaitOne();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                _folderLinkViewModel.LoadFolders());
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => timerAPI_EAGAIN.Stop());

            // If is a folder link fetch nodes
            if (_folderLinkViewModel != null)
                onRequestFinishFolderLink(api, request, e);
            else
                base.onRequestFinish(api, request, e);                
        }

        private void onRequestFinishFolderLink(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                //If getFlag() returns true, the folder link key is invalid.
                if (request.getFlag())
                {
                    // First logout from the folder
                    api.logout();

                    // Set the empty state and disable the app bar buttons
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _folderLinkViewModel.FolderLink.SetEmptyContentTemplate(false);
                        _folderLinkViewModel._folderLinkPage.SetApplicationBarData(false);
                    });

                    //If the user have written the key
                    if (_decryptionAlert)
                        ShowDecryptionKeyNotValidAlert(api, request);
                    else
                        ShowLinkNoValidAlert();
                }
                else
                {
                    OnSuccesAction(api, request);
                }
            }
            else
            {
                if (e.getErrorCode() == MErrorType.API_ENOENT)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _folderLinkViewModel.FolderLink.SetEmptyContentTemplate(false);
                        _folderLinkViewModel._folderLinkPage.SetApplicationBarData(false);
                    });

                    ShowUnavailableFolderLinkAlert();
                }
            }
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
