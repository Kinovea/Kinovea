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
        public PhotofinishConfiguration PhotofinishConfiguration
        {
            get { return photofinishConfiguration; }
            set { photofinishConfiguration = value; }
        }
        public bool VerboseStats
        {
            get { return verboseStats; }
            set { verboseStats = value; }
        }
        public bool SaveUncompressedVideo
        {
            get { return saveUncompressedVideo; }
            set { saveUncompressedVideo = value; }
        }
        public CaptureAutomationConfiguration CaptureAutomationConfiguration
        {
            get { return captureAutomationConfiguration; }
            set { captureAutomationConfiguration = value; }
        }
        public float HighspeedRecordingFramerateThreshold
        {
            get { return highspeedRecordingFramerateThreshold; }
            set { highspeedRecordingFramerateThreshold = value; }
        }
        public float HighspeedRecordingFramerateOutput
        {
            get { return highspeedRecordingFramerateOutput; }
            set { highspeedRecordingFramerateOutput = value; }
        }
        public float SlowspeedRecordingFramerateThreshold
        {
            get { return slowspeedRecordingFramerateThreshold; }
            set { slowspeedRecordingFramerateThreshold = value; }
        }
        public float SlowspeedRecordingFramerateOutput
        {
            get { return slowspeedRecordingFramerateOutput; }
            set { slowspeedRecordingFramerateOutput = value; }
        }
        public KVAExportFlags ExportFlags
        {
            get { return exportFlags; }
            set { exportFlags = value; }
        }
        public string PostRecordCommand
        {
            get { return postRecordCommand; }
            set { postRecordCommand = value; }
        }
        public string CaptureKVA
        {
            get { return captureKVA; }
            set { captureKVA = value; }
        }
        #endregion

        #region Members
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private double displaySynchronizationFramerate = 25.0;
        private CaptureRecordingMode recordingMode = CaptureRecordingMode.Camera;
        private bool saveUncompressedVideo;
        private bool verboseStats = false;
        private int memoryBuffer = 768;
        private Dictionary<string, CameraBlurb> cameraBlurbs = new Dictionary<string, CameraBlurb>();
        private DelayCompositeConfiguration delayCompositeConfiguration = new DelayCompositeConfiguration();
        private PhotofinishConfiguration photofinishConfiguration = new PhotofinishConfiguration();
        private CaptureAutomationConfiguration captureAutomationConfiguration = new CaptureAutomationConfiguration();
        private float highspeedRecordingFramerateThreshold = 150;
        private float highspeedRecordingFramerateOutput = 30;
        private float slowspeedRecordingFramerateThreshold = 1;
        private float slowspeedRecordingFramerateOutput = 30;
        private KVAExportFlags exportFlags = KVAExportFlags.DefaultCaptureRecording;
        private string postRecordCommand;
        private string captureKVA;
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
            writer.WriteElementString("SaveUncompressedVideo", saveUncompressedVideo ? "true" : "false");
            
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

            writer.WriteStartElement("PhotofinishConfiguration");
            photofinishConfiguration.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("CaptureAutomationConfiguration");
            captureAutomationConfiguration.WriteXml(writer);
            writer.WriteEndElement();

            string hrft = highspeedRecordingFramerateThreshold.ToString("0.000", CultureInfo.InvariantCulture);
            string hrfo = highspeedRecordingFramerateOutput.ToString("0.000", CultureInfo.InvariantCulture);
            writer.WriteElementString("HighspeedRecordingFramerateThreshold", hrft);
            writer.WriteElementString("HighspeedRecordingFramerateOutput", hrfo);
            string srft = slowspeedRecordingFramerateThreshold.ToString("0.000", CultureInfo.InvariantCulture);
            string srfo = slowspeedRecordingFramerateOutput.ToString("0.000", CultureInfo.InvariantCulture);
            writer.WriteElementString("SlowspeedRecordingFramerateThreshold", srft);
            writer.WriteElementString("SlowspeedRecordingFramerateOutput", srfo);
            writer.WriteElementString("ExportFlags", exportFlags.ToString());

            writer.WriteElementString("PostRecordCommand", postRecordCommand);
            writer.WriteElementString("CaptureKVA", captureKVA);
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
                        string recordingModeString = reader.ReadElementContentAsString();
                        recordingMode = (CaptureRecordingMode)Enum.Parse(typeof(CaptureRecordingMode), recordingModeString);
                        break;
                    case "SaveUncompressedVideo":
                        saveUncompressedVideo = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
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
                    case "PhotofinishConfiguration":
                        photofinishConfiguration.ReadXml(reader);
                        break;
                    case "CaptureAutomationConfiguration":
                        captureAutomationConfiguration.ReadXml(reader);
                        break;
                    case "HighspeedRecordingFramerateThreshold":
                        string hrft = reader.ReadElementContentAsString();
                        highspeedRecordingFramerateThreshold = float.Parse(hrft, CultureInfo.InvariantCulture);
                        break;
                    case "HighspeedRecordingFramerateOutput":
                        string hrfo = reader.ReadElementContentAsString();
                        highspeedRecordingFramerateOutput = float.Parse(hrfo, CultureInfo.InvariantCulture);
                        break;
                    case "SlowspeedRecordingFramerateThreshold":
                        string srft = reader.ReadElementContentAsString();
                        slowspeedRecordingFramerateThreshold = float.Parse(srft, CultureInfo.InvariantCulture);
                        break;
                    case "SlowspeedRecordingFramerateOutput":
                        string srfo = reader.ReadElementContentAsString();
                        slowspeedRecordingFramerateOutput = float.Parse(srfo, CultureInfo.InvariantCulture);
                        break;
                    case "ExportFlags":
                        string exportFlagsString = reader.ReadElementContentAsString();
                        exportFlags = (KVAExportFlags)Enum.Parse(typeof(KVAExportFlags), exportFlagsString);
                        break;
                    case "PostRecordCommand":
                        postRecordCommand = reader.ReadElementContentAsString();
                        break;
                    case "CaptureKVA":
                        captureKVA = reader.ReadElementContentAsString();
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
