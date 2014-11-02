using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security;

namespace Kinovea.Tests
{
    public static class Performance
    {
        /// <summary>
        /// Unused - We don't use performance counters for now.
        /// </summary>
        public static void CreatePerfCounters()
        {
            const string CategoryName = "Kinovea";

            if (PerformanceCounterCategory.Exists(CategoryName))
                return;

            CounterCreationDataCollection counterDataCollection = new CounterCreationDataCollection();

            var grabFPSCounter = new CounterCreationData();
            grabFPSCounter.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
            grabFPSCounter.CounterName = "GrabFPS";
            counterDataCollection.Add(grabFPSCounter);

            try
            {
                // Create the category.
                PerformanceCounterCategory.Create(
                    CategoryName,
                    "Kinovea performance counters",
                    PerformanceCounterCategoryType.SingleInstance,
                    counterDataCollection);
            }
            catch (SecurityException)
            {
                throw;
            }
            
        }
    }
}
