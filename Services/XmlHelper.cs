/*
Copyright © Joan Charmant 2008.
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml;

namespace Kinovea.Services
{
    public static class XmlHelper
    {
    	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));
        private static readonly TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
        
        
        public static Point ParsePoint(string _sPoint)
        {
            Point point = Point.Empty;
            try
            {
                point = (Point)pointConverter.ConvertFromString(_sPoint);
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing Point value. ({0}).", _sPoint));
            }

            return point;
        }
        public static Color ParseColor(string _sColor)
        {
            Color output = Color.Black;

            try
            {
                output = (Color)colorConverter.ConvertFromString(_sColor);
            }
            catch (Exception)
            {
            	log.Error(String.Format("An error happened while parsing color value. ({0}).", _sColor));
            }

            return output;
        }
        public static bool ParseBoolean(string _str)
        {
            // This function helps fix the discrepancy between:
            // - Boolean.ToString() which returns "False" or "True",
            // - ReadElementContentAsBoolean() which only accepts "false", "true", "1" or "0" otherwise throws an exception.
            return (_str != "false" && _str != "False" && _str != "0");
        }
    }
}
