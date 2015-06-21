using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Processes images from capture (decompressed) and creates a result.
    /// This model could be used for online tracking, calibration, etc.
    /// </summary>
    public abstract class OnlineImageProcessor
    {
        public bool Active
        {
            get { return active; }
        }

        private Thread worker;
        private bool stopAsked;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private object locker = new object();
        private Bitmap currentFrame;
        private Rectangle rect;
        private bool active;

        protected Bitmap workingFrame;
        
        public void Start(int width, int height, PixelFormat pixelFormat)
        {
            if (worker != null && worker.IsAlive)
                Stop();

            currentFrame = new Bitmap(width, height, pixelFormat);
            workingFrame = new Bitmap(width, height, pixelFormat);
            rect = new Rectangle(Point.Empty, currentFrame.Size);
            stopAsked = false;

            active = true;
            worker = new Thread(ProcessorLoop);
            worker.Start();
        }

        public void Stop()
        {
            lock (locker)
            {
                stopAsked = true;
            }

            waitHandle.Set();
            worker.Join();
            active = false;
        }

        /// <summary>
        /// Copies the frame to an internal buffer.
        /// Small lock contention with swap bitmaps.
        /// </summary>
        public void Update(Bitmap frame)
        {
            // Runs in main thread.
            lock (locker)
            {
                BitmapHelper.Copy(frame, currentFrame, rect);
            }

            waitHandle.Set();
        }

        private void ProcessorLoop()
        {
            while (true)
            {
                waitHandle.WaitOne();

                lock (locker)
                {
                    if (stopAsked)
                        break;

                    SwapBitmaps();
                }

                ProcessWorkingFrame();
            }

            // deallocate the bitmaps.
            currentFrame.Dispose();
            workingFrame.Dispose();
        }

        private void SwapBitmaps()
        {
            Bitmap temp = workingFrame;
            workingFrame = currentFrame;
            currentFrame = temp;
        }

        protected abstract void ProcessWorkingFrame();
    }
}
