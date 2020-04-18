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
        public string Name
        {
            get { return "FileExplorer"; }
        }

        public int MaxRecentFiles
        {
            get { return maxRecentFiles; }
            set 
            { 
                if(recentFiles.Count > value)
                {
                    recentFiles.RemoveRange(value, recentFiles.Count - value);
                    NotificationCenter.RaiseRecentFilesChanged(this);
                }
                
                maxRecentFiles = value;
            }
        }

        public int MaxRecentCapturedFiles
        {
            get { return maxRecentCapturedFiles; }
            set
            {
                if (recentCapturedFiles.Count > value)
                    recentCapturedFiles.RemoveRange(value, recentCapturedFiles.Count - value);

                maxRecentCapturedFiles = value;
            }
        }

        public List<string> RecentFiles
        {
            get { return recentFiles;}
        }

        public List<string> RecentWatchers
        {
            get { return recentWatchers; }
        }

        public List<string> RecentCapturedFiles
        {
            get { return recentCapturedFiles; }
        }

        public int ExplorerFilesSplitterDistance
        {
            // Splitter between folders and files on Explorer tab
            get { return explorerFilesSplitterDistance; }
            set { explorerFilesSplitterDistance = value; }
        }
        public ExplorerThumbSize ExplorerThumbsSize
        {
            // Size category of the thumbnails.
            get { return explorerThumbsSize; }
            set { explorerThumbsSize = value; }				
        }
        public int ShortcutsFilesSplitterDistance
        {
            // Splitter between folders and files on Shortcuts tab
            get { return shortcutsFilesSplitterDistance; }
            set { shortcutsFilesSplitterDistance = value; }
        }
        public List<ShortcutFolder> ShortcutFolders
        {
            get{ return shortcutFolders;}
        }
        public ActiveFileBrowserTab ActiveTab 
        {
            get { return activeTab; }
            set { activeTab = value; }
        }
        public string LastBrowsedDirectory 
        {
            get { return lastBrowsedDirectory; }
            set { lastBrowsedDirectory = value;}
        }
        public FilePropertyVisibility FilePropertyVisibility
        {
            get { return filePropertyVisibility; }
        }
        public string LastReplayFolder
        {
            get { return lastReplayFolder; }
            set { lastReplayFolder = value; }
        }

        private int maxRecentFiles = 10;
        private int maxRecentCapturedFiles = 10;
        private List<string> recentFiles = new List<string>();
        private List<string> recentWatchers = new List<string>();
        private List<string> recentCapturedFiles = new List<string>();
        private int explorerFilesSplitterDistance = 350;
        private int shortcutsFilesSplitterDistance = 350;
        private ExplorerThumbSize explorerThumbsSize = ExplorerThumbSize.Medium; 
        private List<ShortcutFolder> shortcutFolders = new List<ShortcutFolder>();
        private ActiveFileBrowserTab activeTab = ActiveFileBrowserTab.Explorer;
        private string lastBrowsedDirectory;
        private FilePropertyVisibility filePropertyVisibility = new FilePropertyVisibility();
        private string lastReplayFolder;
        
        public void AddRecentFile(string file)
        {
            PreferencesManager.UpdateRecents(file, recentFiles, maxRecentFiles);
            NotificationCenter.RaiseRecentFilesChanged(this);
        }

        public void AddRecentWatcher(string file)
        {
            PreferencesManager.UpdateRecents(file, recentWatchers, maxRecentFiles);
            NotificationCenter.RaiseRecentFilesChanged(this);
        }

        public void ResetRecentFiles()
        {
            recentFiles.Clear();
            recentWatchers.Clear();
            NotificationCenter.RaiseRecentFilesChanged(this);
        }

        public void AddRecentCapturedFile(string file)
        {
            PreferencesManager.UpdateRecents(file, recentCapturedFiles, maxRecentCapturedFiles);
        }

        public void ConsolidateRecentCapturedFiles()
        {
            ConsolidateRecentFiles(recentCapturedFiles);
        }

        /// <summary>
        /// Remove missing entries from recent list.
        /// </summary>
        private void ConsolidateRecentFiles(List<string> recentFiles)
        {
            for (int i = recentFiles.Count - 1; i >= 0; i--)
            {
                if (Path.GetFileName(recentFiles[i]).Contains("*"))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(recentFiles[i])))
                        recentFiles.RemoveAt(i);
                }
                else if (!File.Exists(recentFiles[i]))
                {
                    recentFiles.RemoveAt(i);
                }
            }
        }
        
        public void ResetRecentCapturedFiles()
        {
            recentCapturedFiles.Clear();
        }

        public void RemoveShortcut(ShortcutFolder shortcut)
        {
            shortcutFolders.RemoveAll(s => s.Location == shortcut.Location);
        }
        
        public void AddShortcut(ShortcutFolder shortcut)
        {
            bool known = shortcutFolders.Any(s => s.Location == shortcut.Location);

            if(!known)
            {
                shortcutFolders.Add(shortcut);
                shortcutFolders.Sort();
            }
        }
        
        public bool IsShortcutKnown(string path)
        {
            return shortcutFolders.Any(s => s.Location == path);
        }

        public void SetFilePropertyVisible(FileProperty prop, bool state)
        {
            filePropertyVisibility.Visible[prop] = state;
        }
        
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
            
            writer.WriteElementString("ExplorerFilesSplitterDistance", explorerFilesSplitterDistance.ToString());
            writer.WriteElementString("ShortcutsFilesSplitterDistance", shortcutsFilesSplitterDistance.ToString());
            writer.WriteElementString("ActiveTab", activeTab.ToString());

            writer.WriteStartElement("FilePropertyVisibility");
            filePropertyVisibility.WriteXML(writer);
            writer.WriteEndElement();

            writer.WriteElementString("LastReplayFolder", lastReplayFolder);
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
                    case "ExplorerFilesSplitterDistance":
                        explorerFilesSplitterDistance = reader.ReadElementContentAsInt();
                        break;
                    case "ShortcutsFilesSplitterDistance":
                        shortcutsFilesSplitterDistance = reader.ReadElementContentAsInt();
                        break;                        
                    case "ActiveTab":
                        activeTab = (ActiveFileBrowserTab) Enum.Parse(typeof(ActiveFileBrowserTab), reader.ReadElementContentAsString());
                        break;
                    case "FilePropertyVisibility":
                        filePropertyVisibility = FilePropertyVisibility.FromXML(reader);
                        break;
                    case "LastReplayFolder":
                        lastReplayFolder = reader.ReadElementContentAsString();
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
    }
}
