#region License
/*
Copyright © Joan Charmant 2011.
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

namespace Kinovea.Base
{
    /// <summary>
    /// A simple time accumulator to get average loop time.
    /// Usage: call AddLoopTime once per loop, passing a time.
    /// </summary>
    public class LoopWatcher
    {
        private long m_Loops;
        private double m_Time;
        
        
        public void AddLoopTime(double time)
        {
            m_Loops++;
            m_Time += time;
        }
        public string Average
        {
            get {
                double loopTime = m_Time / m_Loops;
                return string.Format("Average loop time: {0:0.000}ms", loopTime);
            }
        }
        public void Restart()
        {
            m_Loops = 0;
            m_Time = 0.0;
        }
    }
}
