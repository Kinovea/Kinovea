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
using System.Drawing;
using System.Xml;

namespace Kinovea.Services
{
    public class DeviceConfiguration
	{
		public string ID
		{
		    get { return id;}
		}
		public DeviceCapability Capability
		{
		    get { return capability; }
		}
		
		private string id;
		private DeviceCapability capability;
		
		public DeviceConfiguration(string id, DeviceCapability capability)
		{
		    this.id = id;
		    this.capability = capability;
		}
		
		public void UpdateCapability(DeviceCapability capability)
		{
		    this.capability = new DeviceCapability(capability.FrameSize, capability.Framerate);
		}
		
		public void WriteXML(XmlWriter writer)
		{
		    writer.WriteElementString("Identification", ID);
		    writer.WriteElementString("Size", string.Format("{0};{1}", Capability.FrameSize.Width, Capability.FrameSize.Height));
		    writer.WriteElementString("Framerate", Capability.Framerate.ToString());
		}
		
		public DeviceConfiguration(XmlReader reader)
		{
		    Size size = Size.Empty;
		    int framerate = 0;
		    
		    reader.ReadStartElement();
		    
		    while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
				{
                    case "Identification":
                        id = reader.ReadElementContentAsString();
                        break;
                    case "Size":
                        size = XmlHelper.ParseSize(reader.ReadElementContentAsString());
                        break;
                    case "Framerate":
                        framerate = reader.ReadElementContentAsInt();
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
		    
		    reader.ReadEndElement();
		    
		    capability = new DeviceCapability(size, framerate);
		}
	}
}
