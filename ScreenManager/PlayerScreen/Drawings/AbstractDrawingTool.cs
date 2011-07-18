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
    	/// Name of the tool. MUST BE UNIQUE as it's used to match the tool when importing style presets.
    	/// </summary>
    	public abstract string InternalName
    	{
    		get;
    	}
    	
    	/// <summary>
        /// The display name of the tool. Used for tooltips and in tool presets config.
        /// </summary>
    	public abstract string DisplayName
    	{
    		get;
    	}
    	
    	/// <summary>
        /// The image representing the tool. Used on buttons.
        /// </summary>
    	public abstract Bitmap Icon
    	{
    		get;
    	}
    	
    	/// <summary>
        /// The type of drawing that this tool generates.
        /// TODO: remove as part of refactoring ?
        /// </summary>
    	public abstract DrawingType DrawingType
        {
            get;
        }
    	/// <summary>
    	/// Return true if this tool creates drawings attached to a particular key image.
    	/// </summary>
    	public abstract bool Attached
    	{
    		get;
    	}
    	
    	/// <summary>
    	/// Current style preset
    	/// </summary>
    	public abstract DrawingStyle StylePreset
    	{
    		get;
    		set;
    	}
    	/// <summary>
    	/// Default style preset
    	/// </summary>
    	public abstract DrawingStyle DefaultStylePreset
    	{
    		get;
    	}
    	#endregion
    		
    	#region Public Methods
    	/// <summary>
    	/// Generate a new artefact from this tool.
    	/// </summary>
    	/// <param name="_Origin">The image coordinates the drawing should be initialized with</param>
    	/// <param name="_iTimestamp">The time position where the drawing is added</param>
    	/// <param name="_AverageTimeStampsPerFrame"></param>
    	/// <returns>A new drawing object</returns>
    	public abstract AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame);

    	/// <summary>
    	/// Called upon release of the second point of initial setup of the tool.
    	/// </summary>
    	/// <returns>The tool that we should fall back to after the operation. Typically same tool or pointer tool.</returns>
    	public abstract DrawingToolType OnMouseUp();

    	/// <summary>
    	/// Retrieve the cursor we should dispaly when this tool is the active tool.
    	/// </summary>
    	/// <param name="_color">The current color of the tool</param>
    	/// <param name="_iSize">The current size of the tool</param>
    	/// <returns>A cursor object to be used while this tool is active</returns>
    	public abstract Cursor GetCursor(Color _color, int _iSize);
    	
    	public void ResetToDefaultStyle()
    	{
    		StylePreset = DefaultStylePreset.Clone();
    	}
    	
    	#endregion
    }
}
