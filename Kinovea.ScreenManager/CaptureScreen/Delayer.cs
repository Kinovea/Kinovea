using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Video;
using Kinovea.Pipeline;
using System.Drawing.Imaging;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Circular buffer storing delayed frames.
    /// Images are stored as full RGB24.
    /// This buffer uses the infinite array abstraction.
    /// This class is not thread safe. For a thread safe and optimised producer/consumer framework, look at Kinovea.Pipeline.
    /// </summary>
    public class Delayer
    {
        #region Properties
        public int Capacity
        {
            get { return capacity; }
        }
        public bool Allocated
        {
            get { return allocated; }
        }
        #endregion

        #region Members
        private List<Bitmap> images = new List<Bitmap>();
        private Rectangle rect;
        private int capacity = 10;
        private int currentPosition = -1; // The last absolute position written to by the producer.
        private bool allocated;
        private long availableMemory;
        private ImageDescriptor imageDescriptor;
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

            Free();
            
            int targetCapacity = (int)(availableMemory / imageDescriptor.BufferSize);
            int width = imageDescriptor.Width;
            int height = imageDescriptor.Height;
            PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

            try
            {
                for (int i = 0; i < targetCapacity; i++)
                {
                    Bitmap slot = new Bitmap(width, height, pixelFormat);
                    images.Add(slot);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while allocating delayer circular buffer.");
                log.Error(e);
            }

            if (images.Count > 0)
            {
                this.rect = new Rectangle(0, 0, width, height);

                this.allocated = true;
                this.capacity = images.Count;
                this.availableMemory = availableMemory;
                this.imageDescriptor = imageDescriptor;

                // Better do the GC now to push everything to gen2 and LOH rather than taking a hit later during normal streaming operations.
                GC.Collect(2);
            }
            
            return allocated;
        }

        /// <summary>
        /// Push a single frame to the buffer.
        /// Copies the bitmap into a preallocated slot.
        /// </summary>
        public void Push(Bitmap src)
        {
            if (!allocated)
                return;

            currentPosition++;

            int index = currentPosition % capacity;
            BitmapHelper.Copy(src, images[index], rect);
        }

        /// <summary>
        /// Retrieve a frame from "age" frames ago.
        /// If the requested frame is no longer in memory, returns the oldest frame.
        /// </summary>
        public Bitmap Get(int age)
        {
            if (!allocated || currentPosition < 0 || images.Count == 0)
                return null;

            if (currentPosition - age <= 0)
                return images[0];

            int position = Math.Max(currentPosition - age, (currentPosition - capacity + 1));
            return images[position % capacity];
        }
        
        /// <summary>
        /// Free the circular buffer and reset state.
        /// </summary>
        public void Free()
        {
            foreach (Bitmap image in images)
                image.Dispose();

            images.Clear();
            
            allocated = false;
            capacity = 0;
            rect = Rectangle.Empty;
            imageDescriptor = ImageDescriptor.Invalid;
            availableMemory = 0;
            currentPosition = -1;
        }
        #endregion

        #region Private methods
        private bool NeedsReallocation(ImageDescriptor imageDescriptor, long availableMemory)
        {
            return !allocated || this.imageDescriptor != imageDescriptor || this.availableMemory != availableMemory;
        }
        #endregion
    }
}
