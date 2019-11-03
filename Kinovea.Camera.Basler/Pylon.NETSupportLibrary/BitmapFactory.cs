using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace PylonC.NETSupportLibrary
{
    /* Provides methods for creating and updating bitmaps with raw image data. */
    public static class BitmapFactory
    {
        /* Returns the corresponding pixel format of a bitmap. */
        private static PixelFormat GetFormat(bool color)
        {
            return color ? PixelFormat.Format32bppRgb : PixelFormat.Format8bppIndexed;
        }

        /* Calculates the length of one line in byte. */
        private static int GetStride(int width, bool color)
        {
            return color ? width * 4 : width;
        }

        /* Compares the properties of the bitmap with the supplied image data. Returns true if the properties are compatible. */
        public static bool IsCompatible(Bitmap bitmap, int width, int height, bool color)
        {
            if (bitmap == null
                || bitmap.Height != height
                || bitmap.Width != width
                || bitmap.PixelFormat != GetFormat(color)
             )
            {
                return false;
            }
            return true;
        }

        /* Creates a new bitmap object with the supplied properties. */
        public static void CreateBitmap(out Bitmap bitmap, int width, int height, bool color)
        {
            bitmap = new Bitmap(width, height, GetFormat(color));

            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette colorPalette = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                {
                    colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = colorPalette;
            }
        }

        /* Copies the raw image data to the bitmap buffer. */
        public static void UpdateBitmap(Bitmap bitmap, byte[] buffer, int width, int height, bool color)
        {
            /* Check if the bitmap can be updated with the image data. */
            if (!IsCompatible( bitmap, width, height, color))
            {
                throw new Exception("Cannot update incompatible bitmap.");
            }

            /* Lock the bitmap's bits. */
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            /* Get the pointer to the bitmap's buffer. */
            IntPtr ptrBmp = bmpData.Scan0;
            /* Compute the width of a line of the image data. */
            int imageStride = GetStride(width, color);
            /* If the widths in bytes are equal, copy in one go. */
            if (imageStride == bmpData.Stride)
            {
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, ptrBmp, bmpData.Stride * bitmap.Height );
            }
            else /* The widths in bytes are not equal, copy line by line. This can happen if the image width is not divisible by four. */
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(buffer, i * imageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), width);
                }
            }
            /* Unlock the bits. */
            bitmap.UnlockBits(bmpData);
        }
    }
}
