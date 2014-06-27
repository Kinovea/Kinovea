using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Math;
using System.Drawing.Imaging;
using System.IO;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds axes and tick lines in perspective, clipped against the view window.
    /// This class is used in the context of drawing the coordinate system.
    /// </summary>
    public class PlaneFiller
    {
        private CalibrationHelper calibrationHelper;
        private RectangleF imageRectangle;
        private Random random = new Random();

        public PlaneFiller(CalibrationHelper calibrationHelper)
        {
            this.calibrationHelper = calibrationHelper;
            this.imageRectangle = new RectangleF(Point.Empty, calibrationHelper.ImageSize);
        }

        public void Fill()
        {
            // WIP.
            // This method will ultimately return a complete object representing the coordinate system axes and tick lines.

            if (calibrationHelper.CalibratorType != CalibratorType.Plane)
                return;

            Rectangle window = new Rectangle(50, 50, (int)imageRectangle.Width, (int)imageRectangle.Height);
            Bitmap bitmap = new Bitmap((int)(imageRectangle.Width + 100), (int)(imageRectangle.Height + 100), PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bitmap);

            g.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            g.DrawRectangle(Pens.Black, window);

            // We work directly with the transform in order to stay in rectified space (no lens distortion) 
            // and use the methods working with homogenous coordinates.
            ProjectiveMapping projectivity = calibrationHelper.CalibrationByPlane_GetProjectiveMapping();

            // Test
            QuadrilateralF quad = GetRandomQuadrilateral(window);
            projectivity.Update(new QuadrilateralF(100, 100), quad);

            //PointF o = projectivity.Forward(PointF.Empty);
            //g.DrawEllipse(Pens.Blue, o.Box(5));

            // Define the plane bounds.
            // FIXME: even at a scale of 3 we sometimes go too far in the projection and end up on the other side of the camera.

            SizeF size = calibrationHelper.CalibrationByPlane_GetRectangleSize();

            float scaler = 3;
            float width = size.Width * scaler;
            float height = size.Height * scaler;
            PointF center = new PointF(size.Width / 2, size.Height / 2);
            float left = center.X - width / 2;
            float top = center.Y - height / 2;
            float bottom = center.Y + height / 2;
            float right = center.X + width / 2;

            int steps = 20; // Todo: find best stepping with the other algo.
            float stepWidth = width / steps;
            float stepHeight = height / steps;

            //-------------------------------------------------------------------------------------------------
            // There is a complication with points behind the camera, as they projects above the vanishing line.
            // The general strategy is the following:
            // Find out whether the vanishing point is inside the image. The vanishing point is the same for parallel lines.
            // If it is not inside the image, there is no risk, so we take two points on the line and draw an infinite line.
            // If it is inside the image:
            // Project two secure points, points guaranteed to be in front of the camera, we use points at the intersection between the line and the quadrilateral.
            // Projected in the image, these two points and the vanishing point are colinear.
            // Find on which side of the quadrilateral the vanishing point is.
            // If the vanishing point is above the quad, draw a ray from the top of the quad to infinity.
            // If the vanishing point is below the quad, draw a ray from the bottom of the quad to infinity.
            //
            // Issues: when the vanishing point is just outside the image it gives converging lines, not pretty.
            //-------------------------------------------------------------------------------------------------

            // y lines.
            
            // Check if vanishing points is visible.
            PointF yVanish = new PointF(0, float.MinValue);
            Vector3 yv = projectivity.Forward(new Vector3(0, 1, 0));
            if (yv.Z != 0)
                yVanish = new PointF(yv.X / yv.Z, yv.Y / yv.Z);

            bool yVanishVisible = window.Contains(yVanish.ToPoint());

            for (int i = 0; i < steps; i++)
            {
                float x = left + i * stepWidth;

                // Project two secure points.
                PointF a = projectivity.Forward(new PointF(x, 0));
                PointF b = projectivity.Forward(new PointF(x, size.Height));

                //g.DrawEllipse(Pens.Red, a.Box(3));
                //g.DrawEllipse(Pens.Red, b.Box(3));

                if (yVanishVisible)
                {
                    float scale = (b.X - a.X) / (yVanish.X - a.X);

                    if (scale > 0)
                    {
                        // Vanishing point is after b.
                        b = projectivity.Forward(new PointF(x, bottom));
                        
                        ClipResult result = LiangBarsky.ClipLine(window, a, b, double.MinValue, 1);

                        if (result.Visible)
                            g.DrawLine(Pens.Red, result.A, result.B);
                    }
                    else
                    {
                        // Vanishing point is before a.
                        a = projectivity.Forward(new PointF(x, top));

                        ClipResult result = LiangBarsky.ClipLine(window, a, b, 0, double.MaxValue);

                        if (result.Visible)
                            g.DrawLine(Pens.Red, result.A, result.B);
                    }
                }
                else
                {
                    ClipResult result = LiangBarsky.ClipLine(window, a, b);

                    if (result.Visible)
                        g.DrawLine(Pens.Red, result.A, result.B);
                }
            }
            

            // x lines

            // Check if vanishing points is visible.
            PointF xVanish = new PointF(float.MinValue, 0);
            Vector3 xv = projectivity.Forward(new Vector3(1, 0, 0));
            if (xv.Z != 0)
                xVanish = new PointF(xv.X / xv.Z, xv.Y / xv.Z);

            bool xVanishVisible = window.Contains(xVanish.ToPoint());

            for (int i = 0; i < steps; i++)
            {
                float y = top + i * stepHeight;

                // Project two secure points
                PointF a = projectivity.Forward(new PointF(0, y));
                PointF b = projectivity.Forward(new PointF(size.Width, y));

                if (xVanishVisible)
                {
                    float scale = (b.X - a.X) / (yVanish.X - a.X);

                    if (scale > 0)
                    {
                        // Vanishing point is after b.
                        b = projectivity.Forward(new PointF(right, y));

                        ClipResult result = LiangBarsky.ClipLine(window, a, b, double.MinValue, 1);

                        if (result.Visible)
                            g.DrawLine(Pens.Red, result.A, result.B);
                    }
                    else
                    {
                        // Vanishing point is before a.
                        a = projectivity.Forward(new PointF(left, y));

                        ClipResult result = LiangBarsky.ClipLine(window, a, b, 0, double.MaxValue);

                        if (result.Visible)
                            g.DrawLine(Pens.Red, result.A, result.B);
                    }
                }
                else
                {
                    ClipResult result = LiangBarsky.ClipLine(window, a, b);

                    if (result.Visible)
                        g.DrawLine(Pens.Red, result.A, result.B);   
                }
            }

            string filename = GetRandomString(10);
            bitmap.Save(Path.Combine(@"C:\Users\Joan\Videos\Kinovea\Video Testing\Projective\infinite plane\rnd", string.Format("{0}.png", filename)));
        }

        private Vector3 LineFromPoints(PointF p1, PointF p2)
        {
            double a = p1.Y - p2.Y;
            double b = p2.X - p1.X;
            double c = (p1.X - p2.X) * p1.Y + (p2.Y - p1.Y) * p1.X;
            return new Vector3((float)a, (float)b, (float)c);
        }

        private static float RulerStepSize(float range, float targetSteps)
        {
            float minimum = range / targetSteps;

            // Find magnitude of the initial guess.
            float magnitude = (float)Math.Floor(Math.Log10(minimum));
            float orderOfMagnitude = (float)Math.Pow(10, magnitude);

            // Reduce the number of steps.
            float residual = minimum / orderOfMagnitude;
            float stepSize;

            if (residual > 5)
                stepSize = 10 * orderOfMagnitude;
            else if (residual > 2)
                stepSize = 5 * orderOfMagnitude;
            else if (residual > 1)
                stepSize = 2 * orderOfMagnitude;
            else
                stepSize = orderOfMagnitude;

            return stepSize;
        }

        #region Test utils
        private QuadrilateralF GetRandomQuadrilateral(Rectangle window)
        {
            // A corner in each quadrant.
            // Does not guarantee convexity nor non-colinearity.
            PointF a = GetRandomPoint(window.Left, window.Left + window.Width / 2, window.Top, window.Top + window.Height / 2);
            PointF b = GetRandomPoint(window.Left + window.Width / 2, window.Right, window.Top, window.Top + window.Height / 2);
            PointF c = GetRandomPoint(window.Left + window.Width / 2, window.Right, window.Top + window.Height / 2, window.Bottom);
            PointF d = GetRandomPoint(window.Left, window.Left + window.Width / 2, window.Top + window.Height / 2, window.Bottom);

            return new QuadrilateralF(a, b, c, d);
        }

        private PointF GetRandomPoint(int left, int right, int top, int bottom)
        {
            return new PointF(random.Next(left, right), random.Next(top, bottom));
        }

        public string GetRandomString(int length)
        {
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string res = "";
            while (0 < length--)
                res += valid[random.Next(valid.Length)];
            return res;
        }
        #endregion
    }
}
