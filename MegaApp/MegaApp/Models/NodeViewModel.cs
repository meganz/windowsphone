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
        private readonly MNode _baseMegaNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public NodeViewModel(MegaSDK megaSdk, MNode baseMegaNode, object parentCollection = null, object childCollection = null)
            : base(megaSdk)
        {
            this._baseMegaNode = baseMegaNode;
            this.DisplayMode = NodeDisplayMode.Normal;
            this.Name = baseMegaNode.getName();
            this.Size = baseMegaNode.getSize();
            this.CreationTime = ConvertDateToString(baseMegaNode.getCreationTime()).ToString("dd MMM yyyy");
            this.SizeAndSuffix = Size.ToStringAndSuffix();
            this.Type = baseMegaNode.getType();
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
            this.Transfer = new TransferObjectModel(MegaSdk, this, TransferType.Download, ImagePath);

            this.MegaService = new MegaService();

            if(this.Type == MNodeType.TYPE_FOLDER)
                SetFolderInfo();
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

        public void Remove()
        {
            if (!IsUserOnline()) return;
            MegaService.Remove(this.MegaSdk, this);
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
        }

        public void CancelTransfer()
        {
            OnCancelingTransfer();
        }

        protected virtual void OnCancelingTransfer()
        {
            if (CancelingTransfer == null) return;

            CancelingTransfer(this, new EventArgs());
        }

        private void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(this._baseMegaNode);
            int childFiles = this.MegaSdk.getNumChildFiles(this._baseMegaNode);
            this.FolderInfo = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder : UiResources.MultipleFolders,
                childFiles, childFiles == 1 ? UiResources.SingleFile : UiResources.MultipleFiles);
        }

        public void SetThumbnailImage()
        {
            if (this.ThumbnailImage != null) return;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

            ThumbnailIsDefaultImage = true;
            this.ThumbnailImage = ImageService.GetDefaultFileImage(this.Name);
            
            if (this.IsImage && this.GetMegaNode().hasThumbnail())
            {
                GetThumbnail();
            }
        }

        private void GetThumbnail()
        {
            if (FileService.FileExists(ThumbnailPath))
            {
                LoadThumbnailImage(ThumbnailPath);
            }
            else
            {
                if(Convert.ToBoolean(MegaSdk.isLoggedIn()))
                    this.MegaSdk.getThumbnail(this.GetMegaNode(), ThumbnailPath, new GetThumbnailRequestListener(this));
            }
        }

        public void SetPreviewImage()
        {
            if (this.PreviewImage != null && this.PreviewImage != this.ThumbnailImage) return;
            if (this.IsBusy) return;
            if (!this.IsImage) return;
          
            if (this.GetMegaNode().hasPreview())
            {
                GetPreview();
            }
            else
            {
                GetImage();
            }
        }

        private void GetPreview()
        {
            if (FileService.FileExists(PreviewPath))
            {
                LoadPreviewImage(PreviewPath);
            }
            else
            {
                this.MegaSdk.getPreview(this._baseMegaNode, PreviewPath, new GetPreviewRequestListener(this));
            }
        }

        public void SetImage()
        {
            if (this.Image != null) return;
            if (this.IsBusy) return;
            if (!this.IsImage) return;
            GetImage();
        }

        private void GetImage()
        {
            if (FileService.FileExists(ImagePath))
            {
                LoadImage(ImagePath);
            }
            else
            {
                Transfer.AutoLoadImageOnFinish = true;
                Transfer.StartTransfer();
            }
        }

        public void LoadThumbnailImage(string path)
        {
            ThumbnailIsDefaultImage = false;
            this.ThumbnailImage = null;
            this.ThumbnailImage = new BitmapImage
            {
                DecodePixelHeight = Convert.ToInt32(AppResources.ThumbnailHeight),
                DecodePixelWidth = Convert.ToInt32(AppResources.ThumbnailWidth),
                DecodePixelType = DecodePixelType.Logical
            };
            this.ThumbnailImage.ImageFailed += ThumbnailImageOnImageFailed;
            this.ThumbnailImage.UriSource = new Uri(path);

        }

        public void LoadPreviewImage(string path)
        {
            this.PreviewImage = null;
            this.PreviewImage = new BitmapImage();
            this.PreviewImage.ImageFailed += PreviewImageOnImageFailed;
            this.PreviewImage.UriSource = new Uri(path);
        }

        public void LoadImage(string path)
        {
            this.Image = null;
            this.Image = new BitmapImage();
            this.Image.ImageFailed += ImageOnImageFailed;
            this.Image.UriSource = new Uri(path);

            if (this.GetMegaNode().hasPreview()) return;

            LoadPreviewImage(path);
        }

        public void SaveImageToCameraRoll()
        {
            if (this.Image == null) return;

            if (MessageBox.Show(AppMessages.SaveImageQuestion, AppMessages.SaveImageQuestion_Title,
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

            if (ImageService.SaveToCameraRoll(this.Name, this.Image))
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

        private void ImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            MessageBox.Show("DEBUG: " + exceptionRoutedEventArgs.ErrorException.Message);
            var bitmapImage = new BitmapImage(new Uri("/Assets/Images/preview_error.png", UriKind.Relative));
            this.Image = bitmapImage;
        }

        private void PreviewImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            MessageBox.Show("DEBUG: " + exceptionRoutedEventArgs.ErrorException.Message);
            var bitmapImage = new BitmapImage(new Uri("/Assets/Images/preview_error.png", UriKind.Relative));
            this.PreviewImage = bitmapImage;
        }

        private void ThumbnailImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            this.ThumbnailImage = ImageService.GetDefaultFileImage(this.Name);
        }

        #endregion

        #region Properties

        public TransferObjectModel Transfer { get; set; }

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

        public ulong Size { get; private set; }

        public MNodeType Type { get; private set ; }

        public string CreationTime { get; private set; }

        public string SizeAndSuffix { get; private set; }

        public string FolderInfo { get; private set; }

        public object ParentCollection { get; set; }
        public object ChildCollection { get; set; }

        public bool ThumbnailIsDefaultImage { get; set; }

        private BitmapImage _thumbnailImage;
        public BitmapImage ThumbnailImage
        {
            get { return _thumbnailImage; }
            set
            {
                _thumbnailImage = value;
                OnPropertyChanged("ThumbnailImage");
            }
        }

        private BitmapImage _previewImage;
        public BitmapImage PreviewImage
        {
            get { return _previewImage; }
            set
            {
                _previewImage = value;
                OnPropertyChanged("PreviewImage");
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
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
