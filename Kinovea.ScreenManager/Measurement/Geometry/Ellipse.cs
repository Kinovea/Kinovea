#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public struct Ellipse
    {
        public PointF Center;
        public float SemiMajorAxis;
        public float SemiMinorAxis;
        public float Rotation;
        
        public bool Vertical
        {
            get 
            {
                return (Rotation > (Math.PI / 4)) && (Rotation < (Math.PI * 0.75));
            }
        }
       
        public Ellipse(PointF center, float semiMajorAxis, float semiMinorAxis, float rotation)
        {
            this.Center = center;
            this.SemiMajorAxis = semiMajorAxis;
            this.SemiMinorAxis = semiMinorAxis;
            this.Rotation = rotation;
        }
        
        public override string ToString()
        {
            return string.Format("[Ellipse Center={0}, a={1}, b={2}, rotation={3:0.000}]", Center, SemiMajorAxis, SemiMinorAxis, Rotation);
        }
    }
}
