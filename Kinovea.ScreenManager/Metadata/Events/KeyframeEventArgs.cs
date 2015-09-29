using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class KeyframeEventArgs : EventArgs
    {
        public Guid KeyframeId
        {
            get { return keyframeId; }
        }

        private readonly Guid keyframeId;

        public KeyframeEventArgs(Guid keyframeId)
        {
            this.keyframeId = keyframeId;
        }
    }
}
