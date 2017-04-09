#region License
/*
Copyright © Joan Charmant 2017.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Contains filtered and unfiltered data for one time series of 2D points and the corresponding cutoff frequencies.
    /// </summary>
    public class FilteredTrajectory
    {
        /// <summary>
        /// Number of samples.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Whether the trajectory can be filtered, depends on the number of samples.
        /// </summary>
        public bool CanFilter { get; private set; }

        /// <summary>
        /// Time coordinates.
        /// </summary>
        public long[] Times { get; private set; }

        /// <summary>
        /// Raw coordinates.
        /// </summary>
        public double[] RawXs { get; private set; }

        /// <summary>
        /// Raw coordinates.
        /// </summary>
        public double[] RawYs { get; private set; }

        /// <summary>
        /// Filtered coordinates at the currently selected cutoff frequency.
        /// </summary>
        public double[] Xs
        {
            get
            {
                return XCutoffIndex < 0 ? RawXs : FilterResultXs[XCutoffIndex].Data;
            }
        }

        /// <summary>
        /// Filtered coordinates at the currently selected cutoff frequency.
        /// </summary>
        public double[] Ys
        {
            get
            {
                return YCutoffIndex < 0 ? RawYs : FilterResultYs[YCutoffIndex].Data;
            }
        }

        /// <summary>
        /// Filtered X coordinates time series at various cutoff frequencies.
        /// </summary>
        public List<FilteringResult> FilterResultXs { get; set; }

        /// <summary>
        /// Best-guess cutoff frequency index for Xs series.
        /// </summary>
        public int XCutoffIndex { get; set; }

        /// <summary>
        /// Filtered time series at various cutoff frequencies.
        /// </summary>
        public List<FilteringResult> FilterResultYs { get; set; }

        /// <summary>
        /// Best-guess cutoff frequency index for Ys series.
        /// </summary>
        public int YCutoffIndex { get; set; }

        public Circle BestFitCircle { get; private set; }

        /// <summary>
        /// Initialize the data and filter it if possible.
        /// </summary>
        public void Initialize(List<TimedPoint> samples, CalibrationHelper calibrationHelper)
        {
            this.Length = samples.Count;

            Times = new long[samples.Count];
            RawXs = new double[samples.Count];
            RawYs = new double[samples.Count];

            XCutoffIndex = -1;
            YCutoffIndex = -1;

            // Raw coordinates.
            for (int i = 0; i < samples.Count; i++)
            {
                PointF point = calibrationHelper.GetPointAtTime(samples[i].Point, samples[i].T);
                RawXs[i] = point.X;
                RawYs[i] = point.Y;
                Times[i] = samples[i].T;
            }

            this.CanFilter = samples.Count > 10;
            if (this.CanFilter)
            {
                double framerate = calibrationHelper.CaptureFramesPerSecond;

                ButterworthFilter filter = new ButterworthFilter();
                

                // Filter the results a hundred times and store all data along with the best cutoff frequency.
                int tests = 100;
                int bestCutoffIndexX;
                FilterResultXs = filter.FilterSamples(RawXs, framerate, tests, out bestCutoffIndexX);
                XCutoffIndex = bestCutoffIndexX;

                int bestCutoffIndexY;
                FilterResultYs = filter.FilterSamples(RawYs, framerate, tests, out bestCutoffIndexY);
                YCutoffIndex = bestCutoffIndexY;
            }

            BestFitCircle = CircleFitter.Fit(this);
        }

        public PointF RawCoordinates(int index)
        {
            return new PointF((float)RawXs[index], (float)RawYs[index]);
        }

        public PointF Coordinates(int index)
        {
            if (XCutoffIndex < 0 || YCutoffIndex < 0)
                return RawCoordinates(index);
            else
                return new PointF((float)FilterResultXs[XCutoffIndex].Data[index], (float)FilterResultYs[YCutoffIndex].Data[index]);
        }
    }
}
