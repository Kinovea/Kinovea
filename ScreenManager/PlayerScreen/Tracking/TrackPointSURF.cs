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
using OpenSURF;
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// TrackPointSURF is the representation of a point tracked from SURF feature matching.
	/// This class must be used in conjonction with TrackerSURF.
	/// </summary>
	public class TrackPointSURF : AbstractTrackPoint
	{
		
		public List<Ipoint> FoundFeatures;
		public Ipoint MatchedFeature;
		public Point SearchWindow;		// This is the top left location of the search window.
										// The feature coordinates are relative to it.
		
		public TrackPointSURF(int _x, int _y, long _t)
		{
			X = _x;
        	Y = _y;
        	T = _t;
		}
		
		public override void ResetTrackData()
		{
			FoundFeatures.Clear();
			MatchedFeature = null;
			SearchWindow = new Point(0,0);
		}
		
		public override string ToString()
		{
			if(MatchedFeature != null)
			{
				return String.Format("X:{0}, Y:{1}, T:{2}, FoundFeatures:{3}, MatchedFeature .x:{4:0.00}, .y:{5:0.00}",
			                    X, Y, T, FoundFeatures.Count, MatchedFeature.x, MatchedFeature.y);
			}
			else
			{
				return String.Format("X:{0}, Y:{1}, T:{2}, Initial state or reseted.", X, Y, T);
			}
		}
		
	}
}
