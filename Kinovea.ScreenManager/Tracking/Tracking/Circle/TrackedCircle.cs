#region License
/*
Copyright © Joan Charmant 2024.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Wrapper for the circle to be matched in following frames.
    /// </summary>
    public class TrackedCircle
    {
        #region Properties

        /// <summary>
        /// The timestamp of the frame from which the circle was tracked.
        /// </summary>
        public long Time
        {
            get { return time; }
        }

        /// <summary>
        /// The position of the center.
        /// </summary>
        public PointF Location
        {
            get { return location; }
        }

        /// <summary>
        /// Radius
        /// </summary>
        public float Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// The vote score of the circle. 
        /// The value is left to 0 for manually placed circles.
        /// </summary>
        public float Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// Whether the point was placed manually or automatically tracked.
        /// </summary>
        public TrackingSource PositionningSource
        {
            get { return positionningSource; }
        }

        /// <summary>
        /// Content hash.
        /// </summary>
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= time.GetHashCode();
                hash ^= location.GetHashCode();
                hash ^= radius.GetHashCode();
                hash ^= score.GetHashCode();
                hash ^= positionningSource.GetHashCode();
                return hash;
            }
        }
        #endregion

        private long time;
        private PointF location;
        private float radius;
        private float score;
        private Bitmap template;
        private TrackingSource positionningSource;
                
        public TrackedCircle(long time, PointF location, float radius, float score, TrackingSource positionningSource)
        {
            this.time = time;
            this.location = location;
            this.radius = radius;
            this.score = score;
            this.positionningSource = positionningSource;
        }
    }
}
