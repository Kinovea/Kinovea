using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public interface IDrawingHostView
    {
        void InvalidateFromMenu();
        void InitializeEndFromMenu(bool cancelLastPoint);
    }
}
