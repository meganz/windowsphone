using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    public class GlobalTransferListener: MTransferListenerInterface
    {
        #region MTransferListenerInterface

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        #if WINDOWS_PHONE_80
        public void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        #elif WINDOWS_PHONE_81
        public async void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        #endif
        {
            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransfersService.SearchTransfer(TransfersService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                
                megaTransfer.Transfer = transfer;
                megaTransfer.TransferState = transfer.getState();
                megaTransfer.TransferPriority = transfer.getPriority();

                TransfersService.GetTransferAppData(transfer, megaTransfer);

                megaTransfer.TotalBytes = transfer.getTotalBytes();
                megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                megaTransfer.TransferSpeed = string.Empty;
                megaTransfer.IsBusy = false;
                megaTransfer.CancelButtonState = false;
            });

            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        megaTransfer.TransferedBytes = megaTransfer.TotalBytes;
                        megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/completed transfers.Screen-WXGA.png", UriKind.Relative);
                        megaTransfer.TransferButtonForegroundColor = (SolidColorBrush)Application.Current.Resources["MegaRedSolidColorBrush"];
                    });
                
                    switch (megaTransfer.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            if (megaTransfer.IsSaveForOfflineTransfer) //If is a save for offline download transfer
                            {
                                var node = megaTransfer.SelectedNode as NodeViewModel;
                                if (node != null)
                                {
                                    // Need get the path on the transfer finish because the file name can be changed
                                    // if already exists in the destiny path.
                                    var newOfflineLocalPath = Path.Combine(transfer.getParentPath(), transfer.getFileName()).Replace("/", "\\");

                                    var sfoNode = new SavedForOffline
                                    {
                                        Fingerprint = SdkService.MegaSdk.getNodeFingerprint(node.OriginalMNode),
                                        Base64Handle = node.OriginalMNode.getBase64Handle(),
                                        LocalPath = newOfflineLocalPath,
                                        IsSelectedForOffline = true
                                    };

                                    // If is a public node (link) the destination folder is the SFO root, so the parent handle
                                    // is the handle of the root node.
                                    if (node.ParentContainerType != ContainerType.PublicLink)
                                        sfoNode.ParentBase64Handle = (SdkService.MegaSdk.getParentNode(node.OriginalMNode)).getBase64Handle();
                                    else
                                        sfoNode.ParentBase64Handle = SdkService.MegaSdk.getRootNode().getBase64Handle();

                                    if (!(SavedForOffline.ExistsNodeByLocalPath(sfoNode.LocalPath)))
                                        SavedForOffline.Insert(sfoNode);
                                    else
                                        SavedForOffline.UpdateNode(sfoNode);

                                    Deployment.Current.Dispatcher.BeginInvoke(() => 
                                    {
                                        node.IsAvailableOffline = node.IsSelectedForOffline = true;
                                        TransfersService.MoveMegaTransferToCompleted(TransfersService.MegaTransfers, megaTransfer);
                                    });

                                    #if WINDOWS_PHONE_80
                                    //If is download transfer of an image file
                                    var imageNode = node as ImageNodeViewModel;
                                    if (imageNode != null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(megaTransfer.TransferPath));

                                        bool exportToPhotoAlbum = SettingsService.LoadSetting<bool>(SettingsResources.ExportImagesToPhotoAlbum, false);
                                        if (exportToPhotoAlbum)
                                            Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.SaveImageToCameraRoll(false));
                                    }
                                    #endif
                                }
                            }
                            else //If is a standard download transfer (no for save for offline)
                            {
                                bool result = true;

                                //If is download transfer of an image file 
                                var imageNode = megaTransfer.SelectedNode as ImageNodeViewModel;
                                if (imageNode != null)
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(megaTransfer.TransferPath));

                                    if (megaTransfer.AutoLoadImageOnFinish)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            if (imageNode.OriginalMNode.hasPreview()) return;
                                            imageNode.PreviewImageUri = new Uri(imageNode.PreviewPath);
                                            imageNode.IsBusy = false;
                                        });
                                    }

                                    #if WINDOWS_PHONE_81
                                    result = await megaTransfer.FinishDownload(megaTransfer.TransferPath, imageNode.Name);
                                    #endif
                                }
                                #if WINDOWS_PHONE_81                                    
                                else //If is a download transfer of other file type 
                                {
                                    var node = megaTransfer.SelectedNode as FileNodeViewModel;
                                    if (node != null)
                                        result = await megaTransfer.FinishDownload(megaTransfer.TransferPath, node.Name);
                                }
                                #endif

                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (!result)
                                        megaTransfer.TransferState = MTransferState.STATE_FAILED;
                                    else
                                        TransfersService.MoveMegaTransferToCompleted(TransfersService.MegaTransfers, megaTransfer);
                                });
                            }
                        break;

                        case MTransferType.TYPE_UPLOAD:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                TransfersService.MoveMegaTransferToCompleted(TransfersService.MegaTransfers, megaTransfer));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case MErrorType.API_EOVERQUOTA: // Storage overquota error
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // Stop all upload transfers
                        api.cancelTransfers((int)MTransferType.TYPE_UPLOAD);

                        // Disable the "camera upload" service
                        MediaService.SetAutoCameraUpload(false);
                        SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);

                        DialogService.ShowOverquotaAlert();
                    });
                    break;

                case MErrorType.API_EINCOMPLETE:
                    Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.TransferState = MTransferState.STATE_CANCELLED);
                    break;

                default:
                    Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.TransferState = MTransferState.STATE_FAILED);
                    switch (megaTransfer.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                new CustomMessageDialog(
                                    AppMessages.DownloadNodeFailed_Title,
                                    String.Format(AppMessages.DownloadNodeFailed, e.getErrorString()),
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            });

                            break;

                        case MTransferType.TYPE_UPLOAD:
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                new CustomMessageDialog(
                                    AppMessages.UploadNodeFailed_Title,
                                    String.Format(AppMessages.UploadNodeFailed, e.getErrorString()),
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            });

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var megaTransfer = TransfersService.AddTransferToList(TransfersService.MegaTransfers, transfer);
                if (megaTransfer != null)
                {
                    TransfersService.GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.Transfer = transfer;
                    megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                    megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferPriority = transfer.getPriority();

                    megaTransfer.CancelButtonState = true;
                    megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                    megaTransfer.TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
                }
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransfersService.SearchTransfer(TransfersService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (DebugService.DebugSettings.IsDebugMode || Debugger.IsAttached)
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]);

                megaTransfer.Transfer = transfer;
                megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                megaTransfer.TransferPriority = transfer.getPriority();

                // Transfer overquota error
                if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
                    DialogService.ShowTransferOverquotaWarning(); 
            });
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Extra checking to avoid NullReferenceException
            if (transfer == null) return;

            // Search the corresponding transfer in the transfers list
            var megaTransfer = TransfersService.SearchTransfer(TransfersService.MegaTransfers.SelectAll(), transfer);
            if (megaTransfer == null) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);

                megaTransfer.Transfer = transfer;
                megaTransfer.IsBusy = api.areTransfersPaused((int)transfer.getType()) ? false : true;
                megaTransfer.TransferState = api.areTransfersPaused((int)transfer.getType()) ? MTransferState.STATE_QUEUED : transfer.getState();
                megaTransfer.TotalBytes = transfer.getTotalBytes();
                megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
                megaTransfer.TransferMeanSpeed = transfer.getMeanSpeed();
                megaTransfer.TransferPriority = transfer.getPriority();

                megaTransfer.CancelButtonState = true;
                megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                megaTransfer.TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
            });
        }

        #endregion
    }
}
