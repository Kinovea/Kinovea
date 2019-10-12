using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;

namespace Kinovea.Tests
{
    public class TimecodeFormatTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Test()
        {
            List<long> a = new List<long>() {
                0,
                1,
                10,
                999,
                1000, // 1 second.
                1001,
                1009,
                9999,
                59999,
                60000, // 1 minute.
                60100,
                60999,
                3599999,
                3600000, // 1 hour.
                36000000, // 10 hours.
            };

            foreach (long value in a)
            {
                log.Debug("------------------");
                TestValue(value, true);
                TestValue(value, false);
                TestValue(-value, true);
                TestValue(-value, false);
            }
        }

        private void TestValue(long value, bool thousandth)
        {
            string timecode = TimeHelper.MillisecondsToTimecode(value, thousandth);
            log.DebugFormat("Input: {0} ms, Output:{1}. Thousandth:{2}.", value, timecode, thousandth);
        }
    }
}
