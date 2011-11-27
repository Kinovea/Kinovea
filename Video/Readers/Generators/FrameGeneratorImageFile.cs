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
using System.Drawing;

namespace Kinovea.Video
{
    /// <summary>
    /// Provide access to a single image. Used to turn an image into a video.
    /// </summary>
    public class FrameGeneratorImageFile : IFrameGenerator
    {
        public Size Size {
            get { return (m_Bitmap != null) ? m_Bitmap.Size : m_ErrorBitmap.Size; }
        }
        private Bitmap m_Bitmap;
        private Bitmap m_ErrorBitmap;
        public FrameGeneratorImageFile()
        {
            m_ErrorBitmap = new Bitmap(640, 480);
        }
        public void Initialize(string _init)
        {
            m_Bitmap = new Bitmap(_init);
        }
        public Bitmap Generate(long _timestamp)
        {
            return (m_Bitmap != null) ? m_Bitmap : m_ErrorBitmap;
        }
        public void DisposePrevious(Bitmap _previous){}
        public void Close()
        {
            if(m_Bitmap != null)
                m_Bitmap.Dispose();
            
            if(m_ErrorBitmap != null)
                m_ErrorBitmap.Dispose();
        }
    }
}
