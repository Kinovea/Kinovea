using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Abstraction of the concept of a container of drawings.
    /// Examples: Keyframe, ChronoManager.
    /// </summary>
    public abstract class AbstractDrawingManager
    {
        public abstract Guid Id { get; set; }

        public abstract AbstractDrawing GetDrawing(Guid id);
        public abstract void AddDrawing(AbstractDrawing drawing);
        public abstract void RemoveDrawing(Guid id);
    }
}
