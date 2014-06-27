using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The main utility class for lens distortion conversion routines.
    /// The calibration must have been done already, here we just convert from
    /// distorted space to undistorted space and back.
    /// </summary>
    public class DistortionHelper
    {
        public DistortionParameters Parameters
        {
            get { return parameters; }
        }

        public bool Initialized
        {
            get { return initialized; }
        }

        private bool initialized;
        private DistortionParameters parameters;
        private IntrinsicCameraParameters icp;
        private Size imageSize;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Initialize(DistortionParameters parameters, Size imageSize)
        {
            this.parameters = parameters;
            this.imageSize = imageSize;
            icp = parameters.IntrinsicCameraParameters;
            initialized = true;
        }

        /// <summary>
        /// Given coordinates in distorted space, returns the point in undistorted space.
        /// </summary>
        public PointF Undistort(PointF point)
        {
            if (!initialized)
                return point;

            double x = 0;
            double y = 0;

            using (Matrix<float> src = EmguHelper.ToMatrix(point))
            using (Matrix<float> dst = new Matrix<float>(1, 1, 2))
            {
                CvInvoke.cvUndistortPoints(
                    src.Ptr,
                    dst.Ptr,
                    icp.IntrinsicMatrix.Ptr,
                    icp.DistortionCoeffs.Ptr,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

                x = dst.Data[0, 0] * parameters.Fx + parameters.Cx;
                y = dst.Data[0, 1] * parameters.Fy + parameters.Cy;
            }

            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// Given coordinates in undistorted space, returns the point in distorted space.
        /// </summary>
        public PointF Distort(PointF point)
        {
            if (!initialized)
                return point;

            // Ref:
            // http://docs.opencv.org/modules/imgproc/doc/geometric_transformations.html#cv.InitUndistortRectifyMap

            // To relative coordinates
            double x = (point.X - parameters.Cx) / parameters.Fx;
            double y = (point.Y - parameters.Cy) / parameters.Fy;

            double r2 = x * x + y * y;

            //radial distorsion
            // Note: We discard k3 as it yields instability.
            double xDistort = x * (1 + parameters.K1 * r2 + parameters.K2 * r2 * r2 /*+ parameters.K3 * r2 * r2 * r2*/);
            double yDistort = y * (1 + parameters.K1 * r2 + parameters.K2 * r2 * r2 /*+ parameters.K3 * r2 * r2 * r2*/);
            
            //tangential distorsion
            xDistort = xDistort + (2 * parameters.P1 * x * y + parameters.P2 * (r2 + 2 * x * x));
            yDistort = yDistort + (parameters.P1 * (r2 + 2 * y * y) + 2 * parameters.P2 * x * y);

            // To absolute coordinates.
            xDistort = xDistort * parameters.Fx + parameters.Cx;
            yDistort = yDistort * parameters.Fy + parameters.Cy;

            PointF result = new PointF((float)xDistort, (float)yDistort);
            return result;
        }

        /// <summary>
        /// Takes the start and end point of a segment in distorted space and return 
        /// the same segment as a list of points still in distorted space.
        /// The segment is split in several subsegments that can be drawn with drawCurve.
        /// </summary>
        public List<PointF> DistortLine(PointF start, PointF end)
        {
            List<PointF> points = new List<PointF>();
            points.Add(start);

            int innerPoints = 5;
            float factor = 1.0f / ((float)innerPoints + 1);

            PointF uStart = Undistort(start);
            PointF uEnd = Undistort(end);
            Vector v = new Vector(uStart, uEnd);

            for (int i = 1; i <= innerPoints; i++)
            {
                Vector v1 = v * (i * factor);
                PointF undistorted = uStart + v1;
                PointF distorted = Distort(undistorted);
                points.Add(distorted);
            }

            points.Add(end);

            return points;
        }

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
        public Bitmap GetDistortionGrid(Color background, Color foreground, int steps)
        {
            Bitmap bmp = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format24bppRgb);

            if (!initialized)
                return bmp;
            
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias; 
            
            SolidBrush b = new SolidBrush(background);
            g.FillRectangle(b, 0, 0, imageSize.Width, imageSize.Height);
            b.Dispose();

            Pen p = new Pen(foreground, 1);

            float stepWidth = (float)imageSize.Width / steps;
            float stepHeight = (float)imageSize.Height / steps;

            for (int i = 0; i <= steps; i++)
            {
                int col = (int)Math.Min(imageSize.Width - 1, Math.Round(i * stepWidth));
                
                PointF start = new PointF(col, 0);
                PointF end = new PointF(col, imageSize.Height);

                List<PointF> points = DistortLine(start, end);
                g.DrawCurve(p, points.ToArray());
            }

            for (int i = 0; i <= steps; i++)
            {
                int row = (int)Math.Min(imageSize.Height - 1, Math.Round(i * stepHeight));
                
                PointF start = new PointF(0, row);
                PointF end = new PointF(imageSize.Width, row);

                List<PointF> points = DistortLine(start, end);
                g.DrawCurve(p, points.ToArray());
            }

            p.Dispose();

            return bmp;
        }

        public Bitmap GetUndistortedImage(Bitmap sourceImage)
        {
            // The source image is possibly at reduced size, we need to upscale it for the map to work, 
            // as it's based on the coefficients computed for the full size.

            Matrix<float> mapx;
            Matrix<float> mapy;
            icp.InitUndistortMap(imageSize.Width, imageSize.Height, out mapx, out mapy);

            Bitmap source = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(source);
            g.DrawImage(sourceImage, 0, 0, imageSize.Width, imageSize.Height);

            BitmapData sourceImageData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
            Image<Bgr, Byte> cvSource = new Image<Bgr, Byte>(sourceImageData.Width, sourceImageData.Height, sourceImageData.Stride, sourceImageData.Scan0);

            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            BitmapData resultImageData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadOnly, result.PixelFormat);
            Image<Bgr, Byte> cvResult = new Image<Bgr, Byte>(resultImageData.Width, resultImageData.Height, resultImageData.Stride, resultImageData.Scan0);

            CvInvoke.cvRemap(cvSource, cvResult, mapx, mapy, 0, new MCvScalar(0));

            source.UnlockBits(sourceImageData);
            result.UnlockBits(resultImageData);

            return result;
        }
    }

}
