using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class FormPointsAnalysis : Form
    {
        private List<DrawingCrossMark> drawings = new List<DrawingCrossMark>();
        private List<TimedPoint> points = new List<TimedPoint>();
        private Metadata metadata;
        private RectangleAnnotation rectangleAnnotation;
        private double memoXMin;
        private double memoXMax;
        private double memoYMin;
        private double memoYMax;

        public FormPointsAnalysis(Metadata metadata)
        {
            this.metadata = metadata;

            InitializeComponent();
            Localize();

            foreach (Keyframe kf in metadata.Keyframes)
            {
                long t = kf.Position;    
                List<DrawingCrossMark> kfDrawings = kf.Drawings.Where(d => d is DrawingCrossMark).Select(d => (DrawingCrossMark)d).ToList();
                drawings.AddRange(kfDrawings);
                points.AddRange(kfDrawings.Select(d => new TimedPoint(d.Location.X, d.Location.Y, t)).ToList());
            }

            CreateScatterPlot();
        }
        
        private void Localize()
        {
            Text = "Data analysis";
            
            gbLabels.Text = "Labels";
            lblTitle.Text = "Title :";
            lblXAxis.Text = "X axis :";
            lblYAxis.Text = "Y axis :";
            tbTitle.Text = "Scatter plot";
            tbXAxis.Text = "X axis";
            tbYAxis.Text = "Y axis";
            cbCalibrationPlane.Text = "Calibration plane";
            
            gbExportGraph.Text = "Export graph";
            lblPixels.Text = "pixels";
            btnImageCopy.Text = "Copy to Clipboard";
            btnExportGraph.Text = "Save to file";

            gbExportData.Text = "Export data";
            btnDataCopy.Text = "Copy to Clipboard";
            btnExportData.Text = "Save to file";
        }

        private void CreateScatterPlot()
        {
            PlotModel model = new PlotModel();
            model.PlotType = PlotType.Cartesian;
            model.Title = this.tbTitle.Text;

            double padding = 0.1;

            LinearAxis xAxis = new LinearAxis();
            xAxis.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            xAxis.MinorGridlineStyle = LineStyle.Solid;
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MinimumPadding = 0.1;
            xAxis.MaximumPadding = 0.1;
            xAxis.Title = tbXAxis.Text;
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            yAxis.MinorGridlineStyle = LineStyle.Solid;
            yAxis.MinimumPadding = 0.1;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = tbYAxis.Text;
            model.Axes.Add(yAxis);

            ScatterSeries series = new ScatterSeries();
            series.MarkerType = MarkerType.Plus;
            series.MarkerStroke = OxyColors.Black;

            float yDataMinimum = float.MaxValue;
            float yDataMaximum = float.MinValue;
            float xDataMinimum = float.MaxValue;
            float xDataMaximum = float.MinValue;

            foreach (TimedPoint point in points)
            {
                PointF p = metadata.CalibrationHelper.GetPoint(point.Point);
                series.Points.Add(new ScatterPoint(p.X, p.Y));

                yDataMinimum = Math.Min(yDataMinimum, p.Y);
                yDataMaximum = Math.Max(yDataMaximum, p.Y);
                xDataMinimum = Math.Min(xDataMinimum, p.X);
                xDataMaximum = Math.Min(xDataMaximum, p.X);
            }

            model.Series.Add(series);

            if (metadata.CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                cbCalibrationPlane.Checked = true;
                cbCalibrationPlane.Enabled = true;

                CalibrationHelper calibrator = metadata.CalibrationHelper;
                QuadrilateralF quadImage = calibrator.CalibrationByPlane_GetProjectedQuad();
                PointF a = calibrator.GetPointFromRectified(quadImage.A);
                PointF b = calibrator.GetPointFromRectified(quadImage.B);
                PointF c = calibrator.GetPointFromRectified(quadImage.C);
                PointF d = calibrator.GetPointFromRectified(quadImage.D);

                rectangleAnnotation = new RectangleAnnotation();
                rectangleAnnotation.MinimumX = a.X;
                rectangleAnnotation.MaximumX = b.X;
                rectangleAnnotation.MinimumY = d.Y;
                rectangleAnnotation.MaximumY = a.Y;
                rectangleAnnotation.Fill = OxyColor.FromArgb(96, 173, 223, 247);
                rectangleAnnotation.Layer = AnnotationLayer.BelowAxes;
                model.Annotations.Add(rectangleAnnotation);

                if (a.Y > yDataMaximum || d.Y < yDataMinimum)
                {
                    yDataMaximum = Math.Max(yDataMaximum, a.Y);
                    yDataMinimum = Math.Min(yDataMinimum, d.Y);

                    double yPadding = (yDataMaximum - yDataMinimum) * padding;
                    yAxis.Maximum = yDataMaximum + yPadding;
                    yAxis.Minimum = yDataMinimum - yPadding;
                }

                if (b.X > xDataMaximum || a.X < xDataMinimum)
                {
                    xDataMaximum = Math.Max(xDataMaximum, b.X);
                    xDataMinimum = Math.Min(xDataMinimum, a.X);

                    double xPadding = (xDataMaximum - xDataMinimum) * padding;
                    xAxis.Maximum = xDataMaximum + xPadding;
                    xAxis.Minimum = xDataMinimum - xPadding;
                }
            }
            else
            {
                cbCalibrationPlane.Checked = false;
                cbCalibrationPlane.Enabled = false;
            }

            plotScatter.Model = model;
        }

        private void LabelsChanged(object sender, EventArgs e)
        {
            plotScatter.Model.Title = tbTitle.Text;
            plotScatter.Model.Axes[0].Title = tbXAxis.Text;
            plotScatter.Model.Axes[1].Title = tbYAxis.Text;

            plotScatter.InvalidatePlot(false);
        }

        private void btnExportGraph_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export graph";
            saveFileDialog.Filter = "PNG (*.png)|*.png";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            BackupView();
            PngExporter.Export(plotScatter.Model, saveFileDialog.FileName, (int)nudWidth.Value, (int)nudHeight.Value, Brushes.White);
            RestoreView();
            
            plotScatter.InvalidatePlot(false);
        }

        private void btnImageCopy_Click(object sender, EventArgs e)
        {
            BackupView();
            Bitmap bmp = PngExporter.ExportToBitmap(plotScatter.Model, (int)nudWidth.Value, (int)nudHeight.Value, Brushes.White);
            Clipboard.SetImage(bmp);
            bmp.Dispose();
            RestoreView();
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
                string unit = UnitHelper.LengthAbbreviation(metadata.CalibrationHelper.LengthUnit);
                w.WriteLine(string.Format("t (ms);x ({0});y ({1})", unit, unit));

                foreach (TimedPoint point in points)
                {
                    string time = metadata.TimeCodeBuilder(point.T, TimeType.Time, TimecodeFormat.Milliseconds, false);
                    PointF p = metadata.CalibrationHelper.GetPoint(point.Point);
                    w.WriteLine(string.Format("{0};{1};{2}", time, p.X, p.Y));
                }
            }
        }

        private void btnDataCopy_Click(object sender, EventArgs e)
        {
            StringBuilder b = new StringBuilder();

            string unit = UnitHelper.LengthAbbreviation(metadata.CalibrationHelper.LengthUnit);
            b.AppendLine(string.Format("t (ms);x ({0});y ({1})", unit, unit));

            foreach (TimedPoint point in points)
            {
                string time = metadata.TimeCodeBuilder(point.T, TimeType.Time, TimecodeFormat.Milliseconds, false);
                PointF p = metadata.CalibrationHelper.GetPoint(point.Point);
                b.AppendLine(string.Format("{0};{1};{2}", time, p.X, p.Y));
            }

            string text = b.ToString();
            Clipboard.SetText(text);
        }

        private void BackupView()
        {
            memoXMin = plotScatter.Model.Axes[0].ActualMinimum;
            memoXMax = plotScatter.Model.Axes[0].ActualMaximum;
            memoYMin = plotScatter.Model.Axes[1].ActualMinimum;
            memoYMax = plotScatter.Model.Axes[1].ActualMaximum;
        }

        private void RestoreView()
        {
            plotScatter.Model.Axes[0].Zoom(memoXMin, memoXMax);
            plotScatter.Model.Axes[1].Zoom(memoYMin, memoYMax);
            plotScatter.InvalidatePlot(false);
        }

        private void cbCalibrationPlane_CheckedChanged(object sender, EventArgs e)
        {
            if (plotScatter.Model == null)
                return;

            if (cbCalibrationPlane.Checked && plotScatter.Model.Annotations.Count == 0)
                plotScatter.Model.Annotations.Add(rectangleAnnotation);
            else if (!cbCalibrationPlane.Checked && plotScatter.Model.Annotations.Count == 1) 
                plotScatter.Model.Annotations.Remove(rectangleAnnotation);

            plotScatter.InvalidatePlot(false);
        }
    }
}
