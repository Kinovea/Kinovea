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
        public AbstractTrackPoint()
        {
        	//not implemented.
        }
        #endregion
        
		#region Concrete Public Methods
		public void ToXml(XmlTextWriter _xmlWriter, Metadata _ParentMetadata, Point _origin)
        {
        	// In addition to the native data (the one that will be used when reading back the trajectory.)
        	// we also add the data in user units as attributes.
        	// This will be used when exporting to spreadsheet.
        	
            _xmlWriter.WriteStartElement("TrackPosition");
        	
            // Data in user units.
            // - The origin of the coordinates system is given as parameter.
            // - X goes left (same than internal), Y goes up (opposite than internal).
            double userX = _ParentMetadata.CalibrationHelper.GetLengthInUserUnit((double)X - (double)_origin.X);
            double userY = _ParentMetadata.CalibrationHelper.GetLengthInUserUnit((double)_origin.Y - (double)Y);
            string userT = _ParentMetadata.m_TimeStampsToTimecodeCallback(T, TimeCodeFormat.Unknown, false);
			
            _xmlWriter.WriteAttributeString("UserX", String.Format("{0:0.00}", userX));
            _xmlWriter.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", userX));
            _xmlWriter.WriteAttributeString("UserY", String.Format("{0:0.00}", userY));
            _xmlWriter.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", userY));
            _xmlWriter.WriteAttributeString("UserTime", userT);
            
			// Native data.
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
		public Point ToPoint()
        {
        	// Extract to a simple Point.
        	return new Point(X, Y);
        }
		public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ T.GetHashCode();
        }
		#endregion
	}
}
