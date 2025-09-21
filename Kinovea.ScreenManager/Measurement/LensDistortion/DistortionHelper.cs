using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The main utility class for converting between points from image space (distorted) to ideal image space (rectified).
    /// This uses the focal distance, projection center and radial distortion.
    /// 
    /// The lens calibration must have been done already and stored passed in the DistortionParameters object, 
    /// here we just convert from distorted space to rectified space and back.
    /// </summary>
    public class DistortionHelper
    {
        #region Properties
        public DistortionParameters Parameters
        {
            get { return parameters; }
        }

        public bool Initialized
        {
            get { return initialized; }
        }
        public int ContentHash
        {
            get { return contentHash; }
        }
        #endregion

        private bool initialized;
        private int contentHash;
        private DistortionParameters parameters;
        private Size imageSize;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Initialize(DistortionParameters parameters, Size imageSize)
        {
            if (parameters.Cx == 0 && parameters.Cy == 0)
                this.parameters = new DistortionParameters(imageSize);
            else
                this.parameters = parameters;

            this.imageSize = imageSize;
            initialized = true;

            contentHash = parameters.ContentHash;
        }

        public void Uninitialize()
        {
            initialized = false;
            parameters = null;
            contentHash = 0;
        }

        /// <summary>
        /// Given coordinates in distorted space, returns the point in undistorted space.
        /// </summary>
        public PointF Undistort(PointF point)
        {
            if (!initialized)
                return point;

            var src = OpenCvSharp.Mat.FromPixelData(1, 1, OpenCvSharp.MatType.CV_32FC2, new float[] { point.X, point.Y });
            var dst = new OpenCvSharp.Mat(1, 1, OpenCvSharp.MatType.CV_32FC2);
            var matCameraMatrix = OpenCvSharp.Mat.FromPixelData(3, 3, OpenCvSharp.MatType.CV_64FC1, parameters.cameraMatrix);
            var matDistCoeffs = OpenCvSharp.Mat.FromPixelData(5, 1, OpenCvSharp.MatType.CV_64FC1, parameters.distCoeffs);

            OpenCvSharp.Cv2.UndistortPoints(
                src,
                dst,
                matCameraMatrix,
                matDistCoeffs
            );

            OpenCvSharp.Vec2f recti = dst.Get<OpenCvSharp.Vec2f>(0);
            float x = (float)(recti[0] * parameters.Fx + parameters.Cx);
            float y = (float)(recti[1] * parameters.Fy + parameters.Cy);
                    
            matCameraMatrix.Dispose();
            matDistCoeffs.Dispose();
            src.Dispose();
            dst.Dispose();

            return new PointF(x, y);
        }

        /// <summary>
        /// Given coordinates in undistorted space, returns the point in distorted space.
        /// </summary>
        public PointF Distort(PointF point)
        {
            PointF result = point;

            if (!initialized)
                return result;

            try
            {
                // Ref:
                // http://docs.opencv.org/modules/imgproc/doc/geometric_transformations.html#cv.InitUndistortRectifyMap

                // To relative coordinates
                double x = (point.X - parameters.Cx) / parameters.Fx;
                double y = (point.Y - parameters.Cy) / parameters.Fy;

                double r2 = x * x + y * y;

                //radial distorsion
                double xDistort = x * (1 + parameters.K1 * r2 + parameters.K2 * r2 * r2 + parameters.K3 * r2 * r2 * r2);
                double yDistort = y * (1 + parameters.K1 * r2 + parameters.K2 * r2 * r2 + parameters.K3 * r2 * r2 * r2);

                //tangential distorsion
                xDistort = xDistort + (2 * parameters.P1 * x * y + parameters.P2 * (r2 + 2 * x * x));
                yDistort = yDistort + (parameters.P1 * (r2 + 2 * y * y) + 2 * parameters.P2 * x * y);

                // To absolute coordinates.
                xDistort = xDistort * parameters.Fx + parameters.Cx;
                yDistort = yDistort * parameters.Fy + parameters.Cy;

                result = new PointF((float)xDistort, (float)yDistort);
            }
            catch
            {
                result = point;
            }

            return result;
        }

        /// <summary>
        /// Takes the start and end point of a segment in distorted space and return 
        /// the same segment as a list of points still in distorted space.
        /// The segment is split in several subsegments that can be drawn with drawCurve.
        /// </summary>
        public List<PointF> DistortLine(PointF start, PointF end)
        {
            int innerPoints = 5;
            float factor = 1.0f / ((float)innerPoints + 1);

            List<PointF> points = new List<PointF>();
            points.Add(start);
            DistortInnerPoints(Undistort(start), Undistort(end), factor, innerPoints, points);
            points.Add(end);

            return points;
        }

        /// <summary>
        /// Takes the start and end point of a segment in rectified space and return 
        /// the same segment as a list of points in distorted space.
        /// The segment is split in several subsegments that can be drawn with drawCurve.
        /// </summary>
        public List<PointF> DistortRectifiedLine(PointF start, PointF end)
        {
            int innerPoints = 5;
            float factor = 1.0f / ((float)innerPoints + 1);

            List<PointF> points = new List<PointF>();
            points.Add(Distort(start));
            DistortInnerPoints(start, end, factor, innerPoints, points);
            points.Add(Distort(end));

            return points;
        }

        private void DistortInnerPoints(PointF start, PointF end, float factor, int innerPoints, List<PointF> points)
        {
            // Takes a segment end points in rectified space and fill the passed in list with 
            // distorted versions of points on the segment.
            Vector v = new Vector(start, end);

            for (int i = 1; i <= innerPoints; i++)
            {
                Vector v1 = v * (i * factor);
                PointF undistorted = start + v1;
                PointF distorted = Distort(undistorted);
                points.Add(distorted);
            }
        }

        /// <summary>
        /// Returns an undistorted quadrilateral.
        /// </summary>
        public QuadrilateralF Undistort(QuadrilateralF quad)
        {
            return new QuadrilateralF(
                Undistort(quad.A),
                Undistort(quad.B),
                Undistort(quad.C),
                Undistort(quad.D));
        }

        /// <summary>
        /// Builds a full scale image with distortion vectors.
        /// If distort is true, the vectors go from undistorted space to distorted space.
        /// </summary>
        public Bitmap GetDistortionMap(bool distort)
        {
            Bitmap bmp = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format24bppRgb);

            if (!initialized)
                return bmp;

            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRectangle(Brushes.White, 0, 0, imageSize.Width, imageSize.Height);

            Pen p = new Pen(Color.Black);
            p.EndCap = LineCap.ArrowAnchor;

            int steps = 20;
            int stepWidth = (imageSize.Width - 1) / steps;
            int stepHeight = (imageSize.Height - 1) / steps;
            for (int col = 0; col < imageSize.Width; col += stepWidth)
            {
                for (int row = 0; row < imageSize.Height; row += stepHeight)
                {
                    PointF src = new PointF(col, row);
                    PointF dst = distort ? Distort(src) : Undistort(src);
                    g.FillEllipse(Brushes.Black, new PointF(col, row).Box(3));
                    g.DrawLine(p, src, dst);
                }
            }

            p.Dispose();

            return bmp;
        }

        /// <summary>
        /// Builds a full scale image with a distorted grid pattern.
        /// </summary>
        public Bitmap GetDistortionGrid(Color color, int lineSize, int cols, int rows)
        {
            Bitmap bmp = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format32bppPArgb);
            
            if (!initialized)
                return bmp;

            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias; 
            
            Pen p = new Pen(color, lineSize);

            float stepWidth = (float)imageSize.Width / cols;
            float stepHeight = (float)imageSize.Height / rows;

            // Verticals
            for (int i = 0; i <= cols; i++)
            {
                int col = (int)Math.Min(imageSize.Width - 1, Math.Round(i * stepWidth));

                PointF start = new PointF(col, 0);
                PointF end = new PointF(col, imageSize.Height);

                List<PointF> points = DistortRectifiedLine(start, end);
                g.DrawCurve(p, points.ToArray());
            }

            // Horizontals
            for (int i = 0; i <= rows; i++)
            {
                int row = (int)Math.Min(imageSize.Height - 1, Math.Round(i * stepHeight));
                
                PointF start = new PointF(0, row);
                PointF end = new PointF(imageSize.Width, row);

                List<PointF> points = DistortRectifiedLine(start, end);
                g.DrawCurve(p, points.ToArray());
            }

            p.Dispose();

            return bmp;
        }
    }

}
