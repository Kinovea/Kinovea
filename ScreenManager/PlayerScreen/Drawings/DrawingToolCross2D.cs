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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Videa.ScreenManager
{
    public class DrawingToolCross2D : AbstractDrawingTool
    {
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return new DrawingCross2D(_Origin.X, _Origin.Y, _iTimestamp, _AverageTimeStampsPerFrame);
        }

        public override void OnMouseMove(Keyframe _Keyframe, Point _MouseCoordinates)
        {
        }
        public override DrawingToolType OnMouseUp()
        {
            return DrawingToolType.Cross2D; // After placed, we keep using the Cross2D tool.
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            // Draw custom cursor: cross inside a semi transparent circle (same as drawing).
            Pen p = new Pen(_color, 1);
            Bitmap b = new Bitmap(9, 9);
            Graphics g = Graphics.FromImage(b);

            // Center point is {4,4}
            g.DrawLine(p, 1, 4, 7, 4);
            g.DrawLine(p, 4, 1, 4, 7);
            g.FillEllipse(new SolidBrush(Color.FromArgb(32, _color)), 0, 0, 8, 8);

            return new Cursor(b.GetHicon());
        }
    }
}
