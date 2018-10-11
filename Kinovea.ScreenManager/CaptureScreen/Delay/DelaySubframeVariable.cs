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
        /// Location and size of the rectangle to extract from the frame.
        /// </summary>
        public Rectangle Source { get; set; }

        /// <summary>
        /// The location and size of destination rectangle in the composite.
        /// </summary>
        public Rectangle Destination { get; set; }

        private float refreshRate;
        private int totalFrames;
        private int startPosition;
        private int cycleDuration;

        public DelaySubframeVariable(Rectangle source, Rectangle destination, float refreshRate, int totalFrames, int startPosition, int cycleDuration)
        {
            this.Source = source;
            this.Destination = destination;
            this.refreshRate = refreshRate;
            this.totalFrames = totalFrames;
            this.startPosition = startPosition;
            this.cycleDuration = cycleDuration;
        }

        public void UpdateRefreshRate(float refreshRate, int cycleDuration, int currentPosition)
        {
            this.refreshRate = refreshRate;
            this.cycleDuration = cycleDuration;
            startPosition = currentPosition;
        }

        public void Sync(int currentPosition)
        {
            startPosition = currentPosition;
        }

        public int GetCountdown(int currentPosition)
        {
            return startPosition + totalFrames - currentPosition;
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
