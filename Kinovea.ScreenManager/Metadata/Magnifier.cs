#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Picture-in-picture with magnification.
    /// </summary>
    public class Magnifier : ITrackable
    {
        // TODO: save positions in the KVA.
        // TODO: support for rendering unscaled.
        
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
        #endregion
        
        #region Properties
        public MagnifierMode Mode 
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Current magnification level.
        /// </summary>
        public float Zoom 
        {
            get { return zoom; }
        }
        public Point Center 
        {
            get { return source.Rectangle.Center(); }
        }

        /// <summary>
        /// Area of the image being magnified.
        /// </summary>
        public Rectangle Source 
        {
            get { return source.Rectangle; }
        }

        /// <summary>
        /// Area where we paint the magnified version of the source rectangle.
        /// </summary>
        public RectangleF Destination
        {
            get { return destination; }
        }

        /// <summary>
        /// List of context menus specific to the magnifier.
        /// </summary>
        public List<ToolStripMenuItem> ContextMenu 
        { 
            get 
            {
                return contextMenu;
            }
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private BoundingBox source = new BoundingBox();         // Wrapper for the region of interest in the original image.
        private RectangleF destination;                         // Where we paint the magnified region.
        private MagnifierMode mode;
        private PointF srcLastLocation;
        private PointF dstLastLocation;
        private int hitHandle = -1;
        private float zoom = 2.0f;
        private static readonly float[] ZoomFactors = new float[] { 1.0f, 1.50f, 2.0f, 4.0f };

        private List<ToolStripMenuItem> contextMenu = new List<ToolStripMenuItem>();
        #endregion

        #region Constructor
        public Magnifier()
        {
            ResetData();

            foreach (float factor in ZoomFactors)
            {
                ToolStripMenuItem mnu = new ToolStripMenuItem();
                mnu.Text = string.Format("{0:0.0}x", factor);
                mnu.Click += (s, e) => {
                    foreach (var m in contextMenu) 
                        m.Checked = false;

                    zoom = factor;
                    ResizeDestination();
                    mnu.Checked = true;
                    InvalidateFromMenu(s);
                };

                mnu.Checked = factor == zoom;
                contextMenu.Add(mnu);
            }

            ResizeDestination();
        }
        #endregion

        #region Public interface
        public void Draw(Bitmap bitmap, Graphics canvas, ImageTransform imageTransform, bool mirrored, Size originalSize)
        {
            if(mode == MagnifierMode.None)
                return;
            
            source.Draw(canvas, imageTransform.Transform(source.Rectangle), Pens.White, (SolidBrush)Brushes.White, 4);
            DrawDestination(bitmap, canvas, imageTransform, mirrored, originalSize);
        }
        private void DrawDestination(Bitmap bitmap, Graphics canvas, ImageTransform imageTransform, bool mirrored, Size originalSize)
        {
            // The bitmap passed in is the image decoded, so it might be at a different size than the original image size.
            // We also need to take mirroring into account (until it's included in the ImageTransform).

            double scaleX = (double)bitmap.Size.Width / originalSize.Width;
            double scaleY = (double)bitmap.Size.Height / originalSize.Height;
            Rectangle srcRect = source.Rectangle.Scale(scaleX, scaleY);

            if (mirrored)
                srcRect = new Rectangle(bitmap.Width - srcRect.Left, srcRect.Top, -srcRect.Width, srcRect.Height);
            
            canvas.DrawImage(bitmap, imageTransform.Transform(destination), srcRect, GraphicsUnit.Pixel);
            canvas.DrawRectangle(Pens.White, imageTransform.Transform(destination));
        }

        public void InitializeCommit(PointF location)
        {
            if(Mode == MagnifierMode.Direct)
                Mode = MagnifierMode.Indirect;
        }
        public bool Move(PointF location)
        {
            // Currently the magnifier does not use the same move/moveHandle mechanics as other drawings.
            // (Going through the pointer tool to keep track of last mouse location and calling move or moveHandle from there)
            // Hence, we keep the last location here and recompute the deltas locally.
            if(mode == MagnifierMode.Direct || hitHandle == 0)
            {
                source.Move(location.X - srcLastLocation.X, location.Y - srcLastLocation.Y);
                srcLastLocation = location;
                points["0"] = source.Rectangle.Center();
                SignalTrackablePointMoved();
            }
            else if(hitHandle > 0 && hitHandle < 5)
            {
                source.MoveHandle(location, hitHandle, Size.Empty, false);
                points["0"] = source.Rectangle.Center();
                ResizeDestination();
                SignalTrackablePointMoved();
            }
            else if(hitHandle == 5)
            {
                destination = new RectangleF(destination.X + (location.X - dstLastLocation.X), destination.Y + (location.Y - dstLastLocation.Y), destination.Width, destination.Height);
                dstLastLocation = location;
            }

            return false;
        }
        public bool OnMouseDown(PointF location, ImageTransform transformer)
        {
            if(mode != MagnifierMode.Indirect)
                return false;

            hitHandle = HitTest(location, transformer);
            
            if(hitHandle == 0)
                srcLastLocation = location;
            else if(hitHandle == 5)
                dstLastLocation = location;

            return hitHandle >= 0;
        }
        public bool IsOnObject(PointF location, ImageTransform transformer)
        {
            return HitTest(location, transformer) >= 0;
        }
        private int HitTest(PointF point, ImageTransform transformer)
        {
            // Mapping:
            // 0: the source rectangle.
            // 1-4: the corners of the source rectangle.
            // 5: the target rendering area.

            int result = -1;
            if(destination.Contains(point))
                result = 5;
            else
                result = source.HitTest(point, transformer);

            return result;
        }
        public void ResetData()
        {
            points["0"] = PointF.Empty;
            source.Rectangle = points["0"].Box(50).ToRectangle();
            destination = new RectangleF(10, 10, (float)(source.Rectangle.Width * zoom), (float)(source.Rectangle.Height * zoom));
            
            srcLastLocation = points["0"];
            dstLastLocation = points["0"];
            
            mode = MagnifierMode.None;
        }

        /// <summary>
        /// Transform the canvas where the magnifier is drawn, into the mini canvas with only the magnified area.
        /// This is used to paint the drawings on top of the magnified area.
        /// </summary>
        public void TransformCanvas(Graphics canvas, ImageTransform transform)
        {
            float invStretch = (float)(1.0f / transform.Stretch);
            float stretch = (float)transform.Stretch;

            canvas.ScaleTransform(stretch, stretch);

            // Account for the border.
            Rectangle clip = new RectangleF(destination.X + 2, destination.Y + 2, destination.Width - 4, destination.Height - 4).ToRectangle();

            canvas.SetClip(clip);
            canvas.TranslateTransform(destination.X, destination.Y);
            canvas.ScaleTransform(zoom, zoom);
            canvas.TranslateTransform(-source.X, -source.Y);

            canvas.ScaleTransform(invStretch, invStretch);
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Guid Id
        {
            get { return id; }
        }
        public string Name
        {
            get { return ScreenManagerLang.ToolTip_Magnifier; }
        }
        public Color Color
        {
            get { return Color.Black; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            source.Rectangle = points[name].Box(source.Rectangle.Size).ToRectangle();
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
        }
        #endregion
        
        private void ResizeDestination()
        {
            destination = new RectangleF(destination.Left, destination.Top, (float)(source.Rectangle.Width * zoom), (float)(source.Rectangle.Height * zoom));
        }

        private void InvalidateFromMenu(object sender)
        {
            // Update the main viewport.
            // The screen hook was injected inside the menu.
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host == null)
                return;

            host.InvalidateFromMenu();
        }
    }
    
    public enum MagnifierMode
    {
        None,
        Direct, // When the mouse move makes the magnifier move (Initial mode).
        Indirect // When the user has to click to change the boundaries of the magnifier.
    }
}
