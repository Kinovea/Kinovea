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
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Linq;
using System.Globalization;

namespace Kinovea.Services
{
    public class CapturePreferences : IPreferenceSerializer
    {
        #region Properties
        public string Name
        {
            get { return "Capture"; }
        }

        public CapturePathConfiguration CapturePathConfiguration
        {
            get { return capturePathConfiguration; }
            set { capturePathConfiguration = value; }
        }

        public double DisplaySynchronizationFramerate
        {
            get { return displaySynchronizationFramerate; }
            set { displaySynchronizationFramerate = value; }
        }

        public CaptureRecordingMode RecordingMode
        {
            get { return recordingMode; }
            set { recordingMode = value; }
        }
        
        public int CaptureMemoryBuffer
        {
            get { return memoryBuffer; }
            set { memoryBuffer = value; }
        }
        public IEnumerable<CameraBlurb> CameraBlurbs
        {
            get { return cameraBlurbs.Values.Cast<CameraBlurb>(); }
        }
        public DelayCompositeConfiguration DelayCompositeConfiguration
        {
            get { return delayCompositeConfiguration; }
            set { delayCompositeConfiguration = value; }
        }
        public bool VerboseStats
        {
            get { return verboseStats; }
            set { verboseStats = value; }
        }
        #endregion

        #region Members
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private double displaySynchronizationFramerate = 25.0;
        private CaptureRecordingMode recordingMode = CaptureRecordingMode.Camera;
        private bool verboseStats = false;
        private int memoryBuffer = 768;
        private Dictionary<string, CameraBlurb> cameraBlurbs = new Dictionary<string, CameraBlurb>();
        private DelayCompositeConfiguration delayCompositeConfiguration = new DelayCompositeConfiguration();
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
            writer.WriteStartElement("CapturePathConfiguration");
            capturePathConfiguration.WriteXml(writer);
            writer.WriteEndElement();

            string dsf = displaySynchronizationFramerate.ToString("0.000", CultureInfo.InvariantCulture);
            writer.WriteElementString("DisplaySynchronizationFramerate", dsf);
            writer.WriteElementString("CaptureRecordingMode", recordingMode.ToString());
            writer.WriteElementString("VerboseStats", verboseStats ? "true" : "false");

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

            writer.WriteStartElement("DelayCompositeConfiguration");
            delayCompositeConfiguration.WriteXml(writer);
            writer.WriteEndElement();
        }
        
        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "CapturePathConfiguration":
                        capturePathConfiguration.ReadXml(reader);
                        break;
                    case "DisplaySynchronizationFramerate":
                        string str = reader.ReadElementContentAsString();
                        displaySynchronizationFramerate = double.Parse(str, CultureInfo.InvariantCulture);
                        break;
                    case "CaptureRecordingMode":
                        recordingMode = (CaptureRecordingMode)Enum.Parse(typeof(CaptureRecordingMode), reader.ReadElementContentAsString());
                        break;
                    case "VerboseStats":
                        verboseStats = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "MemoryBuffer":
                        memoryBuffer = reader.ReadElementContentAsInt();
                        break;
                    case "Cameras":
                        ParseCameras(reader);
                        break;
                    case "DelayCompositeConfiguration":
                        delayCompositeConfiguration.ReadXml(reader);
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
