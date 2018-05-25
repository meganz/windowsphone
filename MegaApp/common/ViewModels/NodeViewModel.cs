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
using Telerik.Windows.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
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
            if (Convert.ToBoolean(MegaSdk.isLoggedIn()) || ParentContainerType == ContainerType.FolderLink)
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

            if (this.MegaSdk.checkMove(this.OriginalMNode, newParentNode.OriginalMNode).getErrorCode() != MErrorType.API_OK)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(AppMessages.MoveFailed_Title, AppMessages.MoveFailed,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                });
                
                return NodeActionResult.Failed;
            }

            this.MegaSdk.moveNode(this.OriginalMNode, newParentNode.OriginalMNode, new MoveNodeRequestListener());
            return NodeActionResult.IsBusy;
        }

        /// <summary>
        /// Move the node from its current location to a new folder destination
        /// </summary>
        /// <param name="newParentNode">The root node of the destination folder</param>
        /// <returns>Result of the action</returns>
        public NodeActionResult Copy(IMegaNode newParentNode)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;            

            this.MegaSdk.copyNode(this.OriginalMNode, newParentNode.OriginalMNode, new CopyNodeRequestListener());
            return NodeActionResult.IsBusy;
        }

        public async Task<NodeActionResult> RemoveAsync(bool isMultiRemove, AutoResetEvent waitEventRequest = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;

            if (this.OriginalMNode == null) return NodeActionResult.Failed;

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

            if (this.OriginalMNode.isExported())
            {
                DialogService.ShowShareLink(this);
                return NodeActionResult.Succeeded;
            }
            else
            {
                this.MegaSdk.exportNode(this.OriginalMNode, new ExportNodeRequestListener(this));
                return NodeActionResult.IsBusy;
            }
        }

        public NodeActionResult SetLinkExpirationTime(long expireTime)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return NodeActionResult.NotOnline;
            if (expireTime < 0) return NodeActionResult.Failed;

            this.MegaSdk.exportNodeWithExpireTime(this.OriginalMNode, expireTime, new ExportNodeRequestListener(this));

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
        public virtual void Download(TransferQueue transferQueue, string downloadPath = null)
        {
            if (!IsUserOnline()) return;
            SaveForOffline();
        }
