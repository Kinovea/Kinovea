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

namespace Kinovea.ScreenManager
{
    public partial class FormTrackAnalysis : Form
    {
        private Metadata metadata;
        private TrajectoryKinematics kinematics;
        private Color color;
        private PlotHelper plotHelper;
        private Dictionary<string, PlotModel> plots = new Dictionary<string, PlotModel>();
        private bool manualUpdate;
        private PlotObject currentPlot;

        public FormTrackAnalysis(Metadata metadata, DrawingTrack track)
        {
            this.metadata = metadata;
            this.kinematics = track.TrajectoryKinematics;
            this.color = track.MainColor;

            InitializeComponent();
            
            plotHelper = new PlotHelper(plotView);
            Localize();
            CreatePlots();
        }

        private void Localize()
        {
            Text = "Data analysis";

            gbSource.Text = "Data source";
            lblData.Text = "Data :";
            //cmbDataSource

            gbLabels.Text = "Labels";
            lblTitle.Text = "Title :";
            lblXAxis.Text = "X axis :";
            lblYAxis.Text = "Y axis :";
            tbTitle.Text = "Scatter plot";
            tbXAxis.Text = "Time";
            tbYAxis.Text = "Y axis";

            gbExportGraph.Text = "Export graph";
            lblPixels.Text = "pixels";
            btnImageCopy.Text = "Copy to Clipboard";
            btnExportGraph.Text = "Save to file";

            gbExportData.Text = "Export data";
            btnDataCopy.Text = "Copy to Clipboard";
            btnExportData.Text = "Save to file";

            //cmbDataSource.Items.Add(new DataComponent("xs", "Horizontal position"));
        }

        private void CreatePlots()
        {
            // TODO: localize title and time.

            string d = metadata.CalibrationHelper.GetLengthAbbreviation();
            string v = metadata.CalibrationHelper.GetSpeedAbbreviation();
            string a = metadata.CalibrationHelper.GetAccelerationAbbreviation();
            string da = metadata.CalibrationHelper.GetAngleAbbreviation();
            string va = metadata.CalibrationHelper.GetAngularVelocityAbbreviation();

            AddPlot(kinematics.Xs, "Horizontal position", d, false);
            AddPlot(kinematics.Ys, "Vertical position", d, false);
            AddPlot(kinematics.TotalDistance, "Total distance", d, false);
            AddPlot(kinematics.Speed, "Speed", v, true);
            AddPlot(kinematics.HorizontalVelocity, "Horizontal velocity", v, false);
            AddPlot(kinematics.VerticalVelocity, "Vertical velocity", v, false);
            AddPlot(kinematics.Acceleration, "Acceleration", a, false);
            AddPlot(kinematics.HorizontalAcceleration, "Horizontal acceleration", a, false);
            AddPlot(kinematics.VerticalAcceleration, "Vertical acceleration", a, false);
            AddPlot(kinematics.DisplacementAngle, "Angular displacement", da, false);
            AddPlot(kinematics.AngularVelocity, "Angular velocity", va, false);
            //AddPlot(kinematics.AngularAcceleration, "Angular acceleration", false);
            //AddPlot(kinematics.CentripetalAcceleration, "Centripetal acceleration", false);
        }

        private void AddPlot(double[] data, string title, string abbreviation, bool selected)
        {
            string t = "Time (ms)";
            PlotModel model = CreatePlot(data, title, abbreviation, t);

            cmbDataSource.Items.Add(new PlotObject(title, model, data, abbreviation));
            if (selected)
                cmbDataSource.SelectedItem = cmbDataSource.Items[cmbDataSource.Items.Count-1];
        }
        
        private PlotModel CreatePlot(double[] data, string title, string abbreviation, string xLegend)
        {
            if (data == null)
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
            xAxis.Title = xLegend;
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;
            yAxis.MinimumPadding = 0.05;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = string.Format("{0} ({1})", title, abbreviation);
            model.Axes.Add(yAxis);

            LineSeries series = new LineSeries();
            series.Title = title;
            series.Color = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
            series.MarkerType = MarkerType.None;
            series.Smooth = true;

            for (int i = 0; i < data.Length; i++)
            {
                double value = data[i];
                double time = TimestampToMilliseconds(kinematics.Times[i]);
                if (!double.IsNaN(value))
                    series.Points.Add(new DataPoint(time, value));
            }

            model.Series.Add(series);
            return model;
        }

        private double TimestampToMilliseconds(long ts)
        {
            double seconds = (double)ts / metadata.AverageTimeStampsPerSecond;
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
            plotHelper.ExportGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        private void btnImageCopy_Click(object sender, EventArgs e)
        {
            plotHelper.CopyGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        

        private void cmbDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            PlotObject p = cmbDataSource.SelectedItem as PlotObject;
            if (p == null)
                return;

            manualUpdate = true;

            plotView.Model = p.PlotModel;
            currentPlot = p;
            tbTitle.Text = plotView.Model.Title;
            tbXAxis.Text = plotView.Model.Axes[0].Title;
            tbYAxis.Text = plotView.Model.Axes[1].Title;

            manualUpdate = false;
        }

        private void btnDataCopy_Click(object sender, EventArgs e)
        {
            StringBuilder b = new StringBuilder();

            b.AppendLine(string.Format("{0};{1}", currentPlot.PlotModel.Axes[0].Title, currentPlot.PlotModel.Axes[1].Title));

            double[] data = currentPlot.Data;
            for (int i = 0; i < data.Length; i++)
            {
                double value = Math.Round(data[i], 3);
                double time = Math.Round(TimestampToMilliseconds(kinematics.Times[i]));
                if (!double.IsNaN(value))
                    b.AppendLine(string.Format("{0};{1}", time, value));
            }

            string text = b.ToString();
            Clipboard.SetText(text);
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export data";
            saveFileDialog.Filter = "Comma Separated Values (*.csv)|*.csv";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            using (StreamWriter w = File.CreateText(saveFileDialog.FileName))
            {
                w.WriteLine(string.Format("{0};{1}", currentPlot.PlotModel.Axes[0].Title, currentPlot.PlotModel.Axes[1].Title));

                double[] data = currentPlot.Data;
                for (int i = 0; i < data.Length; i++)
                {
                    double value = Math.Round(data[i], 3);
                    double time = Math.Round(TimestampToMilliseconds(kinematics.Times[i]));
                    if (!double.IsNaN(value))
                        w.WriteLine(string.Format("{0};{1}", time, value));
                }
            }
        }

        private class PlotObject
        {
            public string Label { get; private set; }
            public PlotModel PlotModel { get; private set; }
            public double[] Data { get; private set; }
            public string Abbreviation { get; private set; }
            public PlotObject(string label, PlotModel plotModel, double[] data, string abbreviation)
            {
                this.Label = label;
                this.PlotModel = plotModel;
                this.Data = data;
                this.Abbreviation = abbreviation;
            }

            public override string ToString()
            {
                return Label;
            }
        }
    }
}
