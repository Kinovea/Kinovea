using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Pipeline;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Allows to review a low-refresh rate mosaic of the motion.
    /// </summary>
    public class DelayCompositeFrozenMosaic : IDelayComposite
    {
        public List<IDelaySubframe> Subframes
        {
            get { return subframes; }
        }

        public bool NeedsRefresh
        {
            get { return needsRefresh; }
        }

        private List<IDelaySubframe> subframes = new List<IDelaySubframe>();
        private int currentPosition;
        private int lastRefreshPosition;
        private bool needsRefresh;
        private int imageCount;
        private int start; // age of the first image.
        private int interval; // age difference between images.
        private int period; // amount of time between refreshes. (all images are refreshed at once).

        public DelayCompositeFrozenMosaic(DelayCompositeConfiguration configuration)
        {
            imageCount = configuration.ImageCount;
            start = configuration.Start;
            interval = configuration.Interval;
        }

        public void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames)
        {
            SetConfiguration(imageDescriptor, totalFrames, imageCount);
        }

        public void Tick()
        {
            currentPosition++;

            int lag = currentPosition - lastRefreshPosition;
            needsRefresh = lag >= period;

            if (needsRefresh)
                lastRefreshPosition = currentPosition;
        }

        public int GetAge(IDelaySubframe subframe)
        {
            if (!needsRefresh)
                throw new InvalidOperationException("NeedsRefresh should have been tested before calling GetAge");
            
            DelaySubframeConstant dsc = subframe as DelaySubframeConstant;
            return dsc.Age;
        }

        private void SetConfiguration(ImageDescriptor imageDescriptor, int totalFrames, int count)
        {
            // Frozen mosaic.
            // Note that images are reverted so that motion unwind from left to right and top to bottom.
            // The first image of the index is the oldest one.

            subframes.Clear();

            period = totalFrames / 2;

            int sidecount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));

            Size size = new Size(imageDescriptor.Width / sidecount, imageDescriptor.Height / sidecount);

            for (int i = 0; i < sidecount; i++)
            {
                for (int j = 0; j < sidecount; j++)
                {
                    int index = (i * sidecount + j);
                    if (index > count - 1)
                        continue;

                    Rectangle bounds = new Rectangle(j * size.Width, i * size.Height, size.Width, size.Height);
                    
                    int age = start + ((count - 1 - index) * interval);

                    DelaySubframeConstant subframe = new DelaySubframeConstant(bounds, age);
                    subframes.Add(subframe);
                }
            }
        }
    }
}
