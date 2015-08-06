using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MegaApp.Extensions
{
    static class BitmapExtensions
    {
        public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    //var image = new Image {Source = bitmapImage};
                    //var btmMap = new WriteableBitmap(image, null);
                    var btmMap = new WriteableBitmap(bitmapImage);

                    // write an image into the stream
                    btmMap.SaveJpeg(ms, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 0, 100);

                    return ms.ToArray();
                }
                catch(Exception)
                {
                    return null;
                }                
            }
        }
    }
}
