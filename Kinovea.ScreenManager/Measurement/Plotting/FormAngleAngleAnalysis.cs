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
    /// Plot page for angle-angle diagrams.
    /// </summary>
    public partial class FormAngleAngleAnalysis : Form
    {
        private Metadata metadata;
        private List<TimeSeriesPlotData> timeSeriesData = new List<TimeSeriesPlotData>();
        private PlotHelper plotHelper;
        private bool manualUpdate;

        public FormAngleAngleAnalysis(Metadata metadata)
        {
            this.metadata = metadata;
            AngularPlotHelper.ImportData(metadata, timeSeriesData);

            InitializeComponent();

            plotHelper = new PlotHelper(plotView);
            Localize();
            PopulateDataSources();
            PopulatePlotSpecifications();

            UpdatePlot();
        }

        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis_AngleAngleDiagrams;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbSource.Text = ScreenManagerLang.DataAnalysis_DataSource;
            lblSourceXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblSourceYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            lblData.Text = ScreenManagerLang.DataAnalysis_DataLabel;

            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_AngleAngleDiagrams;
            
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
            foreach (TimeSeriesPlotData data in timeSeriesData)
            {
                cbSourceX.Items.Add(data);
                cbSourceY.Items.Add(data);
            }

            if (timeSeriesData.Count > 0)
            {
                cbSourceX.SelectedIndex = 0;

                if (timeSeriesData.Count > 1)
                    cbSourceY.SelectedIndex = 1;
            }
        }

        private void PopulatePlotSpecifications()
        {
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            string theta = metadata.CalibrationHelper.GetAngleAbbreviation();
            string omega = metadata.CalibrationHelper.GetAngularVelocityAbbreviation();
            string alpha = metadata.CalibrationHelper.GetAngularAccelerationAbbreviation();
            
            AddPlotSpecification(Kinematics.AngularPosition, theta, ScreenManagerLang.DataAnalysis_AngularPosition);
            AddPlotSpecification(Kinematics.AngularVelocity, omega, ScreenManagerLang.dlgConfigureTrajectory_ExtraData_AngularVelocity);
            AddPlotSpecification(Kinematics.TangentialVelocity, v, ScreenManagerLang.DataAnalysis_TangentialVelocity);
            cmbDataSource.SelectedIndex = 0;
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

        private void UpdatePlot()
        {
            // Create plot values from selected options.

            TimeSeriesPlotData xSeries = cbSourceX.SelectedItem as TimeSeriesPlotData;
            TimeSeriesPlotData ySeries = cbSourceY.SelectedItem as TimeSeriesPlotData;
            if (xSeries == null || ySeries == null || xSeries == ySeries)
                return;

            TimeSeriesPlotSpecification spec = cmbDataSource.SelectedItem as TimeSeriesPlotSpecification;
            if (spec == null)
                return;

            PlotModel model = CreatePlot(xSeries, ySeries, spec.Component, spec.Abbreviation, spec.Label);

            plotView.Model = model;
            tbTitle.Text = model.Title;
            tbXAxis.Text = model.Axes[0].Title;
            tbYAxis.Text = model.Axes[1].Title;
        }

        private PlotModel CreatePlot(TimeSeriesPlotData xSeries, TimeSeriesPlotData ySeries, Kinematics component, string abbreviation, string title)
        {
            if (xSeries == null || ySeries == null || xSeries == ySeries)
                return null;

            PlotModel model = new PlotModel();
            model.PlotType = PlotType.XY;

            model.Title = string.Format("{0} v {1} - {2}", xSeries.Label, ySeries.Label, title);

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dot;
            xAxis.MinimumPadding = 0.05;
            xAxis.MaximumPadding = 0.1;
            xAxis.Title = string.Format("{0} {1} ({2})", xSeries.Label, title, abbreviation);
            model.Axes.Add(xAxis);
            
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;
            yAxis.MinimumPadding = 0.05;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = string.Format("{0} {1} ({2})", ySeries.Label, title, abbreviation);
            model.Axes.Add(yAxis);

            LineSeries series = new LineSeries();
            //series.Title = 
            //series.Color = OxyColor.;
            series.MarkerType = MarkerType.Circle;
            series.Smooth = true;

            double[] xPoints = xSeries.TimeSeriesCollection[component];
            double[] yPoints = ySeries.TimeSeriesCollection[component];
            long[] xTimes = xSeries.TimeSeriesCollection.Times;
            long[] yTimes = ySeries.TimeSeriesCollection.Times;

            // The plot is only defined in the range of common time coordinates.
            int xIndex = 0;
            int yIndex = 0;
            while (xIndex < xTimes.Length && yIndex < yTimes.Length)
            {
                long xTime = xTimes[xIndex];
                long yTime = yTimes[yIndex];

                if (xTime < yTime)
                {
                    xIndex++;
                    continue;
                }
                else if (yTime < xTime)
                {
                    yIndex++;
                    continue;
                }
                else
                {
                    series.Points.Add(new DataPoint(xPoints[xIndex], yPoints[yIndex]));
                    xIndex++;
                    yIndex++;
                }
            }

            model.Series.Add(series);

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

            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(plotView.Model.Axes[0].Title));
            headers.Add(CSVHelper.WriteCell(plotView.Model.Axes[1].Title));
            csv.Add(CSVHelper.MakeRow(headers, listSeparator));
            
            Dictionary<double, double> points = new Dictionary<double, double>();
            LineSeries s = plotView.Model.Series[0] as LineSeries;
            if (s == null)
                return csv;

            foreach (DataPoint p in s.Points)
            {
                List<string> cells = new List<string>();
                cells.Add(CSVHelper.WriteCell((float)p.X, nfi));
                cells.Add(CSVHelper.WriteCell((float)p.Y, nfi));
                
                csv.Add(CSVHelper.MakeRow(cells, listSeparator));
            }
            
            return csv;
        }
    }
}
