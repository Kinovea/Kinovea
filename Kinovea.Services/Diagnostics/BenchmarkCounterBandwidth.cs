using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kinovea.Services
{
    /// <summary>
    /// Collects a series of duration/bytes pairs. Caller should invoke the Post() method passing values.
    /// This class is not thread-safe. Each thread should have its own counter.
    /// Computes total, bandwidth, average duration, median duration, standard deviation, 95th and 99th percentiles duration.
    /// </summary>
    public class BenchmarkCounterBandwidth : IBenchmarkCounter
    {
        private List<int> durations = new List<int>(5000);
        private List<int> sizes = new List<int>(5000);

        /// <summary>
        /// Add a value to the counter.
        /// </summary>
        public void Post(int duration, int size)
        {
            durations.Add(duration);
            sizes.Add(size);
        }

        /// <summary>
        /// Retrieve metrics about the values.
        /// </summary>
        public Dictionary<string, float> GetMetrics()
        {
            if (durations.Count == 0)
                return null;

            Dictionary<string, float> metrics = new Dictionary<string, float>();
            if (durations.Count < 50)
                return metrics;

            int totalDuration = durations.Sum();
            double averageDuration = durations.Average();
            metrics.Add("TotalDuration", (float)totalDuration);
            metrics.Add("AverageDuration", (float)averageDuration);

            int totalSize = sizes.Sum();
            double averageSize = durations.Average();
            metrics.Add("TotalSize", (float)totalSize);
            metrics.Add("AverageSize", (float)averageSize);

            float megabytes = (float)totalSize / (1024 * 1024) ;
            float seconds = totalDuration / 1000F;
            if (seconds > 0.001F)
            {
                float bandwidth = megabytes / seconds;
                metrics.Add("Bandwidth", bandwidth);
            }
            else
            {
                metrics.Add("Bandwidth", 0F);
            }

            durations.Sort();

            metrics.Add("MedianDuration", (float)GetPercentile(durations, 0.5f));
            metrics.Add("Percentile95Duration", (float)GetPercentile(durations, 0.95f));
            metrics.Add("Percentile99Duration", (float)GetPercentile(durations, 0.99f));

            double deviationTotalDuration = 0;
            foreach (float duration in durations)
                deviationTotalDuration += ((duration - averageDuration) * (duration - averageDuration));

            metrics.Add("StandardDeviationDuration", (float)Math.Sqrt(deviationTotalDuration / durations.Count));
            return metrics;
        }

        private int GetPercentile(List<int> vv, float n)
        {
            int index = (int)(n * vv.Count);
            return vv[index];
        }
    }
}
