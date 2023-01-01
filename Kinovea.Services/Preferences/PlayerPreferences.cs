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
        public AccelerationUnit AccelerationUnit
        {
            get { return accelerationUnit; }
            set { accelerationUnit = value; }
        }
        public AngleUnit AngleUnit
        {
            get { return angleUnit; }
            set { angleUnit = value; }
        }
        public AngularVelocityUnit AngularVelocityUnit
        {
            get { return angularVelocityUnit; }
            set { angularVelocityUnit = value; }
        }
        public AngularAccelerationUnit AngularAccelerationUnit
        {
            get { return angularAccelerationUnit; }
            set { angularAccelerationUnit = value; }
        }
        public string CustomLengthUnit
        {
            get { return customLengthUnit; }
            set { customLengthUnit = value; }
        }
        public string CustomLengthAbbreviation
        {
            get { return customLengthAbbreviation; }
            set { customLengthAbbreviation = value; }
        }
        public ImageAspectRatio AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }
        public CSVDecimalSeparator CSVDecimalSeparator
        {
            get { return csvDecimalSeparator; }
            set { csvDecimalSeparator = value; }
        }
        public ExportSpace ExportSpace
        {
            get { return exportSpace; }
            set { exportSpace = value; }
        }
        public bool DeinterlaceByDefault
        {
            get { return deinterlaceByDefault; }
            set { deinterlaceByDefault = value; }
        }
        public bool InteractiveFrameTracker
        {
            get { return interactiveFrameTracker; }
            set { interactiveFrameTracker = value; }
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

        public bool SyncByMotion
        {
            get { return syncByMotion; }
            set { syncByMotion = value; }
        }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }
        public InfosFading DefaultFading
        {
            get { return defaultFading; }
            set { defaultFading = value; }
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
        public TrackingProfile TrackingProfile
        {
            get { return trackingProfile; }
            set { trackingProfile = value; }
        }
        public bool EnableFiltering
        {
            get { return enableFiltering; }
            set { enableFiltering = value; }
        }
        public bool EnableHighSpeedDerivativesSmoothing
        {
            get { return enableHighSpeedDerivativesSmoothing; }
            set { enableHighSpeedDerivativesSmoothing = value; }
        }
        public bool EnableCustomToolsDebugMode
        {
            get { return enableCustomToolsDebugMode; }
            set { enableCustomToolsDebugMode = value; }
        }
        public float DefaultReplaySpeed
        {
            get { return defaultReplaySpeed; }
            set { defaultReplaySpeed = value; }
        }
        public bool DetectImageSequences
        {
            get { return detectImageSequences; }
            set { detectImageSequences = value; }
        }
        public int PreloadKeyframes
        {
            get { return preloadKeyframes; }
            set { preloadKeyframes = value; }
        }
        public string PlaybackKVA
        {
            get { return playbackKVA; }
            set { playbackKVA = value; }
        }
        public KinogramParameters Kinogram
        {
            get { return kinogramParameters.Clone(); }
            set { kinogramParameters = value; }
        }

        public KeyframePresetsParameters KeyframePresets
        {
            get { return keyframePresetsParameters.Clone(); }
            set { keyframePresetsParameters = value; }
        }
        #endregion

        private TimecodeFormat timecodeFormat = TimecodeFormat.ClassicTime;
        private SpeedUnit speedUnit = SpeedUnit.MetersPerSecond;
        private AccelerationUnit accelerationUnit = AccelerationUnit.MetersPerSecondSquared;
        private AngleUnit angleUnit = AngleUnit.Degree;
        private AngularVelocityUnit angularVelocityUnit = AngularVelocityUnit.DegreesPerSecond;
        private AngularAccelerationUnit angularAccelerationUnit = AngularAccelerationUnit.DegreesPerSecondSquared;
        private string customLengthUnit = "";
        private string customLengthAbbreviation = "";
        private CSVDecimalSeparator csvDecimalSeparator = CSVDecimalSeparator.System;
        private ExportSpace exportSpace = ExportSpace.WorldSpace;
        private ImageAspectRatio aspectRatio = ImageAspectRatio.Auto;
        private bool deinterlaceByDefault;
        private bool interactiveFrameTracker = true;
        private int workingZoneMemory = 768;
        private InfosFading defaultFading = new InfosFading();
        private Color backgroundColor = Color.FromArgb(0, 255, 255, 255);
        private Color defaultBackgroundColor = Color.FromArgb(0, 255, 255, 255);
        private bool drawOnPlay = true;
        private List<Color> recentColors = new List<Color>();
        private int maxRecentColors = 12;
        private bool syncLockSpeed = true;
        private bool syncByMotion = false;
        private KinoveaImageFormat imageFormat = KinoveaImageFormat.JPG;
        private KinoveaVideoFormat videoFormat = KinoveaVideoFormat.MKV;
        private TrackingProfile trackingProfile = new TrackingProfile();
        private bool enableFiltering = true;
        private bool enableHighSpeedDerivativesSmoothing = true;
        private bool enableCustomToolsDebugMode = false;
        private float defaultReplaySpeed = 100;
        private bool detectImageSequences = true;
        private int preloadKeyframes = 20;
        private string playbackKVA;
        private KinogramParameters kinogramParameters = new KinogramParameters();
        private KeyframePresetsParameters keyframePresetsParameters = new KeyframePresetsParameters();
        public void AddRecentColor(Color _color)
        {
            PreferencesManager.UpdateRecents(_color, recentColors, maxRecentColors);
        }
        
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("TimecodeFormat", timecodeFormat.ToString());
            writer.WriteElementString("SpeedUnit", speedUnit.ToString());
            writer.WriteElementString("AccelerationUnit", accelerationUnit.ToString());
            writer.WriteElementString("AngleUnit", angleUnit.ToString());
            writer.WriteElementString("AngularVelocityUnit", angularVelocityUnit.ToString());
            writer.WriteElementString("AngularAccelerationUnit", angularAccelerationUnit.ToString());
            writer.WriteElementString("CustomLengthUnit", customLengthUnit);
            writer.WriteElementString("CustomLengthAbbreviation", customLengthAbbreviation);
            writer.WriteElementString("CSVDecimalSeparator", csvDecimalSeparator.ToString());
            writer.WriteElementString("ExportSpace", exportSpace.ToString());
            writer.WriteElementString("AspectRatio", aspectRatio.ToString());
            writer.WriteElementString("DeinterlaceByDefault", deinterlaceByDefault ? "true" : "false");
            writer.WriteElementString("InteractiveFrameTracker", interactiveFrameTracker ? "true" : "false");
            writer.WriteElementString("WorkingZoneMemory", workingZoneMemory.ToString());
            writer.WriteElementString("SyncLockSpeed", syncLockSpeed ? "true" : "false");
            writer.WriteElementString("SyncByMotion", syncByMotion ? "true" : "false");
            writer.WriteElementString("ImageFormat", imageFormat.ToString());
            writer.WriteElementString("VideoFormat", videoFormat.ToString());
            writer.WriteElementString("Background", XmlHelper.WriteColor(backgroundColor, true));
            
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

            writer.WriteStartElement("TrackingProfile");
            trackingProfile.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteElementString("EnableFiltering", enableFiltering ? "true" : "false");
            writer.WriteElementString("EnableCustomToolsDebugMode", enableCustomToolsDebugMode ? "true" : "false");
            writer.WriteElementString("DefaultReplaySpeed", defaultReplaySpeed.ToString("0", CultureInfo.InvariantCulture));
            writer.WriteElementString("DetectImageSequences", detectImageSequences ? "true" : "false");
            writer.WriteElementString("PreloadKeyframes", preloadKeyframes.ToString());
            writer.WriteElementString("PlaybackKVA", playbackKVA);

            writer.WriteStartElement("Kinogram");
            kinogramParameters.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("KeyframePresets");
            keyframePresetsParameters.WriteXml(writer);
            writer.WriteEndElement();
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
                    case "CSVDecimalSeparator":
                        csvDecimalSeparator = (CSVDecimalSeparator)Enum.Parse(typeof(CSVDecimalSeparator), reader.ReadElementContentAsString());
                        break;
                    case "ExportSpace":
                        exportSpace = (ExportSpace)Enum.Parse(typeof(ExportSpace), reader.ReadElementContentAsString());
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
                    case "Background":
                        backgroundColor = XmlHelper.ParseColor(reader.ReadElementContentAsString(), defaultBackgroundColor);
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
                    case "TrackingProfile":
                        trackingProfile.ReadXml(reader);
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
                    case "KeyframePresets":
                        keyframePresetsParameters.ReadXml(reader);
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
