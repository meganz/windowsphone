using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

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
            var file = await StorageFile.GetFileFromPathAsync(filePath);

            if (file != null)
                return await Windows.System.Launcher.LaunchFileAsync(file);
            
            return false;
        }

        public static string CreateRandomFilePath(string path)
        {
            return Path.Combine(path, Guid.NewGuid().ToString("N"));
        }
    }
}
