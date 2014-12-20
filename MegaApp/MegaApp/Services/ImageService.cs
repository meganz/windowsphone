using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MegaApp.Classes;
using MegaApp.Enums;
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

            if (extension == null) return false;
            
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
                return new Uri("generic" + GetResolutionExtension() + ".png", UriKind.Relative);
            }
            
            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return new Uri("generic" + GetResolutionExtension() + ".png", UriKind.Relative);

            switch (fileExtension.ToLower())
            {
                case ".3ds":
                case ".3dm":
                case ".max":
                case ".obj":
                    {
                        return new Uri("3d" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".aep":
                case ".aet":
                    {
                        return new Uri("aftereffects" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".dxf":
                case ".dwg":
                    {
                        return new Uri("cad" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".dwt":
                    {
                        return new Uri("dreamweaver" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".accdb":
                case ".sql":
                case ".db":
                case ".dbf":
                case ".mdb":
                case ".pdb":
                    {
                        return new Uri("database" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".exe":
                case ".com":
                case ".bin":
                case ".apk":
                case ".app":
                case ".msi":
                case ".cmd":
                case ".gadget":
                    {
                        return new Uri("executable" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".as":
                case ".asc":
                case ".ascs":
                    {
                        return new Uri("fla_lang" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".fla":
                    {
                        return new Uri("flash" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".fnt":
                case ".otf":
                case ".ttf":
                case ".fon":
                    {
                        return new Uri("font" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".bmp":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".tga":
                case ".png":
                case ".ico":
                    {
                        return new Uri("graphic" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".gpx":
                case ".kml":
                case ".kmz":
                    {
                        return new Uri("gis" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".html":
                case ".htm":
                case ".dhtml":
                case ".xhtml":
                    {
                        return new Uri("html" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".ai":
                case ".ait":
                    {
                        return new Uri("illustrator" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".indd":
                    {
                        return new Uri("indesign" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".jar":
                case ".java":
                case ".class":
                    {
                        return new Uri("java" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".midi":
                case ".mid":
                    {
                        return new Uri("midi" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".abr":
                case ".psb":
                case ".psd":
                    {
                        return new Uri("photoshop" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".pls":
                case ".m3u":
                case ".asx":
                    {
                        return new Uri("playlist" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".pcast":
                    {
                        return new Uri("podcast" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".prproj":
                case ".ppj":
                    {
                        return new Uri("premiere" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".3fr":
                case ".arw":
                case ".bay":
                case ".cr2":
                case ".dcr":
                case ".dng":
                case ".fff":
                case ".mef":
                case ".mrw":
                case ".nef":
                case ".pef":
                case ".rw2":
                case ".srf":
                case ".orf":
                case ".rwl":
                    {
                        return new Uri("raw" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".rm":
                case ".ra":
                case ".ram":
                    {
                        return new Uri("real_audio" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".sh":
                case ".c":
                case ".cc":
                case ".cpp":
                case ".cxx":
                case ".h":
                case ".hpp":
                case ".dll":
                case ".cs":
                case ".vb":
                    {
                        return new Uri("sourcecode" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".torrent":
                    {
                        return new Uri("torrent" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".vcf":
                    {
                        return new Uri("vcard" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".mkv":
                case ".webm":
                case ".avi":
                case ".mp4":
                case ".m4v":
                case ".mpg":
                case ".mpeg":
                case ".mov":
                case ".3g2":
                case ".asf":
                case ".wmv":
                    {
                        return new Uri("video" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".srt":
                    {
                        return new Uri("video_subtitle" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".vob":
                    {
                        return new Uri("video_vob" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".xml":
                case ".shtml":
                case ".js":
                case ".css":
                    {
                        return new Uri("web_data" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".php":
                case ".php3":
                case ".php4":
                case ".php5":
                case ".phtml":
                case ".inc":
                case ".asp":
                case ".aspx":
                case ".pl":
                case ".cgi":
                case ".py":
                    {
                        return new Uri("web_lang" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".doc":
                case ".docx":
                case ".dotx":
                case ".wps":
                    {
                        return new Uri("word" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".eps":
                case ".svg":
                case ".svgz":
                case ".cdr":
                    {
                        return new Uri("vector" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".jpg":
                case ".jpeg":
                    {
                        return new Uri("image" + GetResolutionExtension() + ".png", UriKind.Relative);
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
                        return new Uri("audio" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".pdf":
                    {
                        return new Uri("pdf" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".ppt":
                case ".pptx":
                case ".pps":
                    {
                        return new Uri("powerpoint" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".swf":
                    {
                        return new Uri("swf" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".txt":
                case ".rtf":
                case ".ans":
                case ".ascii":
                case ".log":
                case ".odt":
                case ".wpd":
                    {
                        return new Uri("text" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                case ".xls":
                case ".xlsx":
                case ".xlt":
                case ".xltm":
                    {
                        return new Uri("excel" + GetResolutionExtension() + ".png", UriKind.Relative);
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
                        return new Uri("compressed" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
                default:
                    {
                        return new Uri("generic" + GetResolutionExtension() + ".png", UriKind.Relative);
                    }
            }
        }
    }
}
