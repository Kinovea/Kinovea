using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoAddKeyframe : HistoryMemento
    {
        private PlayerScreen screen;
        private long time;

        public HistoryMementoAddKeyframe(PlayerScreen screen, long time)
        {
            this.screen = screen;
            this.time = time;
        }

        public override HistoryMemento PerformUndo()
        {
            // TODO: create a DeleteKeyframe memento.
            HistoryMemento redoMemento = new HistoryMementoDeleteKeyframe(screen, time);

            // Undo the keyframe addition.
            int index = screen.FrameServer.Metadata.GetKeyframeIndex(time);
            if (index >= 0)
            {
                screen.FrameServer.Metadata.RemoveAt(index);
                screen.FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
                screen.view.OnRemoveKeyframe(index);
            }

            return redoMemento;
        }
    }
}
