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
        /// Returns the total physical memory of the system in megabytes.
        /// </summary>
        public static int TotalPhysicalMemory()
        {
            // Max allocation of memory is based on bitness and physical memory.
            ulong megabytes = 1024 * 1024;
            ComputerInfo ci = new ComputerInfo();
            int total = (int)(ci.TotalPhysicalMemory / megabytes);
            return total;
        }
    }
}
