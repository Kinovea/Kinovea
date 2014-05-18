using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoAddMultiDrawingItem : HistoryMemento
    {
        public override string  CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private AbstractMultiDrawing manager;
        private Guid itemId;
        private string commandName;

        public HistoryMementoAddMultiDrawingItem(Metadata metadata, AbstractMultiDrawing manager, Guid itemId)
        {
            this.metadata = metadata;
            this.manager = manager;
            this.itemId = itemId;
            
            commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandAddDrawing_FriendlyName, manager.DisplayName);
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoDeleteMultiDrawingItem(metadata, manager, itemId);
            redoMemento.CommandName = commandName;

            metadata.DeleteMultiDrawingItem(manager, itemId);

            return redoMemento;
        }
    }
}
