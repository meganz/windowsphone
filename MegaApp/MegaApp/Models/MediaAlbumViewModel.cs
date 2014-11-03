using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Services;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Models
{
    class MediaAlbumViewModel: BaseSdkViewModel
    {
        public MediaAlbumViewModel(MegaSDK megaSdk, BaseMediaViewModel<PictureAlbum> pictureAlbum)
            : base(megaSdk)
        {
            PictureAlbumName = pictureAlbum.Name;
            Pictures = new ObservableCollection<BaseMediaViewModel<Picture>>(MediaService.GetPictures(pictureAlbum.BaseObject));
        }

        #region Properties

        public string PictureAlbumName { get; private set; }
        
        public ObservableCollection<BaseMediaViewModel<Picture>> Pictures { get; set; }

        #endregion
    }
}
