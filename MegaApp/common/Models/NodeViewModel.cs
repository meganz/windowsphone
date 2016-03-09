using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;

namespace MegaApp.Models
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public abstract class NodeViewModel : BaseAppInfoAwareViewModel, IMegaNode
    {
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);        

        protected NodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation)
        {
            Update(megaNode, parentContainerType);
            SetDefaultValues();
            
            this.ParentCollection = parentCollection;
            this.ChildCollection = childCollection;
        }

        #region Private Methods
        
        private void SetDefaultValues()
        {
            this.IsMultiSelected = false;
            this.DisplayMode = NodeDisplayMode.Normal;

            if (this.Type == MNodeType.TYPE_FOLDER) return;

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

        private void GetThumbnail()
        {
            if (Convert.ToBoolean(MegaSdk.isLoggedIn()))
                this.MegaSdk.getThumbnail(OriginalMNode, ThumbnailPath, new GetThumbnailRequestListener(this));
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

        #region IMegaNode Interface

        public NodeActionResult Rename()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            // Only 1 CustomInputDialog should be open at the same time.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return NodeActionResult.Cancelled;

            var settings = new CustomInputDialogSettings()
            {
                DefaultText = this.Name,
                SelectDefaultText = true,
                IgnoreExtensionInSelection = true,
            };

            var inputDialog = new CustomInputDialog(UiResources.Rename, UiResources.RenameItem, this.AppInformation, settings);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                this.MegaSdk.renameNode(this.OriginalMNode, args.InputText, new RenameNodeRequestListener(this));
            };
            inputDialog.ShowDialog();

            return NodeActionResult.IsBusy;
        }

        public NodeActionResult Move(IMegaNode newParentNode)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (this.MegaSdk.checkMove(this.OriginalMNode, newParentNode.OriginalMNode).getErrorCode() == MErrorType.API_OK)
            {
                this.MegaSdk.moveNode(this.OriginalMNode, newParentNode.OriginalMNode,
                    new MoveNodeRequestListener());
                return NodeActionResult.IsBusy;
            }

            return NodeActionResult.Failed;
        }

        public async Task<NodeActionResult> RemoveAsync(bool isMultiRemove, AutoResetEvent waitEventRequest = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            // Looking for the absolute parent of the node to remove
            MNode parentNode;
            MNode absoluteParentNode = this.OriginalMNode;

            while ((parentNode = this.MegaSdk.getParentNode(absoluteParentNode)) != null)
                absoluteParentNode = parentNode;

            // If the node is on the rubbish bin, delete it forever
            if (absoluteParentNode.getType() == MNodeType.TYPE_RUBBISH)
            {
                if (!isMultiRemove)
                {
                    var result = await new CustomMessageDialog(
                        AppMessages.RemoveItemQuestion_Title,
                        String.Format(AppMessages.RemoveItemQuestion, this.Name),
                        App.AppInformation,
                        MessageDialogButtons.OkCancel,
                        MessageDialogImage.RubbishBin).ShowDialogAsync();

                    if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;
                }

                this.MegaSdk.remove(this.OriginalMNode, new RemoveNodeRequestListener(this, isMultiRemove, absoluteParentNode.getType(),
                    waitEventRequest));
                
                return NodeActionResult.IsBusy;
            }

            // if the node in in the Cloud Drive, move it to rubbish bin
            if (!isMultiRemove)
            {
                var result = await new CustomMessageDialog(
                    AppMessages.MoveToRubbishBinQuestion_Title,
                    String.Format(AppMessages.MoveToRubbishBinQuestion, this.Name),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel,
                    MessageDialogImage.RubbishBin).ShowDialogAsync();

                if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;
            }

            this.MegaSdk.moveNode(this.OriginalMNode, this.MegaSdk.getRubbishNode(),
                new RemoveNodeRequestListener(this, isMultiRemove, absoluteParentNode.getType(), waitEventRequest));

            return NodeActionResult.IsBusy;
        }

        public async Task<NodeActionResult> DeleteAsync()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;


            var result = await new CustomMessageDialog(
                AppMessages.DeleteNodeQuestion_Title,
                String.Format(AppMessages.DeleteNodeQuestion, this.Name),
                App.AppInformation,
                MessageDialogButtons.OkCancel).ShowDialogAsync();

            if (result == MessageDialogResult.CancelNo) return NodeActionResult.Cancelled;

            this.MegaSdk.remove(this.OriginalMNode, new RemoveNodeRequestListener(this, false, this.Type, null));

            return NodeActionResult.IsBusy;
        }

        public NodeActionResult GetLink()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            this.MegaSdk.exportNode(this.OriginalMNode, new ExportNodeRequestListener());

            return NodeActionResult.IsBusy;
        }

        public NodeActionResult RemoveLink()
        {
            if (!IsExported) return NodeActionResult.Cancelled;

            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            this.MegaSdk.disableExport(this.OriginalMNode, new DisableExportRequestListener());

            return NodeActionResult.IsBusy;
        }

