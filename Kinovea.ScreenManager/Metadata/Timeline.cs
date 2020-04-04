#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A generic timeline to store strongly typed objects.
    /// Objects can be inserted sparsely along the timeline.
    /// </summary>
    public class Timeline<T>
    {
        private SortedList<long, T> frames = new SortedList<long, T>();
        
        /// <summary>
        /// Adds an entry to the timeline.
        /// </summary>
        public void Insert(long time, T value)
        {
            if(frames.ContainsKey(time))
                frames[time] = value;
            else
                frames.Add(time, value);    
        }
        
        /// <summary>
        /// Returns the entry closest to the requested time.
        /// Does not modify the timeline.
        /// </summary>
        public T ClosestFrom(long time)
        {
            if(frames.Count == 0)
                return default(T);
            
            int index = frames.Keys.BinarySearch(time);
            if(index >= 0)
                return frames.Values[index];
                
            // If no match is found, BinarySearch returns the bitwise complement of the index of the next element.
            index = ~index;
            
            if(index == 0)
                return frames.Values[index];
            
            if(index == frames.Keys.Count)
                return frames.Values[frames.Keys.Count - 1];

            long distanceToPrevious = time - frames.Keys[index - 1];
            long distanceToNext = frames.Keys[index] - time;
                
            if(distanceToPrevious < distanceToNext)
                return frames.Values[index - 1];
            else
                return frames.Values[index];
        }
        
        public void Clear()
        {
            frames.Clear();
        }
        
        public void Clear(Action<T> disposer)
        {
            foreach(T frame in frames.Values)
                disposer(frame);
            
            frames.Clear();
        }
   
        public IEnumerable<T> Enumerate()
        {
            foreach (KeyValuePair<long, T> pair in frames)
                yield return pair.Value;
        }
   
        public int Count 
        {
            get { return frames.Count; }
        }

        public T First
        {
            get { return frames.Values[0]; }
        }
    }
}
