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

            DrawPlotCutoffDW(kinematics.Ys);
            DrawPlot(plotCoordinates, "Position", "pos", calibrationHelper.GetLengthAbbreviation(), kinematics.RawYs, kinematics.RawXs, kinematics.Ys[kinematics.YCutoffIndex].Data, kinematics.Xs[kinematics.XCutoffIndex].Data);
            DrawPlot(plotHorzVelocity, "Velocity", "velocity", calibrationHelper.GetSpeedAbbreviation(), kinematics.RawVerticalVelocity, kinematics.RawHorizontalVelocity, kinematics.VerticalVelocity, kinematics.HorizontalVelocity);
            DrawPlot(plotHorzAcceleration, "Acceleration", "acceleration", calibrationHelper.GetAccelerationAbbreviation(), kinematics.RawVerticalAcceleration, kinematics.RawHorizontalAcceleration, kinematics.VerticalAcceleration, kinematics.HorizontalAcceleration);
        }

        private void DrawPlotCutoffDW(List<FilteringResult> filteringResults)
        {
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

            LineSeries serie = new LineSeries();
            InitializeSeries(serie, OxyColors.SteelBlue, false);

            foreach (FilteringResult r in filteringResults)
                serie.Points.Add(new DataPoint(r.CutoffFrequency, r.DurbinWatson));

            model.Series.Add(serie);
            plotDurbinWatson.Model = model;
            plotDurbinWatson.BackColor = Color.White;
        }

        /*private void DrawPlot(Plot plot, string title, string serieTitle, double[] raw, double[] filtered)
        {
            PlotModel model = new PlotModel(title) { LegendSymbolLength = 24 };
            model.Axes.Add(new LinearAxis(AxisPosition.Left, string.Format("{0} (m/s)", serieTitle))
            {
                IntervalLength = 20
            });
            model.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Time (frames)"));

            LineSeries serieRaw = new LineSeries("raw"); 
            LineSeries serie = new LineSeries("filtered");
            InitializeSeries(serieRaw, true);
            InitializeSeries(serie, false);

            long time = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                double r = raw[i];
                double f = filtered[i];
                if (!double.IsNaN(r))
                {
                    serieRaw.Points.Add(new DataPoint((double)time, r));
                    serie.Points.Add(new DataPoint((double)time, f));
                }
                time++;
            }

            model.Series.Add(serieRaw);
            model.Series.Add(serie);
            plot.Model = model;
            plot.BackColor = Color.White;
        }*/

        private void DrawPlot(Plot plot, string title, string serieTitle, string abbreviation, double[] rawVert, double[] rawHorz, double[] filteredVert, double[] filteredHorz)
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

            LineSeries serieRawHorz = new LineSeries("raw x");
            LineSeries serieRawVert = new LineSeries("raw y");
            LineSeries serieHorz = new LineSeries("filtered x");
            LineSeries serieVert = new LineSeries("filtered y");
            InitializeSeries(serieRawHorz, OxyColors.DarkGray, false);
            InitializeSeries(serieRawVert, OxyColors.DarkGray, false);
            InitializeSeries(serieHorz, OxyColors.Green, true);
            InitializeSeries(serieVert, OxyColors.Tomato, true);

            long time = 0;
            for (int i = 0; i < rawVert.Length; i++)
            {
                double rv = rawVert[i];
                double rh = rawHorz[i];
                double fv = filteredVert[i];
                double fh = filteredHorz[i];
                if (!double.IsNaN(rv))
                {
                    serieRawVert.Points.Add(new DataPoint((double)time, rv));
                    serieRawHorz.Points.Add(new DataPoint((double)time, rh));
                    serieVert.Points.Add(new DataPoint((double)time, fv));
                    serieHorz.Points.Add(new DataPoint((double)time, fh));
                }
                time++;
            }

            model.Series.Add(serieRawHorz);
            model.Series.Add(serieRawVert);
            model.Series.Add(serieHorz);
            model.Series.Add(serieVert);
            
            plot.Model = model;
            plot.BackColor = Color.White;
        }

        private void InitializeSeries(LineSeries series, OxyColor color, bool smooth)
        {
            series.Color = color;
            series.MarkerType = MarkerType.None;
            series.Smooth = smooth;
        }
    }
}
