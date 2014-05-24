/*
Copyright © Joan Charmant 2008.
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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using AForge.Imaging.Filters;
using Kinovea.Base;
using Kinovea.ScreenManager.Properties;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A control to let the user specify the current position in the video.
    /// The control is comprised of a cursor and a list of markers.
    /// 
    /// When control is modified by user:
    /// - The internal data is modified.
    /// - Events are raised, which are listened to by parent control.
    /// - Parent control update its own internal data state by reading the properties.
    /// 
    /// When control appearence needs to be updated
    /// - This is when internal data of the parent control have been modified by other means.
    /// - (At initialization for example)
    /// - The public properties setters are provided, they doesn't raise the events back.
    /// </summary>
    public partial class FrameTracker : UserControl
    {
        #region Properties
        [Category("Behavior"), Browsable(true)]
        public long Minimum
        {
            get{return minimum;}
            set
            {
                minimum = value;
                if (position < minimum) position = minimum;
                UpdateMarkersPositions();
                UpdateCursorPosition();
                Invalidate();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Maximum
        {
            get{return maximum;}
            set
            {
                maximum = value;
                if (position > maximum) position = maximum;
                UpdateMarkersPositions();
                UpdateCursorPosition();
                Invalidate();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Position
        {
            get{return position;}
            set
            {
                position  = value;
                if (position < minimum) position = minimum;
                if (position > maximum) position = maximum;
                UpdateCursorPosition();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool ReportOnMouseMove
        {
            get { return reportOnMouseMove;  }
            set { reportOnMouseMove = value; }
        }
        #endregion
            
        #region Members
        private bool invalidateAsked;	// To prevent reentry in MouseMove before the paint event has been honored.	
        private long minimum;			// In absolute timestamps.
        private long position;			// In absolute timestamps.
        private long maximum;			// In absolute timestamps.
        
        private int maxWidthPixel;		// Number of pixels in the control that can be used for position.
        private int minimumPixel;
        private int maximumPixel;
        private int pixelPosition;		// Left of the cursor in pixels.
        
        private int halfCursorWidth = Resources.liqcursor.Width / 2;
        private int spacers = 10;
        
        private bool reportOnMouseMove = false;
        private bool enabled = true;
        private Bitmap bmpNavCursor = Resources.liqcursor;
        private Bitmap bmpBumperLeft = Resources.liqbumperleft;
        private Bitmap bmpBumperRight = Resources.liqbumperright;
        private Bitmap bmpBackground = Resources.liqbackdock;
        
        #region Markers handling
        private Metadata metadata;
        
        // Lists are lists of coordinates, or of pair of coordinates (start/end) in pixels.
        
        private List<int> keyframesMarks = new List<int>();
        private static readonly Pen penKeyBorder = Pens.YellowGreen;
        private static readonly Pen penKeyInside = new Pen(Color.FromArgb(96, Color.YellowGreen), 1);
        
        private List<Point> chronosMarks = new List<Point>();
        private static readonly Pen penChronoBorder = Pens.CornflowerBlue;
        private static readonly SolidBrush brushChrono = new SolidBrush(Color.FromArgb(96, Color.CornflowerBlue));
        
        private List<Point> tracksMarks = new List<Point>();
        private static readonly Pen penTrackBorder = Pens.Plum; 
        private static readonly SolidBrush brushTrack = new SolidBrush(Color.FromArgb(96, Color.Plum));
        
        private VideoSection cacheSegment;
        private List<Point> cacheSegmentMarks = new List<Point>();
        private static readonly Pen penCacheBorder = Pens.DarkGray;
        private static readonly SolidBrush brushCache = new SolidBrush(Color.FromArgb(96, Color.DarkGray));
        
        private long syncPointTimestamp;
        private int syncPointMark;
        private static readonly Pen penSyncBorder = Pens.Firebrick;
        private static readonly Pen penSyncInside = new Pen(Color.FromArgb(96, Color.Firebrick), 1);
        #endregion
        
        private static readonly bool prebufferDisplay = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events Delegates
        [Category("Action"), Browsable(true)]
        public event EventHandler<PositionChangedEventArgs> PositionChanging;
        [Category("Action"), Browsable(true)]
        public event EventHandler<PositionChangedEventArgs> PositionChanged;
        #endregion

        #region Constructor
        public FrameTracker()
        {
            InitializeComponent();
            
            // Activates double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            this.Cursor = Cursors.Hand;
            
            minimumPixel = spacers + halfCursorWidth;
            maximumPixel = this.Width - spacers - halfCursorWidth;
            maxWidthPixel = maximumPixel - minimumPixel;
            
            // Prepare the images resources for faster painting.
            //bmpBumperLeft = Resources.liqbumperleft..to32bppPArgb();
        }
        #endregion
        
        #region Public Methods
        public void Remap(long minimum, long maximum)
        {
            // This method is only a shortcut to updating min and max properties at once.
            // This method update the appearence of the control only, it doesn't raise the events back.
            this.minimum = minimum;
            this.maximum = maximum;
            
            if (position < minimum) position = minimum;
            if (position > maximum) position = maximum;
            
            UpdateMarkersPositions();
            UpdateCursorPosition();
            Invalidate();
        }
        public void EnableDisable(bool enable)
        {
            this.enabled = enable;
            Invalidate();
        }
        public void UpdateMarkers(Metadata metadata)
        {
            // Keep a ref on the Metadata object so we can update the
            // markers position when only the size of the control changes.
            
            this.metadata = metadata;
            UpdateMarkersPositions();
            UpdateCacheSegmentMarkerPosition();
        }
        public void UpdateSyncPointMarker(long syncPointTimestamp)
        {
            this.syncPointTimestamp = syncPointTimestamp;
            UpdateSyncPointMarkerPosition();
        }
        public void UpdateCacheSegmentMarker(VideoSection cacheSegment)
        {
            if(!cacheSegment.IsEmpty && prebufferDisplay)
            {
                this.cacheSegment = cacheSegment;
                UpdateCacheSegmentMarkerPosition();
            }
        }
        #endregion
        
        #region Event Handlers - User Manipulation
        private void FrameTracker_MouseMove(object sender, MouseEventArgs e)
        {
            // Note: also raised on mouse down.
            // User wants to jump to position. Update the cursor and optionnaly the image.
            if(!enabled || invalidateAsked || e.Button != MouseButtons.Left)
                return;
            
            Point mouseCoords = this.PointToClient(Cursor.Position);
            
            if ((mouseCoords.X > minimumPixel) && (mouseCoords.X < maximumPixel))
            {
                pixelPosition = mouseCoords.X - halfCursorWidth;
                Invalidate();
                invalidateAsked = true;
            
                if (reportOnMouseMove && PositionChanging != null)
                {
                    position = GetTimestampFromCoord(pixelPosition + halfCursorWidth);
                    PositionChanging(this, new PositionChangedEventArgs(position));
                }
                else
                {
                    Invalidate();
                }
            }
        }
        private void FrameTracker_MouseUp(object sender, MouseEventArgs e)
        {
            // End of a mouse move, jump to position.
            if(!enabled || e.Button != MouseButtons.Left)
                return;
            
            Point mouseCoords = this.PointToClient(Cursor.Position);
            
            if ((mouseCoords.X > minimumPixel) && (mouseCoords.X < maximumPixel))
            {
                pixelPosition = mouseCoords.X - halfCursorWidth;
                Invalidate();
                if (PositionChanged != null)
                { 
                    position = GetTimestampFromCoord(pixelPosition + halfCursorWidth);
                    PositionChanged(this, new PositionChangedEventArgs(position));
                }
            }
        }
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
            // Resize of the control only : internal data doesn't change.
            maximumPixel = this.Width - spacers - halfCursorWidth;
            maxWidthPixel = maximumPixel - minimumPixel;
            UpdateMarkersPositions();
            UpdateCursorPosition();
            Invalidate();
        }
        #endregion

        #region Painting
        private void FrameTracker_Paint(object sender, PaintEventArgs e)
        {
            // When we land in this function, pixelPosition should have been set already.
            // It is the only member variable we'll use here.
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.Half; // <-- fix stretch.
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            Draw(g);
            
            invalidateAsked = false;
        }
        private void Draw(Graphics canvas)
        {
            // Background. (Note: it's faster to draw stretched than multiple tiles).
            canvas.DrawImage(bmpBackground, 22, 0, Width-40, bmpBackground.Height-1);
            
            // Bumpers.
            canvas.DrawImageUnscaled(bmpBumperLeft, 10, 0);
            canvas.DrawImageUnscaled(bmpBumperRight, Width-20, 0);
            
            if(!enabled)
                return;
            
            if(prebufferDisplay)
            {
                foreach(Point mark in cacheSegmentMarks)
                    DrawMark(canvas, Pens.LightSlateGray, Brushes.LightSteelBlue, mark);

                DrawAllFrames(canvas, Pens.Black);
                int pixPos =  GetCoordFromTimestamp(position);
                canvas.DrawLine(Pens.Red, pixPos, 5, pixPos, 13);
            }
            else
            {
                foreach (int mark in keyframesMarks)
                    DrawMark(canvas, penKeyBorder, penKeyInside, mark);

                foreach (Point mark in chronosMarks)
                    DrawMark(canvas, penChronoBorder, brushChrono, mark);

                foreach (Point mark in tracksMarks)
                    DrawMark(canvas, penTrackBorder, brushTrack, mark);

                if(syncPointMark != 0)
                    DrawMark(canvas, penSyncBorder, penSyncInside, syncPointMark);

                canvas.DrawImageUnscaled(bmpNavCursor, pixelPosition, 0);
            }
        }
        private void DrawAllFrames(Graphics canvas, Pen pen)
        {
            if(metadata == null || metadata.AverageTimeStampsPerFrame < 1)
                return;
            
            long totalTs = maximum - minimum + 1;
            long totalFrames = totalTs / metadata.AverageTimeStampsPerFrame;
            
            while(totalFrames > (maximumPixel/5))
                totalFrames /= 10;
            
            float interval = (float)maxWidthPixel / totalFrames;
            for(long i = 0;i<totalFrames;i++)
            {
                int pixPos = minimumPixel + (int)(interval * i);
                canvas.DrawLine(pen, pixPos, 5, pixPos, 13);
            }
        }
        private void DrawMark(Graphics canvas, Pen border, Pen inside, int coord)
        {
            // Mark for a single point in time (key image).
            if(coord <= 0)
                return;
            
            int iLeft = coord;
            int iTop = 5;
            int iWidth = 3;
            int iHeight = 8;
            
            canvas.DrawRectangle(border, iLeft, iTop, iWidth, iHeight );
            canvas.DrawRectangle(inside, iLeft + 1, iTop+1, iWidth-2, iHeight-2 );
        }
        private void DrawMark(Graphics canvas, Pen border, Brush inside, Point coords)
        {
            // Mark for a range in time (chrono or track).
            int iLeft = coords.X;
            int iTop = 5;
            int iWidth = coords.Y;
            int iHeight = 8;
            
            // Bound to bumpers.
            if(iLeft < minimumPixel) iLeft = minimumPixel;
            if(iLeft + iWidth > maximumPixel) iWidth = maximumPixel - iLeft;
            
            canvas.FillRectangle(inside, iLeft, iTop, iWidth, iHeight);
            canvas.DrawRectangle(border, iLeft, iTop, iWidth, iHeight );
        }
        #endregion
        
        #region Binding UI to Data
        private void UpdateCursorPosition()
        {
            pixelPosition = GetCoordFromTimestamp(position) - halfCursorWidth;
        }
        private void UpdateMarkersPositions()
        {
            // Translate timestamps into control coordinates and store the coordinates of the
            // markers to draw them later.
            // Should only be called when either the timestamps or the control size changed.
            if(metadata == null)
                return;
            
            // Key frames
            keyframesMarks.Clear();
            foreach(Keyframe kf in metadata.Keyframes)
            {
                // Only display Key image that are in the selection.
                if(kf.Position >= minimum && kf.Position <= maximum)
                {
                    keyframesMarks.Add(GetCoordFromTimestamp(kf.Position));
                }
            }
            
            // ExtraDrawings
            // We will store the range coords in a Point object, to get a couple of ints structure.
            // X will be the left coordinate, Y the width.
            chronosMarks.Clear();
            tracksMarks.Clear();
            foreach (DrawingChrono chrono in metadata.ChronoManager.Drawings)
            {
                if (chrono.TimeStart != long.MaxValue && chrono.TimeStop != long.MaxValue)
                {
                    // Only chronos that have an end and something inside the selection.
                    if (chrono.TimeStart <= maximum && chrono.TimeStop >= minimum)
                        chronosMarks.Add(GetMarkerRange(chrono.TimeStart, chrono.TimeStop));
                }
            }

            foreach (DrawingTrack track in metadata.TrackManager.Drawings)
            {
                if (track == null)
                    continue;

                if (track.BeginTimeStamp <= maximum && track.EndTimeStamp >= minimum)
                    tracksMarks.Add(GetMarkerRange(track.BeginTimeStamp, track.EndTimeStamp));
            }
        }
        private void UpdateSyncPointMarkerPosition()
        {
            syncPointMark = 0;
            if(syncPointTimestamp != 0 && syncPointTimestamp >= minimum && syncPointTimestamp <= maximum)
                syncPointMark = GetCoordFromTimestamp(syncPointTimestamp);
        }
        private void UpdateCacheSegmentMarkerPosition()
        {
            cacheSegmentMarks.Clear();
            if(cacheSegment.Wrapped)
            {
                cacheSegmentMarks.Add(GetMarkerRange(minimum, cacheSegment.End));
                cacheSegmentMarks.Add(GetMarkerRange(cacheSegment.Start, maximum));
            }
            else
            {
                cacheSegmentMarks.Add(GetMarkerRange(cacheSegment.Start, cacheSegment.End));
            }
        }
        private Point GetMarkerRange(long start, long stop)
        {
            long startTs = Math.Max(start, minimum);
            long stopTs = Math.Min(stop, maximum);
            int newStart = GetCoordFromTimestamp(startTs);
            int newStop = GetCoordFromTimestamp(stopTs);
            
            return new Point(newStart, newStop - newStart);
        }
        private int GetCoordFromTimestamp(long timestamp)
        {
            return minimumPixel + Rescale(timestamp - minimum, maximum - minimum, maxWidthPixel);
        }
        private long GetTimestampFromCoord(int pos)
        {
            return minimum + Rescale(pos - minimumPixel, maxWidthPixel, maximum - minimum);
        }
        private int Rescale(long oldValue, long oldMax, long newMax)
        {
            if(oldMax > 0)
                return (int)(Math.Round((double)((double)oldValue * (double)newMax) / (double)oldMax));
            else
                return 0;
        }
        #endregion
    }
    
    public class PositionChangedEventArgs : EventArgs
    {
        public readonly long Position;
        public PositionChangedEventArgs(long position)
        {
            this.Position = position;
        }
    }
}