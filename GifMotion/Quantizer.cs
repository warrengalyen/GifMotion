using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GifMotion
{
    public abstract class Quantizer
    {
        /// <summary>
        /// Construct the quantizer
        /// </summary>
        /// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
        /// <remarks>
        /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
        /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
        /// and then 'QuantizeImage'.
        /// </remarks>
        protected Quantizer(bool singlePass)
        {
            _singlePass = singlePass;
            _pixelSize = Marshal.SizeOf(typeof(Color32));
        }

        public Bitmap Quantize(Image source)
        {
            int height = source.Height;
            int width = source.Width;

            Rectangle bounds = new Rectangle(0, 0, width, height);

            // construct a 32bpp version
            Bitmap copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // construct a 8bpp version
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // lock the bitmap into memory
            using (Graphics g = Graphics.FromImage(copy))
            {
                g.PageUnit = GraphicsUnit.Pixel;

                // Draw the source image onto the copy, which will effect a widening
                // as appropriate.
                g.DrawImage(source, bounds);
            }

            // define a pointer to the bitmap data
            BitmapData sourceData = null;

            try
            {
                // get the source image bits and lock into memory
                sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // Call the FirstPass function if not a single pass algorithm.
                // For something like an octree quantizer, this will run through
                // all image pixels, build a data structure, and create a palette.
                if (!_singlePass)
                    FirstPass(sourceData, width, height);
            }
            finally
            {
                // ensure that bits are unlocked
                copy.UnlockBits(sourceData);
            }

            return output;
        }

        /// <summary>
        /// Execute the first pass through the pixels in the image
        /// </summary>
        /// <param name="sourceData">The source data</param>
        /// <param name="width">The width of the pixels of the image</param>
        /// <param name="height">The height of the pixels of the image</param>
        protected virtual void FirstPass(BitmapData sourceData, int width, int height)
        {
            // Define the source data pointers. The source row is a byte to
            // make addition of the stride value easier (as this is in bytes)
            IntPtr pSourceRow = sourceData.Scan0;

            // Loop through each row
            for (int row = 0; row < height; row++)
            {
                // set the source pixel to the first pixel in this row
                IntPtr pSourcePixel = pSourceRow;

                // now loop through each column
                for (int col = 0; col < width; col++)
                {
                    InitialQuantizePixel(new Color32(pSourcePixel));
                    pSourcePixel = (IntPtr)((long) pSourcePixel + _pixelSize);
                }

                // add the stride to the source row
                pSourceRow = (IntPtr) ((long) pSourceRow + sourceData.Stride);
            }
        }

        /// <summary>
        /// Execute a second pass through the bitmap
        /// </summary>
        /// <param name="sourceData">The source bitmap, locked into memory</param>
        /// <param name="output">The output bitmap</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        /// <param name="bounds">The bounding rectangle</param>
        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData outputData = null;

            try
            {
                // lock the output bitmap into memory
                outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

                // Define the source data pointers. The source row is a byte to
                // make addition of the stride value easier (as this is in bytes)
                IntPtr pSourceRow = sourceData.Scan0;
                IntPtr pSourcePixel = pSourceRow;
                IntPtr pPreviousPixel = pSourcePixel;

                // Define the destination data pointers
                IntPtr pDestinationRow = outputData.Scan0;
                IntPtr pDestinationPixel = pDestinationRow;

                // Convert the first pixel, so we have values going into the loop
                byte pixelValue = QuantizePixel(new Color32(pSourcePixel));

                // Assign the value of the first pixel
                Marshal.WriteByte(pDestinationPixel, pixelValue);

                // Loop through each row
                for (int row = 0; row < height; row++)
                {
                    // set the source pixel to the first pixel in this row
                    pSourcePixel = pSourceRow;

                    // set the destination pixel pointer to the first pixel in the row
                    pDestinationPixel = pDestinationRow;

                    // Loop through each pixel on this scan line
                    for (int col = 0; col < width; col++)
                    {
                        // Check if this is the same as the last pixel. If so use that value
                        // rather than calculating again. This is an inexpensive optimization.
                        if (Marshal.ReadInt32(pPreviousPixel) != Marshal.ReadInt32(pSourcePixel))
                        {
                            // Quantize the pixel
                            pixelValue = QuantizePixel(new Color32(pSourcePixel));

                            // setup the previous pointer
                            pPreviousPixel = pSourcePixel;
                        }

                        // set the pixel in the output
                        Marshal.WriteByte(pDestinationPixel, pixelValue);

                        pSourcePixel = (IntPtr) ((long) pSourcePixel + _pixelSize);
                        pDestinationPixel = (IntPtr) ((long) pDestinationPixel + 1);
                    }

                    // Add the stride to the source row
                    pSourceRow = (IntPtr) ((long) pSourceRow + sourceData.Stride);

                    // Add to the destination row
                    pDestinationRow = (IntPtr) ((long) pDestinationRow + outputData.Stride);
                }
            }
            finally
            {
                // ensure we unlock the output bits
                output.UnlockBits(outputData);
            }
        }

        /// <summary>
        /// Override this to process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function only needs to be overridden if your quantize algorithm needs two passes,
        /// such as an Octree quantizer.
        /// </remarks>
        protected virtual void InitialQuantizePixel(Color32 pixel)
        {
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected abstract byte QuantizePixel(Color32 pixel);

        /// <summary>
        /// Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="original">The old palette, this is overwritten</param>
        /// <returns>The new color palette</returns>
        protected abstract ColorPalette GetPalette(ColorPalette original);

        /// <summary>
        /// Flag used to indicate whether a single pass or two passes are need for quantization.
        /// </summary>
        private bool _singlePass;

        private int _pixelSize;

        /// <summary>
        /// Struct that defines a 32 bpp color
        /// </summary>
        /// <remarks>
        /// This struct is used to read data from a 32 bit pixel image
        /// in memory and is ordered in this manner as this is the way
        /// the data is layed out in memory
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {
            public Color32(IntPtr pSourcePixel)
            {
                this = (Color32) Marshal.PtrToStructure(pSourcePixel, typeof(Color32));
            }

            [FieldOffset(0)] public byte Blue;

            [FieldOffset(0)] public byte Green;

            [FieldOffset(0)] public byte Red;

            [FieldOffset(0)] public byte Alpha;

            [FieldOffset(0)] public int ARGB;

            public Color Color
            {
                get { return Color.FromArgb(Alpha, Red, Green, Blue); }
            }
        }
    }
}

