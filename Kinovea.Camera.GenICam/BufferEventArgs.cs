using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera.GenICam
{
    public class BufferEventArgs : EventArgs
    {
        public readonly BGAPI2.Buffer Buffer;
        public BufferEventArgs(BGAPI2.Buffer buffer)
        {
            this.Buffer = buffer;
        }
    }
}
