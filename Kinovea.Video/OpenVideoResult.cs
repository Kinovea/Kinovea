using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public enum OpenVideoResult
    {
        Success,
        UnknownError,
        NotSupported,
        FileNotOpenned,
        StreamInfoNotFound,
        VideoStreamNotFound,
        CodecNotFound,
        CodecNotOpened,
        CodecNotSupported,
        Cancelled,
        EmptyWatcher,
    }
}
