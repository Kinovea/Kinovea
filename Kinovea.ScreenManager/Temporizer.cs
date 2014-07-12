using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Kinovea.ScreenManager
{
    public class Temporizer
    {
        private Timer timer = new Timer();
        private Action action;

        public Temporizer(int delay, Action action)
        {
            this.action = action;
            timer.Interval = delay;
            timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            if (action != null)
                action();
        }

        public void Call()
        {
            timer.Stop();
            timer.Start();
        }
    }
}
