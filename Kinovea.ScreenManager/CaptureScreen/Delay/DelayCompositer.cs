using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Takes several frames from the delay buffer and build a composite image.
    /// The description of the composite itself is in a DelayComposite object.
    /// There is one instance of this class per Capture screen, with the same lifetime.
    /// </summary>
    public class DelayCompositer
    {
        #region Members
        private Delayer delayer;
        //private IDelayComposite composite = new DelayCompositeBasic();
        //private IDelayComposite composite = new DelayCompositeMultiReview();
        //private IDelayComposite composite = new DelayCompositeFrozenMosaic();
        private IDelayComposite composite;
        private bool allocated;
        private Bitmap image;
        private Rectangle rect;
        private ImageDescriptor imageDescriptor;
        private int currentPosition;
        private Font font = new Font("Arial", 20, FontStyle.Bold);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public DelayCompositer(Delayer delayer)
        {
            this.delayer = delayer;
        }

        /// <summary>
        /// Attempt to preallocate the circular buffer for as many images as possible that fits in available memory.
        /// </summary>
        public bool AllocateBuffers(ImageDescriptor imageDescriptor)
        {
            if (composite == null)
                throw new InvalidOperationException();

            if (!NeedsReallocation(imageDescriptor))
                return true;

            Free();

            int width = imageDescriptor.Width;
            int height = imageDescriptor.Height;
            PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

            try
            {
                image = new Bitmap(width, height, pixelFormat);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while allocating delay compositer buffer.");
                log.Error(e);
            }

            if (image != null)
            {
                this.rect = new Rectangle(0, 0, width, height);

                this.allocated = true;
                this.imageDescriptor = imageDescriptor;

                currentPosition = 0;
                composite.UpdateSubframes(imageDescriptor, delayer.Capacity);

                // Better do the GC now to push everything to gen2 and LOH rather than taking a hit later during normal streaming operations.
                GC.Collect(2);
            }

            return allocated;
        }

        public void Free()
        {
            if (image != null)
                image.Dispose();
            
            allocated = false;
            rect = Rectangle.Empty;
            imageDescriptor = ImageDescriptor.Invalid;
        }

        public void SetComposite(IDelayComposite composite)
        {
            this.composite = composite;
        }

        public void ResetComposite(IDelayComposite composite)
        {
            this.composite = composite;
            currentPosition = 0;
            composite.UpdateSubframes(imageDescriptor, delayer.Capacity);
        }

        public Bitmap Get(int age)
        {
            if (composite == null)
                throw new InvalidOperationException();

            if (!allocated || image == null)
                return null;

            if (composite.Subframes == null || composite.Subframes.Count == 0)
                return delayer.Get(age);

            Graphics g = Graphics.FromImage(image);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            composite.Tick();
            currentPosition++;
            if (!composite.NeedsRefresh)
                return image;

            foreach (IDelaySubframe subframe in composite.Subframes)
            {
                int subframeAge = composite.GetAge(subframe);

                if (subframeAge < 0)
                    continue;

                Bitmap subframeImage = delayer.Get(subframeAge);
                g.DrawImage(subframeImage, subframe.Bounds);
                
                // Debug
                int subframePosition = currentPosition - subframeAge;
                string text = string.Format("frame:{0}, current:{1}.", subframePosition, currentPosition);
                g.DrawString(text, font, Brushes.Red, subframe.Bounds.Location);
            }
            
            return image;
        }

        private bool NeedsReallocation(ImageDescriptor imageDescriptor)
        {
            return !allocated || this.imageDescriptor != imageDescriptor;
        }
    }
}
