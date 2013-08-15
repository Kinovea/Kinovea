#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
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

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent line width.
    /// Editor: owner drawn combo box.
    /// Very similar to StyleElementPenSize, just the rendering changes. (lines vs circles)
    /// </summary>
    public class StyleElementLineSize : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return penSize; }
            set 
            { 
                penSize = (value is int) ? (int)value : defaultSize;
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
        
        #region Members
        private static readonly int[] options = { 1, 2, 3, 4, 5, 7, 9, 11, 13 };
        private static readonly int defaultSize = 3;
        private int penSize;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public StyleElementLineSize(int givenDefault)
        {
            penSize = (Array.IndexOf(options, givenDefault) >= 0) ? givenDefault : defaultSize;
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
            editor.ItemHeight = options[options.Length-1] + 4;
            editor.DrawMode = DrawMode.OwnerDrawFixed;
            foreach(int i in options) editor.Items.Add(new object());
            editor.SelectedIndex = Array.IndexOf(options, penSize);
            editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementLineSize(penSize);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");
            
            int value = defaultSize;
            try
            {
                TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
                value = (int)intConverter.ConvertFromString(s);
            }
            catch(Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Line size. {0}", s);
            }
            
            // Restrict to the actual list of "athorized" values.
            penSize = (Array.IndexOf(options, value) >= 0) ? value : defaultSize;
            
            xmlReader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", penSize.ToString());
        }
        #endregion
        
        #region Private Methods
        private void editor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= options.Length)
                return;
            
            int itemPenSize = options[e.Index];
            int top = (e.Bounds.Height - itemPenSize) / 2;
            e.Graphics.FillRectangle(Brushes.Black, e.Bounds.Left, e.Bounds.Top + top, e.Bounds.Width, itemPenSize);
        }
        private void editor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if( index >= 0 && index < options.Length)
            {
                penSize = options[index];
                RaiseValueChanged();
            }
        }
        #endregion
    }
}
