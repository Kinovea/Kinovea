using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Panel showing the speed of one or more tracks over time, with a vertical cursor following the playhead.
    /// Each track is drawn in its own color, a legend is shown when there is more than one.
    /// The data is computed when a track is added and again for all tracks when the user clicks "Refresh".
    /// Only the cursor and the value markers on the curves are updated during playback.
    /// Optional statistics per track: mean line, peak and low markers, values in the legend.
    /// The graph can be exported as an image and the data as CSV, with the helpers shared with the analysis dialogs.
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
        private CheckBox chkStatistics = new CheckBox();
        private Button btnExport = new Button();
        private Button btnRefresh = new Button();
        private Button btnClose = new Button();
        private ContextMenuStrip exportMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuExportGraph = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportGraphCopy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportGraphSave = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportData = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportDataCopy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportDataSave = new ToolStripMenuItem();
        private LineAnnotation cursor;
        private List<PointAnnotation> valueMarkers = new List<PointAnnotation>();
        private string speedAbbreviation = "";
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

            chkStatistics.Dock = DockStyle.Right;
            chkStatistics.Width = 100;
            chkStatistics.Checked = true;
            chkStatistics.CheckedChanged += (s, e) => RebuildPlot();

            btnExport.Dock = DockStyle.Right;
            btnExport.Width = 90;
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += (s, e) => exportMenu.Show(btnExport, new Point(0, btnExport.Height));

            mnuExportGraphCopy.Click += (s, e) => CopyGraph();
            mnuExportGraphSave.Click += (s, e) => SaveGraph();
            mnuExportDataCopy.Click += (s, e) => CopyData();
            mnuExportDataSave.Click += (s, e) => SaveData();
            mnuExportGraph.DropDownItems.AddRange(new ToolStripItem[] { mnuExportGraphCopy, mnuExportGraphSave });
            mnuExportData.DropDownItems.AddRange(new ToolStripItem[] { mnuExportDataCopy, mnuExportDataSave });
            exportMenu.Items.AddRange(new ToolStripItem[] { mnuExportGraph, mnuExportData });

            btnRefresh.Dock = DockStyle.Right;
            btnRefresh.Width = 90;
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += (s, e) => RefreshData();

            btnClose.Dock = DockStyle.Right;
            btnClose.Width = 30;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += (s, e) => CloseAsked?.Invoke(this, EventArgs.Empty);

            // Docking is resolved in reverse order of addition: the Fill control must be added first.
            header.Controls.Add(lblTitle);
            header.Controls.Add(chkStatistics);
            header.Controls.Add(btnExport);
            header.Controls.Add(btnRefresh);
            header.Controls.Add(btnClose);

            plotView.Dock = DockStyle.Fill;
            plotView.BackColor = Color.White;
            plotView.Controller = CreateController();

            this.Controls.Add(plotView);
            this.Controls.Add(header);

            ReloadCulture();
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
            valueMarkers.Clear();
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

            speedAbbreviation = metadata.CalibrationHelper.GetSpeedAbbreviation();
            UpdateTitle(speedAbbreviation);
            plotView.Model = CreatePlot(speedAbbreviation);
        }

        /// <summary>
        /// Rebuild the plot from the current data, without recomputing the kinematics.
        /// </summary>
        private void RebuildPlot()
        {
            if (tracks.Count == 0 || timelines.Count != tracks.Count)
                return;

            plotView.Model = CreatePlot(speedAbbreviation);
        }

        /// <summary>
        /// Update the texts after the user changed the interface language.
        /// The data is not recomputed.
        /// </summary>
        public void ReloadCulture()
        {
            btnRefresh.Text = ScreenManagerLang.SpeedGraph_Refresh;
            btnExport.Text = ScreenManagerLang.SpeedGraph_Export;
            chkStatistics.Text = ScreenManagerLang.SpeedGraph_Statistics;
            mnuExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            mnuExportGraphCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            mnuExportGraphSave.Text = ScreenManagerLang.DataAnalysis_SaveToFile;
            mnuExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            mnuExportDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            mnuExportDataSave.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

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
            UpdateValueMarkers(plotView.Model, x);
            plotView.InvalidatePlot(false);
        }

        /// <summary>
        /// Move the marker of each curve to the passed time and show the value there.
        /// A marker is removed from the plot while the time is outside the range of its track.
        /// </summary>
        private void UpdateValueMarkers(PlotModel model, double x)
        {
            if (model == null)
                return;

            for (int i = 0; i < valueMarkers.Count && i < timelines.Count; i++)
            {
                PointAnnotation marker = valueMarkers[i];
                double value = timelines[i].ValueAt(x);
                bool shown = model.Annotations.Contains(marker);

                if (double.IsNaN(value))
                {
                    if (shown)
                        model.Annotations.Remove(marker);

                    continue;
                }

                marker.X = x;
                marker.Y = value;
                marker.Text = FormatValue(value);
                if (!shown)
                    model.Annotations.Add(marker);
            }
        }

        /// <summary>
        /// Same mouse interactions as the default OxyPlot controller (pan, zoom, Ctrl/Shift tracker),
        /// except plain left click which seeks the video instead of showing the tracker.
        /// The tracker (time and value under the mouse) is shown on hover instead.
        /// All keyboard bindings are removed: once the plot has focus, keys like the arrows must only
        /// drive the player (frame stepping) and not pan or zoom the plot at the same time.
        /// </summary>
        private PlotController CreateController()
        {
            PlotController controller = new PlotController();
            UnbindKeyboard(controller);
            controller.BindMouseEnter(PlotCommands.HoverSnapTrack);
            controller.UnbindMouseDown(OxyMouseButton.Left);
            controller.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>((view, c, args) =>
            {
                if (!IsInPlotArea(view, args.Position))
                    return;

                c.AddMouseManipulator(view, new SeekManipulator(view, this), args);
            }));

            return controller;
        }

        private static void UnbindKeyboard(PlotController controller)
        {
            // Keys bound by the default OxyPlot 1.0 controller.
            OxyKey[] keys = { OxyKey.Left, OxyKey.Right, OxyKey.Up, OxyKey.Down, OxyKey.Add, OxyKey.Subtract, OxyKey.PageUp, OxyKey.PageDown };
            foreach (OxyKey key in keys)
            {
                controller.UnbindKeyDown(key);
                controller.UnbindKeyDown(key, OxyModifierKeys.Control);
            }

            controller.UnbindKeyDown(OxyKey.A);
            controller.UnbindKeyDown(OxyKey.Home);
            controller.UnbindKeyDown(OxyKey.C, OxyModifierKeys.Control | OxyModifierKeys.Alt);
            controller.UnbindKeyDown(OxyKey.R, OxyModifierKeys.Control | OxyModifierKeys.Alt);
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

            bool statistics = chkStatistics.Checked;
            for (int i = 0; i < tracks.Count; i++)
            {
                model.Series.Add(CreateSeries(tracks[i], timelines[i], abbreviation, statistics));
                if (statistics)
                    AddStatistics(model, tracks[i], timelines[i]);
            }

            if (tracks.Count > 1 || statistics)
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

            // The value markers are added to the model by UpdateValueMarkers when the cursor is within their track.
            valueMarkers.Clear();
            foreach (DrawingTrack track in tracks)
            {
                PointAnnotation marker = new PointAnnotation();
                marker.Shape = MarkerType.Circle;
                marker.Size = 4;
                marker.Fill = ToOxyColor(track.MainColor);
                marker.Stroke = OxyColors.White;
                marker.StrokeThickness = 1;
                marker.FontWeight = FontWeights.Bold;
                valueMarkers.Add(marker);
            }

            UpdateValueMarkers(model, cursor.X);

            return model;
        }

        /// <summary>
        /// Mean as a dashed horizontal line, peak and low as labelled markers, all in the color of the track.
        /// </summary>
        private void AddStatistics(PlotModel model, DrawingTrack track, SpeedTimeline timeline)
        {
            if (timeline.IsEmpty)
                return;

            OxyColor color = ToOxyColor(track.MainColor);

            LineAnnotation mean = new LineAnnotation();
            mean.Type = LineAnnotationType.Horizontal;
            mean.Y = timeline.Mean;
            mean.Color = color;
            mean.LineStyle = OxyPlot.LineStyle.Dash;
            mean.StrokeThickness = 1;
            mean.Text = "\u00D8 " + FormatValue(timeline.Mean);
            mean.TextColor = color;
            model.Annotations.Add(mean);

            model.Annotations.Add(CreateExtremumMarker(color, MarkerType.Triangle, timeline.MaximumTime, timeline.Maximum, "max"));
            model.Annotations.Add(CreateExtremumMarker(color, MarkerType.Diamond, timeline.MinimumTime, timeline.Minimum, "min"));
        }

        private static PointAnnotation CreateExtremumMarker(OxyColor color, MarkerType shape, double x, double y, string label)
        {
            PointAnnotation marker = new PointAnnotation();
            marker.X = x;
            marker.Y = y;
            marker.Shape = shape;
            marker.Size = 5;
            marker.Fill = color;
            marker.Text = label + " " + FormatValue(y);
            marker.TextColor = color;
            return marker;
        }

        private static LineSeries CreateSeries(DrawingTrack track, SpeedTimeline timeline, string abbreviation, bool statistics)
        {
            LineSeries series = new LineSeries();
            series.Title = track.Name;
            if (statistics && !timeline.IsEmpty)
            {
                series.Title = string.Format("{0}   \u00D8 {1}   max {2}   min {3}",
                    track.Name, FormatValue(timeline.Mean), FormatValue(timeline.Maximum), FormatValue(timeline.Minimum));
            }

            // The tracker shows the track name, not the title with the statistics.
            series.TrackerFormatString = EscapeFormat(track.Name) + "\n{2:0.000} s\n{4:0.00} " + EscapeFormat(abbreviation);
            series.Color = ToOxyColor(track.MainColor);
            series.StrokeThickness = 1.5;
            series.MarkerType = MarkerType.None;
            for (int i = 0; i < timeline.Count; i++)
                series.Points.Add(new DataPoint(timeline.Times[i], timeline.Values[i]));

            return series;
        }

        private static OxyColor ToOxyColor(Color c)
        {
            return OxyColor.FromArgb(255, c.R, c.G, c.B);
        }

        private static string FormatValue(double value)
        {
            return value.ToString("0.00", CultureInfo.CurrentCulture);
        }

        private static string EscapeFormat(string text)
        {
            return (text ?? "").Replace("{", "{{").Replace("}", "}}");
        }

        #region Export
        private void CopyGraph()
        {
            if (plotView.Model == null)
                return;

            new PlotHelper(plotView).CopyGraph(ExportWidth(), ExportHeight());
        }

        private void SaveGraph()
        {
            if (plotView.Model == null)
                return;

            new PlotHelper(plotView).ExportGraph(ExportWidth(), ExportHeight());
        }

        /// <summary>
        /// The image keeps the proportions of the panel, with a minimum size so it stays readable in a document.
        /// </summary>
        private int ExportWidth()
        {
            return Math.Max(plotView.Width, 1000);
        }

        private int ExportHeight()
        {
            double ratio = plotView.Width > 0 ? (double)plotView.Height / plotView.Width : 0.4;
            return Math.Max((int)(ExportWidth() * ratio), 300);
        }

        private void CopyData()
        {
            List<string> csv = GetCSV();
            CSVHelper.CopyToClipboard(csv);
        }

        private void SaveData()
        {
            if (tracks.Count == 0)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.DataAnalysis_ExportData;
            saveFileDialog.Filter = FilesystemHelper.SaveCSVFilter();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            try
            {
                List<string> csv = GetCSV();
                if (csv.Count > 1)
                    File.WriteAllLines(saveFileDialog.FileName, csv);
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// One row per time, one column per track, using the decimal separator configured in the preferences.
        /// </summary>
        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            if (tracks.Count == 0 || timelines.Count != tracks.Count)
                return csv;

            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);

            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(ScreenManagerLang.DataAnalysis_TimeAxisSeconds));
            foreach (DrawingTrack track in tracks)
                headers.Add(CSVHelper.WriteCell(string.Format("{0} ({1})", track.Name, speedAbbreviation)));

            csv.Add(CSVHelper.MakeRow(headers, listSeparator));

            foreach (double[] row in SpeedTimeline.MergeRows(timelines))
            {
                List<string> cells = new List<string>(row.Length);
                cells.Add(CSVHelper.WriteCell(row[0].ToString("0.000", nfi)));
                for (int i = 1; i < row.Length; i++)
                    cells.Add(double.IsNaN(row[i]) ? "" : CSVHelper.WriteCell(row[i].ToString("0.000", nfi)));

                csv.Add(CSVHelper.MakeRow(cells, listSeparator));
            }

            return csv;
        }
        #endregion
    }
}
