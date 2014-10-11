using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{

    public class FilteringResult
    {
        public double CutoffFrequency { get; private set; }
        public double[] Data { get; private set; }
        public double DurbinWatson { get; private set; }

        public FilteringResult(double fc, double[] data, double dw)
        {
            this.CutoffFrequency = fc;
            this.Data = data;
            this.DurbinWatson = dw;
        }
    }
}
