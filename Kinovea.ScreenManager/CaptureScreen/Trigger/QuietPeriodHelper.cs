using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Static helper class to manage the quiet period.
    /// The quiet period is global to the application and all trigger methods.
    /// </summary>
    public static class QuietPeriodHelper
    {
        private static DateTime quietPeriodStart = DateTime.MinValue;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reset the start of the quiet period for all trigger monitors.
        /// </summary>
        public static void StartQuietPeriod()
        {
            log.DebugFormat("Entering quiet period");
            quietPeriodStart = DateTime.Now;
        }

        /// <summary>
        /// Return the remaining time within the quiet period, as the ratio 
        /// of the ellapsed time to the configured time.
        /// 0.0 means the start of the quiet period.
        /// 1.0 or more means we are past the quiet period.
        /// If the quiet period is not active, returns 1.0f.
        /// </summary>
        public static float QuietProgress()
        {
            float quietPeriod = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerQuietPeriod;
            float ellapsed = (float)(DateTime.Now - quietPeriodStart).TotalSeconds;
            if (quietPeriod == 0)
                return 1.0f;

            return ellapsed / quietPeriod;
        }

        public static bool IsQuiet()
        {
            return QuietProgress() < 1.0f;
        }
    }
}
