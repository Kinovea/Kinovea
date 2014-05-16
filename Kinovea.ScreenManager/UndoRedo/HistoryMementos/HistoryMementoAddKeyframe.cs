using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoAddKeyframe : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private string commandName = ScreenManagerLang.ToolTip_AddKeyframe;
        private Metadata metadata;
        private Guid keyframeId;

        public HistoryMementoAddKeyframe(Metadata metadata, Guid keyframeId)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoDeleteKeyframe(metadata, keyframeId);
            redoMemento.CommandName = CommandName;

            metadata.DeleteKeyframe(keyframeId);
            
            return redoMemento;
        }
    }
}
