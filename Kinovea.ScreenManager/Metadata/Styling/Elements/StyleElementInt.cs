#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent an integer.
    /// Editor: regular numeric up-down.
    /// </summary>
    public class StyleElementInt : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set 
            { 
                this.value = (value is int) ? (int)value : defaultValue;
                ExportValueToData();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.coordinates_grid;}
        }
        public override string DisplayName
        {
            get { return displayName;}
        }
        public override string XmlName
        {
            get { return "Int";}
        }
        #endregion

        #region Members
        private int min = 0;
        private int max = 100;
        private int value = 10;
        private string displayName = "StyleElementInt";

        private static readonly int defaultValue = 10;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleElementInt(int value)
        {
            this.value = value;
        }
        public StyleElementInt(int min, int max, int value, string displayName)
        {
            // TODO: displayName should be the name of a resource, not a raw string.
            this.min = min;
            this.max = max;
            this.value = value;
            this.displayName = displayName;
        }
        public StyleElementInt(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            NumericUpDown editor = new NumericUpDown();
            NudHelper.FixNudScroll(editor);

            editor.Minimum = min;
            editor.Maximum = max;
            editor.Value = value;

            editor.ValueChanged += Editor_ValueChanged;
            return editor;
        }

        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementInt(min, max, value, displayName);
            clone.BindClone(this);
            return clone;
        }
        public override void ReadXML(XmlReader reader)
        {
            value = defaultValue;

            string displayName = reader.GetAttribute("DisplayName");
            if (!string.IsNullOrEmpty(displayName))
                this.displayName = displayName;

            reader.ReadStartElement();

            try
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Min":
                            min = reader.ReadElementContentAsInt("Min", "");
                            break;
                        case "Max":
                            max = reader.ReadElementContentAsInt("Max", "");
                            break;
                        case "Value":
                            value = reader.ReadElementContentAsInt("Value", "");
                            break;
                        default:
                            log.ErrorFormat("Element unknown in StyleElementInt: {0}", reader.ReadOuterXml());
                            break;
                    }
                }
            }
            catch(Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Int style element.");
            }

            reader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", value.ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region Private Methods
        private static string GetDisplayValue(int value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
        private void Editor_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown editor = sender as NumericUpDown;
            if (editor == null)
                return;

            value = (int)editor.Value;
            ExportValueToData();
        }
        #endregion
    }
}
