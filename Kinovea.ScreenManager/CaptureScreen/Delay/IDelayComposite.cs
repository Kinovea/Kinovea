using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline;

namespace Kinovea.ScreenManager
{
    public interface IDelayComposite
    {
        /// <summary>
        /// A list of sub frames descriptors that will be composited into the final image.
        /// </summary>
        List<IDelaySubframe> Subframes { get; }

        /// <summary>
        /// Whether the final image needs to be regenerated or if the current one can be reused.
        /// </summary>
        bool NeedsRefresh { get; }

        /// <summary>
        /// Called during allocation or reset of the composite.
        /// </summary>
        void UpdateSubframes(ImageDescriptor imageDescriptor, int totalFrames);

        /// <summary>
        /// Called for each display frame.
        /// </summary>
        void Tick();

        /// <summary>
        /// Returns the age to use for a given sub frame.
        /// </summary>
        int GetAge(IDelaySubframe subframe);
    }
}
