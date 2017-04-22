﻿using System;
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
            ImportData(metadata);

            InitializeComponent();

            plotHelper = new PlotHelper(plotView);
            Localize();
            PopulateDataSources();
            PopulatePlotSpecifications();

            UpdatePlot();
        }

        private void ImportData(Metadata metadata)
        {
            AngularKinematics angularKinematics = new AngularKinematics();
            
            // Angle drawings
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

        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbSource.Text = ScreenManagerLang.DataAnalysis_DataSource;
            lblSourceXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblSourceYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            lblData.Text = ScreenManagerLang.DataAnalysis_DataLabel;

            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_ScatterPlot;
            
            gbExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            lblPixels.Text = ScreenManagerLang.DataAnalysis_Pixels;
            btnImageCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportGraph.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            gbExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            btnDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportData.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            //lblCutoffFrequencies.Text = "Selected cutoff frequencies:";
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
            
            AddPlotSpecification(Kinematics.AngularPosition, theta, "Angle");
            AddPlotSpecification(Kinematics.AngularVelocity, omega, "Angular velocity");
            AddPlotSpecification(Kinematics.TangentialVelocity, v, "Tangential velocity");
            /*AddPlotSpecification(Kinematics.AngularAcceleration, alpha, "Angular acceleration");
            AddPlotSpecification(Kinematics.TangentialAcceleration, a, "Tangential acceleration");
            AddPlotSpecification(Kinematics.CentripetalAcceleration, a, "Centripetal acceleration");
            AddPlotSpecification(Kinematics.ResultantLinearAcceleration, a, "Resultant acceleration");*/

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
            return csv;

            /*string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

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

            return csv;*/
        }
    }
}
