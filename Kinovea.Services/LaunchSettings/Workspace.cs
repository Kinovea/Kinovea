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
        public List<IScreenDescription> Screens { get; private set; } = new List<IScreenDescription>();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Load(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return false;

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
                log.Error("An error happened during the parsing of a workspace file");
                log.Error(e);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return loaded;
        }

        private void ReadXML(XmlReader reader)
        {
            reader.MoveToContent();

            if (!(reader.Name == "KinoveaWorkspace"))
                return;

            reader.ReadStartElement();
            reader.ReadElementContentAsString("FormatVersion", "");

            Screens.Clear();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ScreenDescriptionPlayback":
                        ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback(reader);
                        Screens.Add(sdp);
                        break;
                    case "ScreenDescriptionCapture":
                        ScreenDescriptionCapture sdc = new ScreenDescriptionCapture(reader);
                        Screens.Add(sdc);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }
}
