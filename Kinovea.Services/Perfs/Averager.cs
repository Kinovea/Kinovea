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

namespace Kinovea.Services
{
    /// <summary>
    /// Online averager using Exponential Moving Average.
    /// This class is not thread safe.
    /// The average can be consulted at any time.
    /// </summary>
    public class Averager
    {
        public double Average
        {
            get { return average;}
        }
        
        private double alpha;
        private double alphaComplement;
        private double average;
        private bool initialized;
        
        /// <summary>
        /// Creates a new Averager.
        /// </summary>
        /// <param name="alpha">Weight given to the most up to date sample.</param>
        public Averager(double alpha)
        {
            if (alpha < 0 || alpha > 1.0)
                throw new ArgumentOutOfRangeException("Alpha must be between 0 and 1.");

            this.alpha = alpha;
            this.alphaComplement = 1 - alpha;
        }
        
        /// <summary>
        /// Update the moving average.
        /// </summary>
        public void Post(long value)
        {
            if (!initialized)
            {
                average = (double)value;
                initialized = true;
                return;
            }

            average = (value * alpha) + (average * alphaComplement);
        }

        /// <summary>
        /// Update the moving average.
        /// </summary>
        public void Post(double value)
        {
            if (!initialized)
            {
                if (value == double.PositiveInfinity)
                    return;

                average = value;
                initialized = true;
                return;
            }

            average = (value * alpha) + (average * alphaComplement);
        }

        public void Reset()
        {
            initialized = false;
            average = 0;
        }
    }
}
