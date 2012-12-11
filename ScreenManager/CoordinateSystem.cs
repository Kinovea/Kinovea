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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Helper class to encapsulate calculations about the coordinate system for drawings,
	/// to compensate the differences between the original image and the displayed one.
	/// Note : This is not the coordinate system that the user can adjust for distance calculations, that one is PlayerScreen/CalibrationHelper.cs.
	/// 
	/// Includes : 
	/// - stretching, image may be stretched or squeezed relative to the original.
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
        /// <summary>
        /// The total scale at which to render the image. Combines stretching and zooming.
        /// </summary>
		public double Scale
        {
            get { return m_fStretch * m_fZoom; }
        }
		/// <summary>
		/// The stretching to apply to the image due to container manipulation. Does not take zoom into account.
		/// </summary>
		public double Stretch
		{
			get { return m_fStretch; }
			set { m_fStretch = value; }
		}
		/// <summary>
		/// The zoom factor to apply to the image.
		/// </summary>
		public double Zoom
		{
			get { return m_fZoom; }
			set { m_fZoom = value; }
		}
		public bool Zooming
		{
			get { return m_fZoom > 1.0f;}
		}
		/// <summary>
		/// Location of the zoom window.
		/// </summary>
		public Point Location
		{
			get { return m_DirectZoomWindow.Location;}
		}
		public Rectangle ZoomWindow
		{
			get { return m_DirectZoomWindow;}
		}
		public Rectangle RenderingZoomWindow
		{
		    get { return m_RenderingZoomWindow; }
		}
		public bool FreeMove
		{
			get { return m_bFreeMove; }
			set { m_bFreeMove = value; }
		}
        public CoordinateSystem Identity
        {
            // Return a barebone system with no stretch and no zoom, based on current image size. Used for saving. 
            get { return new CoordinateSystem(m_OriginalSize); }
        }
		#endregion
		
		#region Members
		private Size m_OriginalSize;			// Decoding size of the image
		private double m_fStretch = 1.0f;		// factor to go from decoding size to display size.
		private double m_fZoom = 1.0f;		
		private Rectangle m_DirectZoomWindow;
		private bool m_bFreeMove;				// If we allow the image to be moved out of bounds.
		private Rectangle m_RenderingZoomWindow;
		private double m_RenderingZoomFactor;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

        #region Constructor
        public CoordinateSystem() : this(new Size(1,1)){}
        public CoordinateSystem(Size _size)
        {
            SetOriginalSize(_size);
        }
        #endregion

        #region System manipulation
        public void SetOriginalSize(Size _size)
		{
			m_OriginalSize = _size;
		}
		public void Reset()
		{
			m_fStretch = 1.0f;
			m_fZoom = 1.0f;
			m_DirectZoomWindow = Rectangle.Empty;
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
			Size newSize = new Size((int)(m_OriginalSize.Width / m_fZoom), (int)(m_OriginalSize.Height / m_fZoom));
			int left = _center.X - (newSize.Width / 2);
			int top = _center.Y - (newSize.Height / 2);
			
			Point newLocation = ConfineZoomWindow(left, top, newSize, m_OriginalSize);

			m_DirectZoomWindow = new Rectangle(newLocation, newSize);
			UpdateRenderingZoomWindow();
		}
		public void MoveZoomWindow(double _fDeltaX, double _fDeltaY)
		{
			// Move the zoom window keeping the same zoom factor.
			Point newLocation = ConfineZoomWindow((int)(m_DirectZoomWindow.Left - _fDeltaX), (int)(m_DirectZoomWindow.Top - _fDeltaY), m_DirectZoomWindow.Size, m_OriginalSize);
			m_DirectZoomWindow = new Rectangle(newLocation.X, newLocation.Y, m_DirectZoomWindow.Width, m_DirectZoomWindow.Height);
			UpdateRenderingZoomWindow();
		}
		public void SetRenderingZoomFactor(double _zoomFactor)
		{
		    m_RenderingZoomFactor = _zoomFactor;
		    UpdateRenderingZoomWindow();
		}
		private void UpdateRenderingZoomWindow()
		{
		    m_RenderingZoomWindow = m_DirectZoomWindow.Scale(m_RenderingZoomFactor, m_RenderingZoomFactor);
		}
		private Point ConfineZoomWindow(int _left, int _top, Size _zoomWindow, Size _containerSize)
		{
		    // Prevent the zoom window to move outside the rendering window.
            
		    if(m_bFreeMove)
		        return new Point(_left, _top);

            int left = Math.Min(Math.Max(0, _left), _containerSize.Width - _zoomWindow.Width);
            int top = Math.Min(Math.Max(0, _top), _containerSize.Height - _zoomWindow.Height);
            
			return new Point(left, top);
		}
		#endregion
		
		#region Transformations
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
		
		public int Untransform(int v)
		{
		    double result = (v / m_fStretch) / m_fZoom;
		    return (int)result;
		}
		
        /// <summary>
        /// Transform a point from image system to screen system. Handles scale, zoom and translate.
        /// </summary>
        /// <param name="_point">The point in image coordinate system</param>
        /// <returns>The point in screen coordinate system</returns>
		public Point Transform(Point _point)
		{
			// Zoom and translate
            double fZoomedX = (double)(_point.X - m_DirectZoomWindow.Left) * m_fZoom;
			double fZoomedY = (double)(_point.Y - m_DirectZoomWindow.Top) * m_fZoom;

            // Scale
            double fStretchedX = fZoomedX * m_fStretch;
			double fStretchedY = fZoomedY * m_fStretch;

			return new Point((int)fStretchedX, (int)fStretchedY);
		}
		public Point Transform(PointF point)
		{
            float x = point.X;
            float y = point.Y;
            
			// Zoom and translate
            double fZoomedX = (x - m_DirectZoomWindow.Left) * m_fZoom;
            double fZoomedY = (y - m_DirectZoomWindow.Top) * m_fZoom;

            // Scale
            double fStretchedX = fZoomedX * m_fStretch;
			double fStretchedY = fZoomedY * m_fStretch;

			return new Point((int)fStretchedX, (int)fStretchedY);
		}
		public List<Point> Transform(List<PointF> _points)
		{
		    List<Point> points = new List<Point>();
		    foreach(PointF p in _points)
		        points.Add(Transform(p));
		    
			return points;
		}

        /// <summary>
        /// Transform a length in the image coordinate system to its equivalent in screen coordinate system.
        /// Only uses scale and zoom.
        /// </summary>
        /// <param name="_length">The length value to transform</param>
        /// <returns>The length value in screen coordinate system</returns>
        public int Transform(int _length)
        {
            return (int)(_length * m_fStretch * m_fZoom);
        }

        /// <summary>
        /// Transform a size from the image coordinate system to its equivalent in screen coordinate system.
        /// Only uses stretch and zoom.
        /// </summary>
        /// <param name="_size">The Size value to transform</param>
        /// <returns>The size value in screen coordinate system</returns>
        public Size Transform(Size _size)
        {
            return new Size(Transform(_size.Width), Transform(_size.Height));
        }

        public Size Transform(SizeF _size)
        {
            return new Size(Transform((int)_size.Width), Transform((int)_size.Height));
        }
        
        /// <summary>
        /// Transform a rectangle from the image coordinate system to its equivalent in screen coordinate system.
        /// Uses stretch, zoom and translate.
        /// </summary>
        /// <param name="_rect">The rectangle value to transform</param>
        /// <returns>The rectangle value in screen coordinate system</returns>
        public Rectangle Transform(Rectangle _rect)
        {
            return new Rectangle(Transform(_rect.Location), Transform(_rect.Size));
        }
        public List<Rectangle> Transform(List<Rectangle> _rectangles)
        {
            List<Rectangle> rectangles = new List<Rectangle>();
            foreach(Rectangle r in _rectangles)
                rectangles.Add(Transform(r));
            
            return rectangles;
        }
        
        public Rectangle Transform(RectangleF rect)
        {
            return new Rectangle(Transform(rect.Location), Transform(rect.Size));
        }
        
        public QuadrilateralF Transform(QuadrilateralF quad)
        {
            return new QuadrilateralF() {
                A=Transform(quad.A),
                B=Transform(quad.B),
                C=Transform(quad.C),
                D=Transform(quad.D)
            };
        }
		#endregion
	}
}
