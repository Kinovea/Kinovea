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
using System.Drawing;

namespace Kinovea.Video
{
    /// <summary>
    /// The frames in the working zone as seen by video filters.
    /// </summary>
    public interface IWorkingZoneFramesContainer
    {
        /// <summary>
        /// The frame collection.
        /// </summary>
        IReadOnlyList<VideoFrame> Frames { get; }
        
        /// <summary>
        /// An arbitrary image suitable for demonstrating the effect of a filter.
        /// </summary>
        Bitmap Representative { get; }
    }
}
