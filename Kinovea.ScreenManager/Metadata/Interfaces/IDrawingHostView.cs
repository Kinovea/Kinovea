using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public interface IDrawingHostView
    {
        long CurrentTimestamp { get; }

        Bitmap CurrentImage { get; }

        void InvalidateFromMenu();
        void InitializeEndFromMenu(bool cancelLastPoint);
        void UpdateFramesMarkers();
    }
}
