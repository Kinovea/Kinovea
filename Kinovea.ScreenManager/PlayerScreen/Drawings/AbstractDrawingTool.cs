/*
Copyright � Joan Charmant 2008.
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
    	/// Return true if this tool creates drawings attached to a particular key image.
    	/// </summary>
    	public abstract bool Attached
    	{
    		get;
    	}
    	
    	/// <summary>
    	/// Whether we should stay on the same tool after the drawing has been added.
    	/// </summary>
    	public abstract bool KeepTool
    	{
    		get;
    	}
    	
    	/// <summary>
    	/// Whether we should stay on the same tool when the video moves forward.
    	/// </summary>
    	public abstract bool KeepToolFrameChanged
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
    	
    	#region Public Interface
    	/// <summary>
    	/// Generate a new artefact from this tool.
    	/// </summary>
    	/// <param name="_Origin">The image coordinates the drawing should be initialized with</param>
    	/// <param name="_iTimestamp">The time position where the drawing is added</param>
    	/// <param name="_AverageTimeStampsPerFrame"></param>
    	/// <returns>A new drawing object</returns>
    	public abstract AbstractDrawing GetNewDrawing(Point origin, long timestamp, long averageTimeStampsPerFrame);

    	/// <summary>
    	/// Retrieve the cursor we should dispaly when this tool is the active tool.
    	/// </summary>
    	/// <param name="_fStretchFactor">Current stretchfactor (including zoom)</param>
    	/// <returns>The cursor to use</returns>
    	public abstract Cursor GetCursor(double stretchFactor);
    	#endregion
    	
    	#region Public Concrete Methods
    	public void ResetToDefaultStyle()
    	{
    		StylePreset = DefaultStylePreset.Clone();
    	}
    	public override string ToString()
    	{
    		return DisplayName;
    	}
    	#endregion
    }
}
