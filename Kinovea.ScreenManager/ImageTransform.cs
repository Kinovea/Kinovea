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
    /// 
    /// TODO: replace all these special cases with matrix maths.
    /// TODO: merge with ImageToViewportTransformer.
    /// 
    /// The class will keep track of the current changes relatively to the 
    /// reference image size and provide conversion routines.
    /// The reference image size is the original image size adjusted for aspect ratio and rotation.
    /// 
    /// All drawings coordinates are kept in the system of the reference size.
    /// For actually drawing them on screen we ask the transformation. 
    /// 
    /// The image aspect ratio is never altered. Skew is not supported.
    /// </summary>
    public class ImageTransform : IImageToViewportTransformer
    {
        #region Properties
        /// <summary>
        /// The total scale at which to render the image. Combines stretching and zooming.
        /// </summary>
        public double Scale
        {
            get { return stretch * zoom; }
        }
        /// <summary>
        /// The stretching to apply to the image due to container manipulation. Does not take zoom into account.
        /// </summary>
        public double Stretch
        {
            get { return stretch; }
            set { stretch = value; }
        }
        /// <summary>
        /// The zoom factor to apply to the image.
        /// </summary>
        public double Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }
        public bool Zooming
        {
            get { return zoom > 1.0f;}
        }
        /// <summary>
        /// Location of the zoom window.
        /// </summary>
        public Point Location
        {
            get { return directZoomWindow.Location;}
        }
        public Rectangle ZoomWindow
        {
            get { return directZoomWindow;}
        }
        public Rectangle RenderingZoomWindow
        {
            get { return renderingZoomWindow; }
        }
        public bool FreeMove
        {
            get { return freeMove; }
            set { freeMove = value; }
        }
        public ImageTransform Identity
        {
            // Return a barebone system with no stretch and no zoom, based on current image size. Used for saving. 
            get { return new ImageTransform(referenceSize); }
        }
        #endregion
        
        #region Members
        private Size referenceSize;			
        private double stretch = 1.0;		// factor to go from decoding size to viewport size.
        private double zoom = 1.0;
        private Rectangle directZoomWindow;
        private Rectangle renderingZoomWindow;
        private double renderingZoomFactor = 1.0;
        private bool freeMove;				// Whether we allow the image to be moved out of screen bounds.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ImageTransform() : this(new Size(1,1)){}
        public ImageTransform(Size _size)
        {
            referenceSize = _size;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            renderingZoomWindow = directZoomWindow;
        }
        #endregion

        #region System manipulation
        public void SetReferenceSize(Size _size)
        {
            referenceSize = _size;
        }
        public void Reset()
        {
            stretch = 1.0f;
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateRenderingZoomWindow();
        }
        public void ReinitZoom()
        {
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateRenderingZoomWindow();
        }
        public void RelocateZoomWindow()
        {
            RelocateZoomWindow(new Point(directZoomWindow.Left + (directZoomWindow.Width/2), directZoomWindow.Top + (directZoomWindow.Height/2)));
        }
        public void RelocateZoomWindow(Point center)
        {
            // Recreate the zoom window coordinates, given a new zoom factor, keeping the window center.
            // This used when increasing and decreasing the zoom factor,
            // to automatically adjust the viewing window.
            Size newSize = new Size((int)(referenceSize.Width / zoom), (int)(referenceSize.Height / zoom));
            int left = center.X - (newSize.Width / 2);
            int top = center.Y - (newSize.Height / 2);
            
            Point newLocation = ConfineZoomWindow(left, top, newSize, referenceSize);

            directZoomWindow = new Rectangle(newLocation, newSize);
            UpdateRenderingZoomWindow();
        }
        public void MoveZoomWindow(double dx, double dy)
        {
            // Move the zoom window keeping the same zoom factor.
            Point newLocation = ConfineZoomWindow((int)(directZoomWindow.Left - dx), (int)(directZoomWindow.Top - dy), directZoomWindow.Size, referenceSize);
            directZoomWindow = new Rectangle(newLocation.X, newLocation.Y, directZoomWindow.Width, directZoomWindow.Height);
            UpdateRenderingZoomWindow();
        }

        /// <summary>
        /// Set the factor used to compute the zoom window in the received images.
        /// This should be related to the decoding size vs (unrotated) reference size.
        /// </summary>
        public void SetRenderingZoomFactor(double zoomFactor)
        {
            renderingZoomFactor = zoomFactor;
            UpdateRenderingZoomWindow();
        }
        private void UpdateRenderingZoomWindow()
        {
            renderingZoomWindow = directZoomWindow.Scale(renderingZoomFactor, renderingZoomFactor);

            //log.DebugFormat("UpdateRenderingZoomWindow: stretch:{0:0.00}, zoom:{1:0.00}, renderingZoomFactor:{2:0.00}, directZoomWindow:{3}, renderingZoomWindow:{4}", 
            //    stretch, zoom, renderingZoomFactor, directZoomWindow, renderingZoomWindow);
        }
        private Point ConfineZoomWindow(int left, int top, Size zoomWindow, Size containerSize)
        {
            // Prevent the zoom window to move outside the rendering window.
            
            if(freeMove)
                return new Point(left, top);

            int newLeft = Math.Min(Math.Max(0, left), containerSize.Width - zoomWindow.Width);
            int newTop = Math.Min(Math.Max(0, top), containerSize.Height - zoomWindow.Height);
            
            return new Point(newLeft, newTop);
        }
        #endregion
        
        #region Transformations
        public PointF Untransform(Point point)
        {
            // in: screen coordinates
            // out: image coordinates.
            // Image may have been stretched, zoomed and moved.

            // 1. Unstretch coords -> As if stretch factor was 1.0f.
            double unstretchedX = (double)point.X / stretch;
            double unstretchedY = (double)point.Y / stretch;

            // 2. Unzoom coords -> As if zoom factor was 1.0f.
            double unzoomedX = (double)directZoomWindow.Left + (unstretchedX / zoom);
            double unzoomedY = (double)directZoomWindow.Top + (unstretchedY / zoom);

            return new PointF((float)unzoomedX, (float)unzoomedY);	
        }

        public SizeF Untransform(SizeF size)
        {
            double scale = stretch * zoom;
            return new SizeF((float)(size.Width / scale), (float)(size.Height / scale));
        }
        
        public int Untransform(int v)
        {
            double result = (v / stretch) / zoom;
            return (int)result;
        }
        
        /// <summary>
        /// Transform a point from image system to screen system. Handles scale, zoom and translate.
        /// </summary>
        /// <param name="point">The point in image coordinate system</param>
        /// <returns>The point in screen coordinate system</returns>
        public Point Transform(Point point)
        {
            // Zoom and translate
            double zoomedX = (double)(point.X - directZoomWindow.Left) * zoom;
            double zoomedY = (double)(point.Y - directZoomWindow.Top) * zoom;

            // Scale
            double stretchedX = zoomedX * stretch;
            double stretchedY = zoomedY * stretch;

            return new Point((int)stretchedX, (int)stretchedY);
        }
        public Point Transform(PointF point)
        {
            float x = point.X;
            float y = point.Y;
            
            // Zoom and translate
            double zoomedX = (x - directZoomWindow.Left) * zoom;
            double zoomedY = (y - directZoomWindow.Top) * zoom;

            // Scale
            double stretchedX = zoomedX * stretch;
            double stretchedY = zoomedY * stretch;

            return new Point((int)stretchedX, (int)stretchedY);
        }
        public List<Point> Transform(IEnumerable<PointF> points)
        {
            List<Point> newPoints = new List<Point>();
            foreach(PointF p in points)
                newPoints.Add(Transform(p));
            
            return newPoints;
        }

        /// <summary>
        /// Transform a length in the image coordinate system to its equivalent in screen coordinate system.
        /// Only uses scale and zoom.
        /// </summary>
        /// <param name="length">The length value to transform</param>
        /// <returns>The length value in screen coordinate system</returns>
        public int Transform(int length)
        {
            return (int)(length * stretch * zoom);
        }

        /// <summary>
        /// Transform a size from the image coordinate system to its equivalent in screen coordinate system.
        /// Only uses stretch and zoom.
        /// </summary>
        /// <param name="size">The Size value to transform</param>
        /// <returns>The size value in screen coordinate system</returns>
        public Size Transform(Size size)
        {
            return new Size(Transform(size.Width), Transform(size.Height));
        }

        public Size Transform(SizeF size)
        {
            return new Size(Transform((int)size.Width), Transform((int)size.Height));
        }
        
        /// <summary>
        /// Transform a rectangle from the image coordinate system to its equivalent in screen coordinate system.
        /// Uses stretch, zoom and translate.
        /// </summary>
        /// <param name="rect">The rectangle value to transform</param>
        /// <returns>The rectangle value in screen coordinate system</returns>
        public Rectangle Transform(Rectangle rect)
        {
            return new Rectangle(Transform(rect.Location), Transform(rect.Size));
        }
        public List<Rectangle> Transform(List<Rectangle> rectangles)
        {
            List<Rectangle> newRectangles = new List<Rectangle>();
            foreach(Rectangle r in rectangles)
                newRectangles.Add(Transform(r));

            return newRectangles;
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
