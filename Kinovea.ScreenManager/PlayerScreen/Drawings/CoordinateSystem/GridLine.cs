using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class GridLine
    {
        public bool IsAxis { get; set; }
        public PointF Start { get; set; }
        public PointF End { get; set; }

        public GridLine(PointF start, PointF end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
