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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Kinovea.Services
{

    /// <summary>
    /// Preferences data, save/load and synchronization logic.
    /// </summary>
    public class PreferencesManager : IDisposable
    {
        #region Static properties to access the actual preferences
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
        #endregion

        #region Members
        private static PreferencesManager instance = null;
        private GeneralPreferences generalPreferences = new GeneralPreferences();
        private FileExplorerPreferences fileExplorerPreferences = new FileExplorerPreferences();
        private PlayerPreferences playerPreferences = new PlayerPreferences();
        private CapturePreferences capturePreferences = new CapturePreferences();
        private KeyboardPreferences keyboardPreferences = new KeyboardPreferences();

        private XmlWriterSettings xmlWriterSettings;
        private XmlReaderSettings xmlReaderSettings;

        // sync
        private DateTime lastImport = DateTime.MinValue;
        private DateTime lastFileModification = DateTime.MinValue;
        private FileSystemWatcher watcher;
        private Stopwatch stopwatch = new Stopwatch();
        private static bool saveSuspended = false;
        private static object locker = new object();
        public const string FORMAT_VERSION = "3.0";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Initialization/Destruction

        public static void Initialize()
        {
            instance = new PreferencesManager();
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private PreferencesManager()
        {
            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;

            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.CloseInput = true;

            watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = false;
            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.Deleted += watcher_Changed;
            watcher.Renamed += watcher_Changed;

            try
            {
                string dir = Path.GetDirectoryName(Software.PreferencesFile);
                if (Directory.Exists(dir))
                {
                    watcher.Path = dir;
                    watcher.Filter = Path.GetFileName(Software.PreferencesFile);
                    watcher.EnableRaisingEvents = true;
                }
            }
            catch
            {
                watcher.EnableRaisingEvents = false;
            }

            FileChanged();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PreferencesManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                watcher.Changed -= watcher_Changed;
                watcher.Created -= watcher_Changed;
                watcher.Deleted -= watcher_Changed;
                watcher.Renamed -= watcher_Changed;
                watcher.Dispose();
            }
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Save the whole preferences file to the file system.
        /// This should be called after any update to the data structure.
        /// </summary>
        public static void Save()
        {
            if(instance == null)
                return;

            // Bail out if we are in the process of updating multiple preferences at once.
            if (saveSuspended)
                return;

            log.DebugFormat("After save pref prop");

            lock (locker)
            {
                instance.Export();
            }
        }

        /// <summary>
        /// Make sure we have the latest version of the preferences before getting a value.
        /// This should be called before any "get" call on the properties.
        /// </summary>
        public static void BeforeRead()
        {
            log.DebugFormat("Before read pref prop");
            if (instance == null)
                return;

            instance.EnsureUpToDate();
        }

        /// <summary>
        /// Force import the whole preferences file from the file system.
        /// This should be called after another instance signalled that it has updated 
        /// the preferences in a way that requires immediate refresh. Typically after
        /// going through the preferences dialog and saving changes.
        /// </summary>
        public static void Refresh()
        {
            if (instance == null)
                return;

            lock (locker)
            {
                instance.Import();
            }
        }

        /// <summary>
        /// Temporarily prevents the preferences from being saved to the file system on every change.
        /// Use this before updating multiple preferences at once.
        /// </summary>
        public static void SuspendSave()
        {
            saveSuspended = true;
        }

        /// <summary>
        /// Resume saving the preferences to the file system on every change, and trigger a save.
        /// </summary>
        public static void ResumeSave()
        {
            saveSuspended = false;
            Save();
        }
        #endregion

        #region Synchronization

        /// <summary>
        /// Export the core preferences to the file system.
        /// Normally this is called after any change to the preferences.
        /// This should trigger a refresh in other instances.
        /// Instance specific preferences are not here.
        /// The write is done on a best-effort basis, if another instance is currently writing it will fail.
        /// If two instances write in very rapid succession before they get the notification that 
        /// the file was updated, the last one wins.
        /// </summary>
        private void Export()
        {
            // Bail out if the file was never imported, this may happen on import/export error.
            if (lastImport == DateTime.MinValue)
                return;
            
            log.DebugFormat("Exporting {0}", Path.GetFileName(Software.PreferencesFile));
            stopwatch.Restart();
            try
            {
                // 1. Write prefs to a temporary file.
                string tmpFile = Path.GetTempFileName();
                bool success = Write(tmpFile);
                if (!success)
                    return;

                log.DebugFormat("Exported prefs to temp file: {0} ms", stopwatch.ElapsedMilliseconds);

                // 2. Swap the temp file.
                try
                {
                    File.Copy(tmpFile, Software.PreferencesFile, true);
                    File.Delete(tmpFile);
                }
                catch (Exception e)
                {
                    // If the swap fails it means another instance is writing the file at the same time.
                    // We don't handle this case and it should be rare.
                    // The general heuristic is that preferences are saved immediately after a change,
                    // so as long as the user is making changes to only one instance at a time we 
                    // shouldn't have any issues.
                    // To make this work instances shouldn't write a bunch of preferences when closing down,
                    // as it's possible to batch close all windows of the application from Explorer taskbar.
                    log.ErrorFormat("An error happened while exporting the preferences: {0}", e.Message);

                    // Invalidate our state to force a reload later.
                    lastImport = DateTime.MinValue;
                    return;
                }

                // The swap worked.
                FileChanged();
                lastImport = DateTime.UtcNow;
                log.DebugFormat("Exported prefs total time: {0} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                log.ErrorFormat("An error happened while exporting the preferences: {0}", e.Message);
            }
        }

        private DateTime GetLastFileModification()
        {
            try
            {
                if (File.Exists(Software.PreferencesFile))
                {
                    return File.GetLastWriteTimeUtc(Software.PreferencesFile);
                }
            }
            catch
            {
                // no-op
            }

            return DateTime.MaxValue;
        }

        /// <summary>
        /// Reload the core preferences from the file system.
        /// </summary>
        private void Import()
        {
            log.InfoFormat("Importing {0}", Path.GetFileName(Software.PreferencesFile));
            stopwatch.Restart();
            try
            {
                if (File.Exists(Software.PreferencesFile))
                {
                    Read(Software.PreferencesFile);
                    lastImport = DateTime.UtcNow;
                }
                else
                {
                    lastImport = DateTime.MinValue;
                }
            
                log.DebugFormat("Imported prefs in {0} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                log.ErrorFormat("An error happened while importing the preferences file.");
                lastImport = DateTime.MinValue;
            }
        }

        /// <summary>
        /// The file was changed from us or externally, update the last modification time
        /// used to check if we have a stale version of the preferences or not.
        /// </summary>
        private void FileChanged()
        {
            lastFileModification = GetLastFileModification();
        }

        /// <summary>
        /// The file was changed externally.
        /// </summary>
        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileChanged();
        }

        /// <summary>
        /// Make sure the preferences data structure is up to date with the file system.
        /// This should be called prior to any "get" call on the properties.
        /// </summary>
        private void EnsureUpToDate()
        {
            if (lastImport == DateTime.MinValue || lastFileModification > lastImport)
            {
                lock (locker)
                {
                    Import();
                }
            }
        }
        #endregion


        #region Serialization
        private bool Write(string path)
        {
            bool success = false;
            try
            {
                using (XmlWriter w = XmlWriter.Create(path, xmlWriterSettings))
                {
                    WriteXML(w);
                }

                success = true;
            }
            catch (Exception e)
            {
                log.Error("An error happened during the writing of the preferences file");
                log.Error(e);
            }

            return success;
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

        private string ConvertIfNeeded(string path)
        {
            XmlDocument prefs = new XmlDocument();

            try
            {
                prefs.Load(path);
            }
            catch (XmlException e)
            {
                log.ErrorFormat("Could not read preferences file. {0}", e.ToString());
                File.Copy(path, path + ".bak", true);
                return path;
            }

            XmlNode formatNode = prefs.DocumentElement.SelectSingleNode("descendant::FormatVersion");

            double format;
            bool read = double.TryParse(formatNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
            if(!read)
            {
                log.ErrorFormat("Could not read preferences file format version");
                File.Copy(path, path + ".bak", true);
                return path;
            }

            if(format >= 2.0)
                return path;

            // Conversion needed.
            File.Copy(path, path + ".bak", true);

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
        
        private void Read(string path)
        {
            XmlReader reader = null;
            reader = XmlReader.Create(path, xmlReaderSettings);

            try
            {
                ReadXML(reader);
            }
            catch (Exception e)
            {
                log.Error("An error happened during the parsing of the preferences file.");
                log.Error(e);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
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
        #endregion
    }
}
