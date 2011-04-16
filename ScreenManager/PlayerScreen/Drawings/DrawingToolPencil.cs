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

namespace Kinovea.ScreenManager
{
    public class DrawingToolPencil : AbstractDrawingTool
    {
    	public override DrawingType DrawingType
        {
        	get { return DrawingType.Pencil; }
        }
		public override bool Attached
        {
        	get { return true; }
        }
		
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return new DrawingPencil(_Origin.X, _Origin.Y, _Origin.X + 1, _Origin.Y, _iTimestamp, _AverageTimeStampsPerFrame);
        }
        public override DrawingToolType OnMouseUp()
        {
            return DrawingToolType.Pencil;
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            // Draw custom cursor: Colored and sized circle.
            Pen p = new Pen(_color, 1);
            Bitmap b = new Bitmap(_iSize + 2, _iSize + 2);
            Graphics g = Graphics.FromImage(b);
            g.DrawEllipse(p, 1, 1, _iSize, _iSize);
            p.Dispose();
            return new Cursor(b.GetHicon());
        }
    }
}