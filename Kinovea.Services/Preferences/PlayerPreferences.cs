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
using System.Globalization;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Preferences for the player, including annotations.
    /// </summary>
    public class PlayerPreferences : IPreferenceSerializer
    {
        #region Properties
        public string Name
        {
            get { return "Player"; }
        }
        public int DecimalPlaces
        {
            get { BeforeRead(); return decimalPlaces; }
            set { decimalPlaces = value; Save(); }
        }
        public TimecodeFormat TimecodeFormat
        {
            get { BeforeRead(); return timecodeFormat; }
            set { timecodeFormat = value; Save(); }
        }
        public SpeedUnit SpeedUnit
        {
            get { BeforeRead(); return speedUnit; }
            set { speedUnit = value; Save(); }
        }
        public AccelerationUnit AccelerationUnit
        {
            get { BeforeRead(); return accelerationUnit; }
            set { accelerationUnit = value; Save(); }
        }
        public AngleUnit AngleUnit
        {
            get { BeforeRead(); return angleUnit; }
            set { angleUnit = value; Save(); }
        }
        public AngularVelocityUnit AngularVelocityUnit
        {
            get { BeforeRead(); return angularVelocityUnit; }
            set { angularVelocityUnit = value; Save(); }
        }
        public AngularAccelerationUnit AngularAccelerationUnit
        {
            get { BeforeRead(); return angularAccelerationUnit; }
            set { angularAccelerationUnit = value; Save(); }
        }
        public string CustomLengthUnit
        {
            get { BeforeRead(); return customLengthUnit; }
            set { customLengthUnit = value; Save(); }
        }
        public string CustomLengthAbbreviation
        {
            get { BeforeRead(); return customLengthAbbreviation; }
            set { customLengthAbbreviation = value; Save(); }
        }

        public CadenceUnit CadenceUnit
        {
            get { BeforeRead(); return cadenceUnit; }
            set { cadenceUnit = value; Save(); }
        }
        public ImageAspectRatio AspectRatio
        {
            get { BeforeRead(); return aspectRatio; }
            set { aspectRatio = value; Save(); }
        }
        public CSVDecimalSeparator CSVDecimalSeparator
        {
            get { BeforeRead(); return csvDecimalSeparator; }
            set { csvDecimalSeparator = value; Save(); }
        }
        public ExportSpace ExportSpace
        {
            get { BeforeRead(); return exportSpace; }
            set { exportSpace = value; Save(); }
        }
        public bool ExportImagesInDocuments
        {
            get { BeforeRead(); return exportImagesInDocuments; }
            set { exportImagesInDocuments = value; Save(); }
        }
        public bool DeinterlaceByDefault
        {
            get { BeforeRead(); return deinterlaceByDefault; }
            set { deinterlaceByDefault = value; Save(); }
        }
        public bool InteractiveFrameTracker
        {
            get { BeforeRead(); return interactiveFrameTracker; }
            set { interactiveFrameTracker = value; Save(); }
        }
        public int WorkingZoneMemory
        {
            get { BeforeRead(); return workingZoneMemory; }
            set { workingZoneMemory = value; Save(); }
        }
        public bool ShowCacheInTimeline
        {
            get { BeforeRead(); return showCacheInTimeline; }
            set { showCacheInTimeline = value; Save(); }
        }
        public bool SyncLockSpeed
        {
            get { BeforeRead(); return syncLockSpeed;}
            set { syncLockSpeed = value; Save(); }
        }

        public bool SyncByMotion
        {
            get { BeforeRead(); return syncByMotion; }
            set { syncByMotion = value; Save(); }
        }
        
        public InfosFading DefaultFading
        {
            get { BeforeRead(); return defaultFading; }
            set { defaultFading = value; Save(); }
        }
        public bool DrawOnPlay
        {
            get { BeforeRead(); return drawOnPlay; }
            set { drawOnPlay = value; Save(); }
        }
        public List<Color> RecentColors
        {
            get { BeforeRead(); return recentColors; }
        }
        public KinoveaImageFormat ImageFormat
        {
            get { BeforeRead(); return imageFormat; }
            set { imageFormat = value; Save(); }
        }
        public KinoveaVideoFormat VideoFormat
        {
            get { BeforeRead(); return videoFormat; }
            set { videoFormat = value; Save(); }
        }
        public TrackingParameters TrackingParameters
        {
            get { BeforeRead(); return trackingParameters; }
            set { trackingParameters = value; Save(); }
        }
        public bool EnableFiltering
        {
            get { BeforeRead(); return enableFiltering; }
            set { enableFiltering = value; Save(); }
        }
        public bool EnableHighSpeedDerivativesSmoothing
        {
            get { BeforeRead(); return enableHighSpeedDerivativesSmoothing; }
            set { enableHighSpeedDerivativesSmoothing = value; Save(); }
        }
        public bool EnableCustomToolsDebugMode
        {
            get { BeforeRead(); return enableCustomToolsDebugMode; }
            set { enableCustomToolsDebugMode = value; Save(); }
        }
        public float DefaultReplaySpeed
        {
            get { BeforeRead(); return defaultReplaySpeed; }
            set { defaultReplaySpeed = value; Save(); }
        }
        public bool DetectImageSequences
        {
            get { BeforeRead(); return detectImageSequences; }
            set { detectImageSequences = value; Save(); }
        }
        public int PreloadKeyframes
        {
            get { BeforeRead(); return preloadKeyframes; }
            set { preloadKeyframes = value; Save(); }
        }
        public string PlaybackKVA
        {
            get { BeforeRead(); return playbackKVA; }
            set { playbackKVA = value; Save(); }
        }
        public KinogramParameters Kinogram
        {
            get { BeforeRead(); return kinogramParameters.Clone(); }
            set { kinogramParameters = value; Save(); }
        }

        public LensCalibrationParameters LensCalibration
        {
            get { BeforeRead(); return lensCalibrationParameters.Clone(); }
            set { lensCalibrationParameters = value; Save(); }
        }

        public CameraMotionParameters CameraMotionParameters
        {
            get { BeforeRead(); return cameraMotionParameters.Clone(); }
            set { cameraMotionParameters = value; Save(); }
        }

        public KeyframePresetsParameters KeyframePresets
        {
            get { BeforeRead(); return keyframePresetsParameters.Clone(); }
            set { keyframePresetsParameters = value; Save(); }
        }
        public string PandocPath
        {
            get { BeforeRead(); return pandocPath; }
            set { pandocPath = value; Save(); }
        }

        public bool SideBySideHorizontal
        {
            get { BeforeRead(); return sideBySideHorizontal; }
            set { sideBySideHorizontal = value; Save(); }
        }

        #endregion

        #region Members
        private int decimalPlaces = 2;
        private TimecodeFormat timecodeFormat = TimecodeFormat.ClassicTime;
        private SpeedUnit speedUnit = SpeedUnit.MetersPerSecond;
        private AccelerationUnit accelerationUnit = AccelerationUnit.MetersPerSecondSquared;
        private AngleUnit angleUnit = AngleUnit.Degree;
        private AngularVelocityUnit angularVelocityUnit = AngularVelocityUnit.DegreesPerSecond;
        private AngularAccelerationUnit angularAccelerationUnit = AngularAccelerationUnit.DegreesPerSecondSquared;
        private string customLengthUnit = "";
        private string customLengthAbbreviation = "";
        private CadenceUnit cadenceUnit = CadenceUnit.Hertz;
        private CSVDecimalSeparator csvDecimalSeparator = CSVDecimalSeparator.System;
        private ExportSpace exportSpace = ExportSpace.WorldSpace;
        private bool exportImagesInDocuments = true;
        private string pandocPath = "";
        private ImageAspectRatio aspectRatio = ImageAspectRatio.Auto;
        private bool deinterlaceByDefault;
        private bool interactiveFrameTracker = true;
        private int workingZoneMemory = 768;
        private InfosFading defaultFading = new InfosFading();
        private bool drawOnPlay = true;
        private List<Color> recentColors = new List<Color>();
        private int maxRecentColors = 12;
        private bool syncLockSpeed = true;
        private bool syncByMotion = false;
        private KinoveaImageFormat imageFormat = KinoveaImageFormat.JPG;
        private KinoveaVideoFormat videoFormat = KinoveaVideoFormat.MKV;
        private TrackingParameters trackingParameters = new TrackingParameters();
        private bool enableFiltering = true;
        private bool enableHighSpeedDerivativesSmoothing = true;
        private bool enableCustomToolsDebugMode = false;
        private float defaultReplaySpeed = 100;
        private bool detectImageSequences = true;
        private int preloadKeyframes = 20;
        private string playbackKVA;
        private KinogramParameters kinogramParameters = new KinogramParameters();
        private LensCalibrationParameters lensCalibrationParameters = new LensCalibrationParameters();
        private CameraMotionParameters cameraMotionParameters = new CameraMotionParameters();
        private KeyframePresetsParameters keyframePresetsParameters = new KeyframePresetsParameters();
        private bool showCacheInTimeline = false;
        private bool sideBySideHorizontal = true;
        #endregion

        private void Save()
        {
            PreferencesManager.Save();
        }

        private void BeforeRead()
        {
            PreferencesManager.BeforeRead();
        }


        public void AddRecentColor(Color _color)
        {
            PreferencesHelper.UpdateRecents(_color, recentColors, maxRecentColors);
            Save();
        }

        #region Serialization

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("DecimalPlaces", decimalPlaces.ToString());
            writer.WriteElementString("TimecodeFormat", timecodeFormat.ToString());
            writer.WriteElementString("SpeedUnit", speedUnit.ToString());
            writer.WriteElementString("AccelerationUnit", accelerationUnit.ToString());
            writer.WriteElementString("AngleUnit", angleUnit.ToString());
            writer.WriteElementString("AngularVelocityUnit", angularVelocityUnit.ToString());
            writer.WriteElementString("AngularAccelerationUnit", angularAccelerationUnit.ToString());
            writer.WriteElementString("CustomLengthUnit", customLengthUnit);
            writer.WriteElementString("CustomLengthAbbreviation", customLengthAbbreviation);
            writer.WriteElementString("CadenceUnit", cadenceUnit.ToString());
            writer.WriteElementString("CSVDecimalSeparator", csvDecimalSeparator.ToString());
            writer.WriteElementString("ExportSpace", exportSpace.ToString());
            writer.WriteElementString("ExportImagesInDocuments", exportSpace.ToString());
            writer.WriteElementString("AspectRatio", aspectRatio.ToString());
            writer.WriteElementString("DeinterlaceByDefault", XmlHelper.WriteBoolean(deinterlaceByDefault));
            writer.WriteElementString("InteractiveFrameTracker", XmlHelper.WriteBoolean(interactiveFrameTracker));
            writer.WriteElementString("WorkingZoneMemory", workingZoneMemory.ToString());
            writer.WriteElementString("ShowCacheInTimeline", XmlHelper.WriteBoolean(showCacheInTimeline));
            writer.WriteElementString("SyncLockSpeed", XmlHelper.WriteBoolean(syncLockSpeed));
            writer.WriteElementString("SyncByMotion", XmlHelper.WriteBoolean(syncByMotion));
            writer.WriteElementString("ImageFormat", imageFormat.ToString());
            writer.WriteElementString("VideoFormat", videoFormat.ToString());
            
            writer.WriteStartElement("InfoFading");
            defaultFading.WriteXml(writer);
            writer.WriteEndElement();
            
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

            writer.WriteStartElement("TrackingParameters");
            trackingParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString("EnableFiltering", enableFiltering ? "true" : "false");
            writer.WriteElementString("EnableCustomToolsDebugMode", enableCustomToolsDebugMode ? "true" : "false");
            writer.WriteElementString("DefaultReplaySpeed", defaultReplaySpeed.ToString("0", CultureInfo.InvariantCulture));
            writer.WriteElementString("DetectImageSequences", XmlHelper.WriteBoolean(detectImageSequences));
            writer.WriteElementString("PreloadKeyframes", preloadKeyframes.ToString());
            writer.WriteElementString("PlaybackKVA", playbackKVA);

            writer.WriteStartElement("Kinogram");
            kinogramParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("LensCalibration");
            lensCalibrationParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("CameraMotion");
            cameraMotionParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("KeyframePresets");
            keyframePresetsParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString("PandocPath", pandocPath);
            writer.WriteElementString("SideBySideHorizontal", XmlHelper.WriteBoolean(sideBySideHorizontal));
        }
        
        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "DecimalPlaces":
                        decimalPlaces = reader.ReadElementContentAsInt();
                        break;
                    case "TimecodeFormat":
                        timecodeFormat = (TimecodeFormat) Enum.Parse(typeof(TimecodeFormat), reader.ReadElementContentAsString());
                        break;
                    case "SpeedUnit":
                        speedUnit = (SpeedUnit) Enum.Parse(typeof(SpeedUnit), reader.ReadElementContentAsString());
                        break;
                    case "AccelerationUnit":
                        accelerationUnit = (AccelerationUnit)Enum.Parse(typeof(AccelerationUnit), reader.ReadElementContentAsString());
                        break;
                    case "AngleUnit":
                        angleUnit = (AngleUnit)Enum.Parse(typeof(AngleUnit), reader.ReadElementContentAsString());
                        break;
                    case "AngularVelocityUnit":
                        angularVelocityUnit = (AngularVelocityUnit)Enum.Parse(typeof(AngularVelocityUnit), reader.ReadElementContentAsString());
                        break;
                    case "AngularAccelerationUnit":
                        angularAccelerationUnit = (AngularAccelerationUnit)Enum.Parse(typeof(AngularAccelerationUnit), reader.ReadElementContentAsString());
                        break;
                    case "CustomLengthUnit":
                        customLengthUnit = reader.ReadElementContentAsString();
                        break;
                    case "CustomLengthAbbreviation":
                        customLengthAbbreviation = reader.ReadElementContentAsString();
                        break;
                    case "CadenceUnit":
                        cadenceUnit = XmlHelper.ParseEnum(reader.ReadElementContentAsString(), CadenceUnit.Hertz);
                        break;
                    case "CSVDecimalSeparator":
                        csvDecimalSeparator = (CSVDecimalSeparator)Enum.Parse(typeof(CSVDecimalSeparator), reader.ReadElementContentAsString());
                        break;
                    case "ExportSpace":
                        exportSpace = (ExportSpace)Enum.Parse(typeof(ExportSpace), reader.ReadElementContentAsString());
                        break;
                    case "ExportImagesInDocuments":
                        exportImagesInDocuments = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "AspectRatio":
                        aspectRatio = (ImageAspectRatio) Enum.Parse(typeof(ImageAspectRatio), reader.ReadElementContentAsString());
                        break;
                    case "DeinterlaceByDefault":
                        deinterlaceByDefault = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "InteractiveFrameTracker":
                        interactiveFrameTracker = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "WorkingZoneMemory":
                        workingZoneMemory = reader.ReadElementContentAsInt();
                        break;
                    case "ShowCacheInTimeline":
                        showCacheInTimeline = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "SyncLockSpeed":
                        syncLockSpeed = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "SyncByMotion":
                        syncByMotion = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "ImageFormat":
                        imageFormat = (KinoveaImageFormat)Enum.Parse(typeof(KinoveaImageFormat), reader.ReadElementContentAsString());
                        break;
                    case "VideoFormat":
                        videoFormat = (KinoveaVideoFormat)Enum.Parse(typeof(KinoveaVideoFormat), reader.ReadElementContentAsString());
                        break;
                    case "InfoFading":
                        defaultFading.ReadXml(reader);
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
                    case "TrackingParameters":
                        trackingParameters.ReadXml(reader);
                        break;
                    case "EnableFiltering":
                        enableFiltering = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "EnableCustomToolsDebugMode":
                        enableCustomToolsDebugMode = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "DefaultReplaySpeed":
                        string str = reader.ReadElementContentAsString();
                        defaultReplaySpeed = float.Parse(str, CultureInfo.InvariantCulture);
                        break;
                    case "DetectImageSequences":
                        detectImageSequences = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "PreloadKeyframes":
                        preloadKeyframes = reader.ReadElementContentAsInt();
                        break;
                    case "PlaybackKVA":
                        playbackKVA = reader.ReadElementContentAsString();
                        break;
                    case "Kinogram":
                        kinogramParameters.ReadXml(reader);
                        break;
                    case "LensCalibration":
                        lensCalibrationParameters.ReadXml(reader);
                        break;
                    case "CameraMotion":
                        cameraMotionParameters.ReadXml(reader);
                        break;
                    case "KeyframePresets":
                        keyframePresetsParameters.ReadXml(reader);
                        break;
                    case "PandocPath":
                        pandocPath = reader.ReadElementContentAsString();
                        break;
                    case "SideBySideHorizontal":
                        sideBySideHorizontal = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
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
        #endregion
    }
}
