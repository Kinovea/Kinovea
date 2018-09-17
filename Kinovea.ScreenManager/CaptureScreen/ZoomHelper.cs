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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Provides a log scale zoom value with simple Increase and Decrease API.
    /// Goes [0.1 -> 10], defaults at 1.
    /// </summary>
    public class ZoomHelper
    {
        public float Value
        {
            get { return Map(linearValue);}
            set { linearValue = Unmap(value);}
        }
        
        // All values are linear [-1.0 -> +1.0], until the caller asks for it.
        private float linearValue = 0;
        private float step = 0.05f;
        private float min = -1.0f;
        private float max = 1.0f;
        private bool roundToNearest = true;
        public void Increase()
        {
            linearValue = Clamp(linearValue + step);
            
            if(roundToNearest)
                linearValue = RoundToNearest(linearValue);
        }
        public void Decrease()
        {
            linearValue = Clamp(linearValue - step);
            
            if(roundToNearest)
                linearValue = RoundToNearest(linearValue);
        }
        public void Reset()
        {
            linearValue = 0;
        }
        
        private static float Map(float linearValue)
        {
            return (float)Math.Pow(10, linearValue);
        }
        private static float Unmap(float logValue)
        {
            return (float)Math.Log10(logValue);
        }
        
        private float Clamp(float value)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        
        private float RoundToNearest(float value)
        {
            return (float)Math.Round(linearValue / step) * step;
        }
        
    }
}
