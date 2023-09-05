using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoModifyKeyframe : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        //private string commandName = ScreenManagerLang.ToolTip_ModifyKeyframe;
        private string commandName = "Modify keyframe";
        private Metadata metadata;
        private Guid keyframeId;
        private string data;

        /// <summary>
        /// Capture the state of the keyframe before a modification.
        /// This only concerns the core data of the keyframe: title, color and comments.
        /// Drawings are handled by other mementos (AddDrawing, DeleteDrawing, ModifyDrawing).
        /// </summary>
        public HistoryMementoModifyKeyframe(Metadata metadata, Guid keyframeId)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;

            Keyframe keyframe = metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                throw new NullReferenceException("keyframe");

            data = KeyframeSerializer.SerializeMemento(metadata, keyframe, SerializationFilter.Core);
        }

        /// <summary>
        /// Restore the backed up state into the keyframe.
        /// This also captures the state of the keyframe after the modification and return the memento for it, to handle redo.
        /// </summary>
        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoModifyKeyframe(metadata, keyframeId);
            KeyframeSerializer.DeserializeModifyMemento(keyframeId, data, metadata);
            metadata.ModifiedKeyframe(keyframeId);
            return redoMemento;
        }
    }
}
