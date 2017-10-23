using System;
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Piece of data required to plot one time series. 
    /// This is used by the UI to fill the list of source objects (trajectory, tracked drawing).
    /// </summary>
    public class TimeSeriesPlotData
    {
        public string Label { get; private set; }
        public Color Color { get; private set; }
        public TimeSeriesCollection TimeSeriesCollection { get; private set; }

        public TimeSeriesPlotData(string label, Color color, TimeSeriesCollection timeSeriesCollection)
        {
            this.Label = label;
            this.Color = color;
            this.TimeSeriesCollection = timeSeriesCollection;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
