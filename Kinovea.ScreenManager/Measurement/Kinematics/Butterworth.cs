#region License
/*
Copyright © Joan Charmant 2013.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Butterworth low-pass filter. Fourth order, zero-lag low pass filter. Two 2nd order passes (forward/backward) are used to reset the phase shift.
    /// Ref: "Biomechanics and Motor control of human movement", David A. Winter, 4th ed. 2009.
    /// 
    /// To initialize the filter the trajectory is extrapolated for 10 data points each side using reflected values around the end points.
    /// The extrapolated points are then removed from the filtered results.
    /// Ref: "Padding point extrapolation techniques for the butterworth digital filter", Gerald Smith, J. Biomechonics Vol. 22, No. s/9, pp. 967-971, 1989.
    ///
    /// The filter is passed at various cutoff frequencies between 0.5Hz and the Nyquist frequency, and all results are kept.
    /// 
    /// The best-guess cutoff frequency is found by estimating autocorrelation of residuals and taking the frequency yielding the least autocorrelated residuals.
    /// Ref: "A procedure for the automatic determination of filter cutoff frequency for the processing of biomechanical data", John Challis, JAB, 1999, 15, 303-317.
    ///
    /// The autocorrelation of residuals is estimated using the Durbin-Watson statistic.
    /// </summary>
    public class ButterworthFilter
    {
        private double a0, a1, a2;
        private double b1, b2;
        private double x0, x1, x2, y0, y1, y2; 
        private double correctionFactor;
        private const double SQRT2 = 1.4142135623730950488;

        /// <summary>
        /// Filter a list of samples and return a list of lists of filtered values at various test cutoff frequencies.
        /// The number of samples must be >= 10.
        /// </summary>
        /// <param name="samples">The raw values to be filtered</param>
        /// <param name="fs">The sampling frequency</param>
        /// <param name="fcTests">The number of cutoff frequencies to test. These will span between 0.5Hz and the Nyquist frequency.</param>
        /// <param name="bestCutoff">The best guess cutoff frequency found at minimal autocorrelation of residuals.</param>
        public List<FilteringResult> FilterSamples(double[] samples, double fs, int fcTests, out int bestCutoffIndex)
        {
            if (samples.Length <= 10)
                throw new ArgumentException("Number of samples must be superior to 10");

            List<FilteringResult> results = new List<FilteringResult>();

            double nyquist = fs / 2;
            UpdateCorrectionFactor(2);

            int padding = 10;
            double[] padded = AddPadding(samples, padding);

            // Compute filtered result for a range of fc.
            // We keep the whole array of results so the user can manually change cutoff frequency without a 
            // complete recomputation, and it is also necessary to compute the best-guess value.
            double min = 0.5;
            double max = nyquist;
            double step = (max - min) / fcTests;
            double bestScore = 1;
            int bestIndex = -1;
            int index = 0;
            for (double fc = min; fc < max; fc += step)
            {
                double[] filteredPadded = FilterSamples(padded, fs, fc);
                double[] filtered = RemovePadding(filteredPadded, padding);

                double[] residuals = samples.Subtract(filtered);
                double dw = StatsHelper.DurbinWatson(residuals);
                if (double.IsNaN(dw))
                  continue;

                double dwNormalized = Math.Abs(2 - dw) / 2;

                FilteringResult result = new FilteringResult(fc, filtered, dwNormalized);
                results.Add(result);

                if (dwNormalized < bestScore)
                {
                    bestScore = dwNormalized;
                    bestIndex = index;
                }

                index++;
            }

            bestCutoffIndex = bestIndex;

            return results;
        }

        private double[] AddPadding(double[] samples, int padding)
        {
            // Extrapolation of trajectory using reflection of values around the end points.
            // Ref: "Padding point extrapolation techniques for the butterworth digital filter". Smith 1989.

            double[] padded = new double[samples.Length + 2 * padding];
            for (int i = 0; i < padding; i++)
                padded[i] = samples[0] + samples[0] - samples[padding - i];

            for (int i = 0; i < samples.Length; i++ )
                padded[padding + i] = samples[i];

            for (int i = 0; i < padding; i++)
                padded[padding + samples.Length + i] = samples[samples.Length - 1] + samples[samples.Length - 1] - samples[samples.Length - 1 - i - 1];

            return padded;
        }

        private double[] RemovePadding(double[] samples, int padding)
        {
            double[] result = new double[samples.Length - 2 * padding];
            for (int i = padding; i < samples.Length - padding; i++)
                result[i - padding] = samples[i];
            
            return result;
        }

        private double[] FilterSamples(double[] samples, double fs, double fc)
        {
            UpdateCoefficients(fs, fc);
            double[] forward = ForwardPass(samples);
            double[] backward = BackwardPass(forward);
            return backward;
        }
        
        private void UpdateCorrectionFactor(int passes)
        {
            // Ref: Chapt. 3.4.4.2 of "Biomechanics and motor control of human movement".
            correctionFactor = Math.Pow((Math.Pow(2, 1.0 / passes) - 1), 0.25);
        }

        private void UpdateCoefficients(double fs, double fc)
        {
            // Ref: Chapt. 2.2.4.4 of "Biomechanics and motor control of human movement".
            double o = Math.Tan(Math.PI * fc / fs) / correctionFactor;
            double k1 = SQRT2 * o;
            double k2 = o * o;

            a0 = k2 / (1 + k1 + k2);
            a1 = 2 * a0;
            a2 = a0;

            double k3 = 2 * a0 / k2;
            b1 = -2 * a0 + k3;
            b2 = 1 - a0 - a1 - a2 - b1;
        }

        private void ResetValues()
        {
            x0 = x1 = x2 = y0 = y1 = y2 = 0;
        }

        private double[] ForwardPass(double[] raw)
        {
            ResetValues();

            y1 = x1 = raw[0];
            y0 = x0 = raw[1];

            double[] filtered = new double[raw.Length];
            filtered[0] = y1;
            filtered[1] = y0;
            for (int i = 2; i < raw.Length; i++)
            {
                double f = FilterSample(raw[i]);
                filtered[i] = f;
            }

            return filtered;
        }

        private double[] BackwardPass(double[] forward)
        {
            ResetValues();

            y1 = x1 = forward[forward.Length - 1];
            y0 = x0 = forward[forward.Length - 2];

            double[] filtered = new double[forward.Length];
            filtered[0] = y1;
            filtered[1] = y0;
            for (int i = forward.Length - 3; i >= 0; i--)
            {
                double f = FilterSample(forward[i]);
                filtered[forward.Length - 1 - i] = f;
            }

            Array.Reverse(filtered);
            return filtered;
        }

        private double FilterSample(double sample)
        {
            x2 = x1;
            x1 = x0;
            x0 = sample;
            y2 = y1;
            y1 = y0;
            
            y0 = a0 * x0 + a1 * x1 + a2 * x2 + b1 * y1 + b2 * y2;

            return y0;
        }
    }
}
