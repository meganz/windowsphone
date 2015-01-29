using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using MegaApp.Resources;

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

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
