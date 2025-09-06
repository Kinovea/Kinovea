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
using System.IO;
using System.Xml;

namespace Kinovea.Services
{
    public class ScreenDescriptorPlayback : IScreenDescriptor
    {
        public ScreenType ScreenType 
        {
            get { return ScreenType.Playback; }
        }

        /// <summary>
        /// Guid of the player screen into which this description should be reloaded.
        /// This is used to re-associate the autosave.kva after video load and restore metadata.
        /// </summary>
        public Guid Id { get; set; }

        public string FriendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(FullPath))
                {
                    return "Empty";
                }
                else if (IsReplayWatcher)
                {
                    // TODO: if the screen is using one of the named path
                    // return its name instead.
                    string directoryName = Path.GetDirectoryName(FullPath);
                    return Path.GetFileName(directoryName);
                }
                else
                {
                    return Path.GetFileNameWithoutExtension(FullPath);
                }
            }
        }

        /// <summary>
        /// Path to the video file to load.
        /// - For a single video this is the full path to the file.
        /// - For a replay watcher on a capture folder it is the GUID.
        /// - For a raw replay watcher this is the folder to monitor with a wildcard "*" as file name.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Whether the video should start playing immediately after load.
        /// </summary>
        public bool Autoplay { get; set; }

        /// <summary>
        /// Speed at which to set the speed slider, whether the video is auto-play or not.
        /// This is relative to the nominal playback speed, not real time.
        /// </summary>
        public double SpeedPercentage { get; set; }

        /// <summary>
        /// Whether the video should be stretched to fill the player screen estate.
        /// </summary>
        public bool Stretch { get; set; }

        /// <summary>
        /// Whether this screen is monitoring new files and loading them automatically.
        /// </summary>
        public bool IsReplayWatcher { get; set; }

        /// <summary>
        /// This screen is part of a dual replay workspace.
        /// This flag is not saved to the screen descriptor XML.
        /// </summary>
        public bool IsDualReplay { get; set; }

        
        public DateTime RecoveryLastSave { get; set; }
        
        public ScreenDescriptorPlayback()
        {
            Id = Guid.NewGuid();
            FullPath = "";
            Autoplay = false;
            SpeedPercentage = 100;
            Stretch = false;
            IsReplayWatcher = false;
            RecoveryLastSave = DateTime.MinValue;
            IsDualReplay = false;
        }

        public IScreenDescriptor Clone()
        {
            ScreenDescriptorPlayback clone = new ScreenDescriptorPlayback();
            clone.Id = this.Id;
            clone.FullPath = this.FullPath;
            clone.Autoplay = this.Autoplay;
            clone.SpeedPercentage = this.SpeedPercentage;
            clone.Stretch = this.Stretch;
            clone.IsReplayWatcher = this.IsReplayWatcher;
            clone.RecoveryLastSave = this.RecoveryLastSave;
            clone.IsDualReplay = this.IsDualReplay;
            return clone;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "FullPath":
                        FullPath = reader.ReadElementContentAsString();
                        break;
                    case "Autoplay":
                        Autoplay = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "SpeedPercentage":
                        float speed;
                        bool read = float.TryParse(reader.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out speed);
                        if (read)
                            this.SpeedPercentage = speed;
                        break;
                    case "Stretch":
                        Stretch = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "IsReplayWatcher":
                        IsReplayWatcher = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
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
            w.WriteElementString("FullPath", FullPath);
            w.WriteElementString("Autoplay", XmlHelper.WriteBoolean(Autoplay));
            w.WriteElementString("SpeedPercentage", XmlHelper.WriteFloat((float)SpeedPercentage));
            w.WriteElementString("Stretch", XmlHelper.WriteBoolean(Stretch));
            w.WriteElementString("IsReplayWatcher", XmlHelper.WriteBoolean(IsReplayWatcher));
        }
    }
}
