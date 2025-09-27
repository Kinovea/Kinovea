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
            AngularPlotHelper.ImportData(metadata, timeSeriesData);

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

        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis_AngularKinematics;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbSource.Text = ScreenManagerLang.DataAnalysis_DataSource;
            lblData.Text = ScreenManagerLang.DataAnalysis_DataLabel;
            lblTimeModel.Text = ScreenManagerLang.DataAnalysis_TimeModel;

            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_AngularKinematics;

            gbExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            lblPixels.Text = ScreenManagerLang.DataAnalysis_Pixels;
            btnImageCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportGraph.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            gbExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            btnDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportData.Text = ScreenManagerLang.DataAnalysis_SaveToFile;
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
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            string theta = metadata.CalibrationHelper.GetAngleAbbreviation();
            string omega = metadata.CalibrationHelper.GetAngularVelocityAbbreviation();
            string alpha = metadata.CalibrationHelper.GetAngularAccelerationAbbreviation();

            // We don't show the relative displacement from frame to frame 
            // as it is dependent on framerate and thus doesn't make a lot of sense here.
            AddPlotSpecification(Kinematics.AngularPosition, theta, ScreenManagerLang.DataAnalysis_AngularPosition);
            AddPlotSpecification(Kinematics.TotalAngularDisplacement, theta, ScreenManagerLang.DataAnalysis_TotalAngularDisplacement);
            AddPlotSpecification(Kinematics.AngularVelocity, omega, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_AngularVelocity);
            AddPlotSpecification(Kinematics.TangentialVelocity, v, ScreenManagerLang.DataAnalysis_TangentialVelocity);
            AddPlotSpecification(Kinematics.AngularAcceleration, alpha, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_AngularAcceleration);
            AddPlotSpecification(Kinematics.TangentialAcceleration, a, ScreenManagerLang.DataAnalysis_TangentialAcceleration);
            AddPlotSpecification(Kinematics.CentripetalAcceleration, a, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_CentripetalAcceleration);
            AddPlotSpecification(Kinematics.ResultantLinearAcceleration, a, ScreenManagerLang.DataAnalysis_ResultantLinearAcceleration);

            cmbDataSource.SelectedIndex = 0;
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
            cmbDataSource.Items.Add(new TimeSeriesPlotSpecification(label, component, abbreviation));
        }

        private void PlotOption_Changed(object sender, EventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            manualUpdate = false;
        }

        private void PlotSpec_Changed(object sender, EventArgs e)
        {
            manualUpdate = true;
            UpdatePlot();
            UpdateTitles();
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

                    points[time][i + 1] = p.Y;
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
