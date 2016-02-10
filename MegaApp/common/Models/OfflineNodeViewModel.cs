using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
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
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
        }

        #region Private Methods

        protected void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.IsFolder) return;

            var existingNode = SavedForOffline.ReadNodeByLocalPath(this.NodePath);
            if (existingNode != null)
            {
                this.Base64Handle = existingNode.Base64Handle;

                if (FileService.FileExists(ThumbnailPath))
                {
                    this.IsDefaultImage = false;
                    this.ThumbnailImageUri = new Uri(ThumbnailPath);
                }
                else
                {
                    this.IsDefaultImage = true;
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
                }
            }
        }

        #endregion

        #region IOfflineNode Interface

        public async Task<NodeActionResult> RemoveAsync(bool isMultiRemove, AutoResetEvent waitEventRequest = null)
        {
            if (!isMultiRemove)
            {
                var result = await new CustomMessageDialog(
                AppMessages.DeleteNodeQuestion_Title,
                String.Format(AppMessages.DeleteNodeQuestion, this.Name),
                App.AppInformation,
                MessageDialogButtons.OkCancel).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;
            }            

            await RemoveForOffline(waitEventRequest);

            return NodeActionResult.IsBusy;
        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        public void SetThumbnailImage()
        {
            if (this.IsFolder) return;

            if (this.ThumbnailImageUri != null && !IsDefaultImage) return;

            if (this.IsImage)
            {
                if (FileService.FileExists(ThumbnailPath))
                {
                    this.IsDefaultImage = false;
                    this.ThumbnailImageUri = new Uri(ThumbnailPath);
                }
                else
                {
                    this.IsDefaultImage = true;
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
                }
            }
        }

        #region Private Methods

        public async Task RemoveForOffline(AutoResetEvent waitEventRequest = null)
        {
            String parentNodePath = ((new DirectoryInfo(this.NodePath)).Parent).FullName;

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            if (this.IsFolder)
            {
                await RecursiveRemoveForOffline(parentNodePath, this.Name);
                FolderService.DeleteFolder(this.NodePath, true);                
            }
            else
            {
                // Search if the file has a pending transfer for offline and cancel it on this case
                foreach (var item in App.MegaTransfers.Downloads)
                {
                    var transferItem = (TransferObjectModel)item;
                    if (transferItem == null || transferItem.Transfer == null) continue;

                    WaitHandle waitEventRequestTransfer = new AutoResetEvent(false);
                    if (String.Compare(this.NodePath, transferItem.Transfer.getPath()) == 0 &&
                        transferItem.IsAliveTransfer())
                    {
                        App.MegaSdk.cancelTransfer(transferItem.Transfer,
                            new CancelTransferRequestListener((AutoResetEvent)waitEventRequestTransfer));
                        waitEventRequestTransfer.WaitOne();
                    }
                }
                
                FileService.DeleteFile(this.NodePath);
            }

            SavedForOffline.DeleteNodeByLocalPath(this.NodePath);

            if (this.ParentCollection != null)
                this.ParentCollection.Remove((IOfflineNode)this);

            if (waitEventRequest != null)
                waitEventRequest.Set();
        }

        private async Task RecursiveRemoveForOffline(String sfoPath, String nodeName)
        {
            String newSfoPath = Path.Combine(sfoPath, nodeName);

            // Search if the folder has a pending transfer for offline and cancel it on this case
            foreach (var item in App.MegaTransfers.Downloads)
            {
                WaitHandle waitEventRequest = new AutoResetEvent(false);

                var transferItem = (TransferObjectModel)item;
                if (transferItem == null || transferItem.Transfer == null) continue;

                if (String.Compare(String.Concat(newSfoPath, "\\"), transferItem.Transfer.getParentPath()) == 0 &&
                    transferItem.IsAliveTransfer())
                {
                    App.MegaSdk.cancelTransfer(transferItem.Transfer,
                        new CancelTransferRequestListener((AutoResetEvent)waitEventRequest));
                    waitEventRequest.WaitOne();
                }
            }

            IEnumerable<string> childFolders = Directory.GetDirectories(newSfoPath);
            if (childFolders != null)
            {
                foreach (var folder in childFolders)
                {
                    if (folder != null)
                    {
                        await RecursiveRemoveForOffline(newSfoPath, folder);
                        SavedForOffline.DeleteNodeByLocalPath(Path.Combine(newSfoPath, folder));
                    }
                }
            }

            IEnumerable<string> childFiles = Directory.GetFiles(newSfoPath);
            if (childFiles != null)
            {
                foreach (var file in childFiles)
                {
                    if (file != null)
                        SavedForOffline.DeleteNodeByLocalPath(Path.Combine(newSfoPath, file));
                }
            }
        }

        #endregion

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
                                    AppResources.ThumbnailsDirectory,
                                    this.Base64Handle);
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
