using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public abstract class OfflineNodeViewModel : BaseViewModel, IOfflineNode
    {
        protected OfflineNodeViewModel(ObservableCollection<IOfflineNode> parentCollection = null, 
            ObservableCollection<IOfflineNode> childCollection = null)
            : base()
        {
            SetDefaultValues();

            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
        }

        #region Private Methods

        private void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.IsFolder) return;
        }

        #endregion

        #region IOfflineNode Interface

        public async Task DeleteAsync()
        {

        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        #region Interface Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        private string _nodePath;
        public string NodePath
        {
            get { return _nodePath; }
            set { SetField(ref _nodePath, value); }
        }

        public string CreationTime { get; set; }

        public string ModificationTime { get; set; }

        public string ThumbnailPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.ThumbnailsDirectory);
            }
        }

        private string _information;
        public string Information
        {
            get { return _information; }
            set { SetField(ref _information, value); }
        }

        public String Base64Handle { get; set; }

        public ulong Size { get; set; }

        private string _sizeText;
        public string SizeText
        {
            get { return _sizeText; }
            set { SetField(ref _sizeText, value); }
        }

        public ObservableCollection<IOfflineNode> ParentCollection { get; set; }

        public ObservableCollection<IOfflineNode> ChildCollection { get; set; }

        private NodeDisplayMode _displayMode;
        public NodeDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set { SetField(ref _displayMode, value); }
        }

        private bool _isMultiSelected;
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        private bool _isFolder;
        public bool IsFolder
        {
            get { return _isFolder; }
            set { SetField(ref _isFolder, value); }
        }

        public bool IsImage
        {
            get { return ImageService.IsImage(this.Name); }
        }

        private bool _IsDefaultImage;
        public bool IsDefaultImage
        {
            get { return _IsDefaultImage; }
            set { SetField(ref _IsDefaultImage, value); }
        }

        private Uri _thumbnailImageUri;
        public Uri ThumbnailImageUri
        {
            get { return _thumbnailImageUri; }
            set { SetField(ref _thumbnailImageUri, value); }
        }

        private string _defaultImagePathData;
        public string DefaultImagePathData
        {
            get { return _defaultImagePathData; }
            set { SetField(ref _defaultImagePathData, value); }
        }

        #endregion

        #endregion
    }
}
