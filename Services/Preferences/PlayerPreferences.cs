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
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

using Kinovea.Video;

namespace Kinovea.Services
{
    public class PlayerPreferences : IPreferenceSerializer
    {
        #region Properties
        public string Name
        {
            get { return "Player"; }
        }
        public TimecodeFormat TimecodeFormat
        {
            get { return timecodeFormat; }
            set { timecodeFormat = value; }
        }
        public SpeedUnit SpeedUnit
        {
            get { return speedUnit; }
            set { speedUnit = value; }
        }
        public ImageAspectRatio AspectRatio
		{
			get { return aspectRatio; }
			set { aspectRatio = value; }
		}
        public bool DeinterlaceByDefault
		{
			get { return deinterlaceByDefault; }
			set { deinterlaceByDefault = value; }
		}
        public int WorkingZoneSeconds
        {
            get { return workingZoneSeconds; }
            set { workingZoneSeconds = value; }
        }
        public int WorkingZoneMemory
        {
            get { return workingZoneMemory; }
            set { workingZoneMemory = value; }
        }
        public bool SyncLockSpeed
        {
            get { return syncLockSpeed;}
            set { syncLockSpeed = value;}
        }
        public InfosFading DefaultFading
        {
            get { return defaultFading; }
            set { defaultFading = value; }
        }
		public int MaxFading
		{
			get { return maxFading; }
			set { maxFading = value; }
		}
        public bool DrawOnPlay
        {
            get { return drawOnPlay; }
            set { drawOnPlay = value; }
        }
        public List<Color> RecentColors
		{
			get { return recentColors; }
		}
        #endregion
        
        private TimecodeFormat timecodeFormat = TimecodeFormat.ClassicTime;
        private SpeedUnit speedUnit = SpeedUnit.MetersPerSecond;
        private ImageAspectRatio aspectRatio = ImageAspectRatio.Auto;
        private bool deinterlaceByDefault;
        private int workingZoneSeconds = 12;
        private int workingZoneMemory = 512;
        private InfosFading defaultFading = new InfosFading();
        private int maxFading = 200;
        private bool drawOnPlay = true;
        private List<Color> recentColors = new List<Color>();
        private int maxRecentColors = 12;
        private bool syncLockSpeed = true;
        
        public void AddRecentColor(Color _color)
    	{
    	    PreferencesManager.UpdateRecents(_color, recentColors, maxRecentColors);
    	}
        
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("TimecodeFormat", timecodeFormat.ToString());
            writer.WriteElementString("SpeedUnit", speedUnit.ToString());
            writer.WriteElementString("AspectRatio", aspectRatio.ToString());
            writer.WriteElementString("DeinterlaceByDefault", deinterlaceByDefault ? "true" : "false");
            writer.WriteElementString("WorkingZoneSeconds", workingZoneSeconds.ToString());
            writer.WriteElementString("WorkingZoneMemory", workingZoneMemory.ToString());
            writer.WriteElementString("SyncLockSpeed", syncLockSpeed ? "true" : "false");
            
            writer.WriteStartElement("InfoFading");
            defaultFading.WriteXml(writer);
            writer.WriteEndElement();
            
            writer.WriteElementString("MaxFading", maxFading.ToString());
            writer.WriteElementString("DrawOnPlay", drawOnPlay ? "true" : "false");
            
            if(recentColors.Count > 0)
            {
                writer.WriteStartElement("RecentColors");
                
                for(int i = 0; i < maxRecentColors; i++)
                {
                    if(i >= recentColors.Count)
                        break;
                    
                    writer.WriteElementString("RecentColor", string.Format("{0};{1};{2}", recentColors[i].R.ToString(), recentColors[i].G.ToString(), recentColors[i].B.ToString()));
                }
                writer.WriteEndElement();
            }
            
            writer.WriteElementString("MaxRecentColors", maxRecentColors.ToString());
        }
        
        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
			{
				switch(reader.Name)
				{
					case "TimecodeFormat":
				        timecodeFormat = (TimecodeFormat) Enum.Parse(typeof(TimecodeFormat), reader.ReadElementContentAsString());
                        break;
                    case "SpeedUnit":
                        speedUnit = (SpeedUnit) Enum.Parse(typeof(SpeedUnit), reader.ReadElementContentAsString());
                        break;
                    case "AspectRatio":
                        aspectRatio = (ImageAspectRatio) Enum.Parse(typeof(ImageAspectRatio), reader.ReadElementContentAsString());
                        break;
                    case "DeinterlaceByDefault":
                        deinterlaceByDefault = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "WorkingZoneSeconds":
                        workingZoneSeconds = reader.ReadElementContentAsInt();
                        break;
                    case "WorkingZoneMemory":
                        workingZoneMemory = reader.ReadElementContentAsInt();
                        break;
                    case "SyncLockSpeed":
                        syncLockSpeed = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "InfoFading":
                        defaultFading.ReadXml(reader);
                        break;
                    case "MaxFading":
                        maxFading = reader.ReadElementContentAsInt();
                        break;
                    case "DrawOnPlay":
                        drawOnPlay = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;                        
                    case "RecentColors":
                        ParseRecentColors(reader);
                        break;
                    case "MaxRecentColors":
                        maxRecentColors = reader.ReadElementContentAsInt();
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
				}
            }
            
            reader.ReadEndElement();
        }
        
        private void ParseRecentColors(XmlReader reader)
        {
    	    recentColors.Clear();
    	    bool empty = reader.IsEmptyElement;
            
    	    reader.ReadStartElement();
    	    
    	    if(empty)
    	        return;
            
            while(reader.NodeType == XmlNodeType.Element)
            {
                if(reader.Name == "RecentColor")
                    recentColors.Add(XmlHelper.ParseColor(reader.ReadElementContentAsString(), Color.Black));
                else
                    reader.ReadOuterXml();
            }
            
            reader.ReadEndElement();
        }
    }
}
