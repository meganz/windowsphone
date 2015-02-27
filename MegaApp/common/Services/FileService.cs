using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        public static void ClearFiles(IEnumerable<string> filesToDelete)
        {
            foreach (var file in filesToDelete)
            {
                File.Delete(file);
            }
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

                copy = await file.CopyAsync(folder, newFileName, NameCollisionOption.GenerateUniqueName); 
            }
            catch (UnauthorizedAccessException) 
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppMessages.CopyFileUnauthorizedAccessException,
                        AppMessages.CopyFileUnauthorizedAccessException_Title, MessageBoxButton.OK);
                });
                return false;
            }
            catch (Exception e) 
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format(AppMessages.CopyFileFailed, e.Message),
                        AppMessages.CopyFileFailed_Title, MessageBoxButton.OK);
                });
                return false;
            }

            return copy != null;
        }
       
        public static async Task<bool> OpenFile(string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);

                if (file != null)
                    return await Windows.System.Launcher.LaunchFileAsync(file);
                
                MessageBox.Show(AppMessages.FileNotFound, AppMessages.FileNotFound_Title, MessageBoxButton.OK);
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show(AppMessages.OpenFileFailed, AppMessages.OpenFileFailed_Title, MessageBoxButton.OK);
                return false;
            }            
        }

        #if WINDOWS_PHONE_81
            public static void SelectMultipleFiles()
            {
                var fileOpenPicker = new FileOpenPicker();
            
                fileOpenPicker.ContinuationData["Operation"] = "SelectedFiles";
            
                // Use wildcard filter to start FileOpenPicker in location selection screen instead of 
                // photo selection screen
                fileOpenPicker.FileTypeFilter.Add("*");
                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            
                fileOpenPicker.PickMultipleFilesAndContinue();
            }
        #endif

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
