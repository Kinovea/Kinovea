#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kinovea.Base
{
    /// <summary>
    /// Extends System.Stopwatch with a Dictionary to keep intermediate times.
    /// Used for instrumentation to avoid writing to the log at each intermediate step.
    /// Usage: Call LogTime at each intermediate step inside the loop, then dump all recorded times.
    /// </summary>
    public class TimeWatcher : Stopwatch
    {
        private Dictionary<string, long> m_Timings = new Dictionary<string, long>();
        private double ticksToMicros;
        private double ticksToMillis;
        private string loggedTimeMillis = "{0} : {1:0.000} ms";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public TimeWatcher()
        {
            ticksToMicros = 1000000.0 / Frequency;
            ticksToMillis = 1000.0 / Frequency;
        }
        
        /// <summary>
        /// Clear the list of recorded times and restart the stopwatch.
        /// </summary>
        public void Restart()
        {
            m_Timings.Clear();
            Stop();
            Reset();
            Start();
        }
        
        /// <summary>
        /// Record the current time in association to a given message.
        /// Duplicate message in the same timing session will be ignored.
        /// </summary>
        /// <param name="message">Message to record at this specific time.</param>
        public void LogTime(string message)
        {
           if(!m_Timings.ContainsKey(message))
               m_Timings.Add(message, ElapsedTicks);
        }
        
        /// <summary>
        /// Retrieve the specific time associated with a given message, in text form.
        /// </summary>
        /// <param name="message">Message that was used to record the time.</param>
        /// <returns>A formated string with the message and time.</returns>
        public string Time(string message)
        {
            if(m_Timings.ContainsKey(message))
                return string.Format(loggedTimeMillis, message, m_Timings[message] * ticksToMillis);
            else
                return "";
        }
        
        /// <summary>
        /// Retrieve the specific time associated with a given message, in numeric form.
        /// </summary>
        /// <param name="message">Message that was used to record the time.</param>
        /// <returns>The time in milliseconds.</returns>
        public double RawTime(string message)
        {
            if(m_Timings.ContainsKey(message))
                return m_Timings[message] * ticksToMillis;
            else
                return 0;
        }
        
        /// <summary>
        /// Logs the list of times recorded during the timing session so far.
        /// </summary>
        public void DumpTimes()
        {
            foreach(KeyValuePair<string, long> t in m_Timings)
                log.DebugFormat(loggedTimeMillis, t.Key, t.Value * ticksToMillis);
        }
        
        /*
        /// <summary>
        /// Get the average time of the records.
        /// </summary>
        /// <returns>A formatted string giving the average time recorded.</returns>
        public string Average()
        {
            get {
                double average = m_Timings.Average(t => t.Value);
                return string.Format("Average: {0:0.000}", average);
            }
        }*/
    }
}
