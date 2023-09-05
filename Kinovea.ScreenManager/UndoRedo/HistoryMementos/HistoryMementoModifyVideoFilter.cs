using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager.Languages;
using System.Drawing;
using System.Xml;
using System.IO;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoModifyVideoFilter : HistoryMemento
    {
        public override string CommandName
        {
            get { return commandName; }
            set { commandName = value; }
        }

        private Metadata metadata;
        private VideoFilterType filterType;
        private string filterName;
        private string commandName;
        private string data;

        /// <summary>
        /// Capture the state of the filter before a modification.
        /// </summary>
        public HistoryMementoModifyVideoFilter(Metadata metadata, VideoFilterType filterType, string filterName)
        {
            this.metadata = metadata;
            this.filterType = filterType;
            this.filterName = filterName;
            UpdateCommandName(filterName);

            IVideoFilter filter = metadata.GetVideoFilter(filterType);

            if (filter != null)
                data = Serialize(filter);
        }

        /// <summary>
        /// Restore the backed up state into the filter.
        /// This also captures the state of the filter after the modification and returns the memento for it, to handle redo.
        /// </summary>
        public override HistoryMemento PerformUndo()
        {
            HistoryMemento redoMemento = new HistoryMementoModifyVideoFilter(metadata, filterType, filterName);
            
            IVideoFilter filter = metadata.GetVideoFilter(filterType);
            Deserialize(filter, data);
            metadata.ModifiedVideoFilter();

            return redoMemento;
        }

        public void UpdateCommandName(string name)
        {
            filterName = name;
            //commandName = string.Format("{0} ({1})", ScreenManagerLang.CommandModifyDrawing_FriendlyName, filterName);
            commandName = string.Format("{0} {1}", "Modify", filterName);
        }

        private string Serialize(IVideoFilter filter)
        {
            string result;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();

            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                w.WriteStartElement("VideoFilterMemento");

                w.WriteStartElement("Parameters");
                filter.WriteData(w);
                w.WriteEndElement();

                w.WriteEndElement();

                w.Flush();
                result = builder.ToString();
            }

            return result;
        }

        private void Deserialize(IVideoFilter filter, string data)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();

                if (!(r.Name == "VideoFilterMemento"))
                    return;

                r.ReadStartElement();

                filter.ReadData(r);
            }
        }
    }
}