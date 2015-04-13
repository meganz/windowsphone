using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentCollection, childCollection)
        {
            SetFolderInfo();
            this.IsThumbnailDefaultImage = true;
            this.ThumbnailImageUri = new Uri("folder" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
        }

        #region Override Methods

        public override void Open()
        {
            throw new NotSupportedException("Open file is not supported on folder nodes");
        }

       
        #endregion

        #region Public Methods

        public void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(base.OriginalMNode);
            int childFiles = this.MegaSdk.getNumChildFiles(base.OriginalMNode);
            this.Information = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder : UiResources.MultipleFolders,
                childFiles, childFiles == 1 ? UiResources.SingleFile : UiResources.MultipleFiles);
        }

        #endregion
    }
}
