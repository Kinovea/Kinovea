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
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Kinovea.ScreenManager
{
    public partial class FormTimeseries : Form
    {
        public FormTimeseries(TrajectoryKinematics kinematics, CalibrationHelper calibrationHelper)
        {
            InitializeComponent();
            
            this.Text = "Data analysis";

            DrawPlotCutoffDW(kinematics.Xs, kinematics.Ys);
            DrawPlot(plotCoordinates, "Position", "pos", calibrationHelper.GetLengthAbbreviation(), kinematics.RawYs, kinematics.RawXs, kinematics.Ys[kinematics.YCutoffIndex].Data, kinematics.Xs[kinematics.XCutoffIndex].Data);
            DrawPlot(plotHorzVelocity, "Velocity", "velocity", calibrationHelper.GetSpeedAbbreviation(), kinematics.RawVerticalVelocity, kinematics.RawHorizontalVelocity, kinematics.VerticalVelocity, kinematics.HorizontalVelocity);
            //DrawPlot(plotHorzAcceleration, "Acceleration", "acceleration", calibrationHelper.GetAccelerationAbbreviation(), kinematics.RawVerticalAcceleration, kinematics.RawHorizontalAcceleration, kinematics.VerticalAcceleration, kinematics.HorizontalAcceleration);
            DrawPlot(plotHorzAcceleration, "Acceleration", "acceleration", calibrationHelper.GetAccelerationAbbreviation(), null, null, kinematics.VerticalAcceleration, kinematics.HorizontalAcceleration);
            //DrawPlot(plotHorzAcceleration, "Acceleration", "acceleration", calibrationHelper.GetAccelerationAbbreviation(), null, null, null, kinematics.HorizontalAcceleration);
        }

        private void DrawPlotCutoffDW(List<FilteringResult> xs, List<FilteringResult> ys)
        {
            if (xs == null || ys == null)
                return;

            PlotModel model = new PlotModel("Residuals autocorrelation vs Cutoff frequency") 
            { 
                LegendSymbolLength = 24,
                TitleFontSize = 12
            };
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Autocorrelation (norm.)")
            {
                IntervalLength = 20,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Cutoff frequency (Hz)")
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            LineSeries serieX = new LineSeries();
            InitializeSeries(serieX, OxyColors.Green, false);
            LineSeries serieY = new LineSeries();
            InitializeSeries(serieY, OxyColors.Tomato, false);

            foreach (FilteringResult r in xs)
                serieX.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            foreach (FilteringResult r in ys)
                serieY.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            model.Series.Add(serieX);
            model.Series.Add(serieY);
            plotDurbinWatson.Model = model;
            plotDurbinWatson.BackColor = Color.White;
        }

        private void DrawPlot(Plot plot, string title, string serieTitle, string abbreviation, double[] rawVert, double[] rawHorz, double[] filteredVert, double[] filteredHorz)
        {
            PlotModel model = CreatePlotModel(title, serieTitle, abbreviation);
            
            AddSeries(rawVert, "raw y", OxyColors.DarkGray, model);
            AddSeries(rawHorz, "raw x", OxyColors.DarkGray, model);
            AddSeries(filteredVert, "filtered y", OxyColors.Tomato, model);
            AddSeries(filteredHorz, "filtered x", OxyColors.Green, model);
                                   
            plot.Model = model;
            plot.BackColor = Color.White;
        }

        private PlotModel CreatePlotModel(string title, string serieTitle, string abbreviation)
        {
            PlotModel model = new PlotModel(title)
            {
                LegendSymbolLength = 24,
                TitleFontSize = 12
            };

            model.Axes.Add(new LinearAxis(AxisPosition.Left, string.Format("{0} ({1})", serieTitle, abbreviation))
            {
                IntervalLength = 20,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Time (frames)")
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            return model;
        }
        
        private void AddSeries(double[] data, string name, OxyColor color, PlotModel model)
        {
            if (data == null)
                return;

            LineSeries series = new LineSeries(name);
            InitializeSeries(series, color, false);
            long time = 0;
            for (int i = 0; i < data.Length; i++)
            {
                double value = data[i];
                if (!double.IsNaN(value))
                    series.Points.Add(new DataPoint((double)time, value));
                
                time++;
            }

            model.Series.Add(series);
        }

        private void InitializeSeries(LineSeries series, OxyColor color, bool smooth)
        {
            series.Color = color;
            series.MarkerType = MarkerType.None;
            series.Smooth = smooth;
        }
    }
}
