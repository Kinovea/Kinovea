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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// LabelBackground draws a rounded rectangle at the right scale.
	/// All coordinates here are already rescaled.
	/// The rectangle size is computed in the caller, from the string length.
	/// </summary>
	public class LabelBackground
	{
		#region Properties
		public Point Location
		{
			get { return m_Location; }
			set { m_Location = value; }
		}
		public Point TextLocation
		{
			get { return new Point(m_Location.X + (m_iMarginWidth/2), m_Location.Y + (m_iMarginHeight/2)); }
			//get { return new Point(m_Location.X + 0, m_Location.Y + 0); }
		}		
		public int MarginWidth
		{
			get { return m_iMarginWidth; }
		}
		public int MarginHeight
		{
			get { return m_iMarginHeight; }
		}
		#endregion
		
		#region Members
		private Point m_Location;
		private bool m_bDrop;
		private static readonly int m_iDefaultBackgroundAlpha = 128;
		private int m_iMarginWidth = 12;//11;
		private int m_iMarginHeight = 6;//7;
		#endregion
		
		#region Construction
		public LabelBackground() 
		{
			m_Location = new Point(0,0);
		}
		public LabelBackground(Point _location, bool _bDrop, int _iMarginWidth, int _iMarginHeight) 
		{
			// This constructor is currently only used for the mini label on top of chronometers.
			m_Location = _location;
			m_bDrop = _bDrop;
			m_iMarginWidth = _iMarginWidth;
			m_iMarginHeight = _iMarginHeight;
		}
		#endregion
		
		public void Draw(Graphics _canvas, double _fOpacityFactor, int _radius, int _width, int _height, Color _color)
		{
			// The rectangle size is computed in the caller, depending the string length.
			// Margins ?
				
            int diameter = _radius * 2;

            Rectangle RescaledBackground = new Rectangle(m_Location.X, m_Location.Y, _width + m_iMarginWidth, _height + m_iMarginHeight);
            
            //_canvas.DrawRectangle(Pens.Red, RescaledBackground);
            
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            if(m_bDrop)
            {
            	gp.AddLine(RescaledBackground.X, RescaledBackground.Y, RescaledBackground.X + RescaledBackground.Width - diameter, RescaledBackground.Y);

	            gp.AddArc(RescaledBackground.X + RescaledBackground.Width - diameter, RescaledBackground.Y, diameter, diameter, 270, 90);
	            gp.AddLine(RescaledBackground.X + RescaledBackground.Width, RescaledBackground.Y + _radius, RescaledBackground.X + RescaledBackground.Width, RescaledBackground.Y + RescaledBackground.Height);
	
	            gp.AddLine(RescaledBackground.X + RescaledBackground.Width, RescaledBackground.Y + RescaledBackground.Height, RescaledBackground.X + _radius, RescaledBackground.Y + RescaledBackground.Height);
	
	            gp.AddArc(RescaledBackground.X, RescaledBackground.Y + RescaledBackground.Height - diameter, diameter, diameter, 90, 90);
	            gp.AddLine(RescaledBackground.X, RescaledBackground.Y + RescaledBackground.Height - _radius, RescaledBackground.X, RescaledBackground.Y);
            }
            else
            {
            	gp.AddArc(RescaledBackground.X, RescaledBackground.Y, diameter, diameter, 180, 90);
	            gp.AddLine(RescaledBackground.X + _radius, RescaledBackground.Y, RescaledBackground.X + RescaledBackground.Width - diameter, RescaledBackground.Y);
	
	            gp.AddArc(RescaledBackground.X + RescaledBackground.Width - diameter, RescaledBackground.Y, diameter, diameter, 270, 90);
	            gp.AddLine(RescaledBackground.X + RescaledBackground.Width, RescaledBackground.Y + _radius, RescaledBackground.X + RescaledBackground.Width, RescaledBackground.Y + RescaledBackground.Height - diameter);
	
	            gp.AddArc(RescaledBackground.X + RescaledBackground.Width - diameter, RescaledBackground.Y + RescaledBackground.Height - diameter, diameter, diameter, 0, 90);
	            gp.AddLine(RescaledBackground.X + RescaledBackground.Width - _radius, RescaledBackground.Y + RescaledBackground.Height, RescaledBackground.X + _radius, RescaledBackground.Y + RescaledBackground.Height);
	
	            gp.AddArc(RescaledBackground.X, RescaledBackground.Y + RescaledBackground.Height - diameter, diameter, diameter, 90, 90);
	            gp.AddLine(RescaledBackground.X, RescaledBackground.Y + RescaledBackground.Height - _radius, RescaledBackground.X, RescaledBackground.Y + _radius);
            }
            
            gp.CloseFigure();

            int BackgroundAlpha = (int)((double)m_iDefaultBackgroundAlpha * _fOpacityFactor);
            _canvas.FillPath(new SolidBrush(Color.FromArgb(BackgroundAlpha, _color)), gp);
		}
	}
}
