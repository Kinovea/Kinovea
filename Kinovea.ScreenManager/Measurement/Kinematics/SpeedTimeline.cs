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

        private readonly long[] timestamps;
        private readonly long timeOrigin;
        private readonly double timestampsPerSecond;
        private readonly double highSpeedFactor;

        private SpeedTimeline(long[] timestamps, double[] times, double[] values, long timeOrigin, double timestampsPerSecond, double highSpeedFactor)
        {
            this.timestamps = timestamps;
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

            List<long> validTimestamps = new List<long>(timestamps.Length);
            List<double> times = new List<double>(timestamps.Length);
            List<double> values = new List<double>(timestamps.Length);
            for (int i = 0; i < timestamps.Length; i++)
            {
                double v = speeds[i];
                if (double.IsNaN(v) || double.IsInfinity(v))
                    continue;

                validTimestamps.Add(timestamps[i]);
                times.Add(ToSeconds(timestamps[i], timeOrigin, timestampsPerSecond, highSpeedFactor));
                values.Add(v);
            }

            return new SpeedTimeline(validTimestamps.ToArray(), times.ToArray(), values.ToArray(), timeOrigin, timestampsPerSecond, highSpeedFactor);
        }

        public static SpeedTimeline Empty()
        {
            return new SpeedTimeline(new long[0], new double[0], new double[0], 0, 1, 1);
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
        /// The result is rounded to the nearest timestamp and clamped to the range covered by the samples,
        /// so a click anywhere in the plot always lands on a frame of the track.
        /// Returns -1 if the timeline is empty.
        /// </summary>
        public long SecondsToTimestamp(double seconds)
        {
            if (IsEmpty || double.IsNaN(seconds))
                return -1;

            long first = timestamps[0];
            long last = timestamps[timestamps.Length - 1];

            if (double.IsInfinity(seconds))
                return seconds > 0 ? last : first;

            double offset = Math.Round(seconds * highSpeedFactor * timestampsPerSecond);
            double timestamp = timeOrigin + offset;
            if (timestamp <= first)
                return first;
            if (timestamp >= last)
                return last;

            return (long)timestamp;
        }

        /// <summary>
        /// Convert a time coordinate, in seconds, to a video timestamp for a plot showing several timelines.
        /// The timelines must have been built with the same time origin and time scale (same video).
        /// The result is clamped to the union of the sample ranges: a time inside a gap between two
        /// non-overlapping timelines goes to the closest end of either of them.
        /// Returns -1 if there is no usable timeline or the time is NaN.
        /// </summary>
        public static long SecondsToTimestamp(IEnumerable<SpeedTimeline> timelines, double seconds)
        {
            if (timelines == null)
                throw new ArgumentNullException("timelines");

            long best = -1;
            double bestDistance = double.PositiveInfinity;
            foreach (SpeedTimeline timeline in timelines)
            {
                long candidate = timeline.SecondsToTimestamp(seconds);
                if (candidate < 0)
                    continue;

                if (double.IsInfinity(seconds))
                {
                    // Every candidate is infinitely far, keep the extreme one in the requested direction.
                    if (best < 0 || (seconds > 0 ? candidate > best : candidate < best))
                        best = candidate;

                    continue;
                }

                double distance = Math.Abs(timeline.TimestampToSeconds(candidate) - seconds);
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
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

        /// <summary>
        /// Merge several timelines into one table, for example to export them to a spreadsheet.
        /// Each row is [time in seconds, value of timeline 0, value of timeline 1, ...], sorted by time.
        /// A timeline without a sample at the time of the row has NaN in its column.
        /// The timelines must have been built with the same time origin and time scale (same video).
        /// </summary>
        public static List<double[]> MergeRows(IList<SpeedTimeline> timelines)
        {
            if (timelines == null)
                throw new ArgumentNullException("timelines");

            // Rows are keyed by timestamp rather than by seconds to avoid floating point mismatches.
            SortedDictionary<long, double[]> rows = new SortedDictionary<long, double[]>();
            for (int column = 0; column < timelines.Count; column++)
            {
                SpeedTimeline timeline = timelines[column];
                for (int i = 0; i < timeline.Count; i++)
                {
                    double[] row;
                    if (!rows.TryGetValue(timeline.timestamps[i], out row))
                    {
                        row = new double[timelines.Count + 1];
                        for (int j = 1; j < row.Length; j++)
                            row[j] = double.NaN;

                        row[0] = timeline.Times[i];
                        rows.Add(timeline.timestamps[i], row);
                    }

                    row[column + 1] = timeline.Values[i];
                }
            }

            return new List<double[]>(rows.Values);
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
