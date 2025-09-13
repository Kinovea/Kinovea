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
    public class ScreenDescriptorCapture : IScreenDescriptor
    {
        public ScreenType ScreenType 
        {
            get { return ScreenType.Capture; }
        }


        /// <summary>
        /// Guid of the screen descriptor.
        /// </summary>
        public Guid Id { get; set; }

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

        /// <summary>
        /// Maximum duration of the recording in seconds. 
        /// (approximate but should never be less than that).
        /// </summary>
        public float MaxDuration { get; set; }

        /// <summary>
        /// Id of the capture folder to use when recording.
        /// </summary>
        public Guid CaptureFolder { get; set; }

        /// <summary>
        /// File name to record to.
        /// May contain variables.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Whether the post-recording command is enabled.
        /// </summary>
        public bool EnableCommand { get; set; }

        /// <summary>
        /// Post-recording command data structure.
        /// </summary>
        public UserCommand UserCommand { get; set; }

        public bool CapturedFilesPanelForceCollapsed { get; set; }

        public ScreenDescriptorCapture()
        {
            Id = Guid.NewGuid();
            CameraName = "";
            Autostream = true;
            Delay = 0;
            DelayedDisplay = true;
            MaxDuration = 0;
            CaptureFolder = Guid.Empty;
            FileName = "";
            EnableCommand = false;
            UserCommand = new UserCommand();
            CapturedFilesPanelForceCollapsed = false;
        }

        public IScreenDescriptor Clone()
        {
            ScreenDescriptorCapture clone = new ScreenDescriptorCapture();
            clone.Id = Id;
            clone.CameraName = this.CameraName;
            clone.Autostream = this.Autostream;
            clone.Delay = this.Delay;
            clone.DelayedDisplay = this.DelayedDisplay;
            clone.MaxDuration = this.MaxDuration;
            clone.CaptureFolder = this.CaptureFolder;
            clone.FileName = this.FileName;
            clone.EnableCommand = this.EnableCommand;
            clone.UserCommand = this.UserCommand.Clone();
            clone.CapturedFilesPanelForceCollapsed = this.CapturedFilesPanelForceCollapsed;
            return clone;
        }

        public void Readxml(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Id":
                        Id = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        break;
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
                    case "MaxDuration":
                        float maxDuration;
                        read = float.TryParse(reader.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out maxDuration);
                        if (read)
                            this.MaxDuration = maxDuration;
                        break;
                    case "CaptureFolder":
                        CaptureFolder = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        break;
                    case "FileName":
                        FileName = reader.ReadElementContentAsString();
                        break;
                    case "EnableCommand":
                        EnableCommand = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "UserCommand":
                        UserCommand = new UserCommand(reader);
                        break;
                    case "CapturedFilesPanelForceCollapsed":
                        CapturedFilesPanelForceCollapsed = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
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
            w.WriteElementString("Id", Id.ToString());
            w.WriteElementString("CameraName", CameraName);
            w.WriteElementString("Autostream", XmlHelper.WriteBoolean(Autostream));
            w.WriteElementString("Delay", XmlHelper.WriteFloat(Delay));
            w.WriteElementString("DelayedDisplay", XmlHelper.WriteBoolean(DelayedDisplay));
            w.WriteElementString("MaxDuration", XmlHelper.WriteFloat(MaxDuration));
            w.WriteElementString("CaptureFolder", CaptureFolder.ToString());
            w.WriteElementString("FileName", FileName);
            w.WriteElementString("EnableCommand", XmlHelper.WriteBoolean(EnableCommand));
            
            if (UserCommand.Instructions.Count > 0)
            {
                w.WriteStartElement("UserCommand");
                UserCommand.WriteXML(w);
                w.WriteEndElement();
            }

            w.WriteElementString("CapturedFilesPanelForceCollapsed", XmlHelper.WriteBoolean(CapturedFilesPanelForceCollapsed));

        }
    }
}
