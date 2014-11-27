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
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern int memcpy(void* dest, void* src, int count);

        /// <summary>
        /// Copy + flip the buffer into the bitmap.
        /// The buffer is assumed RGB24 and the Bitmap must already be allocated.
        /// </summary>
        public unsafe static void FillFromRGB24(Bitmap bitmap, Rectangle rect, byte[] buffer)
        {
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int stride = bmpData.Stride;

            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;
                byte* dst = (byte*)bmpData.Scan0.ToPointer() + stride * (rect.Height - 1);

                for (int i = 0; i < rect.Height; i++)
                {
                    memcpy(dst, src, stride);
                    src += stride;
                    dst -= stride;
                }
            }

            bitmap.UnlockBits(bmpData);
        }
    }
}
