/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolPencil : AbstractDrawingTool
    {
        #region Properties
        public override string DisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolPencil; }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.pencil; }
        }
        public override bool Attached
        {
            get { return true; }
        }
        public override bool KeepTool
        {
            get { return true; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return true; }
        }
        public override DrawingStyle StylePreset
        {
            get { return stylePreset;}
            set { stylePreset = value;}
        }
        public override DrawingStyle DefaultStylePreset
        {
            get { return defaultStylePreset;}
        }
        #endregion
        
        #region Members
        private DrawingStyle defaultStylePreset = new DrawingStyle();
        private DrawingStyle stylePreset;
        #endregion
        
        #region Constructor
        public DrawingToolPencil()
        {
            defaultStylePreset.Elements.Add("color", new StyleElementColor(Color.SeaGreen));
            defaultStylePreset.Elements.Add("pen size", new StyleElementPenSize(9));
            stylePreset = defaultStylePreset.Clone();
        }
        #endregion
        
        #region Public Methods
        public override AbstractDrawing GetNewDrawing(Point origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return new DrawingPencil(origin, new Point(origin.X + 1, origin.Y), timestamp, averageTimeStampsPerFrame, stylePreset);
        }
        public override Cursor GetCursor(double stretchFactor)
        {
            // Draw custom cursor: Colored and sized circle.
            Color c = (Color)stylePreset.Elements["color"].Value;
            int size = (int)(stretchFactor * (int)stylePreset.Elements["pen size"].Value);
            Pen p = new Pen(c, 1);
            Bitmap b = new Bitmap(size + 2, size + 2);
            Graphics g = Graphics.FromImage(b);
            g.DrawEllipse(p, 1, 1, size, size);
            p.Dispose();
            return new Cursor(b.GetHicon());
        }
        #endregion
    }
}