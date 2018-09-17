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
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using System.Xml.Serialization;

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
    [XmlType ("Track")]
    public class DrawingTrack : AbstractDrawing, IDecorable, IScalable, IKvaSerializable
    {
        #region Events
        public event EventHandler TrackerParametersChanged;
        #endregion

        #region Delegates
        // To ask the UI to display the frame closest to selected pos.
        // used when moving the target in direct interactive mode.
        public ClosestFrameDisplayer ClosestFrameDisplayer;     
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
                hash ^= trackView.GetHashCode();
                foreach (AbstractTrackPoint p in positions)
                    hash ^= p.ContentHash;
                
                hash ^= defaultCrossRadius.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= miniLabel.GetHashCode();
                
                foreach (MiniLabel kfl in keyframesLabels)
                    hash ^= kfl.GetHashCode();

                hash ^= tracker.Parameters.ContentHash;
                
                return hash;
            }
        }
        public TrackView View
        {
            get { return trackView; }
            set { trackView = value; }
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
        public TrackExtraData ExtraData
        {
            get { return trackExtraData; }
            set 
            { 
                trackExtraData = value; 
                IntegrateKeyframes();
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
        public bool DisplayBestFitCircle
        {
            get { return displayBestFitCircle; }
            set { displayBestFitCircle = value; }
        }

        public long BeginTimeStamp
        {
            get { return beginTimeStamp; }
        }
        public long EndTimeStamp
        {
            get { return endTimeStamp; }
        }
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public Color MainColor
        {    
            get { return styleHelper.Color; }
        }
        public Metadata ParentMetadata
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
            get { return invalid;}
        }
        // Fading is not modifiable from outside.
        public override InfosFading  InfosFading
        {
            get { return null;}
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
                if (trackStatus != TrackStatus.Interactive)
                    return null;

                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                // Initialize menu each time to get translated texts.
                ReinitializeMenu();
                contextMenu.Add(mnuMeasurement);
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
        private TrackView trackView = TrackView.Complete;
        private TrackStatus trackStatus = TrackStatus.Interactive;
        private TrackExtraData trackExtraData = TrackExtraData.None;
        private TrackMarker trackMarker = TrackMarker.Cross;
        private bool displayBestFitCircle;
        private int movingHandler = -1;
        private bool invalid;                                 // Used for XML import.
        private bool scalingDone;
            
        // Tracker tool.
        private AbstractTracker tracker;
        
        // Hardwired parameters.
        private const int defaultCrossRadius = 4;
        private const int allowedFramesOver = 12;      // Number of frames over which the global fading spans (after end point).
        private const int focusFadingFrames = 30;    // Number of frames of the focus section. 
       
        // Internal data.
        private List<AbstractTrackPoint> positions = new List<AbstractTrackPoint>();
        private FilteredTrajectory filteredTrajectory = new FilteredTrajectory();
        private TimeSeriesCollection timeSeriesCollection;
        private LinearKinematics linearKinematics = new LinearKinematics();
        private IImageToViewportTransformer transformer;
        
        private long beginTimeStamp;                 // absolute.
        private long endTimeStamp = long.MaxValue;     // absolute.
        private int totalDistance;                   // This is used to normalize timestamps to a par scale with distances.
        private int currentPoint;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private MiniLabel miniLabel = new MiniLabel();
        private List<MiniLabel> keyframesLabels = new List<MiniLabel>();
        private InfosFading infosFading = new InfosFading(long.MaxValue, 1);
        private const int baseAlpha = 224;                // alpha of track in most cases.
        private const int afterCurrentAlpha = 64;        // alpha of track after the current point when in normal mode.
        private const int editModeAlpha = 128;            // alpha of track when in Edit mode.
        private const int labelFollowsTrackAlpha = 80;    // alpha of track when in LabelFollows view.

        // Configuration
        private BoundingBox searchWindow = new BoundingBox(10);
        private BoundingBox blockWindow = new BoundingBox(4);

        // Context menu
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private List<ToolStripMenuItem> mnuMeasurementOptions = new List<ToolStripMenuItem>();
        
        // Memorization poul
        private TrackView memoTrackView;
        private string memoLabel;
        private Metadata parentMetadata;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingTrack(PointF origin, long t, DrawingStyle preset)
        {
            tracker = new TrackerBlock2(GetTrackerParameters(new Size(800, 600)));
            positions.Add(new TrackPointBlock(origin.X, origin.Y, t));

            beginTimeStamp = t;
            endTimeStamp = t;
            miniLabel.SetAttach(origin, true);
                
            infosFading.FadingFrames = allowedFramesOver;
            infosFading.UseDefault = false;
            infosFading.Enabled = true;
            
            styleHelper.Color = Color.Black;
            styleHelper.LineSize = 3;
            styleHelper.TrackShape = TrackShape.Dash;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset != null)
            {
                style = preset.Clone();
                BindStyle();
            }
            
            ReinitializeMenu();
        }
        
        public DrawingTrack(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(PointF.Empty, 0, null)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (currentTimestamp < beginTimeStamp)
                return;
                
            this.transformer = transformer;
            
            // 0. Compute the fading factor. 
            // Special case from other drawings:
            // ref frame is last point, and we only fade after it, not before.
            double opacityFactor = 1.0;
            if (trackStatus == TrackStatus.Interactive && currentTimestamp > endTimeStamp)
            {
                infosFading.ReferenceTimestamp = endTimeStamp;
                opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            }
            
            if(opacityFactor <= 0)
                return;

            currentPoint = FindClosestPoint(currentTimestamp);
            
            // Draw various elements depending on combination of view and status.
            // The exact alpha at which the traj will be drawn will be decided in GetTrackPen().
            if(positions.Count > 1)
            {
                // Key Images titles.
                if (trackStatus == TrackStatus.Interactive && trackView != TrackView.Label && trackExtraData != TrackExtraData.None)
                    DrawKeyframesTitles(canvas, opacityFactor, transformer);
                
                // Track.
                int first = GetFirstVisiblePoint();
                int last = GetLastVisiblePoint();
                if (trackStatus == TrackStatus.Interactive && trackView == TrackView.Complete)
                {
                    DrawTrajectory(canvas, first, currentPoint, true, opacityFactor, transformer);
                    DrawTrajectory(canvas, currentPoint, last, false, opacityFactor, transformer);
                }
                else
                {
                    DrawTrajectory(canvas, first, last, false, opacityFactor, transformer);
                }
            }
            
            if(positions.Count > 0)
            {
                // Angular motion
                if (displayBestFitCircle && trackStatus == TrackStatus.Interactive)
                    DrawBestFitCircle(canvas, currentPoint, opacityFactor, transformer);

                // Track.
                if( opacityFactor == 1.0 && trackView != TrackView.Label)
                    DrawMarker(canvas, opacityFactor, transformer);
                
                if (opacityFactor == 1.0)
                    DrawTrackerHelp(canvas, transformer, styleHelper.Color, opacityFactor);

                // Main label.
                if (trackStatus == TrackStatus.Interactive && trackView == TrackView.Label ||
                    trackStatus == TrackStatus.Interactive && trackExtraData != TrackExtraData.None)
                {
                    DrawMainLabel(canvas, currentPoint, opacityFactor, transformer);
                }
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            if (trackStatus == TrackStatus.Interactive && movingHandler > 1)
            {
                MoveLabelTo(dx, dy, movingHandler);
                return;
            }

            if (movingHandler == 1 && (trackStatus == TrackStatus.Edit || trackStatus == TrackStatus.Configuration))
            {
                positions[currentPoint].X += dx;
                positions[currentPoint].Y += dy;

                if (trackStatus == TrackStatus.Configuration)
                    UpdateBoundingBoxes();
                return;
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (trackStatus == TrackStatus.Interactive && (handleNumber == 0 || handleNumber == 1))
            {
                MoveCursor(point.X, point.Y);
            }
            else if (trackStatus == TrackStatus.Configuration)
            {
                TrackerParameters old = tracker.Parameters;
                
                if (movingHandler > 1 && movingHandler < 6)
                    searchWindow.MoveHandleKeepSymmetry(point.ToPoint(), movingHandler - 1, positions[currentPoint].Point);
                else if (movingHandler >= 6 && movingHandler < 11)
                    blockWindow.MoveHandleKeepSymmetry(point.ToPoint(), movingHandler - 5, positions[currentPoint].Point);

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
            if (currentTimestamp < beginTimeStamp || currentTimestamp > endTimeStamp)
            {
                movingHandler = -1;
                return -1;
            }

            int result = -1;
            switch (trackStatus)
            {
                case TrackStatus.Edit:
                    result = HitTestEdit(point, currentTimestamp, transformer);
                    break;
                case TrackStatus.Configuration:
                    result = HitTestConfiguration(point, currentTimestamp, transformer);
                    break;
                case TrackStatus.Interactive:
                    result = HitTestInteractive(point, currentTimestamp, transformer);
                    break;
            }
        
            movingHandler = result;
            
            return result;
        }
        private int HitTestEdit(PointF point, long currentTimestamp, IImageToViewportTransformer transformer)
        {
            // 1: search window.
            RectangleF search = positions[currentPoint].Point.Box(tracker.Parameters.SearchWindow);
            if (search.Contains(point))
                return 1;

            return -1;
        }
        private int HitTestConfiguration(PointF point, long currentTimestamp, IImageToViewportTransformer transformer)
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
        private int HitTestInteractive(PointF point, long currentTimestamp, IImageToViewportTransformer transformer)
        {
            // 0: track, 1: current point on track, 2: main label, 3+: keyframe label.
            int result = IsOnKeyframesLabels(point, transformer);
            if (result >= 0)
                return result;

            if (HitTester.HitTest(positions[currentPoint].Point, point, transformer))
                return 1;

            result = HitTestTrajectory(point, transformer);
            
            if (result == 0)
                MoveCursor(point.X, point.Y);

            return result;
        }
        private int HitTestTrajectory(PointF point, IImageToViewportTransformer transformer)
        {
            // 0: track. -1: not on track.
            int result = -1;
            
            try
            {
                int iStart = GetFirstVisiblePoint();
                int iEnd = GetLastVisiblePoint();
                int iTotalVisiblePoints = iEnd - iStart;
                Point[] points = new Point[iTotalVisiblePoints];
                for (int i = iStart; i < iEnd; i++)
                    points[i - iStart] = positions[i].Point.ToPoint();

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddCurve(points, 0.5f);
                    RectangleF bounds = path.GetBounds();
                    if (!bounds.IsEmpty)
                    {
                        bool hit = HitTester.HitTest(path, point, styleHelper.LineSize, false, transformer);
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
        public override PointF GetPosition()
        {
            return positions[0].Point;
        }
        #endregion

        #region Drawing routines
        private void DrawTrajectory(Graphics canvas, int start, int end, bool before, double fadingFactor, IImageToViewportTransformer transformer)
        {
            // Points are drawn with various alpha values, possibly 0:
            // In edit mode, all segments are drawn at 64 alpha.
            // In normal mode, segments before the current point are drawn at 224, segments after at 64.
            // In focus mode, (edit or normal) only a subset of segments are drawn from each part.
            // It is not possible currently to make the curve vary smoothly in alpha.
            // Either we make it vary in alpha for each segment but draw as connected lines.
            // or draw as curve but at the same alpha for all.
            // All segments are drawn at 224, even the after section.

            if (trackStatus == TrackStatus.Configuration)
                return;

            Point[] points = new Point[end - start + 1];
            for (int i = 0; i <= end - start; i++)
                points[i] = transformer.Transform(positions[start + i].Point);

            if (points.Length <= 1)
                return;
            
            using(Pen trackPen = GetTrackPen(trackStatus, fadingFactor, before))
            {
                // Tension parameter is at 0.5f for bezier effect (smooth curve).
                canvas.DrawCurve(trackPen, points, 0.5f);
                    
                if(styleHelper.TrackShape.ShowSteps)
                {
                    using(Pen stepPen = new Pen(trackPen.Color, 2))
                    {
                        int margin = (int)(trackPen.Width * 1.5);
                        foreach(Point p in points)
                            canvas.DrawEllipse(stepPen, p.Box(margin));
                    }
                }
            }
        }
        private void DrawMarker(Graphics canvas,  double fadingFactor, IImageToViewportTransformer transformer)
        {
            int radius = defaultCrossRadius;
            Point location = transformer.Transform(positions[currentPoint].Point);
            
            if (trackMarker == TrackMarker.Cross || trackStatus == TrackStatus.Edit || trackStatus == TrackStatus.Configuration)
            {
                using(Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleHelper.Color)))
                {
                  canvas.DrawLine(p, location.X, location.Y - radius, location.X, location.Y + radius);
                  canvas.DrawLine(p, location.X - radius, location.Y, location.X + radius, location.Y);
                }
            }
            else if (trackMarker == TrackMarker.Circle)
            {
                using (Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleHelper.Color)))
                {
                    canvas.DrawEllipse(p, location.Box(radius));
                }
            }
            else if (trackMarker == TrackMarker.Target)
            {
                int diameter = radius * 2;
                canvas.FillPie(Brushes.Black, location.X - radius , location.Y - radius , diameter, diameter, 0, 90);
                canvas.FillPie(Brushes.White, location.X - radius , location.Y - radius , diameter, diameter, 90, 90);
                canvas.FillPie(Brushes.Black, location.X - radius , location.Y - radius , diameter, diameter, 180, 90);
                canvas.FillPie(Brushes.White, location.X - radius , location.Y - radius , diameter, diameter, 270, 90);
                canvas.DrawEllipse(Pens.White, location.Box(radius + 2));
            }   
        }
        private void DrawTrackerHelp(Graphics canvas, IImageToViewportTransformer transformer, Color color, double opacity)
        {
            if (trackStatus == TrackStatus.Edit)
            {
                tracker.Draw(canvas, positions[currentPoint], transformer, styleHelper.Color, opacity);
            }
            /*else if (trackStatus == TrackStatus.Interactive)
            {
                tracker.Draw(canvas, positions[currentPoint], transformer, styleHelper.Color, opacity);
            }*/
            else if (trackStatus == TrackStatus.Configuration)
            {
                Point location = transformer.Transform(positions[currentPoint].Point);
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
                using (Pen p = new Pen(Color.FromArgb(255, styleHelper.Color)))
                using (SolidBrush b = new SolidBrush(p.Color))
                {
                    searchWindow.Draw(canvas, searchBox, p, b, 4);
                    blockWindow.Draw(canvas, location.Box(blockSize), p, b, 3);
                }
            }
        }
        private void DrawKeyframesTitles(Graphics canvas, double fadingFactor, IImageToViewportTransformer transformer)
        {
            //------------------------------------------------------------
            // Draw the Keyframes labels
            // Each Label has its own coords and is movable.
            // Each label is connected to the TrackPosition point.
            // Rescaling for the current image size has already been done.
            //------------------------------------------------------------
            if (fadingFactor < 0 || trackStatus == TrackStatus.Configuration)
                return;
            
            foreach (MiniLabel kl in keyframesLabels)
            {
                // In focus mode, only show labels that are in focus section.
                if(trackView == TrackView.Complete || infosFading.IsVisible(positions[currentPoint].T, kl.Timestamp, focusFadingFrames))
                    kl.Draw(canvas, transformer, fadingFactor);
            }
        }
        private void DrawBestFitCircle(Graphics canvas, int currentPoint, double fadingFactor, IImageToViewportTransformer transformer)
        {
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
            float angle = (float)(ellipse.Rotation * MathHelper.RadiansToDegrees);
            
            using (Pen p = new Pen(Color.FromArgb((int)(fadingFactor * 255), styleHelper.Color)))
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
        private void DrawMainLabel(Graphics canvas, int currentPoint, double fadingFactor, IImageToViewportTransformer transformer)
        {
            // Draw the main label and its connector to the current point.
            if (fadingFactor != 1.0f || trackStatus == TrackStatus.Configuration)
                return;
            
            miniLabel.SetAttach(positions[currentPoint].Point, true);
                
            string text = trackView == TrackView.Label ? name : GetExtraDataText(currentPoint);
            miniLabel.SetText(text);
            miniLabel.Draw(canvas, transformer, fadingFactor);
        }
        private Pen GetTrackPen(TrackStatus status, double fadingFactor, bool before)
        {
            int alpha = 0;
            
            if(status == TrackStatus.Edit)
            {
                alpha = editModeAlpha;
            }
            else 
            {
                if(trackView == TrackView.Complete)
                {
                    if(before)
                    {
                        alpha = (int)(fadingFactor * baseAlpha);
                    }
                    else
                    {
                        alpha = afterCurrentAlpha;
                    }
                }
                else if(trackView == TrackView.Focus)
                {
                    alpha = (int)(fadingFactor * baseAlpha);
                }
                else if(trackView == TrackView.Label)
                {
                    alpha = (int)(fadingFactor * labelFollowsTrackAlpha);
                }
            }
            
            return styleHelper.GetPen(alpha, 1.0);
        }
        #endregion

        #region Extra informations (Speed, distance)
        public string GetExtraDataOptionText(TrackExtraData data)
        {
            switch (data)
            {
                case TrackExtraData.None: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None;
                case TrackExtraData.Name: return ScreenManagerLang.dlgConfigureDrawing_Name;
                case TrackExtraData.Position: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Position;

                case TrackExtraData.TotalDistance: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalDistance;
                case TrackExtraData.TotalHorizontalDisplacement: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalHorizontalDisplacement;
                case TrackExtraData.TotalVerticalDisplacement: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalVerticalDisplacement;

                case TrackExtraData.Speed: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed;
                case TrackExtraData.HorizontalVelocity: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalVelocity;
                case TrackExtraData.VerticalVelocity: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalVelocity;
                
                case TrackExtraData.Acceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Acceleration;
                case TrackExtraData.HorizontalAcceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalAcceleration;
                case TrackExtraData.VerticalAcceleration: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalAcceleration;
            }

            return "";
        }

        public bool IsUsingAngularKinematics()
        {
            return false;
            /*return trackExtraData == TrackExtraData.AngularDisplacement ||
                trackExtraData == TrackExtraData.AngularVelocity ||
                trackExtraData == TrackExtraData.AngularAcceleration ||
                trackExtraData == TrackExtraData.CentripetalAcceleration;*/
        }

        private string GetExtraDataText(int index)
        {
            CalibrationHelper helper = parentMetadata.CalibrationHelper;
            CultureInfo culture = CultureInfo.InvariantCulture;
            string displayText = "###";

            switch(trackExtraData)
            {
                case TrackExtraData.Name:
                    displayText = name;
                    break;
                case TrackExtraData.Position:
                    double x = timeSeriesCollection[Kinematics.XRaw][index];
                    double y = timeSeriesCollection[Kinematics.YRaw][index];
                    displayText = string.Format(culture, "{0:0.00} ; {1:0.00} {2}", x, y, helper.GetLengthAbbreviation());
                    break;
                
                case TrackExtraData.TotalDistance:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearDistance, index, helper.GetLengthAbbreviation());
                    break;
                case TrackExtraData.TotalHorizontalDisplacement:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalDisplacement, index, helper.GetLengthAbbreviation());
                    break;
                case TrackExtraData.TotalVerticalDisplacement:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalDisplacement, index, helper.GetLengthAbbreviation());
                    break;

                case TrackExtraData.Speed:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearSpeed, index, helper.GetSpeedAbbreviation());
                    break;
                case TrackExtraData.HorizontalVelocity:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalVelocity, index, helper.GetSpeedAbbreviation());
                    break;
                case TrackExtraData.VerticalVelocity:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalVelocity, index, helper.GetSpeedAbbreviation());
                    break;

                case TrackExtraData.Acceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.HorizontalAcceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearHorizontalAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.VerticalAcceleration:
                    displayText = GetKinematicsDisplayText(Kinematics.LinearVerticalAcceleration, index, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.None:
                default:
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
                // Move cursor to new coords
                // In this case, _X and _Y are delta values.
                // Image will be reseted at mouse up. (=> UpdateTrackPoint)
                positions[currentPoint].X += dx;
                positions[currentPoint].Y += dy;
            }
            else
            {
                // Move Playhead to closest frame (x,y,t).
                // In this case, _X and _Y are absolute values.
                if (ClosestFrameDisplayer != null && positions.Count > 1)
                    ClosestFrameDisplayer(new Point((int)dx, (int)dy), positions, totalDistance, false);
            }
        }
        private void MoveLabelTo(float dx, float dy, int labelNumber)
        {
            // _iLabelNumber coding: 2 = main label, 3+ = keyframes labels.
            
            if (trackStatus == TrackStatus.Edit || trackView != TrackView.Label)
            {
                if(trackExtraData != TrackExtraData.None && labelNumber == 2)
                {
                    // Move the main label.
                    miniLabel.MoveLabel(dx, dy);
                }
                else
                {
                    // Move the specified label by specified amount.
                    int iLabel = labelNumber - 3;
                    keyframesLabels[iLabel].MoveLabel(dx, dy);
                }
            }
            else if (trackView == TrackView.Label)
            {
                miniLabel.MoveLabel(dx, dy);
            }
        }
        private int IsOnKeyframesLabels(PointF point, IImageToViewportTransformer transformer)
        {
            // Convention: -1 = miss, 2 = on main label, 3+ = on keyframe label.
            int hitResult = -1;
            if (trackView == TrackView.Label)
            {
                if (miniLabel.HitTest(point, transformer))
                    hitResult = 2;
            }
            else
            {
                // Even when we aren't in TrackView.Label, the main label is visible
                // if we are displaying the extra data (distance, speed).
                if (trackExtraData != TrackExtraData.None)
                {
                    if (miniLabel.HitTest(point, transformer))
                        hitResult = 2;
                }
                
                for (int i = 0; i < keyframesLabels.Count; i++)
                {
                    bool isVisible = infosFading.IsVisible(positions[currentPoint].T, 
                                                             keyframesLabels[i].Timestamp, 
                                                             focusFadingFrames);
                    if(trackView == TrackView.Complete || isVisible)
                    {
                        if (keyframesLabels[i].HitTest(point, transformer))
                        {
                            hitResult = i + 3;
                            break;
                        }
                    }
                }
            }

            return hitResult;
        }
        private int GetFirstVisiblePoint()
        {
            if((trackView != TrackView.Complete || trackStatus == TrackStatus.Edit) && currentPoint - focusFadingFrames > 0)
                return currentPoint - focusFadingFrames;
            else
                return 0;
        }
        private int GetLastVisiblePoint()
        {
            if((trackView != TrackView.Complete || trackStatus == TrackStatus.Edit) && currentPoint + focusFadingFrames < positions.Count - 1)
                return currentPoint + focusFadingFrames;
            else
                return positions.Count - 1;
        }
        #endregion
        
        #region Context Menu implementation
        public void ChopTrajectory(long currentTimestamp)
        {
            // Delete end of track.
            currentPoint = FindClosestPoint(currentTimestamp);
            if (currentPoint < positions.Count - 1)
                positions.RemoveRange(currentPoint + 1, positions.Count - currentPoint - 1);

            endTimeStamp = positions[positions.Count - 1].T;

            UpdateKinematics();
            IntegrateKeyframes();
        }
        public void StopTracking()
        {
            trackStatus = TrackStatus.Interactive;
            AfterTrackStatusChanged();
        }
        public void RestartTracking()
        {
            trackStatus = TrackStatus.Edit;
            AfterTrackStatusChanged();
        }
        #endregion
        
        #region Tracking
        public void TrackCurrentPosition(VideoFrame current)
        {
            // Match the previous point in current image.
            // New points to trajectories are always created from here, 

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
            bool bMatched = tracker.Track(positions, current.Image, current.Timestamp, out p);
                
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
            ComputeFlatDistance();
            IntegrateKeyframes();
        }
        private void ComputeFlatDistance()
        {
            // This distance is used to normalize distance vs time in interactive manipulation.
            
            int smallestTop = int.MaxValue;
            int smallestLeft = int.MaxValue;
            int highestBottom = -1;
            int highestRight = -1;

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].X < smallestLeft)
                    smallestLeft = (int)positions[i].X;

                if (positions[i].X > highestRight)
                    highestRight = (int)positions[i].X;

                if (positions[i].Y < smallestTop)
                    smallestTop = (int)positions[i].Y;
                
                if (positions[i].Y > highestBottom)
                    highestBottom = (int)positions[i].Y;
            }

            totalDistance = (int)Math.Sqrt(((highestRight - smallestLeft) * (highestRight - smallestLeft))
                                       + ((highestBottom - smallestTop) * (highestBottom - smallestTop)));
        }
        public void UpdateTrackPoint(Bitmap currentImage)
        {
            // The user moved a point that had been previously placed.
            // We need to reconstruct tracking data stored in the point, for later tracking.
            // The coordinate of the point have already been updated during the mouse move.
            if (currentImage == null || positions.Count < 1 || currentPoint < 0)
                return;
            
            AbstractTrackPoint current = positions[currentPoint];
        
            current.ResetTrackData();
            AbstractTrackPoint atp = tracker.CreateTrackPoint(true, current.Point, 1.0f, current.T,  currentImage, positions);
            
            if(atp != null)
                 positions[currentPoint] = atp;
            
            // Update the mini label (attach, position of label, and text).
            for (int i = 0; i < keyframesLabels.Count; i++)
            {
                if(keyframesLabels[i].Timestamp == current.T)
                {
                    keyframesLabels[i].SetAttach(current.Point, true);
                    if(trackExtraData != TrackExtraData.None)
                        keyframesLabels[i].SetText(GetExtraDataText(keyframesLabels[i].AttachIndex));
                    
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

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackView));
                string xmlMode = enumConverter.ConvertToString(trackView);
                w.WriteElementString("Mode", xmlMode);
            
                enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
                string xmlExtraData = enumConverter.ConvertToString(trackExtraData);
                w.WriteElementString("ExtraData", xmlExtraData);

                enumConverter = TypeDescriptor.GetConverter(typeof(TrackMarker));
                string xmlTrackMarker = enumConverter.ConvertToString(trackMarker);
                w.WriteElementString("Marker", xmlTrackMarker);

                w.WriteElementString("DisplayBestFitCircle", displayBestFitCircle.ToString().ToLower());

                w.WriteStartElement("TrackerParameters");
                tracker.Parameters.WriteXml(w);
                w.WriteEndElement();

                TrackPointsToXml(w);

                w.WriteStartElement("MainLabel");
                w.WriteAttributeString("Text", name);

                // Reset to first point.
                if (positions.Count > 0)
                    miniLabel.SetAttach(positions[0].Point, true);

                miniLabel.WriteXml(w);
                w.WriteEndElement();

                if (positions.Count > 0 && currentPoint < positions.Count)
                    miniLabel.SetAttach(positions[currentPoint].Point, true);

                if (keyframesLabels.Count > 0)
                {
                    w.WriteStartElement("KeyframeLabelList");
                    w.WriteAttributeString("Count", keyframesLabels.Count.ToString());

                    foreach (MiniLabel kfl in keyframesLabels)
                    {
                        w.WriteStartElement("KeyframeLabel");
                        kfl.WriteXml(w);
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }
        }
        private void TrackPointsToXml(XmlWriter w)
        {
            w.WriteStartElement("TrackPointList");
            w.WriteAttributeString("Count", positions.Count.ToString());
            w.WriteAttributeString("UserUnitLength", parentMetadata.CalibrationHelper.GetLengthAbbreviation());
            
            if(positions.Count > 0)
            {
                foreach (AbstractTrackPoint tp in positions)
                {
                    w.WriteStartElement("TrackPoint");
                    
                    PointF p = parentMetadata.CalibrationHelper.GetPointAtTime(tp.Point, tp.T);
                    string userT = parentMetadata.TimeCodeBuilder(tp.T, TimeType.Time, TimecodeFormat.Unknown, false);
                    
                    w.WriteAttributeString("UserX", String.Format("{0:0.00}", p.X));
                    w.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.X));
                    w.WriteAttributeString("UserY", String.Format("{0:0.00}", p.Y));
                    w.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.Y));
                    w.WriteAttributeString("UserTime", userT);
            
                    tp.WriteXml(w);
                    
                    w.WriteEndElement();
                }
            }

            w.WriteEndElement();
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
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "TimePosition":
                        beginTimeStamp = timestampMapper(xmlReader.ReadElementContentAsLong(), false);
                        break;
                    case "Mode":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackView));
                            trackView = (TrackView)enumConverter.ConvertFromString(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "ExtraData":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
                            trackExtraData = (TrackExtraData)enumConverter.ConvertFromString(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "Marker":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackMarker));
                            trackMarker = (TrackMarker)enumConverter.ConvertFromString(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "DisplayBestFitCircle":
                        displayBestFitCircle = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "TrackerParameters":
                        tracker.Parameters = TrackerParameters.ReadXml(xmlReader, scale);
                        break;
                    case "TrackPointList":
                        ParseTrackPointList(xmlReader, scale, timestampMapper);
                        break;
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
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
                miniLabel.SetText(name);
                
                if(positions.Count > 1 || 
                   positions[0].X != 0 || 
                   positions[0].Y != 0 || 
                   positions[0].T != 0)
                {
                    invalid = false;
                }
            }

            // Depending on the order of parsing the main style initialization may have not impacted the mini labels.
            AfterMainStyleChange();
        }
        public void ParseTrackPointList(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            positions.Clear();
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                if(xmlReader.Name == "TrackPoint")
                {
                    AbstractTrackPoint tp = tracker.CreateOrphanTrackPoint(PointF.Empty, 0);
                    tp.ReadXml(xmlReader);
                    
                    // Time is stored in absolute timestamps.
                    AbstractTrackPoint adapted = tracker.CreateOrphanTrackPoint(tp.Point.Scale(scale.X, scale.Y), timestampMapper(tp.T, true));

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
            keyframesLabels.Clear();

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                if(xmlReader.Name == "KeyframeLabel")
                {
                    MiniLabel kfl = new MiniLabel(xmlReader, scale);
                    
                    if (positions.Count > 0)
                    {
                        // Match with TrackPositions previously found.
                        int iMatchedTrackPosition = FindClosestPoint(kfl.Timestamp, positions);
                        kfl.AttachIndex = iMatchedTrackPosition;
                        
                        kfl.SetAttach(positions[iMatchedTrackPosition].Point, false);
                        keyframesLabels.Add(kfl);
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
            IntegrateKeyframes();
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
            keyframesLabels.Clear();
        }
        public void IntegrateKeyframes()
        {
            //-----------------------------------------------------------------------------------
            // The Keyframes list changed (add/remove/comments)
            // Reconstruct the Keyframes Labels, but don't completely reset those we already have
            // (Keep custom coordinates)
            //-----------------------------------------------------------------------------------

            // Keep track of matched keyframes so we can remove the others.
            bool[] matched = new bool[keyframesLabels.Count];

            // Filter out key images that are not in the trajectory boundaries.
            for (int i = 0; i < parentMetadata.Count; i++)
            {
                // Strictly superior because we don't show the keyframe that was created when the
                // user added the CrossMarker drawing to make the Track out of it.
                if (parentMetadata[i].Position > beginTimeStamp && 
                    parentMetadata[i].Position <= positions.Last().T)
                {
                    // The Keyframe is within the Trajectory interval.
                    // Do we know it already ?
                    int iKnown = - 1;
                    for(int j=0;j<keyframesLabels.Count;j++)
                    {
                        if (keyframesLabels[j].Timestamp == parentMetadata[i].Position)
                        {
                            iKnown = j;
                            matched[j] = true;
                            break;
                        }
                    }
                    
                    if (iKnown >= 0)
                    {
                        // Known Keyframe, Read text again in case it changed
                        keyframesLabels[iKnown].SetText(parentMetadata[i].Title);
                    }
                    else
                    {
                        // Unknown Keyframe, Configure and add it to list.
                        MiniLabel kfl = new MiniLabel();
                        kfl.AttachIndex = FindClosestPoint(parentMetadata[i].Position);
                        kfl.SetAttach(positions[kfl.AttachIndex].Point, true);
                        kfl.Timestamp = positions[kfl.AttachIndex].T;                        
                        kfl.SetText(parentMetadata[i].Title);
                        
                        keyframesLabels.Add(kfl);
                    }
                }
            }

            // Remove unused Keyframes.
            // We only look in the original list and remove in reverse order so the index aren't messed up.
            for (int iLabel = matched.Length - 1; iLabel >= 0; iLabel--)
            {
                if (!matched[iLabel])
                    keyframesLabels.RemoveAt(iLabel);
            }
            
            // Reinject the labels in the list for extra data.
            if(trackExtraData != TrackExtraData.None)
            {
                for( int iKfl = 0; iKfl < keyframesLabels.Count; iKfl++)
                    keyframesLabels[iKfl].SetText(GetExtraDataText(keyframesLabels[iKfl].AttachIndex));
            }

            // Reapply style.
            foreach (MiniLabel kfl in keyframesLabels)
                kfl.BackColor = styleHelper.Color;
        }
        public void MemorizeState()
        {
            // Used by formConfigureTrajectory to be able to modify the trajectory in real time.
            memoTrackView = trackView;
            memoLabel = name;
        }
        public void RecallState()
        {
            // Used when the user cancels his modifications on formConfigureTrajectory.
            // styleHelper has been reverted already as part of style elements framework.
            // The minilabels should have been reverted through the main styleHelper value changed event.
            trackView = memoTrackView;
            name = memoLabel;
        }
        public PointF GetPosition(long timestamp)
        {
            int index = FindClosestPoint(timestamp);
            return positions[index].Point;
        }
        #endregion

        #region Miscellaneous private methods
        private void ReinitializeMenu()
        {
            InitializeMenuMeasurement();
        }
        private void InitializeMenuMeasurement()
        {
            mnuMeasurement.MergeIndex = 1;
            mnuMeasurement.Image = Properties.Drawings.measure;
            mnuMeasurement.Text = ScreenManagerLang.mnuShowMeasure;

            // TODO: unhook event handlers ?
            mnuMeasurement.DropDownItems.Clear();

            foreach (TrackExtraData trackExtraData in Enum.GetValues(typeof(TrackExtraData)))
                mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(trackExtraData));
        }
        private ToolStripMenuItem GetMeasurementMenu(TrackExtraData data)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Text = GetExtraDataOptionText(data);
            mnu.Checked = trackExtraData == data;

            mnu.Click += (s, e) =>
            {
                trackExtraData = data;
                displayBestFitCircle = IsUsingAngularKinematics();
                IntegrateKeyframes();
                InvalidateFromMenu(s);
            };

            return mnu;
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
            searchWindow.Rectangle = positions[currentPoint].Point.Box(tracker.Parameters.SearchWindow).ToRectangle();
            blockWindow.Rectangle = positions[currentPoint].Point.Box(tracker.Parameters.BlockWindow).ToRectangle();
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
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
            style.Bind(styleHelper, "TrackShape", "track shape");
        }

        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            AfterMainStyleChange();
        }

        private void AfterMainStyleChange()
        {
            // Impact the style of mini labels based on the main color.
            miniLabel.BackColor = styleHelper.Color;
            foreach (MiniLabel kfl in keyframesLabels)
                kfl.BackColor = styleHelper.Color;
        }
        #endregion
    }
}
