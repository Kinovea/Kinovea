using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Kinovea.ScreenManager
{

    /// <summary>
    /// A thin wrapper around WaveInCapabilities.
    /// </summary>
    public class AudioInputDevice
    {
        public WaveInCapabilities WaveInCapabilities { get; private set; }

        public AudioInputDevice(WaveInCapabilities caps)
        {
            this.WaveInCapabilities = caps;
        }

        public override string ToString()
        {
            return WaveInCapabilities.ProductName;
        }
    }
}
