using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Services
{
    class ImageService
    {
        public static bool IsImage(string filename)
        {
            string extension = Path.GetExtension(filename);

            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".png":
                case ".tif":
                case ".tiff":
                case ".tga":
                case ".bmp":
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}
