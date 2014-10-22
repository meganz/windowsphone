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
    class MediaSelectionPageModel: BaseSdkViewModel
    {
        public MediaSelectionPageModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            PictureAlbums = new ObservableCollection<BaseMediaModel<PictureAlbum>>(MediaService.GetPictureAlbums());
            Pictures = new ObservableCollection<BaseMediaModel<Picture>>(MediaService.GetPictures());
        }
        
        #region Methods

       

        #endregion

        #region Properties

        public ObservableCollection<BaseMediaModel<PictureAlbum>> PictureAlbums { get; set; }
        public ObservableCollection<BaseMediaModel<Picture>> Pictures { get; set; } 

        #endregion
    }
}
