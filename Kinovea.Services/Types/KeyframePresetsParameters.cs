using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Mapping of commands for named events to the actual name.
    /// This is used to create new keyframes with an preset name.
    /// </summary>
    public class KeyframePresetsParameters
    {
        #region Properties
        /// <summary>
        /// Mapping of command id to presets. 
        /// </summary>
        public List<KeyframePreset> Presets { get; set; } = new List<KeyframePreset>();

        #endregion

        private static readonly Color DefaultColor = Color.SteelBlue;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public KeyframePresetsParameters()
        {
            // Initialize with some anonymous colors.
            // This is only to bootstrap preferences files from before 0.9.6.
            // These presets will be overwritten by the ones saved in the preferences if any.
            Presets.Clear();
            Presets.Add(new KeyframePreset("", Color.FromArgb(51, 152, 255)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(51, 255, 255)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(152, 255, 51)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(255, 255, 51)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(255, 152, 51)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(255, 51, 51)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(255, 51, 255)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(152, 51, 255)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(51, 51, 255)));
            Presets.Add(new KeyframePreset("", Color.FromArgb(152, 152, 152)));
        }

        /// <summary>
        /// Returns a deep clone of this object.
        /// </summary>
        public KeyframePresetsParameters Clone()
        {
            KeyframePresetsParameters clone = new KeyframePresetsParameters();
            clone.Presets.Clear();
            foreach (var preset in Presets)
                clone.Presets.Add(preset);

            return clone;
        }

        public int GetContentHash()
        {
            int hash = 0;
            foreach (var preset in Presets)
                hash ^= preset.GetHashCode();

            return hash;
        }

        public KeyframePreset GetPreset(PlayerScreenCommands command)
        {
            KeyframePreset preset = new KeyframePreset("", DefaultColor);
            int index = ((int)command) - (int)PlayerScreenCommands.Preset1;
            if (Presets.Count > index)
                preset = Presets[index];

            return preset;
        }

        #region Serialization
        public void ReadXml(XmlReader r)
        {
            Presets.Clear();

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "KeyframePreset":
                        ParsePreset(r);
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        private void ParsePreset(XmlReader r)
        {
            bool empty = r.IsEmptyElement;

            r.ReadStartElement();
            if (empty)
                return;

            string name = "";
            Color color = DefaultColor;

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Name")
                {
                    name = r.ReadElementContentAsString();
                }
                else if (r.Name == "Color")
                {
                    color = XmlHelper.ParseColor(r.ReadElementContentAsString(), Color.SteelBlue);
                }
                else
                {
                    r.ReadOuterXml();
                }
            }

            r.ReadEndElement();
            
            KeyframePreset preset = new KeyframePreset(name, color);
            Presets.Add(preset);
        }

        public void WriteXml(XmlWriter w)
        {
            foreach (var preset in Presets)
            {
                w.WriteStartElement("KeyframePreset");
                w.WriteElementString("Name", preset.Name);
                w.WriteElementString("Color", XmlHelper.WriteColor(preset.Color, false));
                w.WriteEndElement();
            }
        }
        #endregion
    }
}
