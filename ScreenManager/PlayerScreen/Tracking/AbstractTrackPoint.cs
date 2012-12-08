#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Drawing;
using System.Globalization;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractTrackPoint defines the common interface of a track position, 
	/// and implements the common utility methods.
	/// This class is not intended to be instanciated directly, 
	/// use one of the derivative class instead, like TrackPointSURF or TrackPointBlock.
	/// 
	/// TrackPoints are always instanciated by Tracker concrete implementations.
	/// At this abstract level, the TrackPoint is basically a 3D (x, y, timestamp) point.
	/// </summary>
	public abstract class AbstractTrackPoint
	{
        public Point Point
        {
            get { return new Point(X, Y);}
        }
        
        public int ContentHash
        {
            get { return X.GetHashCode() ^ Y.GetHashCode() ^ T.GetHashCode();}
        }
        
        #region Members
		public int X;
        public int Y;
        public long T;          // timestamp relative to the first time stamp of the track
        #endregion
		
        #region Abstract Methods
		/// <summary>
		/// Reset data. This is used when the user manually moves a point.
		/// Dispose any unmanaged resource.
		/// </summary>
        public abstract void ResetTrackData();
        #endregion
        
        #region Concrete Constructor
        protected AbstractTrackPoint()
        {
        	//not implemented.
        }
        #endregion
        
		#region Concrete Public Methods
		public void WriteXml(XmlWriter _xmlWriter)
		{
			_xmlWriter.WriteString(String.Format("{0};{1};{2}", X, Y, T));
		}
		public void ReadXml(XmlReader _xmlReader)
        {
            string xmlString = _xmlReader.ReadElementContentAsString();

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
                // will default to {0,0,0}.
            }
        }
		public Rectangle Box(int _radius)
		{
		    return new Point(X,Y).Box(_radius);
		}
		#endregion
	}
}
