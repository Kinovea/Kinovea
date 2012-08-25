#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
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
    /// A single frame inside the tracking timeline.
    /// </summary>
    public class TrackFrame
    {
        public long Time
        {
            get { return time; }
        }
        
        public Point Location
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

        private long time;
        private Point location;
        private Bitmap template;
        private PositionningSource positionningSource;
                
        public TrackFrame(long time, Point location, Bitmap template, PositionningSource positionningSource)
        {
            this.time = time;
            this.location = location;
            this.template = template;
            this.positionningSource = positionningSource;
        }
        
        
    }
}
