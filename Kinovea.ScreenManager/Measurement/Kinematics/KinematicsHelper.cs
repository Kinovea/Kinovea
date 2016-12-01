#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Linq;
using System.Text;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;

namespace Kinovea.ScreenManager
{
    public class KinematicsHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TrajectoryKinematics AnalyzeTrajectory(List<TimedPoint> samples, CalibrationHelper calibrationHelper)
        {
            TrajectoryKinematics kinematics = new TrajectoryKinematics();
            
            if (samples.Count == 0)
                return kinematics;

            kinematics.Initialize(samples.Count);

            ComputeRawCoordinates(kinematics, samples, calibrationHelper);
            ComputeRawTotalDistance(kinematics, calibrationHelper);
            ComputeRawVelocities(kinematics, calibrationHelper);
            ComputeRawAccelerations(kinematics, calibrationHelper);

            if (kinematics.CanFilter)
            {
                ComputeFilteredCoordinates(kinematics, calibrationHelper);
                ComputeTotalDistance(kinematics, calibrationHelper);
                ComputeVelocities(kinematics, calibrationHelper);
                ComputeAccelerations(kinematics, calibrationHelper);
            }
            else
            {
                kinematics.ForceRawSeries();
            }

            try
            {
                // Angular kinematics based on the best fit circle.
                ComputeRotationCircle(kinematics, calibrationHelper);
                ComputeDisplacementAngle(kinematics, calibrationHelper);
                ComputeAngularVelocities(kinematics, calibrationHelper);
                ComputeAngularAccelerations(kinematics, calibrationHelper);
            }
            catch(Exception e)
            {
                log.ErrorFormat("Error while computing angular kinematics from best fit circle on trajectory.");
                log.ErrorFormat(e.ToString());
            }
            
            return kinematics;
        }

        private void ComputeRawCoordinates(TrajectoryKinematics kinematics, List<TimedPoint> input, CalibrationHelper calibrationHelper)
        {
            for (int i = 0; i < input.Count; i++)
            {
                PointF point = calibrationHelper.GetPointAtTime(input[i].Point, input[i].T);
                kinematics.RawXs[i] = point.X;
                kinematics.RawYs[i] = point.Y;
                kinematics.Times[i] = input[i].T;
            }
        }

        private void ComputeFilteredCoordinates(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            double framerate = calibrationHelper.CaptureFramesPerSecond;

            ButterworthFilter filter = new ButterworthFilter();
            int bestCutoffIndex;

            kinematics.FilterResultXs = filter.FilterSamples(kinematics.RawXs, framerate, 100, out bestCutoffIndex);
            kinematics.XCutoffIndex = bestCutoffIndex;

            kinematics.FilterResultYs = filter.FilterSamples(kinematics.RawYs, framerate, 100, out bestCutoffIndex);
            kinematics.YCutoffIndex = bestCutoffIndex;
        }

        private void ComputeRawTotalDistance(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            float distance = 0;
            kinematics.RawTotalDistance[0] = distance;

            for (int i = 1; i < kinematics.Length; i++)
            {
                PointF a = kinematics.RawCoordinates(i - 1);
                PointF b = kinematics.RawCoordinates(i);
                float d = GeometryHelper.GetDistance(a, b);
                distance += d;
                kinematics.RawTotalDistance[i] = distance;
            }
        }

        private void ComputeTotalDistance(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            float distance = 0;
            kinematics.TotalDistance[0] = distance;

            for (int i = 1; i < kinematics.Length; i++)
            {
                PointF a = kinematics.Coordinates(i - 1);
                PointF b = kinematics.Coordinates(i);
                float d = GeometryHelper.GetDistance(a, b);
                distance += d;
                kinematics.TotalDistance[i] = distance;
            }
        }

        private void ComputeRawVelocities(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 2)
            {
                PadRawVelocities(kinematics);
                return;
            }

