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
        public static bool SaveToCameraRoll(string name, Uri bitmapImageUri)
        {
            using (var mediaLibrary = new MediaLibrary())
            {
                try
                {
                    var bitmapImage = new BitmapImage(bitmapImageUri);
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

        public static BitmapImage GetBitmapFromStream(Stream stream, int decodePixelHeight, int decodePixelWidth)
        {
            stream.Position = 0;
            
            var result = new BitmapImage
            {
                DecodePixelHeight = decodePixelHeight, 
                DecodePixelWidth = decodePixelWidth,
                DecodePixelType = DecodePixelType.Logical
            };
            result.SetSource(stream);
            
            return result;
        }

        public static Uri GetDefaultFileImage(string filename)
        {
            string fileExtension;

            try
            {
                fileExtension = Path.GetExtension(filename);
            }
            catch (Exception)
            {
                return new Uri("/Assets/FileTypes/file.png", UriKind.Relative);
            }
            
            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return new Uri("/Assets/FileTypes/file.png", UriKind.Relative);

            switch (fileExtension.ToLower())
            {
                case ".accdb":
                    {
                        return new Uri("/Assets/FileTypes/accdb.png", UriKind.Relative);
                    }
                case ".bmp":
                    {
                        return new Uri("/Assets/FileTypes/bmp.png", UriKind.Relative);
                    }
                case ".doc":
                case ".docx":
                    {
                        return new Uri("/Assets/FileTypes/doc.png", UriKind.Relative);
                    }
                case ".eps":
                    {
                        return new Uri("/Assets/FileTypes/eps.png", UriKind.Relative);
                    }
                case ".gif":
                    {
                        return new Uri("/Assets/FileTypes/gif.png", UriKind.Relative);
                    }
                case ".ico":
                    {
                        return new Uri("/Assets/FileTypes/ico.png", UriKind.Relative);
                    }
                case ".jpg":
                case ".jpeg":
                    {
                        return new Uri("/Assets/FileTypes/jpg.png", UriKind.Relative);
                    }
                case ".mp3":
                    {
                        return new Uri("/Assets/FileTypes/mp3.png", UriKind.Relative);
                    }
                case ".pdf":
                    {
                        return new Uri("/Assets/FileTypes/pdf.png", UriKind.Relative);
                    }
                case ".png":
                    {
                        return new Uri("/Assets/FileTypes/png.png", UriKind.Relative);
                    }
                case ".ppt":
                case ".pptx":
                    {
                        return new Uri("/Assets/FileTypes/ppt.png", UriKind.Relative);
                    }
                case ".swf":
                    {
                        return new Uri("/Assets/FileTypes/swf.png", UriKind.Relative);
                    }
                case ".tga":
                    {
                        return new Uri("/Assets/FileTypes/tga.png", UriKind.Relative);
                    }
                case ".tiff":
                    {
                        return new Uri("/Assets/FileTypes/tiff.png", UriKind.Relative);
                    }
                case ".txt":
                    {
                        return new Uri("/Assets/FileTypes/txt.png", UriKind.Relative);
                    }
                case ".wav":
                    {
                        return new Uri("/Assets/FileTypes/wav.png", UriKind.Relative);
                    }
                case ".xls":
                case ".xlsx":
                    {
                        return new Uri("/Assets/FileTypes/xls.png", UriKind.Relative);
                    }
                case ".zip":
                    {
                        return new Uri("/Assets/FileTypes/zip.png", UriKind.Relative);
                    }
                default:
                    {
                        return new Uri("/Assets/FileTypes/file.png", UriKind.Relative);
                    }
            }
        }
    }
}
