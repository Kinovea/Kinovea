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

namespace Kinovea.Video
{
    public class ThreadCanceler
    {
        public bool CancellationPending {
            get { lock(m_Locker) return m_CancelRequest; }
        }
        
        private object m_Locker = new object();
        private bool m_CancelRequest;
        
        public void Cancel()
        {
            lock(m_Locker)
                m_CancelRequest = true;
        }
        public void Reset()
        {
            lock(m_Locker)
                m_CancelRequest = false;
        }
    }
}
