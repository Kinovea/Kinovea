#region License
/*
Copyright © Joan Charmant 2014.
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
using Kinovea.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kinovea.Video.Synthetic
{
    public class FrameGeneratorSyntheticVideo : IFrameGenerator
    {
        public Size OriginalSize 
        {
            get { return video.ImageSize; }
        }

        public Size ReferenceSize
        {
            get { return OriginalSize; }
        }

        private SyntheticVideo video;
        private Bitmap errorBitmap;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FrameGeneratorSyntheticVideo(SyntheticVideo video)
        {
            this.video = video;
            errorBitmap = new Bitmap(640, 480);
        }

        #region IFrameGenerator implementation
        public OpenVideoResult Open(string init)
        {
            return OpenVideoResult.Success;
        }
        
        public Bitmap Generate(long timestamp)
        {
            Bitmap bitmap = null;

            try
            {
                bitmap = new Bitmap(video.ImageSize.Width, video.ImageSize.Height, PixelFormat.Format32bppPArgb);

                using (Graphics g = Graphics.FromImage(bitmap))
                using (SolidBrush backBrush = new SolidBrush(video.BackgroundColor))
                {
                    g.FillRectangle(backBrush, g.ClipBounds);

                SolidBrush foreBrush = new SolidBrush(video.BackgroundColor.Invert());
                
                    if (video.FrameNumber)
                    {
                        using (Font font = new Font("Arial", 24, FontStyle.Regular))
                        {
                            string text = string.Format("Current frame : {0}", timestamp);
                            g.DrawString(text, font, foreBrush, new PointF(25, 25));

                            text = string.Format("Current time : {0:0.000} ms", timestamp  * 1000.0f / video.FramePerSecond);
                            g.DrawString(text, font, foreBrush, new PointF(25, 75));
                        }
                    }

                double t = timestamp / video.FramePerSecond;

                foreach (SyntheticObject o in video.Objects)
                {
                    float x = (float)(o.Position.X + (o.VX * t) + (0.5f * o.AX * t * t));
                    float y = (float)(o.Position.Y + (o.VY * t) + (0.5f * o.AY * t * t));
                    float r = o.Radius;
                    RectangleF box = new RectangleF(x - r, y - r, r * 2, r * 2);
                    g.FillEllipse(foreBrush, box);
                }

                foreBrush.Dispose();
                }
            }
            catch
            {
                log.ErrorFormat("Erro while generating frame for synthetic video.");
            }

            return bitmap ?? errorBitmap;
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

        public void SetRotation(ImageRotation rotation)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}

