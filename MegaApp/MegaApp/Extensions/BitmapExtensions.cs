using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MegaApp.Extensions
{
    static class BitmapExtensions
    {
        public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
        {
            using (var ms = new MemoryStream())
            {
                var btmMap = new WriteableBitmap(bitmapImage);

                // write an image into the stream
                btmMap.SaveJpeg(ms, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 0, 100);

                return ms.ToArray();
            }
        }
    }
}
