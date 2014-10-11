using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Holds description of the coordinate system grid boundaries in image space.
    /// </summary>
    public class CoordinateSystemGrid
    {
        public List<GridLine> GridLines { get; private set; }
        public List<TickMark> TickMarks { get; private set; }
        public GridAxis HorizontalAxis { get; set; }
        public GridAxis VerticalAxis { get; set; }

        public CoordinateSystemGrid()
        {
            GridLines = new List<GridLine>();
            TickMarks = new List<TickMark>();
        }

    }
}
