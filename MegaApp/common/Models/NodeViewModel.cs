using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage;
using mega;
using MegaApp.Classes;
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

        protected NodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation)
        {
            Update(megaNode);
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

#if WINDOWS_PHONE_80
        public virtual void Download(TransferQueu transferQueu, string downloadPath = null)
        {
            if (!IsUserOnline()) return;
            NavigateService.NavigateTo(typeof(DownloadPage), NavigationParameter.Normal, this);
        }
#elif WINDOWS_PHONE_81
        public async void Download(TransferQueu transferQueu, string downloadPath = null)
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;
            
            if (AppInformation.PickerOrAsyncDialogIsOpen) return;

            if (downloadPath == null)
            {
                if (!await FolderService.SelectDownloadFolder(this)) return;
            }
                

            this.Transfer.DownloadFolderPath = downloadPath;
            transferQueu.Add(this.Transfer);
            this.Transfer.StartTransfer();

            // TODO Remove this global declaration in method
            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }
#endif

        public void Update(MNode megaNode)
        {
            OriginalMNode = megaNode;
            this.Handle = megaNode.getHandle();
            this.Type = megaNode.getType();
            this.Name = megaNode.getName();
            this.Size = MegaSdk.getSize(megaNode);
            this.SizeText = this.Size.ToStringAndSuffix();
            this.IsExported = megaNode.isExported();
            this.CreationTime = ConvertDateToString(megaNode.getCreationTime()).ToString("dd MMM yyyy");

            if (this.Type == MNodeType.TYPE_FILE)
            {
                this.ModificationTime = ConvertDateToString(megaNode.getModificationTime()).ToString("dd MMM yyyy");
                
                if (IsImage)
                {
                    this.IsDownloadAvailable = File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path,
                        AppResources.DownloadsDirectory, String.Format("{0}{1}", this.OriginalMNode.getBase64Handle(),
                        Path.GetExtension(this.Name))));
                }
                else
                {
                    this.IsDownloadAvailable = File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path,
                        AppResources.DownloadsDirectory, this.Name));
                }                    
            }                
            else
            {
                this.ModificationTime = this.CreationTime;
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

        public ulong Size { get; private set; }

        private string _sizeText;
        public string SizeText
        {
            get { return _sizeText; }
            set { SetField(ref _sizeText, value); }
        }

        public ObservableCollection<IMegaNode> ParentCollection { get; set; }

        public ObservableCollection<IMegaNode> ChildCollection { get; set; }

        public MNodeType Type { get; private set; }

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

        private bool _isDownloadAvailable;
        public bool IsDownloadAvailable
        {
            get { return _isDownloadAvailable; }
            set 
            {
                SetField(ref _isDownloadAvailable, value);
                IsDownloadAvailableText = _isDownloadAvailable ? UiResources.On : UiResources.Off;
            }
        }

        private String _isDownloadAvailableText;
        public String IsDownloadAvailableText
        {
            get { return _isDownloadAvailableText; }
            set { SetField(ref _isDownloadAvailableText, value); }
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
