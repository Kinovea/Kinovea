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
    public class DrawingToolCross2D : AbstractDrawingTool
    {
        #region Properties
        public override string DisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolCross2D; }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.crossmark; }
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
            get { return m_StylePreset;}
            set { m_StylePreset = value;}
        }
        public override DrawingStyle DefaultStylePreset
        {
            get { return m_DefaultStylePreset;}
        }
        #endregion
        
        #region Private Methods
        private DrawingStyle m_DefaultStylePreset = new DrawingStyle();
        private DrawingStyle m_StylePreset;
        #endregion
        
        #region Constructor
        public DrawingToolCross2D()
        {
            m_DefaultStylePreset.Elements.Add("back color", new StyleElementColor(Color.CornflowerBlue));
            m_StylePreset = m_DefaultStylePreset.Clone();
        }
        #endregion
        
        #region Public Methods
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return new DrawingCross2D(_Origin, _iTimestamp, _AverageTimeStampsPerFrame, m_StylePreset, transformer);
        }
        public override Cursor GetCursor(double _fStretchFactor)
        {
            // Draw custom cursor: cross inside a semi transparent circle (same as drawing).
            Color c = (Color)m_StylePreset.Elements["back color"].Value;
            Pen p = new Pen(c, 1);
            Bitmap b = new Bitmap(9, 9);
            Graphics g = Graphics.FromImage(b);

            // Center point is {4,4}
            g.DrawLine(p, 1, 4, 7, 4);
            g.DrawLine(p, 4, 1, 4, 7);
            
            SolidBrush tempBrush = new SolidBrush(Color.FromArgb(32, c));
            g.FillEllipse(tempBrush, 0, 0, 8, 8);
            tempBrush.Dispose();
            p.Dispose();
            
            return new Cursor(b.GetHicon());
        }
        #endregion
    }
}
