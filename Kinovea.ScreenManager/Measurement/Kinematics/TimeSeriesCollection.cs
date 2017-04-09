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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Collects a number of time series for a given type of kinematics.
    /// Both linear and angular kinematics uses this with different entries in the dictionary.
    /// Times and data are kept separately.
    /// </summary>
    public class TimeSeriesCollection
    {
        /// <summary>
        /// Time series.
        /// </summary>
        public double[] this[Kinematics k]
        {
            get
            {
                if (!components.ContainsKey(k))
                    throw new InvalidOperationException();

                return components[k];
            }
        }

        /// <summary>
        /// Number of samples.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Time coordinates.
        /// </summary>
        public long[] Times { get; private set; }

        private Dictionary<Kinematics, double[]> components = new Dictionary<Kinematics, double[]>();

        public TimeSeriesCollection(int length)
        {
            this.Length = length;
            Times = new long[length];
        }

        public void AddTimes(long[] times)
        {
            this.Times = times;
        }

        public void AddComponent(Kinematics key, double[] series)
        {
            if (!components.ContainsKey(key))
                components.Add(key, new double[Length]);

            components[key] = series;
        }

        public void InitializeKinematicComponents(List<Kinematics> list)
        {
            foreach (Kinematics key in list)
                components.Add(key, new double[Length]);
        }
    }
}
