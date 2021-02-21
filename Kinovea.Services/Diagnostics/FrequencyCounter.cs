using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kinovea.Services
{
    /// <summary>
    /// Computes the frequency of a signal using a averaging window to get primary values, 
    /// and optionnally an exponential moving average over the primary values for smoothing.
    /// The averaging window should be aligned on input data cycles if any. 
    /// For example, DirectShow number of buffers when measuring framerates.
    /// </summary>
    public class FrequencyCounter
    {
        public double Frequency
        {
            get { return frequency; }
        }

        private int window; // number of intervals that are averaged to get a primary data point.
        private int ignoredIntervals; // number of intervals ignored from the start of the stream.
        private bool useExponentialMovingAverage = true;

        private long lastTick;
        private long received = 0;
        private long runningTotal = 0;
        private double frequency = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private Averager averager = new Averager(0.1);
        
        public FrequencyCounter(int window, int ignoredIntervals, bool useExponentialMovingAverage)
        {
            this.window = window;
            this.ignoredIntervals = ignoredIntervals;
            this.useExponentialMovingAverage = useExponentialMovingAverage;

            stopwatch.Start();
        }

        public void Tick()
        {
            received++;
            long now = stopwatch.ElapsedTicks;

            if (received == 1)
            {
                lastTick = now;
                return;
            }

            long interval = now - lastTick;
            lastTick = now;
            long receivedIntervals = received - 1;

            if (receivedIntervals <= ignoredIntervals)
                return;

            long countedIntervals = receivedIntervals - ignoredIntervals;

            runningTotal += interval;

            if (countedIntervals % window != 0)
                return;

            // An averaging cycle is complete. 
            double average = (double)runningTotal / window;

            if (useExponentialMovingAverage)
            {
                averager.Post(average);
                frequency = (double)Stopwatch.Frequency / averager.Average;
            }
            else
            {
                frequency = (double)Stopwatch.Frequency / average;
            }

            runningTotal = 0;
        }

        public void Reset()
        {
            received = 0;
            frequency = 0;
            runningTotal = 0;
        }
    }
}
