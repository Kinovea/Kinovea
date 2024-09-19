#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolMagnifier : AbstractDrawingTool
    {
        #region Properties
        public override string Name
        {
            get { return "Magnifier"; }
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.ToolTip_Magnifier; }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.magnifier; }
        }
        public override bool Attached
        {
            get { return false; }
        }
        public override bool KeepTool
        {
            get { return false; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return false; }
        }
        public override StyleElements StyleElements
        {
            get { return null;}
            set { return;}
        }
        public override StyleElements DefaultStyleElements
        {
            get { return null;}
        }
        #endregion
        
        #region Public Methods
        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return null;
        }
        #endregion
    }
}
