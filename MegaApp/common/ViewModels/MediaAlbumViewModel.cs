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

namespace MegaApp.ViewModels
{
    class MediaAlbumViewModel: BaseSdkViewModel
    {
        public MediaAlbumViewModel(MegaSDK megaSdk, BaseMediaViewModel<PictureAlbum> pictureAlbum)
            : base(megaSdk)
        {
            PictureAlbumName = pictureAlbum.Name;
            Pictures = new ObservableCollection<BaseMediaViewModel<Picture>>(MediaService.GetPictures(pictureAlbum.BaseObject));
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

        public string PictureAlbumName { get; private set; }
        
        public ObservableCollection<BaseMediaViewModel<Picture>> Pictures { get; set; }

        #endregion
    }
}
