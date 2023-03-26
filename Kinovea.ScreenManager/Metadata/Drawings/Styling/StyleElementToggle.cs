#region License
/*
Copyright © Joan Charmant 2014.
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent a boolean value used by the drawing.
    /// Editor: checkbox.
    /// </summary>
    public class StyleElementToggle : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set
            {
                this.value = (value is Boolean) ? (Boolean)value : defaultValue;
                RaiseValueChanged();
            }
        }
        public override Bitmap Icon
        {
            get { return icon; }
        }
        public override string DisplayName
        {
            get { return displayName; }
        }
        public override string XmlName
        {
            get { return "Toggle"; }
        }

        /// <summary>
        /// Returns true if this option should be hidden from the user interface.
        /// </summary>
        public bool IsHidden
        {
            get { return DrawingStyle.IsHiddenToggle(variant); }
        }
        #endregion

        public static readonly bool defaultValue = false;

        #region Members
        private bool value;
        private Bitmap icon;
        private StyleToggleVariant variant = StyleToggleVariant.Unknown;
        private string displayName;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleElementToggle(bool initialValue, StyleToggleVariant variant)
        {
            this.value = initialValue;
            this.variant = variant;
            UpdateVariant(variant);
        }
        public StyleElementToggle(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            CheckBox editor = new CheckBox();
            editor.Checked = value;
            editor.CheckedChanged += editor_CheckedChanged;

            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            StyleElementToggle clone = new StyleElementToggle(value, variant);
            clone.icon = icon;
            clone.displayName = displayName;
            clone.variant = variant;
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();	// <Toggle>

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "Value":
                        value = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "Variant":
                        variant = (StyleToggleVariant)Enum.Parse(typeof(StyleToggleVariant), xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in style element toggle XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();

            // If the variant is unknown, the most likely cause is that it's an older file where "Curved" was assumed.
            // Worst case scenario from doing this is that we show "Curved" instead of "Unknown" in the mini editor.
            if (variant == StyleToggleVariant.Unknown)
            {
                variant = StyleToggleVariant.Curved;
                log.ErrorFormat("Unknown variant in style element toggle. Assumed: {0}", variant.ToString());
            }
    
            UpdateVariant(variant);
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", value.ToString().ToLower());
            xmlWriter.WriteElementString("Variant", variant.ToString());
        }
        #endregion

        #region Private Methods
        private void editor_CheckedChanged(object sender, EventArgs e)
        {
            value = ((CheckBox)sender).Checked;
            RaiseValueChanged();
        }
        private void UpdateVariant(StyleToggleVariant variant)
        {
            // Update the appearance of the mini editor based on the variant.
            switch (variant)
            {
                case StyleToggleVariant.Perspective:
                    icon = Properties.Drawings.plane;
                    displayName = ScreenManagerLang.Generic_Perspective;
                    break;
                case StyleToggleVariant.Clock:
                    icon = Properties.Drawings.stopwatch;
                    displayName = ScreenManagerLang.Generic_Clock;
                    break;
                case StyleToggleVariant.DistanceGrid:
                    icon = Properties.Drawings.plane;
                    displayName = "Distance grid";
                    break;
                case StyleToggleVariant.Curved:
                default:
                    icon = Properties.Drawings.curve;
                    displayName = ScreenManagerLang.Generic_Curved;
                    break;
            }
        }
        #endregion
    }
}
