using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A subframe with a variable age.
    /// The age increase progressively until there are no more slots to use in the buffer.
    /// At that point the age is reset to zero relatively to a baseline.
    /// </summary>
    public class DelaySubframeVariable : IDelaySubframe
    {
        /// <summary>
        /// The location and size of the subframe into the composite.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        private float refreshRate;
        private int totalFrames;
        private int startPosition;
        private int cycleDuration;

        public DelaySubframeVariable(Rectangle bounds, float refreshRate, int totalFrames, int startPosition, int cycleDuration)
        {
            this.Bounds = bounds;
            this.refreshRate = refreshRate;
            this.totalFrames = totalFrames;
            this.startPosition = startPosition;
            this.cycleDuration = cycleDuration;
        }

        public void UpdateRefreshRate(float refreshRate, int cycleDuration, int currentPosition)
        {
            this.refreshRate = refreshRate;
            this.cycleDuration = cycleDuration;
            this.startPosition = currentPosition;
        }

        public int GetAge(int currentPosition, int totalFrames)
        {
            int positionInCycle = currentPosition - startPosition;

            if (positionInCycle < 0)
                return -1;

            int targetPosition = startPosition + (int)(positionInCycle * refreshRate);

            if (targetPosition >= startPosition + cycleDuration)
            {
                targetPosition = currentPosition;
                startPosition = currentPosition;
            }

            int age = currentPosition - targetPosition;
            return age;
        }
    }
}
