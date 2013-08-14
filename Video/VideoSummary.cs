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
using System.IO;

namespace Kinovea.Video
{
    /// <summary>
    /// Summary of the video. Provides support for animated thumbnails in the integrated explorer.
    /// </summary>
    public class VideoSummary
    {
        public string Filename { get; private set; }
        public bool IsImage { get; private set; }
        public bool HasKva { get; private set; }
        public Size ImageSize { get; private set; }
        public long DurationMilliseconds { get; private set; }
        public List<Bitmap> Thumbs { get; private set; }
        
        private static readonly VideoSummary invalid = new VideoSummary("", false, false, Size.Empty, 0, null);
        
        public VideoSummary(string filename, bool isImage, bool hasKva, Size imageSize, long durationMs, List<Bitmap> thumbs)
        {
            this.Filename = filename;
            this.IsImage = isImage;
            this.HasKva = hasKva;
            this.ImageSize = imageSize;
            this.DurationMilliseconds = durationMs;
            this.Thumbs = thumbs;
        }
        
        public static VideoSummary Invalid {
            get { return invalid; }
        }
        public static VideoSummary GetInvalid(string filename)
        {
            return new VideoSummary(filename, false, false, Size.Empty, 0, null);
        }
        public static bool HasCompanionKva(string filename)
        {
            string kvaFile = string.Format("{0}\\{1}.kva", Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
            return File.Exists(kvaFile);
        }
    }
}
