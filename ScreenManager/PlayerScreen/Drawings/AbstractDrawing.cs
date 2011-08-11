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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Describes a generic drawing.
    /// All drawings must implement rendering and manipulation methods.
	/// </summary>
    public abstract class AbstractDrawing
    {
    	
    	#region Properties
    	/// <summary>
    	/// Gets or set the fading object for this drawing. 
    	/// This is used in opacity calculation for Persistence.
    	/// </summary>
        public abstract InfosFading infosFading
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get the capabilities of this drawing for the generic part of context menu.
        /// </summary>
        public abstract DrawingCapabilities Caps
        {
        	get;
        }
        
        /// <summary>
    	/// Gets the list of extra context menu specific to this drawing.
    	/// </summary>
        public abstract List<ToolStripMenuItem> ContextMenu
        {
            get;
        }
        #endregion
    	
        #region Abstract Methods
        /// <summary>
        /// Draw this drawing on the provided canvas.
        /// </summary>
        /// <param name="_canvas">The GDI+ surface on which to draw</param>
        /// <param name="_transformer">A helper object providing coordinate systems transformation</param>
        /// <param name="_fStretchFactor">The scaling factor between the canvas and the original image size</param>
        /// <param name="_bSelected">Whether the drawing is currently selected</param>
        /// <param name="_iCurrentTimestamp">The current time position in the video</param>
        /// <param name="_DirectZoomTopLeft">The position of the zoom window relatively to the top left corner of the original image</param>
        public abstract void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft);
        
        /// <summary>
        /// Evaluates if a particular point is inside the drawing, on a handler, or completely outside the drawing.
        /// </summary>
        /// <param name="_point">The coordinates at original image scale of the point to evaluate</param>
        /// <param name="_iCurrentTimestamp">The current time position in the video</param>
        /// <returns>-1 : missed. 0 : The drawing as a whole has been hit. n (with n>0) : The id of a manipulation handle that has been hit</returns>
        public abstract int HitTest(Point _point, long _iCurrentTimestamp);
        
        /// <summary>
        /// Move the specified handle to its new location.
        /// </summary>
        /// <param name="point">The new location of the handle, in original image scale coordinates</param>
        /// <param name="handleNumber">The handle identifier</param>
        public abstract void MoveHandle(Point point, int handleNumber);
        
        /// <summary>
        /// Move the drawing as a whole.
        /// </summary>
        /// <param name="_deltaX">Change in x coordinates</param>
        /// <param name="_deltaY">Change in y coordinates</param>
        /// <param name="_ModifierKeys">Modifiers key pressed while moving the drawing</param>
        public abstract void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys);
        #endregion

        #region Concrete methods
        public Rectangle HandleBox(Point _point, int _widen)
        {
            return new Rectangle(_point.X - _widen, _point.Y - _widen, _widen * 2, _widen * 2);
        }
        public static void CallInvalidateFromMenu(object sender)
        {
            // The screen invalidate hook was injected inside menus during popMenu attach.
            // This avoids having an injection hanging in DrawingTool.
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi != null)
            {
                Action screenInvalidate = tsmi.Tag as Action;
                if (screenInvalidate != null) screenInvalidate();
            }
        }
        #endregion
    }

    /// <summary>
	/// The various capabilities of a drawing, used to support dynamically adding generic menus.
	/// </summary>
	[Flags]
	public enum DrawingCapabilities
	{
		None = 0,
	    ConfigureColor = 1,
	    ConfigureColorSize = 2,
	    Fading = 4,
	    Opacity = 8
	}
}
