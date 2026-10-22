using System;
using System.Collections.Generic;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Speed over time for a single track, ready to be plotted.
    /// Times are in seconds relative to the time origin captured at build time,
    /// so the curve and the playhead cursor always use the same reference,
    /// even if the user changes the time origin before the next refresh.
    ///
    /// This class has no dependency on UI, calibration or preferences so it can be unit tested in isolation.
    /// Smoothing is not done here: the input speeds come from the existing kinematics pipeline
    /// (Butterworth filtering of coordinates when enabled in preferences).
    /// </summary>
    public class SpeedTimeline
    {
        /// <summary>
        /// Time of each valid sample, in seconds relative to the time origin.
        /// </summary>
        public double[] Times { get; private set; }

        /// <summary>
        /// Speed of each valid sample, in the calibrated speed unit.
        /// </summary>
        public double[] Values { get; private set; }

        public int Count
        {
            get { return Times.Length; }
        }

        public bool IsEmpty
        {
            get { return Times.Length == 0; }
        }

        /// <summary>
        /// Mean speed over the samples (each sample has the same weight). NaN if empty.
        /// </summary>
        public double Mean { get; private set; }

        /// <summary>
        /// Highest speed. NaN if empty.
        /// </summary>
        public double Maximum { get; private set; }

        /// <summary>
        /// Time of the highest speed, in seconds. The first occurrence wins on ties. NaN if empty.
        /// </summary>
        public double MaximumTime { get; private set; }

        /// <summary>
        /// Lowest speed. NaN if empty.
        /// </summary>
        public double Minimum { get; private set; }

        /// <summary>
        /// Time of the lowest speed, in seconds. The first occurrence wins on ties. NaN if empty.
        /// </summary>
        public double MinimumTime { get; private set; }

        private readonly long timeOrigin;
        private readonly double timestampsPerSecond;
        private readonly double highSpeedFactor;

        private SpeedTimeline(double[] times, double[] values, long timeOrigin, double timestampsPerSecond, double highSpeedFactor)
        {
            this.Times = times;
            this.Values = values;
            this.timeOrigin = timeOrigin;
            this.timestampsPerSecond = timestampsPerSecond;
            this.highSpeedFactor = highSpeedFactor;
            ComputeStatistics();
        }

        /// <summary>
        /// Build the timeline from raw timestamps and speed values.
        /// Samples with a non finite speed (NaN, infinity) are dropped.
        /// </summary>
        /// <param name="timestamps">Timestamps of the samples, in video timestamp units.</param>
        /// <param name="speeds">Speed of each sample. Must have the same length as timestamps.</param>
        /// <param name="timeOrigin">Timestamp of the user-defined time origin.</param>
        /// <param name="timestampsPerSecond">Number of timestamps per second of video.</param>
        /// <param name="highSpeedFactor">Ratio between capture framerate and video framerate (1 for real time videos).</param>
        public static SpeedTimeline Build(long[] timestamps, double[] speeds, long timeOrigin, double timestampsPerSecond, double highSpeedFactor)
        {
            if (timestamps == null)
                throw new ArgumentNullException("timestamps");
            if (speeds == null)
                throw new ArgumentNullException("speeds");
            if (timestamps.Length != speeds.Length)
                throw new ArgumentException("timestamps and speeds must have the same length.");
            if (!IsPositiveFinite(timestampsPerSecond))
                throw new ArgumentOutOfRangeException("timestampsPerSecond");
            if (!IsPositiveFinite(highSpeedFactor))
                throw new ArgumentOutOfRangeException("highSpeedFactor");

            List<double> times = new List<double>(timestamps.Length);
            List<double> values = new List<double>(timestamps.Length);
            for (int i = 0; i < timestamps.Length; i++)
            {
                double v = speeds[i];
                if (double.IsNaN(v) || double.IsInfinity(v))
                    continue;

                times.Add(ToSeconds(timestamps[i], timeOrigin, timestampsPerSecond, highSpeedFactor));
                values.Add(v);
            }

            return new SpeedTimeline(times.ToArray(), values.ToArray(), timeOrigin, timestampsPerSecond, highSpeedFactor);
        }

        public static SpeedTimeline Empty()
        {
            return new SpeedTimeline(new double[0], new double[0], 0, 1, 1);
        }

        /// <summary>
        /// Convert a video timestamp to the time coordinate used by this timeline, in seconds.
        /// </summary>
        public double TimestampToSeconds(long timestamp)
        {
            return ToSeconds(timestamp, timeOrigin, timestampsPerSecond, highSpeedFactor);
        }

        /// <summary>
        /// Convert a time coordinate of this timeline, in seconds, back to a video timestamp.
        /// The result is rounded to the nearest timestamp. It is not limited to the range covered by the samples,
        /// only to [minTimestamp, maxTimestamp] (the working zone).
        /// Returns -1 if the timeline is empty, the time is NaN or minTimestamp is greater than maxTimestamp.
        /// </summary>
        public long SecondsToTimestamp(double seconds, long minTimestamp, long maxTimestamp)
        {
            if (IsEmpty || double.IsNaN(seconds) || minTimestamp > maxTimestamp)
                return -1;

            if (double.IsPositiveInfinity(seconds))
                return maxTimestamp;
            if (double.IsNegativeInfinity(seconds))
                return minTimestamp;

            double offset = Math.Round(seconds * highSpeedFactor * timestampsPerSecond);
            double timestamp = timeOrigin + offset;
            if (timestamp <= minTimestamp)
                return minTimestamp;
            if (timestamp >= maxTimestamp)
                return maxTimestamp;

            return (long)timestamp;
        }

        /// <summary>
        /// Speed at the passed time, in seconds, linearly interpolated between the two closest samples.
        /// Returns NaN outside the range covered by the samples.
        /// </summary>
        public double ValueAt(double seconds)
        {
            if (IsEmpty || double.IsNaN(seconds) || seconds < Times[0] || seconds > Times[Times.Length - 1])
                return double.NaN;

            int index = Array.BinarySearch(Times, seconds);
            if (index >= 0)
                return Values[index];

            // Not an exact match: ~index is the first sample after the time, there is always one before it.
            int after = ~index;
            int before = after - 1;
            double ratio = (seconds - Times[before]) / (Times[after] - Times[before]);
            return Values[before] + ratio * (Values[after] - Values[before]);
        }

        private void ComputeStatistics()
        {
            if (IsEmpty)
            {
                Mean = Maximum = MaximumTime = Minimum = MinimumTime = double.NaN;
                return;
            }

            int maxIndex = 0;
            int minIndex = 0;
            double sum = 0;
            for (int i = 0; i < Values.Length; i++)
            {
                sum += Values[i];
                if (Values[i] > Values[maxIndex])
                    maxIndex = i;
                if (Values[i] < Values[minIndex])
                    minIndex = i;
            }

            Mean = sum / Values.Length;
            Maximum = Values[maxIndex];
            MaximumTime = Times[maxIndex];
            Minimum = Values[minIndex];
            MinimumTime = Times[minIndex];
        }

        private static double ToSeconds(long timestamp, long timeOrigin, double timestampsPerSecond, double highSpeedFactor)
        {
            return (timestamp - timeOrigin) / timestampsPerSecond / highSpeedFactor;
        }

        private static bool IsPositiveFinite(double value)
        {
            return value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
        }
    }
}
