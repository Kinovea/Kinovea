using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    // TODO: implement logarithmic mapping.

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
        private const double epsilon = 0.0001;

        // Slow motion slider input values.
        private int minInput = 0; 
        private int maxInput = 1000;

        // Slow motion factor values. 1 = file baseline.
        private double minSlowMotion = epsilon;
        private double maxSlowMotion = 2;

        private double fileInterval = 40; 
        private double userInterval = 40;
        private double captureInterval = 40;
        #endregion

        #region Public methods
        /// <summary>
        /// Set the range of slider input values.
        /// This should be directly coming from the UI.
        /// </summary>
        public void SetInputRange(int minInput, int maxInput)
        {
            this.minInput = minInput;
            this.maxInput = maxInput;
        }

        /// <summary>
        /// Set the range of allowed slow motion factor values. 
        /// 0 = extremely slow (clamped).
        /// Values above 1 are "fast motion" rather than slow motion.
        /// </summary>
        public void SetSlowMotionRange(double minSlowMotion, double maxSlowMotion)
        {
            this.minSlowMotion = Math.Max(epsilon, minSlowMotion);
            this.maxSlowMotion = maxSlowMotion;
        }

        /// <summary>
        /// Returns the frame interval in ms, to be used by the playback timer.
        /// </summary>
        public double GetInterval(int input)
        {
            double slowMotion = MapInput(input);
            return userInterval / slowMotion;
        }

        /// <summary>
        /// Returns the percentage of real-time, for information purposes.
        /// </summary>
        public double GetPercentage(int input)
        {
            double slowdownFactor = userInterval / captureInterval;
            
            double slowMotion = MapInput(input);
            double percentage = slowMotion * 100.0 / slowdownFactor;
            return percentage;
        }

        /// <summary>
        /// Returns the slider input corresponding to a specific slow motion factor.
        /// Used to draw tick marks.
        /// </summary>
        public int GetInputFromSlowMotion(double slowMotion)
        {
            int input = MapSlowMotion(slowMotion);
            return input;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Maps from slider input value to slow motion factor.
        /// </summary>
        private double MapInput(int input)
        {
            input = Math.Min(Math.Max(input, minInput), maxInput);
            return MapInputLinear(input);
        }

        /// <summary>
        /// Maps from slider input value to slow motion factor linearly.
        /// </summary>
        private double MapInputLinear(int input)
        {
            double inputNormalized = ((double)input - minInput) / ((double)maxInput - minInput);
            double result = minSlowMotion + (inputNormalized * (maxSlowMotion - minSlowMotion));
            return result;
        }

        private int MapSlowMotion(double slowMotion)
        {
            slowMotion = Math.Min(Math.Max(slowMotion, minSlowMotion), maxSlowMotion);
            return MapSlowMotionLinear(slowMotion);
        }

        private int MapSlowMotionLinear(double slowMotion)
        {
            double slowMotionNormalized = ((double)slowMotion - minSlowMotion) / ((double)maxSlowMotion - minSlowMotion);
            double result = minInput + (slowMotionNormalized * (maxInput - minInput));
            return (int)Math.Round(result);
        }
        #endregion

    }
}
