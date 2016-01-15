using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Pickers;
using MegaApp.Classes;
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
                string extension = Path.GetExtension(filename);

                if (extension == null) return false;

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

        public static void DeleteFile(string path)
        {
            if(File.Exists(path))
                File.Delete(path);            
        }

        public static void ClearFiles(IEnumerable<string> filesToDelete)
        {
            try
            {
                if (filesToDelete == null) return;
                
                foreach (var file in filesToDelete)
                {
                    if (file != null)
                        File.Delete(file);
                }                
            }
            catch(IOException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.DeleteNodeFailed_Title,
                            String.Format(AppMessages.DeleteNodeFailed,  e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
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

        // Move a file. Copies the file and remove the source file if the copy was successful
        public static async Task<bool> MoveFile(string sourcePath, string destinationFolderPath, string newFileName = null)
        {
            if(!await CopyFile(sourcePath, destinationFolderPath, newFileName)) return false;

            DeleteFile(sourcePath);
            
            return true;
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
