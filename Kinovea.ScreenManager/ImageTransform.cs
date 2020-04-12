#region License
/*
Copyright © Joan Charmant 2009.
jcharmant@gmail.com 
 
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
    /// - stretching: the rectangle where the image is displayed may be stretched or squeezed relative to the original size.
    /// - zooming: the region of interest may be a sub window of the original image.
    /// 
    /// 
    /// The class will keep track of the current changes relatively to the 
    /// reference image size and provide conversion routines.
    /// The reference image size is the original image size adjusted for aspect ratio and rotation.
    /// 
    /// All drawings coordinates are kept in the system of the reference size.
    /// For actually drawing them on screen we ask the transformation here. 
    /// The image itself is decoded at a custom size that can be smaller than the reference size.
    /// The rendering surface also has its own size that can be changed by user stretching.
    /// 
    /// The image aspect ratio is never altered. Skew is not supported.
    /// TODO: merge with ImageToViewportTransformer.
    /// </summary>
    public class ImageTransform : IImageToViewportTransformer
    {
        #region Properties
        /// <summary>
        /// The total scale used to transform coordinates in the reference size into coordinates in the rendering size.
        /// This does not take into account the decoding size and should not be used to render the image itself.
        /// </summary>
        public double Scale
        {
            get { return stretch * zoom; }
        }

        /// <summary>
        /// The scale factor solely due to user stretching the rendering surface. 
        /// Goes from reference size to rendering surface size.
        /// Does not take decoding size into account.
        /// </summary>
        public double Stretch
        {
            get { return stretch; }
            set { stretch = value; }
        }

        /// <summary>
        /// The zoom factor applied to the image due to user zooming in/out.
        /// Goes from reference size to zoom window size.
        /// Does not take decoding size into account.
        /// </summary>
        public double Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }

        /// <summary>
        /// Whether we are currently zooming or not.
        /// </summary>
        public bool Zooming
        {
            get { return zoom > 1.0f;}
        }

        /// <summary>
        /// The location of the zoom window inside the reference image.
        /// Does not take decoding size into account.
        /// </summary>
        public Rectangle ZoomWindow
        {
            get { return directZoomWindow;}
        }

        /// <summary>
        /// The factor between the decoded image and the reference image.
        /// </summary>
        public double DecodingScale
        {
            get { return decodingScale; }
            set
            {
                decodingScale = value;
                UpdateZoomWindowInDecodedImage();
            }
        }

        /// <summary>
        /// The location of the zoom window inside the decoded image.
        /// </summary>
        public Rectangle ZoomWindowInDecodedImage
        {
            get { return zoomWindowInDecodedImage; }
        }
        
        public bool AllowOutOfScreen
        {
            get { return allowOutOfScreen; }
            set { allowOutOfScreen = value; }
        }
        public ImageTransform Identity
        {
            // Return a barebone system with no stretch and no zoom, based on current image size. Used for saving. 
            get { return new ImageTransform(referenceSize); }
        }
        #endregion
        
        #region Members

        // Variables used to transform coordinates in the reference size into coordinates in the final rendering surface.
        // These should only be used by drawings and cursors. Not by the image itself because the image is not necessarily decoded at the reference size.
        private Size referenceSize;
        private double stretch = 1.0;
        private double zoom = 1.0;
        private Rectangle directZoomWindow;

        // Variables used by the paint routine to render the image on the rendering surface.
        private double decodingScale = 1.0;
        private Rectangle zoomWindowInDecodedImage;

        private bool allowOutOfScreen;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ImageTransform() : this(new Size(1,1)){}
        public ImageTransform(Size referenceSize)
        {
            this.referenceSize = referenceSize;
            stretch = 1.0;
            zoom = 1.0;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);

            decodingScale = 1.0;
            zoomWindowInDecodedImage = directZoomWindow;
        }
        #endregion

        #region System manipulation
        public void SetReferenceSize(Size referenceSize)
        {
            this.referenceSize = referenceSize;
        }
        public void Reset()
        {
            stretch = 1.0f;
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateZoomWindowInDecodedImage();
        }
        public void ReinitZoom()
        {
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateZoomWindowInDecodedImage();
        }
        public void UpdateZoomWindow()
        {
            UpdateZoomWindow(directZoomWindow.Center());
        }
        public void UpdateZoomWindow(Point center)
        {
            // Recreate the zoom window coordinates, after the zoom factor was changed externally, keeping the window center.
            // Used when increasing and decreasing the zoom factor.
            Size newSize = new Size((int)(referenceSize.Width / zoom), (int)(referenceSize.Height / zoom));
            int left = center.X - (newSize.Width / 2);
            int top = center.Y - (newSize.Height / 2);
            
            Point newLocation = ConfineZoomWindow(left, top, newSize, referenceSize);

            directZoomWindow = new Rectangle(newLocation, newSize);
            UpdateZoomWindowInDecodedImage();
        }
        public void MoveZoomWindow(double dx, double dy)
        {
            // Move the zoom window keeping the same zoom factor.
            Point newLocation = ConfineZoomWindow((int)(directZoomWindow.Left - dx), (int)(directZoomWindow.Top - dy), directZoomWindow.Size, referenceSize);
            directZoomWindow = new Rectangle(newLocation.X, newLocation.Y, directZoomWindow.Width, directZoomWindow.Height);
            UpdateZoomWindowInDecodedImage();
        }

        private void UpdateZoomWindowInDecodedImage()
        {
            zoomWindowInDecodedImage = directZoomWindow.Scale(decodingScale, decodingScale);
        }
        private Point ConfineZoomWindow(int left, int top, Size zoomWindow, Size containerSize)
        {
            // Prevent the zoom window to move outside the rendering window.
            
            if(allowOutOfScreen)
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
