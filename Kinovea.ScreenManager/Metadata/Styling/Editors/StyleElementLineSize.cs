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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent line size.
    /// Editor: owner drawn combo box.
    /// Very similar to StyleElementPenSize, just the rendering changes. (lines vs circles)
    /// </summary>
    public class StyleElementLineSize : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set 
            { 
                this.value = (value is int) ? (int)value : defaultValue;
                RaiseValueChanged();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.linesize;}
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_LineSizePicker;}
        }
        public override string XmlName
        {
            get { return "LineSize";}
        }
        #endregion

        public static readonly List<int> options = new List<int>() { 1, 2, 3, 4, 6, 8, 10, 12, 14, 18, 24, 30, 36 };
        private static readonly int defaultValue = 3;
        
        #region Members
        private int value;
        private int itemHeight = 18;
        private int textMargin = 20;
        private static readonly Font font = new Font("Arial", 8, FontStyle.Bold);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public StyleElementLineSize(int initialValue)
        {
            value = options.PickAmong(initialValue);
        }
        public StyleElementLineSize(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion
        
        #region Public Methods
        public override Control GetEditor()
        {
            ComboBox editor = new ComboBox();
            editor.DropDownStyle = ComboBoxStyle.DropDownList;
            editor.ItemHeight = itemHeight;
            editor.DrawMode = DrawMode.OwnerDrawFixed;

            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                editor.Items.Add(new object());

                if (options[i] == value)
                    selectedIndex = i;
            } 
            
            editor.SelectedIndex = selectedIndex;
            editor.DrawItem += editor_DrawItem;
            editor.SelectedIndexChanged += editor_SelectedIndexChanged;
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementLineSize(value);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");
            
            int value = defaultValue;
            try
            {
                TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                value = (int)intConverter.ConvertFromString(s);
            }
            catch(Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Line size. {0}", s);
            }

            this.value = options.PickAmong(value);
            xmlReader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", value.ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region Private Methods
        
        private void editor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= options.Count)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw a rectangle vertically centered, of the same height as the line weight.
            // Also add the value as text in front of the line drawing.
            int itemValue = options[e.Index];
            int itemSize = Math.Min(itemValue, itemHeight - 2);
            int top = (e.Bounds.Height - itemSize) / 2;

            Brush foregroundBrush = Brushes.Black;
            Brush backgroundBrush = Brushes.White;
            if ((e.State & DrawItemState.Focus) != 0)
                backgroundBrush = Brushes.LightSteelBlue;

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawString(itemValue.ToString(CultureInfo.InvariantCulture), font, foregroundBrush, e.Bounds.Left, e.Bounds.Top + 2);
            e.Graphics.FillRectangle(foregroundBrush, e.Bounds.Left + textMargin, e.Bounds.Top + top, e.Bounds.Width - textMargin, itemSize);
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
        }
        #endregion
    }
}
