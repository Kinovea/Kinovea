#region License
/*
Copyright © Joan Charmant 2008-2009.
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

namespace Kinovea.Services
{
	/// <summary>
	/// Description of TimeHelper.
	/// </summary>
	public static class TimeHelper
	{
		/// <summary>
		/// Input    : Milliseconds (Can be negative.)
		/// Output   : 'hh:mm:ss:00'
		/// 
		/// /!\ TimeCode granularity is in Hundredth or Thousandth, not in Frames.
		/// If bThousandth is true, it means we have more than 100 frames per seconds 
		/// and needs to show it in the time returned.
		/// This can happen when the user manually tune the input fps.	
		/// </summary>
		public static string MillisecondsToTimecode(long _iTotalMilliseconds, bool _bThousandth)
		{
			int iMinutes, iSeconds, iMilliseconds;
			int iTotalHours, iTotalMinutes, iTotalSeconds;
			string timecode;
			bool bNegative = (_iTotalMilliseconds < 0);
			
			//Computes the total of hours, minutes, seconds, milliseconds.
			iTotalSeconds   = (int)(_iTotalMilliseconds / 1000);
			iTotalMinutes   = iTotalSeconds / 60;
			iTotalHours     = iTotalMinutes / 60;

			iMinutes        = iTotalMinutes - (iTotalHours * 60);
			iSeconds        = iTotalSeconds - (iTotalMinutes * 60);
			iMilliseconds   = (int)_iTotalMilliseconds - (iTotalSeconds * 1000);
			
			// Since the time can be relative to a sync point, it can be negative.
			if (bNegative)
			{
				iTotalHours = -iTotalHours;
				iMinutes = -iMinutes;
				iSeconds = -iSeconds;
				iMilliseconds = -iMilliseconds;

				if (!_bThousandth)
				{
					timecode = String.Format("- {0:0}:{1:00}:{2:00}:{3:00}", iTotalHours, iMinutes, iSeconds, iMilliseconds / 10);
				}
				else
				{
					timecode = String.Format("- {0:0}:{1:00}:{2:00}:{3:000}", iTotalHours, iMinutes, iSeconds, iMilliseconds);
				}
			}
			else
			{
				if (!_bThousandth)
				{
					timecode = String.Format("{0:0}:{1:00}:{2:00}:{3:00}", iTotalHours, iMinutes, iSeconds, iMilliseconds / 10);
				}
				else
				{
					timecode = String.Format("{0:0}:{1:00}:{2:00}:{3:000}", iTotalHours, iMinutes, iSeconds, iMilliseconds);
				}
			}

			return timecode;
		}
	}
}
