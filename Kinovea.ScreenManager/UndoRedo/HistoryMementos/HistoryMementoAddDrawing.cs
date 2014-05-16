using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoAddDrawing : HistoryMemento
    {
        public override string  CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private Guid keyframeId;
        private Guid drawingId;
        private string drawingName;
        private string commandName;

        public HistoryMementoAddDrawing(Metadata metadata, Guid keyframeId, Guid drawingId, string drawingName)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;
            this.drawingId = drawingId;
            this.drawingName = drawingName;

            commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandAddDrawing_FriendlyName, drawingName);
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoDeleteDrawing(metadata, keyframeId, drawingId, drawingName);
            redoMemento.CommandName = commandName;

            metadata.DeleteDrawing(keyframeId, drawingId);

            return redoMemento;
        }
    }
}
