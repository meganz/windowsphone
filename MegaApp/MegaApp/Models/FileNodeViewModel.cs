using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class FileNodeViewModel: NodeViewModel
    {
        public FileNodeViewModel(MegaSDK megaSdk, MNode megaNode, object parentCollection = null, object childCollection = null)
            : base(megaSdk, megaNode, parentCollection, childCollection)
        {
            this.FileSize = base.Size.ToStringAndSuffix();
            this.Transfer = new TransferObjectModel(MegaSdk, this, TransferType.Download, FilePath);
        }

        #region Override Methods

        public override async void OpenFile()
        {
            await FileService.OpenFile(FilePath);
        }

        #endregion


        #region Public Methods

        public void SetFile()
        {
            if (!FileService.FileExists(FilePath))
            {
                Transfer.StartTransfer();
            }
            else
            {
                this.ThumbnailImageUri = ImageService.GetDefaultFileImage(this.Name);
            }
        }

        #endregion

        #region Properties

        public string FileSize { get; private set; }

        public string FilePath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory,
                                    this.Name);
            }
        }

        #endregion
    }
}