#if WINDOWS_PHONE_80
        public virtual void Download(TransferQueu transferQueu, string downloadPath = null)
        {
            if (!IsUserOnline()) return;
            //NavigateService.NavigateTo(typeof(DownloadPage), NavigationParameter.Normal, this);
            
            SaveForOffline(transferQueu);

            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }
#elif WINDOWS_PHONE_81
        public async void Download(TransferQueu transferQueu, string downloadPath = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;
            
            if (AppInformation.PickerOrAsyncDialogIsOpen) return;

            if (downloadPath == null)
                if (!await FolderService.SelectDownloadFolder(this)) return;

            if (String.IsNullOrWhiteSpace(downloadPath))
            {
                await new CustomMessageDialog(AppMessages.SelectFolderFailed_Title,
                    AppMessages.SelectFolderFailedNoErrorCode, App.AppInformation, 
                    MessageDialogButtons.Ok).ShowDialogAsync();
                return;
            }

            // Check for illegal characters in the download path
            if (FolderService.HasIllegalChars(downloadPath))
            {
                await new CustomMessageDialog(AppMessages.SelectFolderFailed_Title,
                    String.Format(AppMessages.InvalidFolderNameOrPath, downloadPath),
                    this.AppInformation).ShowDialogAsync();
                return;
            }
                        
            // If selected file is a folder then also select it childnodes to download
            bool result;
            if(this.IsFolder)
                result = await RecursiveDownloadFolder(transferQueu, downloadPath, this);
            else
                result = await DownloadFile(transferQueu, downloadPath, this);

            // TODO Remove this global declaration in method
            App.CloudDrive.NoFolderUpAction = true;
            if (!result || !transferQueu.Any(t => t.IsAliveTransfer())) return;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }        

        private async Task<bool> RecursiveDownloadFolder(TransferQueu transferQueu, String downloadPath, NodeViewModel folderNode)
        {            
            if (String.IsNullOrWhiteSpace(folderNode.Name))
            {
                await new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                    AppMessages.AM_DownloadNodeFailedNoErrorCode, this.AppInformation).ShowDialogAsync();
                return false;
            }

            // Check for illegal characters in the folder name
            if (FolderService.HasIllegalChars(folderNode.Name))
            {
                await new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                    String.Format(AppMessages.InvalidFolderNameOrPath, folderNode.Name),
                    this.AppInformation).ShowDialogAsync();
                return false;
            }
            
            String newDownloadPath = Path.Combine(downloadPath, folderNode.Name);
            StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(downloadPath);

            if (!FolderService.FolderExists(newDownloadPath))
                await downloadFolder.CreateFolderAsync(folderNode.Name, CreationCollisionOption.OpenIfExists);
            
            MNodeList childList = MegaSdk.getChildren(folderNode.OriginalMNode);

            bool result = true; // Default value in case that the folder is empty
            for (int i=0; i < childList.size(); i++)
            {
                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;

                var childNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, childList.get(i), ContainerType.CloudDrive);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (childNode == null) continue;

                bool partialResult;
                if (childNode.IsFolder)
                    partialResult = await RecursiveDownloadFolder(transferQueu, newDownloadPath, childNode);
                else
                    partialResult= await DownloadFile(transferQueu, newDownloadPath, childNode);

                // Only change the global result if the partial result indicates an error
                if (!partialResult) result = partialResult;
            }

            return result;
        }

        private async Task<bool> DownloadFile(TransferQueu transferQueu, String downloadPath, NodeViewModel fileNode)
        {
            if (String.IsNullOrWhiteSpace(fileNode.Name))
            {
                await new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                    AppMessages.AM_DownloadNodeFailedNoErrorCode, App.AppInformation).ShowDialogAsync();
                return false;
            }

            if (FileService.HasIllegalChars(fileNode.Name))
            {
                await new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                    String.Format(AppMessages.InvalidFileName, fileNode.Name),
                    App.AppInformation).ShowDialogAsync();
                return false;
            }
                        
            try
            {
                if (!FolderService.FolderExists(Path.GetDirectoryName(fileNode.Transfer.FilePath)))
                    FolderService.CreateFolder(Path.GetDirectoryName(fileNode.Transfer.FilePath));

                fileNode.Transfer.DownloadFolderPath = downloadPath;
                transferQueu.Add(fileNode.Transfer);
                fileNode.Transfer.StartTransfer();
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is NotSupportedException)
                    new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                        String.Format(AppMessages.InvalidFileName, fileNode.Transfer.FilePath),
                        App.AppInformation).ShowDialog();

                if (e is PathTooLongException)
                    new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                        String.Format(AppMessages.PathTooLong, fileNode.Transfer.FilePath),
                        App.AppInformation).ShowDialog();

                if (e is UnauthorizedAccessException)
                    new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                        String.Format(AppMessages.FolderUnauthorizedAccess, Path.GetDirectoryName(fileNode.Transfer.FilePath)),
                        App.AppInformation).ShowDialog();

                return false;
            }

            return true;
        }
