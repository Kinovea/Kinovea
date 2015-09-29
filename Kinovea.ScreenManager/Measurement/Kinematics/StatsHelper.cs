using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public static class StatsHelper
    {
        public static double DurbinWatson(double[] e)
        {
            double num = 0;
            double den = 0;
            for(int i = 0; i < e.Length - 1 ; i++)
            {
                num += Math.Pow((e[i+1] - e[i]), 2);
                den += Math.Pow(e[i], 2);
            }
            
            den += Math.Pow(e[e.Length-1], 2);
            return num/den;
        }
    }
}
