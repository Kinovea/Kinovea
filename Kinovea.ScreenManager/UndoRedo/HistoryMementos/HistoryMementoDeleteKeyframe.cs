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

        // TODO: replace this by XML serialization.
        private long time;
        private string title;
        private Bitmap thumbnail;
        private string commandName = ScreenManagerLang.CommandDeleteKeyframe_FriendlyName;

        public HistoryMementoDeleteKeyframe(Metadata metadata, Guid keyframeId)
        {
            this.metadata = metadata;
            this.keyframeId = keyframeId;

            // TODO: save the complete keyframe to XML (including the thumbnail).
            // Including all drawings and trackable drawings associated with these drawings.
            Keyframe keyframe = metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                return;

            this.time = keyframe.Position;
            this.title = keyframe.Title;
            this.thumbnail = keyframe.Thumbnail; // <- not a proper clone.
        }

        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoAddKeyframe(metadata, keyframeId);
            redoMemento.CommandName = commandName;

            // TODO: recreate the entire keyframe from XML.
            Keyframe keyframe = new Keyframe(time, title, thumbnail, metadata);
            keyframe.Id = keyframeId;

            metadata.AddKeyframe(keyframe);

            return redoMemento;
        }
    }
}
