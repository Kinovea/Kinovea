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
    public class DrawingToolPencil : AbstractDrawingTool
    {
    	#region Properties
    	public override string InternalName
		{
			get { return "pencil"; }
		}
    	public override string DisplayName
    	{
    		get { return ScreenManagerLang.ToolTip_DrawingToolPencil; }
    	}
    	public override Bitmap Icon
    	{
    		get { return Properties.Drawings.pencil; }
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
    	#endregion
		
    	#region Constructor
		public DrawingToolPencil()
		{
			m_DefaultStylePreset.Elements.Add("color", new StyleElementColor(Color.SeaGreen));
			m_DefaultStylePreset.Elements.Add("pen size", new StyleElementPenSize(9));
			m_StylePreset = m_DefaultStylePreset.Clone();
		}
		#endregion
    	
    	#region Public Methods
    	public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
    	{
    		return new DrawingPencil(_Origin.X, _Origin.Y, _Origin.X + 1, _Origin.Y, _iTimestamp, _AverageTimeStampsPerFrame, m_StylePreset);
    	}
    	public override DrawingToolType OnMouseUp()
    	{
    		return DrawingToolType.Pencil;
    	}
    	public override Cursor GetCursor(double _fStretchFactor)
    	{
    		// Draw custom cursor: Colored and sized circle.
    		Color c = (Color)m_StylePreset.Elements["color"].Value;
    		int size = (int)(_fStretchFactor * (int)m_StylePreset.Elements["pen size"].Value);
    		Pen p = new Pen(c, 1);
    		Bitmap b = new Bitmap(size + 2, size + 2);
    		Graphics g = Graphics.FromImage(b);
    		g.DrawEllipse(p, 1, 1, size, size);
    		p.Dispose();
    		return new Cursor(b.GetHicon());
    	}
    	#endregion
    }
}