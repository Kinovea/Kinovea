using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Video;
using Kinovea.Pipeline;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Circular buffer storing delayed frames.
    /// Images are stored as full RGB24 bitmaps.
    /// This buffer uses the infinite array abstraction.
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
        private int fullCapacity;               // Total number of frames kept.
        private int minCapacity = 12;
        private int reserveCapacity = 8;  // Number of frames kept unreachable to clients.
        private int currentPosition = -1;       // Freshest absolute position written to and available to consumers.
        private bool allocated;
        private long availableMemory;
        private ImageDescriptor imageDescriptor;
        private Stopwatch stopwatch = new Stopwatch();
        private object lockerFrame = new object();
        private object lockerPosition = new object();
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

            bool memoryPressure = minCapacity * imageDescriptor.BufferSize > availableMemory;
            if (memoryPressure)
            {
                // The user explicitly asked to not use enough memory. We try to honor the request by lowering the min levels.
                // This may result in thread cross talks.
                reserveCapacity = Math.Max(targetCapacity, 2);
                minCapacity = Math.Max(targetCapacity + 1, 3);
                targetCapacity = Math.Max(targetCapacity, minCapacity);
            }
            else
            {
                reserveCapacity = 8;
                minCapacity = 12;
                targetCapacity = Math.Max(targetCapacity, minCapacity);
            }

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
            //-----------------------------------------
            // Runs in UI thread in mode Camera.
            // Runs in consumer thread in mode Delayed.
            //-----------------------------------------
            if (!allocated)
                return false;

            int nextPosition = currentPosition + 1;
            int index = nextPosition % fullCapacity;
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
            
            // Lock on write just to avoid a torn read in Get().
            lock(lockerPosition)
                currentPosition = nextPosition;

            return pushed;
        }

        /// <summary>
        /// Get the frame from `age` frames ago, wait for it if necessary, copy it into the passed buffer.
        /// </summary>
        public bool GetStrong(int age, byte[] buffer)
        {
            //-----------------------------------------------
            // Runs in the consumer thread, during recording.
            //-----------------------------------------------
            Bitmap image = Get(age);
            if (image == null)
                return false;

            // The UI thread and the recording thread can ask the same image at the same time.
            // Here we have a strong need to get the image out, so in the event the UI has 
            // taken the lock on the image, we wait for it.
            lock (lockerFrame)
            {
                BitmapHelper.CopyBitmapToBuffer(image, buffer);
            }

            return true;
        }

        /// <summary>
        /// Get the frame from `age` frames ago, do not wait for it if it's not available, returns a copy or null. 
        /// </summary>
        public Bitmap GetWeak(int age)
        {
            //----------------------------------------------------
            // Runs in the UI thread, to get the image to display.
            //----------------------------------------------------
            Bitmap image = Get(age);
            if (image == null)
                return null;

            Bitmap copy = null;

            // The UI thread and the recording thread can ask the same image at the same time.
            // Here we yield priority to the recording, so if the lock is taken, we return immediately.
            if (Monitor.TryEnter(lockerFrame, 0))
            {
                try
                {
                    copy = BitmapHelper.Copy(image);
                }
                finally
                {
                    Monitor.Exit(lockerFrame);
                }
            }

            return copy;
        }

        /// <summary>
        /// Retrieve a frame from "age" frames ago. Returns the original image or null.
        /// </summary>
        private Bitmap Get(int age)
        {
            //----------------------------------------------------------
            // Runs in UI thread in mode Camera for display (through compositor).
            // Runs in UI thread in mode Delayed for display (through compositor).
            // Runs in consumer thread in mode Delayed for recording.
            //----------------------------------------------------------

            if (!allocated || images.Count == 0)
                return null;

            int newestAvailablePosition = 0;

            // We only lock on reading to avoid a torn read if the other thread is writing to this variable.
            // The mechanism to avoid actually reading the frame we want while the other thread is writing to it 
            // is the reserve capacity.
            lock (lockerPosition)
                newestAvailablePosition = currentPosition;

            if (newestAvailablePosition < 0)
                return null;

            if (newestAvailablePosition - age <= 0)
            {
                // This happens if delay is set and we haven't captured these images yet.
                return null;
            }

            // The producer may currently be writing the slot after the newest available position, which may wrap around the ring buffer.
            // we use the reserve capacity to give the writer some room.
            // Both are only doing copies so there should be very little chance that the writer had time to 
            // overwrite more than reserve capacity while the reader is still making one copy.
            int requestedPosition = newestAvailablePosition - age;
            int oldestAvailablePosition = newestAvailablePosition - (fullCapacity - 1) + reserveCapacity;
            int finalPosition = Math.Max(requestedPosition, oldestAvailablePosition);

            // We return the actual image, not a copy. The caller is responsible for doing its own copy as fast as possible.
            // If not fast enough, the writer could catch up the reserve capacity and start writing this slot.
            return images[finalPosition % fullCapacity];
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
