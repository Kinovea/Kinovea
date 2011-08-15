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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractTracker is the generic class for perforing tracking.
	/// This class is not to be instanciated, use a concrete tracker instead,
	/// like TrackerSURF or TrackerBlock. 
	/// </summary>
	public abstract class AbstractTracker
	{
		#region Abstract Methods
		
		/// <summary>
		/// Performs the tracking. 
		/// Finds the coordinate in current image of the point tracked, using data from previous matches. 
		/// </summary>
		/// <param name="_previousPoints">The list of tracked points so far.</param>
		/// <param name="_CurrentImage">Current image.</param>
		/// <param name="_currentPoint">The resulting point that should be added to the list.</param>
		/// <param name="_t">The current timestamp to create the TrackPoint. Relative to the first timestamp.</param>
		/// <returns>true if the tracking is reliable, false if the point couldn't be found.</returns>
		public abstract bool Track(List<AbstractTrackPoint> _previousPoints, Bitmap _CurrentImage, long _t, out AbstractTrackPoint _currentPoint);
		
		/// <summary>
		/// Creates a TrackPoint nearest possible of the spatial position passed in parameter.
		/// This function should store algorithm related infos in the created track point,
		/// to be used later for tracking the next point.
		/// </summary>
		public abstract AbstractTrackPoint CreateTrackPoint(bool _manual, int _x, int _y, double _fSimilarity, long _t, Bitmap _bmp, List<AbstractTrackPoint> _previousPoints);
		
		/// <summary>
		/// Creates a bare bone TrackPoint.
        /// This is used only in the case of importing from xml.
        /// Can't be used to track the next point. 
        /// Will have to be updated later with algo related info.
       	/// </summary>
		public abstract AbstractTrackPoint CreateOrphanTrackPoint(int _x, int _y, long _t);

		/// <summary>
		/// Draw a visual representation of the tracking taking place.
		/// This may only make sense for users having an idea of how the tracking works.
		/// </summary>
		public abstract void Draw(Graphics _canvas, Point _point, CoordinateSystem _transformer, Color _color, double _fOpacityFactor);
		
		/// <summary>
		/// Retrieve the target rectangle for easy mouse manipulation.
		/// </summary>
		public abstract Rectangle GetEditRectangle(Point _position);
		
		#endregion
	}
}
