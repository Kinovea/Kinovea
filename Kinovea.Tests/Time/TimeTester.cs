using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager;

namespace Kinovea.Tests
{
    public class TimeTester
    {
        public void TestTimeMapper()
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

            t = mapper.GetRealtimeMultiplier(-1);
            t = mapper.GetRealtimeMultiplier(0);
            t = mapper.GetRealtimeMultiplier(10);
            t = mapper.GetRealtimeMultiplier(100);
            t = mapper.GetRealtimeMultiplier(500);
            t = mapper.GetRealtimeMultiplier(999);
            t = mapper.GetRealtimeMultiplier(1000);
            t = mapper.GetRealtimeMultiplier(1001);

            double i = 0;
            i = mapper.GetInputFromSlowMotion(-1);
            i = mapper.GetInputFromSlowMotion(0);
            i = mapper.GetInputFromSlowMotion(0.1);
            i = mapper.GetInputFromSlowMotion(0.5);
            i = mapper.GetInputFromSlowMotion(1);
            i = mapper.GetInputFromSlowMotion(1.5);
            i = mapper.GetInputFromSlowMotion(2);
            i = mapper.GetInputFromSlowMotion(2.1);
        }
        
        public void TestSliderSpeed()
        {
            SliderLinear s = new SliderLinear();
            s.Minimum = 0;
            s.Maximum = 1000;
            s.Value = 500;
            
            double v = 0;
            s.StepJump(0.1);
            s.StepJump(0.1);
            s.StepJump(0.1);

            s.StepJump(-0.2);
            s.StepJump(-0.25);

            s.Value = 0;
             s.StepJump(-0.1);

            s.Value = 5;
            s.StepJump(-0.1);

            s.Value = 1000;
            s.StepJump(0.1);

            s.Value = 995;
            s.StepJump(0.1);
        }
    }
}
