using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interface for drawings that respond to the chronometer start/stop and split commands.
    /// </summary>
    public interface ITimeable
    {
        /// <summary>
        /// Perform the start/stop command.
        /// </summary>
        void StartStop(long timestamp);

        /// <summary>
        /// Perform the split command.
        /// </summary>
        void Split(long timestamp);
    }
}
