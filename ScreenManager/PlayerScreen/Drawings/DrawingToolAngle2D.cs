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

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolAngle2D : AbstractDrawingTool
    {
    	
    	#region Properties
    	public override string InternalName
		{
			get { return "angle"; }
		}
		public override string DisplayName
		{
			get { return ScreenManagerLang.ToolTip_DrawingToolAngle2D; }
		}
		public override Bitmap Icon
		{
			get { return Properties.Drawings.angle; }
		}
    	public override DrawingType DrawingType
    	{
    		get { return DrawingType.Angle; }
    	}
    	public override bool Attached
    	{
    		get { return true; }
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
    	
    	#region Members
    	private DrawingStyle m_DefaultStylePreset = new DrawingStyle();
    	private DrawingStyle m_StylePreset;
    	private DelegateScreenInvalidate m_invalidate;
    	#endregion
		
		#region Constructor
		public DrawingToolAngle2D()
		{
			m_DefaultStylePreset.Elements.Add("back color", new StyleElementColor(Color.DarkOliveGreen));
			m_StylePreset = m_DefaultStylePreset.Clone();
		}
		public DrawingToolAngle2D(DelegateScreenInvalidate _invalidate)
		{
			m_invalidate = _invalidate;
		}
		#endregion
		
		#region Public Methods
		public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
		{
			return new DrawingAngle2D(_Origin.X, _Origin.Y, _Origin.X + 50, _Origin.Y, _Origin.X, _Origin.Y - 50, 
			                          _iTimestamp, _AverageTimeStampsPerFrame, m_invalidate, m_StylePreset);
		}
		public override DrawingToolType OnMouseUp()
		{
			return DrawingToolType.Pointer; // Instantly fall back to Pointer Tool after setup.
		}
		public override Cursor GetCursor(Color _color, int _iSize)
		{
			return Cursors.Cross;
		}
		#endregion
    }
}
