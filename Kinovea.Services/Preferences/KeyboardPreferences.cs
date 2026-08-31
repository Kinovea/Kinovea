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

            CommandBindings allBindings = HotkeySettingsManager.ActiveBindings;
            foreach (string category in allBindings.GetCategories())
            {
                List<HotkeyCommand> bindings = allBindings.GetCommandBindings(category);
                writer.WriteStartElement("Category");
                writer.WriteAttributeString("name", category);

                foreach (HotkeyCommand hk in bindings)
                {
                    writer.WriteStartElement("Hotkey");
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
            CommandBindings bindings = new CommandBindings();

            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Hotkeys")
                {
                    ParseHotkeys(reader, bindings);
                }
                else
                {
                    reader.ReadOuterXml();
                }
            }

            reader.ReadEndElement();

            HotkeySettingsManager.ActiveBindings.Load(bindings);
        }

        private void ParseHotkeys(XmlReader reader, CommandBindings bindings)
        {
            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (empty)
                return;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Category")
                    ParseCategory(reader, bindings);
                else
                    reader.ReadOuterXml();
            }

            reader.ReadEndElement();
        }

        private void ParseCategory(XmlReader reader, CommandBindings bindings)
        {
            string name = reader.GetAttribute("name");

            bool empty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (empty)
                return;

            if (bindings.HasCategory(name))
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
                {
                    reader.ReadOuterXml();
                }
            }

            bindings.AddCategory(name, hotkeysCommands);
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
            
            string name = reader.GetAttribute("name");
            string strKey = reader.GetAttribute("key");

            Keys key = (Keys)Enum.Parse(typeof(Keys), strKey);

            reader.ReadStartElement();

            return new HotkeyCommand(name, key);
        }
    }
}
