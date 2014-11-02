using System.Runtime.InteropServices;
using System.Threading;

namespace Kinovea.Pipeline.MemoryLayout
{
    /// <summary>
    /// Bool in its own cache line with full fences on reads and writes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2 * CacheLine.Size)]
    public struct CacheLineStorageBool
    {
        /// <summary>
        /// Expose data with full fence on read and write
        /// </summary>
        public bool Data
        {
            get
            {
                bool d = data;
                Thread.MemoryBarrier();
                return d;
            }
            set
            {
                // All writes are release so the memory barrier shouldn't technically be necessary.
                Thread.MemoryBarrier();
                data = value;
            }
        }

        [FieldOffset(CacheLine.Size)]
        private bool data;

        public CacheLineStorageBool(bool data)
        {
            this.data = data;
        }
    }
}