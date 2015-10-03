using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Kinovea.Video
{
    public static class BitmapHelper
    {
        /// <summary>
        /// Allocate a new bitmap and copy the passed bitmap into it.
        /// </summary>
        public static Bitmap Copy(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height, src.PixelFormat);
            Rectangle rect = new Rectangle(0, 0, src.Width, src.Height);

            Copy(src, dst, rect);
            
            return dst;
        }

        /// <summary>
        /// Copy a bitmap into another. Destination bitmap is assumed to already be allocated and at the proper size/pixel format.
        /// </summary>
        public unsafe static void Copy(Bitmap src, Bitmap dst, Rectangle rect)
        {
            BitmapData srcData = src.LockBits(rect, ImageLockMode.ReadOnly, src.PixelFormat);
            BitmapData dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, dst.PixelFormat);

            NativeMethods.memcpy(dstData.Scan0.ToPointer(), srcData.Scan0.ToPointer(), srcData.Height * srcData.Stride);

            dst.UnlockBits(dstData);
            src.UnlockBits(srcData);
        }

        /// <summary>
        /// Copy the buffer into the bitmap line by line, with optional vertical flip.
        /// The buffer is assumed RGB24 and the Bitmap must already be allocated.
        /// FIXME: this probably doesn't work well with image size with row padding.
        /// </summary>
        public unsafe static void FillFromRGB24(Bitmap bitmap, Rectangle rect, bool topDown, byte[] buffer)
        {
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int srcStride = rect.Width * 3;
            int dstStride = bmpData.Stride;

            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;

                if (topDown)
                {
                    byte* dst = (byte*)bmpData.Scan0.ToPointer();

                    for (int i = 0; i < rect.Height; i++)
                    {
                        NativeMethods.memcpy(dst, src, srcStride);
                        src += srcStride;
                        dst += dstStride;
                    }
                }
                else
                {
                    byte* dst = (byte*)bmpData.Scan0.ToPointer() + (dstStride * (rect.Height - 1));

                    for (int i = 0; i < rect.Height; i++)
                    {
                        NativeMethods.memcpy(dst, src, srcStride);
                        src += srcStride;
                        dst -= dstStride;
                    }
                }
            }

            bitmap.UnlockBits(bmpData);
        }

        /// <summary>
        /// Copy the buffer into the bitmap.
        /// The buffer is assumed Y800 with no padding and the Bitmap is RGB24 and already allocated.
        /// </summary>
        public unsafe static void FillFromY800(Bitmap bitmap, Rectangle rect, bool topDown, byte[] buffer)
        {
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int dstOffset = bmpData.Stride - (rect.Width * 3);
            
            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;
                byte* dst = (byte*)bmpData.Scan0.ToPointer();

                for (int i = 0; i < rect.Height; i++)
                {
                    for (int j = 0; j < rect.Width; j++)
                    {
                        dst[0] = dst[1] = dst[2] = *src;
                        src++;
                        dst += 3;
                    }

                    dst += dstOffset;
                }
            }

            bitmap.UnlockBits(bmpData);
        }
    
        /// <summary>
        /// Copy the whole bitmap into a rectangle in the frame buffer.
        /// The source bitmap is expected to be smaller than destination.
        /// </summary>
        public unsafe static void CopyBitmapRectangle(Bitmap bitmap, Point location, byte[] buffer, int dstStride)
        {
            Rectangle bmpRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(bmpRectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int srcStride = bmpData.Stride;

            fixed (byte* pBuffer = buffer)
            {
                byte* src = (byte*)bmpData.Scan0.ToPointer();
                byte* dst = pBuffer + ((location.Y * dstStride) + (location.X * 3));

                for (int i = 0; i < bmpRectangle.Height; i++)
                {
                    NativeMethods.memcpy(dst, src, srcStride);
                    src += srcStride;
                    dst += dstStride;
                }
            }

            bitmap.UnlockBits(bmpData);
        }
    }
}
