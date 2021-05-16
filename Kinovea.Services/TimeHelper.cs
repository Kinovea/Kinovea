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
    public static class TimeHelper
    {
        /// <summary>
        /// Convert time information to a string.
        /// </summary>
        /// <param name="framerate">Framerate of the video, this is used to compute the precision of the output.</param>
        /// <param name="frames">Number of frames spanning the time period.</param>
        /// <param name="milliseconds">Number of milliseconds spanning the time period.</param>
        /// <param name="timestamps">Number of timestamps spanning the time period.</param>
        /// <param name="durationTimestamps">Total duration, in timestamps, of the larger segment the considered time period is in. This is used to compute normalized time.</param>
        /// <param name="totalFrames">Total duration, in frames, of the larger segment the considered time period is in.</param>
        /// <param name="format">The timecode format used for the output.</param>
        /// <param name="symbol">Whether the final string should contain the time symbol</param>
        public static string GetTimestring(double framerate, int frames, double milliseconds, long timestamps, double durationTimestamps, double totalFrames, TimecodeFormat format, bool symbol)
        {
            string outputTimeCode;

            double framerateMagnitude = Math.Log10(framerate);
            int precision = (int)Math.Ceiling(framerateMagnitude);
            string frameString = String.Format("{0}", frames);
            
            switch (format)
            {
                case TimecodeFormat.ClassicTime:
                    outputTimeCode = MillisecondsToTimecode(milliseconds, precision);
                    break;
                case TimecodeFormat.Frames:
                    outputTimeCode = frameString;
                    break;
                case TimecodeFormat.Milliseconds:
                    outputTimeCode = String.Format("{0}", (int)Math.Round(milliseconds));
                    if (symbol)
                        outputTimeCode += " ms";
                    break;
                case TimecodeFormat.Microseconds:
                    outputTimeCode = String.Format("{0}", (int)Math.Round(milliseconds * 1000));
                    if (symbol)
                        outputTimeCode += " µs";
                    break;
                case TimecodeFormat.TenThousandthOfHours:
                    // 1 Ten Thousandth of Hour = 360 ms.
                    double inTenThousandsOfAnHour = milliseconds / 360.0;
                    outputTimeCode = String.Format("{0}:{1:00}", (int)inTenThousandsOfAnHour, Math.Floor((inTenThousandsOfAnHour - (int)inTenThousandsOfAnHour) * 100));
                    break;
                case TimecodeFormat.HundredthOfMinutes:
                    // 1 Hundredth of minute = 600 ms.
                    double inHundredsOfAMinute = milliseconds / 600.0;
                    outputTimeCode = String.Format("{0}:{1:00}", (int)inHundredsOfAMinute, Math.Floor((inHundredsOfAMinute - (int)inHundredsOfAMinute) * 100));
                    break;
                case TimecodeFormat.TimeAndFrames:
                    String timeString = MillisecondsToTimecode(milliseconds, precision);
                    outputTimeCode = String.Format("{0} ({1})", timeString, frameString);
                    break;
                case TimecodeFormat.Normalized:
                    // 1.0 is the coordinate of the last frame.
                    int magnitude = (int)Math.Ceiling(Math.Log10(totalFrames));
                    string outputFormat = string.Format("{{0:0.{0}}}", new string('0', magnitude));
                    double normalized = timestamps / durationTimestamps;
                    outputTimeCode = String.Format(outputFormat, normalized);
                    break;
                case TimecodeFormat.Timestamps:
                    outputTimeCode = String.Format("{0}", (int)timestamps);
                    break;
                default:
                    outputTimeCode = MillisecondsToTimecode(milliseconds, precision);
                    break;
            }

            return outputTimeCode;
        }



        /// <summary>
        /// Input    : Milliseconds (Can be negative.)
        /// Output   : [h:][mm:]ss.xx[x].
        /// Seconds and the decimal part are always shown. 
        /// Minutes and hours are shown depending on the magnitude of passed time.
        /// 'precision' is the number of digits to show after the seconds separator. It should be based on the magintude of the framerate.
        /// </summary>
        public static string MillisecondsToTimecode(double totalMilliseconds, int precision)
        {
            int millisecondsPerSecond = 1000;
            int millisecondsPerMinute = millisecondsPerSecond * 60;
            int millisecondsPerHour = millisecondsPerMinute * 60;

            int hours = (int)(totalMilliseconds / millisecondsPerHour);
            double remainder = totalMilliseconds % millisecondsPerHour;
            int minutes = (int)(remainder / millisecondsPerMinute);
            remainder = remainder % millisecondsPerMinute;
            int seconds = (int)(remainder / millisecondsPerSecond);

            double milliseconds = remainder % millisecondsPerSecond;
            int centiseconds = (int)Math.Round((int)milliseconds / 10.0);

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
            if (precision > 2)
            {
                int fractionValue = (int)(milliseconds * (precision - 2));
                string format = "D" + precision.ToString();
                string fraction = fractionValue.ToString(format);

                if (hours > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}:{3:00}.{4}", sign, hours, minutes, seconds, fraction);
                else if (minutes > 0)
                    timecode = string.Format("{0}{1:0}:{2:00}.{3}", sign, minutes, seconds, fraction);
                else
                    timecode = string.Format("{0}{1:0}.{2}", sign, seconds, fraction);
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

        public static TimestampMapper IdentityTimestampMapper = (time) => time;
    }
}
