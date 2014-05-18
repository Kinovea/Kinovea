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
        public static string MillisecondsToTimecode(double totalMilliseconds, bool thousandth, bool leadingZeroes)
        {
            int minutes, seconds, milliseconds;
            int totalHours, totalMinutes, totalSeconds;
            string timecode;
            bool negative = totalMilliseconds < 0;
            
            //Computes the total of hours, minutes, seconds, milliseconds.
            totalSeconds = (int)(Math.Round(totalMilliseconds) / 1000);
            totalMinutes = totalSeconds / 60;
            totalHours = totalMinutes / 60;
            minutes = totalMinutes - (totalHours * 60);
            seconds = totalSeconds - (totalMinutes * 60);
            milliseconds = (int)Math.Round(totalMilliseconds % 1000);
                        
            // Since the time can be relative to a sync point, it can be negative.
            string negativeSign = negative ? "- " : "";
            
            if (negative)
            {
                totalHours = -totalHours;
                minutes = -minutes;
                seconds = -seconds;
                milliseconds = -milliseconds;
            }

            if (!thousandth)
            {
                int hundredth = (int) Math.Round((float)milliseconds / 10);
                if(leadingZeroes || totalHours > 0)
                {
                    timecode = String.Format("{0}{1:0}:{2:00}:{3:00}:{4:00}", negativeSign, totalHours, minutes, seconds, hundredth);
                }
                else
                {
                    if(minutes > 0)
                        timecode = String.Format("{0}{1:00}:{2:00}:{3:00}", negativeSign, minutes, seconds, hundredth);
                    else if(seconds > 0)
                        timecode = String.Format("{0}{1:00}:{2:00}", negativeSign, seconds, hundredth);
                    else
                        timecode = String.Format("{0}{1:00}", negativeSign, hundredth);
                }
            }
            else
            {
                if(leadingZeroes || totalHours > 0)
                {
                    timecode = String.Format("{0}{1:0}:{2:00}:{3:00}:{4:000}", negativeSign, totalHours, minutes, seconds, milliseconds);
                }
                else
                {
                    if(minutes > 0)
                        timecode = String.Format("{0}{1:00}:{2:00}:{3:000}", negativeSign, minutes, seconds, milliseconds);
                    else if(seconds > 0)
                        timecode = String.Format("{0}{1:00}:{2:000}", negativeSign, seconds, milliseconds);
                    else
                        timecode = String.Format("{0}{1:000}", negativeSign, milliseconds);
                }
            }
            
            return timecode;
        }
    
        public static TimecodeType GetTimecodeType(TimecodeFormat format)
        {
            TimecodeType type = TimecodeType.String;
            
            switch(format)
            {
                case TimecodeFormat.Frames:
                case TimecodeFormat.Milliseconds:	
                case TimecodeFormat.HundredthOfMinutes:
                case TimecodeFormat.TenThousandthOfHours:
                case TimecodeFormat.Timestamps:
                    type = TimecodeType.Number;
                    break;
                
                case TimecodeFormat.ClassicTime:
                case TimecodeFormat.TimeAndFrames:
                default:
                    type = TimecodeType.String;
                    break;
            }
            
            return type;
        }

        public static TimestampMapper IdentityTimestampMapper = (time, relative) => time;
    }
}
