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
    /// Simpler version of the DelayCompositeSlowMotion with only one stream.
    /// The stream has synchronization gaps but the user can reset it manually.
    /// This is more usable than the version where the user has to guess where to look.
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

        public float RefreshRate
        {
            get { return refreshRate; }
        }

        private List<IDelaySubframe> subframes = new List<IDelaySubframe>();
        private int currentPosition;
        private int totalFrames;
        private float refreshRate = 1.0f;

        public DelayCompositeSlowMotion()
        {
        }

        public void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames)
        {
            this.totalFrames = totalFrames;
            SetConfiguration(imageDescriptor, totalFrames);
        }

        public void Tick()
        {
            currentPosition++;
        }

        public void Rewind(int position)
        {
            currentPosition = position;
            Sync();
        }
        
        public int GetAge(IDelaySubframe subframe)
        {
            DelaySubframeVariable dsv = subframe as DelaySubframeVariable;
            return dsv.GetAge(currentPosition, totalFrames);
        }

        public void UpdateRefreshRate(float refreshRate)
        {
            // This does not trigger a reset of the buffer.
            this.refreshRate = refreshRate;
            int cycleDuration = (int)Math.Round(totalFrames * refreshRate);
            foreach (IDelaySubframe subframe in subframes)
                ((DelaySubframeVariable)subframe).UpdateRefreshRate(refreshRate, cycleDuration, currentPosition);
        }

        public void Sync()
        {
            foreach (IDelaySubframe subframe in subframes)
                ((DelaySubframeVariable)subframe).Sync(currentPosition);
        }

        public int GetCountdown()
        {
            if (subframes.Count > 0)
                return ((DelaySubframeVariable)subframes[0]).GetCountdown(currentPosition);
            else
                return 0;
        }

        private void SetConfiguration(ImageDescriptor imageDescriptor, int totalFrames)
        {
            subframes.Clear();
            currentPosition = 0;
            
            // Each substream can only consume that many contiguous frames from the buffer.
            refreshRate = Math.Min(refreshRate, 1.0f);
            int cycleDuration = (int)Math.Round(totalFrames * refreshRate);
            int startPosition = 0;
            
            Rectangle source = new Rectangle(0, 0, imageDescriptor.Width, imageDescriptor.Height);
            Rectangle destination = new Rectangle(0, 0, imageDescriptor.Width, imageDescriptor.Height);
            DelaySubframeVariable subframe = new DelaySubframeVariable(source, destination, refreshRate, totalFrames, startPosition, cycleDuration);
            subframes.Add(subframe);
        }
    }
}
