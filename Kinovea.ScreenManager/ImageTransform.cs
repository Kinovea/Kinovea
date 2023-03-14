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
    /// Helper class to encapsulate the transform between rectified image space and screen space.
    /// Note : This is not the image to world transform, that one is in Measurement/Calibration/CalibrationHelper.cs.
    ///
    /// Rectified image space is based on the original image space but adjusted for aspect ratio, rotation and distortion.
    ///
    /// Includes : 
    /// - stretching: the rectangle where the image is displayed may be stretched or squeezed relative to the original size.
    /// - zooming and panning: the region of interest may be a sub window of the original image.
    /// 
    /// Note: mirroring is not handled here but explicitly at rendering time. Mirroring only concerns the image not drawings.
    /// 
    /// All drawings coordinates are kept in the system of the reference size.
    /// For actually drawing them on screen we ask the transformation here. 
    /// The image itself is decoded at a custom size that can be smaller than the reference size.
    /// The rendering surface also has its own size that can be changed by user stretching.
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
            set { stretch = (float)value; }
        }

        /// <summary>
        /// The zoom factor applied to the image due to user zooming in/out.
        /// Goes from reference size to zoom window size.
        /// Does not take decoding size into account.
        /// </summary>
        public double Zoom
        {
            get { return zoom; }
            set { zoom = (float)value; }
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
                decodingScale = (float)value;
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
        private float stretch = 1.0f;
        private float zoom = 1.0f;
        private Rectangle directZoomWindow;

        // Variables used by the paint routine to render the image on the rendering surface.
        private float decodingScale = 1.0f;
        private Rectangle zoomWindowInDecodedImage;

        private bool allowOutOfScreen;
        private bool forceContain = true;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ImageTransform() : this(new Size(1,1)){}
        public ImageTransform(Size referenceSize)
        {
            this.referenceSize = referenceSize;
            stretch = 1.0f;
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);

            decodingScale = 1.0f;
            zoomWindowInDecodedImage = directZoomWindow;
        }
        #endregion

        #region System manipulation
        public void SetReferenceSize(Size referenceSize)
        {
            this.referenceSize = referenceSize;
        }

        /// <summary>
        /// Reset zoom and stretch to 1x.
        /// </summary>
        public void Reset()
        {
            stretch = 1.0f;
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateZoomWindowInDecodedImage();
        }

        /// <summary>
        /// Reset the zoom to 1x.
        /// </summary>
        public void ResetZoom()
        {
            zoom = 1.0f;
            directZoomWindow = new Rectangle(0, 0, referenceSize.Width, referenceSize.Height);
            UpdateZoomWindowInDecodedImage();
        }

        /// <summary>
        /// Update the sub-window of the image that is rendered on the viewport rendering surface after a zoom operation.
        /// The pivot point is given in coordinates of the stretched rendering surface.
        /// </summary>
        public void UpdateZoomWindow(Point pivotSurface, bool contain)
        {
            // All computations are done in UV space.
            RectangleF zoomWindowUVInImage = new RectangleF(
                (float)(directZoomWindow.Left + 0.5f) / referenceSize.Width,
                (float)(directZoomWindow.Top + 0.5f) / referenceSize.Height,
                (float)directZoomWindow.Width / referenceSize.Width,
                (float)directZoomWindow.Height / referenceSize.Height);

            Size containerSize = new SizeF(referenceSize.Width * stretch, referenceSize.Height * stretch).ToSize();
            PointF pivotUVInZoomWindow = new PointF(
                (pivotSurface.X + 0.5f) / containerSize.Width, 
                (pivotSurface.Y + 0.5f) / containerSize.Height);

            PointF pivotUVInImage = new PointF(
                zoomWindowUVInImage.Left + pivotUVInZoomWindow.X * zoomWindowUVInImage.Width,
                zoomWindowUVInImage.Top + pivotUVInZoomWindow.Y * zoomWindowUVInImage.Height
            );

            // This must be aligned to the pixel boundaries, not just (1.0/zoom).
            SizeF newSizeUV = new SizeF(
                (float)Math.Floor(referenceSize.Width / zoom) / referenceSize.Width,
                (float)Math.Floor(referenceSize.Height / zoom) / referenceSize.Height
            );

            PointF newTopLeftUV = new PointF(
                pivotUVInImage.X - pivotUVInZoomWindow.X * newSizeUV.Width,
                pivotUVInImage.Y - pivotUVInZoomWindow.Y * newSizeUV.Height
            );

            directZoomWindow = new RectangleF(
                newTopLeftUV.X * referenceSize.Width - 0.5f,
                newTopLeftUV.Y * referenceSize.Height - 0.5f,
                newSizeUV.Width * referenceSize.Width,
                newSizeUV.Height * referenceSize.Height
            ).ToRectangle();

            if (contain || forceContain)
                directZoomWindow.Location = ConfineZoomWindow(directZoomWindow, referenceSize);
            
            UpdateZoomWindowInDecodedImage();
        }
        public void MoveZoomWindow(float dx, float dy, bool contain)
        {
            // Move the zoom window keeping the same zoom factor.
            directZoomWindow.Location = new PointF(directZoomWindow.Left - dx, directZoomWindow.Top - dy).ToPoint();
            
            if (contain || forceContain)
                directZoomWindow.Location = ConfineZoomWindow(directZoomWindow, referenceSize);
            
            UpdateZoomWindowInDecodedImage();
        }

        private void UpdateZoomWindowInDecodedImage()
        {
            zoomWindowInDecodedImage = directZoomWindow.Scale(decodingScale, decodingScale);
        }
        
        /// <summary>
        /// Restrict the zoom window to the bounds of the image.
        /// Returns the image space location of the zoom window.
        /// </summary>
        private Point ConfineZoomWindow(Rectangle candidate, Size containerSize)
        {
            int newLeft = Math.Min(Math.Max(0, candidate.X), containerSize.Width - candidate.Width);
            int newTop = Math.Min(Math.Max(0, candidate.Y), containerSize.Height - candidate.Height);
            return new Point(newLeft, newTop);
        }
        #endregion

        #region Screen space to rectified image space.
        /// <summary>
        /// Transform a point from screen coordinates to image coordinates.
        /// </summary>
        public PointF Untransform(Point point)
        {
            float x = point.X;
            float y = point.Y;

            x = directZoomWindow.Left + (x / (stretch * zoom));
            y = directZoomWindow.Top + (y / (stretch * zoom));

            return new PointF(x, y);	
        }

        /// <summary>
        /// Transform a rectangle from screen coordinates to image coordinates.
        /// </summary>

        public RectangleF Untransform(Rectangle rectangle)
        {
            return new RectangleF(Untransform(rectangle.Location), Untransform(rectangle.Size));
        }

        /// <summary>
        /// Transform a distance from screen coordinates to image coordinates.
        /// </summary>
        public int Untransform(int v)
        {
            return (int)(v / stretch / zoom);
        }

        /// <summary>
        /// Transform a size from screen coordinates to image coordinates.
        /// </summary>
        public SizeF Untransform(SizeF size)
        {
            float scale = stretch * zoom;
            return new SizeF(size.Width / scale, size.Height / scale);
        }
        #endregion



        #region Rectified image space to screen space

        /// <summary>
        /// Transform a point from image coordinates to screen coordinates.
        /// </summary>
        public Point Transform(PointF point)
        {
            float x = (point.X - directZoomWindow.Left) * zoom * stretch;
            float y = (point.Y - directZoomWindow.Top) * zoom * stretch;

            return new Point((int)x, (int)y);
        }

        /// <summary>
        /// Transform a point from image coordinates to screen coordinates.
        /// </summary>
        public Point Transform(Point point)
        {
            return Transform(new PointF(point.X, point.Y));
        }

        /// <summary>
        /// Transform a list of points from image coordinates to screen coordinates.
        /// </summary>
        public List<Point> Transform(IEnumerable<PointF> points)
        {
            List<Point> newPoints = new List<Point>();
            foreach(PointF p in points)
                newPoints.Add(Transform(p));
            
            return newPoints;
        }

        /// <summary>
        /// Transform a distance from image coordinates to screen coordinates.
        /// </summary>
        public int Transform(float length)
        {
            return (int)(length * zoom * stretch);
        }

        /// <summary>
        /// Transform a distance from image coordinates to screen coordinates.
        /// </summary>
        public int Transform(int length)
        {
            return Transform((float)length);
        }

        /// <summary>
        /// Transform a size from image coordinates screen coordinates.
        /// </summary>
        public Size Transform(SizeF size)
        {
            return new Size(Transform(size.Width), Transform(size.Height));
        }

        /// <summary>
        /// Transform a size from image coordinates screen coordinates.
        /// </summary>
        public Size Transform(Size size)
        {
            return new Size(Transform(size.Width), Transform(size.Height));
        }

        /// <summary>
        /// Transform a rectangle from image coordinates to screen coordinates.
        /// </summary>
        public Rectangle Transform(RectangleF rect)
        {
            return new Rectangle(Transform(rect.Location), Transform(rect.Size));
        }

        /// <summary>
        /// Transform a rectangle from image coordinates to screen coordinates.
        /// </summary>
        public Rectangle Transform(Rectangle rect)
        {
            return new Rectangle(Transform(rect.Location), Transform(rect.Size));
        }

        /// <summary>
        /// Transform a list of rectangles from image coordinates to screen coordinates.
        /// </summary>
        public List<Rectangle> Transform(List<Rectangle> rectangles)
        {
            List<Rectangle> newRectangles = new List<Rectangle>();
            foreach(Rectangle r in rectangles)
                newRectangles.Add(Transform(r));

            return newRectangles;
        }

        /// <summary>
        /// Transform a quadrilateral from image coordinates to screen coordinates.
        /// </summary>
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
