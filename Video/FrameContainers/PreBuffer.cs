#region License
/*
Copyright © Joan Charmant 2011.
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

namespace Kinovea.Video
{
    /// <summary>
    /// A buffer to anticipate some frames after the current point, and remember some before it.
    /// The prebuffered section is contained inside the working zone boundaries.
    /// It is a contiguous set of frames, however it may wrap over the end of the working zone.
    /// </summary>
    /// <remarks>
    /// Naming:
    /// - Segment: the video section of frames, contained inside the working zone.
    /// - Remembrance: the number of frames kept that are older than the current point.
    ///
    /// Thread safety:
    /// Locking is necessary around all access to m_Frames as it is read and written by both the UI and the decoding thread.
    /// Assumedly, there is no need to lock around access to m_Current.
    /// This is because m_Frames is only accessed for add by the decoding thread and this has no impact on m_Current reference.
    /// The only thing that alters the reference to m_Current are: MoveNext, MoveTo, PurgeOutsiders, Clear.
    /// All these are initiated by the UI thread itself, so it will not be using m_Current simultaneously.
    /// Similarly, drop count is only updated in MoveNext and MoveTo, so only from the UI thread.
    ///</remarks>
    
    public class PreBuffer
    {
        public VideoFrame CurrentFrame {
            get { return null; }
        }
    }
}
