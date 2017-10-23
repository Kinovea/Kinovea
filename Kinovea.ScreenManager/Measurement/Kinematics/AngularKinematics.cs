#region License
/*
Copyright © Joan Charmant 2017.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Build time series data for various angular kinematics values pertaining to one angle object.
    /// Input: two or three filtered trajectories of calibrated and undistorted values.
    /// </summary>
    public class AngularKinematics
    {
        private float[] radii;
        private float[] positions;
        private float[] velocities;
        private float[] accelerations;
        private const double TAU = Math.PI * 2;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TimeSeriesCollection BuildKinematics(Dictionary<string, FilteredTrajectory> trajs, AngleOptions angleOptions, CalibrationHelper calibrationHelper)
        {
            if (trajs == null || trajs.Count != 3)
                throw new InvalidProgramException();

            // Assume o, a, b keys for now.
            // We also use the "o" key as a reference, this implies that all three trajectories must have data at the same time points.
            // We must take care during tracking to keep the length of trajectories the same.
            TimeSeriesCollection tsc = new TimeSeriesCollection(trajs["o"].Length);

            tsc.AddTimes(trajs["o"].Times);

            tsc.InitializeKinematicComponents(new List<Kinematics>(){
                Kinematics.AngularPosition,
                Kinematics.AngularDisplacement,
                Kinematics.TotalAngularDisplacement,
                Kinematics.AngularVelocity,
                Kinematics.TangentialVelocity,
                Kinematics.AngularAcceleration,
                Kinematics.TangentialAcceleration,
                Kinematics.CentripetalAcceleration,
                Kinematics.ResultantLinearAcceleration
            });

            // Keep series in the reference unit.
            radii = new float[tsc.Length];
            positions = new float[tsc.Length];
            velocities = new float[tsc.Length];
            accelerations = new float[tsc.Length];

            ComputeAngles(tsc, calibrationHelper, trajs, angleOptions);
            ComputeVelocity(tsc, calibrationHelper);
            ComputeAcceleration(tsc, calibrationHelper);

            return tsc;
        }

        private void ComputeAngles(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper, Dictionary<string, FilteredTrajectory> trajs, AngleOptions angleOptions)
        {
            for (int i = 0; i < tsc.Length; i++)
            {
                PointF o = PointF.Empty;
                PointF a = PointF.Empty;
                PointF b = PointF.Empty;

                if (trajs["o"].CanFilter)
                {
                    o = trajs["o"].Coordinates(i);
                    a = trajs["a"].Coordinates(i);
                    b = trajs["b"].Coordinates(i);
                }
                else
                {
                    o = trajs["o"].RawCoordinates(i);
                    a = trajs["a"].RawCoordinates(i);
                    b = trajs["b"].RawCoordinates(i);
                }

                // Compute the actual angle value. The logic here should match the one in AngleHelper.Update(). 
                // They work on different type of inputs so it's difficult to factorize the functions.
                if (angleOptions.Supplementary)
                {
                    // Create a new point by point reflection of a around o.
                    PointF c = new PointF(2 * o.X - a.X, 2 * o.Y - a.Y);
                    a = b;
                    b = c;
                }

                float angle = 0;
                if (angleOptions.CCW)
                    angle = GeometryHelper.GetAngle(o, a, b);
                else
                    angle = GeometryHelper.GetAngle(o, b, a);

                if (!angleOptions.Signed && angle < 0)
                    angle = (float)(TAU + angle);

                positions[i] = angle;
                radii[i] = GeometryHelper.GetDistance(o, b);
                
                tsc[Kinematics.AngularPosition][i] = calibrationHelper.ConvertAngle(angle); 
                
                if (i == 0)
                {
                    tsc[Kinematics.AngularDisplacement][i] = 0;
                    tsc[Kinematics.TotalAngularDisplacement][i] = 0;
                }
                else
                {
                    float totalDisplacementAngle = angle - positions[0];
                    float displacementAngle = angle - positions[i-1];
                    tsc[Kinematics.AngularDisplacement][i] = calibrationHelper.ConvertAngle(displacementAngle);
                    tsc[Kinematics.TotalAngularDisplacement][i] = calibrationHelper.ConvertAngle(totalDisplacementAngle);
                }
            }
        }

        private void ComputeVelocity(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper)
        {
            if (tsc.Length <= 2)
            {
                PadVelocities(tsc);
                return;
            }

            for (int i = 1; i < tsc.Length - 1; i++)
            {
                float a1 = positions[i - 1];
                float a2 = positions[i + 1];
                float t = calibrationHelper.GetTime(2);
                float omega = (a2 - a1) / t;

                velocities[i] = omega;
                tsc[Kinematics.AngularVelocity][i] = (double)calibrationHelper.ConvertAngularVelocity(omega);

                float v = radii[i] * omega;
                tsc[Kinematics.TangentialVelocity][i] = (double)calibrationHelper.ConvertSpeed(v);
            }

            PadVelocities(tsc);
        }

        private void ComputeAcceleration(TimeSeriesCollection tsc, CalibrationHelper calibrationHelper)
        {
            if (tsc.Length <= 2)
            {
                PadAccelerations(tsc);
                return;
            }

            for (int i = 1; i < tsc.Length - 1; i++)
            {
                float v1 = velocities[i - 1];
                float v2 = velocities[i + 1];
                float t = calibrationHelper.GetTime(2);
                float alpha = (v2 - v1) / t;
                
                tsc[Kinematics.AngularAcceleration][i] = (double)calibrationHelper.ConvertAngularAcceleration(alpha);

                float at = radii[i] * alpha;
                tsc[Kinematics.TangentialAcceleration][i] = (double)calibrationHelper.ConvertAcceleration(at);

                float ac = radii[i] * velocities[i] * velocities[i];
                tsc[Kinematics.CentripetalAcceleration][i] = (double)calibrationHelper.ConvertAcceleration(ac);

                float a = (float)Math.Sqrt(at * at + ac * ac);
                tsc[Kinematics.ResultantLinearAcceleration][i] = (double)calibrationHelper.ConvertAcceleration(a);
            }

            PadAccelerations(tsc);
        }

        #region Low level

        private void PadVelocities(TimeSeriesCollection tsc)
        {
            TimeSeriesPadder.Pad(tsc[Kinematics.AngularVelocity], 1);
            TimeSeriesPadder.Pad(tsc[Kinematics.TangentialVelocity], 1);
            return;
        }

        private void PadAccelerations(TimeSeriesCollection tsc)
        {
            TimeSeriesPadder.Pad(tsc[Kinematics.AngularAcceleration], 2);
            TimeSeriesPadder.Pad(tsc[Kinematics.TangentialAcceleration], 2);
            TimeSeriesPadder.Pad(tsc[Kinematics.CentripetalAcceleration], 2);
            TimeSeriesPadder.Pad(tsc[Kinematics.ResultantLinearAcceleration], 2);
            return;
        }

        
        #endregion
    }
}
