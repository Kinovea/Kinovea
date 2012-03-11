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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Spotlights.
	/// This is the proxy object dispatching all spotlights requests. (draw, hit testing, etc.)
	/// </summary>
	public class SpotlightManager : AbstractDrawing, IInitializable
	{
		#region Properties
		// Fading is not currently modifiable from outside.
        public override InfosFading  infosFading
        {
            get { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
            set { throw new NotImplementedException("Spotlight, The method or operation is not implemented."); }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.None; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
		#endregion
		
		#region Members
		private List<SpotLight> m_Spots = new List<SpotLight>();
		private int m_iSelected = -1;
		private static readonly Pen m_WidenPen = new Pen(Color.Black, 2);
		private static readonly int m_iDefaultBackgroundAlpha = 128;
		private static readonly SolidBrush m_BrushBackground = new SolidBrush(Color.FromArgb(m_iDefaultBackgroundAlpha, Color.Black));
		#endregion
		
		#region AbstractDrawing Implementation
		public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
		    // We draw a single translucent black rectangle to cover the whole image.
			// (Opacity varies between 0% and 50%, depending on the opacity factor of the closest spotlight in time)
			if(m_Spots.Count < 1)
			    return;
			
			// Create a mask rectangle and obliterate spotlights from it.
			// FIXME: spots subtract from each other which is not desirable.
			GraphicsPath globalPath = new GraphicsPath();
			globalPath.AddRectangle(_canvas.ClipBounds);
			
			// Combine all spots into a single GraphicsPath.
			// Get their opacity in the process to compute the global opacity of the covering rectangle.
			double maxOpacity = 0.0;
			GraphicsPath spotsPath = new GraphicsPath();
			foreach(SpotLight spot in m_Spots)
			{
				double opacity = spot.AddSpot(_iCurrentTimestamp, spotsPath, _transformer);
				maxOpacity = Math.Max(maxOpacity, opacity);
			}
			
			if(maxOpacity <= 0)
                return;
			
			// Obliterate the spots from the mask.
			globalPath.AddPath(spotsPath, false);
			
			// Draw the mask with the spot holes on top of the frame.
			int backgroundAlpha = (int)((double)m_iDefaultBackgroundAlpha * maxOpacity);
			using(SolidBrush brushBackground = new SolidBrush(Color.FromArgb(backgroundAlpha, Color.Black)))
            {
                _canvas.FillPath(brushBackground, globalPath);
            }
			
			// Draw each spot border or any visuals.
            foreach(SpotLight spot in m_Spots)
                spot.Draw(_canvas, _transformer, _iCurrentTimestamp);
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_Spots.Count)
				m_Spots[m_iSelected].MouseMove(_deltaX, _deltaY);
		}
		public override void MoveHandle(Point point, int handleNumber)
		{
		    if(m_iSelected >= 0 && m_iSelected < m_Spots.Count)
				m_Spots[m_iSelected].MoveHandleTo(point);
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
		    int currentSpot = 0;
		    int handle = -1;
		    foreach(SpotLight spot in m_Spots)
		    {
		        handle = spot.HitTest(_point, _iCurrentTimestamp);
		        if(handle >= 0)
		        {
		            m_iSelected = currentSpot;
		            break;
		        }
		        currentSpot++;
		    }
		    
		    return handle;
		}
		#endregion
		
		#region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			MoveHandle(point, -1);
		}
        #endregion
        
		#region Public methods
		public void Add(Point _point, long _iPosition, long _iAverageTimeStampsPerFrame)
		{
		    // Equivalent to GetNewDrawing() for regular drawing tools.
		    
			m_Spots.Add(new SpotLight(_iPosition, _iAverageTimeStampsPerFrame, _point));
			m_iSelected = m_Spots.Count - 1;
		}
		#endregion
	}
}

