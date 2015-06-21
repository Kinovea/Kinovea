using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;

namespace Kinovea.ScreenManager
{
    public interface IDelayComposite
    {
        List<IDelaySubframe> Subframes { get; }
        bool NeedsRefresh { get; }

        void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames);
        void Tick();
        int GetAge(IDelaySubframe subframe);
    }
}
