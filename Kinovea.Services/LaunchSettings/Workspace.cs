using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kinovea.Services
{

    public class Workspace
    {
        public List<IScreenDescriptor> Screens { get; private set; } = new List<IScreenDescriptor>();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return false;
            
            if (!File.Exists(filename))
            {
                log.ErrorFormat("The workspace file could not be found. {0}", filename);
                return false;
            }

            bool loaded = false;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = null;
            reader = XmlReader.Create(filename, settings);

            try
            {
                ReadXML(reader);
                loaded = true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("An error happened during the parsing of the workspace file. {0}", Path.GetFileName(filename));
                log.Error(e);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return loaded;
        }

        public void Write(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;

            using (XmlWriter w = XmlWriter.Create(filename, settings))
            {
                w.WriteStartElement("KinoveaWorkspace");
                w.WriteElementString("FormatVersion", "1.0");
                WriteXML(w);
                w.WriteEndElement();
            }
        }

        public void ReadXML(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmpty = reader.IsEmptyElement;

            if (reader.Name != "Workspace" && reader.Name != "KinoveaWorkspace" || isEmpty)
            {
                reader.ReadOuterXml();
                return;
            }

            reader.ReadStartElement();
            //reader.ReadElementContentAsString("FormatVersion", "");

            Screens.Clear();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ScreenDescriptionPlayback":

                        ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                        sdp.ReadXml(reader);
                        Screens.Add(sdp);
                        break;
                    case "ScreenDescriptionCapture":
                        ScreenDescriptionCapture sdc = new ScreenDescriptionCapture();
                        sdc.Readxml(reader);
                        Screens.Add(sdc);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            // Extra flag for dual replay.
            if (Screens.Count == 2 &&
                Screens[0] is ScreenDescriptionPlayback && ((ScreenDescriptionPlayback)Screens[0]).IsReplayWatcher &&
                Screens[1] is ScreenDescriptionPlayback && ((ScreenDescriptionPlayback)Screens[1]).IsReplayWatcher)
            {
                ((ScreenDescriptionPlayback)Screens[0]).IsDualReplay = true;
                ((ScreenDescriptionPlayback)Screens[1]).IsDualReplay = true;
            }

            reader.ReadEndElement();
        }

        public void WriteXML(XmlWriter w)
        {
            foreach (var screen in Screens)
            {
                if (screen.ScreenType == ScreenType.Playback)
                    w.WriteStartElement("ScreenDescriptionPlayback");
                else
                    w.WriteStartElement("ScreenDescriptionCapture");
                    
                screen.WriteXml(w);
                w.WriteEndElement();
            }
        }
    }
}
