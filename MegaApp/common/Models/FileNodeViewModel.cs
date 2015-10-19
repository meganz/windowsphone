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
        public FileNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentCollection, childCollection)
        {
            this.Information = this.Size.ToStringAndSuffix();
            this.Transfer = new TransferObjectModel(MegaSdk, this, TransferType.Download, LocalFilePath);

            this.IsDownloadAvailable = File.Exists(this.LocalFilePath);
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
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory,
                                    this.Name);
            }
        }

        public string PublicFilePath
        {
            get
            {
                return Path.Combine(AppService.GetSelectedDownloadDirectoryPath(),
                                    this.Name);
            }
        }

        #endregion
    }
}
