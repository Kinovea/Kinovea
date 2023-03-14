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
using Kinovea.Services;
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
    /// - The public properties setters are provided, they don't raise the events back.
    /// </summary>
    public partial class FrameTracker : UserControl
    {
        private enum MarkerType
        {
            Metadata,
            Cache,
        }

        #region Properties
        /// <summary>
        /// The smallest timestamp of the current selection, in absolute timestamps.
        /// </summary>
        public long Minimum
        {
            get { return minTimestamp; }
            set
            {
                minTimestamp = value;
                curTimestamp = Math.Max(curTimestamp, minTimestamp);
                UpdateCachesMarkersPosition();
                UpdateMarkersPositions();
                UpdateSyncPointMarkerPosition();
                UpdateCursorPosition();
                Invalidate();
            }
        }

        /// <summary>
        /// The largest timestamp of the current selection, in absolute timestamps.
        /// </summary>
        public long Maximum
        {
            get { return maxTimestamp; }
            set
            {
                maxTimestamp = value;
                curTimestamp = Math.Min(curTimestamp, maxTimestamp);
                UpdateCachesMarkersPosition();
                UpdateMarkersPositions();
                UpdateSyncPointMarkerPosition();
                UpdateCursorPosition();
                Invalidate();
            }
        }

        /// <summary>
        /// The current timestamp, in absolute timestamps.
        /// </summary>
        public long Position
        {
            get { return curTimestamp; }
            set
            {
                curTimestamp = Clamp(value, minTimestamp, maxTimestamp);
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
        /// <summary>
        /// The position of the center of the cursor block.
        /// </summary>
        public int PixelPosition
        {
            get 
            {
                return TimestampToPixel(curTimestamp + (tsPerFrame / 2));
            }
        }
        #endregion
            
        #region Members
        private bool invalidateAsked;	        // Used to prevent reentry in MouseMove before the paint event has been honored.	
        private long minTimestamp;
        private long curTimestamp;
        private long maxTimestamp;
        private long tsPerFrame;

        private int gutterLeft;                             // Start of the mapped area of the gutter.
        private int gutterRight;                            // End of the mapped area of the gutter, this includes the interval of the last frame.
        private static readonly int gutterMargin = 8;       // Margin between the control edge and the start of the gutter endpoint.
        private static readonly int gutterUnusable = 14;    // Width of the unusable area at the gutter ends.
        private static readonly int gutterTop = 5;          // Markers can start drawing from here.
        private static readonly int gutterHeight = 10;      // Markers can draw this height.
        private static readonly int gutterHeightCache = 4;  // Cache markers can draw this height.
        private static readonly int frameMarkerWidth = 5;   // Fixed width for frame markers. Also the minimal width of the cursor.
        private int cursorLeft;                             // The cursor is left-aligned with frame intervals.
        private int cursorWidth = 30;                       // The width of the cursor is one frame interval.
        
        private bool enabled = true;
        private bool isCommonTimeline;
        private Bitmap bmpGutterLeft = Resources.gutter_left;
        private Bitmap bmpGutterRight = Resources.gutter_right;
        private Bitmap bmpGutterCenter = Resources.gutter_center;
        
        #region Markers handling
        private Metadata metadata;

        // Markers coordinates and colors.
        private List<Pair<int, Color>> keyframesMarks = new List<Pair<int, Color>>();
        private List<Pair<Point, Color>> chronosMarks = new List<Pair<Point, Color>>();
        private List<Pair<Point, Color>> tracksMarks = new List<Pair<Point, Color>>();
        private VideoSection cacheSegment;
        private List<Pair<Point, Color>> cacheMarks = new List<Pair<Point, Color>>();
        private bool showCacheMarkers = false;
        private long syncPointTimestamp;
        private Pair<int, Color> syncPointMark;
        private long leftHairline;
        private long rightHairline;
        private int leftPlayHeadMark;
        private int rightPlayHeadMark;

        // Standard colors.
        private static readonly Pen penPlayHead = Pens.DarkCyan;
        private static readonly SolidBrush brushPlayHead = new SolidBrush(Color.FromArgb(96, Color.DarkCyan));
        #endregion
        
        private static readonly bool prebufferDisplay = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events
        public event EventHandler<TimeEventArgs> PositionChanging;
        public event EventHandler<TimeEventArgs> PositionChanged;
        public event EventHandler KeyframeDropped;
        #endregion

        #region Constructor
        public FrameTracker()
        {
            InitializeComponent();
            
            // Activates double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            this.Cursor = Cursors.Hand;

            // Initialize the mapped area.
            // gutterLeft should never change but gutterRight should be updated when the control is resized.
            // Note: cursorWidth depends on the size of the mapped area.
            gutterLeft = gutterMargin + gutterUnusable;
            gutterRight = this.Width - gutterMargin - gutterUnusable;

            this.AllowDrop = true;
        }
        #endregion
        
        #region Public Methods
        public void SetAsCommonTimeline(bool isCommonTimeline)
        {
            this.isCommonTimeline = isCommonTimeline;
        }

        /// <summary>
        /// Update the appearance of the control after the selection end points were changed.
        /// Does not raise events back.
        /// </summary>
        public void Remap(long minTimestamp, long maxTimeStamp, long tsPerFrame)
        {
            // This method is only a shortcut to updating min and max properties at once.
            // This method update the appearence of the control only, it doesn't raise the events back.
            this.minTimestamp = minTimestamp;
            this.maxTimestamp = maxTimeStamp;
            this.tsPerFrame = tsPerFrame;
            curTimestamp = Clamp(curTimestamp, minTimestamp, maxTimeStamp);
            cursorWidth = (int)Rescale(tsPerFrame, 0, maxTimestamp - minTimestamp, 0, gutterRight - gutterLeft);
            cursorWidth = Math.Max(cursorWidth, frameMarkerWidth);
            
            // Make room for one more frame so the gutter contains the interval of the last frame.
            this.maxTimestamp += tsPerFrame;
            UpdateCachesMarkersPosition();
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
            UpdateCachesMarkersPosition();
            UpdateMarkersPositions();
            UpdateSyncPointMarkerPosition();

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
            if (leftHairline >= minTimestamp && leftHairline <= maxTimestamp)
                leftPlayHeadMark = TimestampToPixel(leftHairline);

            rightPlayHeadMark = 0;
            if (rightHairline >= minTimestamp && rightHairline <= maxTimestamp)
                rightPlayHeadMark = TimestampToPixel(rightHairline);
        }

        public void UpdateCacheSegmentMarker(VideoSection cacheSegment)
        {
           this.cacheSegment = cacheSegment;
           UpdateCachesMarkersPosition();
        }
        #endregion
        
        #region Event Handlers - User Manipulation
        private void FrameTracker_MouseMove(object sender, MouseEventArgs e)
        {
            // Note: also raised on mouse down.
            // User wants to jump to position. Update the cursor and optionnaly the image.
            if(!enabled || invalidateAsked || e.Button != MouseButtons.Left)
                return;

            Scrub();
        }
        private void FrameTracker_MouseUp(object sender, MouseEventArgs e)
        {
            // End of a mouse move, jump to position.
            if(!enabled || e.Button != MouseButtons.Left)
                return;

            Commit();
        }
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
            // Resize of the control only : internal data doesn't change.
            gutterRight = this.Width - gutterMargin - gutterUnusable;
            cursorWidth = (int)Rescale(tsPerFrame, 0, maxTimestamp - minTimestamp, 0, gutterRight - gutterLeft);
            cursorWidth = Math.Max(cursorWidth, frameMarkerWidth);
            UpdateCachesMarkersPosition();
            UpdateMarkersPositions();
            UpdateSyncPointMarkerPosition();
            UpdateCursorPosition();
            Invalidate();
        }

        private void FrameTracker_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            Scrub();
        }

        private void FrameTracker_DragDrop(object sender, DragEventArgs e)
        {
            Commit();

            object keyframeBox = e.Data.GetData(typeof(KeyframeBox));
            if (keyframeBox != null && keyframeBox is KeyframeBox)
            {
                KeyframeDropped?.Invoke(keyframeBox, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Scrub the timeline to the cursor point.
        /// This may be called from the outside in the context of drag and drop events on other surfaces, 
        /// as a way to turn these surfaces into large timelines.
        /// </summary>
        public void Scrub()
        {
            if (!enabled || invalidateAsked || isCommonTimeline)
                return;

            // gutterRight is the last available pixel, so the right side of the cursor block.
            Point mouseCoords = this.PointToClient(Cursor.Position);
            cursorLeft = (int)Clamp(mouseCoords.X, gutterLeft, gutterRight - cursorWidth);

            Invalidate();
            invalidateAsked = true;

            if (PositionChanging != null)
            {
                curTimestamp = PixelToTimestamp(cursorLeft);
                PositionChanging(this, new TimeEventArgs(curTimestamp));
            }
            else
            {
                Invalidate();
            }
        }

        /// <summary>
        /// Commit the timeline to the cursor point.
        /// This may be called from the outside in the context of drag and drop events on other surfaces, 
        /// as a way to turn these surfaces into large timelines.
        /// </summary>
        public void Commit()
        {
            if (!enabled || isCommonTimeline)
                return;

            Point mouseCoords = this.PointToClient(Cursor.Position);
            cursorLeft = Math.Min(Math.Max(mouseCoords.X, gutterLeft), gutterRight);

            Invalidate();
            if (PositionChanged != null)
            {
                curTimestamp = PixelToTimestamp(cursorLeft);
                PositionChanged(this, new TimeEventArgs(curTimestamp));
            }
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
            // Draw the gutter.
            int gutterCenterStart = gutterMargin + bmpGutterLeft.Width;
            int gutterCenterWidth = this.Width - (gutterMargin * 2 + bmpGutterLeft.Width + bmpGutterRight.Width);
            canvas.DrawImageUnscaled(bmpGutterLeft, gutterMargin, 0);
            canvas.DrawImageUnscaled(bmpGutterRight, gutterCenterStart + gutterCenterWidth - 4, 0);
            canvas.DrawImage(bmpGutterCenter, gutterCenterStart, 0, gutterCenterWidth, bmpGutterCenter.Height);

            if(!enabled)
                return;
            
            // Draw the main cursor in the background, then the ranges, then the frames.
            if (isCommonTimeline)
            {
                DrawSideMark(canvas, penPlayHead, brushPlayHead, leftPlayHeadMark, true);
                DrawSideMark(canvas, penPlayHead, brushPlayHead, rightPlayHeadMark, false);
            }
            else
            {
                DrawMainCursor(canvas);
            }

            if (showCacheMarkers)
            {
                foreach (var mark in cacheMarks)
                    DrawRangeMark(canvas, mark, MarkerType.Cache);
            }

            foreach (var mark in chronosMarks)
                DrawRangeMark(canvas, mark, MarkerType.Metadata);

            foreach (var mark in tracksMarks)
                DrawRangeMark(canvas, mark, MarkerType.Metadata);

            foreach (var mark in keyframesMarks)
                DrawFrameMark(canvas, mark);

            DrawFrameMark(canvas, syncPointMark);
        }

        /// <summary>
        ///  Draw one frame marker.
        ///  The frame marker has a fixed width and doesn't take the whole frame interval.
        ///  The left edge of the marker is drawn at the pixel mapped to the timestamp.
        /// </summary>
        private void DrawFrameMark(Graphics canvas, Pair<int, Color> mark)
        {
            int coord = mark.First;
            Color color = mark.Second;
            if (coord <= 0)
                return;

            canvas.SmoothingMode = SmoothingMode.Default;
            using (SolidBrush brush = new SolidBrush(color))
                canvas.FillRectangle(brush, coord, gutterTop + 0.5f, frameMarkerWidth, gutterHeight - 0.5f);
        }
        private void DrawSideMark(Graphics canvas, Pen border, SolidBrush inside, int coord, bool lookLeft)
        {
            // Draw each screen playhead as a half-disc.
            if (coord <= 0)
                return;
            
            float width = 9;
            float left = lookLeft ? coord - width * 0.25f : coord - width * 0.75f;
            
            canvas.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rect = new RectangleF(left, gutterTop, width, gutterHeight + 0.5f);
            
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
        private void DrawRangeMark(Graphics canvas, Pair<Point, Color> mark, MarkerType markerType)
        {
            int start = mark.First.X;
            int range = mark.First.Y;
            float left = Math.Max(start, gutterLeft);
            float width = Math.Min(range, gutterRight - left);
            
            canvas.SmoothingMode = SmoothingMode.Default;

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(96, mark.Second)))
            {
                switch (markerType)
                {
                    case MarkerType.Metadata:
                        canvas.FillRectangle(brush, left, gutterTop + 0.5f, width, gutterHeight - 0.5f);
                        break;
                    case MarkerType.Cache:
                        canvas.FillRectangle(brush, left, gutterTop + 0.5f, width, gutterHeightCache - 0.5f);
                        break;
                }

            }
        }
        
        private void DrawMainCursor(Graphics canvas)
        {
            canvas.FillRectangle(Brushes.DarkGray, new Rectangle(cursorLeft, gutterTop, cursorWidth, gutterHeight));
        }
        #endregion
        
        #region Binding UI to Data
        private void UpdateCursorPosition()
        {
            cursorLeft = TimestampToPixel(curTimestamp);
        }

        /// <summary>
        /// Update the pixel position and range of all annotation-based markers.
        /// Should be called when either the timestamp range or the control size change.
        /// </summary>
        private void UpdateMarkersPositions()
        {
            if(metadata == null)
                return;
            
            // Key frames
            keyframesMarks.Clear();
            foreach(Keyframe kf in metadata.Keyframes)
            {
                // Only display Key image that are in the selection.
                if(kf.Position >= minTimestamp && kf.Position <= maxTimestamp)
                {
                    int pixelLeft = TimestampToPixel(kf.Position);
                    Color color = kf.Color;
                    keyframesMarks.Add(new Pair<int, Color>(pixelLeft, color));
                }
            }
            
            // ExtraDrawings
            // We will store the range coords in a Point object, to get a couple of ints structure.
            // X will be the left coordinate, Y the width.
            chronosMarks.Clear();
            foreach (AbstractDrawing d in metadata.ChronoManager.Drawings)
            {
                if (d is DrawingChrono)
                {
                    DrawingChrono chrono = d as DrawingChrono;
                    if (chrono.TimeStart == long.MaxValue || chrono.TimeStop == long.MaxValue)
                        continue;

                    // Only if we have an end and something inside the selection.
                    if (chrono.TimeStart <= maxTimestamp && chrono.TimeStop >= minTimestamp)
                    {
                        Point range = TimestampToPixel(chrono.TimeStart, chrono.TimeStop);
                        Color color = chrono.Color;
                        chronosMarks.Add(new Pair<Point, Color>(range, color));
                    }
                }
                else if (d is DrawingChronoMulti)
                {
                    DrawingChronoMulti chrono = d as DrawingChronoMulti;
                    foreach (var section in chrono.VideoSections)
                    {
                        Point range = TimestampToPixel(section.Start, section.End);
                        Color color = chrono.Color;
                        chronosMarks.Add(new Pair<Point, Color>(range, color));
                    }
                }
            }

            tracksMarks.Clear();
            foreach (DrawingTrack track in metadata.TrackManager.Drawings)
            {
                if (track == null)
                    continue;

                if (track.BeginTimeStamp <= maxTimestamp && track.EndTimeStamp >= minTimestamp)
                {
                    Point range = TimestampToPixel(track.BeginTimeStamp, track.EndTimeStamp);
                    Color color = track.MainColor;
                    tracksMarks.Add(new Pair<Point, Color>(range, color));
                }
            }
        }
        private void UpdateSyncPointMarkerPosition()
        {
            syncPointMark = new Pair<int, Color>(0, Color.Firebrick);
            if(syncPointTimestamp != 0 && syncPointTimestamp >= minTimestamp && syncPointTimestamp <= maxTimestamp)
                syncPointMark.First = TimestampToPixel(syncPointTimestamp);
        }

        /// <summary>
        /// Update the pixel range of the cache markers.
        /// There are two markers if the cache wraps around the end of the selection.
        /// </summary>
        private void UpdateCachesMarkersPosition()
        {
            if (!showCacheMarkers)
                return;

            cacheMarks.Clear();
            if (cacheSegment == VideoSection.Empty)
                return;

            if(cacheSegment.Wrapped)
            {
                Point rangeEnd = TimestampToPixel(minTimestamp, cacheSegment.End);
                Point rangeStart = TimestampToPixel(cacheSegment.Start, maxTimestamp);

                cacheMarks.Add(new Pair<Point, Color>(rangeEnd, Color.DarkGreen));
                cacheMarks.Add(new Pair<Point, Color>(rangeStart, Color.DarkGreen));
            }
            else
            {
                Point range = TimestampToPixel(cacheSegment.Start, cacheSegment.End);
                cacheMarks.Add(new Pair<Point, Color>(range, Color.DarkGreen));
            }
        }

        /// <summary>
        /// Returns the pixel start and width of a timestamp range.
        /// </summary>
        private Point TimestampToPixel(long start, long end)
        {
            if (end == long.MaxValue)
                end = maxTimestamp;

            int pixelStart = TimestampToPixel(Math.Max(start, minTimestamp));
            int pixelEnd = TimestampToPixel(Math.Min(end + tsPerFrame, maxTimestamp));
            int pixelWidth = Math.Max(pixelEnd - pixelStart, frameMarkerWidth);
            return new Point(pixelStart, pixelWidth);
        }

        /// <summary>
        /// Returns the absolute pixel coordinate of a timestamp within the control.
        /// </summary>
        private int TimestampToPixel(long timestamp)
        {
            return (int)Rescale(timestamp, minTimestamp, maxTimestamp, gutterLeft, gutterRight);
        }

        /// <summary>
        /// Returns the timestamp corresponding to a pixel in the control.
        /// </summary>
        private long PixelToTimestamp(int pixelPos)
        {
            return Rescale(pixelPos, gutterLeft, gutterRight, minTimestamp, maxTimestamp);
        }
        private long Rescale(long value, long oldMin, long oldMax, long newMin, long newMax)
        {
            long oldRange = oldMax - oldMin;
            long newRange = newMax - newMin;
            if (oldRange <= 0 || newRange <= 0)
                return newMin;

            float u = (float)(value - oldMin) / oldRange;
            return (long)(newMin + u * newRange);
        }
        private long Clamp(long value, long min, long max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        #endregion
    }

}