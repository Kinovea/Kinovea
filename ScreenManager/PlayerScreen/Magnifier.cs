#region License
/*
Copyright © Joan Charmant 2012.
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
    public class Magnifier : ITrackable
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
            get { return m_mode; }
            set { m_mode = value; }
        }
        public double MagnificationFactor {
            get { return m_magnificationFactor; }
            set { 
                m_magnificationFactor = value;
                ResizeInsert();
            }
        }
        public Point Center {
            get { return m_source.Rectangle.Center(); }
        }
        #endregion
        
        #region Members
        private Guid id = Guid.NewGuid();
    	private Dictionary<string, Point> points = new Dictionary<string, Point>();
    	
        private BoundingBox m_source = new BoundingBox();   // Wrapper for the region of interest in the original image.
        private Rectangle m_insert;                         // The location and size of the insert window, where we paint the region of interest magnified.
        private MagnifierMode m_mode;
        private Point m_sourceLastLocation;
        private Point m_insertLastLocation;
        private int m_hitHandle = -1;
        private double m_magnificationFactor = MagnificationFactors[1];
        #endregion
       
        #region Constructor
        public Magnifier()
        {
            ResetData();
        }
        #endregion
       
        #region Public interface
        public void Draw(Bitmap _bitmap, Graphics _canvas, CoordinateSystem _transformer, bool _bMirrored, Size _originalSize)
        {
            if(m_mode == MagnifierMode.None)
                return;
            
            m_source.Draw(_canvas, _transformer.Transform(m_source.Rectangle), Pens.White, (SolidBrush)Brushes.White, 4);
            DrawInsert(_bitmap, _canvas, _transformer, _bMirrored, _originalSize);
        }
        private void DrawInsert(Bitmap _bitmap, Graphics _canvas, CoordinateSystem _transformer, bool _bMirrored, Size _originalSize)
        {
            // The bitmap passed in is the image decoded, so it might be at a different size than the original image size.
            // We also need to take mirroring into account (until it's included in the CoordinateSystem).
            
            double scaleX = (double)_bitmap.Size.Width / _originalSize.Width;
            double scaleY = (double)_bitmap.Size.Height / _originalSize.Height;
            Rectangle scaledSource = m_source.Rectangle.Scale(scaleX, scaleY);
            
            Rectangle src;
            if(_bMirrored)
            	src = new Rectangle(_bitmap.Width - scaledSource.Left, scaledSource.Top, -scaledSource.Width, scaledSource.Height);
            else
            	src = scaledSource;
            
            _canvas.DrawImage(_bitmap, _transformer.Transform(m_insert), src, GraphicsUnit.Pixel);
            _canvas.DrawRectangle(Pens.White, _transformer.Transform(m_insert));
        }
        public void OnMouseUp(Point _location)
        {
            if(Mode == MagnifierMode.Direct)
                Mode = MagnifierMode.Indirect;
        }
        public bool Move(Point _location)
        {
            // Currently the magnifier does not use the same move/moveHandle mechanics as other drawings.
            // (Going through the pointer tool to keep track of last mouse location and calling move or moveHandle from there)
            // Hence, we keep the last location here and recompute the deltas locally.
            if(m_mode == MagnifierMode.Direct || m_hitHandle == 0)
            {
                m_source.Move(_location.X - m_sourceLastLocation.X, _location.Y - m_sourceLastLocation.Y);
                m_sourceLastLocation = _location;
                points["0"] = m_source.Rectangle.Center();
                SignalTrackablePointMoved();
            }
            else if(m_hitHandle > 0 && m_hitHandle < 5)
            {
                m_source.MoveHandle(_location, m_hitHandle, Size.Empty, false);
                points["0"] = m_source.Rectangle.Center();
                ResizeInsert();
                SignalTrackablePointMoved();
            }
            else if(m_hitHandle == 5)
            {
                m_insert = new Rectangle(m_insert.X + (_location.X - m_insertLastLocation.X), m_insert.Y + (_location.Y - m_insertLastLocation.Y), m_insert.Width, m_insert.Height);
                m_insertLastLocation = _location;
            }
            return false;
        }
        public bool OnMouseDown(Point location, CoordinateSystem transformer)
        {
            if(m_mode != MagnifierMode.Indirect)
                return false;

            m_hitHandle = HitTest(location, transformer);
            
            if(m_hitHandle == 0)
                m_sourceLastLocation = location;
            else if(m_hitHandle == 5)
                m_insertLastLocation = location;

            return m_hitHandle >= 0;
        }
        public bool IsOnObject(Point _location, CoordinateSystem transformer)
        {
            return HitTest(_location, transformer) >= 0;
        }
        public int HitTest(Point point, CoordinateSystem transformer)
        {
            int result = -1;
            if(m_insert.Contains(point))
                result = 5;
            else
                result = m_source.HitTest(point, transformer);

            return result;
        }
        public void ResetData()
        {
            points["0"] = Point.Empty;
            m_source.Rectangle = points["0"].Box(50);
            m_insert = new Rectangle(10, 10, (int)(m_source.Rectangle.Width * m_magnificationFactor), (int)(m_source.Rectangle.Height * m_magnificationFactor));
            
            m_sourceLastLocation = points["0"];
            m_insertLastLocation = points["0"];
            
            m_mode = MagnifierMode.None;
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Guid ID
        {
            get { return id; }
        }
        public Dictionary<string, Point> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
        }
        public void SetTrackablePointValue(string name, Point value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            m_source.Rectangle = points[name].Box(m_source.Rectangle.Size);
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
            m_insert = new Rectangle(m_insert.Left, m_insert.Top, (int)(m_source.Rectangle.Width * m_magnificationFactor), (int)(m_source.Rectangle.Height * m_magnificationFactor));
        }
    }
    
    public enum MagnifierMode
    {
        None,
        Direct, 		// When the mouse move makes the magnifier move (Initial mode).
        Indirect    	// When the user has to click to change the boundaries of the magnifier.
    }
}
