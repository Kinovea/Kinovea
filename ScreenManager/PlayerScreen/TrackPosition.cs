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
using System.Xml;

namespace Videa.ScreenManager
{
	/// <summary>
	/// A class to represent a 3D (x, y, time) point for trajectories.
	/// It also embed an image patch for comparisons.
	/// </summary>
    public class TrackPosition
    {
        public int X;
        public int Y;
        public long T;          // timestamp relative to the first time stamp of the track
        public Bitmap Image;    // Template zone to be matched.

        public TrackPosition(int _x, int _y, long _t)
        {
            X = _x;
            Y = _y;
            T = _t;
            Image = null;
        }
        public TrackPosition(int _x, int _y, long _t, Bitmap _img)
        {
            X = _x;
            Y = _y;
            T = _t;
            Image = _img;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ T.GetHashCode();
        }
        public void ToXml(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("TrackPosition");
            _xmlWriter.WriteString(X.ToString() + ";" + Y.ToString() + ";" + T.ToString());
            _xmlWriter.WriteEndElement();
        }
        public void FromXml(XmlReader _xmlReader)
        {
            string xmlString = _xmlReader.ReadString();

            string[] split = xmlString.Split(new Char[] { ';' });
            try
            {
                X = int.Parse(split[0]);
                Y = int.Parse(split[1]);
                T = int.Parse(split[2]);
            }
            catch (Exception)
            {
                // Conversion issue
                // will be : (0, 0, 0).
            }
        }
        
    }
}
