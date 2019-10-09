using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
using Kinovea.Video;

namespace Kinovea.Services
{
    public static class CaptureHistory
    {
        private static string directory;
        private static Dictionary<string, List<CaptureHistoryEntry>> sessions = new Dictionary<string, List<CaptureHistoryEntry>>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static CaptureHistory()
        {
            directory = Software.CaptureHistoryDirectory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static IEnumerable<string> GetRoots()
        {
            return Directory.GetFiles(directory).Select(f => Path.GetFileNameWithoutExtension(f)).Reverse();
        }

        public static IEnumerable<CaptureHistoryEntry> GetEntries(string session)
        {
            string sessionFile = Path.Combine(directory, session + ".xml");

            if (!sessions.ContainsKey(sessionFile))
            {
                List<CaptureHistoryEntry> entries = ImportEntries(sessionFile);
                sessions.Add(sessionFile, entries);
            }

            List<CaptureHistoryEntry> result = new List<CaptureHistoryEntry>(sessions[sessionFile]);
            result.Reverse();
            return result;
        }

        public static void AddEntry(CaptureHistoryEntry entry)
        {
            log.DebugFormat("Writing entry to Capture history.", entry.CaptureFile);
            string session = string.Format("{0:yyyyMMdd}.xml", entry.Start);
            string sessionFile = Path.Combine(directory, session);

            if (!sessions.ContainsKey(sessionFile))
            {
                List<CaptureHistoryEntry> entries = new List<CaptureHistoryEntry>();

                if (!File.Exists(sessionFile))
                {
                    log.DebugFormat("Creating session file {0}.", session);
                    try
                    {
                        File.Create(sessionFile).Close();
                    }
                    catch(Exception e)
                    {
                        log.ErrorFormat("Error while trying to create the session file. {0}", e.Message);
                    }
                }
                else
                {
                    entries = ImportEntries(sessionFile);
                }

                sessions.Add(sessionFile, entries);
            }

            int indexOf = -1;
            for (int i = 0; i < sessions[sessionFile].Count; i++)
            {
                if (entry.CaptureFile != sessions[sessionFile][i].CaptureFile)
                    continue;

                indexOf = i;
                break;
            }

            if (indexOf >= 0)
                sessions[sessionFile][indexOf] = entry;
            else
                sessions[sessionFile].Add(entry);

            ExportEntries(sessionFile);
        }

        public static void RemoveEntry(string session, CaptureHistoryEntry entry)
        {
            string sessionFile = Path.Combine(directory, session + ".xml");

            if (!sessions.ContainsKey(sessionFile))
                return;

            if (sessions[sessionFile].Contains(entry))
                sessions[sessionFile].Remove(entry);
        }

        public static void ImportDirectory(string dir)
        {
            // Import all files from the directory into corresponding entries.
            // Used for debugging and as a conveniency for first time use.

            List<CaptureHistoryEntry> entries = new List<CaptureHistoryEntry>();
            foreach (string file in Directory.GetFiles(dir))
            {
                string extension = Path.GetExtension(file);
                if (!VideoTypeManager.IsSupported(extension))
                    continue;

                CaptureHistoryEntry entry = CreateEntryFromFile(file);
                entries.Add(entry);
            }

            entries.Sort((e1, e2) => e1.Start.CompareTo(e2.Start));
            foreach (CaptureHistoryEntry entry in entries)
                AddEntry(entry);
        }

        private static List<CaptureHistoryEntry> ImportEntries(string sessionFile)
        {
            List<CaptureHistoryEntry> entries = new List<CaptureHistoryEntry>();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(sessionFile, settings))
            {
                try
                {
                    ParseSession(r, entries);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("An error occurred during the parsing of capture history session file: {0}", sessionFile);
                    log.ErrorFormat(e.ToString());
                }
            }

            return entries;
        }

