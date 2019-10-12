#region License
/*
Copyright © Joan Charmant 2008-2009.
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
        /// Output   : [h:][mm:]ss.mm[m].
        /// Seconds and hundredths are always shown. Minutes and hours are shown depending on magnitude of passed time.
        /// If 'thousandth' is true, show 3 digits after the separator, otherwise round to nearest centisecond.
        /// </summary>
        public static string MillisecondsToTimecode(double totalMilliseconds, bool thousandth)
        {
            int millisecondsPerSecond = 1000;
            int millisecondsPerMinute = millisecondsPerSecond * 60;
            int millisecondsPerHour = millisecondsPerMinute * 60;

            int hours = (int)(totalMilliseconds / millisecondsPerHour);
            double remainder = totalMilliseconds % millisecondsPerHour;
            int minutes = (int)(remainder / millisecondsPerMinute);
            remainder = remainder % millisecondsPerMinute;
            int seconds = (int)(remainder / millisecondsPerSecond);

            int milliseconds = (int)(remainder % millisecondsPerSecond);
            int centiseconds = (int)Math.Round(milliseconds / 10.0);

            bool negative = totalMilliseconds < 0;
            if (negative)
            {
                hours = -hours;
                minutes = -minutes;
                seconds = -seconds;
                centiseconds = -centiseconds;
                milliseconds = -milliseconds;
            }

            string timecode;
            string sign = negative ? "- " : "";
            if (thousandth)
            {
                if (hours > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}:{3:00}.{4:000}", sign, hours, minutes, seconds, milliseconds);
                else if (minutes > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}.{3:000}", sign, minutes, seconds, milliseconds);
                else
                    timecode = string.Format("{0}{1:0}.{2:000}", sign, seconds, milliseconds);
            }
            else
            {
                // Since we round to the nearest centisecond, we may have to carry.
                if (centiseconds == 100)
                {
                    centiseconds = 0;
                    seconds++;
                }
                if (seconds == 60)
                {
                    seconds = 0;
                    minutes++;
                }
                if (minutes == 60)
                {
                    minutes = 0;
                    hours++;
                }

                if (hours > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}:{3:00}.{4:00}", sign, hours, minutes, seconds, centiseconds);
                else if (minutes > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}.{3:00}", sign, minutes, seconds, centiseconds);
                else
                    timecode = string.Format("{0}{1:0}.{2:00}", sign, seconds, centiseconds);
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
                case TimecodeFormat.Microseconds:
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
