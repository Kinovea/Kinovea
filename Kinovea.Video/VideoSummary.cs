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
using System.IO;

namespace Kinovea.Video
{
    /// <summary>
    /// Summary of the video. Provides support for animated thumbnails in the integrated explorer.
    /// </summary>
    public class VideoSummary
    {
        #region Properties
        public static VideoSummary Invalid
        {
            get { return invalid; }
        }

        public string Filename { get; private set; }
        public bool IsImage { get; set; }
        public bool HasKva { get; set; }
        public Size ImageSize { get; set; }
        public long DurationMilliseconds { get; set; }
        public DateTime Creation { get; set; }
        public double Framerate { get; set; }
        public List<Bitmap> Thumbs { get; private set; }
        #endregion

        private static readonly VideoSummary invalid = new VideoSummary("");
        
        public VideoSummary(string filename)
        {
            this.Filename = filename;

            this.IsImage = false;
            this.HasKva = false;
            this.ImageSize = Size.Empty;
            this.DurationMilliseconds = 0;
            this.Thumbs = new List<Bitmap>();

            if (!string.IsNullOrEmpty(Filename) && File.Exists(Filename))
            {
                this.HasKva = HasCompanionKva();
                this.Creation = File.GetCreationTime(Filename);
            }
            else
            {
                this.HasKva = false;
            }
        }
        
        private bool HasCompanionKva()
        {
            string kvaFile = string.Format("{0}\\{1}.kva", Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename));
            return File.Exists(kvaFile);
        }
    }
}
