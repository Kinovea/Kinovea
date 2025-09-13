using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using Kinovea.Services.TurboJpeg;

namespace Kinovea.Services
{
    public static class BitmapHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Copy a bitmap into another
        /// <summary>
        /// Allocate a new bitmap and copy the passed bitmap into it.
        /// </summary>
        public static Bitmap Copy(Bitmap src)
        {
            if (src is null)
                return null;

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
            BitmapData srcData = null;
            BitmapData dstData = null;
            try
            {
                srcData = src.LockBits(rect, ImageLockMode.ReadOnly, src.PixelFormat);
                dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, dst.PixelFormat);
                NativeMethods.memcpy(dstData.Scan0.ToPointer(), srcData.Scan0.ToPointer(), srcData.Height * srcData.Stride);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying bitmaps. {0}", e.Message);
            }
            finally
            {
                if (dstData != null)
                    dst.UnlockBits(dstData);

                if (srcData != null)
                    src.UnlockBits(srcData);
            }

            return;
        }

        /// <summary>
        /// Allocate a new bitmap and copy a grayscale version of the source into it.
        /// The source bitmap MUST be 4 bytes per pixel.
        /// The output bitmap has the same pixel format as the source.
        /// </summary>
        public unsafe static Bitmap Grayscale(Bitmap src)
        {
            const float r = 0.2125f;
            const float g = 0.7154f;
            const float b = 0.0721f;

            if (src.PixelFormat != PixelFormat.Format32bppRgb &&
                src.PixelFormat != PixelFormat.Format32bppArgb &&
                src.PixelFormat != PixelFormat.Format32bppPArgb)
                return null;

            Rectangle rect = new Rectangle(0, 0, src.Width, src.Height);
            BitmapData srcData = src.LockBits(rect, ImageLockMode.ReadOnly, src.PixelFormat);
            int pixelSize = 4;
            int srcOffset = srcData.Stride - src.Width * pixelSize;

            Bitmap dst = new Bitmap(src.Width, src.Height, src.PixelFormat);
            BitmapData dstData = dst.LockBits(rect, ImageLockMode.ReadOnly, dst.PixelFormat);
            int dstOffset = dstData.Stride - dst.Width * pixelSize;

            byte* pSrc = (byte*)srcData.Scan0.ToPointer();
            byte* pDst = (byte*)dstData.Scan0.ToPointer();

            for (int y = 0; y < rect.Height; y++)
            {
                for (int x = 0; x < rect.Width; x++, pSrc += pixelSize, pDst += pixelSize)
                {
                    byte value = (byte)(b * pSrc[0] + g * pSrc[1] + r * pSrc[2]);
                    pDst[0] = value;
                    pDst[1] = value;
                    pDst[2] = value;
                    pDst[3] = 255;
                }

                pSrc += srcOffset;
                pDst += dstOffset;
            }

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }
        
        /// <summary>
        /// Copy a region of interest from src into dst.
        /// The ROI rectangle is taken from the top-left point, the width and height should already be 
        /// set in the destination image size.
        /// </summary>
        public unsafe static void CopyROI(Bitmap src, Bitmap dst, Point topLeft)
        {
            BitmapData imageData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, src.PixelFormat);
            BitmapData templateData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.ReadWrite, dst.PixelFormat);

            int pixelSize = 4;

            int tplStride = templateData.Stride;
            int templateWidthInBytes = dst.Width * pixelSize;
            int tplOffset = tplStride - templateWidthInBytes;

            int imgStride = imageData.Stride;
            int imageWidthInBytes = src.Width * pixelSize;
            int imgOffset = imgStride - (src.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;

            unsafe
            {
                byte* pTpl = (byte*)templateData.Scan0.ToPointer();
                byte* pImg = (byte*)imageData.Scan0.ToPointer() + (imgStride * topLeft.Y) + (pixelSize * topLeft.X);

                for (int row = 0; row < dst.Height; row++)
                {
                    if (topLeft.Y + row > imageData.Height - 1)
                    {
                        break;
                    }

                    for (int col = 0; col < templateWidthInBytes; col++, pTpl++, pImg++)
                    {
                        if (topLeft.X * pixelSize + col < imageWidthInBytes)
                        {
                            *pTpl = *pImg;
                        }
                    }

                    pTpl += tplOffset;
                    pImg += imgOffset;
                }
            }

            src.UnlockBits(imageData);
            dst.UnlockBits(templateData);
        }

        /// <summary>
        /// Extension method. Extract a rectangular region out of a bitmap.
        /// </summary>
        public static Bitmap ExtractTemplate(this Bitmap image, Rectangle region)
        {
            // TODO: test perfs by simply drawing in the new image.

            Bitmap template = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppPArgb);

            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            BitmapData templateData = template.LockBits(new Rectangle(0, 0, template.Width, template.Height), ImageLockMode.ReadWrite, template.PixelFormat);

            int pixelSize = 4;

            int tplStride = templateData.Stride;
            int templateWidthInBytes = region.Width * pixelSize;
            int tplOffset = tplStride - templateWidthInBytes;

            int imgStride = imageData.Stride;
            int imageWidthInBytes = image.Width * pixelSize;
            int imgOffset = imgStride - (image.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;

            int startY = Math.Max(0, region.Top);
            int startX = Math.Max(0, region.Left);

            unsafe
            {
                byte* pTpl = (byte*)templateData.Scan0.ToPointer();
                byte* pImg = (byte*)imageData.Scan0.ToPointer() + (imgStride * startY) + (pixelSize * startX);

                for (int row = 0; row < region.Height; row++)
                {
                    if (startY + row > imageData.Height - 1)
                        break;

                    for (int col = 0; col < templateWidthInBytes; col++, pTpl++, pImg++)
                    {
                        if (startX * pixelSize + col < imageWidthInBytes)
                            *pTpl = *pImg;
                    }

                    pTpl += tplOffset;
                    pImg += imgOffset;
                }
            }

            image.UnlockBits(imageData);
            template.UnlockBits(templateData);

            return template;
        }

        #endregion

        #region Copy a byte buffer into a Bitmap

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
        /// Copy an RGB32 buffer into an RGB24 bitmap.
        /// The buffer is expected dense.
        /// </summary>
        public unsafe static void FillFromRGB32(Bitmap bitmap, Rectangle rect, bool topDown, byte[] buffer)
        {
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int dstStride = bmpData.Stride;
            int dstOffset = dstStride - (rect.Width * 3);

            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;

                if (topDown)
                {
                    byte* dst = (byte*)bmpData.Scan0.ToPointer();
                    for (int i = 0; i < rect.Height; i++)
                    {
                        for (int j = 0; j < rect.Width; j++)
                        {
                            dst[0] = src[0];
                            dst[1] = src[1];
                            dst[2] = src[2];
                            src += 4;
                            dst += 3;
                        }

                        dst += dstOffset;
                    }
                }
                else
                {
                    for (int i = 0; i < rect.Height; i++)
                    {
                        byte* dst = (byte*)bmpData.Scan0.ToPointer() + (dstStride * (rect.Height - 1 - i));

                        for (int j = 0; j < rect.Width; j++)
                        {
                            dst[0] = src[0];
                            dst[1] = src[1];
                            dst[2] = src[2];
                            src += 4;
                            dst += 3;
                        }
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
            int dstStride = bmpData.Stride;
            int dstOffset = bmpData.Stride - (rect.Width * 3);
            
            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;

                if (topDown)
                {
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
                else
                {
                    byte* dst = null;
                    for (int i = 0; i < rect.Height; i++)
                    {
                        dst = (byte*)bmpData.Scan0.ToPointer() + (dstStride * (rect.Height - 1 - i));

                        for (int j = 0; j < rect.Width; j++)
                        {
                            dst[0] = dst[1] = dst[2] = *src;
                            src++;
                            dst += 3;
                        }
                    }
                }
            }

            bitmap.UnlockBits(bmpData);
        }

        /// <summary>
        /// Copy the buffer into the bitmap.
        /// The buffer is assumed JPEG. 
        /// The Bitmap should be RGB24, already allocated.
        /// The decoded array should already be allocated and big enough to hold the RGB24 frame bytes.
        /// </summary>
        public static void FillFromJPEG(Bitmap bitmap, Rectangle rect, byte[] decoded, byte[] buffer, int payloadLength, int pitch)
        {
            // Convert JPEG to RGB24 buffer then to bitmap.
            // Assumes the JPEG width is a multiple of 4.

            IntPtr handle = tjnet.tjInitDecompress();

            uint jpegSize = (uint)payloadLength;
            int width;
            int height;
            TJSAMP jpegSubsamp;
            tjnet.tjDecompressHeader2(handle, buffer, jpegSize, out width, out height, out jpegSubsamp);

            tjnet.tjDecompress2(handle, buffer, jpegSize, decoded, width, pitch, height, TJPF.TJPF_BGR, TJFLAG.TJFLAG_FASTDCT);

            tjnet.tjDestroy(handle);

            // Encapsulate into bitmap.
            // Fixme: do we need the copy here? What about getting an IntPtr from tjnet and setting it to scan0?
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(decoded, 0, bmpData.Scan0, bmpData.Stride * bitmap.Height);
            bitmap.UnlockBits(bmpData);
        }

        #endregion

        #region Copy a Bitmap into a byte buffer

        /// <summary>
        /// Copy the whole bitmap into a rectangle in the frame buffer.
        /// The source bitmap is expected to be smaller than destination.
        /// </summary>
        public unsafe static void CopyBitmapToBufferRectangle(Bitmap bitmap, Point location, byte[] buffer, int dstStride)
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

        /// <summary>
        /// Copies the whole bitmap into the frame buffer.
        /// The source bitmap is expected to be the same size as the frame buffer.
        /// </summary>
        public static void CopyBitmapToBuffer(Bitmap bitmap, byte[] buffer)
        {
            BitmapData bmpData = null;
            try
            {
                Rectangle bmpRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bmpData = bitmap.LockBits(bmpRectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                Marshal.Copy(bmpData.Scan0, buffer, 0, bmpData.Stride * bitmap.Height);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying bitmap to buffer. {0}", e.Message);
            }
            finally
            {
                if (bmpData != null)
                    bitmap.UnlockBits(bmpData);
            }
        }
        #endregion
    }
}
