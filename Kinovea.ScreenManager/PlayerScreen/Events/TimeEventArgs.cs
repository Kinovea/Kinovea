using System;

namespace Kinovea.ScreenManager
{
    public class TimeEventArgs : EventArgs
    {
        public long Time
        {
            get { return time; }
        }

        private readonly long time;
        
        public TimeEventArgs(long time)
        {
            this.time = time;
        }
    }
}
