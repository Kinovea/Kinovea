using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public static class RangeHelper
    {
        /// <summary>
        /// Finds step size based on desired number of steps so that the end result is user friendly.
        /// </summary>
        public static float FindUsableStepSize(float range, float targetSteps)
        {
            float minimum = range / targetSteps;

            // Find magnitude of the initial guess.
            float magnitude = (float)Math.Floor(Math.Log10(minimum));
            float orderOfMagnitude = (float)Math.Pow(10, magnitude);

            // Reduce the number of steps.
            float residual = minimum / orderOfMagnitude;
            float stepSize;

            if (residual > 5)
                stepSize = 10 * orderOfMagnitude;
            else if (residual > 2)
                stepSize = 5 * orderOfMagnitude;
            else if (residual > 1)
                stepSize = 2 * orderOfMagnitude;
            else
                stepSize = orderOfMagnitude;

            return stepSize;
        }
    }
}
