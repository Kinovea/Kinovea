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
    public class DrawingToolAngle2D : AbstractDrawingTool
    {
    	public override DrawingType DrawingType
        {
        	get { return DrawingType.Angle; }
        }
		public override bool Attached
        {
        	get { return true; }
        }
        
		private DelegateScreenInvalidate m_invalidate;
		
		public DrawingToolAngle2D(DelegateScreenInvalidate _invalidate)
		{
			m_invalidate = _invalidate;	
		}
		
		public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return new DrawingAngle2D(_Origin.X, _Origin.Y, _Origin.X + 50, _Origin.Y, _Origin.X, _Origin.Y - 50, _iTimestamp, _AverageTimeStampsPerFrame, m_invalidate);
        }
        public override DrawingToolType OnMouseUp()
        {
            return DrawingToolType.Pointer; // Instantly fall back to Pointer Tool after setup.
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            return Cursors.Cross;
        }
    }
}
