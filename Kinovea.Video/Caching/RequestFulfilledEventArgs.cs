using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public class RequestFulfilledEventArgs
    {
        public readonly PlayerState PlayerState;
        public readonly bool Fulfilled;
        public readonly long Timestamp;
        public RequestFulfilledEventArgs(PlayerState playerState, bool fulfilled, long timestamp)
        {
            this.PlayerState = playerState;
            this.Fulfilled = fulfilled;
            this.Timestamp = timestamp;
        }
    }
}
