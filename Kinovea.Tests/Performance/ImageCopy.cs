using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Kinovea.Tests
{
    /// <summary>
    /// Test various ways to copy image bytes around.
    /// </summary>
    public class ImageCopy
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        private static Random random = new Random();

        public static void Test()
        {
            // This test emulates copy between images or image bytes.
            // Considers that the buffers will be pooled instead of recreated each time.

            // Emulate image bytes, buffer size is aligned on 64K blocks.
            Size size = new Size(2048, 1084);
            int depth = 1;
            int imageWeight = size.Width * size.Height * depth;
            int chunkSize = 64 * 1024;
            int remainder = imageWeight % chunkSize;
            int paddedWeight = imageWeight - remainder + chunkSize;

            byte[] buffer1 = CreateBuffer(paddedWeight);
            byte[] buffer2 = CreateBuffer(paddedWeight);

            

            // Inject bytes into both Bitmap.
            // We do this up front to make sure we only measure "copy" not memory allocation.
            Bitmap bmp1 = CreateBitmap(size, PixelFormat.Format8bppIndexed);
            Bitmap bmp2 = CreateBitmap(size, PixelFormat.Format8bppIndexed);
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            
            CopyBytesToBitmap(buffer1, bmp1, rect);
            CopyBytesToBitmap(buffer2, bmp2, rect);

            float length = (float)buffer1.Length / (1024 * 1024);
            int loops = 10000;

            //TestCopy1(loops, bmp1, bmp2, rect, length);
            //TestCopy2(10, bmp1, bmp2, rect, length);
            //TestCopy3(loops, buffer1, buffer2, length);
            //TestCopy4(1000, buffer1, buffer2, bmp1, bmp2, rect, length);
            TestCopy5(1000, buffer1, buffer2, bmp1, bmp2, rect, length);

            Console.ReadKey();
        }

        private static void TestCopy1(int loops, Bitmap bmp1, Bitmap bmp2, Rectangle rect, float length)
        {
            // Native copy with bitmap locking.

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < loops; i++)
            {
                if (i % 2 == 0)
                    CopyBitmap1(bmp1, bmp2, rect);
                else
                    CopyBitmap1(bmp2, bmp1, rect);
            }

            double elapsed = (double)sw.ElapsedTicks / Stopwatch.Frequency;
            double averageMilliseconds = (elapsed * 1000) / loops;
            Console.WriteLine("Buffer length: {0:0.00} MB. Average copy time ({1} loops): {2:0.000} ms.", length, loops, averageMilliseconds);
        }

        private static void TestCopy3(int loops, byte[] b1, byte[] b2, float length)
        {
            // Managed buffer copy.

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < loops; i++)
            {
                if (i % 2 == 0)
                    CopyBuffer(b1, b2);
                else
                    CopyBuffer(b2, b1);
            }

            double elapsed = (double)sw.ElapsedTicks / Stopwatch.Frequency;
            double averageMilliseconds = (elapsed * 1000) / loops;
            Console.WriteLine("Buffer length: {0:0.00} MB. Average copy time ({1} loops): {2:0.000} ms.", length, loops, averageMilliseconds);
        }

        private static void TestCopy4(int loops, byte[] b1, byte[] b2, Bitmap bmp1, Bitmap bmp2, Rectangle rect, float length)
        {
            // Managed bytes to native buffer.

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < loops; i++)
            {
                if (i % 2 == 0)
                    CopyBytesToBitmap(b1, bmp2, rect);
                else
                    CopyBytesToBitmap(b2, bmp1, rect);
            }

            double elapsed = (double)sw.ElapsedTicks / Stopwatch.Frequency;
            double averageMilliseconds = (elapsed * 1000) / loops;
            Console.WriteLine("Buffer length: {0:0.00} MB. Average copy time ({1} loops): {2:0.000} ms.", length, loops, averageMilliseconds);
        }

        private static void TestCopy5(int loops, byte[] b1, byte[] b2, Bitmap bmp1, Bitmap bmp2, Rectangle rect, float length)
        {
            // Native buffer to managed bytes.

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < loops; i++)
            {
                if (i % 2 == 0)
                    CopyBitmapToBytes(b1, bmp2, rect);
                else
                    CopyBitmapToBytes(b2, bmp1, rect);
            }

            double elapsed = (double)sw.ElapsedTicks / Stopwatch.Frequency;
            double averageMilliseconds = (elapsed * 1000) / loops;
            Console.WriteLine("Buffer length: {0:0.00} MB. Average copy time ({1} loops): {2:0.000} ms.", length, loops, averageMilliseconds);
        }


        private static byte[] CreateBuffer(int size)
        {
            byte[] buffer = new byte[size];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(random.Next() % 256);

            return buffer;
        }

        private static Bitmap CreateBitmap(Size size, PixelFormat format)
        {
            Bitmap b = new Bitmap(size.Width, size.Height, format);

            if (format == PixelFormat.Format8bppIndexed)
            {
                ColorPalette cp = b.Palette;
                for (int i = 0; i < 256; i++)
                    cp.Entries[i] = Color.FromArgb(255, i, i, i);
                b.Palette = cp;
            }

            return b;
        }
    
        /// <summary>
        /// Marshal copy from managed bytes to unmanaged bytes inside bitmap, involves bitmap locking.
        /// </summary>
        private static void CopyBytesToBitmap(byte[] bytes, Bitmap a, Rectangle rect)
        {
            BitmapData bmpData = a.LockBits(rect, ImageLockMode.WriteOnly, a.PixelFormat);
            
            Marshal.Copy(bytes, 0, bmpData.Scan0, bmpData.Stride * a.Height);
            
            a.UnlockBits(bmpData);
        }

        /// <summary>
        /// Marshal copy from unmanaged bytes inside bitmap to managed bytes, involves bitmap locking.
        /// </summary>
        private static void CopyBitmapToBytes(byte[] bytes, Bitmap a, Rectangle rect)
        {
            BitmapData bmpData = a.LockBits(rect, ImageLockMode.ReadOnly, a.PixelFormat);

            Marshal.Copy(bmpData.Scan0, bytes, 0, bmpData.Stride * a.Height);

            a.UnlockBits(bmpData);
        }
    
        /// <summary>
        /// Performs a native copy between image buffers.
        /// </summary>
        private static void CopyBitmap1(Bitmap a, Bitmap b, Rectangle rect)
        {
            // Lock both images, native copy, unlock both.
            BitmapData srcData = a.LockBits(rect, ImageLockMode.ReadOnly, a.PixelFormat);
            BitmapData dstData = b.LockBits(rect, ImageLockMode.WriteOnly, b.PixelFormat);

            memcpy(dstData.Scan0, srcData.Scan0, new UIntPtr((uint)srcData.Height * (uint)srcData.Stride));

            b.UnlockBits(dstData);
            a.UnlockBits(srcData); 
        }
        
        private static void CopyBuffer(byte[] b1, byte[] b2)
        {
            Buffer.BlockCopy(b1, 0, b2, 0, b1.Length);
        }

        
    }
}
