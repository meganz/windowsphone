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
using MegaApp.Resources;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Services
{
    static class ImageService
    {
        public static bool SaveToCameraRoll(string name, Uri bitmapImageUri)
        {
            using (var mediaLibrary = new MediaLibrary())
            {
                try
                {
                    using (var bitmapFile = File.OpenRead(bitmapImageUri.LocalPath))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(bitmapFile); 
                        bitmapFile.Close();
                        return mediaLibrary.SavePicture(name, bitmapImage.ConvertToBytes()) != null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
               
            }
        }

        public static bool IsImage(string filename)
        {
            try 
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
            catch(Exception)
            {
                return false;
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

        /// <summary>
        /// Get the vector Path data for a specific filetype extension
        /// </summary>
        /// <param name="filename">filename to extract extension and retrieve vector data</param>
        /// <returns>vector data string</returns>
        public static string GetDefaultFileTypePathData(string filename)
        {
            string fileExtension;

            try
            {
                fileExtension = Path.GetExtension(filename);
            }
            catch (Exception)
            {
                return VisualResources.FileTypePath_generic;
            }
            
            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return VisualResources.FileTypePath_generic;

            switch (fileExtension.ToLower())
            {
                case ".3ds":
                case ".3dm":
                case ".max":
                case ".obj":
                    {
                        return VisualResources.FileTypePath_3d;
                    }
                case ".aep":
                case ".aet":
                    {
                        return VisualResources.FileTypePath_aftereffects;
                    }
                case ".dxf":
                case ".dwg":
                    {
                        return VisualResources.FileTypePath_cad;
                    }
                case ".dwt":
                    {
                        return VisualResources.FileTypePath_dreamweaver;
                    }
                case ".accdb":
                case ".sql":
                case ".db":
                case ".dbf":
                case ".mdb":
                case ".pdb":
                    {
                        return VisualResources.FileTypePath_data;
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
                        return VisualResources.FileTypePath_executable;
                    }
                case ".as":
                case ".asc":
                case ".ascs":
                    {
                        return VisualResources.FileTypePath_fla_lang;
                    }
                case ".fla":
                    {
                        return VisualResources.FileTypePath_flash;
                    }
                case ".fnt":
                case ".otf":
                case ".ttf":
                case ".fon":
                    {
                        // TODO FONT PATH??
                        return VisualResources.FileTypePath_generic;
                    }
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".tga":
                case ".png":
                case ".ico":
                    {
                        // TODO IMAGE PATH DATA??
                        return VisualResources.FileTypePath_graphic;
                    }
                case ".gpx":
                case ".kml":
                case ".kmz":
                    {
                        return VisualResources.FileTypePath_gis;
                    }
                case ".html":
                case ".htm":
                case ".dhtml":
                case ".xhtml":
                    {
                        return VisualResources.FileTypePath_html;
                    }
                case ".ai":
                case ".ait":
                    {
                        return VisualResources.FileTypePath_illustrator;
                    }
                case ".indd":
                    {
                        return VisualResources.FileTypePath_indesign;
                    }
                case ".jar":
                case ".java":
                case ".class":
                    {
                        return VisualResources.FileTypePath_java;
                    }
                case ".midi":
                case ".mid":
                    {
                        return VisualResources.FileTypePath_midi;
                    }
                case ".abr":
                case ".psb":
                case ".psd":
                    {
                        return VisualResources.FileTypePath_photoshop;
                    }
                case ".pls":
                case ".m3u":
                case ".asx":
                    {
                        return VisualResources.FileTypePath_playlist;
                    }
                case ".pcast":
                    {
                        // TODO PODCAST PATH??
                        return VisualResources.FileTypePath_generic;
                    }
                case ".prproj":
                case ".ppj":
                    {
                        return VisualResources.FileTypePath_premiere;
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
                        // TODO RAW PATH
                        return VisualResources.FileTypePath_generic;
                    }
                case ".rm":
                case ".ra":
                case ".ram":
                    {
                        // TODO REAL AUDIO PATH DATA??
                        return VisualResources.FileTypePath_audio;
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
                        return VisualResources.FileTypePath_sourcecode;
                    }
                case ".torrent":
                    {
                        return VisualResources.FileTypePath_torrent;
                    }
                case ".vcf":
                    {
                        return VisualResources.FileTypePath_vcard;
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
                        // TODO VIDEO PATH DATA??
                        return VisualResources.FileTypePath_video_vob;
                    }
                case ".srt":
                    {
                        return VisualResources.FileTypePath_video_subtitle;
                    }
                case ".vob":
                    {
                        return VisualResources.FileTypePath_video_vob;
                    }
                case ".xml":
                case ".shtml":
                case ".js":
                case ".css":
                    {
                        return VisualResources.FileTypePath_web_data;
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
                        return VisualResources.FileTypePath_web_lang;
                    }
                case ".doc":
                case ".docx":
                case ".dotx":
                case ".wps":
                    {
                        return VisualResources.FileTypePath_word;
                    }
                case ".eps":
                case ".svg":
                case ".svgz":
                case ".cdr":
                    {
                        return VisualResources.FileTypePath_vector;
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
                        return VisualResources.FileTypePath_audio;
                    }
                case ".pdf":
                    {
                        return VisualResources.FileTypePath_pdf;
                    }
                case ".ppt":
                case ".pptx":
                case ".pps":
                    {
                        return VisualResources.FileTypePath_powerpoint;
                    }
                case ".swf":
                    {
                        return VisualResources.FileTypePath_swf;
                    }
                case ".txt":
                case ".rtf":
                case ".ans":
                case ".ascii":
                case ".log":
                case ".odt":
                case ".wpd":
                    {
                        return VisualResources.FileTypePath_text;
                    }
                case ".xls":
                case ".xlsx":
                case ".xlt":
                case ".xltm":
                    {
                        return VisualResources.FileTypePath_excel;
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
                        return VisualResources.FileTypePath_compressed;
                    }
                default:
                    {
                        return VisualResources.FileTypePath_generic;
                    }
            }
        }
    }
}