#endif

        public async Task<bool> SaveForOffline(TransferQueu transferQueu)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return false;

            MNode parentNode = App.MegaSdk.getParentNode(this.OriginalMNode);

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            String parentNodePath;
            if(ParentContainerType != ContainerType.PublicLink)
            {
                parentNodePath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""),
                    (App.MegaSdk.getNodePath(parentNode)).Remove(0, 1).Replace("/", "\\"));
            }
            else 
            {
                // If is a public node (link) the destination folder is the SFO root
                parentNodePath = sfoRootPath;
            }            

            if (!FolderService.FolderExists(parentNodePath))
                FolderService.CreateFolder(parentNodePath);

            if (this.IsFolder)
                await RecursiveSaveForOffline(transferQueu, parentNodePath, this);
            else
                await SaveFileForOffline(transferQueu, parentNodePath, this);
            
            this.IsAvailableOffline = this.IsSelectedForOffline = true;

            // Check and add to the DB if necessary the previous folders of the path
            while (String.Compare(parentNodePath, sfoRootPath) != 0)
            {
                var folderPathToAdd = parentNodePath;
                parentNodePath = ((new DirectoryInfo(parentNodePath)).Parent).FullName;

                if (!SavedForOffline.ExistsNodeByLocalPath(folderPathToAdd))
                    SavedForOffline.Insert(parentNode);

                parentNode = App.MegaSdk.getParentNode(parentNode);
            }

            return true;
        }

        private async Task RecursiveSaveForOffline(TransferQueu transferQueu, String sfoPath, NodeViewModel node)
        {
            if (!FolderService.FolderExists(sfoPath))
                FolderService.CreateFolder(sfoPath);

            String newSfoPath = Path.Combine(sfoPath, node.Name);

            if (!FolderService.FolderExists(newSfoPath))
                FolderService.CreateFolder(newSfoPath);

            if (!SavedForOffline.ExistsNodeByLocalPath(newSfoPath))
                SavedForOffline.Insert(node.OriginalMNode, true);
            else
                SavedForOffline.UpdateNode(node.OriginalMNode, true);

            MNodeList childList = MegaSdk.getChildren(node.OriginalMNode);

            for (int i = 0; i < childList.size(); i++)
            {
                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;

                var childNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, childList.get(i), this.ParentContainerType);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (childNode == null) continue;

                if (childNode.IsFolder)
                    await RecursiveSaveForOffline(transferQueu, newSfoPath, childNode);
                else
                    await SaveFileForOffline(transferQueu, newSfoPath, childNode);
            }
        }

        private async Task<bool> SaveFileForOffline(TransferQueu transferQueu, String sfoPath, NodeViewModel node)
        {
            if (FileService.FileExists(Path.Combine(sfoPath, node.Name))) return true;

            var existingNode = SavedForOffline.ReadNodeByFingerprint(MegaSdk.getNodeFingerprint(node.OriginalMNode));
            if (existingNode != null)
            {                
                bool result = await FileService.CopyFile(existingNode.LocalPath, sfoPath);

                if (!result) return false;
                
                SavedForOffline.Insert(node.OriginalMNode, true);
            }
            else
            {                
                transferQueu.Add(node.Transfer);                
                node.Transfer.StartTransfer(true);
            }

            return true;
        }

        public async Task RemoveForOffline()
        {
            MNode parentNode = App.MegaSdk.getParentNode(this.OriginalMNode);

            String parentNodePath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                AppResources.DownloadsDirectory.Replace("\\", ""),
                (App.MegaSdk.getNodePath(parentNode)).Remove(0, 1).Replace("/", "\\"));

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            var nodePath = Path.Combine(parentNodePath, this.Name);

            if (this.IsFolder)
            {
                await RecursiveRemoveForOffline(parentNodePath, this.Name);
                FolderService.DeleteFolder(nodePath, true);
            }                
            else
            {
                // Search if the file has a pending transfer for offline and cancel it on this case
                foreach (var item in App.MegaTransfers.Downloads)
                {
                    WaitHandle waitEventRequest = new AutoResetEvent(false);

                    var transferItem = (TransferObjectModel)item;
                    if (transferItem == null || transferItem.Transfer == null) continue;

                    if (String.Compare(nodePath, transferItem.Transfer.getPath()) == 0 &&
                        transferItem.IsAliveTransfer())
                    {
                        MegaSdk.cancelTransfer(transferItem.Transfer,
                            new CancelTransferRequestListener((AutoResetEvent)waitEventRequest));
                        waitEventRequest.WaitOne();
                    }
                }
                
                FileService.DeleteFile(nodePath);                
            }

            SavedForOffline.DeleteNodeByLocalPath(nodePath);            
            this.IsAvailableOffline = this.IsSelectedForOffline = false;

            // Check if the previous folders of the path are empty and 
            // remove from the offline and the DB on this case
            while (String.Compare(parentNodePath, sfoRootPath) != 0)
            {
                var folderPathToRemove = parentNodePath;
                parentNodePath = ((new DirectoryInfo(parentNodePath)).Parent).FullName;

                if (FolderService.IsEmptyFolder(folderPathToRemove))
                {
                    FolderService.DeleteFolder(folderPathToRemove);
                    SavedForOffline.DeleteNodeByLocalPath(folderPathToRemove);
                }
            }
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
                    MegaSdk.cancelTransfer(transferItem.Transfer,
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

        public void Update(MNode megaNode, ContainerType parentContainerType)
        {
            OriginalMNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Base64Handle = megaNode.getBase64Handle();
            this.Type = megaNode.getType();
            this.ParentContainerType = parentContainerType;
            this.Name = megaNode.getName();
            this.Size = MegaSdk.getSize(megaNode);
            this.SizeText = this.Size.ToStringAndSuffix();
            this.IsExported = megaNode.isExported();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");

            if (this.Type == MNodeType.TYPE_FILE)
                this.ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");                
            else
                this.ModificationTime = this.CreationTime;

            if(!App.MegaSdk.isInShare(megaNode) && this.ParentContainerType != ContainerType.PublicLink &&
                this.ParentContainerType != ContainerType.InShares && this.ParentContainerType != ContainerType.ContactInShares)
                CheckAndUpdateSFO(megaNode);
        }        

        private void CheckAndUpdateSFO(MNode megaNode)
        {
            this.IsAvailableOffline = false;
            this.IsSelectedForOffline = false;

            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,
                    App.MegaSdk.getNodePath(megaNode).Remove(0, 1).Replace("/", "\\"));

            if(SavedForOffline.ExistsNodeByLocalPath(nodeOfflineLocalPath))            
            {
                var existingNode = SavedForOffline.ReadNodeByLocalPath(nodeOfflineLocalPath);
                if ((megaNode.getType() == MNodeType.TYPE_FILE && FileService.FileExists(nodeOfflineLocalPath)) ||
                    (megaNode.getType() == MNodeType.TYPE_FOLDER && FolderService.FolderExists(nodeOfflineLocalPath)))
                {
                    this.IsAvailableOffline = true;
                    this.IsSelectedForOffline = existingNode.IsSelectedForOffline;
                }                    
                else
                    SavedForOffline.DeleteNodeByLocalPath(nodeOfflineLocalPath);
            }
            else
            {
                if (megaNode.getType() == MNodeType.TYPE_FILE && FileService.FileExists(nodeOfflineLocalPath))
                {
                    SavedForOffline.Insert(megaNode, true);
                    this.IsAvailableOffline = this.IsSelectedForOffline = true;
                }
                else if (megaNode.getType() == MNodeType.TYPE_FOLDER && FolderService.FolderExists(nodeOfflineLocalPath))
                {
                    SavedForOffline.Insert(megaNode);
                    this.IsAvailableOffline = true;
                    this.IsSelectedForOffline = false;
                }
            }
        }

        public virtual void Open()
        {
            throw new NotImplementedException();
        }

        public void SetThumbnailImage()
        {
            if (this.Type == MNodeType.TYPE_FOLDER) return;

            if (this.ThumbnailImageUri != null && !IsDefaultImage) return;
            
            if (this.IsImage || this.OriginalMNode.hasThumbnail())
            {
                GetThumbnail();
            }
        }

        #region Interface Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        public string CreationTime { get; private set; }

        public string ModificationTime { get; private set; }

        public string ThumbnailPath
        {
            get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                                      AppResources.ThumbnailsDirectory, 
                                      this.OriginalMNode.getBase64Handle());
            }
        }

        private string _information;
        public string Information
        {
            get { return _information; }
            set { SetField(ref _information, value); }
        }

        public ulong Handle { get; set; }

        public String Base64Handle { get; set; }

        public ulong Size { get; set; }

        private string _sizeText;
        public string SizeText
        {
            get { return _sizeText; }
            set { SetField(ref _sizeText, value); }
        }

        public ObservableCollection<IMegaNode> ParentCollection { get; set; }

        public ObservableCollection<IMegaNode> ChildCollection { get; set; }

        public MNodeType Type { get; private set; }

        public ContainerType ParentContainerType { get; private set; }

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

        public bool IsFolder
        {
            get { return Type == MNodeType.TYPE_FOLDER ? true : false; }
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

        private bool _isSelectedForOffline;
        public bool IsSelectedForOffline
        {
            get { return _isSelectedForOffline; }
            set
            {
                SetField(ref _isSelectedForOffline, value);
                IsSelectedForOfflineText = _isSelectedForOffline ? UiResources.On : UiResources.Off;
            }
        }

        private String _isSelectedForOfflineText;
        public String IsSelectedForOfflineText
        {
            get { return _isSelectedForOfflineText; }
            set
            {
                SetField(ref _isSelectedForOfflineText, value);
            }
        }

        private bool _isAvailableOffline;
        public bool IsAvailableOffline
        {
            get { return _isAvailableOffline; }
            set 
            {
                SetField(ref _isAvailableOffline, value);
            }
        }        

        private bool _isExported;
        public bool IsExported
        {
            get { return _isExported; }
            set { SetField(ref _isExported, value); }
        }

        public TransferObjectModel Transfer { get; set; }

        public MNode OriginalMNode { get; private set; }

        #endregion

        #endregion
    }
}
