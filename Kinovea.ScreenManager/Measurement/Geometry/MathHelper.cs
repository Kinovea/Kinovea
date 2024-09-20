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
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;

namespace Kinovea.ScreenManager
{
    public static class MathHelper
    {
        private const double RadiansToDegrees = 180 / Math.PI;
        private const double DegreesToRadians = Math.PI / 180;

        public static float Degrees(float radians)
        {
            return (float)(radians * RadiansToDegrees);
        }
        public static double Degrees(double radians)
        {
            return radians * RadiansToDegrees;
        }
        public static float Radians(float degrees)
        {
            return (float)(degrees * DegreesToRadians);
        }

        public static float Round(float value, int decimalPlaces)
        {
            if (decimalPlaces >= 0)
            {
                return (float)Math.Round(value, decimalPlaces);
            }
            else
            {
                int magnitude = (int)Math.Pow(10, -decimalPlaces);
                return (float)(Math.Round(value / magnitude) * magnitude);
            }
        }
    }
}
