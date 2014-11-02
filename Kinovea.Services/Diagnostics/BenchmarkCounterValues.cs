using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kinovea.Services
{
    /// <summary>
    /// Collects a series of values. Caller should invoke the Post() method passing a value.
    /// This class is not thread-safe. Each thread should have its own counter.
    /// Computes average, median, standard deviation, 95th and 99th percentiles.
    /// This can be used to measure an interval inside a loop and directly collect the interval values.
    /// To measure application wide loop frequency, use the BenchmarkCounterIntervals.
    /// </summary>
    public class BenchmarkCounterValues : IBenchmarkCounter
    {
        private List<int> values = new List<int>(5000);

        /// <summary>
        /// Add a value to the counter.
        /// </summary>
        public void Post(int value)
        {
            values.Add(value);
        }

        /// <summary>
        /// Returns the full list of values.
        /// </summary>
        public IEnumerable<int> GetData()
        {
            return values.AsEnumerable();
        }

        /// <summary>
        /// Retrieve metrics about the values.
        /// </summary>
        public Dictionary<string, float> GetMetrics()
        {
            if (values.Count == 0)
                return null;

            int total = values.Sum();
            double average = values.Average();
            values.Sort();

            Dictionary<string, float> metrics = new Dictionary<string, float>();
            if (values.Count < 50)
                return metrics;

            metrics.Add("Total", (float)total);
            metrics.Add("Average", (float)average);
            metrics.Add("Median", (float)GetPercentile(values, 0.5f));
            metrics.Add("Percentile95", (float)GetPercentile(values, 0.95f));
            metrics.Add("Percentile99", (float)GetPercentile(values, 0.99f));

            double deviationTotal = 0;
            foreach (float value in values)
                deviationTotal += ((value - average) * (value - average));

            metrics.Add("StandardDeviation", (float)Math.Sqrt(deviationTotal / values.Count));
            return metrics;
        }

        private int GetPercentile(List<int> vv, float n)
        {
            return vv[(int)(n * vv.Count)];
        }
    }
}
