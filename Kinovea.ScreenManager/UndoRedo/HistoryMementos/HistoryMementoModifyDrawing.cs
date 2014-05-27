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
        private string commandName;
        private string data;

        public HistoryMementoModifyDrawing(Metadata metadata, Guid managerId, Guid drawingId, string drawingName)
        {
            this.metadata = metadata;
            this.managerId = managerId;
            this.drawingId = drawingId;
            this.drawingName = drawingName;

            commandName = string.Format("{0} ({1})", "Modify drawing", drawingName);

            AbstractDrawingManager manager = metadata.GetDrawingManager(managerId);

            if (manager != null)
                data = DrawingSerializer.SerializeToString(manager.GetDrawing(drawingId));
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, drawingName);
            DrawingSerializer.ModifyFromString(managerId, drawingId, data, metadata);
            metadata.ModifiedDrawing(managerId, drawingId);
            return redoMemento;
        }
    }
}