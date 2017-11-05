using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Moving average filter.
    /// </summary>
    public class MovingAverage
    {
        /// <summary>
        /// Filter a list of samples by averaging over "span" duration.
        /// </summary>
        public double[] FilterSamples(double[] samples, double fs, double span, int sentinels)
        {
            double interval = 1000 / fs;
            
            if (span / 2 <= interval)
                return samples;

            int padding = 50;
            padding = Math.Max(0, Math.Min(padding, samples.Length - (sentinels*4)));
            double[] padded = AddPadding(samples, padding, sentinels);

            int frames = (int)(span / (2 * interval));

            // TODO: combine all inner loops into one.
            double[] smoothed = new double[padded.Length];
            for (int i = 0; i < padded.Length; i++)
            {
                int first = Math.Max(0, i - frames);
                int last = Math.Min(padded.Length - 1, i + frames);
                int count = last - first + 1;

                double total = 0;
                for (int j = first; j <= last; j++)
                    total += padded[j];

                smoothed[i] = total / count;
            }

            return RemovePadding(smoothed, padding);
        }

        private double[] AddPadding(double[] samples, int padding, int sentinels)
        {
            // Extrapolation of trajectory using reflection of values around the end points.
            // Ref: "Padding point extrapolation techniques for the butterworth digital filter". Smith 1989.

            double[] padded = new double[samples.Length + 2 * padding];
            int pivot = sentinels;
            for (int i = 0; i < padding + sentinels; i++)
                padded[i] = samples[pivot] + (samples[pivot] - samples[pivot + padding - i + sentinels]);

            for (int i = sentinels; i < samples.Length - sentinels; i++)
                padded[padding + i] = samples[i];

            pivot = samples.Length - 1 - sentinels;
            for (int i = 0; i < padding + sentinels; i++)
                padded[padding + samples.Length - sentinels + i] = samples[pivot] + (samples[pivot] - samples[pivot - i - 1]);

            return padded;
        }

        private double[] RemovePadding(double[] samples, int padding)
        {
            double[] result = new double[samples.Length - 2 * padding];
            for (int i = padding; i < samples.Length - padding; i++)
                result[i - padding] = samples[i];

            return result;
        }
    }
}
