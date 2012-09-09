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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml;

namespace Kinovea.Services
{
    public static class XmlHelper
    {
        // Note: the built-in TypeConverters are crashing on some machines for unknown reason. (TypeDescriptor.GetConverter(typeof(Point)))
    	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static Point ParsePoint(string _sPoint)
        {
            Point point = Point.Empty;
            try
            {
                string[] a = _sPoint.Split(new char[] {';'});
                point = new Point(int.Parse(a[0]), int.Parse(a[1]));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing Point value. ({0}).", _sPoint));
            }

            return point;
        }
        public static PointF ParsePointF(string _sPoint)
        {
            PointF point = PointF.Empty;
            try
            {
                string[] a = _sPoint.Split(new char[] {';'});
                point = new PointF(float.Parse(a[0]), float.Parse(a[1]));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing PointF value. ({0}).", _sPoint));
            }

            return point;
        }
        public static Size ParseSize(string sizeString)
        {
            Size size = Size.Empty;
            try
            {
                string[] a = sizeString.Split(new char[] {';'});
                size = new Size(int.Parse(a[0]), int.Parse(a[1]));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing Size value. ({0}).", sizeString));
            }

            return size;
        }
        public static List<int> ParseIntList(string _intList)
        {
            List<int> l = new List<int>();
            try
            {
                string[] listAsStrings = _intList.Split(new char[] {';'});
                foreach(string s in listAsStrings)
                    l.Add(int.Parse(s));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing List of ints. ({0}).", _intList));
            }
            return l;
        }
        public static Color ParseColor(string _sColor)
        {
            Color output = Color.Black;

            try
            {
                string[] a = _sColor.Split(new char[] {';'});
                if(a.Length == 3)
                {
                    output = Color.FromArgb(255, byte.Parse(a[0]), byte.Parse(a[1]), byte.Parse(a[2]));
                }
                else if(a.Length == 4)
                {
                    output = Color.FromArgb(byte.Parse(a[0]), byte.Parse(a[1]), byte.Parse(a[2]), byte.Parse(a[3]));
                }
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
            // - ReadElementContentAsBoolean() which only accepts "false", "true", "1" or "0" as per XML spec and throws an exception otherwise.
            return (_str != "false" && _str != "False" && _str != "0");
        }
    }
}
