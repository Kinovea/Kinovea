#region License
/*
Copyright © Joan Charmant 2013.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Circular buffer in RAM.
    /// The class itself is not thread safe, locking and multithreading should be handled at the caller level.
    /// The capacity in elements must be computed by the caller depending on the size of each element and the total target size.
    /// </summary>
    public class CircularBufferMemory<T>
    {
        private T[] buffer;
        private int capacity = 25;
        private int newest = -1;
        private int total;
        private int insertPoint;
        
        public CircularBufferMemory()
        {
            buffer = new T[capacity];
        }
        
        public void Write(T data)
        {
            bool hadRoom = MakeRoom();
            buffer[insertPoint] = data;
            
            newest = insertPoint;
            insertPoint = (insertPoint + 1) % capacity;
            if(hadRoom)
                total++;
        }
        
        public T Read(int age)
        {
            int index = mod(newest - age, total);
            return buffer[index];
        }
        
        public void Clear()
        {
            for(int i = 0; i < buffer.Length; i++)
                if(buffer[i] != null)
                    ClearAt(i);
        }
        
        public void ChangeCapacity(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
        }
        
        private bool MakeRoom()
        {
            if(buffer[insertPoint] == null)
                return true;

            ClearAt(insertPoint);
            return false;
        }
        
        private void ClearAt(int index)
        {
            if(buffer[index] is IDisposable)
                ((IDisposable)buffer[index]).Dispose();

            buffer[index] = default(T);
        }
        
        private int mod(int x, int m)
        {
            int r = x%m;
            return r<0 ? r+m : r;
        }
    }
}
