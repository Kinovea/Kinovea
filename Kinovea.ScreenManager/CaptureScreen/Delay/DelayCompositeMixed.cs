using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A hard coded composite with two delayed reviews and two slow motion subframes.
    /// If there is interest for arbitrary composites like this, 
    /// It will be better to have a generic composite described externally.
    /// </summary>
    public class DelayCompositeMixed : IDelayComposite
    {
        public List<IDelaySubframe> Subframes
        {
            get { return subframes; }
        }

        public bool NeedsRefresh
        {
            get { return true; }
        }

        private List<IDelaySubframe> subframes = new List<IDelaySubframe>();
        private int currentPosition;
        private int totalFrames;
        private int realTimeImageCount;
        private int slowMotionImageCount;
        private int imageCount;
        private float refreshRate;

        public DelayCompositeMixed()
        {
            realTimeImageCount = 2;
            slowMotionImageCount = 2;
            imageCount = realTimeImageCount + slowMotionImageCount;

            refreshRate = 0.5f;
        }

        public void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames)
        {
            this.totalFrames = totalFrames;
            SetConfiguration(imageDescriptor, totalFrames, imageCount);
        }

        public void Tick()
        {
            currentPosition++;
        }

        public int GetAge(IDelaySubframe subframe)
        {
            if (subframe is DelaySubframeConstant)
            {
                DelaySubframeConstant dsc = subframe as DelaySubframeConstant;
                return dsc.Age;
            }
            else if (subframe is DelaySubframeVariable)
            {
                DelaySubframeVariable dsv = subframe as DelaySubframeVariable;
                return dsv.GetAge(currentPosition, totalFrames);
            }
            else
            {
                return 0;
            }
        }

        private void SetConfiguration(ImageDescriptor imageDescriptor, int totalFrames, int count)
        {
            subframes.Clear();
            currentPosition = 0;
            int sidecount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));

            Size size = new Size(imageDescriptor.Width / sidecount, imageDescriptor.Height / sidecount);

            Rectangle bounds = new Rectangle(0, 0, size.Width, size.Height);
            DelaySubframeConstant subframe0 = new DelaySubframeConstant(bounds, 0);
            subframes.Add(subframe0);

            bounds = new Rectangle(size.Width, 0, size.Width, size.Height);
            DelaySubframeConstant subframe1 = new DelaySubframeConstant(bounds, totalFrames);
            subframes.Add(subframe1);

            // Slow motion.

            int cycleDuration = (int)Math.Round(totalFrames * refreshRate);
            int shift = (int)Math.Round((float)(totalFrames - cycleDuration) / (slowMotionImageCount - 1));

            int startPosition = 0;

            bounds = new Rectangle(0, size.Height, size.Width, size.Height);
            DelaySubframeVariable subframe2 = new DelaySubframeVariable(bounds, refreshRate, totalFrames, startPosition, cycleDuration);
            subframes.Add(subframe2);

            bounds = new Rectangle(size.Width, size.Height, size.Width, size.Height);
            startPosition += shift;
            DelaySubframeVariable subframe3 = new DelaySubframeVariable(bounds, refreshRate, totalFrames, startPosition, cycleDuration);
            subframes.Add(subframe3);
        }
    }
}
