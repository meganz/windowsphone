using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public GlobalTransferListener()
        {
            this.Transfers = new List<TransferObjectModel>();
        }

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
            var megaTransfer = Transfers.FirstOrDefault(t => t.Transfer.getTag() == transfer.getTag());
            if(megaTransfer != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);

                    TransfersService.GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
                    megaTransfer.IsBusy = false;
                    megaTransfer.CancelButtonState = false;
                });

                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                megaTransfer.TransferedBytes = megaTransfer.TotalBytes;
                                megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/completed transfers.Screen-WXGA.png", UriKind.Relative);
                                megaTransfer.TransferButtonForegroundColor = (SolidColorBrush)Application.Current.Resources["MegaRedSolidColorBrush"];
                            });

                            switch (megaTransfer.Type)
                            {
                                case TransferType.Download:
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
                                                Fingerprint = App.MegaSdk.getNodeFingerprint(node.OriginalMNode),
                                                Base64Handle = node.OriginalMNode.getBase64Handle(),
                                                LocalPath = newOfflineLocalPath,
                                                IsSelectedForOffline = true
                                            };

                                            // If is a public node (link) the destination folder is the SFO root, so the parent handle
                                            // is the handle of the root node.
                                            if (node.ParentContainerType != ContainerType.PublicLink)
                                                sfoNode.ParentBase64Handle = (App.MegaSdk.getParentNode(node.OriginalMNode)).getBase64Handle();
                                            else
                                                sfoNode.ParentBase64Handle = App.MegaSdk.getRootNode().getBase64Handle();

                                            if (!(SavedForOffline.ExistsNodeByLocalPath(sfoNode.LocalPath)))
                                                SavedForOffline.Insert(sfoNode);
                                            else
                                                SavedForOffline.UpdateNode(sfoNode);

                                            Deployment.Current.Dispatcher.BeginInvoke(() => node.IsAvailableOffline = node.IsSelectedForOffline = true);

                                            #if WINDOWS_PHONE_80
                                            //If is download transfer of an image file
                                            var imageNode = node as ImageNodeViewModel;
                                            if (imageNode != null)
                                            {
                                                Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(megaTransfer.FilePath));

                                                bool exportToPhotoAlbum = SettingsService.LoadSetting<bool>(SettingsResources.ExportImagesToPhotoAlbum, false);
                                                if (exportToPhotoAlbum)
                                                    Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.SaveImageToCameraRoll(false));
                                            }
                                            #endif
                                        }
                                    }
                                    else //If is a standard download transfer (no for save for offline)
                                    {
                                        //If is download transfer of an image file 
                                        var imageNode = megaTransfer.SelectedNode as ImageNodeViewModel;
                                        if (imageNode != null)
                                        {
                                            Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(megaTransfer.FilePath));

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
                                            if (!await megaTransfer.FinishDownload(megaTransfer.FilePath, imageNode.Name))
                                            {
                                                Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Error);
                                                break;
                                            }
                                            #endif
                                        }
                                        #if WINDOWS_PHONE_81
                                        else //If is a download transfer of other file type 
                                        {
                                            var node = megaTransfer.SelectedNode as FileNodeViewModel;
                                            if (node != null)
                                            {

                                                if (!await megaTransfer.FinishDownload(megaTransfer.FilePath, node.Name))
                                                {
                                                    Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Error);
                                                    break;
                                                }
                                            }
                                        }
                                        #endif
                                    }

                                    Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Downloaded);
                                    break;

                                case TransferType.Upload:
                                    Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Uploaded);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                    case MErrorType.API_EOVERQUOTA:
                        {
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
                        }
                    case MErrorType.API_EINCOMPLETE:
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Canceled);
                            break;
                        }
                    default:
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => megaTransfer.Status = TransferStatus.Error);
                            switch (megaTransfer.Type)
                            {
                                case TransferType.Download:
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        new CustomMessageDialog(
                                            AppMessages.DownloadNodeFailed_Title,
                                            String.Format(AppMessages.DownloadNodeFailed, e.getErrorString()),
                                            App.AppInformation,
                                            MessageDialogButtons.Ok).ShowDialog();
                                    });

                                    break;

                                case TransferType.Upload:
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
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            TransferObjectModel megaTransfer = null;
            if (transfer.getType() == MTransferType.TYPE_DOWNLOAD)
            {
                // If is a public node
                MNode node = transfer.getPublicMegaNode();
                if (node == null) // If not
                    node = api.getNodeByHandle(transfer.getNodeHandle());

                if (node != null)
                {
                    megaTransfer = new TransferObjectModel(api,
                        NodeService.CreateNew(api, App.AppInformation, node, ContainerType.CloudDrive),
                        TransferType.Download, transfer.getPath());
                }
            }
            else
            {
                megaTransfer = new TransferObjectModel(api, App.MainPageViewModel.CloudDrive.FolderRootNode,
                    TransferType.Upload, transfer.getPath());
            }

            if (megaTransfer != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TransfersService.GetTransferAppData(transfer, megaTransfer);

                    megaTransfer.Transfer = transfer;
                    megaTransfer.Status = TransferStatus.Queued;
                    megaTransfer.CancelButtonState = true;
                    megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                    megaTransfer.TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
                    megaTransfer.IsBusy = true;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();

                    App.MegaTransfers.Add(megaTransfer);
                    Transfers.Add(megaTransfer);
                });                
            }
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            if (DebugService.DebugSettings.IsDebugMode || Debugger.IsAttached)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));
            }            
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            var megaTransfer = Transfers.FirstOrDefault(t => t.Transfer.getTag() == transfer.getTag());
            if(megaTransfer != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);

                    megaTransfer.CancelButtonState = true;
                    megaTransfer.TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                    megaTransfer.TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
                    megaTransfer.IsBusy = true;
                    megaTransfer.TotalBytes = transfer.getTotalBytes();
                    megaTransfer.TransferedBytes = transfer.getTransferredBytes();
                    megaTransfer.TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();

                    if (megaTransfer.TransferedBytes > 0)
                    {
                        switch (megaTransfer.Type)
                        {
                            case TransferType.Download:
                                megaTransfer.Status = TransferStatus.Downloading;
                                break;
                            case TransferType.Upload:
                                megaTransfer.Status = TransferStatus.Uploading;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                });
            }
        }

        #endregion

        #region Properties

        public IList<TransferObjectModel> Transfers { get; private set; }        

        #endregion
    }
}
