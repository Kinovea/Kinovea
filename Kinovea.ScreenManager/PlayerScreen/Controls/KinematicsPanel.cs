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
    /// Panel showing the speed of one trajectory over time, with a vertical cursor following the playhead.
    /// The trajectory is chosen in a drop-down listing the trajectories of the video.
    /// The data is taken from the kinematics already computed by the trajectory, and recomputed when the user clicks "Refresh".
    /// Only the cursor and the value marker on the curve are updated during playback.
    /// Optional statistics: mean line, peak and low markers, values in the legend.
    /// The graph can be exported as an image and the data as CSV, with the helpers shared with the analysis dialogs.
    /// Clicking or dragging with the left button in the plot area asks the player to seek to that time,
    /// anywhere within the seek range (the working zone), even outside the trajectory.
    /// The controls are created in code to avoid touching the designer file of the player screen.
    /// </summary>
    public class KinematicsPanel : UserControl
    {
        /// <summary>
        /// Raised when the user clicks the close button.
        /// </summary>
        public event EventHandler CloseAsked;

        /// <summary>
        /// Raised when the user clicks or drags in the plot to move the playhead.
        /// The time is a video timestamp within the seek range.
        /// </summary>
        public event EventHandler<TimeEventArgs> SeekAsked;

        /// <summary>
        /// The trajectory shown in the plot, null if there is none.
        /// </summary>
        public DrawingTrack SelectedTrack
        {
            get { return selectedTrack; }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PlotView plotView = new PlotView();
        private ComboBox cmbTrack = new ComboBox();
        private Label lblTitle = new Label();
        private CheckBox chkStatistics = new CheckBox();
        private Button btnExport = new Button();
        private Button btnRefresh = new Button();
        private Button btnClose = new Button();
        private Button btnRefit = new Button();
        private ToolTip toolTips = new ToolTip();
        private ContextMenuStrip exportMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuExportGraph = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportGraphCopy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportGraphSave = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportData = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportDataCopy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportDataSave = new ToolStripMenuItem();
        private LineAnnotation cursor;
        private PointAnnotation valueMarker;
        private string speedAbbreviation = "";
        private LinearAxis xAxis;
        private long lastSeekTimestamp = -1;
        private long cursorTimestamp = -1;
        private long seekStart = -1;
        private long seekEnd = -1;
        private Metadata metadata;
        private List<DrawingTrack> listedTracks = new List<DrawingTrack>();
        private DrawingTrack selectedTrack;
        private SpeedTimeline timeline;

        public KinematicsPanel()
        {
            this.BackColor = Color.White;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 26;

            cmbTrack.Dock = DockStyle.Left;
            cmbTrack.Width = 180;
            cmbTrack.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTrack.SelectionChangeCommitted += cmbTrack_SelectionChangeCommitted;

            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.AutoEllipsis = true;
            lblTitle.Padding = new Padding(8, 0, 0, 0);

            chkStatistics.Dock = DockStyle.Left;
            chkStatistics.Width = 100;
            chkStatistics.Checked = true;
            chkStatistics.Padding = new Padding(8, 0, 0, 0);
            chkStatistics.CheckedChanged += (s, e) => RebuildPlot(true);

            btnExport.Dock = DockStyle.Left;
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

            ConfigureIconButton(btnRefresh, Properties.Resources.arrow_refresh);
            btnRefresh.Click += (s, e) => { RefreshSelected(true); plotView.Focus(); };

            ConfigureIconButton(btnRefit, Properties.Resources.resettimescale);
            btnRefit.Click += (s, e) => RefitView();

            ConfigureIconButton(btnClose, Properties.Resources.bullet_close_black);
            btnClose.Click += (s, e) => CloseAsked?.Invoke(this, EventArgs.Empty);

            // Docking is resolved in reverse order of addition: the Fill control must be added first.
            // Left docked controls end up left to right in reverse order of addition (drop-down, statistics, export),
            // right docked controls end up right to left in reverse order of addition (close, refresh, refit).
            header.Controls.Add(lblTitle);
            header.Controls.Add(btnExport);
            header.Controls.Add(chkStatistics);
            header.Controls.Add(cmbTrack);
            header.Controls.Add(btnRefit);
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
        /// Set the metadata of the video whose trajectories are listed, and update the list.
        /// </summary>
        public void SetMetadata(Metadata metadata)
        {
            if (this.metadata != metadata)
            {
                // Another video: start over, the plot is rebuilt by UpdateTrackList.
                selectedTrack = null;
                timeline = null;
                xAxis = null;
                cursor = null;
                valueMarker = null;
                plotView.Model = null;
            }

            this.metadata = metadata;
            UpdateTrackList();
        }

        /// <summary>
        /// Rebuild the drop-down from the trajectories of the metadata and choose the one to show:
        /// the current one if it still exists, otherwise the first one with data, otherwise the first one.
        /// The plot is rebuilt only if the selection or its data changed.
        /// </summary>
        public void UpdateTrackList()
        {
            UpdateTrackList(selectedTrack != null ? selectedTrack.Id : Guid.Empty);
        }

        /// <summary>
        /// Rebuild the drop-down and select the trajectory with the passed id if it exists.
        /// The selected trajectory is only replaced after the comparison with the one currently plotted,
        /// so a change of trajectory always gets the default view.
        /// </summary>
        private void UpdateTrackList(Guid preferred)
        {
            listedTracks = metadata != null ? metadata.Tracks() : new List<DrawingTrack>();

            List<SpeedTimeline> timelines = listedTracks.Select(t => BuildTimeline(t, metadata)).ToList();
            List<Guid> ids = listedTracks.Select(t => t.Id).ToList();
            List<bool> hasData = timelines.Select(t => HasEnoughData(t)).ToList();
            int index = KinematicsTrackSelection.Pick(ids, hasData, preferred);

            cmbTrack.BeginUpdate();
            cmbTrack.Items.Clear();
            foreach (DrawingTrack track in listedTracks)
                cmbTrack.Items.Add(track.Name ?? "");

            cmbTrack.SelectedIndex = index;
            cmbTrack.EndUpdate();

            DrawingTrack newTrack = index >= 0 ? listedTracks[index] : null;
            SpeedTimeline newTimeline = index >= 0 ? timelines[index] : null;
            bool selectionChanged = newTrack != selectedTrack;
            bool dataChanged = !SameData(timeline, newTimeline);

            selectedTrack = newTrack;
            timeline = newTimeline;
            if (selectionChanged || dataChanged || plotView.Model == null)
                RebuildPlot(!selectionChanged);
        }

        /// <summary>
        /// Select the passed trajectory in the drop-down and show it.
        /// Does nothing if the trajectory is not part of the metadata passed to SetMetadata.
        /// </summary>
        public void SelectTrack(DrawingTrack track)
        {
            if (track == null || metadata == null || !metadata.Tracks().Contains(track))
                return;

            UpdateTrackList(track.Id);
        }

        /// <summary>
        /// Recompute the kinematics of the selected trajectory and rebuild the plot with the default view.
        /// </summary>
        public void RefreshData()
        {
            RefreshSelected(false);
        }

        /// <summary>
        /// Set the range of video timestamps the user can seek to from the plot, typically the working zone.
        /// </summary>
        public void SetSeekRange(long start, long end)
        {
            seekStart = start;
            seekEnd = end;
        }

        /// <summary>
        /// Forget the trajectories and clear the plot. The seek range is kept.
        /// </summary>
        public void Clear()
        {
            metadata = null;
            listedTracks.Clear();
            selectedTrack = null;
            timeline = null;
            cursor = null;
            cursorTimestamp = -1;
            valueMarker = null;
            xAxis = null;
            cmbTrack.Items.Clear();
            lblTitle.Text = "";
            plotView.Model = null;
        }

        /// <summary>
        /// Flat, borderless 16x16 button showing only an icon, like the close button of the keyframe boxes.
        /// </summary>
        private void ConfigureIconButton(Button button, Image icon)
        {
            button.Dock = DockStyle.Right;
            button.Width = 20;
            button.Size = new Size(20, 16);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.Transparent;
            button.BackgroundImage = icon;
            button.BackgroundImageLayout = ImageLayout.Center;
            button.TabStop = false;
        }

        /// <summary>
        /// Show the whole time range of the trajectory again. This is the default view, obtained by the auto-range of the time axis.
        /// </summary>
        private void RefitView()
        {
            if (plotView.Model != null)
            {
                plotView.Model.ResetAllAxes();
                plotView.InvalidatePlot(false);
            }

            plotView.Focus();
        }

        /// <summary>
        /// Recompute the kinematics of the selected trajectory and rebuild the plot.
        /// When keepZoom is true the time range currently visible is kept, otherwise the plot shows the whole trajectory.
        /// </summary>
        private void RefreshSelected(bool keepZoom)
        {
            if (selectedTrack == null || metadata == null)
                return;

            selectedTrack.UpdateKinematics();
            timeline = BuildTimeline(selectedTrack, metadata);
            RebuildPlot(keepZoom);
        }

        private void cmbTrack_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int index = cmbTrack.SelectedIndex;
            if (index < 0 || index >= listedTracks.Count || listedTracks[index] == selectedTrack)
                return;

            selectedTrack = listedTracks[index];
            timeline = BuildTimeline(selectedTrack, metadata);
            RebuildPlot(false);

            // Give the focus back to the plot so the arrow keys step the video instead of changing the trajectory.
            plotView.Focus();
        }

        /// <summary>
        /// Rebuild the plot from the current data, without recomputing the kinematics.
        /// Without usable data the plot only shows a hint and seeking is disabled.
        /// </summary>
        private void RebuildPlot(bool keepZoom)
        {
            double visibleMinimum = double.NaN;
            double visibleMaximum = double.NaN;
            if (keepZoom && xAxis != null)
            {
                visibleMinimum = xAxis.ActualMinimum;
                visibleMaximum = xAxis.ActualMaximum;
            }

            UpdateTitle();

            if (!HasData())
            {
                xAxis = null;
                cursor = null;
                valueMarker = null;
                string hint = selectedTrack == null ? ScreenManagerLang.KinematicsPanel_NoTrajectory : ScreenManagerLang.KinematicsPanel_NoData;
                plotView.Model = CreateHintPlot(hint);
                return;
            }

            PlotModel model = CreatePlot(speedAbbreviation);
            if (!double.IsNaN(visibleMinimum) && !double.IsNaN(visibleMaximum) && visibleMinimum < visibleMaximum)
                xAxis.Zoom(visibleMinimum, visibleMaximum);

            plotView.Model = model;
        }

        /// <summary>
        /// Update the texts after the user changed the interface language.
        /// The data is not recomputed.
        /// </summary>
        public void ReloadCulture()
        {
            toolTips.SetToolTip(btnRefresh, ScreenManagerLang.KinematicsPanel_Refresh);
            toolTips.SetToolTip(btnRefit, ScreenManagerLang.KinematicsPanel_Refit);
            toolTips.SetToolTip(btnClose, ScreenManagerLang.Generic_Close);
            btnExport.Text = ScreenManagerLang.KinematicsPanel_Export;
            chkStatistics.Text = ScreenManagerLang.KinematicsPanel_Statistics;
            mnuExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            mnuExportGraphCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            mnuExportGraphSave.Text = ScreenManagerLang.DataAnalysis_SaveToFile;
            mnuExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            mnuExportDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            mnuExportDataSave.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            if (metadata == null || plotView.Model == null)
                return;

            if (xAxis == null)
            {
                // The hint is the only text of the empty plot.
                RebuildPlot(false);
                return;
            }

            UpdateTitle();
            xAxis.Title = ScreenManagerLang.DataAnalysis_TimeAxisSeconds;
            plotView.InvalidatePlot(false);
        }

        private void UpdateTitle()
        {
            if (metadata == null)
            {
                lblTitle.Text = "";
                return;
            }

            speedAbbreviation = metadata.CalibrationHelper.GetSpeedAbbreviation();
            lblTitle.Text = string.Format("{0} ({1})", ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed, speedAbbreviation);
        }

        /// <summary>
        /// Move the cursor to the passed video timestamp.
        /// Called on the UI thread every time the current frame changes.
        /// The cursor is drawn even outside the range of the trajectory.
        /// </summary>
        public void UpdateCursor(long timestamp)
        {
            cursorTimestamp = timestamp;
            if (cursor == null || !HasData())
                return;

            double x = timeline.TimestampToSeconds(timestamp);
            if (x == cursor.X)
                return;

            cursor.X = x;
            UpdateValueMarker(plotView.Model, x);
            plotView.InvalidatePlot(false);
        }

        /// <summary>
        /// Move the marker on the curve to the passed time and show the value there.
        /// The marker is removed from the plot while the time is outside the range of the trajectory.
        /// </summary>
        private void UpdateValueMarker(PlotModel model, double x)
        {
            if (model == null || valueMarker == null || timeline == null)
                return;

            double value = timeline.ValueAt(x);
            bool shown = model.Annotations.Contains(valueMarker);

            if (double.IsNaN(value))
            {
                if (shown)
                    model.Annotations.Remove(valueMarker);

                return;
            }

            valueMarker.X = x;
            valueMarker.Y = value;
            valueMarker.Text = FormatValue(value);
            if (!shown)
                model.Annotations.Add(valueMarker);
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
        /// The time is only limited to the seek range, so the playhead can go before or after the trajectory.
        /// </summary>
        private void SeekToScreenX(double x)
        {
            if (xAxis == null || !HasData() || seekStart < 0 || seekEnd < seekStart)
                return;

            long timestamp = timeline.SecondsToTimestamp(xAxis.InverseTransform(x), seekStart, seekEnd);
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
            private readonly KinematicsPanel owner;

            public SeekManipulator(IPlotView view, KinematicsPanel owner)
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
        /// True if the selected trajectory has enough samples to draw a curve.
        /// </summary>
        private bool HasData()
        {
            return HasEnoughData(timeline);
        }

        private static bool HasEnoughData(SpeedTimeline timeline)
        {
            return timeline != null && timeline.Count >= 2;
        }

        private static bool SameData(SpeedTimeline a, SpeedTimeline b)
        {
            if (a == null || b == null)
                return a == b;

            return a.Times.SequenceEqual(b.Times) && a.Values.SequenceEqual(b.Values);
        }

        private static SpeedTimeline BuildTimeline(DrawingTrack track, Metadata metadata)
        {
            TimeSeriesCollection tsc = track.TimeSeriesCollection;
            if (metadata == null || tsc == null || tsc.Length == 0)
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
            model.Series.Add(CreateSeries(selectedTrack, timeline, abbreviation, statistics));
            if (statistics)
            {
                AddStatistics(model, selectedTrack, timeline);
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
            cursor.X = cursorTimestamp >= 0 ? timeline.TimestampToSeconds(cursorTimestamp) : 0;
            model.Annotations.Add(cursor);

            // The value marker is added to the model by UpdateValueMarker when the cursor is within the trajectory.
            valueMarker = new PointAnnotation();
            valueMarker.Shape = MarkerType.Circle;
            valueMarker.Size = 4;
            valueMarker.Fill = ToOxyColor(selectedTrack.MainColor);
            valueMarker.Stroke = OxyColors.White;
            valueMarker.StrokeThickness = 1;
            valueMarker.FontWeight = FontWeights.Bold;

            UpdateValueMarker(model, cursor.X);

            return model;
        }

        /// <summary>
        /// Empty plot with a message in the middle, shown when there is no trajectory or no data to draw.
        /// The axes are hidden and fixed so the message stays centered and the plot cannot be panned or zoomed.
        /// </summary>
        private static PlotModel CreateHintPlot(string text)
        {
            PlotModel model = new PlotModel();
            model.PlotAreaBorderColor = OxyColors.Transparent;
            model.Axes.Add(CreateHiddenAxis(AxisPosition.Bottom));
            model.Axes.Add(CreateHiddenAxis(AxisPosition.Left));

            TextAnnotation hint = new TextAnnotation();
            hint.Text = text;
            hint.TextPosition = new DataPoint(0.5, 0.5);
            hint.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center;
            hint.TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle;
            hint.TextColor = OxyColors.Gray;
            hint.StrokeThickness = 0;
            model.Annotations.Add(hint);

            return model;
        }

        private static LinearAxis CreateHiddenAxis(AxisPosition position)
        {
            LinearAxis axis = new LinearAxis();
            axis.Position = position;
            axis.Minimum = 0;
            axis.Maximum = 1;
            axis.IsAxisVisible = false;
            axis.IsPanEnabled = false;
            axis.IsZoomEnabled = false;
            return axis;
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
            if (!HasData())
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
        /// One row per sample: time and speed of the selected trajectory, using the decimal separator configured in the preferences.
        /// </summary>
        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            if (!HasData())
                return csv;

            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);

            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(ScreenManagerLang.DataAnalysis_TimeAxisSeconds));
            headers.Add(CSVHelper.WriteCell(string.Format("{0} ({1})", selectedTrack.Name, speedAbbreviation)));
            csv.Add(CSVHelper.MakeRow(headers, listSeparator));

            for (int i = 0; i < timeline.Count; i++)
            {
                List<string> cells = new List<string>(2);
                cells.Add(CSVHelper.WriteCell(timeline.Times[i].ToString("0.000", nfi)));
                cells.Add(CSVHelper.WriteCell(timeline.Values[i].ToString("0.000", nfi)));
                csv.Add(CSVHelper.MakeRow(cells, listSeparator));
            }

            return csv;
        }
        #endregion
    }
}
