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
    /// The opaque field is kept in the serialized form until the actual camera manager instanciate a CameraSummary from the blurb.
    /// </summary>
    public class CameraBlurb
    {
        public string CameraType { get; private set;}
        public string Identifier { get; private set;}
        public string Alias { get; private set;}
        public Bitmap Icon { get; private set;}
        public Rectangle DisplayRectangle { get; private set; }
        public string AspectRatio { get; private set;}
        public string Specific { get; private set;}
        
        public CameraBlurb(string cameraType, string identifier, string alias, Bitmap icon, Rectangle displayRectangle, string aspectRatio, string specific)
        {
            this.CameraType = cameraType;
            this.Identifier = identifier;
            this.Alias = alias;
            this.Icon = icon;
            this.DisplayRectangle = displayRectangle;
            this.AspectRatio = aspectRatio;
            this.Specific = specific;
        }
        
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("CameraType", CameraType);
            writer.WriteElementString("Identifier", Identifier);
            writer.WriteElementString("Alias", Alias);
            writer.WriteElementString("Icon", XmlHelper.WriteBitmap(Icon));
            
            string displayRectangle = string.Format("{0};{1};{2};{3}", DisplayRectangle.X, DisplayRectangle.Y, DisplayRectangle.Width, DisplayRectangle.Height);
            writer.WriteElementString("DisplayRectangle", displayRectangle);
            
            writer.WriteElementString("AspectRatio", AspectRatio);
            
            if(!string.IsNullOrEmpty(Specific))
            {
                writer.WriteStartElement("Specific");
                writer.WriteRaw(Specific);
                writer.WriteEndElement();
            }
        }
        
        public static CameraBlurb FromXML(XmlReader reader)
        {
            reader.ReadStartElement();
            string cameraType ="";
            string identifier = "";
            string alias = "";
            Bitmap icon = null;
            Rectangle displayRectangle = Rectangle.Empty;
            string aspectRatio = "";
            string specific = "";
            
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
                case "DisplayRectangle":
                    displayRectangle = XmlHelper.ParseRectangle(reader.ReadElementContentAsString());
                    break;
                case "AspectRatio":
                    aspectRatio = reader.ReadElementContentAsString();
                    break;
                case "Specific":
                    specific = reader.ReadInnerXml();
                    break;
                default:
                    reader.ReadOuterXml();
                    break;
                }
            }
            
            reader.ReadEndElement();  
            
            return new CameraBlurb(cameraType, identifier, alias, icon, displayRectangle, aspectRatio, specific);
        }
    }
}
