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
        public TransferObjectModel(MegaSDK megaSdk, IMegaNode selectedNode, MTransferType transferType, 
            string transferPath, string externalDownloadPath = null) :base(megaSdk)
        {
            Initialize(selectedNode, transferType, transferPath, externalDownloadPath);
        }

        private async void Initialize(IMegaNode selectedNode, MTransferType transferType,
            string transferPath, string externalDownloadPath = null)
        {
            this.TypeAndState = new object[2];

            switch (transferType)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    DisplayName = selectedNode.Name;
                    TotalBytes = selectedNode.Size;
                    break;

                case MTransferType.TYPE_UPLOAD:
                    DisplayName = Path.GetFileName(transferPath);
                    if (FileService.FileExists(transferPath))
                    {
                        var srcFile = await StorageFile.GetFileFromPathAsync(transferPath);
                        if (srcFile != null)
                        {
                            var fileProperties = await srcFile.GetBasicPropertiesAsync();
                            this.TotalBytes = fileProperties.Size;
                        }
                    }
                    break;
            }

            Type = transferType;
            TransferPath = transferPath;
            ExternalDownloadPath = externalDownloadPath;
            TransferState = MTransferState.STATE_NONE;
            TransferedBytes = 0;
            TransferSpeed = string.Empty;
            SelectedNode = selectedNode;
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
                case MTransferType.TYPE_DOWNLOAD:
                {
                    // Download all nodes with the App instance of the SDK and authorize nodes to be downloaded with this SDK instance.
                    // Needed to allow transfers resumption of folder link nodes.
                    SdkService.MegaSdk.startDownloadWithAppData(this.MegaSdk.authorizeNode(SelectedNode.OriginalMNode),
                        TransferPath, TransfersService.CreateTransferAppDataString(isSaveForOffline, ExternalDownloadPath));
                    this.IsSaveForOfflineTransfer = isSaveForOffline;
                    break;
                }
                case MTransferType.TYPE_UPLOAD:
                {
                    // Start uploads with the flag of temporary source activated to always automatically delete the 
                    // uploaded file from the upload temporary folder in the sandbox of the app
                    SdkService.MegaSdk.startUploadWithDataTempSource(TransferPath, SelectedNode.OriginalMNode, String.Empty, true);
                    break; 
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CancelTransfer(object p = null)
        {
            // If the transfer is an upload and is being prepared (copying file to the upload temporary folder)
            if (this.Type == MTransferType.TYPE_UPLOAD && this.PreparingUploadCancelToken != null)
            {
                this.PreparingUploadCancelToken.Cancel();
                return;
            }

            // If the transfer is ready but not started for some reason
            if (!this.IsBusy && this.TransferState == MTransferState.STATE_NONE)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, string.Format("Transfer ({0}) canceled: {1}",
                    this.Type == MTransferType.TYPE_UPLOAD ? "UPLOAD" : "DOWNLOAD", this.DisplayName));
                this.TransferState = MTransferState.STATE_CANCELLED;
                return;
            }

            SdkService.MegaSdk.cancelTransfer(this.Transfer);
        }

        private void SetThumbnail()
        {
            switch (Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
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
                case MTransferType.TYPE_UPLOAD:
                    {
                        if (ImageService.IsImage(TransferPath))
                        {
                            IsDefaultImage = false;
                            ThumbnailUri = new Uri(TransferPath);
                        }                            
                        else
                        {
                            IsDefaultImage = true;
                            FileTypePathData = ImageService.GetDefaultFileTypePathData(TransferPath);
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
                    ExternalDownloadPath ?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                    null), newFileName);
            }
            else
            {
                return await FileService.CopyFile(sourcePath,
                    ExternalDownloadPath ?? SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation,
                    null), newFileName);
            }
        }
        #endif

        #endregion

        #region Properties

        public string DisplayName { get; set; }
        public string TransferPath { get; private set; }
        public string ExternalDownloadPath { get; set; }
        public IMegaNode SelectedNode { get; private set; }

        private MTransferType _type;
        public MTransferType Type
        {
            get { return _type; }
            set
            {
                SetField(ref _type, value);
                this.TypeAndState[0] = value;
                OnPropertyChanged("TypeAndState");
            }
        }

        public CancellationTokenSource PreparingUploadCancelToken;

        public object[] TypeAndState { get; set; }

        private MTransfer _transfer;
        public MTransfer Transfer
        {
            get { return _transfer; }
            set { SetField(ref _transfer, value); }
        }

        public bool IsFolderTransfer 
        {
            get 
            { 
                return (this.Transfer != null) ? this.Transfer.isFolderTransfer() : !Path.HasExtension(this.TransferPath); 
            }
        }

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

        public bool CancelButtonState
        {
            get 
            { 
                switch(this.TransferState)
                {
                    case MTransferState.STATE_CANCELLED:
                    case MTransferState.STATE_COMPLETED:
                    case MTransferState.STATE_FAILED:
                        return false;
                    default:
                        return true;
                }
            }
        }

        private Uri _transferButtonIcon;
        public Uri TransferButtonIcon
        {
            get { return _transferButtonIcon; }
            set { SetField(ref _transferButtonIcon, value); }
        }

        private MTransferState _transferState;
        public MTransferState TransferState
        {
            get { return _transferState; }
            set
            {
                SetField(ref _transferState, value);

                this.IsBusy = (value == MTransferState.STATE_ACTIVE) ? true : false;
                this.TypeAndState[1] = value;

                OnPropertyChanged("TypeAndState");
                OnPropertyChanged("CancelButtonState");
            }
        }

        private ulong _transferPriority;
        public ulong TransferPriority
        {
            get { return _transferPriority; }
            set { SetField(ref _transferPriority, value); }
        }

        private ulong _totalBytes;
        public ulong TotalBytes
        {
            get { return _totalBytes; }
            set { SetField(ref _totalBytes, value); }
        }

        private ulong _transferedBytes;
        public ulong TransferedBytes
        {
            get { return _transferedBytes; }
            set { SetField(ref _transferedBytes, value); }
        }

        private string _transferSpeed;
        public string TransferSpeed
        {
            get { return _transferSpeed; }
            set { SetField(ref _transferSpeed, value); }
        }

        private ulong _transferMeanSpeed;
        public ulong TransferMeanSpeed
        {
            get { return _transferMeanSpeed; }
            set
            {
                SetField(ref _transferMeanSpeed, value);
                OnPropertyChanged("EstimatedTime");
            }
        }

        #endregion
    }
}
