using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The time mapper links the speed slider, the playback frame rate and the capture frame rate.
    /// </summary>
    public class TimeMapper
    {
        #region Properties
        /// <summary>
        /// The interval between frames (ms) for playback purposes specified in the file.
        /// </summary>
        public double FileInterval 
        {
            get { return fileInterval; }
            set { fileInterval = value; }
        }

        /// <summary>
        /// The interval between frames (ms) for playback purposes specified by the user.
        /// </summary>
        public double UserInterval
        {
            get { return userInterval; }
            set { userInterval = value; }
        }

        /// <summary>
        /// The interval of real time (ms) between captured frames, specified by the user.
        /// </summary>
        public double CaptureInterval
        {
            get { return captureInterval; }
            set { captureInterval = value; }
        }
        #endregion

        #region Members
        private const double epsilon = 1e-3;

        // Slider input values.
        // This is an arbitrary range.
        private double minInput = 0; 
        private double maxInput = 1000;
        private double midInput = 500;
        private double safeMinInput = 1;

        // Speed factor values. 1 = file baseline.
        private double minFactor = 0;
        private double maxFactor = 10;
        private double midFactor = 1;
        private double safeMinFactor = 0.002;

        private double fileInterval = 40; 
        private double userInterval = 40;
        private double captureInterval = 40;
        #endregion

        #region Public methods

        /// <summary>
        /// Initialize all values.
        /// </summary>
        public void Initialize(double minInput, double maxInput, double midInput, double minFactor, double maxFactor, double midFactor)
        {
            this.minInput = minInput;
            this.maxInput = maxInput;
            this.midInput = midInput;
            this.safeMinInput = minInput + (epsilon * (maxInput - minInput));

            this.minFactor = minFactor;
            this.maxFactor = maxFactor;
            this.midFactor = midFactor;
            this.safeMinFactor = minFactor + (epsilon * (maxFactor - minFactor));
        }

        /// <summary>
        /// Returns the speed factor corresponding to the input.
        /// </summary>
        public double GetSpeedFactor(double input)
        {
            return MapInput(input);
        }

        /// <summary>
        /// Returns the frame interval in ms, to be used by the playback timer.
        /// </summary>
        public double GetInterval(double input)
        {
            double factor = MapInput(input);
            return userInterval / factor;
        }

        /// <summary>
        /// Returns the fraction of real-time speed of the given input, for information purposes.
        /// Used to display percentage or multiplier of real-time.
        /// </summary>
        public double GetRealtimeMultiplier(double input)
        {
            double realtimeFactor = userInterval / captureInterval;
            double speedFactor = MapInput(input);
            return speedFactor / realtimeFactor;
        }

        /// <summary>
        /// Returns the slider input corresponding to a specific slow motion factor.
        /// Used to draw tick marks.
        /// </summary>
        public double GetInputFromSpeedFactor(double speedFactor)
        {
            return MapSpeedFactor(speedFactor);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Maps from slider input value to speed factor.
        /// </summary>
        private double MapInput(double input)
        {
            input = Math.Min(Math.Max(input, minInput), maxInput);
            return MapInputPiecewise(input);
        }

        /// <summary>
        /// Maps from speed factor to slider input value.
        /// </summary>
        private double MapSpeedFactor(double speedFactor)
        {
            speedFactor = Math.Min(Math.Max(speedFactor, minFactor), maxFactor);
            return MapSpeedFactorPiecewise(speedFactor);
        }
        #endregion

        #region Core mapping functions
        /// <summary>
        /// Slider input -> speed factor.
        /// </summary>
        private double MapInputLinear(double input)
        {
            double inputNormalized = (input - minInput) / (maxInput - minInput);
            double result = minFactor + (inputNormalized * (maxFactor - minFactor));
            return Math.Max(result, safeMinFactor);
        }

        /// <summary>
        /// Slider input -> speed factor.
        /// </summary>
        private double MapInputPiecewise(double input)
        {
            if (input < midInput)
            {
                double inputNormalized = (input - minInput) / (midInput - minInput);
                double result = minFactor + (inputNormalized * (midFactor - minFactor));
                return Math.Max(result, safeMinFactor);
            }
            else
            {
                double inputNormalized = (input - midInput) / (maxInput - midInput);
                double result = midFactor + (inputNormalized * (maxFactor - midFactor));
                return Math.Max(result, safeMinFactor);
            }
        }

        /// <summary>
        /// Speed factor -> slider input.
        /// </summary>
        private double MapSpeedFactorLinear(double speedFactor)
        {
            double speedFactorNormalized = (speedFactor - minFactor) / (maxFactor - minFactor);
            double result = minInput + (speedFactorNormalized * (maxInput - minInput));
            return Math.Max(result, safeMinInput);
        }


        /// <summary>
        /// Speed factor -> slider input.
        /// </summary>
        private double MapSpeedFactorPiecewise(double speedFactor)
        {
            if (speedFactor < midFactor)
            {
                double speedFactorNormalized = (speedFactor - minFactor) / (midFactor - minFactor);
                double result = minInput + (speedFactorNormalized * (midInput - minInput));
                return Math.Max(result, safeMinInput);
            }
            else
            {
                double speedFactorNormalized = (speedFactor - midFactor) / (maxFactor - midFactor);
                double result = midInput + (speedFactorNormalized * (maxInput - midInput));
                return Math.Max(result, safeMinInput);
            }
        }


        #endregion

    }
}
