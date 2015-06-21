using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Allow to review the motion in continuous slow motion.
    /// Each subframe is an independant stream of images in slow motion.
    /// The subframes are staggerred in time so that the global frame does not miss some of the action.
    /// Each subframe only reviews a fraction of the time and each subframe has discontinuities.
    /// 
    /// </summary>
    public class DelayCompositeSlowMotion : IDelayComposite
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
        private int imageCount;
        private float refreshRate;
        private bool covered;

        public DelayCompositeSlowMotion(DelayCompositeConfiguration configuration)
        {
            imageCount = configuration.ImageCount;
            refreshRate = configuration.RefreshRate;
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
            DelaySubframeVariable dsv = subframe as DelaySubframeVariable;
            return dsv.GetAge(currentPosition, totalFrames);
        }

        private void SetConfiguration(ImageDescriptor imageDescriptor, int totalFrames, int count)
        {
            subframes.Clear();
            currentPosition = 0;
            int sidecount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));

            Size size = new Size(imageDescriptor.Width / sidecount, imageDescriptor.Height / sidecount);

            // We can only consume that many contiguous frames from the buffer.
            int cycleDuration = (int)Math.Round(totalFrames * refreshRate);
            
            // Each subframe will start staggered by this amount.
            // If shift is less than cycle duration, we are going to miss some frames.
            int shift = (int)Math.Round((float)(totalFrames - cycleDuration) / (count - 1));

            covered = shift <= cycleDuration;

            for (int i = 0; i < sidecount; i++)
            {
                for (int j = 0; j < sidecount; j++)
                {
                    int index = (i * sidecount + j);
                    if (index > count - 1)
                        continue;

                    int startPosition = index * shift;

                    Rectangle bounds = new Rectangle(j * size.Width, i * size.Height, size.Width, size.Height);
                    DelaySubframeVariable subframe = new DelaySubframeVariable(bounds, refreshRate, totalFrames, startPosition, cycleDuration);
                    subframes.Add(subframe);
                }
            }
        }

    }
}
