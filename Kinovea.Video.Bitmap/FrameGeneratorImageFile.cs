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
using SystemBitmap = System.Drawing.Bitmap;

namespace Kinovea.Video.Bitmap
{
    /// <summary>
    /// Provide access to a single image. Used to turn an image into a video.
    /// </summary>
    public class FrameGeneratorImageFile : IFrameGenerator
    {
        public Size Size {
            get { return (bitmap != null) ? bitmap.Size : errorBitmap.Size; }
        }
        private SystemBitmap bitmap;
        private SystemBitmap errorBitmap;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FrameGeneratorImageFile()
        {
            errorBitmap = new SystemBitmap(640, 480);
        }
        public OpenVideoResult Initialize(string init)
        {
            OpenVideoResult res = OpenVideoResult.NotSupported;
            try
            {
                bitmap = new SystemBitmap(init);
                if(bitmap != null)
                    res = OpenVideoResult.Success;

                // Force input to be 96 dpi like GDI+ so we don't have any surprises when calling Graphics.DrawImageUnscaled().
                bitmap.SetResolution(96, 96);
            }
            catch(Exception e)
            {
                log.ErrorFormat("An error occured while trying to open {0}", init);
                log.Error(e);
            }
            return res;
        }
        public SystemBitmap Generate(long timestamp)
        {
            return (bitmap != null) ? bitmap : errorBitmap;
        }

        public SystemBitmap Generate(long timestamp, Size maxWidth)
        {
            return Generate(timestamp);
        }

        public void DisposePrevious(SystemBitmap previous){}
        public void Close()
        {
            if(bitmap != null)
                bitmap.Dispose();
            
            if(errorBitmap != null)
                errorBitmap.Dispose();
        }
    }
}
