/*
Copyright © Joan Charmant 2008.
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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
            get { return minimum; }
            set
            {
                minimum = value;
                if (position < minimum)
                    position = minimum;
                UpdateMarkersPositions();
                UpdateSyncPointMarkerPosition();
                UpdateCursorPosition();
                Invalidate();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                if (position > maximum) position = maximum;
                UpdateMarkersPositions();
                UpdateSyncPointMarkerPosition();
                UpdateCursorPosition();
                Invalidate();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Position
        {
            get { return position; }
            set
            {
                position  = value;
                if (position < minimum) position = minimum;
                if (position > maximum) position = maximum;
                UpdateCursorPosition();
            }
        }
        public long SyncPosition
        {
            get { return syncPointTimestamp; }
        }
        public long LeftHairline
        {
            get { return leftHairline; }
            set { leftHairline = value; }
        }
        public long RightHairline
        {
            get { return rightHairline; }
            set { rightHairline = value; }
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
        
        private bool enabled = true;
        private bool commonTimeline;
        private Bitmap bmpNavCursor = Resources.liqcursor;
        private Bitmap bmpBumperLeft = Resources.liqbumperleft;
        private Bitmap bmpBumperRight = Resources.liqbumperright;
        private Bitmap bmpBackground = Resources.liqbackdock;
        
        #region Markers handling
        private Metadata metadata;


        // Lists are lists of coordinates, or of pair of coordinates (start/end) in pixels.
        
        private List<int> keyframesMarks = new List<int>();
        private static readonly Pen penKey = Pens.SeaGreen;
        private static readonly SolidBrush brushKey = new SolidBrush(Color.FromArgb(96, Color.SeaGreen));
        
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
        private static readonly Pen penSync = Pens.Firebrick;
        private static readonly SolidBrush brushSync = new SolidBrush(Color.FromArgb(96, Color.Firebrick));

        private long leftHairline;
        private long rightHairline;
        private int leftPlayHeadMark;
        private int rightPlayHeadMark;
        private static readonly Pen penPlayHead = Pens.DarkCyan;
        private static readonly SolidBrush brushPlayHead = new SolidBrush(Color.FromArgb(96, Color.DarkCyan));
        
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
        public void SetAsCommonTimeline(bool value)
        {
            this.commonTimeline = value;
        }
        public void Remap(long minimum, long maximum)
        {
            // This method is only a shortcut to updating min and max properties at once.
            // This method update the appearence of the control only, it doesn't raise the events back.
            this.minimum = minimum;
            this.maximum = maximum;
            
            if (position < minimum) position = minimum;
            if (position > maximum) position = maximum;
            
            UpdateMarkersPositions();
            UpdateSyncPointMarkerPosition();
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
            UpdateSyncPointMarkerPosition();
            UpdateCacheSegmentMarkerPosition();

            this.syncPointTimestamp = metadata.TimeOrigin;
            UpdateSyncPointMarkerPosition();

            this.Invalidate();
        }

        /// <summary>
        /// This should only be used by the common controls.
        /// </summary>
        public void UpdateSyncPointMarker(long syncPointTimestamp)
        {
            this.syncPointTimestamp = syncPointTimestamp;
            UpdateSyncPointMarkerPosition();
        }
        public void UpdatePlayHeadMarkers()
        {
            leftPlayHeadMark = 0;
            if (leftHairline >= minimum && leftHairline <= maximum)
                leftPlayHeadMark = GetCoordFromTimestamp(leftHairline);

            rightPlayHeadMark = 0;
            if (rightHairline >= minimum && rightHairline <= maximum)
                rightPlayHeadMark = GetCoordFromTimestamp(rightHairline);
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
            int x = Math.Min(Math.Max(mouseCoords.X, minimumPixel), maximumPixel);
            
            pixelPosition = x - halfCursorWidth;
            Invalidate();
            invalidateAsked = true;
            
            if (PositionChanging != null)
            {
                position = GetTimestampFromCoord(pixelPosition + halfCursorWidth);
                PositionChanging(this, new PositionChangedEventArgs(position));
            }
            else
            {
                Invalidate();
            }
        }
        private void FrameTracker_MouseUp(object sender, MouseEventArgs e)
        {
            // End of a mouse move, jump to position.
            if(!enabled || e.Button != MouseButtons.Left)
                return;
            
            Point mouseCoords = this.PointToClient(Cursor.Position);
            int x = Math.Min(Math.Max(mouseCoords.X, minimumPixel), maximumPixel);

            pixelPosition = x - halfCursorWidth;
            Invalidate();
            if (PositionChanged != null)
            { 
                position = GetTimestampFromCoord(pixelPosition + halfCursorWidth);
                PositionChanged(this, new PositionChangedEventArgs(position));
            }
        }
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
            // Resize of the control only : internal data doesn't change.
            maximumPixel = this.Width - spacers - halfCursorWidth;
            maxWidthPixel = maximumPixel - minimumPixel;
            UpdateMarkersPositions();
            UpdateSyncPointMarkerPosition();
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
            g.PixelOffsetMode = PixelOffsetMode.Half; // <-- fixes stretch.
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
                    DrawRangeMark(canvas, Pens.LightSlateGray, Brushes.LightSteelBlue, mark.X, mark.Y);

                DrawAllFrames(canvas, Pens.Black);
                int pixPos =  GetCoordFromTimestamp(position);
                canvas.DrawLine(Pens.Red, pixPos, 5, pixPos, 13);
            }
            else
            {
                foreach (Point mark in chronosMarks)
                    DrawRangeMark(canvas, penChronoBorder, brushChrono, mark.X, mark.Y);

                foreach (Point mark in tracksMarks)
                    DrawRangeMark(canvas, penTrackBorder, brushTrack, mark.X, mark.Y);

                foreach (int mark in keyframesMarks)
                    DrawFrameMark(canvas, penKey, brushKey, mark);

                DrawFrameMark(canvas, penSync, brushSync, syncPointMark);
                
                if (commonTimeline)
                {
                    DrawSideMark(canvas, penPlayHead, brushPlayHead, leftPlayHeadMark, true);
                    DrawSideMark(canvas, penPlayHead, brushPlayHead, rightPlayHeadMark, false);
                }
                else
                {
                    canvas.DrawImageUnscaled(bmpNavCursor, pixelPosition, 0);
                }
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
        private void DrawFrameMark(Graphics canvas, Pen border, SolidBrush inside, int coord)
        {
            if (coord <= 0)
                return;

            float left = coord;
            float top = 5;
            float width = 3;
            float height = 8;

            canvas.SmoothingMode = SmoothingMode.Default;

            canvas.FillRectangle(inside, left - width / 2, top + 0.5f, width, height - 0.5f);
            canvas.DrawRectangle(border, left - width / 2, top + 0.5f, width, height - 0.5f);
        }
        private void DrawSideMark(Graphics canvas, Pen border, SolidBrush inside, int coord, bool lookLeft)
        {
            if (coord <= 0)
                return;
            
            float top = 5;
            float width = 9;
            float height = 8;
            float left = lookLeft ? coord - width * 0.25f : coord - width * 0.75f;
            
            canvas.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rect = new RectangleF(left, top, width, height + 0.5f);
            
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            if (lookLeft)
                gp.AddArc(rect, 90, 180);
            else
                gp.AddArc(rect, -90, 180);

            gp.CloseFigure();

            canvas.FillPath(inside, gp);
            canvas.DrawPath(border, gp);

            gp.Dispose();
        }
        private void DrawRangeMark(Graphics canvas, Pen border, Brush inside, int start, int range)
        {
            float left = start;
            float top = 5;
            float width = range;
            float height = 8;
            
            // Bound to bumpers.
            if(left < minimumPixel) 
                left = minimumPixel;
            
            if(left + width > maximumPixel) 
                width = maximumPixel - left;

            canvas.SmoothingMode = SmoothingMode.Default;

            canvas.FillRectangle(inside, left, top + 0.5f, width, height - 0.5f);
            canvas.DrawRectangle(border, left, top + 0.5f, width, height - 0.5f);
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
            return minimumPixel + (int)Rescale(timestamp - minimum, maximum - minimum, maxWidthPixel);
        }
        private long GetTimestampFromCoord(int pos)
        {
            return minimum + Rescale(pos - minimumPixel, maxWidthPixel, maximum - minimum);
        }
        private long Rescale(long oldValue, long oldMax, long newMax)
        {
            if (oldMax <= 0)
                return 0;
            
            return (long)(Math.Round((double)oldValue / (double)oldMax * (double)newMax));
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