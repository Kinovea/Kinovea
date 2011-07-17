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
		public static string MillisecondsToTimecode(double _fTotalMilliseconds, bool _bThousandth, bool _bLeadingZeroes)
		{
			int iMinutes, iSeconds, iMilliseconds;
			int iTotalHours, iTotalMinutes, iTotalSeconds;
			string timecode;
			bool bNegative = (_fTotalMilliseconds < 0);
			
			//Computes the total of hours, minutes, seconds, milliseconds.
			iTotalSeconds   = (int)(Math.Round(_fTotalMilliseconds) / 1000);
			iTotalMinutes   = iTotalSeconds / 60;
			iTotalHours     = iTotalMinutes / 60;

			iMinutes        = iTotalMinutes - (iTotalHours * 60);
			iSeconds        = iTotalSeconds - (iTotalMinutes * 60);
			iMilliseconds = (int)Math.Round(_fTotalMilliseconds % 1000);
						
			// Since the time can be relative to a sync point, it can be negative.
			string negativeSign = bNegative ? "- " : "";
			
			if (bNegative)
			{
				iTotalHours = -iTotalHours;
				iMinutes = -iMinutes;
				iSeconds = -iSeconds;
				iMilliseconds = -iMilliseconds;
			}

			if (!_bThousandth)
			{
				int iHundredth = (int) Math.Round((float)iMilliseconds / 10);
				if(_bLeadingZeroes || iTotalHours > 0)
				{
					timecode = String.Format("{0}{1:0}:{2:00}:{3:00}:{4:00}", negativeSign, iTotalHours, iMinutes, iSeconds, iHundredth);
				}
				else
				{
					if(iMinutes > 0)
					{
						timecode = String.Format("{0}{1:00}:{2:00}:{3:00}", negativeSign, iMinutes, iSeconds, iHundredth);
					}
					else if(iSeconds > 0)
					{
						timecode = String.Format("{0}{1:00}:{2:00}", negativeSign, iSeconds, iHundredth);
					}
					else
					{
						timecode = String.Format("{0}{1:00}", negativeSign, iHundredth);
					}
				}
			}
			else
			{
				if(_bLeadingZeroes || iTotalHours > 0)
				{
					timecode = String.Format("{0}{1:0}:{2:00}:{3:00}:{4:000}", negativeSign, iTotalHours, iMinutes, iSeconds, iMilliseconds);
				}
				else
				{
					if(iMinutes > 0)
					{
						timecode = String.Format("{0}{1:00}:{2:00}:{3:000}", negativeSign, iMinutes, iSeconds, iMilliseconds);
					}
					else if(iSeconds > 0)
					{
						timecode = String.Format("{0}{1:00}:{2:000}", negativeSign, iSeconds, iMilliseconds);
					}
					else
					{
						timecode = String.Format("{0}{1:000}", negativeSign, iMilliseconds);
					}
				}
			}
			
			return timecode;
		}
	
		public static TimeCodeType GetTimecodeType(TimeCodeFormat _tcf)
		{
			TimeCodeType tct = TimeCodeType.String;
			
			switch(_tcf)
			{
				case TimeCodeFormat.Frames:
				case TimeCodeFormat.Milliseconds:	
				case TimeCodeFormat.HundredthOfMinutes:
				case TimeCodeFormat.TenThousandthOfHours:
				case TimeCodeFormat.Timestamps:
					tct = TimeCodeType.Number;
					break;
				
				case TimeCodeFormat.ClassicTime:
				case TimeCodeFormat.TimeAndFrames:
				default:
					tct = TimeCodeType.String;
					break;
			}
			
			return tct;
		}
	
	}
}
