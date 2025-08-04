using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoDeleteKeyframe : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private Guid keyframeId;

        private string data;
        private string commandName = ScreenManagerLang.CommandDeleteKeyframe_FriendlyName;

        public HistoryMementoDeleteKeyframe(Metadata metadata, Guid keyframeId)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;

            Keyframe keyframe = metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                throw new NullReferenceException("keyframe");

            data = KeyframeSerializer.SerializeMemento(metadata, keyframe, SerializationFilter.KVA);
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoAddKeyframe(metadata, keyframeId);
            redoMemento.CommandName = commandName;

            Keyframe keyframe = KeyframeSerializer.DeserializeMemento(data, metadata);
            metadata.MergeInsertKeyframe(keyframe);
            metadata.AfterUndoDeleteKeyframe(keyframe);

            return redoMemento;
        }
    }
}
