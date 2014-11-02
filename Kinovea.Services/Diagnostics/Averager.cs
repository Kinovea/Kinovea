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

namespace Kinovea.Services
{
    /// <summary>
    /// Sliding average.
    /// </summary>
    public class Averager
    {
        public double Average
        {
            get { return avg;}
        }
        
        private List<double> intervals = new List<double>();
        private int max;
        private double total;
        private double avg;
        
        public Averager(int max)
        {
            this.max = max;
        }
        
        public void Add(double interval)
        {
            if(intervals.Count == max)
            {
                total -= intervals[0];
                intervals.RemoveAt(0);
            }
            
            intervals.Add(interval);
            total += interval;
            avg = total / intervals.Count;
        }
    }
}
