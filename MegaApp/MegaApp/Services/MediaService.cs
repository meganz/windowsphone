using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Models;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Services
{
    static class MediaService
    {
        public static IEnumerable<BaseMediaModel<PictureAlbum>> GetPictureAlbums()
        {
            var mediaLibrary = new MediaLibrary();

            var albumsList = new List<BaseMediaModel<PictureAlbum>>();

            foreach (var album in mediaLibrary.RootPictureAlbum.Albums)
            {
                var media = new BaseMediaModel<PictureAlbum>()
                {
                    Name = album.Name,
                    Type = MediaType.Album,
                    DisplayImage = GetRandomImage(album),
                    BaseObject = album
                };

                albumsList.Add(media);
            }

            return albumsList;
        }

        public static BitmapImage GetRandomImage(PictureAlbum pictureAlbum)
        {
            if (pictureAlbum.Pictures.Count < 1) return null;

            var random = new Random(DateTime.Now.Millisecond);

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(pictureAlbum.Pictures[random.Next(0, pictureAlbum.Pictures.Count - 1)].GetImage());

            return bitmapImage;
        }

        public static IEnumerable<BaseMediaModel<Picture>> GetPictures()
        {
            var mediaLibrary = new MediaLibrary();
            
            var pictureList = new List<BaseMediaModel<Picture>>();

            foreach (var picture in mediaLibrary.Pictures)
            {
                var media = new BaseMediaModel<Picture>()
                {
                    Name = picture.Name,
                    Type = MediaType.Picture,
                    DisplayImage = ImageService.GetBitmapFromStream(picture.GetThumbnail()),
                    BaseObject = picture
                };

                pictureList.Add(media);
            }

            return pictureList;
          
        }
    }
}
