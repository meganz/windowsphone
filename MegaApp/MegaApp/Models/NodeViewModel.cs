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
            this.NumberOfFiles = this.Type != MNodeType.TYPE_FOLDER ? null : String.Format("{0} {1}", this._megaSdk.getNumChildren(this._baseNode), UiResources.Files);

            SetThumbnailImage();
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

                        if (this.IsImage)
                        {
                            if (this._baseNode.hasThumbnail())
                            {
                                GetThumbnail();
                                break;
                            }
                        }

                        var fileExtension = Path.GetExtension(this.Name);
                        if (fileExtension == null)
                        {
                            this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));
                            break;
                        }

                        switch (fileExtension.ToLower())
                        {
                            case ".accdb":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/accdb.png", UriKind.Relative));
                                    break;
                                }
                            case ".bmp":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/bmp.png", UriKind.Relative));
                                    break;
                                }
                            case ".doc":
                            case ".docx":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/doc.png", UriKind.Relative));
                                    break;
                                }
                            case ".eps":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/eps.png", UriKind.Relative));
                                    break;
                                }
                            case ".gif":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/gif.png", UriKind.Relative));
                                    break;
                                }
                            case ".ico":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/ico.png", UriKind.Relative));
                                    break;
                                }
                            case ".jpg":
                            case ".jpeg":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/jpg.png", UriKind.Relative));
                                    break;
                                }
                            case ".mp3":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/mp3.png", UriKind.Relative));
                                    break;
                                }
                            case ".pdf":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/pdf.png", UriKind.Relative));
                                    break;
                                }
                            case ".png":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/png.png", UriKind.Relative));
                                    break;
                                }
                            case ".ppt":
                            case ".pptx":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/ppt.png", UriKind.Relative));
                                    break;
                                }
                            case ".swf":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/swf.png", UriKind.Relative));
                                    break;
                                }
                            case ".tga":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/tga.png", UriKind.Relative));
                                    break;
                                }
                            case ".tiff":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/tiff.png", UriKind.Relative));
                                    break;
                                }
                            case ".txt":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/txt.png", UriKind.Relative));
                                    break;
                                }
                            case ".wav":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/wav.png", UriKind.Relative));
                                    break;
                                }
                            case ".xls":
                            case ".xlsx":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/xls.png", UriKind.Relative));
                                    break;
                                }
                            case ".zip":
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/zip.png", UriKind.Relative));
                                    break;
                                }
                            default:
                                {
                                    this.ThumbnailImage = new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));
                                    break;
                                }
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

        public string NumberOfFiles { get; private set; }

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
