using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;
using System.Drawing;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoDeleteDrawing : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private Guid keyframeId;
        private Guid drawingId;
        private string drawingName;
        private string commandName;
        private string data;

        public HistoryMementoDeleteDrawing(Metadata metadata, Guid keyframeId, Guid drawingId, string drawingName)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;
            this.drawingId = drawingId;
            this.drawingName = drawingName;

            commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandDeleteDrawing_FriendlyName, drawingName);

            Keyframe keyframe = metadata.GetKeyframe(keyframeId);
            if (keyframe != null)
                data = DrawingSerializer.SerializeToString(keyframe.GetDrawing(drawingId));

            // TODO: get the associated trackable drawing and save it too.
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoAddDrawing(metadata, keyframeId, drawingId, drawingName);
            redoMemento.CommandName = commandName;

            AbstractDrawing drawing = DrawingSerializer.DeserializeFromString(data, metadata);
            metadata.AddDrawing(keyframeId, drawing);

            // TODO: re instate the associated trackable drawing.

            return redoMemento;
        }
    }
}