            for (int i = 1; i < kinematics.Length - 1; i++)
            {
                PointF a = kinematics.RawCoordinates(i-1);
                PointF b = kinematics.RawCoordinates(i+1);
                float t = calibrationHelper.GetTime(2);

                kinematics.RawSpeed[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Magnitude));
                kinematics.RawHorizontalVelocity[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Horizontal));
                kinematics.RawVerticalVelocity[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Vertical));
            }

            PadRawVelocities(kinematics);
        }

        private void ComputeVelocities(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 2)
            {
                PadVelocities(kinematics);
                return;
            }

            for (int i = 1; i < kinematics.Length - 1; i++)
            {
                PointF a = kinematics.Coordinates(i - 1);
                PointF b = kinematics.Coordinates(i + 1);
                float t = calibrationHelper.GetTime(2);

                kinematics.Speed[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Magnitude));
                kinematics.HorizontalVelocity[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Horizontal));
                kinematics.VerticalVelocity[i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Vertical));
            }

            PadVelocities(kinematics);

            double constantVelocitySpan = 40;
            MovingAverage filter = new MovingAverage();
            kinematics.Speed = filter.FilterSamples(kinematics.Speed, calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);
            kinematics.HorizontalVelocity = filter.FilterSamples(kinematics.HorizontalVelocity, calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);
            kinematics.VerticalVelocity = filter.FilterSamples(kinematics.VerticalVelocity, calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);
        }

        private void ComputeRawAccelerations(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 4)
            {
                PadRawAccelerations(kinematics);
                return;
            }
            
            for (int i = 2; i < kinematics.Length - 2; i++)
            {
                PointF p0 = kinematics.RawCoordinates(i - 2);
                PointF p2 = kinematics.RawCoordinates(i);
                PointF p4 = kinematics.RawCoordinates(i + 2);
                float t02 = calibrationHelper.GetTime(2);
                float t24 = calibrationHelper.GetTime(2);
                float t13 = calibrationHelper.GetTime(2);

                kinematics.RawAcceleration[i] = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Magnitude));
                kinematics.RawHorizontalAcceleration[i] = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Horizontal));
                kinematics.RawVerticalAcceleration[i] = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Vertical));
            }

            PadRawAccelerations(kinematics);
        }

        private void ComputeAccelerations(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 4)
            {
                PadAccelerations(kinematics);
                return;
            }

            // First pass: average speed over 2t centered on each data point.
            for (int i = 2; i < kinematics.Length - 2; i++)
            {
                PointF p0 = kinematics.Coordinates(i - 2);
                PointF p2 = kinematics.Coordinates(i);
                PointF p4 = kinematics.Coordinates(i + 2);
                float t02 = calibrationHelper.GetTime(2);
                float t24 = calibrationHelper.GetTime(2);
                float t13 = calibrationHelper.GetTime(2);

                double acceleration = (kinematics.Speed[i + 1] - kinematics.Speed[i - 1]) / t13;
                kinematics.Acceleration[i] = calibrationHelper.ConvertAccelerationFromVelocity((float)acceleration);
                
                double horizontalAcceleration = (kinematics.HorizontalVelocity[i + 1] - kinematics.HorizontalVelocity[i - 1]) / t13;
                kinematics.HorizontalAcceleration[i] = calibrationHelper.ConvertAccelerationFromVelocity((float)horizontalAcceleration);

                double verticalAcceleration = (kinematics.VerticalVelocity[i + 1] - kinematics.VerticalVelocity[i - 1]) / t13;
                kinematics.VerticalAcceleration[i] = calibrationHelper.ConvertAccelerationFromVelocity((float)verticalAcceleration);
            }

            PadAccelerations(kinematics);

            double constantAccelerationSpan = 50;
            MovingAverage filter = new MovingAverage();
            kinematics.Acceleration = filter.FilterSamples(kinematics.Acceleration, calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);
            kinematics.HorizontalAcceleration = filter.FilterSamples(kinematics.HorizontalAcceleration, calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);
            kinematics.VerticalAcceleration = filter.FilterSamples(kinematics.VerticalAcceleration, calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);
        }

        private void ComputeRotationCircle(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length < 3)
                return;

            // Least-squares circle fitting.
            // Ref: "Circle fitting by linear and nonlinear least squares", Coope, I.D., 
            // Journal of Optimization Theory and Applications Volume 76, Issue 2, New York: Plenum Press, February 1993.
            // Implementation based on JS implementation: 
            // http://jsxgraph.uni-bayreuth.de/wiki/index.php/Least-squares_circle_fitting

            int rows = kinematics.Length;
            Matrix m = new Matrix(rows, 3);
            Matrix v = new Matrix(rows, 1);

            for (int i = 0; i < rows; i++)
            {
                PointF point = kinematics.Coordinates(i);
                m[i, 0] = point.X;
                m[i, 1] = point.Y;
                m[i, 2] = 1.0;
                v[i, 0] = point.X * point.X + point.Y * point.Y;
            }

            Matrix mt = m.Clone();
            mt.Transpose();
            Matrix b = mt.Multiply(m);
            Matrix c = mt.Multiply(v);
            Matrix z = b.Solve(c);

            PointF center = new PointF((float)(z[0, 0] * 0.5), (float)(z[1, 0] * 0.5));
            double radius = Math.Sqrt(z[2, 0] + (center.X * center.X) + (center.Y * center.Y));

            kinematics.RotationCenter = center;
            kinematics.RotationRadius = radius;

            for (int i = 0; i < rows; i++)
            {
                PointF xAxis = center.Translate(100.0f, 0.0f);
                float angle = GeometryHelper.GetAngle(center, xAxis, kinematics.Coordinates(i));
                if (angle < 0)
                    angle = (float)((Math.PI * 2) + angle);
                
                kinematics.AbsoluteAngle[i] = angle;
            }
        }

        private void ComputeDisplacementAngle(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            double total = 0;
            kinematics.DisplacementAngle[0] = 0;
            kinematics.TotalDisplacementAngle[0] = total;

            for (int i = 1; i < kinematics.Length; i++)
            {
                double displacement = GetDisplacementAngle(kinematics, i, i -1);
                total += displacement;
                kinematics.DisplacementAngle[i] = calibrationHelper.ConvertAngle((float)displacement);
                kinematics.TotalDisplacementAngle[i] = calibrationHelper.ConvertAngle((float)total);
            }
        }

        private void ComputeAngularVelocities(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 2)
            {
                PadAngularVelocities(kinematics);
                return;
            }

            for (int i = 1; i < kinematics.Length - 1; i++)
            {
                double d1 = GetDisplacementAngle(kinematics, i, i - 1);
                double d2 = GetDisplacementAngle(kinematics, i + 1, i);
                float time = calibrationHelper.GetTime(2);
                float inRadPerSecond = (float)((d1 + d2) / time);

                kinematics.AngularVelocity[i] = calibrationHelper.ConvertAngularVelocity(inRadPerSecond);
                kinematics.TangentialVelocity[i] = calibrationHelper.ConvertSpeed((float)(inRadPerSecond * kinematics.RotationRadius));
                kinematics.CentripetalAcceleration[i] = calibrationHelper.ConvertAcceleration((float)(inRadPerSecond * inRadPerSecond * kinematics.RotationRadius));
            }

            PadAngularVelocities(kinematics);
        }

        private void ComputeAngularAccelerations(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            if (kinematics.Length <= 4)
            {
                PadAngularAccelerations(kinematics);
                return;
            }

            for (int i = 2; i < kinematics.Length - 2; i++)
            {
                double d1 = GetDisplacementAngle(kinematics, i - 1, i - 2);
                double d2 = GetDisplacementAngle(kinematics, i, i - 1);
                double d3 = GetDisplacementAngle(kinematics, i + 1, i);
                double d4 = GetDisplacementAngle(kinematics, i + 2, i + 1);
                float t02 = calibrationHelper.GetTime(2);
                float t24 = calibrationHelper.GetTime(2);
                float t13 = calibrationHelper.GetTime(2);

                float v1 = (float)((d1 + d2) / t02);
                float v2 = (float)((d3 + d4) / t24);
                float a = (float)((v2 - v1) / t13);
                kinematics.AngularAcceleration[i] = calibrationHelper.ConvertAngularAcceleration(a);
            }

            PadAngularAccelerations(kinematics);
        }

        #region Low level
        
        private float GetDistance(PointF a, PointF b, Component component)
        {
            float d = 0;
            switch (component)
            {
                case Component.Magnitude:
                    d = GeometryHelper.GetDistance(a, b);
                    break;
                case Component.Horizontal:
                    d = b.X - a.X;
                    break;
                case Component.Vertical:
                    d = b.Y - a.Y;
                    break;
            }

            return d;
        }
        
        /// <summary>
        /// Computes instantaneous velocity between points a and b.
        /// t : time span between a and b.
        /// </summary>
        private float GetSpeed(PointF a, PointF b, float t, Component component)
        {
            float d = GetDistance(a, b, component);
            return d / t;
        }

        /// <summary>
        /// Computes instantaneous acceleration at p2.
        /// pi : point at i.
        /// tij : time span between pi and pj.
        /// </summary>
        private float GetAcceleration(PointF p0, PointF p2, PointF p4, float t02, float t24, float t13, Component component)
        {
            float v02 = GetSpeed(p0, p2, t02, component);
            float v24 = GetSpeed(p2, p4, t24, component);
            float a = (v24 - v02) / t13;
            
            return a;
        }

        /// <summary>
        /// Computes displacement angle in radians.
        /// </summary>
        private double GetDisplacementAngle(TrajectoryKinematics kinematics, int a, int b)
        {
            double displacement = Math.Abs(kinematics.AbsoluteAngle[a] - kinematics.AbsoluteAngle[b]);

            // To solve the ambiguity when moving through the x axis we keep the smallest value.
            // This assumes the motion is incrementing by small pieces rather than more than a half circle at a time.
            displacement = Math.Min(displacement, 2 * Math.PI - displacement);
            return displacement;
        }

        private void PadRawVelocities(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.RawSpeed, 1);
            PadUncomputables(kinematics.RawHorizontalVelocity, 1);
            PadUncomputables(kinematics.RawVerticalVelocity, 1);
            return;
        }

        private void PadVelocities(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.Speed, 1);
            PadUncomputables(kinematics.HorizontalVelocity, 1);
            PadUncomputables(kinematics.VerticalVelocity, 1);
            return;
        }

        private void PadRawAccelerations(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.RawAcceleration, 2);
            PadUncomputables(kinematics.RawHorizontalAcceleration, 2);
            PadUncomputables(kinematics.RawVerticalAcceleration, 2);
            return;
        }

        private void PadAccelerations(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.Acceleration, 2);
            PadUncomputables(kinematics.HorizontalAcceleration, 2);
            PadUncomputables(kinematics.VerticalAcceleration, 2);
            return;
        }

        private void PadAngularVelocities(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.AngularVelocity, 1);
            PadUncomputables(kinematics.CentripetalAcceleration, 1);
            PadUncomputables(kinematics.TangentialVelocity, 1);
        }

        private void PadAngularAccelerations(TrajectoryKinematics kinematics)
        {
            PadUncomputables(kinematics.AngularAcceleration, 2);
        }

        /// <summary>
        /// Set uncomputable end points to NaN.
        /// "padding" number of values will be NaN'ed on each side of the series.
        /// </summary>
        private void PadUncomputables(double[] values, int padding)
        {
            // At that point the values should all already be set except for the uncomputables.
            if (values.Length <= padding * 2)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = double.NaN;
            }
            else
            {
                for (int i = 0; i < padding; i++)
                {
                    values[i] = double.NaN;
                    values[values.Length - 1 - i] = double.NaN;
                }
            }
        }
        #endregion
    }
}
