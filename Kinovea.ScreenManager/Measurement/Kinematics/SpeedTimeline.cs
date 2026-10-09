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