        private static void ParseSession(XmlReader r, List<CaptureHistoryEntry> entries)
        {
            r.MoveToContent();

            if (!(r.Name == "KinoveaCaptureHistory"))
                return;

            r.ReadStartElement();
            r.ReadElementContentAsString("FormatVersion", "");

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Entry":
                        CaptureHistoryEntry entry = ParseEntry(r);
                        if (entry != null)
                            entries.Add(entry);
                        break;
                    default:
                        r.ReadOuterXml();
                        break;
                }
            }

            r.ReadEndElement();
        }

        private static CaptureHistoryEntry ParseEntry(XmlReader r)
        {
            string captureFile = "";
            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            string cameraAlias = "";
            string cameraIdentifier = "";
            double configuredFramerate = 0;
            double receivedFramerate = 0;
            int drops = 0;

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "CaptureFile":
                        captureFile = r.ReadElementContentAsString();
                        break;
                    case "Start":
                        string startDateTimeString = r.ReadElementContentAsString();
                        start = DateTime.ParseExact(startDateTimeString, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                        break;
                    case "End":
                        string endDateTimeString = r.ReadElementContentAsString();
                        end = DateTime.ParseExact(endDateTimeString, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                        break;
                    case "CameraAlias":
                        cameraAlias = r.ReadElementContentAsString();
                        break;
                    case "CameraIdentifier":
                        cameraIdentifier = r.ReadElementContentAsString();
                        break;
                    case "ConfiguredFramerate":
                        configuredFramerate = double.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "ReceivedFramerate":
                        receivedFramerate = double.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "Drops":
                        drops = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in capture history entry XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();

            CaptureHistoryEntry entry = new CaptureHistoryEntry(captureFile, start, end, cameraAlias, cameraIdentifier, configuredFramerate, receivedFramerate, drops);
            return entry;
        }

        private static void ExportEntries(string sessionFile)
        {
            if (!sessions.ContainsKey(sessionFile) || sessions[sessionFile].Count == 0)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            try
            {
                log.DebugFormat("Exporting capture history.");
                using (XmlWriter w = XmlWriter.Create(sessionFile, settings))
                {
                    w.WriteStartElement("KinoveaCaptureHistory");
                    w.WriteElementString("FormatVersion", "1.0");
                
                    foreach (CaptureHistoryEntry entry in sessions[sessionFile])
                    {
                        w.WriteStartElement("Entry");

                        w.WriteElementString("CaptureFile", entry.CaptureFile);
                        w.WriteElementString("Start", string.Format("{0:yyyyMMddTHHmmss}", entry.Start));
                        w.WriteElementString("End", string.Format("{0:yyyyMMddTHHmmss}", entry.End));
                        w.WriteElementString("CameraAlias", entry.CameraAlias);
                        w.WriteElementString("CameraIdentifier", entry.CameraIdentifier);
                        w.WriteElementString("ConfiguredFramerate", string.Format("{0:0.000}", entry.ConfiguredFramerate, CultureInfo.InvariantCulture));
                        w.WriteElementString("ReceivedFramerate", string.Format("{0:0.000}", entry.ReceivedFramerate, CultureInfo.InvariantCulture));
                        w.WriteElementString("Drops", string.Format("{0}", entry.Drops, CultureInfo.InvariantCulture));
                    
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while exporting capture history entries. {0}", e.Message);
            }
        }

        private static CaptureHistoryEntry CreateEntryFromFile(string file)
        {
            FileInfo info = new FileInfo(file);

            string captureFile = file;
            DateTime start = info.CreationTime;
            DateTime end = info.LastWriteTime;
            string cameraAlias = "";
            string cameraIdentifier = "";
            double configuredFramerate = 0;
            double receivedFramerate = 0;
            int drops = 0;

            CaptureHistoryEntry entry = new CaptureHistoryEntry(captureFile, start, end, cameraAlias, cameraIdentifier, configuredFramerate, receivedFramerate, drops);
            return entry;
        }
    }
}
