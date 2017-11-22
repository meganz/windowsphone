using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Pickers;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Resources;

#if WINDOWS_PHONE_81
    using Windows.Storage.AccessCache;
#endif

namespace MegaApp.Services
{
    static class FileService
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool IsPendingTransferFile(string filename)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(filename) || HasIllegalChars(filename)) return false;

                string extension = Path.GetExtension(filename);

                if (string.IsNullOrEmpty(extension)) return false;

                switch (extension.ToLower())
                {
                    case ".mega":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting file.", e);
                return false;
            }
        }

        public static bool HasIllegalChars(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                if (name.Contains(c.ToString())) return true;
            }
            return false;
        }

        public static bool ClearFiles(IEnumerable<string> filesToDelete)
        {
            if (filesToDelete == null) return false;

            bool result = true;
            foreach (var file in filesToDelete)
                result = result & DeleteFile(file);

            return result;
        }

        public static async Task<bool> CopyFile(string sourcePath, string destinationFolderPath, string newFileName = null)
        {
            StorageFile copy = null;

            try 
            { 
                var file = await StorageFile.GetFileFromPathAsync(sourcePath);
                if (file == null) return false;
            
                var folder = await StorageFolder.GetFolderFromPathAsync(destinationFolderPath);
                if (folder == null) return false;

                newFileName = newFileName ?? file.Name;

                // Get file properties to determine the size in MB
                var bp = await file.GetBasicPropertiesAsync();
                var mb = (bp.Size/1024)/1024;
                
                if (mb >= 50)
                {
                    // If the file is larger than 50 MB, we will copy it using a small buffered value in a buffered stream
                    var destFile = await folder.CreateFileAsync(newFileName, CreationCollisionOption.GenerateUniqueName);
                    if (destFile == null) return false;

                    await CopyLargeFile(file, destFile);
                    return true;
                }
                // If the file is smaller than 50 MB, let the OS decide how to copy and buffer size
                copy = await file.CopyAsync(folder, newFileName, NameCollisionOption.GenerateUniqueName); 
                
            }
            catch (UnauthorizedAccessException e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error copying file by unauthorized access", e);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.CopyFileUnauthorizedAccessException_Title,
                        AppMessages.CopyFileUnauthorizedAccessException,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
                return false;
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error copying a file", e);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.CopyFileFailed_Title,
                        String.Format(AppMessages.CopyFileFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
                return false;
            }

            return copy != null;
        }

        /// <summary>
        /// Copy a large file with a buffered stream to avoid E_FAIL error on StorageFile copy
        /// </summary>
        /// <param name="fileSource">StorageFile to copy</param>
        /// <param name="fileDest">Destination StorageFile</param>
        private static async Task CopyLargeFile(StorageFile fileSource, StorageFile fileDest)
        {
            using (var streamSource = await fileSource.OpenStreamForReadAsync())
            {
                using (var streamDest = await fileDest.OpenStreamForWriteAsync())
                {
                    await streamSource.CopyToAsync(streamDest, 4096);
                }
            }
        }

        // Move a file. Copies the file and remove the source file if the copy was successful
        public static async Task<bool> MoveFile(string sourcePath, string destinationFolderPath, string newFileName = null)
        {
            try
            {
                if (!await CopyFile(sourcePath, destinationFolderPath, newFileName)) return false;

                DeleteFile(sourcePath);

                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error moving a file", e);
                return false;
            }
        }
       
        public static async Task<bool> OpenFile(string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);

                if (file != null)
                    return await Windows.System.Launcher.LaunchFileAsync(file);

                new CustomMessageDialog(
                        AppMessages.FileNotFound_Title,
                        AppMessages.FileNotFound,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
               
                return false;
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                        AppMessages.OpenFileFailed_Title,
                        AppMessages.OpenFileFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                return false;
            }            
        }

        #if WINDOWS_PHONE_81
            public static void SelectMultipleFiles()
            {
                try
                {
                    var fileOpenPicker = new FileOpenPicker();

                    fileOpenPicker.ContinuationData["Operation"] = "SelectedFiles";

                    // Use wildcard filter to start FileOpenPicker in location selection screen instead of 
                    // photo selection screen
                    fileOpenPicker.FileTypeFilter.Add("*");
                    fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

                    fileOpenPicker.PickMultipleFilesAndContinue();
                }
                catch (Exception e)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                                AppMessages.SelectFileFailed_Title,
                                String.Format(AppMessages.SelectFileFailed, e.Message),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                    });
                }
            }
        #endif

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
