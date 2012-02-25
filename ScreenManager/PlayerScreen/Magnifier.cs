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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Picture-in-picture with magnification.
    /// </summary>
    public class Magnifier
    {
        // As always, coordinates are expressed in terms of the original image size.
        // They are converted to display size at the last moment, using the CoordinateSystem transformer.
        // TODO: save positions in the KVA.
        // TODO: support for rendering unscaled.
        
        public static readonly double[] MagnificationFactors = new double[]{1.50, 1.75, 2.0, 2.25, 2.5};
        
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
        private BoundingBox m_source = new BoundingBox();   // Wrapper for the region of interest in the original image.
        private Rectangle m_insert;                         // The location and size of the insert window, where we paint the region of interest magnified.
        private MagnifierMode m_mode;// = MagnifierMode.None;
        private Point m_sourceLastLocation;
        private Point m_insertLastLocation;
        private int m_hitHandle = -1;
        private double m_magnificationFactor = MagnificationFactors[1];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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
            Rectangle scaledSource = new Rectangle((int)(m_source.Rectangle.Left * scaleX), (int)(m_source.Rectangle.Top * scaleY), (int)(m_source.Rectangle.Width * scaleX), (int)(m_source.Rectangle.Height * scaleY));
            
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
            }
            else if(m_hitHandle > 0 && m_hitHandle < 5)
            {
                m_source.MoveHandle(_location, m_hitHandle, Size.Empty, false);
                ResizeInsert();
            }
            else if(m_hitHandle == 5)
            {
                m_insert = new Rectangle(m_insert.X + (_location.X - m_insertLastLocation.X), m_insert.Y + (_location.Y - m_insertLastLocation.Y), m_insert.Width, m_insert.Height);
                m_insertLastLocation = _location;
            }
            return false;
        }
        public bool OnMouseDown(Point _location)
        {
            if(m_mode != MagnifierMode.Indirect)
                return false;

            m_hitHandle = HitTest(_location);
            
            if(m_hitHandle == 0)
                m_sourceLastLocation = _location;
            else if(m_hitHandle == 5)
                m_insertLastLocation = _location;

            return m_hitHandle >= 0;
        }
        public bool IsOnObject(Point _location)
        {
            return HitTest(_location) >= 0;
        }
        public int HitTest(Point _location)
        {
            // Hit results : 
            // -1: nothing.
            // 0: source rectangle.
            // 1 to 4: source corners, clockwise starting top-left.
            // 5: insert picture.
            
            int hit = -1;
            if(m_insert.Contains(_location))
                hit = 5;
            else
                hit = m_source.HitTest(_location);

            return hit;
        }
        public void ResetData()
        {
            Size defaultSize = new Size(100, 100);
            m_source.Rectangle = new Rectangle(- (defaultSize.Width / 2), - (defaultSize.Height / 2), defaultSize.Width, defaultSize.Height);
            m_insert = new Rectangle(10, 10, (int)(m_source.Rectangle.Width * m_magnificationFactor), (int)(m_source.Rectangle.Height * m_magnificationFactor));
            
            m_sourceLastLocation = Point.Empty;
            m_insertLastLocation = Point.Empty;
            
            m_mode = MagnifierMode.None;
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
