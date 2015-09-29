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
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent a boolean value used by the drawing.
    /// At the moment this is specific to the "curved" value for polylines, 
    /// but this could probably be factored when another boolean value arise.
    /// Editor: checkbox.
    /// </summary>
    public class StyleElementCurved : AbstractStyleElement
    {
        #region Properties
        public override object Value
        {
            get { return v; }
            set
            {
                v = (value is Boolean) ? (Boolean)value : false;
                RaiseValueChanged();
            }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.curve; }
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_Curved; }
        }
        public override string XmlName
        {
            get { return "Curved"; }
        }
        #endregion

        #region Members
        protected bool v;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleElementCurved(bool defaultValue)
        {
            v = defaultValue;
        }
        public StyleElementCurved(XmlReader xmlReader)
        {
            ReadXML(xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            CheckBox editor = new CheckBox();
            editor.Checked = v;
            editor.CheckedChanged += editor_CheckedChanged;

            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementCurved(v);
            clone.Bind(this);
            return clone;
        }
        public override void ReadXML(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            string s = xmlReader.ReadElementContentAsString("Value", "");
            v = XmlHelper.ParseBoolean(s);
            xmlReader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteElementString("Value", v.ToString().ToLower());
        }
        #endregion

        #region Private Methods
        private void editor_CheckedChanged(object sender, EventArgs e)
        {
            v = ((CheckBox)sender).Checked;
            RaiseValueChanged();
        }
        #endregion
    }
}
