#region License
/*
Copyright © Joan Charmant 2009.
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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Helper class to encapsulate calculations about the coordinate system for drawings,
	/// to compensate the differences between the original image and the displayed one.
	/// Note : This is not the coordinate system that the user can adjust for distance calculations.
	/// 
	/// Includes : 
	/// - scaling, image may be stretched or squeezed relative to the original.
	/// - zooming, the actual view may be a sub window of the original image.
	/// - rotating. (todo).
	/// - mirroring. (todo, currently handled elsewhere).
	/// 
	/// The class will keep track of the current changes in the coordinate system relatively to the 
	/// original image size and provide conversion routines.
	/// 
	/// All drawings coordinates are kept in the system of the original image.
	/// For actually drawing them on screen we ask the transformation. 
	/// 
	/// The image ratio is never altered. Skew is not supported.
	/// </summary>
	public class CoordinateSystem
	{
		#region Properties
		public double Stretch
		{
			get { return m_fStretch; }
			set { m_fStretch = value; }
		}
		public double Zoom
		{
			get { return m_fZoom; }
			set { m_fZoom = value; }
		}
		public bool Zooming
		{
			get { return m_fZoom > 1.0f;}
		}
		public Point Location
		{
			get { return m_DirectZoomWindow.Location;}
		}
		public Rectangle ZoomWindow
		{
			get { return m_DirectZoomWindow;}
		}		
		public bool FreeMove
		{
			get { return m_bFreeMove; }
			set { m_bFreeMove = value; }
		}
		#endregion
		
		#region Members
		private Size m_OriginalSize;			// Decoding size of the image
		
		// Might be simplified in just .scale and .translate ?
		private double m_fStretch = 1.0f;		// factor to go from decoding size to display size.
		private double m_fZoom = 1.0f;		
		private Rectangle m_DirectZoomWindow = new Rectangle(0, 0, 0, 0);
		//private bool m_bMirrored;
		private bool m_bFreeMove;				// If we allow the image to be moved out of bounds.
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
				
		#region System manipulation
		public void SetOriginalSize(Size _size)
		{
			m_OriginalSize = new Size(_size.Width, _size.Height);
		}
		public void Reset()
		{
			m_fStretch = 1.0f;
			m_fZoom = 1.0f;
			m_DirectZoomWindow = new Rectangle(0, 0, 0, 0);
		}
		public void ReinitZoom()
		{
			m_fZoom = 1.0f;
			m_DirectZoomWindow = new Rectangle(0, 0, m_OriginalSize.Width, m_OriginalSize.Height);
		}
		public void RelocateZoomWindow()
		{
			RelocateZoomWindow(new Point(m_DirectZoomWindow.Left + (m_DirectZoomWindow.Width/2), m_DirectZoomWindow.Top + (m_DirectZoomWindow.Height/2)));
		}
		public void RelocateZoomWindow(Point _center)
		{
			// Recreate the zoom window coordinates, given a new zoom factor, keeping the window center.
			// This used when increasing and decreasing the zoom factor,
			// to automatically adjust the viewing window.
			
			int iNewWidth = (int)((double)m_OriginalSize.Width / m_fZoom);
			int iNewHeight = (int)((double)m_OriginalSize.Height / m_fZoom);

			int iNewLeft = _center.X - (iNewWidth / 2);
			int iNewTop = _center.Y - (iNewHeight / 2);

			if(!m_bFreeMove)
			{
				if (iNewLeft < 0) iNewLeft = 0;
				if (iNewLeft + iNewWidth >= m_OriginalSize.Width) iNewLeft = m_OriginalSize.Width - iNewWidth;
	
				if (iNewTop < 0) iNewTop = 0;
				if (iNewTop + iNewHeight >= m_OriginalSize.Height) iNewTop = m_OriginalSize.Height - iNewHeight;
			}

			m_DirectZoomWindow = new Rectangle(iNewLeft, iNewTop, iNewWidth, iNewHeight);	
		}
		public void MoveZoomWindow(double _fDeltaX, double _fDeltaY)
		{
			// Move the zoom window keeping the same zoom factor.
			
			// Tentative new coords.
			int iNewLeft = (int)((double)m_DirectZoomWindow.Left - _fDeltaX);
			int iNewTop = (int)((double)m_DirectZoomWindow.Top - _fDeltaY);
			
			// Restraint the tentative coords at image borders.
			if(!m_bFreeMove)
			{
				if (iNewLeft < 0) iNewLeft = 0;
				if (iNewTop < 0) iNewTop = 0;
				
				if (iNewLeft + m_DirectZoomWindow.Width >= m_OriginalSize.Width)
					iNewLeft = m_OriginalSize.Width - m_DirectZoomWindow.Width;
				
				if (iNewTop + m_DirectZoomWindow.Height >= m_OriginalSize.Height)
					iNewTop = m_OriginalSize.Height - m_DirectZoomWindow.Height;
			}
			
			// Reposition.
			m_DirectZoomWindow = new Rectangle(iNewLeft, iNewTop, m_DirectZoomWindow.Width, m_DirectZoomWindow.Height);
		}
		#endregion
		
		#region Point to Point transformations
		public Point Untransform(Point _point)
		{
			// in: screen coordinates
			// out: image coordinates.
			// Image may have been stretched, zoomed and moved.

			// 1. Unstretch coords -> As if stretch factor was 1.0f.
			double fUnstretchedX = (double)_point.X / m_fStretch;
			double fUnstretchedY = (double)_point.Y / m_fStretch;

			// 2. Unzoom coords -> As if zoom factor was 1.0f.
			// Unmoved is m_DirectZoomWindow.Left * m_fDirectZoomFactor.
			// Unzoomed is Unmoved / m_fDirectZoomFactor.
			double fUnzoomedX = (double)m_DirectZoomWindow.Left + (fUnstretchedX / m_fZoom);
			double fUnzoomedY = (double)m_DirectZoomWindow.Top + (fUnstretchedY / m_fZoom);

			return new Point((int)fUnzoomedX, (int)fUnzoomedY);	
		}
		public Point Transform(Point _point)
		{
			// in: image coordinates.
			// out: screen coordinates.
			// Image may have been stretched, zoomed and moved.	
			
			// Zoom coords.
			double fZoomedX = (double)(_point.X - m_DirectZoomWindow.Left) * m_fZoom;
			double fZoomedY = (double)(_point.Y - m_DirectZoomWindow.Top) * m_fZoom;

			// Stretch coords.
			double fStretchedX = fZoomedX * m_fStretch;
			double fStretchedY = fZoomedY * m_fStretch;

			return new Point((int)fStretchedX, (int)fStretchedY);
		}

		#endregion
	}
}
