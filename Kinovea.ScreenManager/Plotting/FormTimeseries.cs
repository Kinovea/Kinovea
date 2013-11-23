using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Kinovea.ScreenManager
{
    public partial class FormTimeseries : Form
    {
        public FormTimeseries(TrajectoryKinematics kinematics)
        {
            InitializeComponent();

            //DrawPlotCutoffDW(kinematics.Ys);
            DrawPlotCoordinates(kinematics);
            DrawPlotVelocity(kinematics);
            DrawPlotAcceleration(kinematics);
        }

        private void DrawPlotCutoffDW(List<FilteringResult> filteringResults)
        {
            PlotModel model = new PlotModel("Residuals autocorrelation vs Cutoff frequency") { LegendSymbolLength = 24 };
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Autocorrelation (normalized)"));
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Cutoff frequency (Hz)"));

            LineSeries serie = new LineSeries();
            InitializeSeries(serie, false);

            foreach (FilteringResult r in filteringResults)
                serie.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            model.Series.Add(serie);
            plotCoordinates.Model = model;
            plotCoordinates.BackColor = Color.White;
        }

        private void DrawPlotCoordinates(TrajectoryKinematics kinematics)
        {
            PlotModel model = new PlotModel("Y Position") { LegendSymbolLength = 24 };
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Y pos (m)"));
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Time (frames)"));

            LineSeries serieRaw = new LineSeries("raw pos");
            LineSeries serie = new LineSeries("pos");
            InitializeSeries(serieRaw, true);
            InitializeSeries(serie, false);

            long time = 0;
            for (int i = 0; i < kinematics.Length; i++)
            {
                serieRaw.Points.Add(new DataPoint((double)time, kinematics.RawCoordinates(i).Y));
                serie.Points.Add(new DataPoint((double)time, kinematics.Coordinates(i).Y));
                time++;
            }
            
            model.Series.Add(serieRaw);
            model.Series.Add(serie);
            plotCoordinates.Model = model;
            plotCoordinates.BackColor = Color.White;
        }

        private void DrawPlotVelocity(TrajectoryKinematics kinematics)
        {
            PlotModel model = new PlotModel("Y Velocity") { LegendSymbolLength = 24 };
            model.Axes.Add(new LinearAxis(AxisPosition.Left, "Velocity (m/s)"));
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Time (frames)"));

            LineSeries serieRaw = new LineSeries("raw Y velocity"); 
            LineSeries serie = new LineSeries("Y Velocity");
            InitializeSeries(serieRaw, true);
            InitializeSeries(serie, false);

            long time = 0;
            for (int i = 0; i < kinematics.Length; i++)
            {
                double raw = kinematics.RawVerticalVelocity[i];
                double v = kinematics.VerticalVelocity[i];
                if (!double.IsNaN(v))
                {
                    serie.Points.Add(new DataPoint((double)time, v));
                    serieRaw.Points.Add(new DataPoint((double)time, raw));
                }
                time++;
            }

            model.Series.Add(serieRaw);
            model.Series.Add(serie);
            plotVelocity.Model = model;
            plotVelocity.BackColor = Color.White;
        }

        private void DrawPlotAcceleration(TrajectoryKinematics kinematics)
        {
            PlotModel model = new PlotModel("Y Acceleration") { LegendSymbolLength = 24 };
            //LinearAxis yAxis = new LinearAxis(AxisPosition.Left, -35, 5, "Acceleration (m/s²)")
            LinearAxis yAxis = new LinearAxis(AxisPosition.Left, "Acceleration (m/s²)")
            {
                IntervalLength = 20,
                //ExtraGridlines = new[] { -9.81 }
            };

            model.Axes.Add(yAxis);
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Time (frames)"));

            LineSeries serieRaw = new LineSeries("Raw Acceleration");
            LineSeries serie = new LineSeries("Filtered Acceleration");
            InitializeSeries(serieRaw, true);
            InitializeSeries(serie, false);

            long time = 0;
            for (int i = 0; i < kinematics.Length; i++)
            {
                double raw = kinematics.RawVerticalAcceleration[i];
                double v = kinematics.VerticalAcceleration[i];
                if (!double.IsNaN(v))
                {
                    serie.Points.Add(new DataPoint((double)time, v));
                    serieRaw.Points.Add(new DataPoint((double)time, raw));
                }
                time++;
            }

            model.Series.Add(serieRaw);
            model.Series.Add(serie);
            plotAcceleration.Model = model;
            plotAcceleration.BackColor = Color.White;
        }

        private void InitializeSeries(LineSeries series, bool raw)
        {
            series.Color = raw ? OxyColors.DarkGray : OxyColors.Tomato;
            series.MarkerType = MarkerType.None;
            series.Smooth = !raw;
        }
    }
}
