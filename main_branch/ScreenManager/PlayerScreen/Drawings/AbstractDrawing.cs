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

using Kinovea.Services;
using System;
using System.Drawing;
using System.Xml;

namespace Kinovea.ScreenManager
{
	/// <summary>
	///  An abstract layer for drawings.
	/// </summary>
    public abstract class AbstractDrawing
    {
        #region Properties
        public abstract DrawingToolType ToolType
        {
        	get;
        }
        public abstract InfosFading infosFading
        {
            get;
            set;
        }
        #endregion

        #region Methods
        
        // Display
        public abstract void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft);
        
        // User interaction
        public abstract int HitTest(Point _point, long _iCurrentTimestamp);
        public abstract void MoveHandleTo(Point point, int handleNumber);
        public abstract void MoveDrawing(int _deltaX, int _deltaY);
        
        // Export
        public abstract void ToXmlString(XmlTextWriter _xmlXriter);
        
        // Decoration
        public abstract void UpdateDecoration(Color _color);
        public abstract void UpdateDecoration(LineStyle _style);
        public abstract void UpdateDecoration(int _iFontSize);
        public abstract void MemorizeDecoration();
        public abstract void RecallDecoration();
        
        // And also:
        // ToString();
        // FromXml();
        // GetHashCode();
        #endregion
    }
}
