using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using mega;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public class NodeViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;
        // Original MNode object from the MEGA SDK
        private readonly MNode _baseNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public NodeViewModel(MegaSDK megaSdk, MNode baseNode)
        {
            this._megaSdk = megaSdk;
            this._baseNode = baseNode;
            this.Name = baseNode.getName();
            this.Size = baseNode.getSize();
            this.CreationTime = ConvertDateToString(_baseNode.getCreationTime()).ToString("dd MMM yyyy");
            this.SizeAndSuffix = Size.ToStringAndSuffix();
            this.Type = baseNode.getType();

            if(this.Type == MNodeType.TYPE_FOLDER)
                SetFolderInfo();

            SetThumbnailImage();
        }

        private void SetFolderInfo()
        {
            int childFolders = this._megaSdk.getNumChildFolders(this._baseNode);
            int childFiles = this._megaSdk.getNumChildFiles(this._baseNode);
            this.FolderInfo = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder : UiResources.MultipleFolders,
                childFiles, childFiles == 1 ? UiResources.SingleFile : UiResources.MultipleFiles);
        }

        #region Methods

        private void SetThumbnailImage()
        {
            switch (this.Type)
            {
                case MNodeType.TYPE_FOLDER:
                {
                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/folder.png", UriKind.Relative));
                    break;
                }
                case MNodeType.TYPE_FILE:
                {
                    this.ThumbnailImage = ImageService.GetDefaultFileImage(this.Name);

                    if (this.IsImage && this._baseNode.hasThumbnail())
                    {
                        GetThumbnail();
                        break;
                    }
                    
                    break;
                }
                default:
                {
                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));
                    break;
                }
            }
        }

        private void GetThumbnail()
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory, this._baseNode.getBase64Handle());

            if (FileService.FileExists(filePath))
            {
                LoadThumbnailImage(filePath);
            }
            else
            {
                this._megaSdk.getThumbnail(this._baseNode, filePath, new GetThumbnailRequestListener(this));
            }
        }

        public void SetPreviewImage()
        {
            if (!this.IsImage) return;
            if (this.PreviewImage != null) return;
            if (this._baseNode.hasPreview())
            {
                GetPreview();
            }
        }

        private void GetPreview()
        {
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.PreviewsDirectory, this._baseNode.getBase64Handle());

            if (FileService.FileExists(filePath))
            {
                LoadPreviewImage(filePath);
            }
            else
            {
                this._megaSdk.getPreview(this._baseNode, filePath, new GetPreviewRequestListener(this));
            }
        }

        public void LoadThumbnailImage(string path)
        {
            var bitmapImage = new BitmapImage(new Uri(path));
            this.ThumbnailImage = bitmapImage;
        }

        public void LoadPreviewImage(string path)
        {
            var bitmapImage = new BitmapImage(new Uri(path));
            this.PreviewImage = bitmapImage;
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

        #region Properties

        public string Name { get; private set;}

        public ulong Size { get; private set; }

        public MNodeType Type { get; private set ; }

        public string CreationTime { get; private set; }

        public string SizeAndSuffix { get; private set; }

        public string FolderInfo { get; private set; }

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

        public bool IsImage
        {
            get { return ImageService.IsImage(this.Name); }
        }
        public MNode GetBaseNode()
        {
            return this._baseNode;
        }

        #endregion

        
    }
}
