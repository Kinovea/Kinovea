#region license
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

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolCoordinateSystem : AbstractDrawingTool
    {
        #region Properties
        public override string Name
        {
            get { return "CoordinateSystem"; }
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.mnuCoordinateSystem; }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.coordinates; }
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
            get { return styleElements;}
            set { styleElements = value;}
        }
        public override StyleElements DefaultStyleElements
        {
            get { return defaultStyleElements;}
        }
        #endregion
        
        #region Members
        private StyleElements defaultStyleElements = new StyleElements();
        private StyleElements styleElements = new StyleElements();
        #endregion
        
        #region Public Methods
        public DrawingToolCoordinateSystem()
        {
            defaultStyleElements.Elements.Add("line color", new StyleElementColor(Color.Red));
            styleElements = defaultStyleElements.Clone();
        }
        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
           return null;
        }
        #endregion
    }
}




