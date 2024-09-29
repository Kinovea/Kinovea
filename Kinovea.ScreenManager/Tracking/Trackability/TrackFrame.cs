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
    /// A single frame inside the tracking timeline.
    /// </summary>
    public class TrackFrame
    {
        #region Properties
        public long Time
        {
            get { return time; }
        }

        public PointF Location
        {
            get { return location; }
        }

        public Bitmap Template
        {
            get { return template; }
        }

        public PositionningSource PositionningSource
        {
            get { return positionningSource; }
        }
        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= time.GetHashCode();
                hash ^= location.GetHashCode();
                hash ^= positionningSource.GetHashCode();
                return hash;
            }
        }
        #endregion

        private long time;
        private PointF location;
        private Bitmap template;
        private PositionningSource positionningSource;
                
        public TrackFrame(long time, PointF location, Bitmap template, PositionningSource positionningSource)
        {
            this.time = time;
            this.location = location;
            this.template = template;
            this.positionningSource = positionningSource;
        }

        public TrackFrame(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            bool isEmpty = r.IsEmptyElement;

            long time = 0;
            PositionningSource source = PositionningSource.Manual;
            PointF location = PointF.Empty;

            if (r.MoveToAttribute("time"))
                time = timestampMapper(r.ReadContentAsLong());

            if (r.MoveToAttribute("source"))
                source = (PositionningSource)Enum.Parse(typeof(PositionningSource), r.ReadContentAsString());

            if (r.MoveToAttribute("location"))
            {
                location = XmlHelper.ParsePointF(r.ReadContentAsString());
                location = location.Scale(scale.X, scale.Y);
            }

            r.ReadStartElement();

            if (!isEmpty)
                r.ReadEndElement();

            this.time = time;
            this.location = location;
            this.positionningSource = source;
        }
        
    }
}