#elif WINDOWS_PHONE_81
        public async void Download(TransferQueue transferQueue, string downloadPath = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;
            
            if (AppInformation.PickerOrAsyncDialogIsOpen) return;

            if (downloadPath == null)
            {
                if (!await FolderService.SelectDownloadFolder(this)) { return; }
                else { downloadPath = AppService.GetSelectedDownloadDirectoryPath(); }
            }

            OnUiThread(() => ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareDownloads));

            // Extra check to try avoid null values
            if (String.IsNullOrWhiteSpace(downloadPath))
            {
                OnUiThread(() => ProgressService.SetProgressIndicator(false));
                await new CustomMessageDialog(AppMessages.SelectFolderFailed_Title,
                    AppMessages.SelectFolderFailed, App.AppInformation, 
                    MessageDialogButtons.Ok).ShowDialogAsync();
                return;
            }

            // Check for illegal characters in the download path
            if (FolderService.HasIllegalChars(downloadPath))
            {
                OnUiThread(() => ProgressService.SetProgressIndicator(false));
                await new CustomMessageDialog(AppMessages.SelectFolderFailed_Title,
                    String.Format(AppMessages.InvalidFolderNameOrPath, downloadPath),
                    this.AppInformation).ShowDialogAsync();                
                return;
            }

            if (!await CheckDownloadPath(downloadPath))
            {
                OnUiThread(() => ProgressService.SetProgressIndicator(false));
                return;
            }
                        
            // If selected file is a folder then also select it childnodes to download
            if(this.IsFolder)
                await RecursiveDownloadFolder(downloadPath, this);
            else
                await DownloadFile(downloadPath, this);

            OnUiThread(() => ProgressService.SetProgressIndicator(false));

            // TODO Remove this global declaration in method
            App.CloudDrive.NoFolderUpAction = true;            
        }

        private async Task<bool> CheckDownloadPath(String downloadPath)
        {
            bool pathExists = true; //Suppose that exists
            try { await StorageFolder.GetFolderFromPathAsync(downloadPath); }
            catch (FileNotFoundException) { pathExists = false; }
            catch (UnauthorizedAccessException)
            {
                new CustomMessageDialog(AppMessages.AM_DowloadPathUnauthorizedAccess_Title,
                    AppMessages.AM_DowloadPathUnauthorizedAccess,
                    this.AppInformation).ShowDialog();
                return false;
            }
            catch (Exception e)
            {
                String exceptionMessage = e.GetType().Name + " - " + e.Message;
                new CustomMessageDialog(AppMessages.AM_DownloadFailed_Title,
                    String.Format(AppMessages.AM_DownloadPathUnknownError, exceptionMessage),
                    this.AppInformation).ShowDialog();
                return false;
            }

            if (!pathExists) 
                return await CreateDownloadPath(downloadPath);

            return true;
        }

        private async Task<bool> CreateDownloadPath(String downloadPath)
        {
            String rootPath = Path.GetPathRoot(downloadPath);
            String tempDownloadPath = downloadPath;
                        
            List<String> foldersNames = new List<String>(); //Folders that will be needed create
            List<String> foldersPaths = new List<String>(); //Paths where will needed create the folders

            //Loop to follow the reverse path to search the first missing folder
            while (String.Compare(tempDownloadPath, rootPath) != 0)
            {                
                try { await StorageFolder.GetFolderFromPathAsync(tempDownloadPath); }
                catch (UnauthorizedAccessException)
                {
                    //The folder exists, but probably is a restricted access system folder in the download path. 
                    break; // Exit the loop.
                }
                catch (FileNotFoundException) //Folder not exists
                {
                    //Include the folder name that will be needed create and the corresponding path
                    foldersNames.Insert(0, Path.GetFileName(tempDownloadPath));
                    foldersPaths.Insert(0, new DirectoryInfo(tempDownloadPath).Parent.FullName);
                }
                finally 
                {
                    //Upgrade to the next path to check (parent folder)
                    tempDownloadPath = new DirectoryInfo(tempDownloadPath).Parent.FullName; 
                }
            }                       
            
            // Create each necessary folder of the download path
            for (int i = 0; i < foldersNames.Count; i++)
            {
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(foldersPaths.ElementAt(i));
                    await folder.CreateFolderAsync(Path.GetFileName(foldersNames.ElementAt(i)), CreationCollisionOption.OpenIfExists);
                }
                catch (Exception e)
                {
                    OnUiThread(() =>
                    {
                        new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                            String.Format(AppMessages.DownloadNodeFailed, e.Message),
                            App.AppInformation).ShowDialog();
                    });
                    
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> RecursiveDownloadFolder(String downloadPath, NodeViewModel folderNode)
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

            try
            {
                String newDownloadPath = Path.Combine(downloadPath, folderNode.Name);
                if (!await CheckDownloadPath(newDownloadPath)) return false;

                MNodeList childList = MegaSdk.getChildren(folderNode.OriginalMNode);

                bool result = true; // Default value in case that the folder is empty
                for (int i = 0; i < childList.size(); i++)
                {
                    // To avoid pass null values to CreateNew
                    if (childList.get(i) == null) continue;

                    var childNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, childList.get(i), ContainerType.CloudDrive);

                    // If node creation failed for some reason, continue with the rest and leave this one
                    if (childNode == null) continue;

                    bool partialResult;
                    if (childNode.IsFolder)
                        partialResult = await RecursiveDownloadFolder(newDownloadPath, childNode);
                    else
                        partialResult = await DownloadFile(newDownloadPath, childNode);

                    // Only change the global result if the partial result indicates an error
                    if (!partialResult) result = partialResult;
                }

                return result;
            }
            catch (Exception e)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                        String.Format(AppMessages.DownloadNodeFailed, e.Message),
                        App.AppInformation).ShowDialog();
                });

                return false;
            }
        }

        private async Task<bool> DownloadFile(String downloadPath, NodeViewModel fileNode)
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
                if (!await CheckDownloadPath(Path.GetDirectoryName(fileNode.Transfer.TransferPath)))
                    return false;

                fileNode.Transfer.ExternalDownloadPath = downloadPath;
                TransfersService.MegaTransfers.Add(fileNode.Transfer);
                fileNode.Transfer.StartTransfer();
            }
            catch (Exception e)
            {
                String message;
                if (e is ArgumentException || e is NotSupportedException)
                    message = String.Format(AppMessages.InvalidFileName, fileNode.Transfer.TransferPath);
                else if (e is PathTooLongException)
                    message = String.Format(AppMessages.PathTooLong, fileNode.Transfer.TransferPath);
                else if (e is UnauthorizedAccessException)
                    message = String.Format(AppMessages.FolderUnauthorizedAccess, Path.GetDirectoryName(fileNode.Transfer.TransferPath));
                else
                    message = String.Format(AppMessages.DownloadNodeFailed, e.Message);

                OnUiThread(() =>
                {
                    new CustomMessageDialog(AppMessages.DownloadNodeFailed_Title,
                        message, App.AppInformation).ShowDialog();
                });

                return false;
            }

            return true;
        }
