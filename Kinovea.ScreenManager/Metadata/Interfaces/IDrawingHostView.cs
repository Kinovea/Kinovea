using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public interface IDrawingHostView
    {
        void DoInvalidate();
        void InitializeEndFromMenu(bool cancelLastPoint);
    }
}
