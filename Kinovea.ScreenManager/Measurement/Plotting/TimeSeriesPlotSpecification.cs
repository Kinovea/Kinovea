using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Description of the data that will be filled for one plot.
    /// This is used by the UI to fill the list of possible kinematic plots (speed, acceleration, etc.).
    /// </summary>
    public class TimeSeriesPlotSpecification
    {
        public string Label { get; private set; }
        public Kinematics Component { get; private set; }
        public string Abbreviation { get; private set; }
        public TimeSeriesPlotSpecification(string label, Kinematics component, string abbreviation)
        {
            this.Label = label;
            this.Component = component;
            this.Abbreviation = abbreviation;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
