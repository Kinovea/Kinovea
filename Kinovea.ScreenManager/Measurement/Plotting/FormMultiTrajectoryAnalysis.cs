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
    /// Plot page for combining multiple trajectories on a single plot.
    /// </summary>
    public partial class FormMultiTrajectoryAnalysis : Form
    {
        private Metadata metadata;
        private List<TrajectoryData> trajectories = new List<TrajectoryData>();
        private PlotHelper plotHelper;
        private bool manualUpdate;

        public FormMultiTrajectoryAnalysis(Metadata metadata)
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
            // TODO: import all singular points from trackable drawings.
            foreach (DrawingTrack track in metadata.Tracks())
            {
                TrajectoryData data = new TrajectoryData(track.Label, track.MainColor, track.TrajectoryKinematics);
                trajectories.Add(data);
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
            foreach (TrajectoryData data in trajectories)
            {
                clbSources.Items.Add(data);
                clbSources.SetItemChecked(clbSources.Items.Count - 1, true);
            }
        }

        private void PopulatePlotSpecifications()
        {
            string d = metadata.CalibrationHelper.GetLengthAbbreviation();
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            string da = metadata.CalibrationHelper.GetAngleAbbreviation();
            string va = metadata.CalibrationHelper.GetAngularVelocityAbbreviation();

            AddPlotSpecification("x", d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalPosition);
            AddPlotSpecification("y", d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalPosition);
            AddPlotSpecification("totalDistance", d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalDistance);
            AddPlotSpecification("speed", v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed);
            AddPlotSpecification("horizontalVelocity", v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalVelocity);
            AddPlotSpecification("verticalVelocity", v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalVelocity);
            AddPlotSpecification("acceleration", a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Acceleration);
            AddPlotSpecification("horizontalAcceleration", a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalAcceleration);
            AddPlotSpecification("verticalAcceleration", a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalAcceleration);
            AddPlotSpecification("displacementAngle", da, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_AngularDisplacement);
            AddPlotSpecification("angularVelocity", va, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_AngularVelocity);

            cmbDataSource.SelectedIndex = 3;
        }

        private void PopulateTimeModels()
        {
            cmbTimeModel.Items.Add("Absolute");
            cmbTimeModel.Items.Add("Relative");
            cmbTimeModel.Items.Add("Normalized");
            cmbTimeModel.SelectedIndex = 0;
        }

        private void AddPlotSpecification(string component, string abbreviation, string label)
        {
            cmbDataSource.Items.Add(new PlotSpecification(label, component, abbreviation));
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

            List<TrajectoryData> enabledTrajectories = new List<TrajectoryData>();
            for (int i = 0; i < clbSources.Items.Count; i++)
            {
                if (clbSources.GetItemChecked(i))
                    enabledTrajectories.Add(clbSources.Items[i] as TrajectoryData);
            }

            if (enabledTrajectories.Count == 0)
                return;

            PlotSpecification spec = cmbDataSource.SelectedItem as PlotSpecification;
            if (spec == null)
                return;

            int selectedTimeModel = cmbTimeModel.SelectedIndex;
            if (selectedTimeModel < 0)
                return;

            TimeModel timeModel = (TimeModel)selectedTimeModel;
            PlotModel model = CreatePlot(enabledTrajectories, spec.Component, spec.Abbreviation, spec.Label, timeModel);

            plotView.Model = model;
            tbTitle.Text = model.Title;
            tbXAxis.Text = model.Axes[0].Title;
            tbYAxis.Text = model.Axes[1].Title;
        }

        private PlotModel CreatePlot(IEnumerable<TrajectoryData> trajectories, string component, string abbreviation, string title, TimeModel timeModel)
        {
            if (trajectories == null)
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

            foreach (TrajectoryData trajectory in trajectories)
            {
                LineSeries series = new LineSeries();
                series.Title = trajectory.Label;
                series.Color = OxyColor.FromArgb(trajectory.Color.A, trajectory.Color.R, trajectory.Color.G, trajectory.Color.B);
                series.MarkerType = MarkerType.None;
                series.Smooth = true;

                double[] points = trajectory.Kinematics[component];
                long[] times = trajectory.Kinematics.Times;
 
                double firstTime = TimestampToMilliseconds(times[0]);
                double timeSpan = TimestampToMilliseconds(times[times.Length-1]) - firstTime;

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

                    points[time][i+1] = p.Y;
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

        /// <summary>
        /// Description of the data that will be filled for one plot.
        /// </summary>
        private class PlotSpecification
        {
            public string Label { get; private set; }
            public string Component { get; private set; }
            public string Abbreviation { get; private set; }
            public PlotSpecification(string label, string component, string abbreviation)
            {
                this.Label = label;
                this.Component = component;
                this.Abbreviation = abbreviation;
            }

            public override string ToString()
            {
                return Label;
            }
        }

        /// <summary>
        /// Piece of data required to plot one trajectory serie.  
        /// Can be populated from a track drawing or from a timeline-based drawing.
        /// </summary>
        private class TrajectoryData
        {
            public string Label { get; private set; }
            public Color Color { get; private set; }
            public TrajectoryKinematics Kinematics { get; private set; }
            public bool Enabled { get; set;}

            public TrajectoryData(string label, Color color, TrajectoryKinematics kinematics)
            {
                this.Label = label;
                this.Color = color;
                this.Kinematics = kinematics;
                this.Enabled = true;
            }

            public override string ToString()
            {
                return Label;
            }
        }
    }
}
