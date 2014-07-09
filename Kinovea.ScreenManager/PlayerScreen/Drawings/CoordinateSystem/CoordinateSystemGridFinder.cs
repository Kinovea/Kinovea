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
    /// Finds axes and grid lines end points for coordinate system drawings (basic or perspective), clipped to image boundaries.
    /// </summary>
    public static class CoordinateSystemGridFinder
    {
        private static Random random = new Random();
        
        public static CoordinateSystemGrid Find(CalibrationHelper calibrationHelper)
        {
            switch(calibrationHelper.CalibratorType)
            {
                case CalibratorType.Line:
                    return FindForLineCalibration(calibrationHelper);
                case CalibratorType.Plane:
                    return FindForPlaneCalibration(calibrationHelper);
            }

            return null;
        }

        private static CoordinateSystemGrid FindForLineCalibration(CalibrationHelper calibrationHelper)
        {
            CoordinateSystemGrid grid = new CoordinateSystemGrid();
            RectangleF imageBounds = new RectangleF(PointF.Empty, calibrationHelper.ImageSize);
            RectangleF clipWindow = imageBounds;

            // Create a fake plane to act as the user-defined projected plane.
            QuadrilateralF quadImage = new QuadrilateralF(imageBounds.Deflate(2.0f));
            PointF a = calibrationHelper.GetPoint(quadImage.A);
            PointF b = calibrationHelper.GetPoint(quadImage.B);
            PointF d = calibrationHelper.GetPoint(quadImage.D);
            RectangleF plane = new RectangleF(0, 0, b.X - a.X, a.Y - d.Y);

            // Define the extended plane as the reprojection of the whole image. (used for vanishing point replacement and drawing stop condition).
            QuadrilateralF extendedPlane = ReprojectImageBounds(calibrationHelper, new QuadrilateralF(imageBounds));
            
            CalibrationPlane calibrator = new CalibrationPlane();
            calibrator.Initialize(plane.Size, quadImage);
            calibrator.SetOrigin(calibrationHelper.GetOrigin());
            
            // From this point on we are mostly in the same situation as for plane calibration.
            
            // stepping is the same in both directions.
            int targetSteps = 15;
            float width = extendedPlane.B.X - extendedPlane.A.X;
            float step = RangeHelper.FindUsableStepSize(width, targetSteps);

            CreateVerticalGridLines(grid, 0, -step, calibrator, clipWindow, plane, extendedPlane, false, PointF.Empty);
            CreateVerticalGridLines(grid, step, step, calibrator, clipWindow, plane, extendedPlane, false, PointF.Empty);
            CreateHorizontalGridLines(grid, 0, -step, calibrator, clipWindow, plane, extendedPlane, false, PointF.Empty);
            CreateHorizontalGridLines(grid, step, step, calibrator, clipWindow, plane, extendedPlane, false, PointF.Empty);

            //ExportImage(calibrationHelper, grid);
            return grid;
        }

        private static CoordinateSystemGrid FindForPlaneCalibration(CalibrationHelper calibrationHelper)
        {
            CoordinateSystemGrid grid = new CoordinateSystemGrid();
            RectangleF imageBounds = new RectangleF(PointF.Empty, calibrationHelper.ImageSize);
            RectangleF clipWindow = imageBounds;

            CalibrationPlane calibrator = calibrationHelper.CalibrationByPlane_GetCalibrator();

            RectangleF plane = new RectangleF(PointF.Empty, calibrator.Size);

            int targetSteps = 15;
            float stepVertical = 1.0f;
            float stepHorizontal = 1.0f;
            
            QuadrilateralF extendedPlane;
            if (!calibrator.QuadImage.IsRectangle)
            {
                // If perspective plane, define as 3 times the nominal plane, shifted by half the user plane.
                // FIXME: find better. This still looks ugly without preventing behind the camera projection.
                PointF a = new PointF(-calibrator.Size.Width, calibrator.Size.Height * 2);
                PointF b = new PointF(calibrator.Size.Width * 2, calibrator.Size.Height * 2);
                PointF c = new PointF(calibrator.Size.Width * 2, -calibrator.Size.Height);
                PointF d = new PointF(-calibrator.Size.Width, -calibrator.Size.Height);
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
                // In that case we know there is no way to get vanishing point inside the image so we can safely 
                // use the whole image reprojection as an extended plane.
                QuadrilateralF quadImageBounds = new QuadrilateralF(imageBounds);
                PointF a = calibrationHelper.GetPoint(quadImageBounds.A);
                PointF b = calibrationHelper.GetPoint(quadImageBounds.B);
                PointF c = calibrationHelper.GetPoint(quadImageBounds.C);
                PointF d = calibrationHelper.GetPoint(quadImageBounds.D);
                extendedPlane = new QuadrilateralF(a, b, c, d);

                float width = extendedPlane.B.X - extendedPlane.A.X;
                stepHorizontal = RangeHelper.FindUsableStepSize(width, targetSteps);
                
                float height = extendedPlane.A.Y - extendedPlane.D.Y;
                stepVertical = RangeHelper.FindUsableStepSize(height, targetSteps);
            }
            
            // Stepping strategy:
            // We start at origin and progress horizontally and vertically until we find a gridline that is completely clipped out.
            // Since we work in undistorted coordinates, we may miss some lines that are only visible when because they bend in.

            
            //-------------------------------------------------------------------------------------------------
            // There is a complication with points behind the camera, as they projects above the vanishing line.
            // The general strategy is the following:
            // Find out if the vanishing point is inside the image. Reminder: parallel lines share the same vanishing point.
            // If it is not inside the image, there is no risk, so we take two points on the line and draw an infinite line that we clip against the image bounds.
            // If it is inside the image:
            // Project two secure points, points guaranteed to be in front of the camera, we use the dimensions of the quad and the axis to find suitable points.
            // Projected in the image, these two points and the vanishing point are colinear. 
            // Find on which side of the quadrilateral the vanishing point is.
            // If the vanishing point is above the quad, draw a ray from the top of the quad to infinity.
            // If the vanishing point is below the quad, draw a ray from the bottom of the quad to infinity.
            //
            // Issues: 
            // 1. when the vanishing point is just outside the image it gives converging lines, not pretty.
            // 2. The extended quad, used for drawing stop condition can sometimes lie behind the camera too.
            //-------------------------------------------------------------------------------------------------

            // Vertical lines.
            PointF yVanish = new PointF(0, float.MinValue);
            bool yVanishVisible = false;
            Vector3 yv = calibrator.Project(new Vector3(0, 1, 0));
            if (yv.Z != 0)
            {
                yVanish = new PointF(yv.X / yv.Z, yv.Y / yv.Z);
                yVanishVisible = clipWindow.Contains(yVanish.ToPoint());
            }

            CreateVerticalGridLines(grid, 0, -stepHorizontal, calibrator, clipWindow, plane, extendedPlane, yVanishVisible, yVanish);
            CreateVerticalGridLines(grid, stepHorizontal, stepHorizontal, calibrator, clipWindow, plane, extendedPlane, yVanishVisible, yVanish);

            // Horizontal lines
            PointF xVanish = new PointF(float.MinValue, 0);
            bool xVanishVisible = false;
            Vector3 xv = calibrator.Project(new Vector3(1, 0, 0));
            if (xv.Z != 0)
            {
                xVanish = new PointF(xv.X / xv.Z, xv.Y / xv.Z);
                xVanishVisible = clipWindow.Contains(xVanish.ToPoint());
            }

            CreateHorizontalGridLines(grid, 0, -stepVertical, calibrator, clipWindow, plane, extendedPlane, xVanishVisible, xVanish);
            CreateHorizontalGridLines(grid, stepVertical, stepVertical, calibrator, clipWindow, plane, extendedPlane, xVanishVisible, xVanish);

            //ExportImage(calibrationHelper, grid);
            return grid;
        }

        private static void CreateVerticalGridLines(CoordinateSystemGrid grid, float start, float step, CalibrationPlane calibrator, RectangleF clipWindow, RectangleF plane, QuadrilateralF extendedPlane, bool vanishVisible, PointF vanish)
        {
            // Progress from origin to the side until grid lines are no longer visible when projected on image.
            float x = start;
            bool partlyVisible = true;
            while (partlyVisible && x >= extendedPlane.A.X && x <= extendedPlane.B.X)
            {
                PointF a = calibrator.Untransform(new PointF(x, 0));
                PointF b = calibrator.Untransform(new PointF(x, plane.Height));

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

                    if (clipWindow.Contains(a))
                        grid.TickMarks.Add(new TickMark(x, a, textAlignment));
                }

                x += step;
            }
        }

        private static void CreateHorizontalGridLines(CoordinateSystemGrid grid, float start, float step, CalibrationPlane calibrator, RectangleF clipWindow, RectangleF plane, QuadrilateralF extendedPlane, bool vanishVisible, PointF vanish)
        {
            // Progress from origin to the side until grid lines are no longer visible when projected on image.
            float y = start;
            bool partlyVisible = true;
            while (partlyVisible && y >= extendedPlane.D.Y && y <= extendedPlane.A.Y)
            {
                // Project two secure points.
                PointF a = calibrator.Untransform(new PointF(0, y));
                PointF b = calibrator.Untransform(new PointF(plane.Width, y));

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

                        if (clipWindow.Contains(a))
                            grid.TickMarks.Add(new TickMark(y, a, TextAlignment.Left));
                    }
                }

                y += step;
            }
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
            // Project image bounds from image coordinates to plane coordinates, using the line calibration setup. 
            // This can be used to define safe boundaries.
            // The method cannot be used with plane calibration as image points can be above the vanishing line. (reproject behind the camera).
            // The quad passed in is assumed to be a rectangle.
            if (calibrationHelper.CalibratorType != CalibratorType.Line)
                throw new ArgumentException("Unsupported operation.");

            PointF a = calibrationHelper.GetPoint(quadImage.A);
            PointF b = calibrationHelper.GetPoint(quadImage.B);
            PointF c = calibrationHelper.GetPoint(quadImage.C);
            PointF d = calibrationHelper.GetPoint(quadImage.D);
            QuadrilateralF plane = new QuadrilateralF(a, b, c, d);
            
            return plane;
        }

        #region Test utils
        private static void ExportImage(CalibrationHelper calibrationHelper, CoordinateSystemGrid grid)
        {
            // Test
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
            bitmap.Save(Path.Combine(@"C:\Users\Joan\Videos\Kinovea\Video Testing\Projective\infinite plane\rnd", string.Format("{0}.png", filename)));
        }

        private static QuadrilateralF GetRandomQuadrilateral(Rectangle window)
        {
            // A corner in each quadrant.
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
