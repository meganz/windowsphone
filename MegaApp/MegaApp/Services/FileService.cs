using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Services
{
    static class FileService
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
