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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Pipeline consumer wrapping the main UI thread.
    /// Convert the image to RGB24, then Bitmap for display purposes.
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
        private Bitmap bitmap;

        public void Run()
        {
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
        }

        public void Deactivate()
        {
        }

        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            this.imageDescriptor = imageDescriptor;
            if (bitmap != null)
                bitmap.Dispose();

            bitmap = new Bitmap(imageDescriptor.Width, imageDescriptor.Height, PixelFormat.Format24bppRgb);
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
            // TODO: Convert to RGB24 if needed.
            FillBitmap(entry.Buffer);
        }

        private unsafe void FillBitmap(byte[] buffer)
        {
            // Source is a bottom up RGB24 buffer.

            int width = imageDescriptor.Width;
            int height = imageDescriptor.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Copy the buffer to the bitmap while reverting it.
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
    }
}
