using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kinovea.Pipeline.MemoryLayout;
using Kinovea.Services;
using System.Drawing;
using Kinovea.Pipeline;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using TurboJpegNet;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Pipeline consumer wrapping the main UI thread.
    /// Convert the sample buffer to RGB24, then to Bitmap for display purposes.
    /// </summary>
    public class ConsumerDisplay : IFrameConsumer
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern int memcpy(void* dest, void* src, int count);

        public bool Started
        {
            get { return true; }
        }

        public bool Active
        {
            get { return true; }
        }
        
        public long ConsumerPosition
        {
            get 
            {
                // Always report that we are up to date so we never clog the pipe.
                return buffer.ProducerPosition;
            }
        }

        public Bitmap Bitmap
        {
            get { return bitmap; }
        }

        // Frame memory storage
        private RingBuffer buffer;
        private int frameLength;
        private ImageDescriptor imageDescriptor;
        private int width;
        private int height;
        private int pitch;
        private Rectangle rect;
        private Bitmap bitmap;
        private byte[] decoded;

        public void Run()
        {
            throw new NotSupportedException();
        }

        public void SetRingBuffer(RingBuffer buffer)
        {
            this.buffer = buffer;
            this.frameLength = buffer.FrameLength;
        }

        public void ClearRingBuffer()
        {
            buffer = null;
            frameLength = 0;
        }

        public void Activate()
        {
            throw new NotSupportedException();
        }

        public void Deactivate()
        {
            throw new NotSupportedException();
        }

        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
            
            if (decoded != null)
                decoded = null;

            GC.Collect();

            this.imageDescriptor = imageDescriptor;
            width = imageDescriptor.Width;
            height = imageDescriptor.Height;
            rect = new Rectangle(0, 0, width, height);
            pitch = width * 3;
            
            decoded = new byte[pitch * height];
            bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        /// <summary>
        /// Consume a single frame and return immediately.
        /// Publish the Bitmap in Bitmap property.
        /// Returns immediately if no frame are available in the pipe.
        /// This should be called after the main thread received a notification of a frame event from the camera.
        /// </summary>
        public void ConsumeOne()
        {
            // We only consume a single frame to avoid slowing down the chain.
            // Dropping frames is not critical for display.
            // Always consume the very latest frame. <- Fix this for delayed display.

            if (buffer.ProducerPosition < 0)
                return;

            long next = buffer.ProducerPosition;
            
            Frame entry = buffer.GetEntry(next);
            ProcessEntry(next, entry);
        }

        private void ProcessEntry(long position, Frame entry)
        {
            switch (imageDescriptor.Format)
            {
                case Video.ImageFormat.RGB24:
                    FillBitmapRGB24(entry.Buffer);
                    break;
                case Video.ImageFormat.JPEG:
                    FillBitmapJPEG(entry.Buffer, entry.PayloadLength);
                    break;
            }
        }

        private unsafe void FillBitmapRGB24(byte[] buffer)
        {
            // Source is a bottom-up RGB24 buffer.
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Copy + flip the buffer into the bitmap.
            int stride = bmpData.Stride;

            fixed (byte* pBuffer = buffer)
            {
                byte* src = pBuffer;
                byte* dst = (byte*)bmpData.Scan0.ToPointer() + stride * (height - 1);

                for (int i = 0; i < height; i++)
                {
                    memcpy(dst, src, stride);
                    src += stride;
                    dst -= stride;
                }
            }

            bitmap.UnlockBits(bmpData);
        }

        private void FillBitmapJPEG(byte[] buffer, int payloadLength)
        {
            // Convert JPEG to RGB24 buffer then to bitmap.

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
    }
}
