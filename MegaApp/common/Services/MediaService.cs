using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using Microsoft.Phone.Tasks;
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

            // If there is a problem obtaining the random image only will appear the album name
            try { bitmapImage.SetSource(pictureAlbum.Pictures[random.Next(0, pictureAlbum.Pictures.Count)].GetImage()); }
            catch (Exception) { return null; }

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

        public static void CaptureCameraImage(FolderViewModel currentFolder)
        {
            var cameraCaptureTask = new CameraCaptureTask();

            cameraCaptureTask.Completed += async (sender, result) =>
            {
                if (result == null || result.TaskResult != TaskResult.OK) return;

                try
                {
                    string fileName = Path.GetFileName(result.OriginalFileName);
                    if (fileName != null)
                    {
                        string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
                        using (var fs = new FileStream(newFilePath, FileMode.Create))
                        {
                            await result.ChosenPhoto.CopyToAsync(fs);
                            await fs.FlushAsync();
                            fs.Close();
                        }
                        var uploadTransfer = new TransferObjectModel(currentFolder.MegaSdk,
                            currentFolder.FolderRootNode, 
                            TransferType.Upload,
                            newFilePath);
                        App.MegaTransfers.Insert(0, uploadTransfer);
                        uploadTransfer.StartTransfer();
                    }

                    NavigateService.NavigateTo(typeof (TransferPage), NavigationParameter.Normal);
                }
                catch (Exception)
                {
                    MessageBox.Show(AppMessages.PhotoUploadError, AppMessages.PhotoUploadError_Title,
                        MessageBoxButton.OK);
                }
            };
            
            cameraCaptureTask.Show();
        }
        
    }
}
