using mega;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using System.Threading;
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
    public abstract class NodeViewModel : BaseSdkViewModel
    {
        public event EventHandler CancelingTransfer;
        // Original MNode object from the MEGA SDK
        private MNode _originalMegaNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        protected NodeViewModel(MegaSDK megaSdk, MNode megaNode, object parentCollection = null, object childCollection = null)
            : base(megaSdk)
        {
            SetCoreNodeData(megaNode);
            SetDefaultValues();
            
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;

            this.MegaService = new MegaService();
        }

        #region Interfaces

        public IMegaService MegaService { get; set; }

        #endregion

        #region Abstract Methods

        public abstract void OpenFile();

        #endregion

        #region Virtual Methods

        public virtual void Rename()
        {
            if (!IsUserOnline()) return;
            MegaService.Rename(this.MegaSdk, this);
        }

        public virtual void Move(NodeViewModel newParentNode)
        {
            if (!IsUserOnline()) return;
            MegaService.Move(this.MegaSdk, this, newParentNode);
        }

        public virtual void Remove(bool isMultiRemove, AutoResetEvent waitEventRequest = null)
        {
            if (!IsUserOnline()) return;
            MegaService.Remove(this.MegaSdk, this, isMultiRemove, waitEventRequest);
        }

        public virtual void GetPreviewLink()
        {
            if (!IsUserOnline()) return;
            MegaService.GetPreviewLink(this.MegaSdk, this);
        }

        public async void Download(string downloadPath = null)
        {
            if (!IsUserOnline()) return;
            
            if (App.CloudDrive.PickerOrDialogIsOpen) return;

            if (downloadPath == null)
            {
                App.CloudDrive.PickerOrDialogIsOpen = true;
                if (!await FolderService.SelectDownloadFolder(this)) return;
            }
                

            this.Transfer.DownloadFolderPath = downloadPath;
            App.MegaTransfers.Add(this.Transfer);
            this.Transfer.StartTransfer();
            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }

        public virtual void Update(MNode megaNode)
        {
            SetCoreNodeData(megaNode);
        }

        protected virtual void OnCancelingTransfer()
        {
            if (CancelingTransfer == null) return;

            CancelingTransfer(this, new EventArgs());
        }

        #endregion

        #region Private Methods

        private void SetCoreNodeData(MNode megaNode)
        {
            _originalMegaNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Name = megaNode.getName();
            this.Size = megaNode.getSize();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");
            this.ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");
            this.Type = megaNode.getType();
        }

        private void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (FileService.FileExists(ThumbnailPath))
            {
                this.ThumbnailIsDefaultImage = false;
                this.ThumbnailImageUri = new Uri(ThumbnailPath);
            }
            else
            {
                this.ThumbnailIsDefaultImage = true;
                this.ThumbnailImageUri = ImageService.GetDefaultFileImage(this.Name);
            }
        }

        private void GetThumbnail()
        {
            //if (FileService.FileExists(ThumbnailPath))
            //{
            //    ThumbnailIsDefaultImage = false;
            //    ThumbnailImageUri = new Uri(ThumbnailPath);
            //}
            //else
            //{
                if (Convert.ToBoolean(MegaSdk.isLoggedIn()))
                {
                    //System.Threading.ThreadPool.QueueUserWorkItem(state => 
                    this.MegaSdk.getThumbnail(this.GetMegaNode(), ThumbnailPath, new GetThumbnailRequestListener(this));
                }
            //}
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

        #region Public Methods
      
        public void CancelTransfer()
        {
            OnCancelingTransfer();
        }

        public void SetThumbnailImage()
        {
            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (this.ThumbnailImageUri != null && !ThumbnailIsDefaultImage) return;
            
            if (this.IsImage || this.GetMegaNode().hasThumbnail())
            {
                GetThumbnail();
            }
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

        public string ModificationTime { get; private set; }

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
                                      this.GetMegaNode().getBase64Handle());
            }
        }

        public MNode GetMegaNode()
        {
            return this._originalMegaNode;
        }

        #endregion
    }
}
