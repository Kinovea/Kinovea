using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class KeyboardPreferences : IPreferenceSerializer
    {
        public string Name
        {
            get { return "Keyboard"; }
        }

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("Hotkeys");

            Dictionary<string, HotkeyCommand[]> hotkeys = HotkeySettingsManager.Hotkeys;
            foreach (KeyValuePair<string, HotkeyCommand[]> kvp in hotkeys)
            {
                writer.WriteStartElement("Category");
                writer.WriteAttributeString("name", kvp.Key);

                foreach (HotkeyCommand hk in kvp.Value)
                {
                    writer.WriteStartElement("Hotkey");
                    writer.WriteAttributeString("command", hk.CommandCode.ToString());
                    writer.WriteAttributeString("name", hk.Name);
                    writer.WriteAttributeString("key", hk.KeyData.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
        }

        public void ReadXML(XmlReader reader)
        {
            Dictionary<string, HotkeyCommand[]> hotkeys = new Dictionary<string, HotkeyCommand[]>();

            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Hotkeys")
                    ParseHotkeys(reader, hotkeys);
                else
                    reader.ReadOuterXml();
            }

            reader.ReadEndElement();

            HotkeySettingsManager.Import(hotkeys);
        }

        private void ParseHotkeys(XmlReader reader, Dictionary<string, HotkeyCommand[]> hotkeys)
        {
            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (empty)
                return;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Category")
                    ParseCategory(reader, hotkeys);
                else
                    reader.ReadOuterXml();
            }

            reader.ReadEndElement();
        }

        private void ParseCategory(XmlReader reader, Dictionary<string, HotkeyCommand[]> hotkeys)
        {
            string name = reader.GetAttribute("name");

            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (empty)
                return;

            if (hotkeys.ContainsKey(name))
                return;
            
            List<HotkeyCommand> hotkeysCommands = new List<HotkeyCommand>();

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Hotkey")
                {
                    HotkeyCommand hotkey = ParseHotkey(reader);
                    if (hotkey != null)
                        hotkeysCommands.Add(hotkey);
                }
                else
                    reader.ReadOuterXml();
            }

            hotkeys.Add(name, hotkeysCommands.ToArray());
            reader.ReadEndElement();
        }

        private HotkeyCommand ParseHotkey(XmlReader reader)
        {
            bool empty = reader.IsEmptyElement;
            if (!empty)
            {
                reader.ReadOuterXml();
                return null;
            }
            
            string strCommand = reader.GetAttribute("command");
            string name = reader.GetAttribute("name");
            string strKey = reader.GetAttribute("key");

            int command = int.Parse(strCommand);
            Keys key = (Keys)Enum.Parse(typeof(Keys), strKey);

            reader.ReadStartElement();

            return new HotkeyCommand(command, name, key);
        }

        /*

        
        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "MaxRecentFiles":
                        maxRecentFiles = reader.ReadElementContentAsInt();
                        break;
                    case "RecentFiles":
                        ParseRecentFiles(reader);
                        break;
                    case "Shortcuts":
                        ParseShortcuts(reader);
                        break;
                    case "ThumbnailSize":
                        explorerThumbsSize = (ExplorerThumbSize)Enum.Parse(typeof(ExplorerThumbSize), reader.ReadElementContentAsString());
                        break;
                    case "ExplorerFilesSplitterDistance":
                        explorerFilesSplitterDistance = reader.ReadElementContentAsInt();
                        break;
                    case "ShortcutsFilesSplitterDistance":
                        shortcutsFilesSplitterDistance = reader.ReadElementContentAsInt();
                        break;
                    case "ActiveTab":
                        activeTab = (ActiveFileBrowserTab)Enum.Parse(typeof(ActiveFileBrowserTab), reader.ReadElementContentAsString());
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }

        private void ParseRecentFiles(XmlReader reader)
        {
            recentFiles.Clear();
            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement();

            if (empty)
                return;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "RecentFile")
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

            if (empty)
                return;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Shortcut")
                {
                    ShortcutFolder shortcut = new ShortcutFolder(reader);
                    if (Directory.Exists(shortcut.Location))
                        shortcutFolders.Add(shortcut);
                }
                else
                {
                    reader.ReadOuterXml();
                }
            }

            reader.ReadEndElement();

            shortcutFolders.Sort();

        }*/

    }
}
