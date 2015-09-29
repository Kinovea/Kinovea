#region License
/*
Copyright © Joan Charmant 2012.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Note: the angle values follow the .NET System.Drawing convention, (not the Atan2 convention):
    /// Values range from 0 to 360°, always positive, clockwise direction, 0 start at the X axis.
    /// </summary>
    public struct Angle
    {
        public float Start;
        public float Sweep;
        
        public Angle(float start, float sweep)
        {
            this.Start = start;
            this.Sweep = sweep;
        }
        public override string ToString()
        {
            return string.Format("[Angle Start={0}, Sweep={1}]", Start, Sweep);
        }
    }
}
