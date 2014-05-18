using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;
using System.Drawing;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoDeleteMultiDrawingItem : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private AbstractMultiDrawing manager;
        private Guid itemId;
        private string commandName;
        private string data;

        public HistoryMementoDeleteMultiDrawingItem(Metadata metadata, AbstractMultiDrawing manager, Guid itemId)
        {
            this.metadata = metadata;
            this.manager = manager;
            this.itemId = itemId;

            commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandDeleteDrawing_FriendlyName, manager.DisplayName);

            if (manager != null)
                data = MultiDrawingItemSerializer.SerializeToString(manager, manager.GetItem(itemId));

            // TODO: get the associated trackable drawing and save it too.
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoAddMultiDrawingItem(metadata, manager, itemId);
            redoMemento.CommandName = commandName;

            AbstractMultiDrawingItem item = MultiDrawingItemSerializer.DeserializeFromString(data, metadata);
            metadata.AddMultidrawingItem(manager, item);

            // TODO: re instate the associated trackable drawing.

            return redoMemento;
        }
    }
}