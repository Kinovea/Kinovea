using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Video;
using Kinovea.Pipeline;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Circular buffer storing delayed frames.
    /// Images are stored as full RGB24.
    /// This buffer uses the infinite array abstraction.
    /// 
    /// This class is not thread safe and should only be called from the main thread. 
    /// For a thread safe and optimised producer/consumer framework, look at Kinovea.Pipeline.
    /// </summary>
    public class Delayer
    {
        #region Properties
        public int SafeCapacity
        {
            get { return fullCapacity - reserveCapacity; }
        }
        #endregion

        #region Members
        private List<Bitmap> images = new List<Bitmap>();
        private Rectangle rect;
        private int fullCapacity;
        private const int minCapacity = 12;
        private const int reserveCapacity = 8; // number of frames kept unreachable to clients.
        private int currentPosition = -1; // The last absolute position written to by the producer.
        private bool allocated;
        private long availableMemory;
        private ImageDescriptor imageDescriptor;
        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public methods
        /// <summary>
        /// Attempt to preallocate the circular buffer for as many images as possible that fits in available memory.
        /// </summary>
        public bool AllocateBuffers(ImageDescriptor imageDescriptor, long availableMemory)
        {
            if (!NeedsReallocation(imageDescriptor, availableMemory))
                return true;

            int targetCapacity = (int)(availableMemory / imageDescriptor.BufferSize);
            targetCapacity = Math.Max(targetCapacity, minCapacity);
            int width = imageDescriptor.Width;
            int height = imageDescriptor.Height;
            PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

            bool compatible = ImageDescriptor.Compatible(this.imageDescriptor, imageDescriptor);
            if (compatible && targetCapacity <= fullCapacity)
            {
                FreeSome(targetCapacity);
                this.fullCapacity = images.Count;
                this.availableMemory = availableMemory;
                return true;
            }
            
            if (!compatible)
                FreeAll();
            
            stopwatch.Restart();
            images.Capacity = targetCapacity;
            log.DebugFormat("Allocating {0} frames.", targetCapacity - fullCapacity);

            try
            {
                for (int i = fullCapacity; i < targetCapacity; i++)
                {
                    Bitmap slot = new Bitmap(width, height, pixelFormat);
                    images.Add(slot);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while allocating delay buffer.");
                log.Error(e);
            }

            if (images.Count > 0)
            {
                this.rect = new Rectangle(0, 0, width, height);

                this.allocated = true;
                this.fullCapacity = images.Count;
                this.availableMemory = availableMemory;
                this.imageDescriptor = imageDescriptor;

                // Better do the GC now to push everything to gen2 and LOH rather than taking a hit later during normal streaming operations.
                GC.Collect(2);
            }

            log.DebugFormat("Allocated delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, fullCapacity);
            return allocated;
        }

        /// <summary>
        /// Returns true if the delayer needs to allocate or reallocate memory.
        /// </summary>
        public bool NeedsReallocation(ImageDescriptor imageDescriptor, long availableMemory)
        {
            return !allocated || !ImageDescriptor.Compatible(this.imageDescriptor, imageDescriptor) || this.availableMemory != availableMemory;
        }

        /// <summary>
        /// Push a single frame to the buffer.
        /// Copies the bitmap into a preallocated slot.
        /// </summary>
        public bool Push(Bitmap src)
        {
            if (!allocated)
                return false;

            currentPosition++;
            int index = currentPosition % fullCapacity;
            bool pushed = false;
            try
            {
                BitmapHelper.Copy(src, images[index], rect);
                pushed = true;
            }
            catch
            {
                log.ErrorFormat("Failed to push frame to delay buffer.");
            }

            return pushed;
        }

        /// <summary>
        /// Retrieve a frame from "age" frames ago without copy.
        /// If the requested frame is no longer in memory, returns the oldest frame.
        /// Does not have any side-effects.
        /// </summary>
        public Bitmap Get(int age)
        {
            if (!allocated || currentPosition < 0 || images.Count == 0)
                return null;

            if (currentPosition - age <= 0)
                return images[0];

            if (age >= (fullCapacity - reserveCapacity))
            {
                // We are too close to the end of the buffer.
                // This can cause issues now that the writing and reading are done on different threads.
                // The reader (display thread) usually runs at a lower frequency than the writer (camera fps), 
                // so having just a few frames of buffer should be largely enough.
                return null;
            }

            int position = Math.Max(currentPosition - age, (currentPosition - fullCapacity + 1));
            return images[position % fullCapacity];
        }
        
        /// <summary>
        /// Free the circular buffer and reset state.
        /// </summary>
        public void FreeAll()
        {
            stopwatch.Restart();

            if (fullCapacity == 0 && !allocated)
                return;

            log.DebugFormat("Freeing {0} frames.", fullCapacity);

            foreach (Bitmap image in images)
                image.Dispose();

            images.Clear();

            ResetData();

            log.DebugFormat("Freed delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, images.Count);
        }
        
        private void ResetData()
        {
            allocated = false;
            fullCapacity = 0;
            rect = Rectangle.Empty;
            imageDescriptor = ImageDescriptor.Invalid;
            availableMemory = 0;
            currentPosition = -1;
        }

        private void FreeSome(int targetCapacity)
        {
            stopwatch.Restart();

            log.DebugFormat("Freeing {0} frames.", fullCapacity - targetCapacity);

            for (int i = images.Count - 1; i >= targetCapacity; i--)
            {
                images[i].Dispose();
                images.RemoveAt(i);
            }

            log.DebugFormat("Freed delay buffer: {0} ms. Total: {1} frames.", stopwatch.ElapsedMilliseconds, images.Count);
        }
        #endregion

    }
}
