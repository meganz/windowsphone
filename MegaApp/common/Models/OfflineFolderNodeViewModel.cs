using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Database;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class OfflineFolderNodeViewModel : OfflineNodeViewModel
    {
        public OfflineFolderNodeViewModel(DirectoryInfo folderInfo, ObservableCollection<IOfflineNode> parentCollection = null,
            ObservableCollection<IOfflineNode> childCollection = null)
            : base(parentCollection, childCollection)
        {
            Update(folderInfo);            
            SetFolderInfo();

            this.IsDefaultImage = true;
            this.DefaultImagePathData = VisualResources.FolderTypePath_default;
        }

        #region Override Methods

        public override void Open()
        {
            throw new NotSupportedException("Open file is not supported on folder nodes");
        }

        #endregion

        #region Public Methods

        public void Update(DirectoryInfo folderInfo)
        {
            this.Base64Handle = "0";
            var existingNode = SavedForOffline.ReadNodeByLocalPath(folderInfo.FullName);
            if(existingNode != null)
                this.Base64Handle = existingNode.Base64Handle;

            this.Name = folderInfo.Name;
            this.NodePath = folderInfo.FullName;
            this.Size = 0;
            this.SizeText = this.Size.ToStringAndSuffix();
            this.IsFolder = true;
            this.CreationTime = folderInfo.CreationTime.ToString("dd MMM yyyy");
            this.ModificationTime = folderInfo.LastWriteTime.ToString("dd MMM yyyy");

            SetDefaultValues();
        }

        public void SetFolderInfo()
        {
            if (!Directory.Exists(this.NodePath)) Directory.CreateDirectory(this.NodePath);

            int childFolders = FolderService.GetNumChildFolders(this.NodePath);
            int childFiles = FolderService.GetNumChildFiles(this.NodePath, true);

            OnUiThread(() =>
            {
                this.Information = String.Format("{0} {1} | {2} {3}",
                    childFolders, childFolders == 1 ? UiResources.SingleFolder.ToLower() : UiResources.MultipleFolders.ToLower(),
                    childFiles, childFiles == 1 ? UiResources.SingleFile.ToLower() : UiResources.MultipleFiles.ToLower());
            });
        }

        #endregion
    }
}
