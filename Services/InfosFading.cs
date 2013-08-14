/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// This class encapsulate fading / persistence infos and utilities.
    /// It is used by all drawings to delegate the computing of the opacity factor.
    /// Each drawing instance has its own InfosFading with its own set of internal values. 
    /// </summary>
    public class InfosFading
    {
        #region Properties
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        public bool UseDefault
        {
            get { return useDefault; }
            set { useDefault = value; }
        }
        public bool AlwaysVisible
        {
            get { return alwaysVisible; }
            set { alwaysVisible = value; }
        }
        public int FadingFrames
        {
            get { return fadingFrames; }
            set { fadingFrames = value; }
        }
        public long ReferenceTimestamp
        {
            get { return referenceTimestamp; }
            set { referenceTimestamp = value; }
        }
        public long AverageTimeStampsPerFrame
        {
            get { return averageTimeStampsPerFrame; }
            set { averageTimeStampsPerFrame = value; }
        }
        public float MasterFactor
        {
            get { return masterFactor; }
            set { masterFactor = value; }
        }
        public int ContentHash
        {
            get 
            { 
                int hash = enabled.GetHashCode();
                hash ^= useDefault.GetHashCode();
                hash ^= alwaysVisible.GetHashCode();
                hash ^= fadingFrames.GetHashCode();
                hash ^= referenceTimestamp.GetHashCode();
                hash ^= masterFactor.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region Members
        private bool enabled;
        private bool useDefault;
        private bool alwaysVisible;
        private int fadingFrames;
        private long referenceTimestamp;
        private long averageTimeStampsPerFrame;
        private float masterFactor = 1.0f;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction
        public InfosFading()
        {
            // this constructor is directly used only by the Preference manager 
            // to create the default fading values.
            enabled = true;
            useDefault = true;
            alwaysVisible = false;
            fadingFrames = 20;
            referenceTimestamp = 0;
            averageTimeStampsPerFrame = 0;
            masterFactor = 1.0f;
        }

        public InfosFading(long referenceTimestamp, long averageTimeStampsPerFrame)
        {
            // This constructor is used by all drawings to get the default values.
            FromInfosFading(PreferencesManager.PlayerPreferences.DefaultFading);
            this.referenceTimestamp = referenceTimestamp;
            this.averageTimeStampsPerFrame = averageTimeStampsPerFrame;
        }
        #endregion

        #region Import / Export / Clone
        public InfosFading Clone()
        {
            InfosFading clone = new InfosFading(this.ReferenceTimestamp, this.AverageTimeStampsPerFrame);
            clone.FromInfosFading(this);
            return clone;
        }
        public void FromInfosFading(InfosFading _origin)
        {
            this.Enabled = _origin.Enabled;
            this.UseDefault = _origin.UseDefault;
            this.AlwaysVisible = _origin.AlwaysVisible;
            this.FadingFrames = _origin.FadingFrames;
            this.ReferenceTimestamp = _origin.ReferenceTimestamp;
            this.AverageTimeStampsPerFrame = _origin.AverageTimeStampsPerFrame;
            this.MasterFactor = _origin.MasterFactor;
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("Enabled", enabled ? "true" : "false");
            _xmlWriter.WriteElementString("Frames", fadingFrames.ToString());
            _xmlWriter.WriteElementString("AlwaysVisible", alwaysVisible ? "true" : "false");
            _xmlWriter.WriteElementString("UseDefault", useDefault ? "true" : "false");
        }
        public void ReadXml(XmlReader _xmlReader)
        {
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(_xmlReader.Name)
                {
                    case "Enabled":
                        enabled = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
                    case "Frames":
                        fadingFrames = _xmlReader.ReadElementContentAsInt();
                        break;
                    case "UseDefault":
                        useDefault = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
                    case "AlwaysVisible":
                        alwaysVisible = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = _xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            _xmlReader.ReadEndElement();
            
            // Sanity check.
            if (fadingFrames < 1) 
                fadingFrames = 1;
        }
        #endregion

        public double GetOpacityFactor(long _iTimestamp)
        {
            double fOpacityFactor = 0.0f;

            if (!enabled)
            {
                // No fading.
                if (_iTimestamp == referenceTimestamp)
                {
                    fOpacityFactor = 1.0f;
                }
                else
                {
                    fOpacityFactor = 0.0f;
                }
            }
            else if (useDefault)
            {
                // Default value
                InfosFading info = PreferencesManager.PlayerPreferences.DefaultFading;
                if(info.AlwaysVisible)
                {
                    fOpacityFactor = 1.0f;
                }
                else
                {
                    fOpacityFactor = ComputeOpacityFactor(referenceTimestamp, _iTimestamp, info.FadingFrames);
                }
            }
            else if (alwaysVisible)
            {
                // infinite fading. (= persisting drawing)
                fOpacityFactor = 1.0f;
            }
            else
            {
                // Custom value.
                fOpacityFactor = ComputeOpacityFactor(referenceTimestamp, _iTimestamp, fadingFrames);
            }

            return fOpacityFactor * masterFactor;
        }
        public bool IsVisible(long _iRefTimestamp, long _iTestTimestamp, int iVisibleFrames)
        {
            // Is a given point visible at all ?
            // Currently used by trajectory in focus mode to check for kf labels visibility.
            
            return ComputeOpacityFactor(_iRefTimestamp, _iTestTimestamp, (long)iVisibleFrames) > 0;
        }
        private double ComputeOpacityFactor(long _iRefTimestamp, long _iTestTimestamp, long iFadingFrames)
        {
            double fOpacityFactor = 0.0f;

            long iDistanceTimestamps = Math.Abs(_iTestTimestamp - _iRefTimestamp);
            long iFadingTimestamps = iFadingFrames * averageTimeStampsPerFrame;

            if (iDistanceTimestamps > iFadingTimestamps)
            {
                fOpacityFactor = 0.0f;
            }
            else
            {
                fOpacityFactor = 1.0f - ((double)iDistanceTimestamps / (double)iFadingTimestamps);
            }

            return fOpacityFactor;
        }
    }
}
