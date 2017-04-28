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
    public class TransferObjectModel : BaseSdkViewModel
    {
        public TransferObjectModel(MegaSDK megaSdk, IMegaNode selectedNode, TransferType transferType, 
            string filePath, string downloadFolderPath = null) :base(megaSdk)
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
            DownloadFolderPath = downloadFolderPath;
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
                    // Download all nodes with the App instance of the SDK and authorize nodes to be downloaded with this SDK instance.
                    // Needed to allow transfers resumption of folder link nodes.
                    App.MegaSdk.startDownloadWithAppData(this.MegaSdk.authorizeNode(SelectedNode.OriginalMNode), 
                        FilePath, TransfersService.CreateTransferAppDataString(isSaveForOffline, DownloadFolderPath));
                    this.IsSaveForOfflineTransfer = isSaveForOffline;
                    break;
                }
                case TransferType.Upload:
                {
                    // Start uploads with the flag of temporary source activated to always automatically delete the 
                    // uploaded file from the upload temporary folder in the sandbox of the app
                    App.MegaSdk.startUploadWithDataTempSource(FilePath, SelectedNode.OriginalMNode, String.Empty, true);
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
            App.MegaSdk.cancelTransfer(Transfer);
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

        #if WINDOWS_PHONE_81
        public async Task<bool> FinishDownload(String sourcePath, String newFileName)
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
        public MTransfer Transfer { get; set; }

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
            set
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
            set
            {
                _transferSpeed = value;
                OnPropertyChanged("TransferSpeed");
            }
        }

        #endregion
    }
}
