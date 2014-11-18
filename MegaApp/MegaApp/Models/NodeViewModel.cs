using mega;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;

namespace MegaApp.Models
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public class NodeViewModel : BaseSdkViewModel
    {

        public event EventHandler CancelingTransfer;
        // Original MNode object from the MEGA SDK
        private MNode _baseMegaNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public NodeViewModel(MegaSDK megaSdk, MNode baseMegaNode, object parentCollection = null, object childCollection = null)
            : base(megaSdk)
        {
            SetCoreNodeData(baseMegaNode);
            //this._baseMegaNode = baseMegaNode;
            this.DisplayMode = NodeDisplayMode.Normal;
            //this.Name = baseMegaNode.getName();
            //this.Size = baseMegaNode.getSize();
            //this.CreationTime = ConvertDateToString(baseMegaNode.getCreationTime()).ToString("dd MMM yyyy");
            //this.SizeAndSuffix = Size.ToStringAndSuffix();
            //this.Type = baseMegaNode.getType();
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
            this.Transfer = new TransferObjectModel(MegaSdk, this, TransferType.Download, ImagePath);
            this.IsMultiSelected = false;

            this.MegaService = new MegaService();

            //if(this.Type == MNodeType.TYPE_FOLDER)
            //    SetFolderInfo();

            if (this.Type != MNodeType.TYPE_FILE) return;
            
            ThumbnailIsDefaultImage = true;
            this.ThumbnailImageUri = ImageService.GetDefaultFileImage(this.Name);
            InViewingRange = false;
        }

        #region Interfaces

        public IMegaService MegaService { get; set; }

        #endregion

        #region Methods

        public void Rename()
        {
            if (!IsUserOnline()) return;
            MegaService.Rename(this.MegaSdk, this);
        }

        public void Move(NodeViewModel newParentNode)
        {
            if (!IsUserOnline()) return;
            MegaService.Move(this.MegaSdk, this, newParentNode);
        }

        public void Remove(bool isMultiRemove)
        {
            if (!IsUserOnline()) return;
            MegaService.Remove(this.MegaSdk, this, isMultiRemove);
        }
        
        public void GetPreviewLink()
        {
            if (!IsUserOnline()) return;
            MegaService.GetPreviewLink(this.MegaSdk, this);
        }

        public void ViewOriginal()
        {
            if (!IsUserOnline()) return;
            NavigateService.NavigateTo(typeof(DownloadImagePage), NavigationParameter.Normal, this);
        }

        public bool HasPreviewInCache()
        {
            return FileService.FileExists(PreviewPath);
        }

        public void CancelPreviewRequest()
        {
            MegaSdk.cancelGetPreview(GetMegaNode());
            IsBusy = false;
        }

        public void CancelTransfer()
        {
            OnCancelingTransfer();
        }

        public void Update(MNode megaNode)
        {
           SetCoreNodeData(megaNode);
        }

        protected virtual void OnCancelingTransfer()
        {
            if (CancelingTransfer == null) return;

            CancelingTransfer(this, new EventArgs());
        }

        public void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(this._baseMegaNode);
            int childFiles = this.MegaSdk.getNumChildFiles(this._baseMegaNode);
            this.FolderInfo = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder : UiResources.MultipleFolders,
                childFiles, childFiles == 1 ? UiResources.SingleFile : UiResources.MultipleFiles);
        }

        private void SetCoreNodeData(MNode megaNode)
        {
            this._baseMegaNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Name = megaNode.getName();
            this.Size = megaNode.getSize();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");
            this.SizeAndSuffix = Size.ToStringAndSuffix();
            this.Type = megaNode.getType();
            if (this.Type == MNodeType.TYPE_FOLDER)
                SetFolderInfo();
        }

        public void SetThumbnailImage()
        {
            if (this.ThumbnailImageUri != null && !ThumbnailIsDefaultImage) return;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

            //ThumbnailIsDefaultImage = true;
            //this.ThumbnailImageUri = ImageService.GetDefaultFileImage(this.Name);
            
            if (this.IsImage || this.GetMegaNode().hasThumbnail())
            {
                GetThumbnail();
            }
        }

        private void GetThumbnail()
        {
            if (FileService.FileExists(ThumbnailPath))
            {
                ThumbnailImageUri = new Uri(ThumbnailPath);
                ThumbnailIsDefaultImage = false;
            }
            else
            {
                if(Convert.ToBoolean(MegaSdk.isLoggedIn()))
                    this.MegaSdk.getThumbnail(this.GetMegaNode(), ThumbnailPath, new GetThumbnailRequestListener(this));
            }
        }

        public void SetPreviewImage()
        {
            //string previewUri = PreviewImageUri != null ? PreviewImageUri.ToString() : null;
            //string thumbnailUri = ThumbnailImageUri != null ? ThumbnailImageUri.ToString() : null;

            //if (this.PreviewImageUri != null && Path.GetFileName(previewUri) != Path.GetFileName(thumbnailUri)) return;
            if (this.IsBusy) return;
            if (!this.IsImage && !this.GetMegaNode().hasPreview()) return;

            if (this.GetMegaNode().hasPreview())
            {
                GetPreview();
            }
            else
            {
                GetImage(true);
            }
        }

        private void GetPreview()
        {
            if (FileService.FileExists(PreviewPath))
            {
                PreviewImageUri = new Uri(PreviewPath);
            }
            else
            {
                this.MegaSdk.getPreview(this._baseMegaNode, PreviewPath, new GetPreviewRequestListener(this));
            }
        }

        public void SetImage()
        {
            if (this.ImageUri != null) return;
            if (this.IsBusy) return;
            if (!this.IsImage) return;
            GetImage();
        }

        private void GetImage(bool isForPreview = false)
        {
            if (FileService.FileExists(ImagePath))
            {
                ImageUri = new Uri(ImagePath);

                if (!isForPreview) return;

                PreviewImageUri = new Uri(ImagePath);

            }
            else
            {
                if (isForPreview)
                    IsBusy = true;
                Transfer.AutoLoadImageOnFinish = true;
                Transfer.StartTransfer();
            }
        }
       
        public void SaveImageToCameraRoll()
        {
            if (this.ImageUri == null) return;

            if (MessageBox.Show(AppMessages.SaveImageQuestion, AppMessages.SaveImageQuestion_Title,
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

            if (ImageService.SaveToCameraRoll(this.Name, this.ImageUri))
                MessageBox.Show(AppMessages.ImageSaved, AppMessages.ImageSaved_Title, MessageBoxButton.OK);
            else
                MessageBox.Show(AppMessages.ImageSaveError, AppMessages.ImageSaveError_Title, MessageBoxButton.OK);
        }

        /// <summary>
        /// Convert the MEGA time to a C# DateTime object in local time
        /// </summary>
        /// <param name="time">MEGA time</param>
        /// <returns>DateTime object in local time</returns>
        private static DateTime ConvertDateToString(ulong time)
        {
            return OriginalDateTime.AddSeconds(time).ToLocalTime();
        }

        #endregion

        #region Events

        //private void ImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        //{
        //    MessageBox.Show("DEBUG: " + exceptionRoutedEventArgs.ErrorException.Message);
        //    var bitmapImage = new BitmapImage(new Uri("/Assets/Images/preview_error.png", UriKind.Relative));
        //    this.Image = bitmapImage;
        //}

        //private void PreviewImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        //{
        //    MessageBox.Show("DEBUG: " + exceptionRoutedEventArgs.ErrorException.Message);
        //    var bitmapImage = new BitmapImage(new Uri("/Assets/Images/preview_error.png", UriKind.Relative));
        //    this.PreviewImage = bitmapImage;
        //}

        //private void ThumbnailImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        //{
        //    this.ThumbnailImage = ImageService.GetDefaultFileImage(this.Name);
        //}

        #endregion

        #region Properties

        public TransferObjectModel Transfer { get; set; }

        public bool InViewingRange { get; set; }

        private NodeDisplayMode _displayMode;
        public NodeDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set
            {
                _displayMode = value;
                OnPropertyChanged("DisplayMode");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public ulong Handle { get; set; }

        public ulong Size { get; private set; }

        public MNodeType Type { get; private set ; }

        public string CreationTime { get; private set; }

        public string SizeAndSuffix { get; private set; }

        private string _folderInfo;
        public string FolderInfo
        {
            get { return _folderInfo; }
            private set
            {
                _folderInfo = value;
                OnPropertyChanged("FolderInfo");
            }
        }

        public object ParentCollection { get; set; }
        public object ChildCollection { get; set; }

        public bool ThumbnailIsDefaultImage { get; set; }

        private Uri _thumbnailImageUri;
        public Uri ThumbnailImageUri
        {
            get { return _thumbnailImageUri; }
            set
            {
                _thumbnailImageUri = value;
                OnPropertyChanged("ThumbnailImageUri");
            }
        }

        private Uri _previewImageUri;
        public Uri PreviewImageUri
        {
            get
            {
                if (_previewImageUri == null && InViewingRange)
                    SetPreviewImage();
                return _previewImageUri;
            }
            set
            {
                _previewImageUri = value;
                OnPropertyChanged("PreviewImageUri");
            }
        }

        private Uri _imageUri;
        public Uri ImageUri
        {
            get { return _imageUri; }
            set
            {
                _imageUri = value;
                OnPropertyChanged("ImageUri");
            }
        }

        private bool _isMultiSelected;
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set
            {
                _isMultiSelected = value;
                OnPropertyChanged("IsMultiSelected");
            }
        }

        public bool IsImage
        {
            get { return ImageService.IsImage(this.Name); }
        }

        public string ThumbnailPath
        {
            get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                                      AppResources.ThumbnailsDirectory, 
                                      this.GetMegaNode().getBase64Handle()); }
        }

        public string PreviewPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.PreviewsDirectory,
                                    this.GetMegaNode().getBase64Handle());
            }
        }

        public string ImagePath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory,
                                    this.GetMegaNode().getBase64Handle());
            }
        }

        public MNode GetMegaNode()
        {
            return this._baseMegaNode;
        }

        #endregion
    }
}
