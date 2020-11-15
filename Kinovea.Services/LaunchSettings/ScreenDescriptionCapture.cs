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
using System.Xml;

namespace Kinovea.Services
{
    public class ScreenDescriptionCapture : IScreenDescription
    {
        public ScreenType ScreenType 
        {
            get { return ScreenType.Capture; }
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
        public double Delay { get; set; }

        /// <summary>
        /// Whether the camera view should be stretched to fill the capture screen estate.
        /// </summary>
        public bool Stretch { get; set; }

        public ScreenDescriptionCapture()
        {
            CameraName = "";
            Autostream = true;
            Delay = 0;
            Stretch = true;
        }

        public ScreenDescriptionCapture(XmlReader reader) : this()
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
                        Delay = reader.ReadElementContentAsDouble();
                        break;
                    case "Stretch":
                        Stretch = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }
}
