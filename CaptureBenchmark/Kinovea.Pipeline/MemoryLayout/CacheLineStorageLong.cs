using System.Runtime.InteropServices;
using System.Threading;

namespace Kinovea.Pipeline.MemoryLayout
{
    /// <summary>
    /// long in its own cache line with full fences on reads and writes.
    /// From NDisruptor project.
    /// Ref: http://drdobbs.com/go-parallel/article/217500206?pgno=4
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2 * CacheLine.Size)]
    public struct CacheLineStorageLong
    {
        /// <summary>
        /// Expose data with full fence on read and write
        /// </summary>
        public long Data
        {
            get
            {
                return Thread.VolatileRead(ref data);
            }
            set
            {
                Thread.VolatileWrite(ref data, value);
            }
        }

        [FieldOffset(CacheLine.Size)]
        private long data;

        public CacheLineStorageLong(long data)
        {
            this.data = data;
        }
    }
}