using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Kinovea.Pipeline;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Not a composite.
    /// The fact that this composite has no subframes is used by the compositer to reuse the delayer frame directly.
    /// </summary>
    public class DelayCompositeBasic : IDelayComposite
    {
        public List<IDelaySubframe> Subframes
        {
            get { return null; }
        }

        public bool NeedsRefresh
        {
            get { return true; }
        }

        public void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames)
        {
        }

        public void Tick()
        {
        }

        public int GetAge(IDelaySubframe subframe)
        {
            return 0;
        }
    }
}
