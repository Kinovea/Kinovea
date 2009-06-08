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
using System.Text;
using System.Drawing;

namespace Videa.Services
{
    public static class XmlHelper
    {

    	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
    	
        public static Point PointParse(string _sPoint, char _delim)
        {
            Point point = new Point(0, 0);

            string[] split = _sPoint.Split(new Char[] { _delim });
            try
            {
                point.X = int.Parse(split[0]);
                point.Y = int.Parse(split[1]);
            }
            catch (Exception)
            {
                // Conversion issue
                // return : (0, 0).
                log.Error(String.Format("An error happened while parsing Point value. ({0}).", _sPoint));
            }

            return point;
        }
        public static Color ColorParse(string _sColor, char _delim)
        {
            Color output = Color.White;

            string[] split = _sColor.Split(new Char[] { _delim });
            try
            {
            	if(split.Length == 3)
            	{
            		// R;G;B
	                output = Color.FromArgb(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
            	}
            	else if(split.Length == 4)
            	{
            		// A;R;G;B.
	                output = Color.FromArgb(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
            	}
            }
            catch (Exception)
            {
            	log.Error(String.Format("An error happened while parsing color value. ({0}).", _sColor));
            }

            return output;
        }
        
        
    }
}
