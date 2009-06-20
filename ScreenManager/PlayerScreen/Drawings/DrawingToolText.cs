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

namespace Kinovea.ScreenManager
{
    public class DrawingToolText : AbstractDrawingTool
    {
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return new DrawingText(_Origin.X - 8, _Origin.Y - 8, 100, 25, _iTimestamp, _AverageTimeStampsPerFrame);
        }
        public override void OnMouseMove(Keyframe _Keyframe, Point _MouseCoordinates)
        {
            // Nothing to do...
        }
        public override DrawingToolType OnMouseUp()
        {
            return DrawingToolType.Pointer;
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            return Cursors.IBeam;
        }
    }
}