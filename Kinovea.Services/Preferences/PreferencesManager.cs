#region Licence
/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com

This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Kinovea.Services
{
    public class PreferencesManager
    {
        public const string FORMAT_VERSION = "2.2";

        public static GeneralPreferences GeneralPreferences
        {
            get
            {
                if (instance == null)
                   instance = new PreferencesManager();

                return instance.generalPreferences;
            }
        }

        public static CapturePreferences CapturePreferences
        {
            get
            {
                if (instance == null)
                   instance = new PreferencesManager();

                return instance.capturePreferences;
            }
        }

        public static PlayerPreferences PlayerPreferences
        {
            get
            {
                if (instance == null)
                   instance = new PreferencesManager();

                return instance.playerPreferences;
            }
        }

        public static FileExplorerPreferences FileExplorerPreferences
        {
            get
            {
                if (instance == null)
                   instance = new PreferencesManager();

                return instance.fileExplorerPreferences;
            }
        }

        public static KeyboardPreferences KeyboardPreferences
        {
            get
            {
                if (instance == null)
                    instance = new PreferencesManager();

                return instance.keyboardPreferences;
            }
        }
        private GeneralPreferences generalPreferences = new GeneralPreferences();
        private FileExplorerPreferences fileExplorerPreferences = new FileExplorerPreferences();
        private PlayerPreferences playerPreferences = new PlayerPreferences();
        private CapturePreferences capturePreferences = new CapturePreferences();
        private KeyboardPreferences keyboardPreferences = new KeyboardPreferences();

        private static PreferencesManager instance = null;
        private static object locker = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Initialize()
        {
            instance = new PreferencesManager();
        }

        public static void Save()
        {
            if(instance == null)
                return;

            lock(locker)
                instance.Export();
        }

        private PreferencesManager()
        {
            Import();
        }

        private void Export()
        {
            log.DebugFormat("Exporting {0}", Path.GetFileName(Software.PreferencesFile));

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.CloseOutput = true;

                using(XmlWriter w = XmlWriter.Create(Software.PreferencesFile, settings))
                {
                        WriteXML(w);
                }
            }
            catch(Exception e)
            {
                log.Error("An error happened during the writing of the preferences file");
                log.Error(e);
            }
        }

        private void Import()
        {
            if(!File.Exists(Software.PreferencesFile))
                return;

            log.InfoFormat("Importing {0}", Path.GetFileNameWithoutExtension(Software.PreferencesFile));

            string preferencesFile = ConvertIfNeeded(Software.PreferencesFile);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = null;
            reader = XmlReader.Create(preferencesFile, settings);

            try
            {
                ReadXML(reader);
            }
            catch(Exception e)
            {
                log.Error("An error happened during the parsing of the preferences file");
                log.Error(e);
            }
            finally
            {
                if(reader != null)
                    reader.Close();
            }
        }

        private void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("KinoveaPreferences");
            writer.WriteElementString("FormatVersion", FORMAT_VERSION);
            WritePreference(writer, generalPreferences);
            WritePreference(writer, fileExplorerPreferences);
            WritePreference(writer, playerPreferences);
            WritePreference(writer, capturePreferences);
            WritePreference(writer, keyboardPreferences);
        }

        private void WritePreference(XmlWriter writer, IPreferenceSerializer serializer)
        {
            writer.WriteStartElement(serializer.Name);
            serializer.WriteXML(writer);
            writer.WriteEndElement();
        }

        private string ConvertIfNeeded(string preferencesFile)
        {
            XmlDocument prefs = new XmlDocument();

            try
            {
                prefs.Load(preferencesFile);
            }
            catch (XmlException e)
            {
                log.ErrorFormat("Could not read preferences file. {0}", e.ToString());
                File.Copy(preferencesFile, preferencesFile + ".bak", true);
                return preferencesFile;
            }

            XmlNode formatNode = prefs.DocumentElement.SelectSingleNode("descendant::FormatVersion");

            double format;
            bool read = double.TryParse(formatNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
            if(!read)
            {
                log.ErrorFormat("Could not read preferences file format version");
                File.Copy(preferencesFile, preferencesFile + ".bak", true);
                return preferencesFile;
            }

            if(format >= 2.0)
                return preferencesFile;

            // Conversion needed.
            File.Copy(preferencesFile, preferencesFile + ".bak", true);

            string stylesheet = Software.XSLTDirectory + "prefs-1.2to2.0.xsl";
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(stylesheet);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            string converted = Path.Combine(Software.SettingsDirectory, "temp.prefs.xml");

            using (XmlWriter xw = XmlWriter.Create(converted, settings))
            {
                xslt.Transform(prefs, xw);
            }

            return converted;
        }
        private void ReadXML(XmlReader reader)
        {
            reader.MoveToContent();

            if(!(reader.Name == "KinoveaPreferences"))
                return;

            reader.ReadStartElement();
            reader.ReadElementContentAsString("FormatVersion", "");

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "General":
                        generalPreferences.ReadXML(reader);
                        break;
                    case "FileExplorer":
                        fileExplorerPreferences.ReadXML(reader);
                        break;
                    case "Player":
                        playerPreferences.ReadXML(reader);
                        break;
                    case "Capture":
                        capturePreferences.ReadXML(reader);
                        break;
                    case "Keyboard":
                        keyboardPreferences.ReadXML(reader);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Adds an entry to a list of recent objects. Remove the last one if overflow, and handle duplication.
        /// </summary>
        public static void UpdateRecents<T>(T entry, List<T> recentEntries, int max)
        {
            if(max == 0)
                return;

            int found = -1;
            for (int i = 0; i < max; i++)
            {
                if(i >= recentEntries.Count)
                    break;

                if (recentEntries[i].Equals(entry))
                {
                    found = i;
                    break;
                }
            }

            if(found >= 0)
                recentEntries.RemoveAt(found);
            else if(recentEntries.Count == max)
                recentEntries.RemoveAt(recentEntries.Count - 1);

            recentEntries.Insert(0, entry);
        }

    }
}
