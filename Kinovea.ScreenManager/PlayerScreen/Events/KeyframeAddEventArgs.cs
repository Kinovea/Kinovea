using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class KeyframeAddEventArgs : EventArgs
    {
        public long Time
        {
            get { return time; }
        }
        
        public string Name
        {
            get { return name; }
        }

        public Color Color
        {
            get { return color; }
        }

        private readonly long time;
        private readonly string name;
        private readonly Color color;
        public KeyframeAddEventArgs(long time, string name, Color color)
        {
            this.time = time;
            this.name = name;
            this.color = color;
        }
    }
}
