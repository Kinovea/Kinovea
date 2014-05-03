#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Drawing.Imaging;
using System.Resources;

using AForge.Imaging.Filters;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class VideoFilterEdgesOnly : AdjustmentFilter
    {
        #region Properties
        public override string Name {
            get { return ScreenManagerLang.VideoFilterEdgesOnly_FriendlyName; }
        }
        public override Bitmap Icon {
            get { return null; }
        }	
        public override bool Experimental {
            get { return true; }
        }
        public override ImageProcessor ImageProcessor {
            get { return ProcessSingleImage; }
        }
        #endregion
        
        private DifferenceEdgeDetector filter = new DifferenceEdgeDetector();
        
        private void ProcessSingleImage(Bitmap source)
        {
            using(Bitmap gray = Grayscale.CommonAlgorithms.BT709.Apply(source))
            using(Bitmap tmp = filter.Apply(gray))
            {
                // Paint the result on the source to emulate ApplyInPlace.
                Graphics g = Graphics.FromImage(source);
                g.DrawImage(tmp, 0, 0);
            }
        }
    }
}
