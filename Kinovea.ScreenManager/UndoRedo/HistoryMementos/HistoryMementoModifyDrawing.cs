using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;
using System.Drawing;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoModifyDrawing : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private Guid managerId;
        private Guid drawingId;
        private string drawingName;
        private SerializationFilter filter;
        private string commandName;
        private string data;

        public HistoryMementoModifyDrawing(Metadata metadata, Guid managerId, Guid drawingId, string drawingName, SerializationFilter filter)
        {
            this.metadata = metadata;
            this.managerId = managerId;
            this.drawingId = drawingId;
            this.drawingName = drawingName;
            this.filter = filter;

            UpdateCommandName(drawingName);

            AbstractDrawingManager manager = metadata.GetDrawingManager(managerId);

            if (manager != null)
                data = DrawingSerializer.SerializeMemento(metadata, manager.GetDrawing(drawingId), filter, false);
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, drawingName, filter);
            DrawingSerializer.DeserializeModifyMemento(managerId, drawingId, data, metadata);
            metadata.ModifiedDrawing(managerId, drawingId);
            return redoMemento;
        }

        public void UpdateCommandName(string name)
        {
            drawingName = name;
            commandName = commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandModifyDrawing_FriendlyName, drawingName);
        }
    }
}