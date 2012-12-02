#region License
/*
Copyright © Joan Charmant 2011.
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
	/// <summary>
	/// A 3D grid.
	/// </summary>
	public class DrawingToolPlane : AbstractDrawingTool
	{
		#region Properties
		public override string DisplayName
		{
			get { return ScreenManagerLang.mnuPerspectiveGrid; }
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.plane; }
		}
		public override bool Attached
		{
			get { return true; }
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
			get { return m_StylePreset;}
			set { m_StylePreset = value;}
		}
		public override DrawingStyle DefaultStylePreset
		{
			get { return m_DefaultStylePreset;}
		}
		#endregion
    	
		#region Private Methods
    	private DrawingStyle m_DefaultStylePreset = new DrawingStyle();
    	private DrawingStyle m_StylePreset;
    	#endregion
    	
		public DrawingToolPlane()
		{
			m_DefaultStylePreset.Elements.Add("color", new StyleElementColor(Color.CornflowerBlue));
    		m_DefaultStylePreset.Elements.Add("divisions", new StyleElementGridDivisions(8));
			m_StylePreset = m_DefaultStylePreset.Clone();
		}
		
		#region Public Methods
    	public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
    	{
    	    return new DrawingPlane(8, true, _iTimestamp, _AverageTimeStampsPerFrame, m_StylePreset);
    	}
    	public override Cursor GetCursor(double _fStretchFactor)
    	{
    		return Cursors.Cross;
    	}
    	#endregion
	}
}
