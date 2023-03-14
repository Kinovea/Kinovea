/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    public static class XmlHelper
    {
        // Note: the built-in TypeConverters are crashing on some machines for unknown reason. (TypeDescriptor.GetConverter(typeof(Point)))
    	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static PointF ParsePointF(string str)
        {
            PointF point = PointF.Empty;

            try
            {
                string[] a = str.Split(new char[] {';'});
                
                float x;
                float y;
                bool readX = float.TryParse(a[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
                bool readY = float.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
                
                if(readX && readY)
                    point = new PointF(x, y);
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing PointF value. ({0}).", str));
            }

            return point;
        }
        public static Size ParseSize(string str)
        {
            Size size = Size.Empty;

            try
            {
                string[] a = str.Split(new char[] {';'});
                size = new Size(int.Parse(a[0]), int.Parse(a[1]));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing Size value. ({0}).", str));
            }

            return size;
        }
        public static SizeF ParseSizeF(string str)
        {
            SizeF size = SizeF.Empty;

            try
            {
                string[] a = str.Split(new char[] {';'});
                
                float width;
                float height;
                bool readWidth = float.TryParse(a[0], NumberStyles.Any, CultureInfo.InvariantCulture, out width);
                bool readHeight = float.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out height);
                
                if(readWidth && readHeight)
                    size = new SizeF(width, height);
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing SizeF value. ({0}).", str));
            }

            return size;
        }
        public static List<int> ParseIntList(string str)
        {
            List<int> l = new List<int>();
            try
            {
                string[] listAsStrings = str.Split(new char[] {';'});
                foreach(string s in listAsStrings)
                    l.Add(int.Parse(s));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing List of ints. ({0}).", str));
            }

            return l;
        }
        public static Color ParseColor(string xmlColor, Color byDefault)
        {
            Color output = byDefault;

            try
            {
                string[] a = xmlColor.Split(new char[] {';'});
                if(a.Length == 3)
                {
                    output = Color.FromArgb(255, byte.Parse(a[0]), byte.Parse(a[1]), byte.Parse(a[2]));
                }
                else if(a.Length == 4)
                {
                    output = Color.FromArgb(byte.Parse(a[0]), byte.Parse(a[1]), byte.Parse(a[2]), byte.Parse(a[3]));
                }
                else
                {
                    ColorConverter converter = new ColorConverter();
                    output = (Color)converter.ConvertFromString(xmlColor);
                }
            }
            catch (Exception)
            {
            	log.Error(String.Format("An error happened while parsing color value. ({0}).", xmlColor));
            }

            return output;
        }
        public static bool ParseBoolean(string str)
        {
            // This function helps fix the discrepancy between:
            // - Boolean.ToString() which returns "False" or "True",
            // - ReadElementContentAsBoolean() which only accepts "false", "true", "1" or "0" as per XML spec and throws an exception otherwise.
            return (str != "false" && str != "False" && str != "0");
        }
        
        public static int ParsePointReference(string content)
        {            
            // Parse a direct point reference (ex: 12) or a variable identifier (ex: $12).
            // Variable identifier are transported as negative numbers.
            int output = 0;
            try            
            {                
                if(content.StartsWith("$"))
                {                    
                    int variable = int.Parse(content.Substring(1));
                    output = - (variable + 1);
                }
                else                
                {                    
                    output = int.Parse(content);
                }            
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing point reference. ({0}).", content));
            }
            
            return output;
        }
        
        public static Bitmap ParseImageFromBase64(string base64)
        {
            Bitmap result = null;
            
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                result = (Bitmap)Image.FromStream(new MemoryStream(bytes));
            }
            catch(Exception)
            {
                log.Error("An error happened while parsing bitmap value.");
            }
            
            return result;
        }
        
        public static Rectangle ParseRectangle(string rectangleString)
        {
            Rectangle rectangle = Rectangle.Empty;
            try
            {
                string[] a = rectangleString.Split(new char[] {';'});
                rectangle = new Rectangle(
                    int.Parse(a[0]), 
                    int.Parse(a[1]),
                    int.Parse(a[2]),
                    int.Parse(a[3]));
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing Rectangle value. ({0}).", rectangleString));
            }

            return rectangle;
        }

        public static RectangleF ParseRectangleF(string str)
        {
            RectangleF rect = RectangleF.Empty;

            try
            {
                string[] a = str.Split(new char[] { ';' });

                float x;
                float y;
                float width;
                float height;
                bool readX = float.TryParse(a[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
                bool readY = float.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
                bool readWidth = float.TryParse(a[2], NumberStyles.Any, CultureInfo.InvariantCulture, out width);
                bool readHeight = float.TryParse(a[3], NumberStyles.Any, CultureInfo.InvariantCulture, out height);

                if (readX && readY && readWidth && readHeight)
                    rect = new RectangleF(x, y, width, height);
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing RectangleF value. ({0}).", str));
            }

            return rect;
        }

        public static VideoSection ParseVideoSection(string str)
        {
            VideoSection section = VideoSection.Empty;

            try
            {
                string[] a = str.Split(new char[] { ';' });

                long start;
                long end;
                bool readStart = long.TryParse(a[0], NumberStyles.Any, CultureInfo.InvariantCulture, out start);
                bool readEnd = long.TryParse(a[1], NumberStyles.Any, CultureInfo.InvariantCulture, out end);

                if (readStart && readEnd)
                {
                    if (start == -1)
                        start = long.MaxValue;

                    if (end == -1)
                        end = long.MaxValue;

                    section = new VideoSection(start, end);
                }
            }
            catch (Exception)
            {
                log.Error(String.Format("An error happened while parsing VideoSection value. ({0}).", str));
            }

            return section;
        }

        public static string WriteFloat(float value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
        public static string WritePointF(PointF point)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1}", point.X, point.Y);
        }

        public static string WriteRectangleF(RectangleF rect)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3}", rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static string WriteSizeF(SizeF size)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1}", size.Width, size.Height);
        }

        public static string WriteSize(Size size)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0};{1}", size.Width, size.Height);
        }

        public static string WriteColor(Color color, bool alpha)
        {
            if (alpha)
                return string.Format("{0};{1};{2};{3}", color.A.ToString(), color.R.ToString(), color.G.ToString(), color.B.ToString());
            else
                return string.Format("{0};{1};{2}", color.R.ToString(), color.G.ToString(), color.B.ToString());
        }
    
        public static string WriteBoolean(bool value)
        {
            return value.ToString().ToLower();
        }

        public static string WriteBitmap(Bitmap bitmap)
        {
            System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Png;
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, imageFormat);
            byte[] bytes = stream.ToArray();
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        public static string WriteVideoSection(VideoSection section)
        {
            long start = section.Start == long.MaxValue ? -1 : section.Start;
            long end = section.End == long.MaxValue ? -1 : section.End;
            return string.Format(CultureInfo.InvariantCulture, "{0};{1}", start, end);
        }
    }
}
