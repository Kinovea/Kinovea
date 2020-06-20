using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;

namespace Kinovea.Services
{
    public static class MemoryHelper
    {
        /// <summary>
        /// Returns a reasonable maximum amount of memory usable by memory buffers.
        /// Each screen will automatically halve this in case of a two-screen setup.
        /// </summary>
        /// <returns></returns>
        public static int MaxMemoryBuffer()
        {
            // Max allocation of memory is based on bitness and physical memory.
            ulong megabytes = 1024 * 1024;
            ComputerInfo ci = new ComputerInfo();
            int maxMemory = (int)(ci.TotalPhysicalMemory / megabytes);
            int thresholdLargeMemory = 3072;
            int reserve = 2048;

            if (Software.Is32bit || maxMemory < thresholdLargeMemory)
                return 1024;
            else
                return maxMemory - reserve;
        }
    }
}
