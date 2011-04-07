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
    public abstract class AbstractDrawingTool
    {
    	#region Properties
        /// <summary>
        /// The type of drawing that this tool generates.
        /// </summary>
    	public abstract DrawingType DrawingType
        {
            get;
        }
        #endregion
        
        // Return an object of the type of this tool.
        public abstract AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame);

        // Only used at first MouseUp after initial setup.
        public abstract DrawingToolType OnMouseUp();

        // Get this tool's cursor
        public abstract Cursor GetCursor(Color _color, int _iSize);
    }
}
