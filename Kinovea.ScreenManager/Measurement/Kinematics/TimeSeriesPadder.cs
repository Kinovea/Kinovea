using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public static class TimeSeriesPadder
    {
        /// <summary>
        /// Set "padding" number of values to NaN on each side of the series.
        /// </summary>
        public static void Pad(double[] values, int padding)
        {
            if (values.Length <= padding * 2)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = double.NaN;
            }
            else
            {
                for (int i = 0; i < padding; i++)
                {
                    values[i] = double.NaN;
                    values[values.Length - 1 - i] = double.NaN;
                }
            }
        }
    }
}
