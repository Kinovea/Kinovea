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
                CalibrationHelper calibrator = metadata.CalibrationHelper;
                QuadrilateralF quadImage = calibrator.CalibrationByPlane_GetProjectedQuad();
                PointF a = calibrator.GetPoint(quadImage.A);
                PointF b = calibrator.GetPoint(quadImage.B);
                PointF c = calibrator.GetPoint(quadImage.C);
                PointF d = calibrator.GetPoint(quadImage.D);

                RectangleAnnotation rectangleAnnotation = new RectangleAnnotation();
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

            plotScatter.Model = model;
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

            // Saving at a specific size will modify the axis zoom. Backup and restore after save.
            double xmin = plotScatter.Model.Axes[0].ActualMinimum;
            double xmax = plotScatter.Model.Axes[0].ActualMaximum;
            double ymin = plotScatter.Model.Axes[1].ActualMinimum;
            double ymax = plotScatter.Model.Axes[1].ActualMaximum;

            PngExporter.Export(plotScatter.Model, saveFileDialog.FileName, (int)nudWidth.Value, (int)nudHeight.Value, Brushes.White);

            plotScatter.Zoom(plotScatter.Model.Axes[0], xmin, xmax);
            plotScatter.Zoom(plotScatter.Model.Axes[1], ymin, ymax);

            plotScatter.RefreshPlot(false);
        }

        private void LabelsChanged(object sender, EventArgs e)
        {
            plotScatter.Model.Title = tbTitle.Text;
            plotScatter.Model.Axes[0].Title = tbXAxis.Text;
            plotScatter.Model.Axes[1].Title = tbYAxis.Text;

            plotScatter.RefreshPlot(false);
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
                foreach (TimedPoint point in points)
                {
                    string time = metadata.TimeCodeBuilder(point.T, TimeType.Time, TimecodeFormat.Milliseconds, false);
                    w.WriteLine(string.Format("{0};{1};{2}", time, point.X, point.Y));
                }
            }
        }

        private void btnDataCopy_Click(object sender, EventArgs e)
        {
            StringBuilder b = new StringBuilder();

            foreach (TimedPoint point in points)
            {
                string time = metadata.TimeCodeBuilder(point.T, TimeType.Time, TimecodeFormat.Milliseconds, false);
                b.AppendLine(string.Format("{0};{1};{2}", time, point.X, point.Y));
            }

            string text = b.ToString();
            Clipboard.SetText(text);
        }

        private void btnImageCopy_Click(object sender, EventArgs e)
        {
            double xmin = plotScatter.Model.Axes[0].ActualMinimum;
            double xmax = plotScatter.Model.Axes[0].ActualMaximum;
            double ymin = plotScatter.Model.Axes[1].ActualMinimum;
            double ymax = plotScatter.Model.Axes[1].ActualMaximum;

            // TODO: use new version of OxyPlot with PNGExporter.ExportToBitmap.
            Bitmap bmp = new Bitmap(plotScatter.Width, plotScatter.Height);
            plotScatter.DrawToBitmap(bmp, new Rectangle(Point.Empty, plotScatter.Size));
            Clipboard.SetImage(bmp);
            bmp.Dispose();

            plotScatter.Zoom(plotScatter.Model.Axes[0], xmin, xmax);
            plotScatter.Zoom(plotScatter.Model.Axes[1], ymin, ymax);

            plotScatter.RefreshPlot(false);
        }
    }
}
