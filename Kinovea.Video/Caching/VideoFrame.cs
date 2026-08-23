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
using System.Drawing;

namespace Kinovea.Video
{
    public class VideoFrame
    {
        /// <summary>
        /// The video frame bitmap (scaled, rotated, stabilized, etc.).
        /// </summary>
        public Bitmap Image { get; }
        
        /// <summary>
        /// The timestamp of this frame.
        /// </summary>
        public long Timestamp { get; }
        
        /// <summary>
        /// The timestamp of the video frame immediately preceding 
        /// this frame in the media.
        /// </summary>
        public long PreviousTimestamp { get; }
        
        public VideoFrame(Bitmap image, long timestamp, long previousTimestamp)
        {
            Image = image;
            Timestamp = timestamp;
            PreviousTimestamp = previousTimestamp;
        }

        public static VideoFrame Empty()
        {
            return new VideoFrame(null, -1, -1);
        }
    }
}
