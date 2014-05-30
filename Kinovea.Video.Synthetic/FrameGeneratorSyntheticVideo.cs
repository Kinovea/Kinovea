#region License
/*
Copyright © Joan Charmant 2014.
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

namespace Kinovea.Video.Synthetic
{
    public class FrameGeneratorSyntheticVideo : IFrameGenerator
    {
        public Size Size 
        {
            get 
            {
                return video.ImageSize;
            }
        }

        private SyntheticVideo video;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FrameGeneratorSyntheticVideo(SyntheticVideo video)
        {
            this.video = video;
        }

        #region IFrameGenerator implementation
        public OpenVideoResult Initialize(string init)
        {
            return OpenVideoResult.Success;
        }
        
        public Bitmap Generate(long timestamp)
        {
            Bitmap bitmap = new Bitmap(video.ImageSize.Width, video.ImageSize.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            using(SolidBrush backBrush = new SolidBrush(video.BackgroundColor))
            {
                g.FillRectangle(backBrush, g.ClipBounds);

                if (video.FrameNumber)
                {
                    using (SolidBrush foreBrush = new SolidBrush(video.BackgroundColor.Invert()))
                    using (Font font = new Font("Arial", 16, FontStyle.Regular))
                    {
                        g.DrawString(timestamp.ToString(), font, foreBrush, new PointF(25, 25));
                    }
                }
            }

            return bitmap;
        }

        public Bitmap Generate(long timestamp, Size maxWidth)
        {
            return Generate(timestamp);
        }

        public void DisposePrevious(Bitmap previous)
        {
        }
        
        public void Close()
        {
        }
        #endregion

    }
}

