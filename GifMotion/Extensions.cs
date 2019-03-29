using System.IO;
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
                Quantizer quantizer;
                switch (quality)
                {
                    case GIFQuality.Grayscale:
                        quantizer = new GrayscaleQuantizer();
                        break;
                    case GIFQuality.Bit4:
                        quantizer = new OctreeQuantizer(15, 4);
                        break;
                    case GIFQuality.Default:
                    case GIFQuality.Bit8:
                    default:
                        quantizer = new OctreeQuantizer(255, 4);
                        break;
                }

                using (Bitmap quantized = quantizer.Quantize(img))
                {
                    quantized.Save(stream, ImageFormat.Gif);
                }
            }

            
        }

        public static void Write(this FileStream stream, byte[] array)
        {
            stream.Write(array, 0, array.Length);
        }
    }
}
