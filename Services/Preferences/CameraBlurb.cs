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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Small info about a camera, allowing the Camera Manager to match it against discovered camera.
    /// Can also contain necessary info to try to connect to a camera, for types that don't have passive discovery.
    /// </summary>
    public class CameraBlurb
    {
        public string CameraType { get; private set;}
        public string Identifier { get; private set;}
        public string Alias { get; private set;}
        public Bitmap Icon { get; private set;}
        //public object Opaque { get; private set;}
        
        public CameraBlurb(string cameraType, string identifier, string alias, Bitmap icon)
        {
            this.CameraType = cameraType;
            this.Identifier = identifier;
            this.Alias = alias;
            this.Icon = icon;
        }
        
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("CameraType", CameraType);
            writer.WriteElementString("Identifier", Identifier);
            writer.WriteElementString("Alias", Alias);
            
            string iconBase64 = XmlHelper.ImageToBase64(Icon, ImageFormat.Png);
            writer.WriteElementString("Icon", iconBase64);
            
            // TODO: add special info.
        }
        
        public static CameraBlurb FromXML(XmlReader reader)
        {
            reader.ReadStartElement();
            string cameraType ="";
            string identifier = "";
            string alias = "";
            Bitmap icon = null;
            
            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                case "CameraType":
                    cameraType = reader.ReadElementContentAsString();
                    break;
                case "Identifier":
                    identifier = reader.ReadElementContentAsString();
                    break;
                case "Alias":
                    alias = reader.ReadElementContentAsString();
                    break;
                case "Icon":
                    icon = XmlHelper.ParseImageFromBase64(reader.ReadElementContentAsString());
                    break;
                default:
                    reader.ReadOuterXml();
                    break;
                }
            }
            
            reader.ReadEndElement();  
            
            return new CameraBlurb(cameraType, identifier, alias, icon);
        }
        
        
        
        
    }
}
