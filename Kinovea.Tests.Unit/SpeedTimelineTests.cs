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
        public void SecondsToTimestamp_IsInverseOfTimestampToSeconds()
        {
            long[] timestamps = { 1000, 1500, 2000, 2500, 3000 };
            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, new[] { 1.0, 1.0, 1.0, 1.0, 1.0 }, 1200, 1000, 1);

            foreach (long t in timestamps)
                Assert.Equal(t, timeline.SecondsToTimestamp(timeline.TimestampToSeconds(t)));
        }

        [Fact]
        public void SecondsToTimestamp_RoundsToNearestTimestamp()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 100 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(42, timeline.SecondsToTimestamp(0.0421));
            Assert.Equal(43, timeline.SecondsToTimestamp(0.0426));
        }

        [Fact]
        public void SecondsToTimestamp_TakesHighSpeedFactorIntoAccount()
        {
            // 240 fps capture encoded at 30 fps: 1 s of real time = 8 s of video.
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 16000 }, new[] { 1.0, 1.0 }, 0, 1000, 8);

            Assert.Equal(8000, timeline.SecondsToTimestamp(1.0));
        }

        [Fact]
        public void SecondsToTimestamp_ClampsToSampleRange()
        {
            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 1000, 2000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(1000, timeline.SecondsToTimestamp(-3.0));
            Assert.Equal(2000, timeline.SecondsToTimestamp(10.0));
            Assert.Equal(1000, timeline.SecondsToTimestamp(double.NegativeInfinity));
            Assert.Equal(2000, timeline.SecondsToTimestamp(double.PositiveInfinity));
        }

        [Fact]
        public void SecondsToTimestamp_IgnoresDroppedSamplesForRange()
        {
            // The first and last samples have no speed (typical at the ends of a filtered track):
            // seeking must stay within the visible part of the curve.
            long[] timestamps = { 0, 1000, 2000, 3000 };
            double[] speeds = { double.NaN, 1.0, 2.0, double.NaN };
            SpeedTimeline timeline = SpeedTimeline.Build(timestamps, speeds, 0, 1000, 1);

            Assert.Equal(1000, timeline.SecondsToTimestamp(0.0));
            Assert.Equal(2000, timeline.SecondsToTimestamp(3.0));
        }

        [Fact]
        public void SecondsToTimestamp_EmptyOrNaNReturnsMinusOne()
        {
            Assert.Equal(-1, SpeedTimeline.Empty().SecondsToTimestamp(1.0));

            SpeedTimeline timeline = SpeedTimeline.Build(new long[] { 0, 1000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            Assert.Equal(-1, timeline.SecondsToTimestamp(double.NaN));
        }

        [Fact]
        public void SecondsToTimestampMulti_UsesTimelineContainingTheTime()
        {
            SpeedTimeline a = SpeedTimeline.Build(new long[] { 0, 1000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            SpeedTimeline b = SpeedTimeline.Build(new long[] { 3000, 5000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(500, SpeedTimeline.SecondsToTimestamp(new[] { a, b }, 0.5));
            Assert.Equal(4000, SpeedTimeline.SecondsToTimestamp(new[] { a, b }, 4.0));
        }

        [Fact]
        public void SecondsToTimestampMulti_ClampsToUnionOfRanges()
        {
            SpeedTimeline a = SpeedTimeline.Build(new long[] { 1000, 2000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            SpeedTimeline b = SpeedTimeline.Build(new long[] { 3000, 5000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            SpeedTimeline[] both = { a, b };

            Assert.Equal(1000, SpeedTimeline.SecondsToTimestamp(both, 0.0));
            Assert.Equal(5000, SpeedTimeline.SecondsToTimestamp(both, 9.0));
            Assert.Equal(1000, SpeedTimeline.SecondsToTimestamp(both, double.NegativeInfinity));
            Assert.Equal(5000, SpeedTimeline.SecondsToTimestamp(both, double.PositiveInfinity));
        }

        [Fact]
        public void SecondsToTimestampMulti_GapGoesToClosestEnd()
        {
            SpeedTimeline a = SpeedTimeline.Build(new long[] { 0, 1000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            SpeedTimeline b = SpeedTimeline.Build(new long[] { 3000, 5000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);
            SpeedTimeline[] both = { a, b };

            Assert.Equal(1000, SpeedTimeline.SecondsToTimestamp(both, 1.4));
            Assert.Equal(3000, SpeedTimeline.SecondsToTimestamp(both, 2.6));
        }

        [Fact]
        public void SecondsToTimestampMulti_SkipsEmptyTimelines()
        {
            SpeedTimeline a = SpeedTimeline.Build(new long[] { 0, 1000 }, new[] { 1.0, 1.0 }, 0, 1000, 1);

            Assert.Equal(500, SpeedTimeline.SecondsToTimestamp(new[] { SpeedTimeline.Empty(), a }, 0.5));
            Assert.Equal(-1, SpeedTimeline.SecondsToTimestamp(new[] { SpeedTimeline.Empty() }, 0.5));
            Assert.Equal(-1, SpeedTimeline.SecondsToTimestamp(new SpeedTimeline[0], 0.5));
            Assert.Equal(-1, SpeedTimeline.SecondsToTimestamp(new[] { a }, double.NaN));
        }
    }
}
