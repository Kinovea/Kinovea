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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.Video
{
    public class VideoSummary
    {
        public bool IsImage { get; private set; }
        public bool HasKva { get; private set; }
        public Size ImageSize { get; private set; }
        public long DurationMilliseconds { get; private set; }
        public List<Bitmap> Thumbs { get; private set; }
        
        private static readonly VideoSummary m_Invalid = new VideoSummary(false, false, Size.Empty, 0, null);
        
        public VideoSummary(bool _isImage, bool _hasKva, Size _imageSize, long _durationMs, List<Bitmap> _thumbs)
        {
            IsImage = _isImage;
            HasKva = _hasKva;
            ImageSize = _imageSize;
            DurationMilliseconds = _durationMs;
            Thumbs = _thumbs;
        }
        
        public static VideoSummary Invalid {
            get { return m_Invalid; }
        }
    }
}
