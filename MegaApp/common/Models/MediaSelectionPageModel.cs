using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Models
{
    class MediaSelectionPageModel: BaseSdkViewModel
    {
        public MediaSelectionPageModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            PictureAlbums = new ObservableCollection<BaseMediaViewModel<PictureAlbum>>(MediaService.GetPictureAlbums());
            Pictures = new ObservableCollection<BaseMediaViewModel<Picture>>(MediaService.GetPictures());
        }
        
        #region Methods

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            this.TranslateAppBarItems(
                iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                new[] { UiResources.Accept, UiResources.ClearSelection },
                null);
        }

        #endregion

        #region Properties

        public ObservableCollection<BaseMediaViewModel<PictureAlbum>> PictureAlbums { get; set; }
        public ObservableCollection<BaseMediaViewModel<Picture>> Pictures { get; set; } 

        #endregion
    }
}
