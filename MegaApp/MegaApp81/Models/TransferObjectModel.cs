using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class TransferObjectModel : BaseSdkViewModel, MTransferListenerInterface
    {
        public TransferObjectModel(MegaSDK megaSdk, NodeViewModel selectedNode, TransferType transferType, string filePath) 
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
            CancelButtonState = false;
            AutoLoadImageOnFinish = false;
            CancelTransferCommand = new DelegateCommand(CancelTransfer);
            SetThumbnail();
        }
        
        #region Commands

        public ICommand CancelTransferCommand { get; set; }

        #endregion

        #region Methods

        public void StartTransfer()
        {
            switch (Type)
            {
                case TransferType.Download:
                {
                    this.MegaSdk.startDownload(SelectedNode.GetMegaNode(), FilePath, this);
                    break;
                }
                case TransferType.Upload:
                {
                    MegaSdk.startUpload(FilePath, SelectedNode.GetMegaNode(), this);
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
                        ThumbnailUri = ImageService.GetDefaultFileImage(SelectedNode.Name);
                        if (FileService.FileExists(SelectedNode.ThumbnailPath))
                        {
                            ThumbnailUri = new Uri(SelectedNode.ThumbnailPath); ;
                        }
                        break;
                    }
                case TransferType.Upload:
                    {
                        if (ImageService.IsImage(FilePath))
                            ThumbnailUri = new Uri(FilePath);
                        else
                            ThumbnailUri = ImageService.GetDefaultFileImage(FilePath);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        #endregion

        #region Properties

        public string DisplayName { get; set; }
        public string FilePath { get; private set; }
        public string DownloadFolderPath { get; set; }
        public TransferType Type { get; set; }
        public NodeViewModel SelectedNode { get; private set; }
        public MTransfer Transfer { get; private set; }
        private Uri _thumbnailUri;
        public Uri ThumbnailUri
        {
            get { return _thumbnailUri; }
            private set
            {
                _thumbnailUri = value;
                OnPropertyChanged("ThumbnailUri");
            }
        }
        public bool AutoLoadImageOnFinish { get; set; }

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
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();
                IsBusy = false;
                CancelButtonState = false;
            });

            switch (e.getErrorCode())
            {
                case MErrorType.API_OK:
                {
                    var imageNode = SelectedNode as ImageNodeViewModel;
                    if (imageNode != null)
                    {
                        if (AutoLoadImageOnFinish)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                imageNode.ImageUri = new Uri(imageNode.LocalImagePath);
                                imageNode.IsDownloadAvailable = File.Exists(imageNode.LocalImagePath);
                                if (imageNode.GetMegaNode().hasPreview()) return;
                                imageNode.PreviewImageUri = new Uri(imageNode.LocalImagePath);
                                imageNode.IsBusy = false;
                            });
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                imageNode.IsDownloadAvailable = File.Exists(imageNode.LocalImagePath);
                            });
                            imageNode.ImageUri = new Uri(imageNode.LocalImagePath);
                        }

                        bool result = await FileService.CopyFile(imageNode.LocalImagePath,
                            DownloadFolderPath?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                            null));

                        if (!result)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Error);
                            break;
                        }
                        
                    }
                    else
                    {
                        var node = SelectedNode as FileNodeViewModel;
                        if (node != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => node.IsDownloadAvailable = File.Exists(node.LocalFilePath));

                            bool result = await FileService.CopyFile(node.LocalFilePath,
                                DownloadFolderPath ?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                                null)); ;

                            if (!result)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Error);
                                break;
                            }
                        }
                    }
                    

                    Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Finished);
                    break;
                }
                case MErrorType.API_EINCOMPLETE:
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => Status = TransferStatus.Canceled);
                    break;
                }
                default:
                {
                    Status = TransferStatus.Error;
                    switch (Type)
                    {
                        case TransferType.Download:
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                MessageBox.Show(String.Format(AppMessages.DownloadNodeFailed, e.getErrorString()),
                                    AppMessages.DownloadNodeFailed_Title, MessageBoxButton.OK));
                            break;
                        }
                    case TransferType.Upload:
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
                Status = TransferStatus.Connecting;
                CancelButtonState = true;
                IsBusy = true;
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();
            });
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            // Do nothing
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                TotalBytes = transfer.getTotalBytes();
                TransferedBytes = transfer.getTransferredBytes();
                
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

        #endregion
    }
}
