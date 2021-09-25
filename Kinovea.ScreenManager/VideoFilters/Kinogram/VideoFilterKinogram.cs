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
using System.Windows.Forms;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interactive filter to create Kinograms.
    /// A Kinogram is a single image containing copies of the original frames of the video.
    /// 
    /// The parameters let the user change the subset of frames selected, the crop dimension,
    /// the number of columns and rows of the final composition, etc.
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
        private Size frameSize;
        private KinogramParameters parameters = new KinogramParameters();
        private List<PointF> cropPositions = new List<PointF>();
        private IWorkingZoneFramesContainer framesContainer;
        private long timestamp;
        private int movingTile = -1;

        // TODO: Move to parameters.
        private int tileCount = 60;
        private int rows = 5;
        private Size cropSize = new Size(300, 400);
        #endregion

        #region ctor/dtor
        public VideoFilterKinogram()
        {
            UpdateTileCount();
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
            if (framesContainer != null && framesContainer.Frames != null && framesContainer.Frames.Count > 0)
            {
                frameSize = framesContainer.Frames[0].Image.Size;
                UpdateSize(frameSize);
            }
        }

        public void UpdateSize(Size size)
        {
            if (bitmap == null || bitmap.Size != size)
            {
                if (bitmap != null)
                    bitmap.Dispose();

                bitmap = new Bitmap(size.Width, size.Height);
            }

            Update();
        }

        public void UpdateTime(long timestamp)
        {
            this.timestamp = timestamp;
            Update();
        }

        public void StartMove(PointF p)
        {
            movingTile = GetTile(p);
        }

        public void StopMove()
        {
            movingTile = -1;
        }

        public void Move(float dx, float dy, Keys modifiers)
        {
            if (movingTile < 0)
                return;

            if ((modifiers & Keys.Shift) == Keys.Shift)
            {
                for (int i = 0; i < tileCount; i++)
                    MoveTile(dx, dy, i);
                
                Update();
            }
            else
            {
               MoveTile(dx, dy, movingTile);
               Update(movingTile);
            }
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Paint the composite or paint one tile of the composite.
        /// </summary>
        private void Update(int tile = -1)
        {
            if (bitmap == null || framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            float step = (float)framesContainer.Frames.Count / tileCount;
            IEnumerable<VideoFrame> frames = framesContainer.Frames.Where((frame, i) => i % step < 1);

            Size size = bitmap.Size;
            int cols = (int)Math.Ceiling((float)tileCount / rows);
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, size);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / rows);
            Graphics g = Graphics.FromImage(bitmap);
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            if (tile >= 0)
            {
                // Render a single tile.
                int index = tile;
                VideoFrame f = frames.ToList()[index];
                RectangleF srcRect = new RectangleF(cropPositions[index].X, cropPositions[index].Y, cropSize.Width, cropSize.Height);
                Rectangle destRect = GetDestinationRectangle(index, cols, rows, paintArea, tileSize);
                g.FillRectangle(Brushes.CornflowerBlue, destRect);
                g.DrawImage(f.Image, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                // Render the whole composite.
                g.FillRectangle(Brushes.CornflowerBlue, 0, 0, size.Width, size.Height);
                int index = 0;
                foreach (VideoFrame f in frames)
                {
                    RectangleF srcRect = new RectangleF(cropPositions[index].X, cropPositions[index].Y, cropSize.Width, cropSize.Height);
                    Rectangle destRect = GetDestinationRectangle(index, cols, rows, paintArea, tileSize);
                    g.DrawImage(f.Image, destRect, srcRect, GraphicsUnit.Pixel);
                    index++;
                }
            }
        }

        /// <summary>
        /// Returns the part of the target image where the passed tile should be drawn.
        /// </summary>
        private Rectangle GetDestinationRectangle(int index, int cols, int rows, Rectangle paintArea, Size tileSize)
        {
            int row = index / cols;
            int col = index - (row * cols);
            return new Rectangle(paintArea.Left + col * tileSize.Width, paintArea.Top + row * tileSize.Height, tileSize.Width, tileSize.Height);
        }

        private void UpdateTileCount()
        {
            // TODO: find a way to not invalidate the existing crop positions.
            for (int i = 0; i < tileCount; i++)
                cropPositions.Add(new Point(0, 0));
        }

        /// <summary>
        /// Find the tile under this point.
        /// The point is given in the space of the original cached images.
        /// </summary>
        private int GetTile(PointF p)
        {
            Size size = bitmap.Size;
            int cols = (int)Math.Ceiling((float)tileCount / rows);
            Size fullSize = new Size(cropSize.Width * cols, cropSize.Height * rows);
            Rectangle paintArea = UIHelper.RatioStretch(fullSize, size);
            Size tileSize = new Size(paintArea.Width / cols, paintArea.Height / rows);

            // Express the coordinate in the paint area.
            p = new PointF(p.X - paintArea.X, p.Y - paintArea.Y);

            if (p.X < 0 || p.Y < 0 || p.X >= paintArea.Width || p.Y >= paintArea.Height)
                return -1;

            int col = (int)(p.X / tileSize.Width);
            int row = (int)(p.Y / tileSize.Height);
            int index = row * cols + col;
            return index;
        }

        /// <summary>
        /// Move the crop position of a specific tile.
        /// </summary>
        private void MoveTile(float dx, float dy, int index)
        {
            // TODO: get from preferences or parameters.
            bool clamp = true;

            PointF old = cropPositions[index];
            float x = old.X - dx;
            float y = old.Y - dy;

            if (clamp)
            {
                x = Math.Max(0, x);
                x = Math.Min(frameSize.Width - cropSize.Width, x);
                y = Math.Max(0, y);
                y = Math.Min(frameSize.Height - cropSize.Height, y);
            }

            cropPositions[index] = new PointF(x, y);
        }
        #endregion
    }
}
