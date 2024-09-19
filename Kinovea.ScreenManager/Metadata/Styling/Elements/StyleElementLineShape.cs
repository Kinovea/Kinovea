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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent line shape.
    /// Editor: owner drawn combo box.
    /// </summary>
    public class StyleElementLineShape : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set
            {
                this.value = (value is LineShape) ? (LineShape)value : defaultValue;
                ExportValueToData();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.trackshape; }
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_TrackShapePicker; }
        }
        public override string XmlName
        {
            get { return "LineShape"; }
        }
        #endregion

        public static readonly List<LineShape> options = new List<LineShape>() { LineShape.Solid, LineShape.Dash, LineShape.Squiggle };
        public static readonly LineShape defaultValue = LineShape.Solid;

        #region Members
        private LineShape value;
        private static readonly int lineWidth = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleElementLineShape(LineShape initialValue)
        {
            value = options.IndexOf(initialValue) >= 0 ? initialValue : defaultValue;
        }
        public StyleElementLineShape(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            ComboBox editor = new ComboBox();
            editor.DropDownStyle = ComboBoxStyle.DropDownList;
            editor.ItemHeight = 15;
            editor.DrawMode = DrawMode.OwnerDrawFixed;

            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                editor.Items.Add(new object());

                if (options[i] == value)
                    selectedIndex = i;
            }

            editor.SelectedIndex = selectedIndex;
            editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementLineShape(value);
            clone.BindClone(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");

            LineShape value = LineShape.Solid;
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(LineShape));
                value = (LineShape)converter.ConvertFromString(s);
            }
            catch (Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Track shape. {0}", s);
            }

            this.value = options.IndexOf(value) >= 0 ? value : defaultValue;
            xmlReader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(value);
            string s = converter.ConvertToString(value);
            xmlWriter.WriteElementString("Value", s);
        }
        #endregion

        #region Private Methods
        private void editor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= options.Count)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int top = e.Bounds.Height / 2;

            Brush backgroundBrush = Brushes.White;
            if ((e.State & DrawItemState.Focus) != 0)
                backgroundBrush = Brushes.LightSteelBlue;

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

            Pen p = new Pen(Color.Black, lineWidth);
            switch (options[e.Index])
            {
                case LineShape.Solid:
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
                    break;
                case LineShape.Dash:
                    p.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
                    break;
                case LineShape.Squiggle:
                    e.Graphics.DrawSquigglyLine(p, e.Bounds.Left - 20, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width + 20, e.Bounds.Top + top);
                    break;
            }
            
            p.Dispose();
        }
        private void editor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox editor = sender as ComboBox;
            if (editor == null)
                return;

            if (editor.SelectedIndex < 0)
                return;

            value = options[editor.SelectedIndex];
            ExportValueToData();
        }
        #endregion
    }
}
