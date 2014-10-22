using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Extensions;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Services
{
    class ImageService
    {
        public static bool SaveToCameraRoll(string name, BitmapImage bitmapImage)
        {
            using (var mediaLibrary = new MediaLibrary())
            {
                try
                {
                    return mediaLibrary.SavePictureToCameraRoll(name, bitmapImage.ConvertToBytes()) != null;
                }
                catch (Exception)
                {
                    return false;
                }
               
            }
        }

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

        public static BitmapImage GetBitmapFromStream(Stream stream)
        {
            stream.Position = 0;
            var result = new BitmapImage();
            result.SetSource(stream);
            return result;
        }

        public static BitmapImage GetDefaultFileImage(string filename)
        {
            string fileExtension;

            try
            {
                fileExtension = Path.GetExtension(filename);
            }
            catch (Exception)
            {
                return new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));
            }
            
            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));

            switch (fileExtension.ToLower())
            {
                case ".accdb":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/accdb.png", UriKind.Relative));
                    }
                case ".bmp":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/bmp.png", UriKind.Relative));
                    }
                case ".doc":
                case ".docx":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/doc.png", UriKind.Relative));
                    }
                case ".eps":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/eps.png", UriKind.Relative));
                    }
                case ".gif":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/gif.png", UriKind.Relative));
                    }
                case ".ico":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/ico.png", UriKind.Relative));
                    }
                case ".jpg":
                case ".jpeg":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/jpg.png", UriKind.Relative));
                    }
                case ".mp3":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/mp3.png", UriKind.Relative));
                    }
                case ".pdf":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/pdf.png", UriKind.Relative));
                    }
                case ".png":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/png.png", UriKind.Relative));
                    }
                case ".ppt":
                case ".pptx":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/ppt.png", UriKind.Relative));
                    }
                case ".swf":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/swf.png", UriKind.Relative));
                    }
                case ".tga":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/tga.png", UriKind.Relative));
                    }
                case ".tiff":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/tiff.png", UriKind.Relative));
                    }
                case ".txt":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/txt.png", UriKind.Relative));
                    }
                case ".wav":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/wav.png", UriKind.Relative));
                    }
                case ".xls":
                case ".xlsx":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/xls.png", UriKind.Relative));
                    }
                case ".zip":
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/zip.png", UriKind.Relative));
                    }
                default:
                    {
                        return new BitmapImage(new Uri("/Assets/FileTypes/file.png", UriKind.Relative));
                    }
            }
        }
    }
}
