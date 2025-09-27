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
        private List<TimeSeriesPlotData> timeSeriesData = new List<TimeSeriesPlotData>();
        private Dictionary<TimeSeriesPlotData, FilteredTrajectory> filteredTrajectories = new Dictionary<TimeSeriesPlotData, FilteredTrajectory>();
        private PlotHelper plotHelper;
        private bool manualUpdate;
        private HashSet<Guid> knownTracks = new HashSet<Guid>();

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

            manualUpdate = true;
            UpdatePlot();
            UpdateTitles();
            manualUpdate = false;
        }

        private void ImportData(Metadata metadata)
        {
            // Since drawings trackable points are based on tracks, import them first
            // and do not add the individual tracks corresponding to the same points.
            ImportOtherDrawingsData(metadata);
            ImportTrackData(metadata);
        }

        private void ImportTrackData(Metadata metadata)
        {
            foreach (DrawingTrack track in metadata.Tracks())
            {
                if (knownTracks.Contains(track.Id))
                    continue;

                TimeSeriesPlotData data = new TimeSeriesPlotData(track.Name, track.MainColor, track.TimeSeriesCollection);
                timeSeriesData.Add(data);
                filteredTrajectories.Add(data, track.FilteredTrajectory);
            }
        }

        private void ImportOtherDrawingsData(Metadata metadata)
        {
            LinearKinematics linearKinematics = new LinearKinematics();

            // Trackable drawing's individual points.
            // 2025.1: these are now based on tracks instead of their own data structures.
            foreach (ITrackable drawing in metadata.TrackableDrawings())
            {
                Dictionary<string, DrawingTrack> tracks = metadata.TrackabilityManager.GetTrackingTracks(drawing);

                if (tracks == null)
                    continue;

                // Do not show points belonging to the calibration object,
                // since it defines the coordinate system.
                if (drawing.Id == metadata.CalibrationHelper.CalibrationDrawingId || drawing.Id == metadata.DrawingCoordinateSystem.Id)
                {
                    foreach (var pair in tracks)
                    {
                        knownTracks.Add(pair.Value.Id);
                    }
                    
                    continue;
                }

                bool singlePoint = tracks.Count == 1;
                foreach (var pair in tracks)
                {
                    DrawingTrack track = pair.Value;
                    if (knownTracks.Contains(track.Id))
                        continue;

                    TimeSeriesCollection tsc = track.TimeSeriesCollection;

                    string name = drawing.Name;
                    Color color = drawing.Color;

                    // Custom drawings may have dedicated names for their handles.
                    DrawingGenericPosture dgp = drawing as DrawingGenericPosture;
                    if (dgp == null)
                    {
                        if (!singlePoint)
                            name = name + " - " + pair.Key;
                    }
                    else
                    {
                        foreach (var handle in dgp.GenericPostureHandles)
                        {
                            if (handle.Reference.ToString() != pair.Key)
                                continue;

                            name = name + " - " + (string.IsNullOrEmpty(handle.Name) ? pair.Key : handle.Name);
                            color = handle.Color == Color.Transparent ? drawing.Color : handle.Color;
                            break;
                        }
                    }

                    TimeSeriesPlotData data = new TimeSeriesPlotData(name, color, tsc);
                    timeSeriesData.Add(data);
                    filteredTrajectories.Add(data, track.FilteredTrajectory);
                    knownTracks.Add(track.Id);
                }
            }
        }

        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis_LinearKinematics;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbSource.Text = ScreenManagerLang.DataAnalysis_DataSource;
            lblData.Text = ScreenManagerLang.DataAnalysis_DataLabel;
            lblTimeModel.Text = ScreenManagerLang.DataAnalysis_TimeModel;

            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_LinearKinematics;
            
            gbExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            lblPixels.Text = ScreenManagerLang.DataAnalysis_Pixels;
            btnImageCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportGraph.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            gbExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            btnDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportData.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            LocalizeTabAbout();
        }

        private void LocalizeTabAbout()
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

            lblCutoffFrequencies.Text = "Cutoff frequencies (Hz)";
            lvCutoffFrequencies.Clear();
            lvCutoffFrequencies.Columns.Add("Source", 100);
            lvCutoffFrequencies.Columns.Add("X", 73);
            lvCutoffFrequencies.Columns.Add("Y", 73);
        }
        
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
            string d = metadata.CalibrationHelper.GetLengthAbbreviation();
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            
            AddPlotSpecification(Kinematics.X, d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalPosition);
            AddPlotSpecification(Kinematics.Y, d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalPosition);
            
            AddPlotSpecification(Kinematics.LinearDistance, d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalDistance);
            AddPlotSpecification(Kinematics.LinearHorizontalDisplacement, d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalHorizontalDisplacement);
            AddPlotSpecification(Kinematics.LinearVerticalDisplacement, d, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_TotalVerticalDisplacement);

            AddPlotSpecification(Kinematics.LinearSpeed, v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Speed);
            AddPlotSpecification(Kinematics.LinearHorizontalVelocity, v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalVelocity);
            AddPlotSpecification(Kinematics.LinearVerticalVelocity, v, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalVelocity);

            AddPlotSpecification(Kinematics.LinearAcceleration, a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Acceleration);
            AddPlotSpecification(Kinematics.LinearHorizontalAcceleration, a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_HorizontalAcceleration);
            AddPlotSpecification(Kinematics.LinearVerticalAcceleration, a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_VerticalAcceleration);
            
            cmbPlotSpec.SelectedIndex = 5;
        }

        private void PopulateTimeModels()
        {
            cmbTimeModel.Items.Add(ScreenManagerLang.DataAnalysis_TimeModel_Absolute);
            cmbTimeModel.Items.Add(ScreenManagerLang.DataAnalysis_TimeModel_Relative);
            cmbTimeModel.Items.Add(ScreenManagerLang.DataAnalysis_TimeModel_Normalized);
            cmbTimeModel.SelectedIndex = 0;
        }

        private void AddPlotSpecification(Kinematics component, string abbreviation, string label)
        {
            cmbPlotSpec.Items.Add(new TimeSeriesPlotSpecification(label, component, abbreviation));
        }

        private void PlotOption_Changed(object sender, EventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            UpdateCutoffPlot();
            manualUpdate = false;
        }

        private void PlotSpec_Changed(object sender, EventArgs e)
        {
            // This UI change is the only one that should reset the titles.
            manualUpdate = true;
            UpdatePlot();
            UpdateCutoffPlot();
            UpdateTitles();
            manualUpdate = false;
        }
        
        private void clbSources_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            UpdateCutoffPlot();
            manualUpdate = false;
        }

        private void UpdatePlot()
        {
            // Create plot values from selected options.
            List<TimeSeriesPlotData> enabledTimeSeries = GetEnabledTimeSeries();
            TimeSeriesPlotSpecification spec = cmbPlotSpec.SelectedItem as TimeSeriesPlotSpecification;
            if (spec == null)
                return;

            int selectedTimeModel = cmbTimeModel.SelectedIndex;
            if (selectedTimeModel < 0)
                return;

            TimeModel timeModel = (TimeModel)selectedTimeModel;
            PlotModel model = CreatePlot(enabledTimeSeries, spec.Component, spec.Abbreviation, spec.Label, timeModel);

            plotView.Model = model;
        }

        private void UpdateTitles()
        {
            if (plotView.Model == null)
                return;

            tbTitle.Text = plotView.Model.Title;
            tbXAxis.Text = plotView.Model.Axes[0].Title;
            tbYAxis.Text = plotView.Model.Axes[1].Title;
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
                series.Smooth = PreferencesManager.PlayerPreferences.EnableFiltering;

                double[] points = tspd.TimeSeriesCollection[component];
                long[] times = tspd.TimeSeriesCollection.Times;
 
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

        private void UpdateCutoffPlot()
        {
            List<TimeSeriesPlotData> enabledTimeSeries = GetEnabledTimeSeries();
            PlotModel model = CreateCutoffPlot(enabledTimeSeries);
            plotDurbinWatson.Model = model;
            plotDurbinWatson.BackColor = Color.White;
        }

        private PlotModel CreateCutoffPlot(IEnumerable<TimeSeriesPlotData> timeSeriesPlotData)
        {
            if (timeSeriesPlotData == null)
                return null;

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
            yAxis.Title = "Autocorrelation (normalized)";
            yAxis.TitleFontSize = 10;
            yAxis.Maximum = 1.0f;
            model.Axes.Add(yAxis);

            lvCutoffFrequencies.Items.Clear();

            foreach (TimeSeriesPlotData tspd in timeSeriesPlotData)
            {
                if (!filteredTrajectories.ContainsKey(tspd) || !filteredTrajectories[tspd].CanFilter)
                    continue;
                
                // X=red and Y=green matches the typical mapping used in 3D apps.
                LineSeries xseries = new LineSeries();
                xseries.Color = OxyColors.Tomato;
                xseries.MarkerType = MarkerType.None;
                xseries.Smooth = true;

                LineSeries yseries = new LineSeries();
                yseries.Color = OxyColors.Green; 
                yseries.MarkerType = MarkerType.None;
                yseries.Smooth = true;

                FilteredTrajectory ft = filteredTrajectories[tspd];

                foreach (FilteringResult r in ft.FilterResultXs)
                    xseries.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));
                
                foreach (FilteringResult r in ft.FilterResultYs)
                    yseries.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

                model.Series.Add(xseries);
                model.Series.Add(yseries);

                // Filtering tab.
                if (ft.XCutoffIndex >= 0 && ft.YCutoffIndex >= 0)
                {
                    double xcutoff = ft.FilterResultXs[ft.XCutoffIndex].CutoffFrequency;
                    double ycutoff = ft.FilterResultXs[ft.YCutoffIndex].CutoffFrequency;
                    string strXCutoff = string.Format("{0:0.000}", xcutoff);
                    string strYCutoff = string.Format("{0:0.000}", ycutoff);
                    lvCutoffFrequencies.Items.Add(new ListViewItem(new string[] { tspd.Label, strXCutoff, strYCutoff}));
                }
            }

            return model;
       }

        private List<TimeSeriesPlotData> GetEnabledTimeSeries()
        {
            List<TimeSeriesPlotData> enabledTimeSeries = new List<TimeSeriesPlotData>();
            for (int i = 0; i < clbSources.Items.Count; i++)
            {
                if (clbSources.GetItemChecked(i))
                    enabledTimeSeries.Add(clbSources.Items[i] as TimeSeriesPlotData);
            }

            return enabledTimeSeries;
        }
        
        private double TimestampToMilliseconds(long ts)
        {
            long t = ts - metadata.TimeOrigin;
            double seconds = (double)t / metadata.AverageTimeStampsPerSecond;
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

            List<string> csv = GetCSV();
            CSVHelper.CopyToClipboard(csv);
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            if (plotView.Model == null)
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
            catch (IOException ioException)
            {
                MessageBox.Show(string.Format(ioException.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Other error.
            }
        }

        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);

            // Header.
            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(plotView.Model.Axes[0].Title));
            
            foreach (var serie in plotView.Model.Series)
            {
                LineSeries s = serie as LineSeries;
                if (s == null)
                    continue;

                headers.Add(CSVHelper.WriteCell(s.Title));
            }

            csv.Add(CSVHelper.MakeRow(headers, listSeparator));

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

            // Project the table to CSV cells.
            foreach (var p in points)
            {
                IEnumerable<string> row = p.Value.Select(v => CSVHelper.WriteCell((float)v, nfi));
                csv.Add(CSVHelper.MakeRow(row, listSeparator));
            }

            return csv;
        }
    }
}
