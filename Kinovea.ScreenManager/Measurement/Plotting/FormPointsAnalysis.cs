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
using System.Globalization;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Scatter analysis based on all the points registered.
    /// </summary>
    public partial class FormPointsAnalysis : Form
    {
        private List<DrawingCrossMark> drawings = new List<DrawingCrossMark>();
        private List<TimedPoint> points = new List<TimedPoint>();
        private Metadata metadata;
        private RectangleAnnotation rectangleAnnotation;
        private PlotHelper plotHelper;

        public FormPointsAnalysis(Metadata metadata)
        {
            this.metadata = metadata;

            InitializeComponent();
            plotHelper = new PlotHelper(plotScatter);
            Localize();

            foreach (Keyframe kf in metadata.Keyframes)
            {
                long t = kf.Timestamp;
                List<DrawingCrossMark> kfDrawings = kf.Drawings.Where(d => d is DrawingCrossMark).Select(d => (DrawingCrossMark)d).ToList();
                
                // Points are revesed to match the order of addition.
                kfDrawings.Reverse();

                drawings.AddRange(kfDrawings);
                points.AddRange(kfDrawings.Select(d => new TimedPoint(d.Location.X, d.Location.Y, t)).ToList());
            }

            CreateScatterPlot();
        }
        
        private void Localize()
        {
            Text = ScreenManagerLang.DataAnalysis_ScatterDiagram;
            pagePlot.Text = ScreenManagerLang.DataAnalysis_PagePlot;
            gbLabels.Text = ScreenManagerLang.DataAnalysis_Labels;
            lblTitle.Text = ScreenManagerLang.DataAnalysis_Title;
            lblXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisLabel;
            lblYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisLabel;
            tbTitle.Text = ScreenManagerLang.DataAnalysis_ScatterDiagram;
            tbXAxis.Text = ScreenManagerLang.DataAnalysis_XaxisDefaultPoints;
            tbYAxis.Text = ScreenManagerLang.DataAnalysis_YaxisDefaultPoints;
            cbCalibrationPlane.Text = ScreenManagerLang.DataAnalysis_CalibrationPlane;
            
            gbExportGraph.Text = ScreenManagerLang.DataAnalysis_ExportGraph;
            lblPixels.Text = ScreenManagerLang.DataAnalysis_Pixels;
            btnImageCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportGraph.Text = ScreenManagerLang.DataAnalysis_SaveToFile;

            gbExportData.Text = ScreenManagerLang.DataAnalysis_ExportData;
            btnDataCopy.Text = ScreenManagerLang.mnuCopyToClipboard;
            btnExportData.Text = ScreenManagerLang.DataAnalysis_SaveToFile;
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
                PointF p = metadata.CalibrationHelper.GetPointAtTime(point.Point, point.T);
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
            if (plotScatter.Model == null)
                return;

            plotScatter.Model.Title = tbTitle.Text;
            plotScatter.Model.Axes[0].Title = tbXAxis.Text;
            plotScatter.Model.Axes[1].Title = tbYAxis.Text;

            plotScatter.InvalidatePlot(false);
        }

        private void btnExportGraph_Click(object sender, EventArgs e)
        {
            plotHelper.ExportGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        private void btnImageCopy_Click(object sender, EventArgs e)
        {
            plotHelper.CopyGraph((int)nudWidth.Value, (int)nudHeight.Value);
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
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

        private void btnDataCopy_Click(object sender, EventArgs e)
        {
            List<string> csv = GetCSV();
            CSVHelper.CopyToClipboard(csv);
        }

        private List<string> GetCSV()
        {
            List<string> csv = new List<string>();
            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);
            string lengthUnit = UnitHelper.LengthAbbreviation(metadata.CalibrationHelper.LengthUnit);
            string timeUnit = UnitHelper.TimeAbbreviation(PreferencesManager.PlayerPreferences.TimecodeFormat);

            List<string> headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(string.Format("Time ({0})", timeUnit)));
            headers.Add(CSVHelper.WriteCell(string.Format("X ({0})", lengthUnit)));
            headers.Add(CSVHelper.WriteCell(string.Format("Y ({0})", lengthUnit)));
            csv.Add(CSVHelper.MakeRow(headers, listSeparator));

            foreach (TimedPoint point in points)
            {
                float time = metadata.GetNumericalTime(point.T, TimeType.UserOrigin);
                PointF p = metadata.CalibrationHelper.GetPointAtTime(point.Point, point.T);

                List<string> row = new List<string>();
                row.Add(CSVHelper.WriteCell(time, nfi));
                row.Add(CSVHelper.WriteCell(p.X, nfi));
                row.Add(CSVHelper.WriteCell(p.Y, nfi));
                csv.Add(CSVHelper.MakeRow(row, listSeparator));
            }

            return csv;
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
