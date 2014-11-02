using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Kinovea.Services
{
    public partial class BenchmarkReport : Form
    {
        private List<string> extraBefore;
        private List<string> extraAfter;
        private Dictionary<string, IBenchmarkCounter> counters;

        public BenchmarkReport(List<string> extraBefore, List<string> extraAfter, Dictionary<string, IBenchmarkCounter> counters)
        {
            this.extraBefore = extraBefore;
            this.extraAfter = extraAfter;
            this.counters = counters;

            InitializeComponent();

            SetFont();
        }

        private void SetFont()
        {
            string fontName = "Consolas";
            Font font = new Font(fontName, 8, FontStyle.Regular);
            if (font.Name == fontName)
            {
                rtbReport.Font = font;
            }
            else
            {
                font.Dispose();
                rtbReport.Font = new Font("Courier New", 8.25f, FontStyle.Regular);
            }
        }

        private void BenchmarkReport_Load(object sender, EventArgs e)
        {
            CreateSummary();            
            CreatePlot();
        }

        private void CreateSummary()
        {
            AppendExtra(extraBefore);

            foreach (string counterKey in counters.Keys)
                AddMetricSummary(rtbReport, counterKey, counters[counterKey]);

            AppendExtra(extraAfter);
        }

        private void AppendExtra(List<string> extra)
        {
            StringBuilder b = new StringBuilder();
            foreach (string extraLine in extra)
                b.AppendLine(extraLine);

            rtbReport.AppendText(b.ToString());
        }

        private void AddMetricSummary(RichTextBox rtbReport, string title, IBenchmarkCounter counter)
        {
            if (counter is BenchmarkCounterIntervals)
                AddMetricSummary(rtbReport, title, counter as BenchmarkCounterIntervals);
            else if (counter is BenchmarkCounterBandwidth)
                AddMetricSummary(rtbReport, title, counter as BenchmarkCounterBandwidth);
        }

        private void AddMetricSummary(RichTextBox rtbReport, string title, BenchmarkCounterIntervals counter)
        {
            Dictionary<string, float> metrics = counter.GetMetrics();
            if (metrics == null)
                return;

            StringBuilder b = new StringBuilder();
            b.AppendLine(title);

            b.AppendLine(string.Format("- 99th percentile: {0:0.000} FPS", 1000 / metrics["Percentile99"]));
            b.AppendLine(string.Format("- 95th percentile: {0:0.000} FPS", 1000 / metrics["Percentile95"]));
            b.AppendLine(string.Format("- Median: {0:0.000} FPS", 1000 / metrics["Median"]));
            b.AppendLine(string.Format("- Average: {0:0.000} FPS", 1000 / metrics["Average"]));
            b.AppendLine(string.Format("- Standard deviation: {0:0.000} ms", metrics["StandardDeviation"]));
            b.AppendLine("");
            b.AppendLine("");
            rtbReport.AppendText(b.ToString());
        }

        private void AddMetricSummary(RichTextBox rtbReport, string title, BenchmarkCounterBandwidth counter)
        {
            Dictionary<string, float> metrics = counter.GetMetrics();
            if (metrics == null)
                return;

            StringBuilder b = new StringBuilder();
            b.AppendLine(title);
            if (metrics["Bandwidth"] != 0F)
                b.AppendLine(string.Format("- Bandwidth: {0:0.000} MB/s", metrics["Bandwidth"]));
            
            b.AppendLine(string.Format("- Average duration: {0:0.000} ms", metrics["AverageDuration"]));
            b.AppendLine(string.Format("- Standard deviation: {0:0.000} ms", metrics["StandardDeviationDuration"]));
            b.AppendLine(string.Format("- 99th percentile: {0:0} ms", metrics["Percentile99Duration"]));
            b.AppendLine(string.Format("- 95th percentile: {0:0} ms", metrics["Percentile95Duration"]));

            b.AppendLine("");
            b.AppendLine("");
            rtbReport.AppendText(b.ToString());
        }

        private void CreatePlot()
        {
            PlotModel model = new PlotModel();
            model.PlotType = PlotType.XY;

            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Dot;
            xAxis.Title = "Frame";
            model.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Dot;
            yAxis.MinimumPadding = 0.1;
            yAxis.MaximumPadding = 0.1;
            yAxis.Title = "FPS";
            model.Axes.Add(yAxis);

            //foreach (string key in counters.Keys)
                //AddMetricSeries(model, key, counters[key]);
            if (counters.ContainsKey("Heartbeat"))
                AddMetricSeries(model, "Heartbeat", counters["Heartbeat"]);
            //AddMetricSeries(model, "Store", benchmarker.GetStoreData());

            plotView.Model = model;
            plotView.InvalidatePlot(false);
        }

        private void AddMetricSeries(PlotModel model, string title, IBenchmarkCounter icounter)
        {
            BenchmarkCounterIntervals counter = icounter as BenchmarkCounterIntervals;
            if (counter == null)
                return;

            IEnumerable<float> data = counter.GetData();
            if (data == null)
                return;

            LineSeries series = new LineSeries();
            series.Title = title;
            series.MarkerType = MarkerType.None;
            series.Smooth = false;
            series.StrokeThickness = 1;

            IEnumerable<float> fps = data.Select(t => 1000.0f / t);

            int index = 0;
            foreach (float value in fps)
                series.Points.Add(new DataPoint(++index, value));

            model.Series.Add(series);
        }
    }
}
