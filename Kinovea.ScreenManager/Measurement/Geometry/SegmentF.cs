using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public struct SegmentF
    {
        public PointF Start;
        public PointF End;

        public static readonly SegmentF Empty = new SegmentF(PointF.Empty, PointF.Empty);

        public SegmentF(PointF start, PointF end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
