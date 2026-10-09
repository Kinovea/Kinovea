using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Panel showing the speed of one or more tracks over time, with a vertical cursor following the playhead.
    /// Each track is drawn in its own color, a legend is shown when there is more than one.
    /// The data is computed when a track is added and again for all tracks when the user clicks "Refresh".
    /// Only the cursor is updated during playback.
    /// Clicking or dragging with the left button in the plot area asks the player to seek to that time.
    /// The controls are created in code to avoid touching the designer file of the player screen.
    /// </summary>
    public class SpeedTimelinePanel : UserControl
    {
        public event EventHandler CloseAsked;

        /// <summary>
        /// Raised when the user clicks or drags in the plot to move the playhead.
        /// The time is a video timestamp within the range covered by the tracks.
        /// </summary>
        public event EventHandler<TimeEventArgs> SeekAsked;

        /// <summary>
        /// Number of tracks currently displayed.
        /// </summary>
        public int TrackCount
        {
            get { return tracks.Count; }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PlotView plotView = new PlotView();
        private Label lblTitle = new Label();
        private Button btnRefresh = new Button();
        private Button btnClose = new Button();
        private LineAnnotation cursor;
        private LinearAxis xAxis;
        private long lastSeekTimestamp = -1;
        private long cursorTimestamp = -1;
        private List<DrawingTrack> tracks = new List<DrawingTrack>();
        private List<SpeedTimeline> timelines = new List<SpeedTimeline>();
        private Metadata metadata;

        public SpeedTimelinePanel()
        {
            this.BackColor = Color.White;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 26;

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.AutoEllipsis = true;

            btnRefresh.Dock = DockStyle.Right;
            btnRefresh.Width = 90;
            btnRefresh.Text = ScreenManagerLang.SpeedGraph_Refresh;
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += (s, e) => RefreshData();

            btnClose.Dock = DockStyle.Right;
            btnClose.Width = 30;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += (s, e) => CloseAsked?.Invoke(this, EventArgs.Empty);

            // Docking is resolved in reverse order of addition: the Fill control must be added first.
            header.Controls.Add(lblTitle);
            header.Controls.Add(btnRefresh);
            header.Controls.Add(btnClose);

            plotView.Dock = DockStyle.Fill;
            plotView.BackColor = Color.White;
            plotView.Controller = CreateController();

            this.Controls.Add(plotView);
            this.Controls.Add(header);
        }

        /// <summary>
        /// Returns true if the passed track is displayed in the plot.
        /// </summary>
        public bool Contains(DrawingTrack track)
        {
            return tracks.Contains(track);
        }

        /// <summary>
        /// Add a track to the plot. Does nothing if it is already displayed.
        /// </summary>
        public void AddTrack(DrawingTrack track, Metadata metadata)
        {
            if (track == null || tracks.Contains(track))
                return;

            // All the tracks must come from the same video so they share the same time axis.
            if (this.metadata != metadata)
                tracks.Clear();

            this.metadata = metadata;
            tracks.Add(track);
            RefreshData();
        }

        /// <summary>
        /// Remove a track from the plot.
        /// </summary>
        public void RemoveTrack(DrawingTrack track)
        {
            if (!tracks.Remove(track))
                return;

            RefreshData();
        }

        /// <summary>
        /// Remove the tracks that are no longer part of the metadata, for example after they were deleted.
        /// Returns true if at least one track was removed.
        /// </summary>
        public bool RemoveDeadTracks()
        {
            int removed = tracks.RemoveAll(t => !IsTrackAlive(t));
            if (removed == 0)
                return false;

            RefreshData();
            return true;
        }

        /// <summary>
        /// Forget all tracks and clear the plot.
        /// </summary>
        public void Clear()
        {
            tracks.Clear();
            timelines.Clear();
            metadata = null;
            cursor = null;
            cursorTimestamp = -1;
            xAxis = null;
            lblTitle.Text = "";
            plotView.Model = null;
        }

        /// <summary>
        /// Recompute the kinematics of all the tracks and rebuild the plot.
        /// </summary>
        public void RefreshData()
        {
            tracks.RemoveAll(t => !IsTrackAlive(t));
            if (tracks.Count == 0)
            {
                Clear();
                return;
            }

            timelines.Clear();
            foreach (DrawingTrack track in tracks)
            {
                track.UpdateKinematics();
                timelines.Add(BuildTimeline(track, metadata));
            }

            string abbreviation = metadata.CalibrationHelper.GetSpeedAbbreviation();
            UpdateTitle(abbreviation);
            plotView.Model = CreatePlot(abbreviation);
        }

        /// <summary>
        /// Update the texts after the user changed the interface language.
        /// The data is not recomputed.
        /// </summary>
        public void ReloadCulture()
        {
            btnRefresh.Text = ScreenManagerLang.SpeedGraph_Refresh;
            if (tracks.Count == 0 || xAxis == null)
                return;

            UpdateTitle(metadata.CalibrationHelper.GetSpeedAbbreviation());
            xAxis.Title = ScreenManagerLang.DataAnalysis_TimeAxisSeconds;
            plotView.InvalidatePlot(false);
        }

        private void UpdateTitle(string abbreviation)
        {
            string names = string.Join(", ", tracks.Select(t => t.Name));
            lblTitle.Text = string.Format("{0} - {1} ({2})", names, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed, abbreviation);
        }

        /// <summary>
        /// Move the cursor to the passed video timestamp.
        /// Called on the UI thread every time the current frame changes.
        /// </summary>
        public void UpdateCursor(long timestamp)
        {
            cursorTimestamp = timestamp;
            SpeedTimeline reference = ReferenceTimeline();
            if (cursor == null || reference == null)
                return;

            double x = reference.TimestampToSeconds(timestamp);
            if (x == cursor.X)
                return;

            cursor.X = x;
            plotView.InvalidatePlot(false);
        }

        /// <summary>
        /// Same interactions as the default OxyPlot controller (pan, zoom, Ctrl/Shift tracker),
        /// except plain left click which seeks the video instead of showing the tracker.
        /// </summary>
        private PlotController CreateController()
        {
            PlotController controller = new PlotController();
            controller.UnbindMouseDown(OxyMouseButton.Left);
            controller.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, c, args) =>
            {
                if (!IsInPlotArea(view, args.Position))
                    return;

                c.AddMouseManipulator(view, new SeekManipulator(view, this), args);
            }));

            return controller;
        }

        private static bool IsInPlotArea(IPlotView view, ScreenPoint p)
        {
            if (view.ActualModel == null)
                return false;

            OxyRect area = view.ActualModel.PlotArea;
            return p.X >= area.Left && p.X <= area.Right && p.Y >= area.Top && p.Y <= area.Bottom;
        }

        /// <summary>
        /// Convert a horizontal screen position in the plot to a video timestamp and ask the player to go there.
        /// </summary>
        private void SeekToScreenX(double x)
        {
            if (xAxis == null || timelines.Count == 0)
                return;

            long timestamp = SpeedTimeline.SecondsToTimestamp(timelines, xAxis.InverseTransform(x));
            if (timestamp < 0 || timestamp == lastSeekTimestamp)
                return;

            lastSeekTimestamp = timestamp;

            // Move the cursor right away for immediate feedback, the player will confirm the actual frame time.
            UpdateCursor(timestamp);
            SeekAsked?.Invoke(this, new TimeEventArgs(timestamp));
        }

        /// <summary>
        /// Seeks on mouse down and keeps seeking while the mouse is dragged, like scrubbing the main timeline.
        /// </summary>
        private class SeekManipulator : MouseManipulator
        {
            private readonly SpeedTimelinePanel owner;

            public SeekManipulator(IPlotView view, SpeedTimelinePanel owner)
                : base(view)
            {
                this.owner = owner;
            }

            public override void Started(OxyMouseEventArgs e)
            {
                base.Started(e);
                owner.lastSeekTimestamp = -1;
                owner.SeekToScreenX(e.Position.X);
                e.Handled = true;
            }

            public override void Delta(OxyMouseEventArgs e)
            {
                base.Delta(e);
                owner.SeekToScreenX(e.Position.X);
                e.Handled = true;
            }
        }

        /// <summary>
        /// All non empty timelines share the same time axis, any of them can convert the playhead time.
        /// Empty timelines are skipped as they are not built with the time scale of the video.
        /// </summary>
        private SpeedTimeline ReferenceTimeline()
        {
            return timelines.FirstOrDefault(t => !t.IsEmpty);
        }

        private bool IsTrackAlive(DrawingTrack track)
        {
            return metadata != null && metadata.Tracks().Contains(track);
        }

        private static SpeedTimeline BuildTimeline(DrawingTrack track, Metadata metadata)
        {
            TimeSeriesCollection tsc = track.TimeSeriesCollection;
            if (tsc == null || tsc.Length == 0)
                return SpeedTimeline.Empty();

            try
            {
                return SpeedTimeline.Build(
                    tsc.Times,
                    tsc[Kinematics.LinearSpeed],
                    metadata.TimeOrigin,
                    metadata.AverageTimeStampsPerSecond,
                    metadata.HighSpeedFactor);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Could not build speed timeline for {0}: {1}", track.Name, e.Message);
                return SpeedTimeline.Empty();
            }
        }

        private PlotModel CreatePlot(string abbreviation)
        {
            PlotModel model = new PlotModel();
            model.PlotType = PlotType.XY;

            xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.Title = ScreenManagerLang.DataAnalysis_TimeAxisSeconds;
            xAxis.MajorGridlineStyle = OxyPlot.LineStyle.Solid;
            xAxis.MinorGridlineStyle = OxyPlot.LineStyle.Dot;
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.Title = abbreviation;
            yAxis.MajorGridlineStyle = OxyPlot.LineStyle.Solid;
            yAxis.MinorGridlineStyle = OxyPlot.LineStyle.Dot;
            yAxis.MinimumPadding = 0.05;
            yAxis.MaximumPadding = 0.1;
            model.Axes.Add(yAxis);

            for (int i = 0; i < tracks.Count; i++)
                model.Series.Add(CreateSeries(tracks[i], timelines[i]));

            if (tracks.Count > 1)
            {
                model.IsLegendVisible = true;
                model.LegendPlacement = LegendPlacement.Inside;
                model.LegendPosition = LegendPosition.TopRight;
                model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
                model.LegendBorder = OxyColors.Gray;
            }
            else
            {
                model.IsLegendVisible = false;
            }

            cursor = new LineAnnotation();
            cursor.Type = LineAnnotationType.Vertical;
            cursor.Color = OxyColors.Red;
            cursor.LineStyle = OxyPlot.LineStyle.Solid;
            cursor.StrokeThickness = 1.5;
            SpeedTimeline reference = ReferenceTimeline();
            cursor.X = (reference != null && cursorTimestamp >= 0) ? reference.TimestampToSeconds(cursorTimestamp) : 0;
            model.Annotations.Add(cursor);

            return model;
        }

        private static LineSeries CreateSeries(DrawingTrack track, SpeedTimeline timeline)
        {
            LineSeries series = new LineSeries();
            series.Title = track.Name;
            Color c = track.MainColor;
            series.Color = OxyColor.FromArgb(255, c.R, c.G, c.B);
            series.StrokeThickness = 1.5;
            series.MarkerType = MarkerType.None;
            for (int i = 0; i < timeline.Count; i++)
                series.Points.Add(new DataPoint(timeline.Times[i], timeline.Values[i]));

            return series;
        }
    }
}
