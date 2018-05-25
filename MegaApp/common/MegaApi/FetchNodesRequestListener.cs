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

        public FetchNodesRequestListener(MainPageViewModel mainPageViewModel,
            CameraUploadsPageViewModel cameraUploadsPageViewModel = null)
            : base()
        {
            this._mainPageViewModel = mainPageViewModel;
            this._cameraUploadsPageViewModel = cameraUploadsPageViewModel;
        }

        public FetchNodesRequestListener(CameraUploadsPageViewModel cameraUploadsPageViewModel)
            : base()
        {
            this._mainPageViewModel = null;
            this._cameraUploadsPageViewModel = cameraUploadsPageViewModel;
        }

        public FetchNodesRequestListener(FolderLinkViewModel folderLinkViewModel)
            : base(folderLinkViewModel)
        {
            this._mainPageViewModel = null;
            this._cameraUploadsPageViewModel = null;
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
            // Enable transfer resumption for the current MegaSDK instance which is
            // doing the fetch nodes request (app, folder link, etc.)
            api.enableTransferResumption();

            // If is required show the password reminder dialog on background thread
            Task.Run(async () =>
            {
                if (await AccountService.ShouldShowPasswordReminderDialogAsync())
                    Deployment.Current.Dispatcher.BeginInvoke(() => DialogService.ShowPasswordReminderDialog(false));
            });

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
            // If the user is trying to open a MEGA link
            else if (App.LinkInformation.ActiveLink != null)
            {
                // Only need to check if is a "file link" or an "internal node link".
                // The "folder links" are checked in the "SpecialNavigation" method
                if (!App.LinkInformation.ActiveLink.Contains("https://mega.nz/#F!"))
                {
                    // Public file link
                    if (App.LinkInformation.ActiveLink.Contains("https://mega.nz/#!"))
                    {
                        SdkService.MegaSdk.getPublicNode(App.LinkInformation.ActiveLink,
                            new GetPublicNodeRequestListener(_mainPageViewModel.CloudDrive));
                    }
                    // Internal file/folder link
                    else if (App.LinkInformation.ActiveLink.Contains("https://mega.nz/#"))
                    {
                        var nodeHandle = App.LinkInformation.ActiveLink.Split("#".ToCharArray())[1];
                        var megaNode = SdkService.MegaSdk.getNodeByBase64Handle(nodeHandle);
                        if (megaNode != null)
                        {
                            ContainerType containerType = (SdkService.MegaSdk.isInRubbish(megaNode)) ?
                                containerType = ContainerType.RubbishBin : containerType = ContainerType.CloudDrive;

                            var node = NodeService.CreateNew(SdkService.MegaSdk, App.AppInformation, megaNode, containerType);

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                if (node != null)
                                {
                                    if (node.IsFolder)
                                        _mainPageViewModel.ActiveFolderView.BrowseToFolder(node);
                                    else
                                        node.Download(TransfersService.MegaTransfers);
                                }
                                else
                                {
                                    new CustomMessageDialog(
                                        AppMessages.AM_InternalNodeNotFound_Title,
                                        AppMessages.AM_InternalNodeNotFound,
                                        App.AppInformation,
                                        MessageDialogButtons.Ok).ShowDialog();
                                }
                            });
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                new CustomMessageDialog(
                                    AppMessages.AM_InternalNodeNotFound_Title,
                                    AppMessages.AM_InternalNodeNotFound,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            });
                        }
                    }
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
            App.LinkInformation.HasFetchedNodesFolderLink = true;

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
            Deployment.Current.Dispatcher.BeginInvoke(() => apiErrorTimer.Stop());

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

                if (apiErrorTimer != null)
                    apiErrorTimer.Stop();
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
                        ShowFolderLinkNoValidAlert();
                }
                else
                {
                    OnSuccesAction(api, request);
                }
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _folderLinkViewModel.FolderLink.SetEmptyContentTemplate(false);
                    _folderLinkViewModel._folderLinkPage.SetApplicationBarData(false);
                });

                switch(e.getErrorCode())
                {
                    case MErrorType.API_ETOOMANY:   // Taken down link and the link owner's account is blocked
                        ShowAssociatedUserAccountTerminatedFolderLinkAlert();
                        break;

                    case MErrorType.API_ENOENT:     // Link not exists or has been deleted by user
                    case MErrorType.API_EBLOCKED:   // Taken down link
                        ShowUnavailableFolderLinkAlert();                        
                        break;

                    default:
                        ShowFolderLinkNoValidAlert();
                        break;
                }
            }
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Disable MainPage appbar buttons
                if (_mainPageViewModel != null) _mainPageViewModel.SetCommandStatus(false);                

                ProgressService.SetProgressIndicator(true,
                   String.Format(ProgressMessages.FetchingNodes, request.getTransferredBytes().ToStringAndSuffix(2)));
            });
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // If is the first error/retry (timer is not running) start the timer
                if (apiErrorTimer != null && !apiErrorTimer.IsEnabled)
                    apiErrorTimer.Start();
            });

            base.onRequestTemporaryError(api, request, e);
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(true, String.Format(ProgressMessages.FetchingNodes,
                    request.getTransferredBytes().ToStringAndSuffix(2)));
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
