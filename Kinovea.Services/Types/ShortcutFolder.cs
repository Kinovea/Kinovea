#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// A class to encapsulate one item of the shortcut folders.
    /// </summary>
    public class ShortcutFolder : IComparable
    {
        public string Location 
        {
            get { return location; }
        }
        public string FriendlyName 
        {
            get { return friendlyName; }
        }
        
        private string friendlyName;		
        private string location;
        
        public ShortcutFolder(string friendlyName, string location)
        {
            this.friendlyName = friendlyName;
            this.location = location;
        }
        public override string ToString()
        {
            return friendlyName;
        }
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("FriendlyName", friendlyName);
            writer.WriteElementString("Location", location);
        }
        
        public ShortcutFolder(XmlReader reader)
        {
            reader.ReadStartElement();
            
            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "FriendlyName":
                        friendlyName = reader.ReadElementContentAsString();
                        break;
                    case "Location":
                        location = reader.ReadElementContentAsString();
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
            
            reader.ReadEndElement();
        }
    
        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            ShortcutFolder sf = obj as ShortcutFolder;
            if(sf != null)
            {
                String path1 = Path.GetFileName(this.location);
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
