using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.ViewManagement;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class TransferObjectModel : BaseSdkViewModel, MTransferListenerInterface
    {
        public TransferObjectModel(MegaSDK megaSdk, IMegaNode selectedNode, TransferType transferType, string filePath) 
            :base(megaSdk)
        {
            switch (transferType)
            {
                case TransferType.Download:
                    {
                        DisplayName = selectedNode.Name;
                        break;
                    }
                case TransferType.Upload:
                    {
                        DisplayName = Path.GetFileName(filePath);
                        break;
                    }
            }
            Type = transferType;
            FilePath = filePath;
            Status = TransferStatus.NotStarted;
            SelectedNode = selectedNode;
            CancelButtonState = true;
            TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
            AutoLoadImageOnFinish = false;
            CancelTransferCommand = new DelegateCommand(CancelTransfer);
            SetThumbnail();
        }
        
        #region Commands

        public ICommand CancelTransferCommand { get; set; }

        #endregion

        #region Methods

        public void StartTransfer(bool isSaveForOffline = false)
        {
            switch (Type)
            {
                case TransferType.Download:
                {
                    this.IsSaveForOfflineTransfer = isSaveForOffline;
                    this.MegaSdk.startDownload(SelectedNode.OriginalMNode, FilePath, this);
                    break;
                }
                case TransferType.Upload:
                {
                    this.MegaSdk.startUpload(FilePath, SelectedNode.OriginalMNode, this);
                    break; 
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CancelTransfer(object p = null)
        {
            if (!IsBusy)
            {
                if(Status == TransferStatus.NotStarted)
                    Status = TransferStatus.Canceled;
                return;
            }
            Status = TransferStatus.Canceling;
            MegaSdk.cancelTransfer(Transfer);
        }

        private void SetThumbnail()
        {
            switch (Type)
            {
                case TransferType.Download:
                    {
                        IsDefaultImage = true;
                        FileTypePathData = ImageService.GetDefaultFileTypePathData(SelectedNode.Name);
                        if (FileService.FileExists(SelectedNode.ThumbnailPath))
                        {
                            IsDefaultImage = false;
                            ThumbnailUri = new Uri(SelectedNode.ThumbnailPath);
                        }
                        break;
                    }
                case TransferType.Upload:
                    {
                        if (ImageService.IsImage(FilePath))
                        {
                            IsDefaultImage = false;
                            ThumbnailUri = new Uri(FilePath);
                        }                            
                        else
                        {
                            IsDefaultImage = true;
                            FileTypePathData = ImageService.GetDefaultFileTypePathData(FilePath);
                        }
                            
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }

        public bool IsAliveTransfer()
        {
            switch(this.Status)
            {
                case TransferStatus.Canceled:
                case TransferStatus.Downloaded:
                case TransferStatus.Uploaded:
                case TransferStatus.Error:
                    return false;
            }

            return true;
        }

        #if WINDOWS_PHONE_81
        private async Task<bool> FinishDownload(String sourcePath, String newFileName)
        {
            if (!SavedForOffline.ExistsNodeByLocalPath(sourcePath))
            {
                return await FileService.MoveFile(sourcePath,
                    DownloadFolderPath ?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                    null), newFileName);
            }
            else
            {
                return await FileService.CopyFile(sourcePath,
                    DownloadFolderPath ?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                    null), newFileName);
            }
        }
        #endif

        #endregion

        #region Properties

        public string DisplayName { get; set; }
        public string FilePath { get; private set; }
        public string DownloadFolderPath { get; set; }
        public TransferType Type { get; set; }
        public IMegaNode SelectedNode { get; private set; }
        public MTransfer Transfer { get; private set; }

        private bool _isDefaultImage;
        public bool IsDefaultImage
        {
            get { return _isDefaultImage; }
            set { SetField(ref _isDefaultImage, value); }
        }

        private Uri _thumbnailUri;
        public Uri ThumbnailUri
        {
            get { return _thumbnailUri; }
            set { SetField(ref _thumbnailUri, value); }
        }

        private string _fileTypePathData;
        public string FileTypePathData
        {
            get { return _fileTypePathData; }
            set { SetField(ref _fileTypePathData, value);  }
        }

        public bool AutoLoadImageOnFinish { get; set; }

        public bool IsSaveForOfflineTransfer { get; set; }

        private bool _cancelButtonState;
        public bool CancelButtonState
        {
            get { return _cancelButtonState; }
            set
            {
                _cancelButtonState = value;
                OnPropertyChanged("CancelButtonState");
            }
        }

        private Uri _transferButtonIcon;
        public Uri TransferButtonIcon
        {
            get { return _transferButtonIcon; }
            private set
            {
                _transferButtonIcon = value;
                OnPropertyChanged("TransferButtonIcon");
            }
        }

        private SolidColorBrush _transferButtonForegroundColor;
        public SolidColorBrush TransferButtonForegroundColor
        {
            get { return _transferButtonForegroundColor; }
            set
            {
                _transferButtonForegroundColor = value;
                OnPropertyChanged("TransferButtonForegroundColor");
            }
        }

        private TransferStatus _transferStatus;
        public TransferStatus Status
        {
            get { return _transferStatus; }
            set
            {
                _transferStatus = value;
                OnPropertyChanged("Status");
            }
        }

        private ulong _totalBytes;
        public ulong TotalBytes
        {
            get { return _totalBytes; }
            set
            {
                _totalBytes = value;
                OnPropertyChanged("TotalBytes");
            }
        }

        private ulong _transferedBytes;
        public ulong TransferedBytes
        {
            get { return _transferedBytes; }
            set
            {
                _transferedBytes = value;
                OnPropertyChanged("TransferedBytes");
            }
        }

        private string _transferSpeed;
        public string TransferSpeed
        {
            get { return _transferSpeed; }
            private set
            {
                _transferSpeed = value;
                OnPropertyChanged("TransferSpeed");
            }
        }

        #endregion

        #region MTransferListenerInterface

        //Will be called only for transfers started by startStreaming
        //Return true to continue getting data, false to stop the streaming
        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public async void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)        
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();
                TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
                IsBusy = false;
                CancelButtonState = false;                
            });

            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        TransferedBytes = TotalBytes;
                        TransferButtonIcon = new Uri("/Assets/Images/completed transfers.Screen-WXGA.png", UriKind.Relative);
                        TransferButtonForegroundColor = (SolidColorBrush)Application.Current.Resources["MegaRedSolidColorBrush"];
                    });                    
                    
                    switch(Type)
                    {
                        case TransferType.Download:
                            if (IsSaveForOfflineTransfer) //If is a save for offline download transfer
                            {
                                var node = SelectedNode as NodeViewModel;
                                if (node != null)
                                {
                                    // Need get the path on the transfer finish because  the file name can be changed
                                    // if already exists in the destiny path.
                                    var newOfflineLocalPath = Path.Combine(transfer.getParentPath(), transfer.getFileName()).Replace("/", "\\");

                                    var sfoNode = new SavedForOffline
                                    {
                                        Fingerprint = MegaSdk.getNodeFingerprint(node.OriginalMNode),
                                        Base64Handle = node.OriginalMNode.getBase64Handle(),
                                        LocalPath = newOfflineLocalPath,
                                        IsSelectedForOffline = true
                                    };

                                    // Checking to try avoid NullRefenceExceptions (Possible bug #4761)
                                    if(sfoNode != null)
                                    {
                                        // If is a public node (link) the destination folder is the SFO root, so the parent handle
                                        // is the handle of the root node.
                                        if (node.ParentContainerType != ContainerType.PublicLink)
                                            sfoNode.ParentBase64Handle = (MegaSdk.getParentNode(node.OriginalMNode)).getBase64Handle();
                                        else
                                            sfoNode.ParentBase64Handle = MegaSdk.getRootNode().getBase64Handle();

                                        if (!(SavedForOffline.ExistsNodeByLocalPath(sfoNode.LocalPath)))
                                            SavedForOffline.Insert(sfoNode);
                                        else
                                            SavedForOffline.UpdateNode(sfoNode);
                                    }                                    

                                    Deployment.Current.Dispatcher.BeginInvoke(() => node.IsAvailableOffline = node.IsSelectedForOffline = true);

                                    #if WINDOWS_PHONE_80
                                    //If is download transfer of an image file
                                    var imageNode = node as ImageNodeViewModel;
                                    if (imageNode != null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(FilePath));

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
                                var imageNode = SelectedNode as ImageNodeViewModel;
                                if (imageNode != null)
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() => imageNode.ImageUri = new Uri(FilePath));

                                    if (AutoLoadImageOnFinish)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            if (imageNode.OriginalMNode.hasPreview()) return;
                                            imageNode.PreviewImageUri = new Uri(imageNode.PreviewPath);
                                            imageNode.IsBusy = false;
                                        });
                                    }
                                    
                                    #if WINDOWS_PHONE_81
                                    if(!await FinishDownload(FilePath,imageNode.Name))
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Error);
                                        break;
                                    }
                                    #endif
                                }
                                #if WINDOWS_PHONE_81
                                else //If is a download transfer of other file type 
                                {
                                    var node = SelectedNode as FileNodeViewModel;
                                    if (node != null)
                                    {
                                        
                                        if (!await FinishDownload(FilePath, node.Name))
                                        {
                                            Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Error);
                                            break;
                                        }                                        
                                    }
                                }
                                #endif
                            }

                            Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Downloaded);
                            break;
                     
                        case TransferType.Upload:
                            Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Uploaded);
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
                        if (App.MegaTransfers.Count > 0)
                        {
                            foreach (var item in App.MegaTransfers)
                            {
                                var transferItem = (TransferObjectModel)item;
                                if (transferItem == null) continue;

                                if (transferItem.Type == TransferType.Upload)
                                    transferItem.CancelTransfer();
                            }
                        }

                        // Disable the "camera upload" service
                        MediaService.SetAutoCameraUpload(false);
                        SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, false);

                        DialogService.ShowOverquotaAlert();
                    });

                    break;
                }
                case MErrorType.API_EINCOMPLETE:
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Canceled);
                    break;
                }
                default:
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Error);
                    switch (Type)
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

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            Transfer = transfer;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                Status = TransferStatus.Queued;
                CancelButtonState = true;
                TransferButtonIcon = new Uri("/Assets/Images/cancel transfers.Screen-WXGA.png", UriKind.Relative);
                TransferButtonForegroundColor = new SolidColorBrush(Colors.White);
                IsBusy = true;
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();
                TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
            });
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
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();

                TransferSpeed = transfer.getSpeed().ToStringAndSuffixPerSecond();
                //TransferTime.Stop();
                //CalculateTransferSpeed(TransferTime.Elapsed, transfer.getDeltaSize());
                //ransferTime.Restart();
                
                if (TransferedBytes > 0)
                {
                    switch (Type)
                    {
                        case TransferType.Download:
                            Status = TransferStatus.Downloading;
                            break;
                        case TransferType.Upload:
                            Status = TransferStatus.Uploading;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            });
        }

        private void CalculateTransferSpeed(TimeSpan elepsedTransferTime, ulong transferedBytes)
        {
            double bytesPerSecond = transferedBytes / elepsedTransferTime.TotalSeconds;
            double bitsPerSecond = bytesPerSecond * 8;

            TransferSpeed = ((ulong) bitsPerSecond).ToStringAndSuffixPerSecond();
        }

        #endregion
    }
}
