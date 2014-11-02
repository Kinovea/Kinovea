using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kinovea.Services
{
    /// <summary>
    /// Collects a series of intervals. Caller should invoke the Tick() method for each event.
    /// This class is not thread-safe. Each thread should have its own counter.
    /// Computes average, median, standard deviation, 95th and 99th percentiles.
    /// </summary>
    public class BenchmarkCounterIntervals : IBenchmarkCounter
    {
        private Stopwatch watch = new Stopwatch();
        private List<long> times = new List<long>(5000);

        public BenchmarkCounterIntervals()
        {
            watch.Start();
        }

        /// <summary>
        /// Add a mark for this moment in the counter.
        /// </summary>
        public void Tick()
        {
            // As this function is called within the tight loop, we do the absolute minimum.
            // The list will have to be re-processed to extract actual metrics.
            times.Add(watch.ElapsedTicks);
        }

        /// <summary>
        /// Returns the full list of intervals in milliseconds.
        /// </summary>
        public  IEnumerable<float> GetData()
        {
            if (watch.IsRunning)
                throw new InvalidOperationException("This method should only be called after the counter has stopped collecting values");

            if (times.Count == 0)
                return null;

            List<long> spans = new List<long>();
            for (int i = 2; i < times.Count; i++)
                spans.Add(times[i] - times[i - 1]);

            return spans.Select(span => TicksToMilliseconds(span));
        }

        /// <summary>
        /// Retrieve metrics about the values.
        /// </summary>
        public Dictionary<string, float> GetMetrics()
        {
            if (watch.IsRunning)
                throw new InvalidOperationException("This method should only be called after the counter has stopped collecting values");

            if (times.Count == 0)
                return null;

            List<long> spans = new List<long>();
            float total = 0;
            for (int i = 2; i < times.Count; i++)
            {
                long span = times[i] - times[i - 1];
                spans.Add(span);
                total += span;
            }

            float averageTicks = total / spans.Count;

            spans.Sort();

            Dictionary<string, float> metrics = new Dictionary<string, float>();
            if (spans.Count < 50)
                return metrics;

            metrics.Add("Percentile99", GetPercentile(spans, 0.99f));
            metrics.Add("Percentile95", GetPercentile(spans, 0.95f));
            metrics.Add("Median", GetPercentile(spans, 0.5f));
            metrics.Add("Average", TicksToMilliseconds(averageTicks));

            float deviationTotal = 0;
            foreach (float span in spans)
                deviationTotal += ((span - averageTicks) * (span - averageTicks));

            metrics.Add("StandardDeviation", TicksToMilliseconds((float)Math.Sqrt(deviationTotal / spans.Count)));
            return metrics;
        }

        private float GetPercentile(List<long> spans, float n)
        {
            float ticks = spans[(int)(n * spans.Count)];
            float result = TicksToMilliseconds(ticks);
            return result;
        }

        private float TicksToMilliseconds(float ticks)
        {
            return (ticks / Stopwatch.Frequency) * 1000;
        }

        #region Stopwatch management
        public void Restart()
        {
            times.Clear();
            watch.Reset();
            watch.Start();
        }

        public void Stop()
        {
            watch.Stop();
        }
        #endregion
    }
}
