#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Wrapper for the template to be matched in following frames.
    /// The template matching tracker keeps a timeline of these live during 
    /// a tracking session. This is not saved to KVA it is reconstructed 
    /// dynamically on the fly.
    /// </summary>
    public class TrackingTemplate : IDisposable
    {
        #region Properties

        /// <summary>
        /// The timestamp of the frame from which the template was extracted.
        /// </summary>
        public long Time
        {
            get { return time; }
        }

        /// <summary>
        /// The position of the tracked point (center of the template).
        /// </summary>
        public PointF Location
        {
            get { return location; }
        }

        /// <summary>
        /// Template bitmap.
        /// </summary>
        public Bitmap Template
        {
            get { return template; }
        }

        /// <summary>
        /// The similarity score of the template with regards to the 
        /// previous reference template.
        /// This is 1.0 if the template results from manual placement of the point.
        /// It is less or equal to 1.0 if the template was automatically tracked.
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
                hash ^= score.GetHashCode();
                hash ^= positionningSource.GetHashCode();
                return hash;
            }
        }
        #endregion

        private long time;
        private PointF location;
        private float score;
        private Bitmap template;
        private TrackingSource positionningSource;
                
        public TrackingTemplate(long time, PointF location, float score, Bitmap template, TrackingSource positionningSource)
        {
            this.time = time;
            this.location = location;
            this.score = score;
            this.template = template;
            this.positionningSource = positionningSource;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TrackingTemplate()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (template != null)
                    template.Dispose();
            }
        }
    }
}