#endif

        public async Task<bool> SaveForOffline()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return false;

            MNode parentNode = SdkService.MegaSdk.getParentNode(this.OriginalMNode);

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            String parentNodePath;
            if(ParentContainerType != ContainerType.PublicLink)
            {
                parentNodePath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""),
                    (SdkService.MegaSdk.getNodePath(parentNode)).Remove(0, 1).Replace("/", "\\"));
            }
            else 
            {
                // If is a public node (link) the destination folder is the SFO root
                parentNodePath = sfoRootPath;
            }            

            if (!FolderService.FolderExists(parentNodePath))
                FolderService.CreateFolder(parentNodePath);

            if (this.IsFolder)
                await RecursiveSaveForOffline(parentNodePath, this);
            else
                await SaveFileForOffline(parentNodePath, this);
            
            this.IsAvailableOffline = this.IsSelectedForOffline = true;

            // Check and add to the DB if necessary the previous folders of the path
            while (String.Compare(parentNodePath, sfoRootPath) != 0)
            {
                var folderPathToAdd = parentNodePath;
                parentNodePath = ((new DirectoryInfo(parentNodePath)).Parent).FullName;

                if (!SavedForOffline.ExistsNodeByLocalPath(folderPathToAdd))
                    SavedForOffline.Insert(parentNode);

                parentNode = SdkService.MegaSdk.getParentNode(parentNode);
            }

            return true;
        }

        private async Task RecursiveSaveForOffline(String sfoPath, NodeViewModel node)
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
                    await RecursiveSaveForOffline(newSfoPath, childNode);
                else
                    await SaveFileForOffline(newSfoPath, childNode);
            }
        }

        private async Task<bool> SaveFileForOffline(String sfoPath, NodeViewModel node)
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
                TransfersService.MegaTransfers.Add(node.Transfer);
                node.Transfer.ExternalDownloadPath = sfoPath;
                node.Transfer.StartTransfer(true);
            }

            return true;
        }

        public async Task<bool> RemoveForOffline()
        {
            bool result;

            MNode parentNode = SdkService.MegaSdk.getParentNode(this.OriginalMNode);

            String parentNodePath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                AppResources.DownloadsDirectory.Replace("\\", ""),
                (SdkService.MegaSdk.getNodePath(parentNode)).Remove(0, 1).Replace("/", "\\"));

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            var nodePath = Path.Combine(parentNodePath, this.Name);

            if (this.IsFolder)
            {
                await RecursiveRemoveForOffline(parentNodePath, this.Name);
                result = FolderService.DeleteFolder(nodePath, true);
            }                
            else
            {
                // Search if the file has a pending transfer for offline and cancel it on this case                
                TransfersService.CancelPendingNodeOfflineTransfers(nodePath, this.IsFolder);

                result = FileService.DeleteFile(nodePath);                
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
                    result = result & FolderService.DeleteFolder(folderPathToRemove);
                    SavedForOffline.DeleteNodeByLocalPath(folderPathToRemove);
                }
            }

            return result;
        }

        private async Task RecursiveRemoveForOffline(String sfoPath, String nodeName)
        {
            String newSfoPath = Path.Combine(sfoPath, nodeName);

            // Search if the folder has a pending transfer for offline and cancel it on this case            
            TransfersService.CancelPendingNodeOfflineTransfers(String.Concat(newSfoPath, "\\"), this.IsFolder);

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
            this.SizeText = this.Size.ToStringAndSuffix(2);
            this.IsExported = megaNode.isExported();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");

            if (this.Type == MNodeType.TYPE_FILE)
                this.ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");                
            else
                this.ModificationTime = this.CreationTime;

            if(!SdkService.MegaSdk.isInShare(megaNode) && this.ParentContainerType != ContainerType.PublicLink &&
                this.ParentContainerType != ContainerType.InShares && this.ParentContainerType != ContainerType.ContactInShares &&
                this.ParentContainerType != ContainerType.FolderLink)
                CheckAndUpdateSFO(megaNode);
        }        

        private void CheckAndUpdateSFO(MNode megaNode)
        {
            this.IsAvailableOffline = false;
            this.IsSelectedForOffline = false;

            var nodePath = SdkService.MegaSdk.getNodePath(megaNode);
            if (String.IsNullOrWhiteSpace(nodePath)) return;

            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                AppResources.DownloadsDirectory, nodePath.Remove(0, 1).Replace("/", "\\"));

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

        public MNode OriginalMNode { get; set; }

        #endregion

        #endregion

        #region Properties

        public bool LinkWithExpirationTime
        {
            get{ return (LinkExpirationTime > 0) ? true : false; }
        }

        public long LinkExpirationTime
        {
            get { return OriginalMNode.getExpirationTime(); }
        }

        public DateTime? LinkExpirationDate
        {
            get
            {
                DateTime? _linkExpirationDate;
                if (LinkExpirationTime > 0)
                    _linkExpirationDate = OriginalDateTime.AddSeconds(LinkExpirationTime);
                else
                    _linkExpirationDate = null;

                return _linkExpirationDate;
            }
        }

    #endregion

    }
}
