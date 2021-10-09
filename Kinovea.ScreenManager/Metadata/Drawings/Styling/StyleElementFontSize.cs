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
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent the size of font used by the drawing.
    /// Editor: regular combo box.
    /// </summary>
    public class StyleElementFontSize : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set 
            { 
                value = (value is int) ? (int)value : defaultValue;
                RaiseValueChanged();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.editortext;}
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_FontSizePicker;}
        }
        public override string XmlName
        {
            get { return "FontSize";}
        }
        #endregion

        public static List<int> options;
        public static readonly int defaultValue = 14;
        
        #region Members
        private int value;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        static StyleElementFontSize()
        {
            options = new List<int>() { 6, 7, 8, 9, 10, 11, 12, 14, 18, 24, 30, 36, 48, 60, 72, 96 };
        }

        public StyleElementFontSize(int initialValue)
        {
            value = options.PickAmong(initialValue);
        }
        public StyleElementFontSize(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            ComboBox editor = new ComboBox();
            editor.DropDownStyle = ComboBoxStyle.DropDownList;

            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                editor.Items.Add(GetDisplayValue(options[i]));

                if (options[i] == value)
                {
                    selectedIndex = i;
                    editor.Text = GetDisplayValue(value);
                }
            }

            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementFontSize(value);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();
            string s = reader.ReadElementContentAsString("Value", "");
            
            int value = defaultValue;
            try
            {
                TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                value = (int)intConverter.ConvertFromString(s);
            }
            catch(Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Font size. {0}", s);
            }

            this.value = options.PickAmong(value);
            reader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Find the best size based on a given text height.
        /// This used to dynamically change the value based on the user dragging a rectangle.
        /// It depends on the text because the text can have multiple lines.
        /// targetHeight is unscaled.
        /// </summary>
        public void ForceSize(int targetHeight, string text, Font font)
        {
            // We must loop through all allowed font size and compute the output rectangle to find the best match.
            // Look for the first local minima, as the list is linearly increasing.
            int minDiff = int.MaxValue;
            int bestCandidate = options[0];

            foreach (int size in options)
            {
                Font testFont = new Font(font.Name, size, font.Style);
                int height = (int)TextHelper.MeasureString(text + " ", testFont).Height;
                testFont.Dispose();
                
                int diff = Math.Abs(targetHeight - height);
                if (diff > minDiff)
                    break;
                
                minDiff = diff;
                bestCandidate = size;
            }

            value = bestCandidate;
            RaiseValueChanged();
        }
        #endregion

        #region Private Methods
        private static string GetDisplayValue(int value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
        private void editor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox editor = sender as ComboBox;
            if (editor == null)
                return;

            if (editor.SelectedIndex < 0)
                return;

            value = options[editor.SelectedIndex];
            RaiseValueChanged();

            editor.Text = GetDisplayValue(value);
        }
        #endregion
    }
}
