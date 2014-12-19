using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Classes;
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

        public static string GetResolutionExtension()
        {
            switch (AppService.CurrentResolution)
            {
                case Resolutions.HD:
                    return ".Screen-720p";
                case Resolutions.WXGA:
                    return ".Screen-WXGA";
                case Resolutions.WVGA:
                    return ".Screen-WVGA";
                default:
                    throw new InvalidOperationException("Unknown resolution type");
            }
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
                return new Uri("/Assets/FileTypes/List/generic" + GetResolutionExtension() + ".png", UriKind.Relative);
            }
            
            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return new Uri("/Assets/FileTypes/List/generic" + GetResolutionExtension() + ".png", UriKind.Relative);

            switch (fileExtension.ToLower())
            {
                case ".accdb":
                case ".sql":
                case ".db":
                case ".dbf":
                case ".mdb":
                case ".pdb":
                    {
                        return new Uri("/Assets/FileTypes/List/database" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".bmp":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".tga":
                case ".png":
                case ".ico":
                    {
                        return new Uri("/Assets/FileTypes/List/graphic" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".doc":
                case ".docx":
                case ".dotx":
                case ".wps":
                    {
                        return new Uri("/Assets/FileTypes/List/word" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".eps":
                case ".svg":
                case ".svgz":
                case ".cdr":
                    {
                        return new Uri("/Assets/FileTypes/List/vector" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".jpg":
                case ".jpeg":
                    {
                        return new Uri("/Assets/FileTypes/List/image" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".mp3":
                case ".wav":
                case ".3ga":
                case ".aif":
                case ".aiff":
                case ".flac":
                case ".iff":
                case ".m4a":
                case ".wma":
                    {
                        return new Uri("/Assets/FileTypes/List/audio" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".pdf":
                    {
                        return new Uri("/Assets/FileTypes/List/pdf" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".ppt":
                case ".pptx":
                case ".pps":
                    {
                        return new Uri("/Assets/FileTypes/List/powerpoint" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".swf":
                    {
                        return new Uri("/Assets/FileTypes/List/swf" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".txt":
                case ".rtf":
                case ".ans":
                case ".ascii":
                case ".log":
                case ".odt":
                case ".wpd":
                    {
                        return new Uri("/Assets/FileTypes/List/text" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".xls":
                case ".xlsx":
                case ".xlt":
                case ".xltm":
                    {
                        return new Uri("/Assets/FileTypes/List/excel" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".zip":
                case ".rar":
                case ".tgz":
                case ".gz":
                case ".bz2":
                case ".tbz":
                case ".tar":
                case ".7z":
                case ".sitx":
                    {
                        return new Uri("/Assets/FileTypes/List/compressed" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                default:
                    {
                        return new Uri("/Assets/FileTypes/List/generic" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
            }
        }
    }
}
