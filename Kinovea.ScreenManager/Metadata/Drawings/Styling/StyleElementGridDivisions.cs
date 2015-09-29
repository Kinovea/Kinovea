#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Style element to represent the number of divisions used by a grid drawing.
    /// Editor: regular combo box.
    /// </summary>
    public class StyleElementGridDivisions : AbstractStyleElement
    {
        #region Properties
        public static readonly string[] Options;
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
            get { return Properties.Drawings.grid;}
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.Generic_GridDivisionsPicker;}
        }
        public override string XmlName
        {
            get { return "GridDivisions";}
        }
        #endregion
        
        #region Members
        private int value;
        private static readonly int defaultValue = 8;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        static StyleElementGridDivisions()
        {
            List<string> d = new List<string>();
            for (int i = 2; i < 21; i++)
                d.Add(i.ToString());

            Options = d.ToArray();
        }

        public StyleElementGridDivisions(int initialValue)
        {
            value = (Array.IndexOf(Options, initialValue.ToString()) >= 0) ? initialValue : defaultValue;
        }
        public StyleElementGridDivisions(XmlReader _xmlReader)
        {
            ReadXML(_xmlReader);
        }
        #endregion

        #region Public Methods
        public override Control GetEditor()
        {
            ComboBox editor = new ComboBox();
            editor.DropDownStyle = ComboBoxStyle.DropDownList;
            editor.Items.AddRange(Options);
            editor.SelectedIndex = Array.IndexOf(Options, value.ToString());
            editor.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);
            return editor;
        }
        public override AbstractStyleElement Clone()
        {
            AbstractStyleElement clone = new StyleElementGridDivisions(value);
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
                log.ErrorFormat("An error happened while parsing XML for Grid divisions. {0}", s);
            }
            
            // Restrict to the actual list of "athorized" values.
            this.value = (Array.IndexOf(Options, value.ToString()) >= 0) ? value : defaultValue;
            
            reader.ReadEndElement();
        }
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Value", value.ToString());
        }
        #endregion
        
        #region Private Methods
        private void editor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i;
            bool parsed = int.TryParse(((ComboBox)sender).Text, out i);
            this.value = parsed ? i : defaultValue;
            RaiseValueChanged();
            ((ComboBox)sender).Text = value.ToString();
        }
        #endregion
    }
}

