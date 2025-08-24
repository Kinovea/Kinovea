#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Globalization;
using System.Xml;

namespace Kinovea.Services
{
    public class ScreenDescriptionCapture : IScreenDescriptor
    {
        public ScreenType ScreenType 
        {
            get { return ScreenType.Capture; }
        }

        public string FriendlyName
        {
            get 
            {
                if (string.IsNullOrEmpty(CameraName))
                    return "Empty";
                else
                    return CameraName; 
            }
        }

        /// <summary>
        /// The name of the camera to load.
        /// The first camera with this alias will be picked.
        /// </summary>
        public string CameraName { get; set; }

        /// <summary>
        /// Whether the camera should start streaming immediately after load.
        /// </summary>
        public bool Autostream { get; set; }

        /// <summary>
        /// Delay at which to set the delay slider, whether the video is auto-play or not.
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// Whether the display shows the live feed or the delayed feed.
        /// </summary>
        public bool DelayedDisplay { get; set; }

        public ScreenDescriptionCapture()
        {
            CameraName = "";
            Autostream = true;
            Delay = 0;
            DelayedDisplay = true;
        }

        public IScreenDescriptor Clone()
        {
            ScreenDescriptionCapture sdc = new ScreenDescriptionCapture();
            sdc.CameraName = this.CameraName;
            sdc.Autostream = this.Autostream;
            sdc.Delay = this.Delay;
            sdc.DelayedDisplay = this.DelayedDisplay;
            return sdc;
        }

        public void Readxml(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "CameraName":
                        CameraName = reader.ReadElementContentAsString();
                        break;
                    case "Autostream":
                        Autostream = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "Delay":
                        float delay;
                        bool read = float.TryParse(reader.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out delay);
                        if (read)
                            this.Delay = delay;
                        break;
                    case "DelayedDisplay":
                        DelayedDisplay = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("CameraName", CameraName);
            w.WriteElementString("Autostream", XmlHelper.WriteBoolean(Autostream));
            w.WriteElementString("Delay", XmlHelper.WriteFloat(Delay));
            w.WriteElementString("DelayedDisplay", XmlHelper.WriteBoolean(DelayedDisplay));
        }
    }
}
