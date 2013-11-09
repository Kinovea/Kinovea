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
        public List<TrajectoryPoint> ComputeValues(List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // Computes values at all points from the given trajectory.
            // Values will be expressed in the user configured unit for the measurement.
            // For this reason, derivatives should restart from the base positions.
            List<TrajectoryPoint> result = new List<TrajectoryPoint>();
            if (input.Count == 0)
                return result;

            for (int i = 0; i < input.Count; i++)
                result.Add(new TrajectoryPoint());

            ComputeCoordinates(result, input, calibrationHelper);
            ComputeTotalDistance(result, input, calibrationHelper);
            ComputeVelocity(result, input, calibrationHelper);
            ComputeAcceleration(result, input, calibrationHelper);
            
            // All angular kinematics are based on the best fit circle.
            ComputeRotationCenter(result, input, calibrationHelper);
            ComputeDisplacementAngle(result, input, calibrationHelper);
            ComputeAngularVelocity(result, input, calibrationHelper);
            ComputeAngularAcceleration(result, input, calibrationHelper);

            return result;
        }

        private void ComputeCoordinates(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            for (int i = 0; i < input.Count; i++)
            {
                PointF point = calibrationHelper.GetPoint(input[i].Point.ToPointF());
                result[i].Coordinates = point;
            }
        }

        private void ComputeTotalDistance(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            float distance = 0;
            result[0].TotalDistance = 0;

            for (int i = 1; i < input.Count; i++)
            {
                PointF a = calibrationHelper.GetPoint(input[i - 1].Point.ToPointF());
                PointF b = calibrationHelper.GetPoint(input[i].Point.ToPointF());
                float d = GeometryHelper.GetDistance(a, b);
                distance += d;
                result[i].TotalDistance = distance;
            }
        }

        private void ComputeVelocity(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // uncomputable values.
            result[0].Speed = float.NaN;
            result[0].HorizontalVelocity = float.NaN;
            result[0].VerticalVelocity = float.NaN;

            if (input.Count == 1)
                return;

            result[input.Count - 1].Speed = float.NaN;
            result[input.Count - 1].HorizontalVelocity = float.NaN;
            result[input.Count - 1].VerticalVelocity = float.NaN;

            if (input.Count == 2)
                return;

            for (int i = 1; i < input.Count - 1; i++)
            {
                PointF a = calibrationHelper.GetPoint(input[i - 1].Point.ToPointF());
                PointF b = calibrationHelper.GetPoint(input[i + 1].Point.ToPointF());
                float t = calibrationHelper.GetTime(2);

                result[i].Speed = calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Magnitude));
                result[i].HorizontalVelocity = calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Horizontal));
                result[i].VerticalVelocity = calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Vertical));
            }
        }

        private float GetSpeed(PointF a, PointF b, float t, Component component)
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

            return d / t;
        }

        private void ComputeAcceleration(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // uncomputable values.
            Action<TrajectoryPoint> setUncomputable = (p) => 
            {
                p.Acceleration = float.NaN;
                p.HorizontalAcceleration = float.NaN;
                p.VerticalAcceleration = float.NaN;
            };

            setUncomputable(result[0]);
            if (input.Count == 1)
                return;

            setUncomputable(result[1]);
            if (input.Count == 2)
                return;
            
            setUncomputable(result[input.Count - 1]);
            if (input.Count == 3)
                return;
            
            setUncomputable(result[input.Count - 2]);
            if (input.Count == 4)
                return;
            
            for (int i = 2; i < input.Count - 2; i++)
            {
                PointF p0 = calibrationHelper.GetPoint(input[i - 2].Point.ToPointF());
                PointF p2 = calibrationHelper.GetPoint(input[i].Point.ToPointF());
                PointF p4 = calibrationHelper.GetPoint(input[i + 2].Point.ToPointF());
                float t02 = calibrationHelper.GetTime(2);
                float t24 = calibrationHelper.GetTime(2);
                float t13 = calibrationHelper.GetTime(2);

                result[i].Acceleration = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Magnitude));
                result[i].HorizontalAcceleration = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Horizontal));
                result[i].VerticalAcceleration = calibrationHelper.ConvertAcceleration(GetAcceleration(p0, p2, p4, t02, t24, t13, Component.Vertical));
            }
        }

        private float GetAcceleration(PointF p0, PointF p2, PointF p4, float t02, float t24, float t13, Component component)
        {
            float v02 = GetSpeed(p0, p2, t02, component);
            float v24 = GetSpeed(p2, p4, t24, component);
            return (v24 - v02) / t13;
        }

        private void ComputeRotationCenter(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            if (input.Count < 3)
                return;

            // Least-squares circle fitting.
            // Described in:
            // Coope, I.D., Circle fitting by linear and nonlinear least squares, Journal of Optimization Theory and Applications Volume 76, Issue 2, New York: Plenum Press, February 1993.
            // Implementation based on JS implementation: 
            // http://jsxgraph.uni-bayreuth.de/wiki/index.php/Least-squares_circle_fitting

            // TODO: for each point, find the best fitting circle on a subset of surrounding points.

            int rows = result.Count;
            Matrix m = new Matrix(rows, 3);
            Matrix v = new Matrix(rows, 1);

            for (int i = 0; i < rows; i++)
            {
                m[i, 0] = result[i].Coordinates.X;
                m[i, 1] = result[i].Coordinates.Y;
                m[i, 2] = 1.0;
                v[i, 0] = result[i].Coordinates.X * result[i].Coordinates.X + result[i].Coordinates.Y * result[i].Coordinates.Y;
            }

            Matrix mt = m.Clone();
            mt.Transpose();
            Matrix b = mt.Multiply(m);
            Matrix c = mt.Multiply(v);
            Matrix z = b.Solve(c);

            PointF center = new PointF((float)(z[0, 0] * 0.5), (float)(z[1, 0] * 0.5));
            float radius = (float)Math.Sqrt(z[2, 0] + (center.X * center.X) + (center.Y * center.Y));

            for (int i = 0; i < rows; i++)
            {
                result[i].RotationCenter = center;
                result[i].RotationRadius = radius;

                PointF xAxis = center.Translate(100.0f, 0.0f);
                float angle = GeometryHelper.GetAngle(center, xAxis, result[i].Coordinates);
                if (angle < 0)
                    angle = (float)((Math.PI * 2) + angle);
                result[i].AbsoluteAngle = angle;
            }
        }
    
        private void ComputeDisplacementAngle(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            float total = 0;
            result[0].DisplacementAngle = 0;
            result[0].TotalDisplacementAngle = 0;

            for (int i = 1; i < result.Count; i++)
            {
                float displacement = GetDisplacementAngle(result, i, i -1);
                total += displacement;
                result[i].DisplacementAngle = calibrationHelper.ConvertAngle(displacement);
                result[i].TotalDisplacementAngle = calibrationHelper.ConvertAngle(total);
            }
        }

        private void ComputeAngularVelocity(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // uncomputable.
            result[0].AngularVelocity = float.NaN;
            result[0].CentripetalAcceleration = float.NaN;
            result[0].TangentialVelocity = float.NaN;

            if (result.Count == 1)
                return;

            result[result.Count - 1].AngularVelocity = float.NaN;
            result[result.Count - 1].CentripetalAcceleration = float.NaN;
            result[result.Count - 1].TangentialVelocity = float.NaN;

            for (int i = 1; i < result.Count - 1; i++)
            {
                float d1 = GetDisplacementAngle(result, i, i - 1);
                float d2 = GetDisplacementAngle(result, i + 1, i);
                float time = calibrationHelper.GetTime(2);
                float inRadPerSecond = (d1 + d2) / time;
                result[i].AngularVelocity = calibrationHelper.ConvertAngularVelocity(inRadPerSecond);
                result[i].TangentialVelocity = calibrationHelper.ConvertSpeed(inRadPerSecond * result[i].RotationRadius);
                result[i].CentripetalAcceleration = calibrationHelper.ConvertAcceleration(inRadPerSecond * inRadPerSecond * result[i].RotationRadius);
            }
        }

        private void ComputeAngularAcceleration(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            result[0].AngularAcceleration = float.NaN;
            if (input.Count == 1)
                return;

            result[1].AngularAcceleration = float.NaN;
            if (input.Count == 2)
                return;

            result[result.Count - 1].AngularAcceleration = float.NaN;
            if (input.Count == 3)
                return;

            result[result.Count - 2].AngularAcceleration = float.NaN;
            if (input.Count == 4)
                return;

            for (int i = 2; i < input.Count - 2; i++)
            {
                float d1 = GetDisplacementAngle(result, i - 1, i - 2);
                float d2 = GetDisplacementAngle(result, i, i - 1);
                float d3 = GetDisplacementAngle(result, i + 1, i);
                float d4 = GetDisplacementAngle(result, i + 2, i + 1);
                float t02 = calibrationHelper.GetTime(2);
                float t24 = calibrationHelper.GetTime(2);
                float t13 = calibrationHelper.GetTime(2);

                float v1 = (d1 + d2) / t02;
                float v2 = (d3 + d4) / t24;
                float a = (v2 - v1) / t13;
                result[i].AngularAcceleration = calibrationHelper.ConvertAngularAcceleration(a);
            }
        }
    
        /// <summary>
        /// Returns displacement angle in radians.
        /// </summary>
        private float GetDisplacementAngle(List<TrajectoryPoint> result, int a, int b)
        {
            float displacement = Math.Abs(result[a].AbsoluteAngle - result[b].AbsoluteAngle);

            // To solve the ambiguity when moving through the x axis we keep the smallest value.
            // This assumes the motion is incrementing by small pieces rather than more than a half circle at a time.
            displacement = Math.Min(displacement, (float)(2 * Math.PI - displacement));
            return displacement;
        }
    }
}
