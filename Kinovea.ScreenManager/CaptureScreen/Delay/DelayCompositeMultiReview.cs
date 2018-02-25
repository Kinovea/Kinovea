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
    /// This configuration allow to review the motion multiple times at the nominal framerate.
    /// Each subframe is an independant stream of images refreshed every tick.
    /// </summary>
    public class DelayCompositeMultiReview : IDelayComposite
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
        private int imageCount = 4;

        public DelayCompositeMultiReview()
        {
        }

        public void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames)
        {
            SetConfiguration(imageDescriptor, totalFrames, imageCount);
        }

        public void Tick()
        {
        }

        public int GetAge(IDelaySubframe subframe)
        {
            DelaySubframeConstant dsc = subframe as DelaySubframeConstant;
            return dsc.Age;
        }

        private void SetConfiguration(ImageDescriptor imageDescriptor, int totalFrames, int count)
        {
            if (imageDescriptor == null || imageDescriptor == ImageDescriptor.Invalid)
                return;

            int interval = 0;
            if (count != 1)
                interval = (int)Math.Floor(totalFrames / (count - 1.0f));

            subframes.Clear();

            int sidecount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));

            Size size = new Size(imageDescriptor.Width / sidecount, imageDescriptor.Height / sidecount);

            for (int i = 0; i < sidecount; i++)
            {
                for (int j = 0; j < sidecount; j++)
                {
                    int index = (i * sidecount + j);
                    if (index > count - 1)
                        continue;

                    int age = index * interval;
                    
                    Rectangle bounds = new Rectangle(j * size.Width, i * size.Height, size.Width, size.Height);
                    DelaySubframeConstant subframe = new DelaySubframeConstant(bounds, age);
                    subframes.Add(subframe);
                }
            }
        }
    }
}
