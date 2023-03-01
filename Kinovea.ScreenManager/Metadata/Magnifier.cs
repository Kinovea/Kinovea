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
    public class Magnifier : ITrackable, IInitializable
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

        public bool Initializing
        {
            get { return Mode == MagnifierMode.Initializing; }
        }

        public bool Frozen
        {
            get { return frozen; }
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
        private ManipulationType manipulationType;
        private int hitHandle = -1;
        private float zoom = 2.0f;
        private List<ToolStripMenuItem> contextMenu = new List<ToolStripMenuItem>();
        private Bitmap frozenBitmap;
        private bool frozen;
        private static readonly float[] ZoomFactors = new float[] { 1.0f, 1.50f, 2.0f, 4.0f };
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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

        #region Manipulation / Hand tool
        public bool OnMouseDown(PointF location, ImageTransform transformer)
        {
            // Equivalent to DrawingToolPointer.OnMouseDown.
            if (mode != MagnifierMode.Active)
                return false;

            hitHandle = HitTest(location, transformer);
            log.DebugFormat("magnifier on mouse down, hit handle = {0}", hitHandle);

            // Keep track of hit handle, manipulation type and location for when
            // we come back during mouse move.

            if (hitHandle < 0)
            {
                manipulationType = ManipulationType.None;
                return false;
            }

            if (hitHandle == 0)
            {
                manipulationType = ManipulationType.Move;
                srcLastLocation = location;
                log.DebugFormat("magnifier, srcLocation:{0}", srcLastLocation.ToString());
            }
            else
            {
                manipulationType = ManipulationType.Resize;

                if (hitHandle == 5)
                    dstLastLocation = location;
            }

            return true;
        }
        public bool OnMouseMove(PointF location, Keys modifiers)
        {
            // Equivalent to DrawingToolPointer.OnMouseMove.
            float dx = location.X - srcLastLocation.X;
            float dy = location.Y - srcLastLocation.Y;

            if (dx == 0 && dy == 0)
                return false;

            bool isMovingAnObject = true;
            int resizingHandle = hitHandle;

            switch (manipulationType)
            {
                case ManipulationType.Move:
                    {
                        this.MoveDrawing(dx, dy, modifiers);
                        srcLastLocation = location;
                        break;
                    }
                case ManipulationType.Resize:
                    this.MoveHandle(location, resizingHandle, modifiers);
                    break;
                default:
                    isMovingAnObject = false;
                    break;
            }

            return isMovingAnObject;
        }
        public void OnMouseUp()
        {
            // Equivalent to DrawingToolPointer.OnMouseUp.
            manipulationType = ManipulationType.None;
            hitHandle = -1;
        }
        #endregion

        #region Draw, hit testing and manipulation.
        public void Draw(Bitmap bitmap, Graphics canvas, ImageTransform imageTransform, bool mirrored, Size referenceSize)
        {
            if(mode == MagnifierMode.Inactive)
                return;
            
            source.Draw(canvas, imageTransform.Transform(source.Rectangle), Pens.White, (SolidBrush)Brushes.White, 4);
            if (frozen)
                DrawDestination(frozenBitmap, canvas, imageTransform, mirrored, referenceSize);
            else
                DrawDestination(bitmap, canvas, imageTransform, mirrored, referenceSize);
        }
        private void DrawDestination(Bitmap bitmap, Graphics canvas, ImageTransform imageTransform, bool mirrored, Size referenceSize)
        {
            // The bitmap passed in is the image decoded, so it might be at a different size than the original image size.
            // We also need to take mirroring into account (until it's included in the ImageTransform).

            float scaleX = (float)bitmap.Size.Width / referenceSize.Width;
            float scaleY = (float)bitmap.Size.Height / referenceSize.Height;
            Rectangle srcRect = source.Rectangle.Scale(scaleX, scaleY);

            if (mirrored)
                srcRect = new Rectangle(bitmap.Width - srcRect.Left, srcRect.Top, -srcRect.Width, srcRect.Height);
            
            canvas.DrawImage(bitmap, imageTransform.Transform(destination), srcRect, GraphicsUnit.Pixel);
            canvas.DrawRectangle(Pens.White, imageTransform.Transform(destination));
        }
        /// <summary>
        /// Mapping: -1: not hit, 0: source rectangle, 1-4: corners of source, 5: destination rectangle.
        /// </summary>
        public int HitTest(PointF point, ImageTransform transformer)
        {
            if (destination.Contains(point))
                return 5;

            return source.HitTest(point, transformer);
        }
        public void MoveHandle(PointF point, int handle, Keys modifiers)
        {
            if (handle > 0 && handle < 5)
            {
                source.MoveHandle(point, handle, Size.Empty, false);
                points["0"] = source.Rectangle.Center();
                ResizeDestination();
                SignalTrackablePointMoved();
            }
            else if (handle == 5)
            {
                destination.Location = new PointF(
                    destination.X + (point.X - dstLastLocation.X),
                    destination.Y + (point.Y - dstLastLocation.Y));

                dstLastLocation = point;
            }
        }
        public void MoveDrawing(float dx, float dy, Keys modifiersKeys)
        {
            source.Move(dx, dy);
            points["0"] = source.Rectangle.Center();
            SignalTrackablePointMoved();
        }
        #endregion

        #region IInitializable implementation
        public void InitializeMove(PointF point, Keys modifiers)
        {
            source.Move(point.X - srcLastLocation.X, point.Y - srcLastLocation.Y);
            srcLastLocation = point;
            points["0"] = source.Rectangle.Center();
            SignalTrackablePointMoved();
        }
        public string InitializeCommit(PointF location)
        {
            if (Mode == MagnifierMode.Initializing)
                Mode = MagnifierMode.Active;

            return null;
        }

        public string InitializeEnd(bool cancelCurrentPoint)
        {
            return null;
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

        public void ResetData()
        {
            Unfreeze();
            zoom = 2.0f;
            points["0"] = PointF.Empty;
            source.Rectangle = points["0"].Box(50).ToRectangle();
            destination = new RectangleF(10, 10, source.Rectangle.Width * zoom, source.Rectangle.Height * zoom);

            srcLastLocation = points["0"];
            dstLastLocation = points["0"];

            mode = MagnifierMode.Inactive;
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

        public void Freeze(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            frozenBitmap = BitmapHelper.Copy(bitmap);
            frozen = true;
        }

        public void Unfreeze()
        {
            if (!frozen)
                return;

            frozenBitmap.Dispose();
            frozen = false;
        }

        private void ResizeDestination()
        {
            destination.Size = new SizeF(source.Rectangle.Width * zoom, source.Rectangle.Height * zoom);
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
        Inactive,

        // The magnifier was just added and is being moved around.
        Initializing,

        // Normal mode.
        Active
    }
}
