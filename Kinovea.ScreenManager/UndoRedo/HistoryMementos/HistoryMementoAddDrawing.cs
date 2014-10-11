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
        private Guid managerId;
        private Guid drawingId;
        private string drawingName;
        private string commandName;

        public HistoryMementoAddDrawing(Metadata metadata, Guid managerId, Guid drawingId, string drawingName)
        {
            this.metadata = metadata;
            this.managerId = managerId;
            this.drawingId = drawingId;
            this.drawingName = drawingName;

            commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandAddDrawing_FriendlyName, drawingName);
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoDeleteDrawing(metadata, managerId, drawingId, drawingName);
            redoMemento.CommandName = commandName;

            metadata.DeleteDrawing(managerId, drawingId);

            return redoMemento;
        }
    }
}
