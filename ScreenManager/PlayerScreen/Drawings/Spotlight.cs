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
using System.Drawing.Drawing2D;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// SpotLight. (MultiDrawingItem of SpotlightManager)
	/// Describe and draw a single spotlight.
	/// </summary>
	public class Spotlight
	{
		#region Members
		private long m_iPosition;
		
		private Point m_Center;
		private int m_iRadius;
		private Rectangle m_RescaledRect;
		
		private static readonly int m_iMinimalRadius = 10;
		private static readonly int m_iBorderWidth = 2;
		private static readonly DashStyle m_DashStyle = DashStyle.Dash;
		private InfosFading m_InfosFading;
		#endregion
		
		#region Constructor
		public Spotlight(long _iPosition, long _iAverageTimeStampsPerFrame, Point _center)
		{
			m_iPosition = _iPosition;
			m_Center = _center;
			m_iRadius = m_iMinimalRadius;
			m_InfosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
			m_InfosFading.UseDefault = false;
			m_InfosFading.FadingFrames = 25;
			//m_InfosFading.AlwaysVisible = true;
		}
		#endregion
		
		#region Public methods
		public double AddSpot(long _timestamp, GraphicsPath _path, CoordinateSystem _transformer)
		{
			// Add the shape of this spotlight to the global mask for the frame.
			// The dim rectangle is added separately in Spotlights class.
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_timestamp);
			if(fOpacityFactor <= 0)
			    return 0;
			
			//RescaleCoordinates(_fStretchFactor, _DirectZoomTopLeft);
			Point center = _transformer.Transform(m_Center);
			int radius = _transformer.Transform(m_iRadius);
			m_RescaledRect = center.Box(radius);
			_path.AddEllipse(m_RescaledRect);
			
			// Return the opacity factor at this spot so the spotlights manager is able to compute the global dim value.
			return fOpacityFactor;
		}
		public void Draw(Graphics _canvas, CoordinateSystem _transformer, long _timestamp)
        {
			// This just draws the border.
			// Note: the coordinate system hasn't moved since AddSpot, but we recompute it anyway...
			// This might be a good case where we should keep a global.
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_timestamp);
			if(fOpacityFactor <= 0)
			    return;
		
			Color colorPenBorder = Color.FromArgb((int)((double)255 * fOpacityFactor), Color.White);
			using(Pen penBorder = new Pen(colorPenBorder, m_iBorderWidth))
			{
    			penBorder.DashStyle = m_DashStyle;
    			_canvas.DrawEllipse(penBorder, m_RescaledRect);
			}
		}
		public int HitTest(Point _point, long _iCurrentTimeStamp)
		{
			// Note: Coordinates are already descaled.
            // Hit Result: -1: miss, 0: on object, 1 on handle.
			int iHitResult = -1;
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimeStamp);
			if(fOpacityFactor > 0)
			{
				if(IsPointOnHandler(_point))
                    iHitResult = 1;
				else if (IsPointInObject(_point))
                    iHitResult = 0;
			}
			return iHitResult;
		}
		public void MouseMove(int _deltaX, int _deltaY)
		{
			m_Center.X += _deltaX;
            m_Center.Y += _deltaY;
		}
		public void MoveHandleTo(Point point)
        {
            // Point coordinates are descaled.
            // User is dragging the outline of the circle, figure out the new radius at this point.
            int shiftX = Math.Abs(point.X - m_Center.X);
            int shiftY = Math.Abs(point.Y - m_Center.Y);
            m_iRadius = (int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY));
            
            if(m_iRadius < m_iMinimalRadius) m_iRadius = m_iMinimalRadius;
        }
		#endregion
		
		#region Private methods
		private bool IsPointInObject(Point _point)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddEllipse(m_Center.X - m_iRadius, m_Center.Y - m_iRadius, m_iRadius*2, m_iRadius*2);
                using(Region areaRegion = new Region(areaPath))
                {
                    hit = areaRegion.IsVisible(_point);
                }
            }
            return hit;
        }
		private bool IsPointOnHandler(Point _point)
        {
            bool hit = false;
		    using(GraphicsPath areaPath = new GraphicsPath())
			{
                areaPath.AddArc(m_Center.X - m_iRadius, m_Center.Y - m_iRadius, m_iRadius*2, m_iRadius*2, 0, 360);
                using(Pen areaPen = new Pen(Color.Black, 10))
                {
                    areaPath.Widen(areaPen);
                }
                using(Region r = new Region(areaPath))
                {
                    hit = r.IsVisible(_point);
                }
		    }
		    return hit;
        }
		#endregion
	}
}

