using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            this.Information = this.Size.ToStringAndSuffix(2);
            this.Transfer = new TransferObjectModel(MegaSdk, this, MTransferType.TYPE_DOWNLOAD, LocalFilePath);
        }

        #region Override Methods

        public override async void Open()
        {
            await FileService.OpenFile(LocalFilePath);
        }

        #endregion


        #region Public Methods

        public void SetFile()
        {
            if (!FileService.FileExists(LocalFilePath))
            {
                Transfer.StartTransfer();
            }
            else
            {
                this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
            }
        }

        #endregion

        #region Properties

        public string FileSize { get; private set; }

        public string LocalFilePath
        {
            get
            {
                String nodePath = MegaSdk.getNodePath(this.OriginalMNode);

                if (String.IsNullOrWhiteSpace(nodePath) || ParentContainerType == ContainerType.PublicLink)
                {
                    nodePath = this.Name;
                }
                else if(ParentContainerType == ContainerType.InShares)
                {
                    // If is an incoming shared item, we need to remove the contact email address 
                    // from the node path. Is the first part of the node path and the separator 
                    // character is ":". (EXAMPLE: contact@domain.com:nodePath)
                    nodePath = nodePath.Substring(nodePath.IndexOf(":") + 1).Replace("/", "\\");
                }
                else
                {
                    // If node container is Rubbish Bin
                    if (nodePath.StartsWith("//bin"))
                        nodePath = nodePath.Remove(0, 7).Replace("/", "\\"); //Need to remove the "//bin//" of the beginning of the path
                    else
                        nodePath = nodePath.Remove(0, 1).Replace("/", "\\"); //Need to remove the "/" of the beginning of the path
                }

                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory, nodePath);
            }
        }

        #endregion
    }
}
