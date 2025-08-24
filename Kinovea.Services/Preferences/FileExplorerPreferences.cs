#region License
/*
Copyright © Joan Charmant 2012.
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.Services
{
    public class FileExplorerPreferences : IPreferenceSerializer
    {
        #region Properties
        public string Name
        {
            get { return "FileExplorer"; }
        }

        public int MaxRecentFiles
        {
            get { BeforeRead(); return maxRecentFiles; }
            set 
            { 
                if(recentFiles.Count > value)
                {
                    recentFiles.RemoveRange(value, recentFiles.Count - value);
                    NotificationCenter.RaiseRecentFilesChanged(this);
                }
                
                maxRecentFiles = value;
                Save();
            }
        }

        public int MaxRecentCapturedFiles
        {
            get { BeforeRead(); return maxRecentCapturedFiles; }
            set
            {
                if (recentCapturedFiles.Count > value)
                    recentCapturedFiles.RemoveRange(value, recentCapturedFiles.Count - value);

                maxRecentCapturedFiles = value;
                Save();
            }
        }

        public List<string> RecentFiles
        {
            get { BeforeRead(); return recentFiles;}
        }

        public List<string> RecentWatchers
        {
            get { BeforeRead(); return recentWatchers; }
        }

        public List<string> RecentCapturedFiles
        {
            get { BeforeRead(); return recentCapturedFiles; }
        }

        /// <summary>
        /// Size of thumbnails.
        /// </summary>
        public ExplorerThumbSize ExplorerThumbsSize
        {
            get { BeforeRead(); return explorerThumbsSize; }
            set { explorerThumbsSize = value; Save(); }
        }

        public List<ShortcutFolder> ShortcutFolders
        {
            get { BeforeRead(); return shortcutFolders;}
        }
        
        public string LastBrowsedDirectory 
        {
            get { BeforeRead(); return lastBrowsedDirectory; }
            set { lastBrowsedDirectory = value; Save(); }
        }

        public FilePropertyVisibility FilePropertyVisibility
        {
            get { BeforeRead(); return filePropertyVisibility; }
        }
        
        public string LastReplayFolder
        {
            get { return lastReplayFolder; }
            set { lastReplayFolder = value; Save(); }
        }

        public FileSortAxis FileSortAxis
        {
            get { return fileSortAxis; }
            set { fileSortAxis = value; Save(); }
        }

        public bool FileSortAscending
        {
            get { return fileSortAscending; }
            set { fileSortAscending = value; Save(); }
        }
        #endregion

        #region Members
        private int maxRecentFiles = 10;
        private int maxRecentCapturedFiles = 10;
        private List<string> recentFiles = new List<string>();
        private List<string> recentWatchers = new List<string>();
        private List<string> recentCapturedFiles = new List<string>();
        private List<ShortcutFolder> shortcutFolders = new List<ShortcutFolder>();
        private ExplorerThumbSize explorerThumbsSize = ExplorerThumbSize.Medium; 
        private string lastBrowsedDirectory;
        private FilePropertyVisibility filePropertyVisibility = new FilePropertyVisibility();
        private string lastReplayFolder;
        private FileSortAxis fileSortAxis = FileSortAxis.Name;
        private bool fileSortAscending = true;
        #endregion

        private void Save()
        {
            PreferencesManager.Save();
        }

        private void BeforeRead()
        {
            PreferencesManager.BeforeRead();
        }

        public void AddRecentFile(string file)
        {
            PreferencesHelper.UpdateRecents(file, recentFiles, maxRecentFiles);
            NotificationCenter.RaiseRecentFilesChanged(this);
            Save();
        }

        public void AddRecentWatcher(string file)
        {
            PreferencesHelper.UpdateRecents(file, recentWatchers, maxRecentFiles);
            NotificationCenter.RaiseRecentFilesChanged(this);
            Save();
        }

        public void ResetRecentFiles()
        {
            recentFiles.Clear();
            recentWatchers.Clear();
            NotificationCenter.RaiseRecentFilesChanged(this);
            Save();
        }

        public void AddRecentCapturedFile(string file)
        {
            PreferencesHelper.UpdateRecents(file, recentCapturedFiles, maxRecentCapturedFiles);
            Save();
        }

        public void ConsolidateRecentCapturedFiles()
        {
            bool updated = PreferencesHelper.ConsolidateRecentFiles(recentCapturedFiles);
            if (updated)
            {
                Save();
            }
        }
        
        public void ResetRecentCapturedFiles()
        {
            recentCapturedFiles.Clear();
            Save();
        }

        public void RemoveShortcut(ShortcutFolder shortcut)
        {
            shortcutFolders.RemoveAll(s => s.Location == shortcut.Location);
            Save();
        }
        
        public void AddShortcut(ShortcutFolder shortcut)
        {
            bool known = shortcutFolders.Any(s => s.Location == shortcut.Location);

            if(!known)
            {
                shortcutFolders.Add(shortcut);
                shortcutFolders.Sort();
            }

            Save();
        }
        
        public bool IsShortcutKnown(string path)
        {
            return shortcutFolders.Any(s => s.Location == path);
        }

        public void SetFilePropertyVisible(FileProperty prop, bool state)
        {
            filePropertyVisibility.Visible[prop] = state;
            Save();
        }

        #region Serialization
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("MaxRecentFiles", maxRecentFiles.ToString());
            WriteRecents(writer, recentFiles, maxRecentFiles, "RecentFiles", "RecentFile");
            WriteRecents(writer, recentWatchers, maxRecentFiles, "RecentWatchers", "RecentWatcher");

            writer.WriteElementString("MaxRecentCapturedFiles", maxRecentCapturedFiles.ToString());
            WriteRecents(writer, recentCapturedFiles, maxRecentCapturedFiles, "RecentCapturedFiles", "RecentCapturedFile");

            if (shortcutFolders.Count > 0)
            {
                writer.WriteStartElement("Shortcuts");

                foreach(ShortcutFolder shortcut in shortcutFolders)
                {
                    writer.WriteStartElement("Shortcut");
                    shortcut.WriteXML(writer);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteElementString("ThumbnailSize", explorerThumbsSize.ToString());
            
            writer.WriteStartElement("FilePropertyVisibility");
            filePropertyVisibility.WriteXML(writer);
            writer.WriteEndElement();

            writer.WriteElementString("LastReplayFolder", lastReplayFolder);
            writer.WriteElementString("FileSortAxis", fileSortAxis.ToString());
            writer.WriteElementString("FileSortAscending", XmlHelper.WriteBoolean(fileSortAscending));
        }

        private void WriteRecents(XmlWriter writer, List<string> recentFiles, int max, string collectionTag, string itemTag)
        {
            if (recentFiles.Count == 0)
                return;
            
            writer.WriteStartElement(collectionTag);

            for (int i = 0; i < max; i++)
            {
                if (i >= recentFiles.Count)
                    break;

                if (string.IsNullOrEmpty(recentFiles[i]))
                    continue;

                writer.WriteElementString(itemTag, recentFiles[i]);
            }

            writer.WriteEndElement();
        }

        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "MaxRecentFiles":
                        maxRecentFiles = reader.ReadElementContentAsInt();
                        break;
                    case "RecentFiles":
                        ParseRecentFiles(reader, recentFiles, "RecentFile");
                        break;
                    case "RecentWatchers":
                        ParseRecentFiles(reader, recentWatchers, "RecentWatcher");
                        break;
                    case "MaxRecentCapturedFiles":
                        maxRecentCapturedFiles = reader.ReadElementContentAsInt();
                        break;
                    case "RecentCapturedFiles":
                        ParseRecentFiles(reader, recentCapturedFiles, "RecentCapturedFile");
                        break;
                    case "Shortcuts":
                        ParseShortcuts(reader);
                        break;
                    case "ThumbnailSize":
                        explorerThumbsSize = (ExplorerThumbSize) Enum.Parse(typeof(ExplorerThumbSize), reader.ReadElementContentAsString());
                        break;
                    case "FilePropertyVisibility":
                        filePropertyVisibility = FilePropertyVisibility.FromXML(reader);
                        break;
                    case "LastReplayFolder":
                        lastReplayFolder = reader.ReadElementContentAsString();
                        break;
                    case "FileSortAxis":
                        fileSortAxis = (FileSortAxis)Enum.Parse(typeof(FileSortAxis), reader.ReadElementContentAsString());
                        break;
                    case "FileSortAscending":
                        fileSortAscending = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
            
            reader.ReadEndElement();
        }
        
        private void ParseRecentFiles(XmlReader reader, List<string> recentFiles, string itemTag)
        {
            recentFiles.Clear();
            bool empty = reader.IsEmptyElement;
            
            reader.ReadStartElement();
            
            if(empty)
                return;
            
            while(reader.NodeType == XmlNodeType.Element)
            {
                if(reader.Name == itemTag)
                    recentFiles.Add(reader.ReadElementContentAsString());
                else
                    reader.ReadOuterXml();
            }
            
            reader.ReadEndElement();
        }
        
        private void ParseShortcuts(XmlReader reader)
        {
            shortcutFolders.Clear();
            bool empty = reader.IsEmptyElement;
            
            reader.ReadStartElement();
            
            if(empty)
                return;
            
            while(reader.NodeType == XmlNodeType.Element)
            {
                if(reader.Name == "Shortcut")
                {
                    ShortcutFolder shortcut = new ShortcutFolder(reader);
                    if(Directory.Exists(shortcut.Location))
                        shortcutFolders.Add(shortcut);
                }
                else
                {
                    reader.ReadOuterXml();
                }
            }
            
            reader.ReadEndElement();
            
            shortcutFolders.Sort();
            
        }
        #endregion
    }
}
