using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds axes, grid lines and tick marks for the coordinate system drawings (basic or perspective).
    /// Clips to image boundaries and account for vanishing points, lines at infinity and lines behind the camera.
    /// All coordinates found are in rectified image space.
    /// </summary>
    public static class CoordinateSystemGridFinder
    {
        private static Random random = new Random();

        public static CoordinateSystemGrid Find(CalibrationHelper calibrationHelper)
        {
            return FindForPlaneCalibration(calibrationHelper);
        }

        private static CoordinateSystemGrid FindForPlaneCalibration(CalibrationHelper calibrationHelper)
        {
            CoordinateSystemGrid grid = new CoordinateSystemGrid();
            RectangleF imageBounds = new RectangleF(PointF.Empty, calibrationHelper.ImageSize);
            RectangleF clipWindow = imageBounds;

            CalibratorPlane calibrator = calibrationHelper.CalibrationByPlane_GetCalibrator();
            RectangleF plane = new RectangleF(PointF.Empty, calibrator.Size);

            int targetSteps = 15;
            float stepVertical = 1.0f;
            float stepHorizontal = 1.0f;

            // The extended plane is used for vanishing point replacement and iteration stop condition.
            QuadrilateralF extendedPlane;
            bool orthogonal;

            if (!calibrator.QuadImage.IsAxisAlignedRectangle)
            {
                // If perspective plane, define as 2n times the nominal plane centered on origin.
                orthogonal = false;
                float n = 4;
                PointF a = new PointF(-calibrator.Size.Width * n, calibrator.Size.Height * n);
                PointF b = new PointF(calibrator.Size.Width * n, calibrator.Size.Height * n);
                PointF c = new PointF(calibrator.Size.Width * n, -calibrator.Size.Height * n);
                PointF d = new PointF(-calibrator.Size.Width * n, -calibrator.Size.Height * n);
                extendedPlane = new QuadrilateralF(a, b, c, d);

                QuadrilateralF quadImage = calibrator.QuadImage;

                float projectedWidthLength = GeometryHelper.GetDistance(quadImage.A, quadImage.B);
                float scaledTargetHorizontal = targetSteps / (calibrationHelper.ImageSize.Width / projectedWidthLength);
                stepHorizontal = RangeHelper.FindUsableStepSize(plane.Width, scaledTargetHorizontal);

                float projectedHeightLength = GeometryHelper.GetDistance(quadImage.A, quadImage.D);
                float scaledTargetVertical = targetSteps / (calibrationHelper.ImageSize.Height / projectedHeightLength);
                stepVertical = RangeHelper.FindUsableStepSize(plane.Height, scaledTargetVertical);
            }
            else
            {
                // If flat plane we know there is no way to get any vanishing point inside the image,
                // so we can safely use the whole image reprojection as an extended plane.
                orthogonal = true;
                QuadrilateralF quadImageBounds = new QuadrilateralF(imageBounds);
                PointF a = calibrationHelper.GetPointFromRectified(quadImageBounds.A);
                PointF b = calibrationHelper.GetPointFromRectified(quadImageBounds.B);
                PointF c = calibrationHelper.GetPointFromRectified(quadImageBounds.C);
                PointF d = calibrationHelper.GetPointFromRectified(quadImageBounds.D);
                extendedPlane = new QuadrilateralF(a, b, c, d);

                float width = extendedPlane.B.X - extendedPlane.A.X;
                stepHorizontal = RangeHelper.FindUsableStepSize(width, targetSteps);

                float height = extendedPlane.A.Y - extendedPlane.D.Y;
                stepVertical = RangeHelper.FindUsableStepSize(height, targetSteps);
            }

            //-------------------------------------------------------------------------------------------------
            // There is a complication with points behind the camera, as they projects above the vanishing line.
            // The general strategy is the following:
            // Find out if the vanishing point is inside the image. Reminder: parallel lines share the same vanishing point.
            // If it is not inside the image, there is no risk, so we take two points on the line and draw an infinite line that we clip against the image bounds.
            // If it is inside the image:
            // Take two points on the line and project them in image space. They are colinear with the vanishing point in image space.
            // Find on which side of the quadrilateral the vanishing point is.
            // If the vanishing point is above the quad, draw a ray from the top of the extended quad, down to infinity.
            // If the vanishing point is below the quad, draw a ray from the bottom of the extended quad, up to infinity.
            //
            // Stepping strategy:
            // We start at origin and progress horizontally and vertically until we find a gridline that is completely clipped out.
            //-------------------------------------------------------------------------------------------------

            // Vertical lines.
            PointF yVanish = new PointF(0, float.MinValue);
            bool yVanishVisible = false;
            Vector3 yv = calibrator.Project(new Vector3(0, 1, 0));
            if (yv.Z != 0)
            {
                yVanish = new PointF(yv.X / yv.Z, yv.Y / yv.Z);
                yVanishVisible = clipWindow.Contains(yVanish);
            }

            CreateVerticalGridLines(grid, 0, -stepHorizontal, calibrator, clipWindow, plane, extendedPlane, orthogonal, yVanishVisible, yVanish);
            CreateVerticalGridLines(grid, stepHorizontal, stepHorizontal, calibrator, clipWindow, plane, extendedPlane, orthogonal, yVanishVisible, yVanish);

            // Horizontal lines
            PointF xVanish = new PointF(float.MinValue, 0);
            bool xVanishVisible = false;
            Vector3 xv = calibrator.Project(new Vector3(1, 0, 0));
            if (xv.Z != 0)
            {
                xVanish = new PointF(xv.X / xv.Z, xv.Y / xv.Z);
                xVanishVisible = clipWindow.Contains(xVanish);
            }

            CreateHorizontalGridLines(grid, 0, -stepVertical, calibrator, clipWindow, plane, extendedPlane, orthogonal, xVanishVisible, xVanish);
            CreateHorizontalGridLines(grid, stepVertical, stepVertical, calibrator, clipWindow, plane, extendedPlane, orthogonal, xVanishVisible, xVanish);

            return grid;
        }

        private static void CreateVerticalGridLines(CoordinateSystemGrid grid, float start, float step, CalibratorPlane calibrator, RectangleF clipWindow, RectangleF plane, QuadrilateralF extendedPlane, bool orthogonal, bool vanishVisible, PointF vanish)
        {
            // Progress from origin to the side until grid lines are no longer visible when projected on image.
            float x = start;
            bool partlyVisible = true;
            while (partlyVisible && x >= extendedPlane.A.X && x <= extendedPlane.B.X)
            {
                Vector3 pa = calibrator.Project(new PointF(x, 0));
                Vector3 pb = calibrator.Project(new PointF(x, plane.Height));

                // Discard line if one of the points is behind the camera.
                if (pa.Z < 0 || pb.Z < 0)
                {
                    x += step;
                    continue;
                }

                PointF a = new PointF(pa.X / pa.Z, pa.Y / pa.Z);
                PointF b = new PointF(pb.X / pb.Z, pb.Y / pb.Z);

                ClipResult result;

                if (vanishVisible)
                {
                    float scale = (b.X - a.X) / (vanish.X - a.X);

                    if (scale > 0)
                    {
                        // Vanishing point is above b.
                        PointF vb = calibrator.Untransform(new PointF(x, extendedPlane.A.Y));
                        result = LiangBarsky.ClipLine(clipWindow, a, vb, double.MinValue, 1);
                    }
                    else
                    {
                        // Vanishing point is under a.
                        PointF va = calibrator.Untransform(new PointF(x, extendedPlane.D.Y));
                        result = LiangBarsky.ClipLine(clipWindow, va, b, 0, double.MaxValue);
                    }
                }
                else
                {
                    result = LiangBarsky.ClipLine(clipWindow, a, b);
                }

                partlyVisible = result.Visible;

                if (partlyVisible)
                {
                    TextAlignment textAlignment;

                    if (x == 0)
                    {
                        grid.VerticalAxis = new GridAxis(result.A, result.B);
                        textAlignment = TextAlignment.BottomRight;
                    }
                    else
                    {
                        grid.GridLines.Add(new GridLine(result.A, result.B));
                        textAlignment = TextAlignment.Bottom;
                    }

                    if (clipWindow.Contains(a) && (orthogonal || TickMarkVisible((int)(x / step))))
                        grid.TickMarks.Add(new TickMark(x, a, textAlignment));
                }

                x += step;
            }
        }

        private static void CreateHorizontalGridLines(CoordinateSystemGrid grid, float start, float step, CalibratorPlane calibrator, RectangleF clipWindow, RectangleF plane, QuadrilateralF extendedPlane, bool orthogonal, bool vanishVisible, PointF vanish)
        {
            // Progress from origin to the side until grid lines are no longer visible when projected on image.
            float y = start;
            bool partlyVisible = true;
            while (partlyVisible && y >= extendedPlane.D.Y && y <= extendedPlane.A.Y)
            {
                Vector3 pa = calibrator.Project(new PointF(0, y));
                Vector3 pb = calibrator.Project(new PointF(plane.Width, y));

                // Discard line if one of the points is behind the camera.
                if (pa.Z < 0 || pb.Z < 0)
                {
                    y += step;
                    continue;
                }

                PointF a = new PointF(pa.X / pa.Z, pa.Y / pa.Z);
                PointF b = new PointF(pb.X / pb.Z, pb.Y / pb.Z);

                ClipResult result;

                if (vanishVisible)
                {
                    float scale = (b.X - a.X) / (vanish.X - a.X);

                    if (scale > 0)
                    {
                        // Vanishing point is after b.
                        PointF vb = calibrator.Untransform(new PointF(extendedPlane.B.X, y));
                        result = LiangBarsky.ClipLine(clipWindow, a, vb, double.MinValue, 1);
                    }
                    else
                    {
                        // Vanishing point is before a.
                        PointF va = calibrator.Untransform(new PointF(extendedPlane.A.X, y));
                        result = LiangBarsky.ClipLine(clipWindow, va, b, 0, double.MaxValue);
                    }
                }
                else
                {
                    result = LiangBarsky.ClipLine(clipWindow, a, b);
                }

                partlyVisible = result.Visible;

                if (partlyVisible)
                {
                    if (y == 0)
                    {
                        grid.HorizontalAxis = new GridAxis(result.A, result.B);
                    }
                    else
                    {
                        grid.GridLines.Add(new GridLine(result.A, result.B));

                        if (clipWindow.Contains(a) && (orthogonal || TickMarkVisible((int)(y/step))))
                            grid.TickMarks.Add(new TickMark(y, a, TextAlignment.Left));
                    }
                }

                y += step;
            }
        }

        private static bool TickMarkVisible(int step)
        {
            // Decimate some tickmarks in the distance to avoid crowding the plot.
            // Uses a somewhat arbitrary strategy to give increasing spacing: above the fifth gridline away from origin, we only keep lines which are perfect squares.
            return (step <= 5) || Math.Sqrt(step) % 1 == 0;
        }

        private static Vector3 LineFromPoints(PointF p1, PointF p2)
        {
            double a = p1.Y - p2.Y;
            double b = p2.X - p1.X;
            double c = (p1.X - p2.X) * p1.Y + (p2.Y - p1.Y) * p1.X;
            return new Vector3((float)a, (float)b, (float)c);
        }

        private static QuadrilateralF ReprojectImageBounds(CalibrationHelper calibrationHelper, QuadrilateralF quadImage)
        {
            // Project image bounds from image space to plane space, using the line calibration setup.
            // This can be used to define safe boundaries.
            // The method cannot be used with plane calibration as image points can be above the vanishing line. (reproject behind the camera).
            // The quad passed in is assumed to be a rectangle.
            if (calibrationHelper.CalibratorType != CalibratorType.Line)
                throw new ArgumentException("Unsupported operation.");

            PointF a = calibrationHelper.GetPointFromRectified(quadImage.A);
            PointF b = calibrationHelper.GetPointFromRectified(quadImage.B);
            PointF c = calibrationHelper.GetPointFromRectified(quadImage.C);
            PointF d = calibrationHelper.GetPointFromRectified(quadImage.D);
            QuadrilateralF plane = new QuadrilateralF(a, b, c, d);

            return plane;
        }

        #region Test utils
        private static void ExportImage(CalibrationHelper calibrationHelper, CoordinateSystemGrid grid)
        {
            //Bitmap bitmap = new Bitmap((int)(calibrationHelper.ImageSize.Width + 100), (int)(calibrationHelper.ImageSize.Height + 100), PixelFormat.Format24bppRgb);
            Bitmap bitmap = new Bitmap((int)(calibrationHelper.ImageSize.Width), (int)(calibrationHelper.ImageSize.Height), PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bitmap);

            g.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);

            if (grid.VerticalAxis != null)
                g.DrawLine(Pens.Blue, grid.VerticalAxis.Start, grid.VerticalAxis.End);

            if (grid.HorizontalAxis != null)
                g.DrawLine(Pens.Blue, grid.HorizontalAxis.Start, grid.HorizontalAxis.End);

            foreach (GridLine line in grid.GridLines)
                g.DrawLine(Pens.Red, line.Start, line.End);

            string filename = GetRandomString(10);
            bitmap.Save(Path.Combine(@"", string.Format("{0}.png", filename)));
        }

        private static QuadrilateralF GetRandomQuadrilateral(Rectangle window)
        {
            // A vertex in each quadrant.
            // Does not guarantee convexity nor non-colinearity.
            PointF a = GetRandomPoint(window.Left, window.Left + window.Width / 2, window.Top, window.Top + window.Height / 2);
            PointF b = GetRandomPoint(window.Left + window.Width / 2, window.Right, window.Top, window.Top + window.Height / 2);
            PointF c = GetRandomPoint(window.Left + window.Width / 2, window.Right, window.Top + window.Height / 2, window.Bottom);
            PointF d = GetRandomPoint(window.Left, window.Left + window.Width / 2, window.Top + window.Height / 2, window.Bottom);

            return new QuadrilateralF(a, b, c, d);
        }

        private static PointF GetRandomPoint(int left, int right, int top, int bottom)
        {
            return new PointF(random.Next(left, right), random.Next(top, bottom));
        }

        public static string GetRandomString(int length)
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
