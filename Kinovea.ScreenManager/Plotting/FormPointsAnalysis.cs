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
using OxyPlot.Annotations;

namespace Kinovea.ScreenManager
{
    public partial class FormPointsAnalysis : Form
    {
        private List<DrawingCrossMark> drawings = new List<DrawingCrossMark>();
        private Metadata metadata;

        public FormPointsAnalysis(Metadata metadata)
        {
            this.metadata = metadata;

            InitializeComponent();
            Localize();

            foreach (Keyframe kf in metadata.Keyframes)
                drawings.AddRange(kf.Drawings.Where(d => d is DrawingCrossMark).Select(d => (DrawingCrossMark)d));

            CreateScatterPlot();
        }
        
        private void Localize()
        {
            this.Text = "Data analysis";
        }

        private void CreateScatterPlot()
        {
            PlotModel model = new PlotModel();
            model.PlotType = PlotType.Cartesian;
            model.Title = "Scatter plot";

            double padding = 0.1;

            LinearAxis xAxis = new LinearAxis();
            xAxis.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            xAxis.MinorGridlineStyle = LineStyle.Solid;
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MinimumPadding = 0.1;
            xAxis.MaximumPadding = 0.1;
            xAxis.Title = "X-axis";
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            yAxis.MinorGridlineStyle = LineStyle.Solid;
            yAxis.MinimumPadding = 0.1;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = "Y-axis";
            model.Axes.Add(yAxis);

            ScatterSeries series = new ScatterSeries();
            series.MarkerType = MarkerType.Plus;
            series.MarkerStroke = OxyColors.Black;

            float yDataMinimum = float.MaxValue;
            float yDataMaximum = float.MinValue;
            float xDataMinimum = float.MaxValue;
            float xDataMaximum = float.MinValue;

            foreach (DrawingCrossMark drawing in drawings)
            {
                PointF p = drawing.Location;
                p = metadata.CalibrationHelper.GetPoint(p);
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
    }
}
