using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GifMotion
{
    public static class Extensions
    {
        public static void SaveGif(this Image img, Stream stream, GIFQuality quality)
        {
            if (quality == GIFQuality.Default)
            {
                img.Save(stream, ImageFormat.Gif);
            }
            else
            {
                // TODO: handle quantizing here...
            }
        }


        public static void Write(this FileStream stream, byte[] array)
        {
            stream.Write(array, 0, array.Length);
        }
    }
}
