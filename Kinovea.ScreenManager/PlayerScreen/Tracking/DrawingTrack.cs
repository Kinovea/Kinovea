#region License
/*
Copyright © Joan Charmant 2008-2011.
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
    public class DrawingTrack : AbstractDrawing, IDecorable
    {
        #region Events
        public event EventHandler TrackerParametersChanged;
        #endregion

        #region Delegates
        // To ask the UI to display the frame closest to selected pos.
        // used when moving the target in direct interactive mode.
        public ClosestFrameAction ShowClosestFrame;     
        #endregion

        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.mnuTrackTrajectory; }
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
                hash ^= mainLabel.GetHashCode();
                
                foreach (KeyframeLabel kfl in keyframesLabels)
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
            set 
            { 
                styleHelper.Color = value;
                style.ReadValue();
                mainLabel.BackColor = value;
            }
        }
        public string Label
        {
            get { return mainLabelText; }
            set { mainLabelText = value;}
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
        public bool Untrackable
        {
            get { return untrackable; }
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
            get { return DrawingCapabilities.None; }
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
                contextMenu.Add(mnuAnalysis);
                return contextMenu;
            }
        }
        #endregion

        #region Members
        
        // Current state.
        private TrackView trackView = TrackView.Complete;
        private TrackStatus trackStatus = TrackStatus.Edit;
        private TrackExtraData trackExtraData = TrackExtraData.None;
        private TrackMarker trackMarker = TrackMarker.Cross;
        private bool displayBestFitCircle;
        private int movingHandler = -1;
        private bool invalid;                                 // Used for XML import.
            
        // Tracker tool.
        private AbstractTracker tracker;
        private bool untrackable;
        
        // Hardwired parameters.
        private const int defaultCrossRadius = 4;
        private const int allowedFramesOver = 12;      // Number of frames over which the global fading spans (after end point).
        private const int focusFadingFrames = 30;    // Number of frames of the focus section. 
       
        // Internal data.
        private List<AbstractTrackPoint> positions = new List<AbstractTrackPoint>();
        private TrajectoryKinematics trajectoryKinematics = new TrajectoryKinematics();
        private List<KeyframeLabel> keyframesLabels = new List<KeyframeLabel>();
        private KinematicsHelper kinematicsHelper = new KinematicsHelper();
        private IImageToViewportTransformer transformer;
        
        private long beginTimeStamp;                 // absolute.
        private long endTimeStamp = long.MaxValue;     // absolute.
        private int totalDistance;                   // This is used to normalize timestamps to a par scale with distances.
        private int currentPoint;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private KeyframeLabel mainLabel = new KeyframeLabel();
        private string mainLabelText = "Label";
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
        private ToolStripMenuItem mnuAnalysis = new ToolStripMenuItem();
        
        
        // Memorization poul
        private TrackView memoTrackView;
        private string memoLabel;
        private Metadata parentMetadata;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingTrack(Point origin, long t, Bitmap currentImage, Size imageSize)
        {
            //-----------------------------------------------------------------------------------------
            // t is absolute time.
            // _bmp is the whole picture, if null it means we don't need it.
            // (Probably because we already have a few points that we are importing from xml.
            // In this case we'll only need the last frame to reconstruct the last point.)
            //-----------------------------------------------------------------------------------------
            
            // Create the first point
            if (currentImage != null)
            {
                TrackerParameters parameters = GetTrackerParameters(currentImage.Size);
                tracker = new TrackerBlock2(parameters);
                AbstractTrackPoint atp = tracker.CreateTrackPoint(true, origin, 1.0f, t, currentImage, positions);
                if(atp != null)
                    positions.Add(atp);
                else
                    untrackable = true;
            }
            else
            {
                // Happens when loading Metadata from file or demuxing.
                TrackerParameters parameters = GetTrackerParameters(imageSize);
                tracker = new TrackerBlock2(parameters);
                positions.Add(tracker.CreateOrphanTrackPoint(origin, t));
            }

            if(!untrackable)
            {
                beginTimeStamp = t;
                endTimeStamp = t;
                mainLabel.SetAttach(origin, true);
                
                // We use the InfosFading utility to fade the track away.
                // The refererence frame will be the last point (at which fading start).
                // AverageTimeStampsPerFrame will be updated when we get the parent metadata.
                infosFading.FadingFrames = allowedFramesOver;
                infosFading.UseDefault = false;
                infosFading.Enabled = true;
            }
            
            // Decoration
            style = new DrawingStyle();
            style.Elements.Add("color", new StyleElementColor(Color.SeaGreen));
            style.Elements.Add("line size", new StyleElementLineSize(3));
            style.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));
            styleHelper.Color = Color.Black;
            styleHelper.LineSize = 3;
            styleHelper.TrackShape = TrackShape.Dash;
            BindStyle();
            
            styleHelper.ValueChanged += mainStyle_ValueChanged;
            ReinitializeMenu();

            mnuAnalysis.Click += (s, e) =>
            {
                // instanciate form.
                FormTimeseries fts = new FormTimeseries(trajectoryKinematics, parentMetadata.CalibrationHelper);
                FormsHelper.MakeTopmost(fts);
                fts.Show();
            };
        }

        
        public DrawingTrack(XmlReader xmlReader, PointF scale, TimeStampMapper remapTimestampCallback, Size imageSize)
            : this(Point.Empty,0, null, imageSize)
        {
            ReadXml(xmlReader, scale, remapTimestampCallback);
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
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
                if (trackStatus == TrackStatus.Interactive && trackView != TrackView.Label)
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
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
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
        private int HitTestEdit(Point point, long currentTimestamp, IImageToViewportTransformer transformer)
        {
            // 1: search window.
            Rectangle search = positions[currentPoint].Point.Box(tracker.Parameters.SearchWindow).ToRectangle();
            if (search.Contains(point))
                return 1;

            return -1;
        }
        private int HitTestConfiguration(Point point, long currentTimestamp, IImageToViewportTransformer transformer)
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
        private int HitTestInteractive(Point point, long currentTimestamp, IImageToViewportTransformer transformer)
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
        private int HitTestTrajectory(Point point, IImageToViewportTransformer transformer)
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
            
            foreach (KeyframeLabel kl in keyframesLabels)
            {
                // In focus mode, only show labels that are in focus section.
                if(trackView == TrackView.Complete || infosFading.IsVisible(positions[currentPoint].T, kl.Timestamp, focusFadingFrames))
                    kl.Draw(canvas, transformer, fadingFactor);
            }
        }
        private void DrawBestFitCircle(Graphics canvas, int currentPoint, double fadingFactor, IImageToViewportTransformer transformer)
        {
            if (positions.Count < 3)
                return;

            PointF rotationCenter = trajectoryKinematics.RotationCenter;
            double rotationRadius = trajectoryKinematics.RotationRadius;

            // trajectoryPoints values are expressed in user coordinates, so we need to first get them back to image coords, 
            // and then to convert them for display screen.
            Point location = transformer.Transform(positions[currentPoint].Point);
            PointF centerInImage = parentMetadata.CalibrationHelper.GetImagePoint(rotationCenter);
            Point center = transformer.Transform(centerInImage);

            // Get ellipse.
            Ellipse ellipseInImage = parentMetadata.CalibrationHelper.GetEllipseFromCircle(rotationCenter, (float)rotationRadius);

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
            
            mainLabel.SetAttach(positions[currentPoint].Point, true);
                
            string text = trackView == TrackView.Label ? mainLabelText : GetExtraDataText(currentPoint);
            mainLabel.SetText(text);
            mainLabel.Draw(canvas, transformer, fadingFactor);
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
                case TrackExtraData.Position: return "Position";
                case TrackExtraData.TotalDistance: return "Total distance";
                case TrackExtraData.Speed: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed;
                case TrackExtraData.VerticalVelocity: return "Vertical velocity";
                case TrackExtraData.HorizontalVelocity: return "Horizontal velocity";
                case TrackExtraData.Acceleration: return "Acceleration";
                case TrackExtraData.VerticalAcceleration: return "Vertical acceleration";
                case TrackExtraData.HorizontalAcceleration: return "Horizontal acceleration";
                case TrackExtraData.AngularDisplacement: return "Angular displacement";
                case TrackExtraData.AngularVelocity: return "Angular velocity";
                case TrackExtraData.AngularAcceleration: return "Angular acceleration";
                case TrackExtraData.CentripetalAcceleration: return "Centripetal acceleration";
            }

            return "";
        }

        public bool IsUsingAngularKinematics()
        {
            return trackExtraData == TrackExtraData.AngularDisplacement ||
                trackExtraData == TrackExtraData.AngularVelocity ||
                trackExtraData == TrackExtraData.AngularAcceleration ||
                trackExtraData == TrackExtraData.CentripetalAcceleration;
        }

        private string GetExtraDataText(int index)
        {
            CalibrationHelper helper = parentMetadata.CalibrationHelper;
            CultureInfo culture = CultureInfo.InvariantCulture;
            string displayText = "###";

            switch(trackExtraData)
            {
                case TrackExtraData.Position:
                    PointF p = trajectoryKinematics.RawCoordinates(index);
                    displayText = string.Format(culture, "{0:0.00} ; {1:0.00} {2}", p.X, p.Y, helper.GetLengthAbbreviation());
                    break;
                case TrackExtraData.TotalDistance:
                    float d = (float)trajectoryKinematics.TotalDistance[index];
                    displayText = string.Format(culture, "{0:0.00} {1}", d, helper.GetLengthAbbreviation());
                    break;
                case TrackExtraData.Speed:
                    float v = (float)trajectoryKinematics.Speed[index];
                    if (!float.IsNaN(v))
                        displayText = string.Format(culture, "{0:0.00} {1}", v, helper.GetSpeedAbbreviation());
                    break;
                case TrackExtraData.HorizontalVelocity:
                    float hv = (float)trajectoryKinematics.HorizontalVelocity[index];
                    if (!float.IsNaN(hv))
                        displayText = string.Format(culture, "{0:0.00} {1}", hv, helper.GetSpeedAbbreviation());
                    break;
                case TrackExtraData.VerticalVelocity:
                    float vv = (float)trajectoryKinematics.VerticalVelocity[index];
                    if (!float.IsNaN(vv))
                        displayText = string.Format(culture, "{0:0.00} {1}", vv, helper.GetSpeedAbbreviation());
                    break;
                case TrackExtraData.Acceleration:
                    float a = (float)trajectoryKinematics.Acceleration[index];
                    if (!float.IsNaN(a))
                        displayText = string.Format(culture, "{0:0.00} {1}", a, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.HorizontalAcceleration:
                    float ha = (float)trajectoryKinematics.HorizontalAcceleration[index];
                    if (!float.IsNaN(ha))
                        displayText = string.Format(culture, "{0:0.00} {1}", ha, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.VerticalAcceleration:
                    float va = (float)trajectoryKinematics.VerticalAcceleration[index];
                    if (!float.IsNaN(va))
                        displayText = string.Format(culture, "{0:0.00} {1}", va, helper.GetAccelerationAbbreviation());
                    break;
                case TrackExtraData.AngularDisplacement:
                    float angle = (float)trajectoryKinematics.DisplacementAngle[index];
                    displayText = string.Format(culture, "{0:0.00} {1}", angle, helper.GetAngleAbbreviation());
                    break;
                case TrackExtraData.AngularVelocity:
                    float angularVelocity = (float)trajectoryKinematics.AngularVelocity[index];
                    if (!float.IsNaN(angularVelocity)) 
                        displayText = string.Format(culture, "{0:0.00} {1}", angularVelocity, helper.GetAngularVelocityAbbreviation());
                    break;
                case TrackExtraData.AngularAcceleration:
                    float angularAcceleration = (float)trajectoryKinematics.AngularAcceleration[index];
                    if (!float.IsNaN(angularAcceleration))
                        displayText = string.Format(culture, "{0:0.00} {1}", angularAcceleration, helper.GetAngularAccelerationAbbreviation());
                    break;
                case TrackExtraData.CentripetalAcceleration:
                    float centripetalAcceleration = (float)trajectoryKinematics.CentripetalAcceleration[index];
                    if (!float.IsNaN(centripetalAcceleration))
                        displayText = string.Format(culture, "{0:0.00} {1}", centripetalAcceleration, helper.GetAngularAccelerationAbbreviation());
                    break;
                case TrackExtraData.None:
                default:
                    break;
            }    
            return displayText;

            //float tangentialVelocity = trajectoryPoints[index].TangentialVelocity;
            //if (!float.IsNaN(tangentialVelocity))
            //    displayText = string.Format(culture, "{0:0.00} {1}", tangentialVelocity, helper.GetSpeedAbbreviation());

            //float totalArc = trajectoryPoints[index].TotalDisplacementAngle * trajectoryPoints[index].RotationRadius;
            //displayText = string.Format(culture, "{0:0.00} {1}", totalArc, helper.GetLengthAbbreviation());
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
                if (ShowClosestFrame != null && positions.Count > 1)
                    ShowClosestFrame(new Point((int)dx, (int)dy), positions, totalDistance, false);
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
                    mainLabel.MoveLabel(dx, dy);
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
                mainLabel.MoveLabel(dx, dy);
            }
        }
        private int IsOnKeyframesLabels(Point point, IImageToViewportTransformer transformer)
        {
            // Convention: -1 = miss, 2 = on main label, 3+ = on keyframe label.
            int hitResult = -1;
            if (trackView == TrackView.Label)
            {
                if (mainLabel.HitTest(point, transformer))
                    hitResult = 2;
            }
            else
            {
                // Even when we aren't in TrackView.Label, the main label is visible
                // if we are displaying the extra data (distance, speed).
                if (trackExtraData != TrackExtraData.None)
                {
                    if (mainLabel.HitTest(point, transformer))
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
            // Todo: we must now refill the last point with a patch image.

            UpdateKinematics();
            IntegrateKeyframes();
        }
        public List<AbstractTrackPoint> GetEndOfTrack(long timestamp)
        {
            // Called from CommandDeleteEndOfTrack,
            // We need to keep the old values in case the command is undone.
            List<AbstractTrackPoint> endOfTrack = positions.SkipWhile(p => p.T >= timestamp).ToList();
            return endOfTrack;
        }
        public void AppendPoints(long currentTimestamp, List<AbstractTrackPoint> choppedPoints)
        {
            // Called when undoing CommandDeleteEndOfTrack,
            // revival of the discarded points.
            if (choppedPoints.Count == 0)
                return;
            
            // Some points may have been re added already and we don't want to mix the two lists.
            // Find the append insertion point, remove extra stuff, and append.
            int iMatchedPoint = positions.Count - 1;
                
            while (positions[iMatchedPoint].T >= choppedPoints[0].T && iMatchedPoint > 0)
                iMatchedPoint--;

            if (iMatchedPoint < positions.Count - 1)
                positions.RemoveRange(iMatchedPoint + 1, positions.Count - (iMatchedPoint+1));

            foreach (AbstractTrackPoint trkpos in choppedPoints)
                positions.Add(trkpos);

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
            // the user can only moves existing points.
            
            if (current.Timestamp <= positions.Last().T)
                return;
            
            AbstractTrackPoint p = null;
            bool bMatched = tracker.Track(positions, current.Image, current.Timestamp, out p);
                
            if(p==null)
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
            if (positions.Count < 1 || currentPoint < 0)
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
            double similarityThreshold = 0.5;
            double templateUpdateThreshold = 0.8;
            //double templateUpdateThreshold = 1.0;
            int refinementNeighborhood = 1;
            Size searchWindow = new Size((int)(size.Width * 0.2), (int)(size.Height * 0.2));
            Size blockWindow = new Size((int)(size.Width * 0.05), (int)(size.Height * 0.05));

            return new TrackerParameters(similarityThreshold, templateUpdateThreshold, refinementNeighborhood, searchWindow, blockWindow, false);
            //return new TrackerParameters(new TrackingProfile(), size);
        }
        #endregion
        
        #region XML import/export
        public void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("TimePosition", beginTimeStamp.ToString());
            
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackView));
            string xmlMode = enumConverter.ConvertToString(trackView);
            xmlWriter.WriteElementString("Mode", xmlMode);
            
            enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
            string xmlExtraData = enumConverter.ConvertToString(trackExtraData);
            xmlWriter.WriteElementString("ExtraData", xmlExtraData);

            enumConverter = TypeDescriptor.GetConverter(typeof(TrackMarker));
            string xmlTrackMarker = enumConverter.ConvertToString(trackMarker);
            xmlWriter.WriteElementString("Marker", xmlTrackMarker);

            xmlWriter.WriteElementString("DisplayBestFitCircle", displayBestFitCircle.ToString().ToLower());

            xmlWriter.WriteStartElement("TrackerParameters");
            tracker.Parameters.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement();

            TrackPointsToXml(xmlWriter);
            
            xmlWriter.WriteStartElement("DrawingStyle");
            style.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement();
            
            xmlWriter.WriteStartElement("MainLabel");
            xmlWriter.WriteAttributeString("Text", mainLabelText);
            
            // Reset to first point.
            if (positions.Count > 0)
                mainLabel.SetAttach(positions[0].Point, true);
            mainLabel.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement();

            if (positions.Count > 0)
                mainLabel.SetAttach(positions[currentPoint].Point, true);
            
            if (keyframesLabels.Count > 0)
            {
                xmlWriter.WriteStartElement("KeyframeLabelList");
                xmlWriter.WriteAttributeString("Count", keyframesLabels.Count.ToString());

                foreach (KeyframeLabel kfl in keyframesLabels)
                {
                    xmlWriter.WriteStartElement("KeyframeLabel");
                    kfl.WriteXml(xmlWriter);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }
        private void TrackPointsToXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("TrackPointList");
            xmlWriter.WriteAttributeString("Count", positions.Count.ToString());
            xmlWriter.WriteAttributeString("UserUnitLength", parentMetadata.CalibrationHelper.GetLengthAbbreviation());
            
            if(positions.Count > 0)
            {
                foreach (AbstractTrackPoint tp in positions)
                {
                    xmlWriter.WriteStartElement("TrackPoint");
                    
                    PointF p = parentMetadata.CalibrationHelper.GetPoint(tp.Point);
                    string userT = parentMetadata.TimeStampsToTimecode(tp.T, TimeType.Time, TimecodeFormat.Unknown, false);
                    
                    xmlWriter.WriteAttributeString("UserX", String.Format("{0:0.00}", p.X));
                    xmlWriter.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.X));
                    xmlWriter.WriteAttributeString("UserY", String.Format("{0:0.00}", p.Y));
                    xmlWriter.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", p.Y));
                    xmlWriter.WriteAttributeString("UserTime", userT);
            
                    tp.WriteXml(xmlWriter);
                    
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }
        public void ReadXml(XmlReader xmlReader, PointF scale, TimeStampMapper remapTimestampCallback)
        {
            invalid = true;
                
            if (remapTimestampCallback == null)
            {
                string unparsed = xmlReader.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                return;
            }
            
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "TimePosition":
                        beginTimeStamp = remapTimestampCallback(xmlReader.ReadElementContentAsLong(), false);
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
                        tracker.Parameters = TrackerParameters.ReadXml(xmlReader);
                        break;
                    case "TrackPointList":
                        ParseTrackPointList(xmlReader, scale, remapTimestampCallback);
                        break;
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
                        break;
                    case "MainLabel":
                        {
                            mainLabelText = xmlReader.GetAttribute("Text");
                            mainLabel = new KeyframeLabel(xmlReader, scale);
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
            
            if (positions.Count > 0)
            {
                endTimeStamp = positions.Last().T;
                mainLabel.SetAttach(positions[0].Point, false);
                mainLabel.SetText(Label);
                
                if(positions.Count > 1 || 
                   positions[0].X != 0 || 
                   positions[0].Y != 0 || 
                   positions[0].T != 0)
                {
                    invalid = false;
                }
            }
        }
        public void ParseTrackPointList(XmlReader xmlReader, PointF scale, TimeStampMapper remapTimestampCallback)
        {
            positions.Clear();
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                if(xmlReader.Name == "TrackPoint")
                {
                    AbstractTrackPoint tp = tracker.CreateOrphanTrackPoint(PointF.Empty, 0);
                    tp.ReadXml(xmlReader);
                    
                    // time was stored in relative value, we still need to adjust it.
                    AbstractTrackPoint adapted = tracker.CreateOrphanTrackPoint(tp.Point.Scale(scale.X, scale.Y), remapTimestampCallback(tp.T, true));

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
                    KeyframeLabel kfl = new KeyframeLabel(xmlReader, scale);
                    
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
        
        #region Miscellaneous public methods
        public void CalibrationChanged()
        {
            UpdateKinematics();
            IntegrateKeyframes();
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
                        KeyframeLabel kfl = new KeyframeLabel();
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
            
        }
        public void MemorizeState()
        {
            // Used by formConfigureTrajectory to be able to modify the trajectory in real time.
            memoTrackView = trackView;
            memoLabel = mainLabelText;
        }
        public void RecallState()
        {
            // Used when the user cancels his modifications on formConfigureTrajectory.
            // m_StyleHelper has been reverted already as part of style elements framework.
            // This in turn triggered mainStyle_ValueChanged() event handler so the m_MainLabel has been reverted already too.
            trackView = memoTrackView;
            mainLabelText = memoLabel;
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

            mnuAnalysis.MergeIndex = 2;
            mnuAnalysis.Image = Properties.Resources.function;
            mnuAnalysis.Text = "Data analysis";
        }
        private void InitializeMenuMeasurement()
        {
            mnuMeasurement.MergeIndex = 1;
            mnuMeasurement.Image = Properties.Drawings.measure;
            mnuMeasurement.Text = ScreenManagerLang.mnuShowMeasure;

            // TODO: unhook event handlers ?
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.None));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Position));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.TotalDistance));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Speed));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.VerticalVelocity));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.HorizontalVelocity));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Acceleration));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.VerticalAcceleration));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.HorizontalAcceleration));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.AngularDisplacement));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.AngularVelocity));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.AngularAcceleration));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.CentripetalAcceleration));
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
                Action invalidateImage = (Action)mnu.Tag;
                invalidateImage();
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
        private void UpdateKinematics()
        {
            List<TimedPoint> samples = positions.Select(p => new TimedPoint(p.X, p.Y, p.T)).ToList();

            // Testing smoothing: import test data, fs is 69.9Hz.
            /*List<TimedPoint> samples = new List<TimedPoint>();
            int s = 0;
            foreach (double d in TestData.Data)
            {
                samples.Add(new TimedPoint(0, (float)d, s));
                s++;
            }*/

            trajectoryKinematics = kinematicsHelper.AnalyzeTrajectory(samples, parentMetadata.CalibrationHelper);
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
        private void mainStyle_ValueChanged(object sender, EventArgs e)
        {
            mainLabel.BackColor = styleHelper.Color;
        }
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
            style.Bind(styleHelper, "TrackShape", "track shape");
        }
        #endregion
    }
}
