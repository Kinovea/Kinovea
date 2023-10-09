using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interface for drawings that respond to the chronometer start/stop and split commands or counter beat commands.
    /// </summary>
    public interface ITimeable
    {
        /// <summary>
        /// Perform the start/stop command.
        /// This should start or end a time section.
        /// </summary>
        void StartStop(long timestamp);

        /// <summary>
        /// Perform the split command.
        /// End the current time section and start a new one on the same frame.
        /// </summary>
        void Split(long timestamp);

        /// <summary>
        /// Perform the beat command.
        /// Adds a beat at this time.
        /// </summary>
        void Beat(long timestamp);
    }
}
