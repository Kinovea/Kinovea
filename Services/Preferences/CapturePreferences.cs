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
using System.Linq;

namespace Kinovea.Services
{
    public class CapturePreferences
    {
        #region Properties
        public string ImageDirectory
        {
        	get { return imageDirectory; }
			set { imageDirectory = value; }
		}
        public string VideoDirectory 
        {
        	get { return videoDirectory; }
			set { videoDirectory = value; }
		}        
		public KinoveaImageFormat ImageFormat
		{
			get { return imageFormat; }
			set { imageFormat = value; }
		}
		public KinoveaVideoFormat VideoFormat
		{
			get { return videoFormat; }
			set { videoFormat = value; }
		}
        public string ImageFile
        {
        	get { return imageFile; }
			set { imageFile = value; }
        }
		public string VideoFile
        {
        	get { return videoFile; }
			set { videoFile = value; }
        }
		public bool CaptureUsePattern
		{
			get { return usePattern; }
			set { usePattern = value; }
		}
		public string Pattern
		{
			get { return pattern; }
			set { pattern = value; }
		}
		public long CaptureImageCounter
        {
        	get { return imageCounter; }
            set { imageCounter = value;}	
        }
		public long CaptureVideoCounter
        {
        	get { return videoCounter; }
            set { videoCounter = value;}	
        }
		public int CaptureMemoryBuffer
        {
            get { return memoryBuffer; }
            set { memoryBuffer = value; }
        }
        /*public List<DeviceConfiguration> DeviceConfigurations
        {
        	get { return deviceConfigurations; }	
        }
		public string NetworkCameraUrl
		{
			get { return networkCameraUrl; }
			set { networkCameraUrl = value; }
		}      
		public NetworkCameraFormat NetworkCameraFormat
		{
			get { return networkCameraFormat; }
			set { networkCameraFormat = value; }
		}
		public List<string> RecentNetworkCameras
        {
        	get{ return recentNetworkCameras;}
        }*/
        public IEnumerable<CameraBlurb> CameraBlurbs
        {
            get { return cameraBlurbs.Values.Cast<CameraBlurb>(); }
        }
        #endregion
		
		#region Members
        private string imageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string videoDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private KinoveaImageFormat imageFormat = KinoveaImageFormat.JPG;
        private KinoveaVideoFormat videoFormat = KinoveaVideoFormat.MKV;
        private string imageFile = "";
        private string videoFile = "";
        private bool usePattern;
        private string pattern = "Cap-%y-%mo-%d - %i";
        private long imageCounter = 1;
        private long videoCounter = 1;
        private int memoryBuffer = 768;
        private Dictionary<string, CameraBlurb> cameraBlurbs = new Dictionary<string, CameraBlurb>();
        
		#endregion
        
        public void AddCamera(CameraBlurb blurb)
        {
            // Note: there should be a way to remove old entries.
            if(cameraBlurbs.ContainsKey(blurb.Identifier))
                cameraBlurbs.Remove(blurb.Identifier);
                
            cameraBlurbs.Add(blurb.Identifier, blurb);
        }
        
        public void RemoveCamera(string identifier)
        {
            if(cameraBlurbs.ContainsKey(identifier))
               cameraBlurbs.Remove(identifier);
        }
    	
    	public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("ImageDirectory", imageDirectory);
            if(!string.IsNullOrEmpty(imageFile))
                writer.WriteElementString("ImageFile", imageFile);
            writer.WriteElementString("ImageFormat", imageFormat.ToString());
            writer.WriteElementString("ImageCounter", imageCounter.ToString());
            
            writer.WriteElementString("VideoDirectory", videoDirectory);
            if(!string.IsNullOrEmpty(videoFile))
                writer.WriteElementString("VideoFile", videoFile);
            writer.WriteElementString("VideoFormat", videoFormat.ToString());
            writer.WriteElementString("VideoCounter", videoCounter.ToString());
        
            writer.WriteElementString("UsePattern", usePattern ? "true" : "false");
            writer.WriteElementString("Pattern", pattern);
        
            writer.WriteElementString("MemoryBuffer", memoryBuffer.ToString());
            
            if(cameraBlurbs.Count > 0)
            {
                writer.WriteStartElement("Cameras");
                
                foreach(CameraBlurb blurb in cameraBlurbs.Values)
                {
                    writer.WriteStartElement("Camera");
                    blurb.WriteXML(writer);
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement();
            }
        }
    	
    	public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
			{
				switch(reader.Name)
				{
					case "ImageDirectory":
				        imageDirectory = reader.ReadElementContentAsString();
                        break;
                    case "ImageFile":
                        imageFile = reader.ReadElementContentAsString();
                        break;
                    case "ImageFormat":
                        imageFormat = (KinoveaImageFormat) Enum.Parse(typeof(KinoveaImageFormat), reader.ReadElementContentAsString());
                        break;
                    case "ImageCounter":
                        imageCounter = reader.ReadElementContentAsLong();
                        break;
                    case "VideoDirectory":
                        videoDirectory = reader.ReadElementContentAsString();
                        break;
                    case "VideoFile":
                        videoFile = reader.ReadElementContentAsString();
                        break;
                    case "VideoFormat":
                        videoFormat = (KinoveaVideoFormat) Enum.Parse(typeof(KinoveaVideoFormat), reader.ReadElementContentAsString());
                        break;
                    case "VideoCounter":
                        videoCounter = reader.ReadElementContentAsLong();
                        break;
                    case "UsePattern":
                        usePattern = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "Pattern":
                        pattern = reader.ReadElementContentAsString();
                        break;
                    case "MemoryBuffer":
                        memoryBuffer = reader.ReadElementContentAsInt();
                        break;
                    /*case "DeviceConfigurations":
                        ParseDeviceConfigurations(reader);
                        break;
                    case "NetworkCameraUrl":
                        networkCameraUrl = reader.ReadElementContentAsString();
                        break;
                    case "NetworkCameraFormat":
                        networkCameraFormat = (NetworkCameraFormat) Enum.Parse(typeof(NetworkCameraFormat), reader.ReadElementContentAsString());
                        break;
                    case "RecentNetworkCameras":
                        ParseRecentNetworkCameras(reader);
                        break;
                    case "MaxRecentNetworkCameras":
                        maxRecentNetworkCameras = reader.ReadElementContentAsInt();
                        break;*/
                    case "Cameras":
                        ParseCameras(reader);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
            
            reader.ReadEndElement();
        }
        
        private void ParseCameras(XmlReader reader)
        {
            cameraBlurbs.Clear();
            bool empty = reader.IsEmptyElement;
            
            reader.ReadStartElement();
            
            if(empty)
                return;

            while(reader.NodeType == XmlNodeType.Element)
            {
                if(reader.Name == "Camera")
                {
                    CameraBlurb blurb = CameraBlurb.FromXML(reader);
                    if(blurb != null)
                        cameraBlurbs.Add(blurb.Identifier, blurb);
                }
                else
                {
                    reader.ReadOuterXml();
                }
            }

            reader.ReadEndElement();
        }
    }
}
