#region License
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
using System.Drawing.Drawing2D;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AutoNumber. (MultiDrawingItem of AutoNumberManager)
	/// Describe and draw a single autonumber.
	/// </summary>
	public class AutoNumber
	{
	    #region Properties
	    public int Value {
	        get { return m_Value;}
	    }
	    #endregion

		#region Members
		private long m_iPosition;
		private Point m_Center;
		private int m_iRadius;
		//private Rectangle m_RescaledRect;
		private static readonly int m_iMinimalRadius = 12;
		private static readonly int m_iDefaultRadius = 20;
		private InfosFading m_InfosFading;
		private int m_Value = 1;
		#endregion
		
		#region Constructor
		public AutoNumber(long _iPosition, long _iAverageTimeStampsPerFrame, Point _center, int _value)
		{
			m_iPosition = _iPosition;
			m_Center = _center;
			m_iRadius = m_iDefaultRadius;
			m_InfosFading = new InfosFading(_iPosition, _iAverageTimeStampsPerFrame);
			m_InfosFading.UseDefault = false;
			m_InfosFading.FadingFrames = 25;
			m_Value = _value;
		}
		#endregion
		
		#region Public methods
		public void Draw(Graphics _canvas, CoordinateSystem _transformer, long _timestamp)
        {
			// This just draws the border.
			// Note: the coordinate system hasn't moved since AddSpot, but we recompute it anyway...
			// This might be a good case where we should keep a global.
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_timestamp);
			if(fOpacityFactor <= 0)
			    return;
		
			Point center = _transformer.Transform(m_Center);
			int radius = Math.Max(_transformer.Transform(m_iRadius), m_iMinimalRadius);
			Rectangle rect = center.Box(radius);
			
			Color backColor = Color.FromArgb((int)((double)255 * fOpacityFactor), Color.Black);
			using(SolidBrush b = new SolidBrush(backColor))
			using(Font f = new Font("Arial", 16, FontStyle.Bold))
			{
    			_canvas.FillEllipse(b, rect);
    			_canvas.DrawString(m_Value.ToString(), f, (SolidBrush)Brushes.White, rect.Left + 2, rect.Top + 2);
			}
		}
		public int HitTest(Point _point, long _iCurrentTimeStamp)
		{
			// Note: Coordinates are already descaled.
            // Hit Result: -1: miss, 0: on object, 1 on handle.
			int iHitResult = -1;
			double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimeStamp);
			if(fOpacityFactor > 0 && IsPointInObject(_point))
			{
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
		public bool IsVisible(long _timestamp)
		{
			return m_InfosFading.GetOpacityFactor(_timestamp) > 0;
		}
		#endregion
		
		#region Private methods
		private bool IsPointInObject(Point _point)
        {
            // Point coordinates are descaled.
            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddEllipse(m_Center.X - m_iRadius, m_Center.Y - m_iRadius, m_iRadius*2, m_iRadius*2);
            Region areaRegion = new Region(areaPath);
            return areaRegion.IsVisible(_point);
        }
		#endregion
	}
}
