using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class GridAxis
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }

        public GridAxis(PointF start, PointF end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
