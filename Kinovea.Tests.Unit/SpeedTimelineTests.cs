using System;
using Kinovea.ScreenManager;
using Xunit;

namespace Kinovea.Tests.Unit
{
    public class SpeedTimelineTests
    {
        [Fact]
        public void Build_ConvertsTimestampsToSecondsRelativeToOrigin()
        {
            long[] timestamps = { 1000, 2000, 3000 };
            double[] speeds = { 1.0, 2.0, 3.0 };

            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, speeds, 1000, 1000, 1);

            Assert.Equal(new[] { 0.0, 1.0, 2.0 }, timeline.Times);
            Assert.Equal(speeds, timeline.Values);
        }

        [Fact]
        public void Build_TimesBeforeOriginAreNegative()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 500 }, new[] { 1.0, 1.0 }, 1000, 1000, 1);

            Assert.Equal(new[] { -1.0, -0.5 }, timeline.Times);
        }

        [Fact]
        public void Build_HighSpeedFactorScalesTimeToRealTime()
        {
            // A video filmed at 240 fps and encoded at 30 fps: 1 s of video = 1/8 s of real time.
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 8000 }, new[] { 1.0, 1.0 }, 0, 1000, 8);

            Assert.Equal(new[] { 0.0, 1.0 }, timeline.Times);
        }

        [Fact]
        public void Build_DropsNonFiniteSpeeds()
        {
            long[] timestamps = { 0, 1, 2, 3, 4 };
            double[] speeds = { double.NaN, 1.0, double.PositiveInfinity, 2.0, double.NegativeInfinity };

            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, speeds, 0, 1, 1);

            Assert.Equal(new[] { 1.0, 3.0 }, timeline.Times);
            Assert.Equal(new[] { 1.0, 2.0 }, timeline.Values);
        }

        [Fact]
        public void Build_EmptyInputGivesEmptyTimeline()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[0], new double[0], 0, 1000, 1);

            Assert.True(timeline.IsEmpty);
            Assert.Equal(0, timeline.Count);
        }

        [Fact]
        public void Build_AllNaNGivesEmptyTimeline()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 1 }, new[] { double.NaN, double.NaN }, 0, 1000, 1);

            Assert.True(timeline.IsEmpty);
        }

        [Fact]
        public void Build_RejectsMismatchedLengths()
        {
            Assert.Throws<ArgumentException>(() => SpeedTimeline.Build(new long[] { 0, 1 }, new[] { 1.0 }, 0, 1000, 1));
        }

        [Fact]
        public void Build_RejectsNullInputs()
        {
            Assert.Throws<ArgumentNullException>(() => SpeedTimeline.Build(null, new double[0], 0, 1000, 1));
            Assert.Throws<ArgumentNullException>(() => SpeedTimeline.Build(new long[0], null, 0, 1000, 1));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void Build_RejectsInvalidTimestampsPerSecond(double timestampsPerSecond)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SpeedTimeline.Build(new long[0], new double[0], 0, timestampsPerSecond, 1));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-2.0)]
        [InlineData(double.NaN)]
        public void Build_RejectsInvalidHighSpeedFactor(double highSpeedFactor)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SpeedTimeline.Build(new long[0], new double[0], 0, 1000, highSpeedFactor));
        }

        [Fact]
        public void TimestampToSeconds_UsesSameReferenceAsCurve()
        {
            long[] timestamps = { 1500, 2500, 3500 };
            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, new[] { 1.0, 2.0, 3.0 }, 500, 1000, 2);

            for (int i = 0; i < timestamps.Length; i++)
                Assert.Equal(timeline.Times[i], timeline.TimestampToSeconds(timestamps[i]));
        }

        [Fact]
        public void TimestampToSeconds_WorksOutsideTrackRange()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 1000, 2000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(5.0, timeline.TimestampToSeconds(5000));
            Assert.Equal(0.0, timeline.TimestampToSeconds(0));
        }

        [Fact]
        public void Empty_IsEmpty()
        {
            Assert.True(SpeedTimeline.Empty().IsEmpty);
        }

        [Fact]
        public void SecondsToTimestamp_InRange_RoundsToNearestTimestamp()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 100 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(42, timeline.SecondsToTimestamp(0.0421, 0, 1000));
            Assert.Equal(43, timeline.SecondsToTimestamp(0.0426, 0, 1000));
        }

        [Fact]
        public void Statistics_MeanMaxMinAndTheirTimes()
        {
            long[] timestamps = { 0, 1000, 2000, 3000 };
            double[] speeds = { 1.0, 3.0, 0.5, 1.5 };
            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, speeds, 0, 1000, 1);

            Assert.Equal(1.5, timeline.Mean, 10);
            Assert.Equal(3.0, timeline.Maximum);
            Assert.Equal(1.0, timeline.MaximumTime);
            Assert.Equal(0.5, timeline.Minimum);
            Assert.Equal(2.0, timeline.MinimumTime);
        }

        [Fact]
        public void Statistics_IgnoreDroppedSamples()
        {
            long[] timestamps = { 0, 1000, 2000 };
            double[] speeds = { double.NaN, 2.0, 4.0 };
            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, speeds, 0, 1000, 1);

            Assert.Equal(3.0, timeline.Mean, 10);
            Assert.Equal(2.0, timeline.Minimum);
            Assert.Equal(1.0, timeline.MinimumTime);
        }

        [Fact]
        public void Statistics_FirstOccurrenceWinsOnTies()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 1000, 2000 }, new[] { 2.0, 2.0, 2.0 }, 0, 1000, 1);

            Assert.Equal(0.0, timeline.MaximumTime);
            Assert.Equal(0.0, timeline.MinimumTime);
        }

        [Fact]
        public void Statistics_EmptyIsNaN()
        {
            SpeedTimeline timeline = SpeedTimeline.Empty();

            Assert.True(double.IsNaN(timeline.Mean));
            Assert.True(double.IsNaN(timeline.Maximum));
            Assert.True(double.IsNaN(timeline.MaximumTime));
            Assert.True(double.IsNaN(timeline.Minimum));
            Assert.True(double.IsNaN(timeline.MinimumTime));
        }

        [Fact]
        public void ValueAt_ExactSampleAndInterpolation()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 1000, 2000 }, new[] { 1.0, 3.0, 2.0 }, 0, 1000, 1);

            Assert.Equal(3.0, timeline.ValueAt(1.0));
            Assert.Equal(2.0, timeline.ValueAt(0.5), 10);
            Assert.Equal(2.5, timeline.ValueAt(1.5), 10);
            Assert.Equal(1.0, timeline.ValueAt(0.0));
            Assert.Equal(2.0, timeline.ValueAt(2.0));
        }

        [Fact]
        public void ValueAt_OutsideRangeOrEmptyIsNaN()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 1000, 2000 }, new[] { 1.0, 2.0 }, 0, 1000, 1);

            Assert.True(double.IsNaN(timeline.ValueAt(0.5)));
            Assert.True(double.IsNaN(timeline.ValueAt(2.5)));
            Assert.True(double.IsNaN(timeline.ValueAt(double.NaN)));
            Assert.True(double.IsNaN(SpeedTimeline.Empty().ValueAt(0)));
        }

        [Fact]
        public void SecondsToTimestamp_InRange_GoesOutsideTrackSamples()
        {
            var t = SpeedTimeline.Build(new long[] { 2000, 3000 }, new[] { 1.0, 2.0 }, 1000, 1000, 1);
            Assert.Equal(500, t.SecondsToTimestamp(-0.5, 0, 10000));
            Assert.Equal(6000, t.SecondsToTimestamp(5.0, 0, 10000));
        }

        [Fact]
        public void SecondsToTimestamp_InRange_ClampsToRange()
        {
            var t = SpeedTimeline.Build(new long[] { 2000, 3000 }, new[] { 1.0, 2.0 }, 1000, 1000, 1);
            Assert.Equal(1500, t.SecondsToTimestamp(-10, 1500, 4000));
            Assert.Equal(4000, t.SecondsToTimestamp(10, 1500, 4000));
            Assert.Equal(4000, t.SecondsToTimestamp(double.PositiveInfinity, 1500, 4000));
            Assert.Equal(1500, t.SecondsToTimestamp(double.NegativeInfinity, 1500, 4000));
        }

        [Fact]
        public void SecondsToTimestamp_InRange_RoundTripsWithHighSpeedFactorAndOrigin()
        {
            var t = SpeedTimeline.Build(new long[] { 5000, 6000 }, new[] { 1.0, 2.0 }, 4000, 1000, 4);
            foreach (long ts in new long[] { 0, 3999, 4000, 4321, 9000 })
                Assert.Equal(ts, t.SecondsToTimestamp(t.TimestampToSeconds(ts), 0, 10000));
        }

        [Fact]
        public void SecondsToTimestamp_InRange_InvalidInputsReturnMinusOne()
        {
            var t = SpeedTimeline.Build(new long[] { 2000, 3000 }, new[] { 1.0, 2.0 }, 1000, 1000, 1);
            Assert.Equal(-1, t.SecondsToTimestamp(double.NaN, 0, 10000));
            Assert.Equal(-1, t.SecondsToTimestamp(1.0, 5000, 4000));
            Assert.Equal(-1, SpeedTimeline.Empty().SecondsToTimestamp(1.0, 0, 10000));
        }
    }
}
