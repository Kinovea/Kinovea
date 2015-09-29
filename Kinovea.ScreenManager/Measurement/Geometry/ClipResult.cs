using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class ClipResult
    {
        public bool Visible { get; private set; }
        public PointF A { get; private set; }
        public PointF B { get; private set; }

        public ClipResult(bool visible, PointF a, PointF b)
        {
            this.Visible = visible;
            this.A = a;
            this.B = b;
        }

        public static ClipResult Invisible
        {
            get { return new ClipResult(false, PointF.Empty, PointF.Empty); }
        }
    }
}
