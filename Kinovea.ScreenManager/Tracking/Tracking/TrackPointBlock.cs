#region License
/*
Copyright © Joan Charmant 2010.
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
    /// TrackPointBlock is the representation of a point tracked from Block Matching.
    /// This class must be used in conjonction with TrackerBlock.
    /// </summary>
    public class TrackPointBlock : AbstractTrackPoint
    {

        public Bitmap Template { get; set; }

        public bool IsReferenceBlock;
        public double Similarity;
        public int TemplateAge;
        
        public TrackPointBlock(float x, float y, long t)
            : this(x, y, t, null)
        {
        }
        public TrackPointBlock(float x, float y, long t, Bitmap img)
        {
            this.X = x;
            this.Y = y;
            this.T = t;
            Template = img;
            TemplateAge = 0;
        }
        
        public override void ResetTrackData()
        {
            IsReferenceBlock = false;
            Similarity = 1.0;
            TemplateAge = 0;

            if (Template != null)
                Template.Dispose();

            Template = null;
        }
    }
}
