using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Models;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace MegaApp.Services
{
    static class MediaService
    {
        public static IEnumerable<BaseMediaViewModel<PictureAlbum>> GetPictureAlbums()
        {
            var mediaLibrary = new MediaLibrary();
           
            var albumsList = new List<BaseMediaViewModel<PictureAlbum>>();

            foreach (var album in mediaLibrary.RootPictureAlbum.Albums)
            {
                if (album.Pictures.Count <= 0) continue;

                var media = new BaseMediaViewModel<PictureAlbum>()
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

            var bitmapImage = new BitmapImage
            {
                DecodePixelHeight = 200,
                DecodePixelWidth = 200,
                DecodePixelType = DecodePixelType.Logical
            };
            bitmapImage.SetSource(pictureAlbum.Pictures[random.Next(0, pictureAlbum.Pictures.Count)].GetImage());

            return bitmapImage;
        }

        public static IEnumerable<BaseMediaViewModel<Picture>> GetPictures(PictureAlbum pictureAlbum = null)
        {
            var pictureCollection = pictureAlbum == null ? new MediaLibrary().Pictures : pictureAlbum.Pictures;

            var pictureList = new List<BaseMediaViewModel<Picture>>();

            foreach (var picture in pictureCollection)
            {
                var media = new BaseMediaViewModel<Picture>()
                {
                    Name = picture.Name,
                    Type = MediaType.Picture,
                    BaseObject = picture
                };

                pictureList.Add(media);
            }

            return pictureList;
          
        }

        public static IEnumerable<BaseMediaViewModel<Song>> GetSongs(Album album = null)
        {
            var songCollection = album == null ? new MediaLibrary().Songs : album.Songs;

            var songList = new List<BaseMediaViewModel<Song>>();

            foreach (var song in songCollection)
            {
                var media = new BaseMediaViewModel<Song>()
                {
                    Name = song.Name,
                    Details = song.Artist.Name,
                    Type = MediaType.Song,
                    BaseObject = song
                };
              
                songList.Add(media);
            }

            return songList;

        }
    }
}
