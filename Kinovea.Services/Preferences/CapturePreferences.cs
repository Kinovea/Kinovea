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
            get { BeforeRead(); return capturePathConfiguration; }
            set { capturePathConfiguration = value; Save(); }
        }

        public double DisplaySynchronizationFramerate
        {
            get { BeforeRead(); return displaySynchronizationFramerate; }
            set { displaySynchronizationFramerate = value; Save(); }
        }

        public CaptureRecordingMode RecordingMode
        {
            get { BeforeRead(); return recordingMode; }
            set { recordingMode = value; Save(); }
        }

        public int CaptureMemoryBuffer
        {
            get { BeforeRead(); return memoryBuffer; }
            set { memoryBuffer = value; Save(); }
        }
        public IEnumerable<CameraBlurb> CameraBlurbs
        {
            get { BeforeRead(); return cameraBlurbs.Values.Cast<CameraBlurb>(); }
        }
        public PhotofinishConfiguration PhotofinishConfiguration
        {
            get { BeforeRead(); return photofinishConfiguration; }
            set { photofinishConfiguration = value; Save(); }
        }
        public bool SaveUncompressedVideo
        {
            get { BeforeRead(); return saveUncompressedVideo; }
            set { saveUncompressedVideo = value; Save(); }
        }
        public CaptureAutomationConfiguration CaptureAutomationConfiguration
        {
            get { BeforeRead(); return captureAutomationConfiguration; }
            set { captureAutomationConfiguration = value; Save(); }
        }
        public float HighspeedRecordingFramerateThreshold
        {
            get { BeforeRead(); return highspeedRecordingFramerateThreshold; }
            set { highspeedRecordingFramerateThreshold = value; Save(); }
        }
        public float HighspeedRecordingFramerateOutput
        {
            get { BeforeRead(); return highspeedRecordingFramerateOutput; }
            set { highspeedRecordingFramerateOutput = value; Save(); }
        }
        public float SlowspeedRecordingFramerateThreshold
        {
            get { BeforeRead(); return slowspeedRecordingFramerateThreshold; }
            set { slowspeedRecordingFramerateThreshold = value; Save(); }
        }
        public float SlowspeedRecordingFramerateOutput
        {
            get { BeforeRead(); return slowspeedRecordingFramerateOutput; }
            set { slowspeedRecordingFramerateOutput = value; Save(); }
        }
        public KVAExportFlags ExportFlags
        {
            get { BeforeRead(); return exportFlags; }
            set { exportFlags = value; Save(); }
        }
        public string CaptureKVA
        {
            get { BeforeRead(); return captureKVA; }
            set { captureKVA = value; Save(); }
        }
        public bool ContextEnabled
        {
            get { BeforeRead(); return contextEnabled; }
            set { contextEnabled = value; Save(); }
        }
        public string ContextString
        {
            get { BeforeRead(); return contextString; }
            set { contextString = value; Save(); }
        }
        #endregion

        #region Members
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private double displaySynchronizationFramerate = 25.0;
        private CaptureRecordingMode recordingMode = CaptureRecordingMode.Delay;
        private bool saveUncompressedVideo;
        private bool verboseStats = false;
        private int memoryBuffer = 768;
        private Dictionary<string, CameraBlurb> cameraBlurbs = new Dictionary<string, CameraBlurb>();
        private PhotofinishConfiguration photofinishConfiguration = new PhotofinishConfiguration();
        private CaptureAutomationConfiguration captureAutomationConfiguration = new CaptureAutomationConfiguration();
        private float highspeedRecordingFramerateThreshold = 150;
        private float highspeedRecordingFramerateOutput = 30;
        private float slowspeedRecordingFramerateThreshold = 1;
        private float slowspeedRecordingFramerateOutput = 30;
        private KVAExportFlags exportFlags = KVAExportFlags.DefaultCaptureRecording;
        private string captureKVA;
        private bool contextEnabled = true;
        private string contextString;
        #endregion

        private void Save()
        {
            PreferencesManager.Save();
        }

        private void BeforeRead()
        {
            PreferencesManager.BeforeRead();
        }

        /// <summary>
        /// Add or update a camera blurb.
        /// Returns true if we modified the alias.
        /// </summary>
        public bool AddCamera(CameraBlurb blurb)
        {
            bool modifiedAlias = false;
            
            if (cameraBlurbs.ContainsKey(blurb.Identifier))
            {
                string oldAlias = cameraBlurbs[blurb.Identifier].Alias;
                if (oldAlias != blurb.Alias)
                    modifiedAlias = true;

                cameraBlurbs[blurb.Identifier] = blurb;
            }
            else
            {
                cameraBlurbs.Add(blurb.Identifier, blurb);
            }

            Save();

            return modifiedAlias;
        }
        
        public void RemoveCamera(string identifier)
        {
            if(cameraBlurbs.ContainsKey(identifier))
               cameraBlurbs.Remove(identifier);
            
            Save();
        }

        /// <summary>
        /// Add a capture folder if it doesn't exist yet.
        /// The path should be a file system path without the wildcard file name.
        /// This does not resolve variables for the matching against the existing ones.
        /// Returns the found capture folder or the newly inserted one.
        /// If two capture folders point to the same folder the first one is returned.
        /// Callers should then trigger preferences update to signal to other windows.
        /// </summary>
        public CaptureFolder AddCaptureFolder(string path)
        {
            BeforeRead();

            var ccff = CapturePathConfiguration.CaptureFolders;
            var foundCf = ccff.FirstOrDefault(cf => cf.Path == path);
            if (foundCf != null)
            {
                return foundCf;
            }

            CaptureFolder captureFolder = new CaptureFolder();
            captureFolder.Path = path;
            ccff.Insert(0, captureFolder);

            Save();
            return captureFolder;
        }

        #region Serialization
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

            writer.WriteElementString("CaptureKVA", captureKVA);
            writer.WriteElementString("ContextEnabled", XmlHelper.WriteBoolean(contextEnabled));
            writer.WriteElementString("ContextString", contextString);
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
                    case "CaptureKVA":
                        captureKVA = reader.ReadElementContentAsString();
                        break;
                    case "ContextEnabled":
                        contextEnabled = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "ContextString":
                        contextString = reader.ReadElementContentAsString();
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
        #endregion
    }
}
