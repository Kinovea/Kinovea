#region License
/*
Copyright © Joan Charmant 2010.
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
    /// TrackPointBlock is the representation of a point tracked from Block Matching.
    /// This class must be used in conjonction with TrackerBlock.
    /// </summary>
    public class TrackPointBlock : AbstractTrackPoint
    {
        public Bitmap Template;
        public bool IsReferenceBlock;
        public double Similarity;
        public int TemplateAge;
        
        public TrackPointBlock(int _x, int _y, long _t)
            : this(_x, _y, _t, null)
        {
        }
        public TrackPointBlock(int _x, int _y, long _t, Bitmap _img)
        {
            X = _x;
            Y = _y;
            T = _t;
            Template = _img;
            TemplateAge = 0;
        }
        
        public override void ResetTrackData()
        {
            IsReferenceBlock = false;
            Similarity = 1.0;
            TemplateAge = 0;
            
            if(Template != null)
            {
                Template.Dispose();
            }
            Template = null;
        }
    }
}
