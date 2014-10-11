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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent a pen size.
    /// Editor: owner drawn combo box.
    /// Very similar to StyleElementLineStyle, just the rendering changes. (lines vs circles)
    /// </summary>
    public class StyleElementPenSize : AbstractStyleElement
    {
        #region Properties
        public static readonly int[] Options = { 2, 3, 4, 5, 7, 9, 11, 13, 16, 19, 22, 25 };
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
            get { return Properties.Drawings.editorpen;}
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_PenSizePicker;}
        }
        public override string XmlName
        {
            get { return "PenSize";}
        }
        #endregion
        
        #region Members
        private static readonly int defaultSize = 3;
        private int penSize;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public StyleElementPenSize(int givenDefault)
        {
            penSize = (Array.IndexOf(Options, givenDefault) >= 0) ? givenDefault : defaultSize;
        }
        public StyleElementPenSize(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion
        
        #region Public Methods
        public override Control GetEditor()
        {
            ComboBox editor = new ComboBox();
            editor.DropDownStyle = ComboBoxStyle.DropDownList;
            editor.ItemHeight = Options[Options.Length-1] + 2;
            editor.DrawMode = DrawMode.OwnerDrawFixed;
            foreach(int i in Options) editor.Items.Add(new object());
            editor.SelectedIndex = Array.IndexOf(Options, penSize);
            editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementPenSize(penSize);
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
                log.ErrorFormat("An error happened while parsing XML for Pen size. {0}", s);
            }
            
            // Restrict to the actual list of "athorized" values.
            penSize = (Array.IndexOf(Options, value) >= 0) ? value : defaultSize;
            
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
            if (e.Index < 0 || e.Index >= Options.Length)
                return;
            
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int itemPenSize = Options[e.Index];
            int left = (e.Bounds.Width - itemPenSize) / 2;
            int top = (e.Bounds.Height - itemPenSize) / 2;
            e.Graphics.FillEllipse(Brushes.Black, e.Bounds.Left + left, e.Bounds.Top + top, itemPenSize, itemPenSize);
        }
        private void editor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if( index >= 0 && index < Options.Length)
            {
                penSize = Options[index];
                RaiseValueChanged();
            }
        }
        #endregion
    }
}
