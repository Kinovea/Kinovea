using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoDeleteKeyframe : HistoryMemento
    {
        private PlayerScreen screen;
        private long time;

        public HistoryMementoDeleteKeyframe(PlayerScreen screen, long time)
        {
            // TODO: store the entire keyframe here, serializing a "data" object to disk.
            this.screen = screen;
            this.time = time;
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoAddKeyframe(screen, time);

            // TODO: recreate the entire keyframe from a serialized copy.
            screen.AddKeyframe(time);

            return redoMemento;
        }
    }
}
