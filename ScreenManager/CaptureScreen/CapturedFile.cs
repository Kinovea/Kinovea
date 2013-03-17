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
using System.Drawing;
using System.IO;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A small pack of info to help launching a recently captured video.
    /// The unique id is used because the filename can be changed.
    /// </summary>
    public class CapturedFile
    {
        public DateTime Time
        {
            get { return time;}
        }
        public string Filename
        {
            get { return Path.GetFileName(filepath);}
        }
        public string Filepath
        {
            get { return filepath;}
        }
        public Bitmap Thumbnail
        {
            get { return thumbnail;}
        }
        public bool Video
        {
            get{return video;}
        }
        
        private DateTime time;
        private string filepath;
        private Bitmap thumbnail;
        private bool video;
        
        public CapturedFile(string filepath, Bitmap image, bool video)
        {
            time = DateTime.Now;
            this.filepath = filepath;
            this.video = video;
            
            if(image != null)
                thumbnail = new Bitmap(image, 100, 75);
            else
                thumbnail = new Bitmap(100, 75);
        }
        
        public void Dispose()
        {
            thumbnail.Dispose();
        }
        
        public void FileRenamed(string newPath)
        {
            this.filepath = newPath;
        }
    }
}
