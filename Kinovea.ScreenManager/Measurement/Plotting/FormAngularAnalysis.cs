using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Kinovea.Services;
using System.IO;
using System.Globalization;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{

    /// <summary>
    /// Plot page for multiple-source angular kinematics.
    /// </summary>
    public partial class FormAngularAnalysis : Form
    {
        private Metadata metadata;
        private List<TimeSeriesPlotData> timeSeriesData = new List<TimeSeriesPlotData>();
        private PlotHelper plotHelper;
        private bool manualUpdate;

        public FormAngularAnalysis(Metadata metadata)
        {
            this.metadata = metadata;
            ImportData(metadata);

            InitializeComponent();

            plotHelper = new PlotHelper(plotView);
            Localize();
            PopulateDataSources();
            PopulatePlotSpecifications();
            PopulateTimeModels();

            UpdatePlot();
        }

        private void ImportData(Metadata metadata)
        {
            ImportAngleDrawingsData(metadata);
            ImportCustomDrawingsData(metadata);
        }

        private void ImportAngleDrawingsData(Metadata metadata)
        {
            AngularKinematics angularKinematics = new AngularKinematics();

            // Create three filtered trajectories named o, a, b directly based on the trackable points.
            foreach (DrawingAngle drawingAngle in metadata.Angles())
            {
                Dictionary<string, TrackablePoint> trackablePoints = metadata.TrackabilityManager.GetTrackablePoints(drawingAngle);
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();

                bool tracked = true;

                foreach (string key in trackablePoints.Keys)
                {
                    Timeline<TrackFrame> timeline = trackablePoints[key].Timeline;
                    if (timeline.Count == 0)
                    {
                        tracked = false;
                        break;
                    }

                    List<TimedPoint> samples = timeline.Enumerate().Select(p => new TimedPoint(p.Location.X, p.Location.Y, p.Time)).ToList();
                    FilteredTrajectory traj = new FilteredTrajectory();
                    traj.Initialize(samples, metadata.CalibrationHelper);

                    trajs.Add(key, traj);
                }

                if (!tracked)
                    continue;

                TimeSeriesCollection tsc = angularKinematics.BuildKinematics(trajs, drawingAngle.AngleOptions, metadata.CalibrationHelper);
                TimeSeriesPlotData data = new TimeSeriesPlotData(drawingAngle.Name, drawingAngle.Color, tsc);
                timeSeriesData.Add(data);
            }
        }

        private void ImportCustomDrawingsData(Metadata metadata)
        {
            // Collect angular trajectories for all the angles in all the custom tools.
            AngularKinematics angularKinematics = new AngularKinematics();

            foreach (DrawingGenericPosture drawing in metadata.GenericPostures())
            {
                Dictionary<string, TrackablePoint> trackablePoints = metadata.TrackabilityManager.GetTrackablePoints(drawing);

                // First create trajectories for all the trackable points in the drawing.
                // This avoids duplicating the filtering operation for points shared by more than one angle.
                // Here the trajectories are indexed by the original alias in the custom tool, based on the index.
                Dictionary<string, FilteredTrajectory> trajs = new Dictionary<string, FilteredTrajectory>();
                bool tracked = true;

                foreach (string key in trackablePoints.Keys)
                {
                    Timeline<TrackFrame> timeline = trackablePoints[key].Timeline;
                    
                    if (timeline.Count == 0)
                    {
                        // The point is trackable but doesn't have any timeline data.
                        // This happens if the user is not tracking that drawing, so we don't need to go further.
                        tracked = false;
                        break;
                    }

                    List<TimedPoint> samples = timeline.Enumerate().Select(p => new TimedPoint(p.Location.X, p.Location.Y, p.Time)).ToList();
                    FilteredTrajectory traj = new FilteredTrajectory();
                    traj.Initialize(samples, metadata.CalibrationHelper);

                    trajs.Add(key, traj);
                }

                if (!tracked)
                    continue;

                // Loop over all angles in this drawing and find the trackable aliases of the points making up the particular angle.
                // The final collection of trajectories for each angle should have indices named o, a, b.
                foreach (GenericPostureAngle gpa in drawing.GenericPostureAngles)
                {
                    // From integer indices to tracking aliases.
                    string keyO = gpa.Origin.ToString();
                    string keyA = gpa.Leg1.ToString();
                    string keyB = gpa.Leg2.ToString();

                    // All points in an angle must be trackable as there is currently no way to get the static point coordinate.
                    if (!trajs.ContainsKey(keyO) || !trajs.ContainsKey(keyA)|| !trajs.ContainsKey(keyB))
                        continue;

                    // Remap to oab.
                    Dictionary<string, FilteredTrajectory> angleTrajs = new Dictionary<string, FilteredTrajectory>();
                    angleTrajs.Add("o", trajs[keyO]);
                    angleTrajs.Add("a", trajs[keyA]);
                    angleTrajs.Add("b", trajs[keyB]);

                    AngleOptions options = new AngleOptions(gpa.Signed, gpa.CCW, gpa.Supplementary);
                    TimeSeriesCollection tsc = angularKinematics.BuildKinematics(angleTrajs, options, metadata.CalibrationHelper);

                    string name = drawing.Name;
                    if (!string.IsNullOrEmpty(gpa.Name))
                        name = name + " - " + gpa.Name;
                    
                    Color color = gpa.Color == Color.Transparent ? drawing.Color : gpa.Color;
                    TimeSeriesPlotData data = new TimeSeriesPlotData(name, color, tsc);

                    timeSeriesData.Add(data);
                }
            }
        }

        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbSource.Text = ScreenManagerLang.DataAnalysis_DataSource;
            lblData.Text = ScreenManagerLang.DataAnalysis_DataLabel;
            //cmbDataSource

            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_ScatterPlot;
            //tbXAxis.Text = ScreenManagerLang.;
            //tbYAxis.Text = "Y axis";

            gbExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            lblPixels.Text = ScreenManagerLang.DataAnalysis_Pixels;
            btnImageCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportGraph.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            gbExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            btnDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportData.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            lblCutoffFrequencies.Text = "Selected cutoff frequencies:";

            /*if (kinematics.CanFilter)
            {
                LocalizeInfo();
                CreateDurbinWatsonPlot();
            }
            else
            {
                tabControl.TabPages.Remove(pageAbout);
            }*/
        }

        private void LocalizeInfo()
        {
            Font fontHeader = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
            Font fontText = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);

            rtbInfo1.BackColor = Color.White;
            rtbInfo1.SelectionFont = fontHeader;
            rtbInfo1.AppendText("Filtering\n");

            rtbInfo1.SelectionFont = fontText;
            StringBuilder b = new StringBuilder();
            b.AppendLine("The digitized coordinates are passed through a low pass filter to remove noise. The filter does two passes of a second-order Butterworth filter. The two passes (one forward, one backward) are used to reset the phase shift (Winter, 2009).");
            b.AppendLine("To initialize the filter the trajectory is extrapolated for 10 data points each side using reflected values around the end points. The extrapolated points are then removed from the filtered results (Smith, 1989).");
            rtbInfo1.AppendText(b.ToString());

            rtbInfo1.SelectionFont = fontHeader;
            rtbInfo1.AppendText("\nCutoff frequency\n");

            rtbInfo1.SelectionFont = fontText;
            b = new StringBuilder();
            b.AppendLine("The filter is tested on the data at various cutoff frequencies between 0.5Hz and the Nyquist frequency.");
            b.AppendLine("The best cutoff frequency is computed by estimating the autocorrelation of residuals and finding the frequency yielding the residuals that are the least autocorrelated. The filtered data set corresponding to this cutoff frequency is kept as the final result (Challis, 1999).");
            b.AppendLine("The autocorrelation of residuals is estimated using the Durbin-Watson statistic.");
            rtbInfo1.AppendText(b.ToString());

            rtbInfo2.BackColor = Color.White;
            rtbInfo2.SelectionFont = fontHeader;
            rtbInfo2.AppendText("\nReferences\n");
            rtbInfo2.SelectionFont = fontText;
            b = new StringBuilder();
            b.AppendLine("1. Smith G. (1989). Padding point extrapolation techniques for the butterworth digital filter. J. Biomech. Vol. 22, No. s/9, pp. 967-971.");
            b.AppendLine("2. Challis J. (1999). A procedure for the automatic determination of filter cutoff frequency for the processing of biomechanical data., Journal of Applied Biomechanics, Volume 15, Issue 3.");
            b.AppendLine("3. Winter, D. A. (2009). Biomechanics and motor control of human movements (4th ed.). Hoboken, New Jersey: John Wiley & Sons, Inc.");
            rtbInfo2.AppendText(b.ToString());
        }

        /*private void CreateDurbinWatsonPlot()
        {
            if (kinematics.Xs == null || kinematics.Ys == null)
            {
                plotDurbinWatson.Visible = false;
                return;
            }

            PlotModel model = new PlotModel();
            model.TitleFontSize = 12;
            model.Title = "Residuals autocorrelation";

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dot;
            xAxis.Title = "Cutoff frequency (Hz)";
            xAxis.TitleFontSize = 10;
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;
            yAxis.Title = "Autocorrelation (norm.)";
            yAxis.TitleFontSize = 10; 
            model.Axes.Add(yAxis);

            LineSeries xseries = new LineSeries();
            xseries.Color = OxyColors.Green;
            xseries.MarkerType = MarkerType.None;
            xseries.Smooth = true;


            LineSeries yseries = new LineSeries();
            yseries.Color = OxyColors.Tomato;
            yseries.MarkerType = MarkerType.None;
            yseries.Smooth = true;
            
            foreach (FilteringResult r in kinematics.FilterResultXs)
                xseries.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            foreach (FilteringResult r in kinematics.FilterResultYs)
                yseries.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            model.Series.Add(xseries);
            model.Series.Add(yseries);
            plotDurbinWatson.Model = model;
            plotDurbinWatson.BackColor = Color.White;

            lblCutoffX.Text = string.Format("X: {0:0.000} Hz", kinematics.FilterResultXs[kinematics.XCutoffIndex].CutoffFrequency);
            lblCutoffY.Text = string.Format("Y: {0:0.000} Hz", kinematics.FilterResultYs[kinematics.YCutoffIndex].CutoffFrequency);
        }*/

        private void PopulateDataSources()
        {
            // TODO: determine which ones should be checked based on saved state.
            foreach (TimeSeriesPlotData data in timeSeriesData)
            {
                clbSources.Items.Add(data);
                clbSources.SetItemChecked(clbSources.Items.Count - 1, true);
            }
        }

        private void PopulatePlotSpecifications()
        {
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            string theta = metadata.CalibrationHelper.GetAngleAbbreviation();
            string omega = metadata.CalibrationHelper.GetAngularVelocityAbbreviation();
            string alpha = metadata.CalibrationHelper.GetAngularAccelerationAbbreviation();
            
            // We don't show the relative displacement from frame to frame as it is dependent on framerate and thus doesn't make a lot of sense here.
            AddPlotSpecification(Kinematics.AngularPosition, theta, "Angle");
            AddPlotSpecification(Kinematics.TotalAngularDisplacement, theta, "Total displacement");
            AddPlotSpecification(Kinematics.AngularVelocity, omega, "Angular velocity");
            AddPlotSpecification(Kinematics.TangentialVelocity, v, "Tangential velocity");
            AddPlotSpecification(Kinematics.AngularAcceleration, alpha, "Angular acceleration");
            AddPlotSpecification(Kinematics.TangentialAcceleration, a, "Tangential acceleration");
            AddPlotSpecification(Kinematics.CentripetalAcceleration, a, "Centripetal acceleration");
            AddPlotSpecification(Kinematics.ResultantLinearAcceleration, a, "Resultant acceleration");

            cmbDataSource.SelectedIndex = 0;
        }

        private void PopulateTimeModels()
        {
            cmbTimeModel.Items.Add("Absolute");
            cmbTimeModel.Items.Add("Relative");
            cmbTimeModel.Items.Add("Normalized");
            cmbTimeModel.SelectedIndex = 0;
        }

        private void AddPlotSpecification(Kinematics component, string abbreviation, string label)
        {
            cmbDataSource.Items.Add(new TimeSeriesPlotSpecification(label, component, abbreviation));
        }

        private void PlotOption_Changed(object sender, EventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            manualUpdate = false;
        }

        private void clbSources_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            manualUpdate = false;
        }

        private void UpdatePlot()
        {
            // Create plot values from selected options.

            List<TimeSeriesPlotData> enabledSeries = new List<TimeSeriesPlotData>();
            for (int i = 0; i < clbSources.Items.Count; i++)
            {
                if (clbSources.GetItemChecked(i))
                    enabledSeries.Add(clbSources.Items[i] as TimeSeriesPlotData);
            }

            TimeSeriesPlotSpecification spec = cmbDataSource.SelectedItem as TimeSeriesPlotSpecification;
            if (spec == null)
                return;

            int selectedTimeModel = cmbTimeModel.SelectedIndex;
            if (selectedTimeModel < 0)
                return;

            TimeModel timeModel = (TimeModel)selectedTimeModel;
            PlotModel model = CreatePlot(enabledSeries, spec.Component, spec.Abbreviation, spec.Label, timeModel);

            plotView.Model = model;
            tbTitle.Text = model.Title;
            tbXAxis.Text = model.Axes[0].Title;
            tbYAxis.Text = model.Axes[1].Title;
        }

        private PlotModel CreatePlot(IEnumerable<TimeSeriesPlotData> timeSeriesPlotData, Kinematics component, string abbreviation, string title, TimeModel timeModel)
        {
            if (timeSeriesPlotData == null)
                return null;

            PlotModel model = new PlotModel();
            model.PlotType = PlotType.XY;

            model.Title = title;

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dot;
            xAxis.MinimumPadding = 0.02;
            xAxis.MaximumPadding = 0.05;
            model.Axes.Add(xAxis);

            if (timeModel == TimeModel.Absolute || timeModel == TimeModel.Relative)
                xAxis.Title = ScreenManagerLang.DataAnalysis_TimeAxisMilliseconds;
            else
                xAxis.Title = "Time";

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;
            yAxis.MinimumPadding = 0.05;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = string.Format("{0} ({1})", title, abbreviation);
            model.Axes.Add(yAxis);

            foreach (TimeSeriesPlotData tspd in timeSeriesPlotData)
            {
                LineSeries series = new LineSeries();
                series.Title = tspd.Label;
                series.Color = OxyColor.FromArgb(tspd.Color.A, tspd.Color.R, tspd.Color.G, tspd.Color.B);
                series.MarkerType = MarkerType.None;
                series.Smooth = true;

                double[] points = tspd.TimeSeriesCollection[component];
                long[] times = tspd.TimeSeriesCollection.Times;

                double firstTime = TimestampToMilliseconds(times[0]);
                double timeSpan = TimestampToMilliseconds(times[times.Length - 1]) - firstTime;

                for (int i = 0; i < points.Length; i++)
                {
                    double value = points[i];
                    if (double.IsNaN(value))
                        continue;

                    double time = TimestampToMilliseconds(times[i]);

                    switch (timeModel)
                    {
                        case TimeModel.Relative:
                            time -= firstTime;
                            break;
                        case TimeModel.Normalized:
                            time = (time - firstTime) / timeSpan;
                            break;
                        case TimeModel.Absolute:
                        default:
                            break;
                    }

                    series.Points.Add(new DataPoint(time, value));
                }

                model.Series.Add(series);
            }

            return model;
        }

        private double TimestampToMilliseconds(long ts)
        {
            long relative = ts - metadata.SelectionStart;
            double seconds = (double)relative / metadata.AverageTimeStampsPerSecond;
            double milliseconds = (seconds * 1000) / metadata.HighSpeedFactor;
            return milliseconds;
        }

        private void LabelsChanged(object sender, EventArgs e)
        {
            if (manualUpdate || plotView == null || plotView.Model == null || plotView.Model.Axes.Count != 2)
                return;

            plotView.Model.Title = tbTitle.Text;
            plotView.Model.Axes[0].Title = tbXAxis.Text;
            plotView.Model.Axes[1].Title = tbYAxis.Text;

            plotView.InvalidatePlot(false);
        }

        private void btnExportGraph_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null)
                return;

            plotHelper.ExportGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        private void btnImageCopy_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null)
                return;

            plotHelper.CopyGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        private void btnDataCopy_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null)
                return;

            List<string> lines = GetCSV();

            StringBuilder b = new StringBuilder();
            foreach (string line in lines)
                b.AppendLine(line);

            string text = b.ToString();
            Clipboard.SetText(text);
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.DataAnalysis_ExportData;
            saveFileDialog.Filter = "Comma Separated Values (*.csv)|*.csv";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            List<string> lines = GetCSV();

            using (StreamWriter w = File.CreateText(saveFileDialog.FileName))
            {
                foreach (string line in lines)
                    w.WriteLine(line);
            }
        }

        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            // Header.
            List<string> headers = new List<string>();
            headers.Add(plotView.Model.Axes[0].Title);

            foreach (var serie in plotView.Model.Series)
            {
                LineSeries s = serie as LineSeries;
                if (s == null)
                    continue;

                headers.Add(s.Title);
            }

            string header = string.Join(separator, headers.ToArray());
            csv.Add(header);

            // Values.
            TimeModel timeModel = (TimeModel)cmbTimeModel.SelectedIndex;
            SortedDictionary<double, List<double>> points = new SortedDictionary<double, List<double>>();
            int totalSeries = plotView.Model.Series.Count;
            for (int i = 0; i < plotView.Model.Series.Count; i++)
            {
                LineSeries s = plotView.Model.Series[i] as LineSeries;
                if (s == null)
                    continue;

                foreach (DataPoint p in s.Points)
                {
                    if (double.IsNaN(p.Y))
                        continue;

                    double time = p.X;
                    if (timeModel == TimeModel.Absolute || timeModel == TimeModel.Relative)
                        time = Math.Round(time);
                    else
                        time = Math.Round(time, 3);

                    if (!points.ContainsKey(time))
                    {
                        points[time] = new List<double>();
                        points[time].Add(time);

                        // Each line must have slots for all series, even if there is nothing recorded.
                        for (int j = 0; j < totalSeries; j++)
                            points[time].Add(double.NaN);
                    }

                    points[time][i + 1] = p.Y;
                }
            }

            // Project to strings.
            foreach (var p in points)
            {
                string[] values = p.Value.Select(v => double.IsNaN(v) ? "" : v.ToString()).ToArray();
                string line = string.Join(separator, values);
                csv.Add(line);
            }

            return csv;
        }
    }
}
