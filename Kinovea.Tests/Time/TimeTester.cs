using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager;

namespace Kinovea.Tests
{
    public class TimeTester
    {
        public void Test()
        {
            TimeMapper mapper = new TimeMapper();

            mapper.SetInputRange(0, 1000);
            mapper.SetSlowMotionRange(0, 2);
            mapper.FileInterval = 50;
            mapper.UserInterval = 25;
            mapper.CaptureInterval = 2.5;

            double t = 0;
            t = mapper.GetInterval(-1);
            t = mapper.GetInterval(0);
            t = mapper.GetInterval(10);
            t = mapper.GetInterval(100);
            t = mapper.GetInterval(500);
            t = mapper.GetInterval(999);
            t = mapper.GetInterval(1000);
            t = mapper.GetInterval(1001);

            t = mapper.GetPercentage(-1);
            t = mapper.GetPercentage(0);
            t = mapper.GetPercentage(10);
            t = mapper.GetPercentage(100);
            t = mapper.GetPercentage(500);
            t = mapper.GetPercentage(999);
            t = mapper.GetPercentage(1000);
            t = mapper.GetPercentage(1001);

            int i = 0;
            i = mapper.GetInputFromSlowMotion(-1);
            i = mapper.GetInputFromSlowMotion(0);
            i = mapper.GetInputFromSlowMotion(0.1);
            i = mapper.GetInputFromSlowMotion(0.5);
            i = mapper.GetInputFromSlowMotion(1);
            i = mapper.GetInputFromSlowMotion(1.5);
            i = mapper.GetInputFromSlowMotion(2);
            i = mapper.GetInputFromSlowMotion(2.1);
        }
    }
}
