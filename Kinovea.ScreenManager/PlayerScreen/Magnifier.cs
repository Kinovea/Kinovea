#region License
/*
Copyright � Joan Charmant 2012.
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Picture-in-picture with magnification.
    /// </summary>
    public class Magnifier :�ITrackable
    {
        // As always, coordinates are expressed in terms of the original image size.
        // They are converted to display size at the last moment, using the CoordinateSystem transformer.
        // TODO: save positions in the KVA.
        // TODO: support for rendering unscaled.
        
        public static readonly double[] MagnificationFactors = new double[]{1.50, 1.75, 2.0, 2.25, 2.5};
        
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
        #endregion
        
        #region Properties
        public MagnifierMode Mode {
            get { return mode; }
            set { mode = value; }
        }
        public double MagnificationFactor {
            get { return magnificationFactor; }
            set { 
                magnificationFactor = value;
                ResizeInsert();
            }
        }
        public Point Center {
            get { return source.Rectangle.Center(); }
        }
        #endregion
        
        #region Members
        private Guid id = Guid.NewGuid();
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private BoundingBox source = new BoundingBox();   // Wrapper for the region of interest in the original image.
        private Rectangle insert;                         // The location and size of the insert window, where we paint the region of interest magnified.
        private MagnifierMode mode;
        private Point sourceLastLocation;
        private Point insertLastLocation;
        private int hitHandle = -1;
        private double magnificationFactor = MagnificationFactors[1];
        #endregion
       
        #region Constructor
        public Magnifier()
        {
            ResetData();
        }
        #endregion
       
        #region Public interface
        public void Draw(Bitmap bitmap, Graphics canvas, CoordinateSystem transformer, bool mirrored, Size originalSize)
        {
            if(mode == MagnifierMode.None)
                return;
            
            source.Draw(canvas, transformer.Transform(source.Rectangle), Pens.White, (SolidBrush)Brushes.White, 4);
            DrawInsert(bitmap, canvas, transformer, mirrored, originalSize);
        }
        private void DrawInsert(Bitmap bitmap, Graphics canvas, CoordinateSystem transformer, bool mirrored, Size originalSize)
        {
            // The bitmap passed in is the image decoded, so it might be at a different size than the original image size.
            // We also need to take mirroring into account (until it's included in the CoordinateSystem).
            
            double scaleX = (double)bitmap.Size.Width / originalSize.Width;
            double scaleY = (double)bitmap.Size.Height / originalSize.Height;
            Rectangle scaledSource = source.Rectangle.Scale(scaleX, scaleY);
            
            Rectangle src;
            if(mirrored)
                src = new Rectangle(bitmap.Width - scaledSource.Left, scaledSource.Top, -scaledSource.Width, scaledSource.Height);
            else
                src = scaledSource;
            
            canvas.DrawImage(bitmap, transformer.Transform(insert), src, GraphicsUnit.Pixel);
            canvas.DrawRectangle(Pens.White, transformer.Transform(insert));
        }
        public void OnMouseUp(Point location)
        {
            if(Mode == MagnifierMode.Direct)
                Mode = MagnifierMode.Indirect;
        }
        public bool Move(Point location)
        {
            // Currently the magnifier does not use the same move/moveHandle mechanics as other drawings.
            // (Going through the pointer tool to keep track of last mouse location and calling move or moveHandle from there)
            // Hence, we keep the last location here and recompute the deltas locally.
            if(mode == MagnifierMode.Direct || hitHandle == 0)
            {
                source.Move(location.X - sourceLastLocation.X, location.Y - sourceLastLocation.Y);
                sourceLastLocation = location;
                points["0"] = source.Rectangle.Center();
                SignalTrackablePointMoved();
            }
            else if(hitHandle > 0 && hitHandle < 5)
            {
                source.MoveHandle(location, hitHandle, Size.Empty, false);
                points["0"] = source.Rectangle.Center();
                ResizeInsert();
                SignalTrackablePointMoved();
            }
            else if(hitHandle == 5)
            {
                insert = new Rectangle(insert.X + (location.X - insertLastLocation.X), insert.Y + (location.Y - insertLastLocation.Y), insert.Width, insert.Height);
                insertLastLocation = location;
            }

            return false;
        }
        public bool OnMouseDown(Point location, CoordinateSystem transformer)
        {
            if(mode != MagnifierMode.Indirect)
                return false;

            hitHandle = HitTest(location, transformer);
            
            if(hitHandle == 0)
                sourceLastLocation = location;
            else if(hitHandle == 5)
                insertLastLocation = location;

            return hitHandle >= 0;
        }
        public bool IsOnObject(Point location, CoordinateSystem transformer)
        {
            return HitTest(location, transformer) >= 0;
        }
        public int HitTest(Point point, CoordinateSystem transformer)
        {
            int result = -1;
            if(insert.Contains(point))
                result = 5;
            else
                result = source.HitTest(point, transformer);

            return result;
        }
        public void ResetData()
        {
            points["0"] = Point.Empty;
            source.Rectangle = points["0"].Box(50);
            insert = new Rectangle(10, 10, (int)(source.Rectangle.Width * magnificationFactor), (int)(source.Rectangle.Height * magnificationFactor));
            
            sourceLastLocation = points["0"].ToPoint();
            insertLastLocation = points["0"].ToPoint();
            
            mode = MagnifierMode.None;
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Guid ID
        {
            get { return id; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
        }
        public void SetTrackablePointValue(string name, PointF value)
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
        
        private void ResizeInsert()
        {
            insert = new Rectangle(insert.Left, insert.Top, (int)(source.Rectangle.Width * magnificationFactor), (int)(source.Rectangle.Height * magnificationFactor));
        }
    }
    
    public enum MagnifierMode
    {
        None,
        Direct, // When the mouse move makes the magnifier move (Initial mode).
        Indirect // When the user has to click to change the boundaries of the magnifier.
    }
}
