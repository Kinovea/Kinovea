using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class KinematicsHelper
    {
        public List<TrajectoryPoint> ComputeValues(List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // Computes values at all points from the given trajectory.
            List<TrajectoryPoint> result = new List<TrajectoryPoint>();
            if (input.Count == 0)
                return result;

            for (int i = 0; i < input.Count; i++)
                result.Add(new TrajectoryPoint());

            ComputeCoordinates(result, input, calibrationHelper);
            ComputeTotalDistance(result, input, calibrationHelper);
            ComputeVelocity(result, input, calibrationHelper);
            ComputeAcceleration(result, input, calibrationHelper);

            // TODO: estimate rotation center.


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
                PointF p0 = input[i-1].Point.ToPointF();
                PointF p1 = input[i].Point.ToPointF();
                float d = calibrationHelper.GetLength(p0, p1);
                distance += d;
                result[i].TotalDistance = distance;
            }
        }

        private void ComputeVelocity(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // We cannot compute speed in pixel here and just send the result to calibration helper for conversion.
            // we need to pass each point to calibration to compute correct speed.

            // TODO: implement smoothing.
            
            // uncomputable values.
            result[0].Speed = float.NaN;
            result[0].HorizontalVelocity = float.NaN;
            result[0].VerticalVelocity = float.NaN;

            if (input.Count == 1)
                return;

            result[input.Count - 1].Speed = float.NaN;
            result[input.Count - 1].HorizontalVelocity = float.NaN;
            result[input.Count - 1].VerticalVelocity = float.NaN;

            for (int i = 1; i < input.Count - 1; i++)
            {
                PointF p0 = input[i - 1].Point.ToPointF();
                PointF p2 = input[i + 1].Point.ToPointF();

                result[i].Speed = calibrationHelper.GetSpeed(p0, p2, 2, Component.Magnitude);
                result[i].HorizontalVelocity = calibrationHelper.GetSpeed(p0, p2, 2, Component.Horizontal);
                result[i].VerticalVelocity = calibrationHelper.GetSpeed(p0, p2, 2, Component.Vertical);
            }
        }

        private void ComputeAcceleration(List<TrajectoryPoint> result, List<AbstractTrackPoint> input, CalibrationHelper calibrationHelper)
        {
            // TODO: implement smoothing.
            
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
                PointF p0 = input[i - 2].Point.ToPointF();
                PointF p2 = input[i].Point.ToPointF();
                PointF p4 = input[i + 2].Point.ToPointF();

                result[i].Acceleration = calibrationHelper.GetAcceleration(p0, p2, 2, p2, p4, 2, 2, Component.Magnitude);
                result[i].HorizontalAcceleration = calibrationHelper.GetAcceleration(p0, p2, 2, p2, p4, 2, 2, Component.Horizontal);
                result[i].VerticalAcceleration = calibrationHelper.GetAcceleration(p0, p2, 2, p2, p4, 2, 2, Component.Vertical);        
            }
        }
    }
}
