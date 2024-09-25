#region License
/*
Copyright � Joan Charmant 2008-2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A class to encapsulate track drawings.
    /// Contains the list of points and the list of keyframes markers.
    /// Handles the user actions, display modes and xml import/export.
    /// The tracking itself is delegated to a Tracker class.
    ///
    /// The trajectory can be in one of 3 views (complete traj, focused on a section, label).
    /// And in one of two status (edit or interactive).
    /// In Edit state: dragging the target moves the point's coordinates.
    /// In Interactive state: dragging the target moves to the next point (in time).
    /// </summary>
    [XmlType("Track")]
    public class DrawingTrack : AbstractDrawing, IDecorable, IScalable, IKvaSerializable
    {
        #region Events
        public event EventHandler TrackerParametersChanged;
        #endregion

        #region Delegates
        // The track object has some peculiar needs with regards to updating the UI, they are injected here.
        // Ask the UI to display the frame closest to selected pos.
        public DisplayClosestFrame DisplayClosestFrame;
        // Ask the UI to enable or disable custom decoding size, which is incompatible with tracking.
        public CheckCustomDecodingSize CheckCustomDecodingSize;
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.DrawingName_Trajectory; }
        }
        public override int ContentHash
        {
            get
            {
                // Combine all relevant fields with XOR to get the Hash.
                int hash = 0;
                hash ^= visibleTimestamp.GetHashCode();
                hash ^= invisibleTimestamp.GetHashCode();
                foreach (AbstractTrackPoint p in positions)
                    hash ^= p.ContentHash;

                hash ^= defaultCrossRadius.GetHashCode();
                hash ^= styleData.ContentHash;
                hash ^= miniLabel.GetHashCode();

                foreach (MiniLabel kfl in keyframeLabels)
                    hash ^= kfl.GetHashCode();

                hash ^= tracker.Parameters.ContentHash;

                hash ^= seeFuture.GetHashCode();
                hash ^= showTrackLabel.GetHashCode();
                hash ^= showKeyframeLabels.GetHashCode();
                hash ^= useKeyframeColors.GetHashCode();
                hash ^= isInteractiveTrack.GetHashCode();
                hash ^= showRotationCircle.GetHashCode();

                return hash;
            }
        }
        public TrackStatus Status
        {
            get { return trackStatus; }
            set
            {
                trackStatus = value;
                AfterTrackStatusChanged();
            }
        }
        public TrackMarker Marker
        {
            get { return trackMarker; }
            set { trackMarker = value; }
        }
        public TrackerParameters TrackerParameters
        {
            get { return tracker.Parameters; }
            set
            {
                if (scalingDone)
                    return;

                tracker.Parameters = value;
                UpdateBoundingBoxes();
            }
        }

        public long BeginTimeStamp
        {
            get { return beginTimeStamp; }
        }
        public long EndTimeStamp
        {
            get { return endTimeStamp; }
        }
        public StyleElements StyleElements
        {
            get { return styleElements; }
        }
        public Color MainColor
        {
            get { return styleData.Color; }
        }
        public override Metadata ParentMetadata
        {
            get { return parentMetadata; }    // unused.
            set
            {
                parentMetadata = value;
                infosFading.AverageTimeStampsPerFrame = parentMetadata.AverageTimeStampsPerFrame;
            }
        }
        public bool Invalid
        {
            get { return invalid; }
        }
        // Fading is not modifiable from outside.
        public override InfosFading InfosFading
        {
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.DataAnalysis; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuVisibility,
                    mnuTracking,
                    mnuOptions,
                    mnuMeasurement,
                });

                bool isTracking = trackStatus == TrackStatus.Edit;
                mnuTrackingStart.Visible = !isTracking;
                mnuTrackingStop.Visible = isTracking;

                mnuSeeFuture.Checked = seeFuture;
                mnuShowTrackLabel.Checked = showTrackLabel;
                mnuShowKeyframeLabel.Checked = showKeyframeLabels;
                mnuUseKeyframeColor.Checked = useKeyframeColors;
                mnuIsInteractiveTrack.Checked = isInteractiveTrack;
                mnuShowRotationCircle.Checked = showRotationCircle;

                mnuVisibility.Enabled = trackStatus == TrackStatus.Interactive;
                mnuMeasurement.Enabled = trackStatus == TrackStatus.Interactive;
                mnuOptions.Enabled = trackStatus == TrackStatus.Interactive;

                // Disable the keyframe labels menu if we are not showing anything.
                // This serves as a hint that we must first select a measurement type.
                // Furthermore, using keyframe colors only makes sense if we are showing their labels.
                mnuShowKeyframeLabel.Enabled = measureLabelType != MeasureLabelType.None;
                mnuUseKeyframeColor.Enabled = showKeyframeLabels && measureLabelType != MeasureLabelType.None;
                return contextMenu;
            }
        }
        public FilteredTrajectory FilteredTrajectory
        {
            get { return filteredTrajectory; }
        }
        public TimeSeriesCollection TimeSeriesCollection
        {
            get { return timeSeriesCollection; }
        }
        #endregion

        #region Members
        // Current state.
        private TrackStatus trackStatus = TrackStatus.Interactive;
        private TrackMarker trackMarker = TrackMarker.Cross;
        private int movingHandler = -1;
        private bool invalid;                                 // Used for XML import.
        private bool scalingDone;

        // Tracker tool.
        private AbstractTracker tracker;

        // Hardwired parameters.
        private const int defaultCrossRadius = 4;
        private const int allowedFramesOver = 12;      // Number of frames over which the global fading spans (after end point).
        private const int focusFrameCount = 30;    // Number of frames of the focus section.

        // Internal data.
        private List<AbstractTrackPoint> positions = new List<AbstractTrackPoint>();
        private FilteredTrajectory filteredTrajectory = new FilteredTrajectory();
        private TimeSeriesCollection timeSeriesCollection;
        private LinearKinematics linearKinematics = new LinearKinematics();
        private IImageToViewportTransformer transformer;

        private long visibleTimestamp;               	// trajectory becomes visible.
        private long invisibleTimestamp;             	// trajectory stops being visible.
        private long beginTimeStamp;                    // timestamp of the first point.
        private long endTimeStamp = long.MaxValue;      // timestamp of the last point.

        // The trajectory can be drawn from multiple places, but manipulated from one place.
        // So these globals should be kept separately and not interfere between each other.
        private int drawPointIndex;
        private int hitPointIndex;

        // Decoration
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();

        // Opacity
        private InfosFading infosFading = new InfosFading(long.MaxValue, 1);
        private const float opacityNormal = 0.9f;
        private const float opacityFuture = 0.25f;
        private const float opacityTracking = 0.75f;

        // Measurement labels
        private MeasureLabelType measureLabelType = MeasureLabelType.Name;
        private MiniLabel miniLabel = new MiniLabel();
        private List<MiniLabel> keyframeLabels = new List<MiniLabel>();

        // Options
        private bool seeFuture = true;
        private bool showTrackLabel = false;
        private bool showKeyframeLabels = true;
        private bool useKeyframeColors = true;
        private bool isInteractiveTrack = false;
        private bool showRotationCircle = false;

        // Configuration
        private BoundingBox searchWindow = new BoundingBox(10);
        private BoundingBox blockWindow = new BoundingBox(4);

        #region Context menu
        private ToolStripMenuItem mnuVisibility = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHideBefore = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowBefore = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHideAfter = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowAfter = new ToolStripMenuItem();

        private ToolStripMenuItem mnuTracking = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTrackingStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTrackingStop = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTrackingTrim = new ToolStripMenuItem();

        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();

        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSeeFuture = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowTrackLabel = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowKeyframeLabel = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUseKeyframeColor = new ToolStripMenuItem();
        private ToolStripMenuItem mnuIsInteractiveTrack = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowRotationCircle = new ToolStripMenuItem();
        #endregion

        private string memoLabel;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingTrack(PointF p, long start, long averageTimeStampsPerFrame)
        {
            tracker = new TrackerBlock2(GetTrackerParameters(new Size(800, 600)));
            positions.Add(new TrackPointBlock(p.X, p.Y, start));

            visibleTimestamp = start;
            invisibleTimestamp = long.MaxValue;
            beginTimeStamp = start;
            endTimeStamp = start;
            miniLabel.SetAttach(p, true);

            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            infosFading = new InfosFading(invisibleTimestamp, averageTimeStampsPerFrame);
            infosFading.FadingFrames = allowedFramesOver;
            infosFading.UseDefault = false;

            SetupStyle();
            InitializeMenus();
        }

        public DrawingTrack(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(PointF.Empty, 0, 1)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }

        private void SetupStyle()
        {
            Color color = TrackColorCycler.Next();
            styleElements = new StyleElements();
            styleElements.Elements.Add("color", new StyleElementColor(color));
            styleElements.Elements.Add("line size", new StyleElementLineSize(3));
            styleElements.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));
            styleElements.Elements.Add("label size", new StyleElementFontSize(8, ScreenManagerLang.StyleElement_FontSize_LabelSize));

            styleData.Color = color;
            styleData.LineSize = 3;
            styleData.TrackShape = TrackShape.Solid;
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);
            
            // Listen to external changes of the style so we can update 
            // the mini label if needed (if using same color).
            styleData.ValueChanged += StyleHelper_ValueChanged;

            BindStyle();
        }

        private void InitializeMenus()
        {
            // Visibility menus.
            mnuShowBefore.Image = Properties.Drawings.showbefore;
            mnuShowAfter.Image = Properties.Drawings.showafter;
            mnuHideBefore.Image = Properties.Drawings.hidebefore;
            mnuHideAfter.Image = Properties.Drawings.hideafter;
            mnuShowBefore.Click += MnuShowBefore_Click;
            mnuShowAfter.Click += MnuShowAfter_Click;
            mnuHideBefore.Click += MnuHideBefore_Click;
            mnuHideAfter.Click += MnuHideAfter_Click;
            mnuVisibility.Image = Properties.Drawings.persistence;
            mnuVisibility.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowBefore,
                mnuShowAfter,
                new ToolStripSeparator(),
                mnuHideBefore,
                mnuHideAfter });

            // Tracking
            mnuTracking.Image = Properties.Drawings.track;
            mnuTrackingStart.Image = Properties.Drawings.trackingplay;
            mnuTrackingStop.Image = Properties.Drawings.trackstop;
            mnuTrackingTrim.Image = Properties.Resources.bin_empty;
            mnuTrackingStart.Click += MnuTrackingStart_Click;
            mnuTrackingStop.Click += MnuTrackingStop_Click;
            mnuTrackingTrim.Click += MnuTrackingTrim_Click;
            mnuTracking.DropDownItems.AddRange(new ToolStripItem[] {
                mnuTrackingStart,
                mnuTrackingStop,
                new ToolStripSeparator(),
                mnuTrackingTrim,
            });

            // Measurement menus.
            mnuMeasurement.Image = Properties.Drawings.label;
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.AddRange(new ToolStripItem[] {
                CreateMeasureLabelTypeMenu(MeasureLabelType.None),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Name),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Clock),
                CreateMeasureLabelTypeMenu(MeasureLabelType.RelativeTime),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Position),
                CreateMeasureLabelTypeMenu(MeasureLabelType.TravelDistance),
                CreateMeasureLabelTypeMenu(MeasureLabelType.TotalHorizontalDisplacement),
                CreateMeasureLabelTypeMenu(MeasureLabelType.TotalVerticalDisplacement),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Speed),
                CreateMeasureLabelTypeMenu(MeasureLabelType.HorizontalVelocity),
                CreateMeasureLabelTypeMenu(MeasureLabelType.VerticalVelocity),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Acceleration),
                CreateMeasureLabelTypeMenu(MeasureLabelType.HorizontalAcceleration),
                CreateMeasureLabelTypeMenu(MeasureLabelType.VerticalAcceleration),
            });

            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuSeeFuture.Image = Properties.Drawings.binocular;
            mnuShowTrackLabel.Image = Properties.Drawings.label;
            mnuShowKeyframeLabel.Image = Properties.Drawings.label;
            mnuUseKeyframeColor.Image = Properties.Resources.SwatchIcon3;
            mnuIsInteractiveTrack.Image = Properties.Drawings.handtool4;
            mnuShowRotationCircle.Image = Properties.Drawings.circle;

            mnuSeeFuture.Click += MnuSeeFuture_Click;
            mnuShowTrackLabel.Click += MnuShowTrackLabel_Click;
            mnuShowKeyframeLabel.Click += MnuShowKeyframeLabel_Click;
            mnuUseKeyframeColor.Click += MnuUseKeyframeColor_Click;
            mnuIsInteractiveTrack.Click += MnuIsInteractiveTrack_Click;
            mnuShowRotationCircle.Click += MnuShowRotationCircle_Click;

            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuSeeFuture,
                mnuShowTrackLabel,
                mnuShowKeyframeLabel,
                mnuUseKeyframeColor,
                mnuIsInteractiveTrack,
                mnuShowRotationCircle,
            });
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (currentTimestamp < visibleTimestamp)
                return;

            this.transformer = transformer;

            // If we are editing the track points don't hide the trajectory.
            double opacityFactor = 1.0;
            if (trackStatus == TrackStatus.Interactive && currentTimestamp > invisibleTimestamp)
                opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);

            if (opacityFactor <= 0)
                return;

            drawPointIndex = FindClosestPoint(currentTimestamp);

            // Draw various elements depending on combination of status and display options.

            if (positions.Count > 1)
            {
                bool drawKeyframeLabels = showKeyframeLabels &&
                    trackStatus == TrackStatus.Interactive &&
                    measureLabelType != MeasureLabelType.None;

                // Transform all points in the trajectory from their XYT coordinate to image coordinate at the current time.
                // TODO: this should be done during Track() step like for other drawings.
                List<PointF> points = GetPoints(distorter, cameraTransformer, currentTimestamp);

                // Trajectory and keyframe labels.
                int first = GetFirstVisiblePoint(drawPointIndex);
                int last = GetLastVisiblePoint(drawPointIndex);
                float opacity = 0;
                if (trackStatus == TrackStatus.Interactive)
                {
                    // Past and present section.
                    opacity = GetOpacity(trackStatus, (float)opacityFactor, true);
                    DrawTrajectory(canvas, points, first, drawPointIndex, opacity, transformer, currentTimestamp);

                    if (seeFuture)
                    {
                        opacity = GetOpacity(trackStatus, (float)opacityFactor, false);
                        DrawTrajectory(canvas, points, drawPointIndex, last, opacity, transformer, currentTimestamp);
                    }

                    if (drawKeyframeLabels)
                        DrawKeyframesLabels(canvas, points, (float)opacityFactor, transformer);

                    if (showTrackLabel)
                        DrawMainLabel(canvas, drawPointIndex, opacityFactor, transformer);

                }
                else if (trackStatus == TrackStatus.Edit)
                {
                    opacity = GetOpacity(trackStatus, (float)opacityFactor, false);
                    DrawTrajectory(canvas, points, first, last, opacity, transformer, currentTimestamp);
                }

                // Do not show the track path when configuring the tracking parameters.
            }

            if (positions.Count > 0)
            {
                bool isBeforeStart = currentTimestamp < positions[0].T;
                if (isBeforeStart)
                    opacityFactor = GetOpacity(trackStatus, (float)opacityFactor, false);

                // Angular motion
                if (showRotationCircle && trackStatus == TrackStatus.Interactive)
                    DrawBestFitCircle(canvas, drawPointIndex, opacityFactor, transformer);

                // Track cursor.
                if (opacityFactor == 1.0 && currentTimestamp <= positions[positions.Count - 1].T)
                    DrawMarker(canvas, opacityFactor, transformer);

                if (opacityFactor == 1.0)
                    DrawTrackerHelp(canvas, transformer, styleData.Color, opacityFactor);
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            //log.DebugFormat("Move drawing, status={0}", trackStatus);
            if (trackStatus == TrackStatus.Interactive && movingHandler > 1)
            {
                MoveLabelTo(dx, dy, movingHandler);
                return;
            }

            if (movingHandler == 1 && (trackStatus == TrackStatus.Edit || trackStatus == TrackStatus.Configuration))
            {
                positions[hitPointIndex].X += dx;
                positions[hitPointIndex].Y += dy;

                if (trackStatus == TrackStatus.Configuration)
                    UpdateBoundingBoxes();
                return;
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            //log.DebugFormat("Move handle, status={0}", trackStatus);
            if (trackStatus == TrackStatus.Interactive && (handleNumber == 0 || handleNumber == 1))
            {
                MoveCursor(point.X, point.Y);
            }
            else if (trackStatus == TrackStatus.Configuration)
            {
                TrackerParameters old = tracker.Parameters;

                if (movingHandler > 1 && movingHandler < 6)
                    searchWindow.MoveHandleKeepSymmetry(point.ToPoint(), movingHandler - 1, positions[hitPointIndex].Point);
                else if (movingHandler >= 6 && movingHandler < 11)
                    blockWindow.MoveHandleKeepSymmetry(point.ToPoint(), movingHandler - 5, positions[hitPointIndex].Point);

                TrackerParameters newParams = new TrackerParameters(
                    old.SimilarityThreshold, old.TemplateUpdateThreshold, old.RefinementNeighborhood, searchWindow.Rectangle.Size, blockWindow.Rectangle.Size, old.ResetOnMove);

                tracker.Parameters = newParams;
                UpdateBoundingBoxes();
                if (TrackerParametersChanged != null)
                    TrackerParametersChanged(this, EventArgs.Empty);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            //log.DebugFormat("Hit test, status={0}", trackStatus);
            long maxHitTimeStamps = invisibleTimestamp;
            if (maxHitTimeStamps != long.MaxValue)
                maxHitTimeStamps += (allowedFramesOver * parentMetadata.AverageTimeStampsPerFrame);

            if (currentTimestamp < visibleTimestamp || currentTimestamp > maxHitTimeStamps)
            {
                movingHandler = -1;
                return -1;
            }

            // The same trajectory can be modified from different viewports at different timestamps.
            // Make sure the MoveDrawing and MoveHandle are using the correct point.
            hitPointIndex = FindClosestPoint(currentTimestamp);

            int result = -1;
            switch (trackStatus)
            {
                case TrackStatus.Edit:
                    result = HitTestEdit(point, hitPointIndex, transformer);
                    break;
                case TrackStatus.Configuration:
                    result = HitTestConfiguration(point, transformer);
                    break;
                case TrackStatus.Interactive:
                    result = HitTestInteractive(point, currentTimestamp, hitPointIndex, transformer);
                    break;
            }

            movingHandler = result;

            return result;
        }
        private int HitTestEdit(PointF point, int hitPointIndex, IImageToViewportTransformer transformer)
        {
            // 1: search window.
            RectangleF search = positions[hitPointIndex].Point.Box(tracker.Parameters.SearchWindow);
            if (search.Contains(point))
                return 1;

            return -1;
        }
        private int HitTestConfiguration(PointF point, IImageToViewportTransformer transformer)
        {
            // 1: search window, 2-5: search window corners, 6-10: block window corners.
            int blockWindowHit = blockWindow.HitTest(point, transformer);
            if (blockWindowHit >= 1)
                return blockWindowHit + 5;

            int searchWindowHit = searchWindow.HitTest(point, transformer);
            if (searchWindowHit >= 0)
                return searchWindowHit + 1;

            return -1;
        }
        private int HitTestInteractive(PointF point, long currentTimestamp, int hitPointIndex, IImageToViewportTransformer transformer)
        {
            // 0: track, 1: current point on track, 2: main label, 3+: keyframe label.
            int result = HitTestKeyframesLabels(point, currentTimestamp, transformer);
            if (result >= 0)
                return result;

            if (HitTester.HitPoint(point, positions[hitPointIndex].Point, transformer))
                return 1;

            result = HitTestTrajectory(point, hitPointIndex, transformer);

            if (result == 0)
              MoveCursor(point.X, point.Y);

            return result;
        }
        private int HitTestTrajectory(PointF point, int hitPointIndex, IImageToViewportTransformer transformer)
        {
            // 0: track. -1: not on track.
            int result = -1;

            try
            {
                int iStart = GetFirstVisiblePoint(hitPointIndex);
                int iEnd = GetLastVisiblePoint(hitPointIndex);
                int iTotalVisiblePoints = iEnd - iStart;
                Point[] points = new Point[iTotalVisiblePoints];
                for (int i = iStart; i < iEnd; i++)
                    points[i - iStart] = positions[i].Point.ToPoint();

                using (GraphicsPath path = new GraphicsPath())
                {
                    float tension = PreferencesManager.PlayerPreferences.EnableFiltering ? 0.5f : 0.0f;
                    path.AddCurve(points, tension);
                    RectangleF bounds = path.GetBounds();
                    if (!bounds.IsEmpty)
                    {
                        bool hit = HitTester.HitPath(point, path, styleData.LineSize, false, transformer);
                        result = hit ? 0 : -1;
                    }
                }
            }
            catch (Exception exp)
            {
                result = -1;
                log.Error("Error while hit testing track.");
                log.Error("Exception thrown : " + exp.GetType().ToString() + " in " + exp.Source.ToString() + exp.TargetSite.Name.ToString());
                log.Error("Message : " + exp.Message.ToString());
                Exception inner = exp.InnerException;
                while (inner != null)
                {
                    log.Error("Inner exception : " + inner.Message.ToString());
                    inner = inner.InnerException;
                }
            }

            return result;
        }
        public override PointF GetCopyPoint()
        {
            return positions[0].Point;
        }
        #endregion

        #region Drawing routines
        private void DrawTrajectory(Graphics canvas, List<PointF> points, int start, int end, float opacity, IImageToViewportTransformer transformer, long currentTimestamp)
        {
            // Transform from image coordinate in this frame to viewport coordinates.
            var viewPoints = points.GetRange(start, end - start + 1).Select(p => transformer.Transform(p)).ToArray();
            
            if (viewPoints.Length <= 1)
                return;

            using (Pen trackPen = styleData.GetPen(opacity, 1.0))
            {
                // Tension of 0.5f creates a smooth curve.
                float tension = PreferencesManager.PlayerPreferences.EnableFiltering ? 0.5f : 0.0f;

                trackPen.StartCap = LineCap.Round;
                trackPen.EndCap = LineCap.Round;

                if (trackStatus == TrackStatus.Edit)
                {
                    trackPen.Width = 1.0f;
                    canvas.DrawCurve(trackPen, viewPoints, tension);
                    foreach (PointF p in viewPoints)
                        canvas.DrawEllipse(trackPen, p.Box(3));
                }
                else
                {
                    canvas.DrawCurve(trackPen, viewPoints, tension);

                    if (styleData.TrackShape.ShowSteps)
                    {
                        using (Pen stepPen = new Pen(trackPen.Color, 2.0f))
                        {
                            int margin = (int)(trackPen.Width * 1.5);
                            foreach (Point p in viewPoints)
                                canvas.DrawEllipse(stepPen, p.Box(margin));
                        }
                    }
                }
            }
        }

        private void DrawMarker(Graphics canvas, double fadingFactor, IImageToViewportTransformer transformer)
        {
            int radius = defaultCrossRadius;
            Point location = transformer.Transform(positions[drawPointIndex].Point);

            if (trackMarker == TrackMarker.Cross || trackStatus == TrackStatus.Edit || trackStatus == TrackStatus.Configuration)
            {
                using (Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleData.Color)))
                {
                    canvas.DrawLine(p, location.X, location.Y - radius, location.X, location.Y + radius);
                    canvas.DrawLine(p, location.X - radius, location.Y, location.X + radius, location.Y);
                }
            }
            else if (trackMarker == TrackMarker.Circle)
            {
                using (Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleData.Color)))
                {
                    canvas.DrawEllipse(p, location.Box(radius));
                }
            }
            else if (trackMarker == TrackMarker.Target)
            {
                int diameter = radius * 2;
                canvas.FillPie(Brushes.Black, location.X - radius, location.Y - radius, diameter, diameter, 0, 90);
                canvas.FillPie(Brushes.White, location.X - radius, location.Y - radius, diameter, diameter, 90, 90);
                canvas.FillPie(Brushes.Black, location.X - radius, location.Y - radius, diameter, diameter, 180, 90);
                canvas.FillPie(Brushes.White, location.X - radius, location.Y - radius, diameter, diameter, 270, 90);
                canvas.DrawEllipse(Pens.White, location.Box(radius + 2));
            }
        }
        private void DrawTrackerHelp(Graphics canvas, IImageToViewportTransformer transformer, Color color, double opacity)
        {
            if (trackStatus == TrackStatus.Edit)
            {
                tracker.Draw(canvas, positions[drawPointIndex], transformer, styleData.Color, opacity);
            }
            else if (trackStatus == TrackStatus.Configuration)
            {
                Point location = transformer.Transform(positions[drawPointIndex].Point);
                Size searchSize = transformer.Transform(tracker.Parameters.SearchWindow);
                Size blockSize = transformer.Transform(tracker.Parameters.BlockWindow);
                Rectangle searchBox = location.Box(searchSize);

                // Dim background.
                GraphicsPath backgroundPath = new GraphicsPath();
                backgroundPath.AddRectangle(canvas.ClipBounds);
                GraphicsPath searchBoxPath = new GraphicsPath();
                searchBoxPath.AddRectangle(searchBox);
                backgroundPath.AddPath(searchBoxPath, false);
                using (SolidBrush brushBackground = new SolidBrush(Color.FromArgb(160, Color.Black)))
                {
                    canvas.FillPath(brushBackground, backgroundPath);
                }

                // Tool
                using (Pen p = new Pen(Color.FromArgb(255, styleData.Color)))
                using (SolidBrush b = new SolidBrush(p.Color))
                {
                    searchWindow.Draw(canvas, searchBox, p, b, 4);
                    blockWindow.Draw(canvas, location.Box(blockSize), p, b, 3);
                }
            }
        }
        private void DrawKeyframesLabels(Graphics canvas, List<PointF> points, float baselineOpacity, IImageToViewportTransformer transformer)
        {
            //------------------------------------------------------------
            // Draw the keyframes labels
            // Each Label has its own coords and is movable.
            // Each label is connected to the TrackPosition point.
            // Rescaling for the current image size has already been done.
            //------------------------------------------------------------
            if (baselineOpacity < 0 || trackStatus == TrackStatus.Configuration)
                return;

            float opacityPast = GetOpacity(trackStatus, baselineOpacity, true);
            float opacityFuture = GetOpacity(trackStatus, baselineOpacity, false);
            long currentTimestamp = positions[drawPointIndex].T;

            // The point positions are already up to date with regards to camera motion.
            foreach (MiniLabel kfl in keyframeLabels)
            {
                bool isFuture = kfl.Timestamp > currentTimestamp;
                float opacity = isFuture ? opacityFuture : opacityPast;
                if (!isFuture || seeFuture)
                {
                    kfl.Draw(canvas, transformer, opacity);
                }
            }
        }
        private void DrawBestFitCircle(Graphics canvas, int currentPoint, double fadingFactor, IImageToViewportTransformer transformer)
        {
            // TODO: add support for camera motion.

            Circle circle = filteredTrajectory.BestFitCircle;

            if (circle.Center == PointF.Empty)
                return;

            // trajectoryPoints values are expressed in user coordinates, so we need to first get them back to image coords,
            // and then to convert them for display screen.
            Point location = transformer.Transform(positions[currentPoint].Point);
            PointF centerInImage = parentMetadata.CalibrationHelper.GetImagePoint(circle.Center);
            Point center = transformer.Transform(centerInImage);

            // Get ellipse.
            Ellipse ellipseInImage = parentMetadata.CalibrationHelper.GetEllipseFromCircle(circle);

            PointF ellipseCenter = transformer.Transform(ellipseInImage.Center);
            float semiMinorAxis = transformer.Transform((int)ellipseInImage.SemiMinorAxis);
            float semiMajorAxis = transformer.Transform((int)ellipseInImage.SemiMajorAxis);
            Ellipse ellipse = new Ellipse(ellipseCenter, semiMajorAxis, semiMinorAxis, ellipseInImage.Rotation);
            RectangleF rect = new RectangleF(-ellipse.SemiMajorAxis, -ellipse.SemiMinorAxis, ellipse.SemiMajorAxis * 2, ellipse.SemiMinorAxis * 2);
            float angle = (float)MathHelper.Degrees(ellipse.Rotation);

            using (Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleData.Color)))
            {
                // Center and radius
                p.Width = 2;
                canvas.DrawEllipse(p, center.Box(4));
                p.DashStyle = DashStyle.Dash;
                canvas.DrawLine(p, center, location);

                // Ellipse or circle
                canvas.TranslateTransform(ellipse.Center.X, ellipse.Center.Y);
                canvas.RotateTransform(angle);

                canvas.DrawEllipse(p, rect);

                canvas.RotateTransform(-angle);
                canvas.TranslateTransform(-ellipse.Center.X, -ellipse.Center.Y);
            }
        }
        private void DrawMainLabel(Graphics canvas, int drawPointIndex, double opacityFactor, IImageToViewportTransformer transformer)
        {
            // Draw the main label and its connector to the current point.
            if (opacityFactor != 1.0f || trackStatus == TrackStatus.Configuration)
                return;

            // The attach position is already up to date with regards to camera motion.
            miniLabel.SetText(GetMeasureLabelText(drawPointIndex), transformer);
            miniLabel.Draw(canvas, transformer, opacityFactor);
        }
        private float GetOpacity(TrackStatus status, float baselineOpacity, bool isPast)
        {
            if (status == TrackStatus.Edit)
                return opacityTracking;

            if (isPast)
                return baselineOpacity * opacityNormal;

            return seeFuture ? baselineOpacity * opacityFuture : 0;
        }
        #endregion

        #region Measure label
        public string GetMeasureLabelOptionText(MeasureLabelType data)
        {
            switch (data)
            {
                case MeasureLabelType.None: return ScreenManagerLang.mnuMeasure_Label_None;
                case MeasureLabelType.Name: return ScreenManagerLang.mnuMeasure_Name;

                case MeasureLabelType.Clock: return ScreenManagerLang.mnuMeasure_Clock;
                case MeasureLabelType.RelativeTime: return ScreenManagerLang.mnuMeasure_Stopwatch;

                case MeasureLabelType.Position: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Position;
                case MeasureLabelType.TravelDistance: return ScreenManagerLang.ExtraData_Length;
                case MeasureLabelType.TotalHorizontalDisplacement: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalHorizontalDisplacement;
                case MeasureLabelType.TotalVerticalDisplacement: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalVerticalDisplacement;

                case MeasureLabelType.Speed: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed;
                case MeasureLabelType.HorizontalVelocity: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalVelocity;
                case MeasureLabelType.VerticalVelocity: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalVelocity;
                case MeasureLabelType.Acceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Acceleration;
                case MeasureLabelType.HorizontalAcceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalAcceleration;
                case MeasureLabelType.VerticalAcceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalAcceleration;
            }

            return "";
        }

        private string GetMeasureLabelText(int index)
        {
            string displayText = "";
            switch (measureLabelType)
            {
                case MeasureLabelType.None:
                    displayText = "";
                    break;
                case MeasureLabelType.Name:
                    displayText = name;
                    break;

                case MeasureLabelType.Clock:
                case MeasureLabelType.RelativeTime:
                    displayText = GetMeasureLabelTextTime(measureLabelType, index);
                    break;

                case MeasureLabelType.Position:
                case MeasureLabelType.TravelDistance:
                case MeasureLabelType.TotalHorizontalDisplacement:
                case MeasureLabelType.TotalVerticalDisplacement:
                    displayText = GetMeasureLabelPositionDistance(measureLabelType, index);
                    break;

                case MeasureLabelType.Speed:
                case MeasureLabelType.HorizontalVelocity:
                case MeasureLabelType.VerticalVelocity:
                case MeasureLabelType.Acceleration:
                case MeasureLabelType.HorizontalAcceleration:
                case MeasureLabelType.VerticalAcceleration:
                    displayText = GetMeasureLabelSpeedAcceleration(measureLabelType, index);
                    break;

                default:
                    break;
            }

            return displayText;
        }

        private string GetMeasureLabelTextTime(MeasureLabelType type, int index)
        {
            string displayText = "";
            if (type == MeasureLabelType.Clock)
            {
                displayText = parentMetadata.TimeCodeBuilder(positions[index].T, TimeType.UserOrigin, TimecodeFormat.Unknown, true);
            }
            else if (type == MeasureLabelType.RelativeTime)
            {
                long elapsed = positions[index].T - positions[0].T;
                displayText = parentMetadata.TimeCodeBuilder(elapsed, TimeType.Absolute, TimecodeFormat.Unknown, true);
            }

            return displayText;
        }

        private string GetMeasureLabelPositionDistance(MeasureLabelType type, int index)
        {
            CalibrationHelper helper = parentMetadata.CalibrationHelper;
            CultureInfo culture = CultureInfo.InvariantCulture;
            string displayText = "";
            switch (type)
            {
                case MeasureLabelType.Position:
                        double x = timeSeriesCollection[Kinematics.XRaw][index];
                double y = timeSeriesCollection[Kinematics.YRaw][index];
                displayText = string.Format(culture, "{0:0.00} ; {1:0.00} {2}", x, y, helper.GetLengthAbbreviation());
                break;

                case MeasureLabelType.TravelDistance:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearDistance, index, helper.GetLengthAbbreviation());
                    break;
                case MeasureLabelType.TotalHorizontalDisplacement:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalDisplacement, index, helper.GetLengthAbbreviation());
                    break;
                case MeasureLabelType.TotalVerticalDisplacement:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalDisplacement, index, helper.GetLengthAbbreviation());
                    break;
                default:
                    break;
            }

            return displayText;
        }

        private string GetMeasureLabelSpeedAcceleration(MeasureLabelType type, int index)
        {
            CalibrationHelper helper = parentMetadata.CalibrationHelper;
            CultureInfo culture = CultureInfo.InvariantCulture;
            string displayText = "";
            switch (type)
            {
                case MeasureLabelType.Speed:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearSpeed, index, helper.GetSpeedAbbreviation());
                    break;
                case MeasureLabelType.HorizontalVelocity:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalVelocity, index, helper.GetSpeedAbbreviation());
                    break;
                case MeasureLabelType.VerticalVelocity:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalVelocity, index, helper.GetSpeedAbbreviation());
                    break;
                case MeasureLabelType.Acceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
                case MeasureLabelType.HorizontalAcceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
                case MeasureLabelType.VerticalAcceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
            }

            return displayText;
        }

        private string GetKinematicsDisplayText(Kinematics k, int index, string abbreviation)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            double value = timeSeriesCollection[k][index];
            if (!double.IsNaN(value))
                return string.Format(culture, "{0:0.00} {1}", value, abbreviation);
            else
                return "###";
        }
        #endregion

        #region User manipulation
        private void MoveCursor(float dx, float dy)
        {
            if (trackStatus == TrackStatus.Edit)
            {
                // Move the current point.
                // The image will be reseted at mouse up. (=> UpdateTrackPoint)
                positions[hitPointIndex].X += dx;
                positions[hitPointIndex].Y += dy;
            }
            else
            {
                if (!isInteractiveTrack || positions.Count < 2 || DisplayClosestFrame == null)
                    return;

                // Move Playhead to closest frame (x,y,t).
                // In this case, dx and dy are actually absolute values.
                float spaceSpan = ComputeSpaceSpan();
                long timeSpan = positions[positions.Count - 1].T - positions[0].T;
                float timeScale = timeSpan / spaceSpan;
                Point p = new PointF(dx, dy).ToPoint();
                DisplayClosestFrame(p, positions, timeScale, true);
            }
        }
        private void MoveLabelTo(float dx, float dy, int labelId)
        {
            // labelId: 2 = main label, 3+ = keyframes labels.

            if (measureLabelType != MeasureLabelType.None && labelId == 2)
            {
                // Move the main label.
                miniLabel.MoveLabel(dx, dy);
            }
            else
            {
                // Move the specified label by specified amount.
                int iLabel = labelId - 3;
                keyframeLabels[iLabel].MoveLabel(dx, dy);
            }
        }
        private int HitTestKeyframesLabels(PointF point, long currentTimestamp, IImageToViewportTransformer transformer)
        {
            // Convention: -1 = miss, 2 = on main label, 3+ = on keyframe label.
            if (measureLabelType == MeasureLabelType.None)
                return -1;

            if (showTrackLabel)
            {
                if (miniLabel.HitTest(point))
                    return 2;
            }

            if (!showKeyframeLabels)
                return -1;

            for (int i = 0; i < keyframeLabels.Count; i++)
            {
                bool isFuture = keyframeLabels[i].Timestamp > currentTimestamp;
                if (!isFuture || seeFuture)
                {
                    if (keyframeLabels[i].HitTest(point))
                    {
                        return i + 3;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Transform all points in the trajectory from their XYT coordinate to image coordinate at the current time.
        /// Updates the keyframe minilabels with the matching point location.
        /// </summary>
        private List<PointF> GetPoints(DistortionHelper distorter, CameraTransformer cameraTransformer, long currentTimestamp)
        {
            var points = new List<PointF>(new PointF[positions.Count]);
            for (int i = 0; i < positions.Count; i++)
            {
                var tp = positions[i];
                PointF p = tp.Point;

                if (distorter != null && distorter.Initialized)
                    p = distorter.Undistort(p);

                if (cameraTransformer != null && cameraTransformer.Initialized)
                    p = cameraTransformer.Transform(tp.T, currentTimestamp, p);

                points[i] = p;

                // Find if there is a mini label attached to this point and update the attach point.
                for (int j = 0; j < keyframeLabels.Count; j++)
                {
                    if (keyframeLabels[j].Timestamp == tp.T)
                    {
                        keyframeLabels[j].SetAttach(p, true);
                        break;
                    }
                }
                
                // Update the attach point of the main mini label.
                // If we are outside the trajectory range stick it to the first or last point.
                if ((currentTimestamp < positions[0].T && i == 0) || 
                    (currentTimestamp > positions[positions.Count - 1].T && i == positions.Count - 1) || 
                    (currentTimestamp == tp.T))
                {
                    miniLabel.SetAttach(p, true);
                }
            }

            return points;
        }
        private int GetFirstVisiblePoint(int pointIndex)
        {
            int index = 0;

            if (trackStatus == TrackStatus.Edit)
            {
                index = pointIndex - focusFrameCount;
            }
            else
            {
                index = 0;
            }

            return Math.Max(0, index);
        }
        private int GetLastVisiblePoint(int pointIndex)
        {
            int index = 0;

            if (trackStatus == TrackStatus.Edit)
            {
                index = pointIndex + focusFrameCount;
            }
            else if (seeFuture)
            {
                index = positions.Count - 1;
            }
            else
            {
                index = pointIndex;
            }

            return Math.Min(index, positions.Count - 1);
        }
        #endregion

        #region Context Menu implementation

        #region Visibility
        private void MnuShowBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = 0;
            InvalidateFromMenu(sender);
        }
        private void MnuShowAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = long.MaxValue;
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }
        private void MnuHideBefore_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            visibleTimestamp = CurrentTimestampFromMenu(sender);
            InvalidateFromMenu(sender);
        }

        private void MnuHideAfter_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            invisibleTimestamp = CurrentTimestampFromMenu(sender);
            infosFading.ReferenceTimestamp = invisibleTimestamp;
            InvalidateFromMenu(sender);
        }
        #endregion

        private void MnuTrackingStart_Click(object sender, EventArgs e)
        {
            CheckCustomDecodingSize(true);
            trackStatus = TrackStatus.Edit;
            AfterTrackStatusChanged();
            InvalidateFromMenu(sender);
        }

        private void MnuTrackingStop_Click(object sender, EventArgs e)
        {
            StopTracking();

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host == null)
                return;

            host.UpdateFramesMarkers();

            CheckCustomDecodingSize(false);
            InvalidateFromMenu(sender);
        }

        private void MnuTrackingTrim_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);

            long timestamp = CurrentTimestampFromMenu(sender);

            // Delete end of track.
            drawPointIndex = FindClosestPoint(timestamp);
            if (drawPointIndex < positions.Count - 1)
                positions.RemoveRange(drawPointIndex + 1, positions.Count - drawPointIndex - 1);

            endTimeStamp = positions[positions.Count - 1].T;

            UpdateKinematics();
            UpdateKeyframeLabels();
            UpdateFramesMarkersFromMenu(sender);
            InvalidateFromMenu(sender);
        }

        private float ComputeSpaceSpan()
        {
            // This is used as a normalization factor for interactive manipulation.

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = -1;
            float maxY = -1;

            for (int i = 0; i < positions.Count; i++)
            {
                minX = Math.Min(minX, positions[i].X);
                minY = Math.Min(minY, positions[i].Y);
                maxX = Math.Max(maxX, positions[i].X);
                maxY = Math.Max(maxY, positions[i].Y);
            }

            float dx = maxX - minX;
            float dy = maxY - minY;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }


        private void MnuShowTrackLabel_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showTrackLabel = !mnuShowTrackLabel.Checked;
            InvalidateFromMenu(sender);
        }


        private void MnuUseKeyframeColor_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            useKeyframeColors = !mnuUseKeyframeColor.Checked;
            UpdateKeyframeLabels();
            InvalidateFromMenu(sender);
        }

        private void MnuShowKeyframeLabel_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showKeyframeLabels = !mnuShowKeyframeLabel.Checked;
            InvalidateFromMenu(sender);
        }

        private void MnuSeeFuture_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            seeFuture = !mnuSeeFuture.Checked;
            InvalidateFromMenu(sender);
        }

        private void MnuIsInteractiveTrack_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            isInteractiveTrack = !mnuIsInteractiveTrack.Checked;
            InvalidateFromMenu(sender);
        }

        private void MnuShowRotationCircle_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showRotationCircle = !mnuShowRotationCircle.Checked;
            InvalidateFromMenu(sender);
        }

        #endregion

        #region Tracking
        public void StopTracking()
        {
            trackStatus = TrackStatus.Interactive;
            AfterTrackStatusChanged();
        }

        public void TrackCurrentPosition(VideoFrame current, OpenCvSharp.Mat cvImage)
        {
            // Match the previous point in current image.
            // New points to trajectories are always created from here.

            TrackPointBlock closestFrame = positions.Last() as TrackPointBlock;
            if (closestFrame == null || current.Timestamp <= closestFrame.T)
                return;

            if (closestFrame.Template == null)
            {
                // Contiuning a track that was imported through kva.
                PointF location = new PointF(closestFrame.X, closestFrame.Y);
                AbstractTrackPoint trackPoint = tracker.CreateTrackPoint(true, location, 1.0f, closestFrame.T, current.Image, positions);
                positions[positions.Count - 1] = trackPoint;
            }

            AbstractTrackPoint p = null;
            bool bMatched = tracker.Track(positions, current.Image, cvImage, current.Timestamp, out p);

            if (p == null)
            {
                StopTracking();
                return;
            }

            positions.Add(p);

            if (!bMatched)
                StopTracking();

            // Adjust internal data.
            endTimeStamp = positions.Last().T;
            UpdateKeyframeLabels();
        }

        public void UpdateTrackPoint(Bitmap currentImage, IImageToViewportTransformer transformer)
        {
            // The user moved a point that had been previously placed.
            // We need to reconstruct tracking data stored in the point, for later tracking.
            // The coordinate of the point have already been updated during the mouse move.
            if (currentImage == null || positions.Count < 1 || drawPointIndex < 0)
                return;

            AbstractTrackPoint current = positions[drawPointIndex];

            current.ResetTrackData();
            AbstractTrackPoint atp = tracker.CreateTrackPoint(true, current.Point, 1.0f, current.T, currentImage, positions);

            if (atp != null)
                positions[drawPointIndex] = atp;

            // Update the mini labels (attach, position of label, and text).
            for (int i = 0; i < keyframeLabels.Count; i++)
            {
                if (keyframeLabels[i].Timestamp == current.T)
                {
                    keyframeLabels[i].SetAttach(current.Point, true);
                    if (measureLabelType != MeasureLabelType.None)
                        keyframeLabels[i].SetText(GetMeasureLabelText(keyframeLabels[i].AttachIndex), transformer);

                    break;
                }
            }
        }
        private TrackerParameters GetTrackerParameters(Size size)
        {
            TrackingProfile profile = PreferencesManager.PlayerPreferences.TrackingProfile;

            double similarityThreshold = profile.SimilarityThreshold;
            double templateUpdateThreshold = profile.TemplateUpdateThreshold;
            int refinementNeighborhood = profile.RefinementNeighborhood;
            Size searchWindow = profile.SearchWindow;
            Size blockWindow = profile.BlockWindow;

            if (profile.SearchWindowUnit == TrackerParameterUnit.Percentage)
            {
                int width = (int)(size.Width * (profile.SearchWindow.Width / 100.0));
                int height = (int)(size.Height * (profile.SearchWindow.Height / 100.0));
                searchWindow = new Size(width, height);
            }

            if (profile.BlockWindowUnit == TrackerParameterUnit.Percentage)
            {
                int width = (int)(size.Width * (profile.BlockWindow.Width / 100.0));
                int height = (int)(size.Height * (profile.BlockWindow.Height / 100.0));
                blockWindow = new Size(width, height);
            }

            return new TrackerParameters(similarityThreshold, templateUpdateThreshold, refinementNeighborhood, searchWindow, blockWindow, false);
        }
        #endregion

        #region KVA Serialization
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("TimePosition", beginTimeStamp.ToString());
                w.WriteElementString("Visible", (visibleTimestamp == long.MaxValue) ? "-1" : visibleTimestamp.ToString());
                w.WriteElementString("Invisible", (invisibleTimestamp == long.MaxValue) ? "-1" : invisibleTimestamp.ToString());

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
                string xmlMeasureLabelType = enumConverter.ConvertToString(measureLabelType);
                w.WriteElementString("ExtraData", xmlMeasureLabelType);

                enumConverter = TypeDescriptor.GetConverter(typeof(TrackMarker));
                string xmlTrackMarker = enumConverter.ConvertToString(trackMarker);
                w.WriteElementString("Marker", xmlTrackMarker);

                w.WriteStartElement("TrackerParameters");
                tracker.Parameters.WriteXml(w);
                w.WriteEndElement();

                TrackPointsToXml(w);

                w.WriteStartElement("MainLabel");
                w.WriteAttributeString("Text", name);

                // Save all mini labels with regards to the first point.
                miniLabel.WriteXml(w, positions[0].Point);
                w.WriteEndElement();

                if (keyframeLabels.Count > 0)
                {
                    w.WriteStartElement("KeyframeLabelList");
                    w.WriteAttributeString("Count", keyframeLabels.Count.ToString());

                    foreach (MiniLabel kfl in keyframeLabels)
                    {
                        w.WriteStartElement("KeyframeLabel");

                        // Save the mini label relatively to its reference point.
                        kfl.WriteXml(w, positions[kfl.AttachIndex].Point);
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                w.WriteElementString("SeeFuture", XmlHelper.WriteBoolean(seeFuture));
                w.WriteElementString("ShowTrackLabel", XmlHelper.WriteBoolean(showTrackLabel));
                w.WriteElementString("ShowKeyframeLabels", XmlHelper.WriteBoolean(showKeyframeLabels));
                w.WriteElementString("UseKeyframeColors", XmlHelper.WriteBoolean(useKeyframeColors));
                w.WriteElementString("IsInteractiveTrack", XmlHelper.WriteBoolean(useKeyframeColors));
                w.WriteElementString("ShowRotationCircle", XmlHelper.WriteBoolean(showRotationCircle));
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                styleElements.WriteXml(w);
                w.WriteEndElement();
            }
        }
        private void TrackPointsToXml(XmlWriter w)
        {
            w.WriteStartElement("TrackPointList");
            w.WriteAttributeString("Count", positions.Count.ToString());

            if (positions.Count > 0)
            {
                foreach (AbstractTrackPoint tp in positions)
                {
                    w.WriteStartElement("TrackPoint");
                    tp.WriteXml(w);
                    w.WriteEndElement();
                }
            }

            w.WriteEndElement();
        }

        public MeasuredDataTimeseries CollectMeasuredData()
        {
            MeasuredDataTimeseries mdt = new MeasuredDataTimeseries();
            mdt.Name = name;
            mdt.Times = new List<float>();
            foreach (AbstractTrackPoint tp in positions)
            {
                float time = parentMetadata.GetNumericalTime(tp.T, TimeType.UserOrigin);
                mdt.Times.Add(time);
            }

            mdt.Data = new Dictionary<string, List<PointF>>();
            List<PointF> coords = new List<PointF>();
            foreach (AbstractTrackPoint tp in positions)
            {
                PointF p = tp.Point;
                if (PreferencesManager.PlayerPreferences.ExportSpace == ExportSpace.WorldSpace)
                    p = parentMetadata.CalibrationHelper.GetPointAtTime(tp.Point, tp.T);

                coords.Add(p);
            }
            mdt.Data.Add("0", coords);

            if (positions.Count > 0)
                mdt.FirstTimestamp = positions[0].T;

            return mdt;
        }

        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            invalid = true;
            tracker = new TrackerBlock2(GetTrackerParameters(new Size(800, 600)));

            if (timestampMapper == null)
            {
                string unparsed = xmlReader.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                return;
            }

            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            if (xmlReader.MoveToAttribute("name"))
                name = xmlReader.ReadContentAsString();

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "TimePosition":
                        beginTimeStamp = timestampMapper(xmlReader.ReadElementContentAsLong());
                        break;
                    case "Visible":
                        visibleTimestamp = timestampMapper(xmlReader.ReadElementContentAsLong());
                        break;
                    case "Invisible":
                        long hide = xmlReader.ReadElementContentAsLong();
                        invisibleTimestamp = (hide == -1) ? long.MaxValue : timestampMapper(hide);
                        break;
                    case "ExtraData":
                        {
                            measureLabelType = XmlHelper.ParseEnum<MeasureLabelType>(xmlReader.ReadElementContentAsString(), MeasureLabelType.None);
                            break;
                        }
                    case "Marker":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackMarker));
                            trackMarker = (TrackMarker)enumConverter.ConvertFromString(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "TrackerParameters":
                        tracker.Parameters = TrackerParameters.ReadXml(xmlReader, scale);
                        break;
                    case "TrackPointList":
                        ParseTrackPointList(xmlReader, scale, timestampMapper);
                        break;
                    case "DrawingStyle":
                        styleElements.ImportXML(xmlReader);
                        BindStyle();
                        break;
                    case "MainLabel":
                        {
                            miniLabel = new MiniLabel(xmlReader, scale);
                            break;
                        }
                    case "KeyframeLabelList":
                        ParseKeyframeLabelList(xmlReader, scale);
                        break;
                    case "SeeFuture":
                        seeFuture = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ShowTrackLabel":
                        showTrackLabel = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "ShowKeyframeLabels":
                        showKeyframeLabels = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "UseKeyframeColors":
                        useKeyframeColors = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "IsInteractiveTrack":
                        isInteractiveTrack = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "DisplayBestFitCircle":
                    case "ShowRotationCircle":
                        showRotationCircle = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
            scalingDone = true;

            if (positions.Count > 0)
            {
                endTimeStamp = positions.Last().T;
                miniLabel.SetAttach(positions[0].Point, false);
                
                if (positions.Count > 1 ||
                   positions[0].X != 0 ||
                   positions[0].Y != 0 ||
                   positions[0].T != 0)
                {
                    invalid = false;
                }
            }

            SanityCheckValues();

            // Depending on the order of parsing the main style initialization may have not impacted the mini labels.
            AfterMainStyleChange();
        }
        public void ParseTrackPointList(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            positions.Clear();
            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                if (xmlReader.Name == "TrackPoint")
                {
                    AbstractTrackPoint tp = tracker.CreateOrphanTrackPoint(PointF.Empty, 0);
                    tp.ReadXml(xmlReader);

                    // Time is stored in absolute timestamps.
                    AbstractTrackPoint adapted = tracker.CreateOrphanTrackPoint(tp.Point.Scale(scale.X, scale.Y), timestampMapper(tp.T));

                    positions.Add(adapted);
                }
                else
                {
                    string unparsed = xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            xmlReader.ReadEndElement();
        }
        public void ParseKeyframeLabelList(XmlReader xmlReader, PointF scale)
        {
            keyframeLabels.Clear();

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                if (xmlReader.Name == "KeyframeLabel")
                {
                    MiniLabel kfl = new MiniLabel(xmlReader, scale);

                    if (positions.Count > 0)
                    {
                        // Match with TrackPositions previously found.
                        int iMatchedTrackPosition = FindClosestPoint(kfl.Timestamp, positions);
                        kfl.AttachIndex = iMatchedTrackPosition;

                        kfl.SetAttach(positions[iMatchedTrackPosition].Point, false);
                        keyframeLabels.Add(kfl);
                    }
                }
                else
                {
                    string unparsed = xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            xmlReader.ReadEndElement();
        }
        private void SanityCheckValues()
        {
            visibleTimestamp = Math.Max(visibleTimestamp, 0);
            invisibleTimestamp = Math.Max(invisibleTimestamp, 0);
        }
        #endregion

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            if (scalingDone)
                return;

            TrackerParameters parameters = GetTrackerParameters(imageSize);
            tracker = new TrackerBlock2(parameters);
        }
        #endregion

        #region Miscellaneous public methods
        public void CalibrationChanged()
        {
            UpdateKinematics();
            UpdateKeyframeLabels();
        }
        public void UpdateKinematics()
        {
            List<TimedPoint> samples = positions.Select(p => new TimedPoint(p.X, p.Y, p.T)).ToList();
            filteredTrajectory.Initialize(samples, parentMetadata.CalibrationHelper);
            timeSeriesCollection = linearKinematics.BuildKinematics(filteredTrajectory, parentMetadata.CalibrationHelper);
        }
        public void Clear()
        {
            positions.Clear();
            keyframeLabels.Clear();
        }
        public void UpdateKeyframeLabels()
        {
            //-----------------------------------------------------------------------------------
            // The Keyframes list changed (add/remove/comments)
            // Reconstruct the Keyframes Labels, but don't completely reset those we already have
            // (Keep custom coordinates)
            //-----------------------------------------------------------------------------------

            // Keep track of matched keyframes so we can remove the others.
            bool[] matched = new bool[keyframeLabels.Count];

            // Filter out key images that are not in the trajectory boundaries.
            for (int i = 0; i < parentMetadata.Count; i++)
            {
                // Strictly superior because we don't show the keyframe that was created when the
                // user added the CrossMarker drawing to make the Track out of it.
                if (parentMetadata[i].Timestamp > beginTimeStamp &&
                    parentMetadata[i].Timestamp <= positions.Last().T)
                {
                    // The Keyframe is within the Trajectory interval.
                    // Do we know it already ?
                    int iKnown = -1;
                    for (int j = 0; j < keyframeLabels.Count; j++)
                    {
                        if (keyframeLabels[j].Timestamp == parentMetadata[i].Timestamp)
                        {
                            iKnown = j;
                            matched[j] = true;
                            break;
                        }
                    }

                    if (iKnown >= 0)
                    {
                        // Known Keyframe, import name and color.
                        //keyframesLabels[iKnown].SetText(parentMetadata[i].Title);
                        keyframeLabels[iKnown].Name = parentMetadata[i].Name;
                        keyframeLabels[iKnown].BackColor = useKeyframeColors ? parentMetadata[i].Color : styleData.Color;
                        keyframeLabels[iKnown].FontSize = (int)styleData.Font.Size;
                    }
                    else
                    {
                        // Unknown Keyframe, Configure and add it to list.
                        MiniLabel kfl = new MiniLabel();
                        kfl.AttachIndex = FindClosestPoint(parentMetadata[i].Timestamp);
                        kfl.SetAttach(positions[kfl.AttachIndex].Point, true);
                        kfl.Timestamp = positions[kfl.AttachIndex].T;
                        kfl.Name = parentMetadata[i].Name;
                        kfl.BackColor = useKeyframeColors ? parentMetadata[i].Color : styleData.Color;

                        kfl.FontSize = (int)styleData.Font.Size;

                        keyframeLabels.Add(kfl);
                    }
                }
            }

            // Remove unused Keyframes.
            // We only look in the original list and remove in reverse order so the index aren't messed up.
            for (int iLabel = matched.Length - 1; iLabel >= 0; iLabel--)
            {
                if (!matched[iLabel])
                    keyframeLabels.RemoveAt(iLabel);
            }

            // Recompute labels' text.
            if (measureLabelType != MeasureLabelType.None)
            {
                for (int iKfl = 0; iKfl < keyframeLabels.Count; iKfl++)
                {
                    if (measureLabelType == MeasureLabelType.Name)
                        keyframeLabels[iKfl].SetText(keyframeLabels[iKfl].Name, transformer);
                    else
                        keyframeLabels[iKfl].SetText(GetMeasureLabelText(keyframeLabels[iKfl].AttachIndex), transformer);
                }
            }

            // We also use this to update the main label font.
            miniLabel.FontSize = (int)styleData.Font.Size;
        }
        public void MemorizeState()
        {
            // Used by formConfigureTrajectory to be able to modify the trajectory in real time.
            memoLabel = name;
        }
        public void RecallState()
        {
            // Used when the user cancels his modifications on formConfigureTrajectory.
            // styleData has been reverted already as part of style elements mechanics.
            // The minilabels should have been reverted through the main styleData ValueChanged event.
            name = memoLabel;
        }

        public void FixRelativeTrajectories()
        {
            if (positions.Count == 0)
                return;

            // Used when importing an old KVA file from 0.8.15.
            // We used to store trajectory time relatively to the start of the trajectory, now we use absolute time.
            // It's very complicated to do that kind of arithmetic in the XSLT converter so we do it here.
            foreach (var position in positions)
                position.T += beginTimeStamp;

            endTimeStamp += beginTimeStamp;
        }
        public PointF GetPosition(long timestamp)
        {
            int index = FindClosestPoint(timestamp);
            return positions[index].Point;
        }
        #endregion

        #region Miscellaneous private methods

        /// <summary>
        /// Capture the current state and push it to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.TrackManager.Id, this.Id, this.Name, SerializationFilter.Core);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }

        private void ReloadMenusCulture()
        {
            // Visibility
            mnuVisibility.Text = ScreenManagerLang.Generic_Visibility;
            mnuHideBefore.Text = ScreenManagerLang.mnuHideBefore;
            mnuShowBefore.Text = ScreenManagerLang.mnuShowBefore;
            mnuHideAfter.Text = ScreenManagerLang.mnuHideAfter;
            mnuShowAfter.Text = ScreenManagerLang.mnuShowAfter;

            // Tracking
            mnuTracking.Text = ScreenManagerLang.dlgConfigureTrajectory_Tracking;
            mnuTrackingStart.Text = ScreenManagerLang.mnuDrawingTrackingStart;
            mnuTrackingStop.Text = ScreenManagerLang.mnuDrawingTrackingStop;
            mnuTrackingTrim.Text = ScreenManagerLang.mnuDeleteEndOfTrajectory;

            // Measurement
            mnuMeasurement.Text = ScreenManagerLang.mnuMeasure_Labels_Menu;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                ToolStripMenuItem tsmi = pair.Value;
                MeasureLabelType measureLabelType = pair.Key;
                tsmi.Text = GetMeasureLabelOptionText(measureLabelType);
                tsmi.Checked = this.measureLabelType == measureLabelType;
            }

            // Display options
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuSeeFuture.Text = ScreenManagerLang.mnuOptions_Trajectory_SeeFuture;
            mnuShowTrackLabel.Text = ScreenManagerLang.mnuOptions_Trajectory_ShowTrackLabel;
            mnuShowKeyframeLabel.Text = ScreenManagerLang.mnuOptions_Trajectory_ShowKeyImageLabels;
            mnuUseKeyframeColor.Text = ScreenManagerLang.mnuOptions_Trajectory_UseKeyImageColors;
            mnuIsInteractiveTrack.Text = ScreenManagerLang.mnuOptions_Trajectory_Interactive;
            mnuShowRotationCircle.Text = ScreenManagerLang.mnuOptions_Trajectory_ShowRotationCircle;
        }

        /// <summary>
        /// Create a new measure label type menu and store it in the global dictionary.
        /// </summary>
        private ToolStripMenuItem CreateMeasureLabelTypeMenu(MeasureLabelType measureLabelType)
        {
            // Note: the tag is reserved for injecting the screen user interface to support invalidation.
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Click += mnuMeasureLabelType_Click;
            mnuMeasureLabelTypes.Add(measureLabelType, mnu);
            return mnu;
        }

        private void mnuMeasureLabelType_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            MeasureLabelType measureLabelType = MeasureLabelType.None;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                if (pair.Value == tsmi)
                {
                    measureLabelType = pair.Key;
                    break;
                }
            }

            this.measureLabelType = measureLabelType;
            UpdateKeyframeLabels();
            InvalidateFromMenu(tsmi);
        }
        private void AfterTrackStatusChanged()
        {
            if (trackStatus == TrackStatus.Interactive)
                UpdateKinematics();
            else if (trackStatus == TrackStatus.Configuration)
                UpdateBoundingBoxes();
        }
        private void UpdateBoundingBoxes()
        {
            searchWindow.Rectangle = positions[drawPointIndex].Point.Box(tracker.Parameters.SearchWindow).ToRectangle();
            blockWindow.Rectangle = positions[drawPointIndex].Point.Box(tracker.Parameters.BlockWindow).ToRectangle();
        }
        private int FindClosestPoint(long currentTimestamp)
        {
            return FindClosestPoint(currentTimestamp, positions);
        }
        private int FindClosestPoint(long currentTimestamp, List<AbstractTrackPoint> positions)
        {
            // Find the closest registered timestamp
            // Parameter is given in absolute timestamp.
            long minErr = long.MaxValue;
            int closest = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                long err = Math.Abs(positions[i].T - currentTimestamp);
                if (err < minErr)
                {
                    minErr = err;
                    closest = i;
                }
            }

            return closest;
        }

        private void BindStyle()
        {
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "line size");
            styleElements.Bind(styleData, "TrackShape", "track shape");
            styleElements.Bind(styleData, "Font", "label size");
        }

        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            AfterMainStyleChange();
        }

        private void AfterMainStyleChange()
        {
            // Impact the style of mini labels based on the main color.
            miniLabel.BackColor = styleData.Color;
            foreach (MiniLabel kfl in keyframeLabels)
                kfl.BackColor = styleData.Color;
        }
        #endregion
    }
}
