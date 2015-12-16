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
using MegaApp.Services;

namespace MegaApp.Models
{
    public class OfflineFileNodeViewModel : OfflineNodeViewModel
    {
        public OfflineFileNodeViewModel(FileInfo fileInfo, ObservableCollection<IOfflineNode> parentCollection = null,
            ObservableCollection<IOfflineNode> childCollection = null)
            : base(parentCollection, childCollection)
        {
            Update(fileInfo);
            this.Information = this.Size.ToStringAndSuffix();

            this.IsDefaultImage = true;
            this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
        }

        #region Override Methods

        public override async void Open()
        {
            await FileService.OpenFile(this.NodePath);
        }

        #endregion

        #region Public Methods

        public void Update(FileInfo fileInfo)
        {
            this.Base64Handle = "0";
            var existingNode = SavedForOffline.ReadNodeByLocalPath(fileInfo.FullName);
            if (existingNode != null)
                this.Base64Handle = existingNode.Base64Handle;

            this.Name = fileInfo.Name;
            this.NodePath = fileInfo.FullName;
            this.Size = Convert.ToUInt64(fileInfo.Length);
            this.SizeText = this.Size.ToStringAndSuffix();
            this.IsFolder = false;
            this.CreationTime = fileInfo.CreationTime.ToString("dd MMM yyyy");
            this.ModificationTime = fileInfo.LastWriteTime.ToString("dd MMM yyyy");

            SetDefaultValues();
        }

        #endregion
    }
}
