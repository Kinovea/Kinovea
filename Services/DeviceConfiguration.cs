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
		public string id;
		public DeviceCapability cap;
		
		public void ToXml(XmlTextWriter _writer)
		{
			_writer.WriteStartElement("DeviceConfiguration");
			
			_writer.WriteStartElement("Identification");
        	_writer.WriteString(id);
        	_writer.WriteEndElement();
        	
        	_writer.WriteStartElement("Size");
        	_writer.WriteString(cap.FrameSize.Width.ToString() + ";" + cap.FrameSize.Height.ToString());
        	_writer.WriteEndElement();
        	
        	_writer.WriteStartElement("Framerate");
        	_writer.WriteString(cap.Framerate.ToString());
        	_writer.WriteEndElement();
        	
        	_writer.WriteEndElement();
		}
		public static DeviceConfiguration FromXml(XmlReader _xmlReader)
		{
			string id = "";
			Size frameSize = Size.Empty;
			int frameRate = 0;
			
			while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Identification")
                    {
                        id = _xmlReader.ReadString();
                    }
                    else if(_xmlReader.Name == "Size")
                    {
                    	Point p = XmlHelper.ParsePoint(_xmlReader.ReadString());
                    	frameSize = new Size(p);
                    }
                    else if(_xmlReader.Name == "Framerate")
                    {
                    	frameRate = int.Parse(_xmlReader.ReadString());
                    }
                }
                else if (_xmlReader.Name == "DeviceConfiguration")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
			}
			
			DeviceConfiguration conf = null;
			if(id.Length > 0)
			{
				conf = new DeviceConfiguration();
				conf.id = id;
				conf.cap = new DeviceCapability(frameSize, frameRate);
			}
			
			return conf;
		}
	}
}
