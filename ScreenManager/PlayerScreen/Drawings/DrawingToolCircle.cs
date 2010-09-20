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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class DrawingToolCircle : AbstractDrawingTool
    {
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return new DrawingCircle(_Origin.X, _Origin.Y, 25, _iTimestamp, _AverageTimeStampsPerFrame);
        }
        public override void OnMouseMove(Keyframe _Keyframe, Point _MouseCoordinates)
        {
            _Keyframe.Drawings[0].MoveHandleTo(_MouseCoordinates, 2);
        }
        public override DrawingToolType OnMouseUp()
        {
            //return DrawingToolType.Pointer;
            return DrawingToolType.Circle;
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            return Cursors.Cross;
        }
    }
}

