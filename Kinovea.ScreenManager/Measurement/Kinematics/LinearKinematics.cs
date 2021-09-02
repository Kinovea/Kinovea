#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Build time series data for various linear kinematics values.
    /// Input: one list of timed XY positions.
    /// </summary>
    public class LinearKinematics
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TimeSeriesCollection BuildKinematics(FilteredTrajectory traj, CalibrationHelper calibrationHelper)
        {
            TimeSeriesCollection tsc = new TimeSeriesCollection(traj.Length);

            if (traj.Length == 0)
                return tsc;

            tsc.AddTimes(traj.Times);
            tsc.AddComponent(Kinematics.XRaw, traj.RawXs);
            tsc.AddComponent(Kinematics.YRaw, traj.RawYs);
            tsc.AddComponent(Kinematics.X, traj.Xs);
            tsc.AddComponent(Kinematics.Y, traj.Ys);

            Func<int, PointF> getCoord;
            if (traj.CanFilter)
                getCoord = traj.Coordinates;
            else 
                getCoord = traj.RawCoordinates;

            tsc.InitializeKinematicComponents(new List<Kinematics>(){
                Kinematics.LinearDistance,
                Kinematics.LinearHorizontalDisplacement,
                Kinematics.LinearVerticalDisplacement,
                Kinematics.LinearSpeed,
                Kinematics.LinearHorizontalVelocity,
                Kinematics.LinearVerticalVelocity,
                Kinematics.LinearAcceleration,
                Kinematics.LinearHorizontalAcceleration,
                Kinematics.LinearVerticalAcceleration,
            });

            ComputeDistances(tsc, calibrationHelper, getCoord);
            ComputeVelocities(tsc, calibrationHelper, getCoord);
            ComputeAccelerations(tsc, calibrationHelper, getCoord);

            return tsc;
        }

        private void ComputeDistances(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper, Func<int, PointF> getCoord)
        {
            PointF o = getCoord(0);

            tsc[Kinematics.LinearDistance][0] = 0;
            tsc[Kinematics.LinearHorizontalDisplacement][0] = 0;
            tsc[Kinematics.LinearVerticalDisplacement][0] = 0;

            for (int i = 1; i < tsc.Length; i++)
            {
                PointF a = getCoord(i - 1);
                PointF b = getCoord(i);
                tsc[Kinematics.LinearDistance][i] = tsc[Kinematics.LinearDistance][i-1] + GetDistance(a, b, Component.Magnitude);
                tsc[Kinematics.LinearHorizontalDisplacement][i] = GetDistance(o, b, Component.Horizontal);
                tsc[Kinematics.LinearVerticalDisplacement][i] = GetDistance(o, b, Component.Vertical);
            }
        }

        private void ComputeVelocities(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper, Func<int, PointF> getCoord)
        {
            if (tsc.Length <= 2)
            {
                PadVelocities(tsc);
                return;
            }

            for (int i = 1; i < tsc.Length - 1; i++)
            {
                PointF a = getCoord(i-1);
                PointF b = getCoord(i+1);
                float t = calibrationHelper.GetTime(2);

                tsc[Kinematics.LinearSpeed][i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Magnitude));
                tsc[Kinematics.LinearHorizontalVelocity][i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Horizontal));
                tsc[Kinematics.LinearVerticalVelocity][i] = (double)calibrationHelper.ConvertSpeed(GetSpeed(a, b, t, Component.Vertical));
            }

            PadVelocities(tsc);

            if (!PreferencesManager.PlayerPreferences.EnableHighSpeedDerivativesSmoothing)
                return;

            // Second pass: apply extra smoothing to the derivatives.
            // This is only applied for high speed videos where the digitization is very noisy 
            // due to the combination of increased time resolution and decreased spatial resolution.
            double constantVelocitySpan = 40;
            MovingAverage filter = new MovingAverage();
            double[] averagedVelocity = filter.FilterSamples(tsc[Kinematics.LinearSpeed], calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);
            double[] averagedHorizontalVelocity = filter.FilterSamples(tsc[Kinematics.LinearHorizontalVelocity], calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);
            double[] averagedVerticalVelocity = filter.FilterSamples(tsc[Kinematics.LinearVerticalVelocity], calibrationHelper.CaptureFramesPerSecond, constantVelocitySpan, 1);

            for (int i = 0; i < tsc.Length; i++)
            {
                tsc[Kinematics.LinearSpeed][i] = averagedVelocity[i];
                tsc[Kinematics.LinearHorizontalVelocity][i] = averagedHorizontalVelocity[i];
                tsc[Kinematics.LinearVerticalVelocity][i] = averagedVerticalVelocity[i];
            }
        }

        private void ComputeAccelerations(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper, Func<int, PointF> getCoord)
        {
            if (tsc.Length <= 4)
            {
                PadAccelerations(tsc);
                return;
            }

            // First pass: average speed over 2t centered on each data point.
            for (int i = 2; i < tsc.Length - 2; i++)
            {
                float t = calibrationHelper.GetTime(2);

                double acceleration = (tsc[Kinematics.LinearSpeed][i + 1] - tsc[Kinematics.LinearSpeed][i - 1]) / t;
                tsc[Kinematics.LinearAcceleration][i] = calibrationHelper.ConvertAccelerationFromVelocity((float)acceleration);

                double horizontalAcceleration = (tsc[Kinematics.LinearHorizontalVelocity][i + 1] - tsc[Kinematics.LinearHorizontalVelocity][i - 1]) / t;
                tsc[Kinematics.LinearHorizontalAcceleration][i] = calibrationHelper.ConvertAccelerationFromVelocity((float)horizontalAcceleration);

                double verticalAcceleration = (tsc[Kinematics.LinearVerticalVelocity][i + 1] - tsc[Kinematics.LinearVerticalVelocity][i - 1]) / t;
                tsc[Kinematics.LinearVerticalAcceleration][i] = calibrationHelper.ConvertAccelerationFromVelocity((float)verticalAcceleration);
            }

            PadAccelerations(tsc);

            if (!PreferencesManager.PlayerPreferences.EnableHighSpeedDerivativesSmoothing)
                return;
            
            // Second pass: extra smoothing derivatives.
            // This is only applied for high speed videos where the digitization is very noisy 
            // due to the combination of increased time resolution and decreased spatial resolution.
            double constantAccelerationSpan = 50;
            MovingAverage filter = new MovingAverage();
            
            double[] averagedAcceleration = filter.FilterSamples(tsc[Kinematics.LinearAcceleration], calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);
            double[] averagedHorizontalAcceleration = filter.FilterSamples(tsc[Kinematics.LinearHorizontalAcceleration], calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);
            double[] averagedVerticalAcceleration = filter.FilterSamples(tsc[Kinematics.LinearVerticalAcceleration], calibrationHelper.CaptureFramesPerSecond, constantAccelerationSpan, 2);

            for (int i = 0; i < tsc.Length; i++)
            {
                tsc[Kinematics.LinearAcceleration][i] = averagedAcceleration[i];
                tsc[Kinematics.LinearHorizontalAcceleration][i] = averagedHorizontalAcceleration[i];
                tsc[Kinematics.LinearVerticalAcceleration][i] = averagedVerticalAcceleration[i];
            }
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

        private void PadVelocities(TimeSeriesCollection tsc)
        {
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearSpeed], 1);
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalVelocity], 1);
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearVerticalVelocity], 1);
            return;
        }

        private void PadAccelerations(TimeSeriesCollection tsc)
        {
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearAcceleration], 2);
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearHorizontalAcceleration], 2);
            TimeSeriesPadder.Pad(tsc[Kinematics.LinearVerticalAcceleration], 2);
            return;
        }

        #endregion
    }
}
