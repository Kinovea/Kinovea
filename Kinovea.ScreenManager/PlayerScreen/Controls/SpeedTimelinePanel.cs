using System;
using System.Drawing;
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
    /// Panel showing the speed of a single track over time, with a vertical cursor following the playhead.
    /// The data is computed once when a track is assigned and again when the user clicks "Refresh".
    /// Only the cursor is updated during playback.
    /// The controls are created in code to avoid touching the designer file of the player screen.
    /// </summary>
    public class SpeedTimelinePanel : UserControl
    {
        public event EventHandler CloseAsked;

        /// <summary>
        /// The track currently displayed, or null.
        /// </summary>
        public DrawingTrack Track
        {
            get { return track; }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PlotView plotView = new PlotView();
        private Label lblTitle = new Label();
        private Button btnRefresh = new Button();
        private Button btnClose = new Button();
        private LineAnnotation cursor;
        private SpeedTimeline timeline = SpeedTimeline.Empty();
        private DrawingTrack track;
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
            btnRefresh.Text = "Refresh";
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

            this.Controls.Add(plotView);
            this.Controls.Add(header);
        }

        /// <summary>
        /// Show the speed of the passed track.
        /// </summary>
        public void SetTrack(DrawingTrack track, Metadata metadata)
        {
            this.track = track;
            this.metadata = metadata;
            RefreshData();
        }

        /// <summary>
        /// Forget the current track and clear the plot.
        /// </summary>
        public void Clear()
        {
            track = null;
            metadata = null;
            timeline = SpeedTimeline.Empty();
            cursor = null;
            lblTitle.Text = "";
            plotView.Model = null;
        }

        /// <summary>
        /// Returns true if the displayed track is still part of the metadata.
        /// </summary>
        public bool IsTrackAlive()
        {
            return track != null && metadata != null && metadata.Tracks().Contains(track);
        }

        /// <summary>
        /// Recompute the kinematics of the track and rebuild the plot.
        /// </summary>
        public void RefreshData()
        {
            if (!IsTrackAlive())
            {
                Clear();
                return;
            }

            track.UpdateKinematics();
            timeline = BuildTimeline(track, metadata);

            string abbreviation = metadata.CalibrationHelper.GetSpeedAbbreviation();
            lblTitle.Text = string.Format("{0} - {1} ({2})", track.Name, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed, abbreviation);
            plotView.Model = CreatePlot(abbreviation);
        }

        /// <summary>
        /// Move the cursor to the passed video timestamp.
        /// Called on the UI thread every time the current frame changes.
        /// </summary>
        public void UpdateCursor(long timestamp)
        {
            if (cursor == null || timeline.IsEmpty)
                return;

            double x = timeline.TimestampToSeconds(timestamp);
            if (x == cursor.X)
                return;

            cursor.X = x;
            plotView.InvalidatePlot(false);
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

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.Title = "Time (s)";
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

            LineSeries series = new LineSeries();
            Color c = track.MainColor;
            series.Color = OxyColor.FromArgb(255, c.R, c.G, c.B);
            series.StrokeThickness = 1.5;
            series.MarkerType = MarkerType.None;
            for (int i = 0; i < timeline.Count; i++)
                series.Points.Add(new DataPoint(timeline.Times[i], timeline.Values[i]));

            model.Series.Add(series);

            cursor = new LineAnnotation();
            cursor.Type = LineAnnotationType.Vertical;
            cursor.Color = OxyColors.Red;
            cursor.LineStyle = OxyPlot.LineStyle.Solid;
            cursor.StrokeThickness = 1.5;
            cursor.X = timeline.IsEmpty ? 0 : timeline.Times[0];
            model.Annotations.Add(cursor);

            return model;
        }
    }
}
