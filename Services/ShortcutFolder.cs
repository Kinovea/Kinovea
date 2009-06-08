#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using System.IO;
using System.Xml;

namespace Videa.Services
{
	/// <summary>
	/// A class to encapsulate one item of the shortcut folders.
	/// </summary>
	public class ShortcutFolder : IComparable
	{
		#region Properties
		public string Location 
		{
			get { return m_Location; }
			set { m_Location = value; }
		}
		public string FriendlyName 
		{
			get { return m_FriendlyName; }
			set { m_FriendlyName = value; }
		}
		#endregion
		
		private string m_FriendlyName;		
		private string m_Location;
		
		public ShortcutFolder(string _friendlyName, string _location)
		{
			m_FriendlyName = _friendlyName;
			m_Location = _location;
		}
		public override string ToString()
		{
			return m_FriendlyName;
		}
		public void ToXml(XmlTextWriter _xmlWriter)
		{
			_xmlWriter.WriteStartElement("Shortcut");
			
			_xmlWriter.WriteStartElement("FriendlyName");
            _xmlWriter.WriteString(m_FriendlyName);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("Location");
            _xmlWriter.WriteString(m_Location);
            _xmlWriter.WriteEndElement();
			
			_xmlWriter.WriteEndElement();	
		}
		public static ShortcutFolder FromXml(XmlReader _xmlReader)
		{
			// When we land in this method we MUST already be at the "Shortcut" node.
		
			string friendlyName = "";
			string location = "";
			
			while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "FriendlyName")
                    {
                        friendlyName = _xmlReader.ReadString();
                    }
                    else if(_xmlReader.Name == "Location")
                    {
                        location = _xmlReader.ReadString();
                    }
                }
                else if (_xmlReader.Name == "Shortcut")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
			}
			
			ShortcutFolder sf = null;
			if(location.Length > 0)
			{
				sf = new ShortcutFolder(friendlyName, location);
			}
			
			return sf;
		}
	
		#region IComparable Implementation
        public int CompareTo(object obj)
        {
        	ShortcutFolder sf = obj as ShortcutFolder;
            if(sf != null)
            {
            	String path1 = Path.GetFileName(this.m_Location);
            	String path2 = Path.GetFileName(sf.Location);
            	return path1.CompareTo(path2);
            }
            else
            {
                throw new ArgumentException("Impossible comparison");
            }
        }
        #endregion
	}
}
