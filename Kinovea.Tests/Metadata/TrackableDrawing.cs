using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Tests
{
    // Only used in the context of Fuzzing.
    public class TrackableDrawing
    {
        public Guid DrawingId { get; private set; }
        public long Time { get; private set; }
        public List<string> PointKeys { get; private set; }

        public TrackableDrawing(Guid drawingId, long time, List<string> pointKeys)
        {
            this.DrawingId = drawingId;
            this.Time = time;
            this.PointKeys = pointKeys;
        }
    }
}
