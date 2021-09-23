#region License
/*
Copyright © Joan Charmant 2021.
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
using System.Drawing.Drawing2D;
using System.Linq;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interactive filter to create Kinograms.
    /// A Kinogram is a single image containing copies of the original frames of the video.
    /// 
    /// The parameters let the user change the subset of frames selected, the crop dimension,
    /// the aspect ratio of the final composition, etc.
    /// 
    /// The filter is interactive. Changing the timestamp will shift the start time of the frames,
    /// panning in the viewport will pan inside the tile under the mouse.
    /// </summary>
    public class VideoFilterKinogram : IVideoFilter
    {
        #region IVideoFilter properties
        public string Name
        {
            get { return "Kinogram"; }
        }
        public Bitmap Icon
        {
            get { return Properties.Resources.mosaic; }
        }

        public bool Experimental
        {
            get { return false; }
        }
        public Bitmap Current
        {
            get { return bitmap; }
        }
        #endregion

        #region members
        private Bitmap bitmap;
        private KinogramParameters parameters = new KinogramParameters();
        private IWorkingZoneFramesContainer framesContainer;
        #endregion

        #region ctor/dtor
        public VideoFilterKinogram()
        {

        }
        ~VideoFilterKinogram()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }
        #endregion

        #region IVideoFilter methods
        public void SetFrames(IWorkingZoneFramesContainer framesContainer)
        {
            this.framesContainer = framesContainer;
        }

        public void Update(Size size, long timestamp)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            if (bitmap == null || bitmap.Size != size)
            {
                if (bitmap != null)
                    bitmap.Dispose();
                
                bitmap = new Bitmap(size.Width, size.Height);
            }

            // TODO: get from parameters:
            int tileCount = 17;
            int rows = 3;
            Size cropSize = new Size(800, 800);
            List<Point> cropPositions = new List<Point>();
            for (int i = 0; i < tileCount; i++)
                cropPositions.Add(new Point(0, 0));

            //-------------
            float step = (float)framesContainer.Frames.Count / tileCount;
            IEnumerable<VideoFrame> frames = framesContainer.Frames.Where((frame, i) => i % step < 1);

            int cols = (int)Math.Ceiling((float)tileCount / rows);
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, size);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / rows);
            Graphics g = Graphics.FromImage(bitmap);
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            // Render the composite.
            g.FillRectangle(Brushes.CornflowerBlue, 0, 0, size.Width, size.Height);
            int index = 0;
            foreach (VideoFrame f in frames)
            {
                Rectangle srcRect = new Rectangle(cropPositions[index].X, cropPositions[index].Y, cropSize.Width, cropSize.Height);
                Rectangle destRect = GetDestinationRectangle(index, cols, rows, paintArea, tileSize);
                g.DrawImage(f.Image, destRect, srcRect, GraphicsUnit.Pixel);
                index++;
            }

            //bitmap.Save("kinogram.png");
        }
        #endregion

        #region Private methods
        
        /// <summary>
        /// Returns the part of the target image where the passed tile should be drawn.
        /// </summary>
        private Rectangle GetDestinationRectangle(int index, int cols, int rows, Rectangle paintArea, Size tileSize)
        {
            int row = index / cols;
            int col = index - (row * cols);
            return new Rectangle(paintArea.Left + col * tileSize.Width, paintArea.Top + row * tileSize.Height, tileSize.Width, tileSize.Height);
        }
        #endregion
    }
}
