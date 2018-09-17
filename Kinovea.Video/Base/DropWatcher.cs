#region License
/*
Copyright © Joan Charmant 2011.
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

namespace Kinovea.Base
{
    /// <summary>
    /// A simple drop counter to get average number of drops over time.
    /// Usage: call AddDropStatus on each tick, passing a boolean indicating
    /// if the frame is dropped or not.
    /// </summary>
    public class DropWatcher
    {
        private long m_Total;
        private long m_Drops;
        
        public void AddDropStatus(bool drop)
        {
            m_Total++;
            if(drop)
                m_Drops++;
        }
        public double Ratio {
            get {
                return m_Total == 0 ? 0 : (double)m_Drops / m_Total;
            }
        }
        public void Restart()
        {
            m_Total = 0;
            m_Drops = 0;
        }
    }
}
