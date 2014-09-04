using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using mega;
using MegaApp.Extensions;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    public class NodeViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;
        // Original MNode object from the MEGA SDK
        private readonly MNode _baseNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public NodeViewModel(MegaSDK megaSdk, MNode baseNode)
        {
            this._megaSdk = megaSdk;
            this._baseNode = baseNode;
            this.Name = baseNode.getName();
            this.Size = baseNode.getSize();
            this.CreationTime = ConvertDateToString(_baseNode.getCreationTime()).ToString("dd MMM yyyy");
            this.SizeAndSuffix = Size.ToStringAndSuffix();
            this.Type = baseNode.getType();
            this.NumberOfFiles = this.Type != MNodeType.TYPE_FOLDER ? null : String.Format("{0} {1}", this._megaSdk.getNumChildren(this._baseNode), UiResources.Files);

            GetThumbnailIfImage(this.Name);
        }

        #region Methods

        private void GetThumbnailIfImage(string filename)
        {
            if (!ImageService.IsImage(filename)) return;
            if (!this._baseNode.hasThumbnail()) return;

            this._megaSdk.getThumbnail(
                this._baseNode,
                Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory, this._baseNode.getBase64Handle()),
                new GetThumbnailRequestListener(this));
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

        #region Properties

        public string Name { get; private set;}

        public ulong Size { get; private set; }

        public MNodeType Type { get; private set ; }

        public string CreationTime { get; private set; }

        public string SizeAndSuffix { get; private set; }

        public string NumberOfFiles { get; private set; }

        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public MNode GetBaseNode()
        {
            return this._baseNode;
        }

        #endregion

        
    }
}
