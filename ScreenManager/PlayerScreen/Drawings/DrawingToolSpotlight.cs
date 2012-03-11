#region license
/*
Copyright © Joan Charmant 2012.
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
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolSpotlight : AbstractDrawingTool
    {
    	#region Properties
    	public override string DisplayName
    	{
    		//get { return ScreenManagerLang.ToolTip_DrawingToolLine2D; }
    		get { return "Spotlight";}
    	}
    	public override Bitmap Icon
    	{
    		get { return Properties.Drawings.spotlight; }
    	}
    	public override bool Attached
    	{
    		get { return false; }
    	}
    	public override bool KeepTool
    	{
    		get { return false; }
    	}
    	public override bool KeepToolFrameChanged
    	{
    		get { return false; }
    	}
    	public override DrawingStyle StylePreset
		{
			get { return null;}
			set { return;}
		}
		public override DrawingStyle DefaultStylePreset
		{
			get { return null;}
		}
    	public static bool ShowMeasure;
    	#endregion
		
    	#region Members
    	//private SpotlightManager spotlightManager = new SpotlightManager();
    	#endregion
    	
    	
    	#region Constructor
    	public DrawingToolSpotlight()
    	{
    	}
    	#endregion
		
    	#region Public Methods
    	public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
    	{
    	    //spotlightManager.Add(_Origin, _iTimestamp, _AverageTimeStampsPerFrame);
    	    //return spotlightManager;
    	    //return new DrawingLine2D(_Origin, new Point(_Origin.X + 10, _Origin.Y), _iTimestamp, _AverageTimeStampsPerFrame, m_StylePreset);
    	    return null;
    	}
    	public override Cursor GetCursor(double _fStretchFactor)
    	{
    		return Cursors.Cross;
    	}
    	#endregion
    }
}


