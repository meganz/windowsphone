using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

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
                    new CustomMessageDialog(
                            AppMessages.PhotoUploadError_Title,
                            AppMessages.PhotoUploadError,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                }
            };
            
            cameraCaptureTask.Show();
        }

        public static bool SetAutoCameraUpload(bool onOff)
        {
            var resourceIntensiveTask = ScheduledActionService.Find("ScheduledCameraUploadTaskAgent") as ResourceIntensiveTask;
            
            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule.
            if (resourceIntensiveTask != null)
            {
                ScheduledActionService.Remove("ScheduledCameraUploadTaskAgent");
            }

            if (!onOff) return false;

            resourceIntensiveTask = new ResourceIntensiveTask("ScheduledCameraUploadTaskAgent")
            {
                // The description is required for periodic agents. This is the string that the user
                // will see in the background services Settings page on the device.
                Description = AppMessages.ResourceIntensiveTaskDescription
            };

            
            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(resourceIntensiveTask);
                
                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if DEBUG
                ScheduledActionService.LaunchForTest("ScheduledCameraUploadTaskAgent", TimeSpan.FromSeconds(5));
#endif
                return true;
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    new CustomMessageDialog(AppMessages.BackgroundAgentDisabled_Title,
                        AppMessages.BackgroundAgentDisabled, App.AppInformation).ShowDialog();
                }
            }
            catch (SchedulerServiceException)
            {
               // Do nothing
            }

            return false;
        }

        public static bool GetAutoCameraUploadStatus()
        {
            var resourceIntensiveTask = ScheduledActionService.Find("ScheduledCameraUploadTaskAgent") as ResourceIntensiveTask;

            return resourceIntensiveTask != null && resourceIntensiveTask.IsScheduled;
        }
    }
}
