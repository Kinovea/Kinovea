﻿#region License
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
using System.Collections.Generic;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent line endings (for arrows).
    /// Editor: owner drawn combo box.
    /// </summary>
    public class StyleElementLineEnding : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return value; }
            set 
            { 
                this.value = (value is LineEnding) ? (LineEnding)value : defaultValue;
                RaiseValueChanged();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.arrows;}
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_ArrowPicker;}
        }
        public override string XmlName
        {
            get { return "Arrows";}
        }
        #endregion

        public static List<LineEnding> options = new List<LineEnding>() { LineEnding.None, LineEnding.StartArrow, LineEnding.EndArrow, LineEnding.DoubleArrow }; 
        public static readonly LineEnding defaultValue = LineEnding.None;

        #region Members
        private LineEnding value;
        private static readonly int lineWidth = 6;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public StyleElementLineEnding(LineEnding initialValue)
        {
            value = options.IndexOf(initialValue) >= 0 ? initialValue : defaultValue;
        }
        public StyleElementLineEnding(XmlReader xmlReader)
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
            AbstractStyleElement clone = new StyleElementLineEnding(value);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");
            
            LineEnding value = LineEnding.None;
            try
            {
                TypeConverter lineEndingConverter = TypeDescriptor.GetConverter(typeof(LineEnding));
                value = (LineEnding)lineEndingConverter.ConvertFromString(s);
            }
            catch(Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Line ending. {0}", s);
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
            switch(options[e.Index])
            {
                case LineEnding.None:
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
                    break;
                case LineEnding.StartArrow:
                    p.StartCap = LineCap.ArrowAnchor;
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
                    break;
                case LineEnding.EndArrow:
                    p.EndCap = LineCap.ArrowAnchor;
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
                    break;
                case LineEnding.DoubleArrow:
                    p.StartCap = LineCap.ArrowAnchor;
                    p.EndCap = LineCap.ArrowAnchor;
                    e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Left + e.Bounds.Width, e.Bounds.Top + top);
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
            RaiseValueChanged();
        }
        #endregion
    }
}
