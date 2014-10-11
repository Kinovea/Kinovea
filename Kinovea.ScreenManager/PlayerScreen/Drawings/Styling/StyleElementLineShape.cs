#region License
/*
Copyright © Joan Charmant 2014.
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
    /// Style element to represent line shape.
    /// Editor: owner drawn combo box.
    /// </summary>
    public class StyleElementLineShape : AbstractStyleElement
    {
        #region Properties
        public static readonly LineShape[] Options = { LineShape.Solid, LineShape.Dash, LineShape.Squiggle };
        public override object Value
        {
            get { return lineShape; }
            set
            {
                lineShape = (value is LineShape) ? (LineShape)value : LineShape.Solid;
                RaiseValueChanged();
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

        #region Members
        private LineShape lineShape;
        private static readonly int lineWidth = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleElementLineShape(LineShape givenDefault)
        {
            lineShape = (Array.IndexOf(Options, givenDefault) >= 0) ? givenDefault : LineShape.Solid;
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
            for (int i = 0; i < Options.Length; i++) 
                editor.Items.Add(new object());
            
            editor.SelectedIndex = Array.IndexOf(Options, lineShape);
            editor.DrawItem += new DrawItemEventHandler(editor_DrawItem);
            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementLineShape(lineShape);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");

            LineShape value = LineShape.Solid;
            try
            {
                TypeConverter trackShapeConverter = TypeDescriptor.GetConverter(typeof(LineShape));
                value = (LineShape)trackShapeConverter.ConvertFromString(s);
            }
            catch (Exception)
            {
                log.ErrorFormat("An error happened while parsing XML for Track shape. {0}", s);
            }

            // Restrict to the actual list of "authorized" values.
            lineShape = (Array.IndexOf(Options, value) >= 0) ? value : LineShape.Solid;

            xmlReader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(lineShape);
            string s = converter.ConvertToString(lineShape);
            xmlWriter.WriteElementString("Value", s);
        }
        #endregion

        #region Private Methods
        private void editor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Options.Length)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int top = e.Bounds.Height / 2;

            Pen p = new Pen(Color.Black, lineWidth);
            switch (Options[e.Index])
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
            int index = ((ComboBox)sender).SelectedIndex;
            if (index >= 0 && index < Options.Length)
            {
                lineShape = Options[index];
                RaiseValueChanged();
            }
        }
        #endregion
    }
}
