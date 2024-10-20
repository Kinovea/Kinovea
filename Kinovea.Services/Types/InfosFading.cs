/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Globalization;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// This class encapsulate onion-skinning / opacity ramping utilities.
    /// It is used by all drawings to delegate the computing of their opacity at any given frame.
    /// 
    /// We support more options than traditional onion-skinning:
    /// 1. The configuration is per-drawing (each has its own InfosFading object), even if most of the time the global value from preferences is used.
    /// 2. Option to stay fully opaque throughout the entire video.
    /// 3. Option to stay fully opaque for a number of frames.
    /// When considering opacity from a given frame, we treat the start and end of the opaque section as if the drawing was at these frames.
    /// 
    /// TODO: Currently there is no support for changing color based on whether the drawing is in the future or the past.
    /// </summary>
    public class InfosFading
    {
        #region Properties

        /// <summary>
        /// Whether the ramping of opacity before and after the opaque duration is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Whether this drawing fading/duration mode has been customized by the user.
        /// </summary>
        public bool UseDefault
        {
            get { return useDefault; }
            set { useDefault = value; }
        }

        /// <summary>
        /// Whether this drawing should be opaque during the entire video.
        /// </summary>
        public bool AlwaysVisible
        {
            get { return alwaysVisible; }
            set { alwaysVisible = value; }
        }

        /// <summary>
        /// How many frames are used to ramp up and down opacity around the opaque section.
        /// </summary>
        public int FadingFrames
        {
            get { return fadingFrames; }
            set { fadingFrames = value; }
        }

        /// <summary>
        /// How many frames are fully opaque.
        /// </summary>
        public int OpaqueFrames
        {
            get { return opaqueFrames; }
            set { opaqueFrames = value; }
        }

        /// <summary>
        /// The timestamp the drawing is attached to, and first fully opaque frame.
        /// </summary>
        public long ReferenceTimestamp
        {
            get { return referenceTimestamp; }
            set { referenceTimestamp = value; }
        }

        /// <summary>
        /// The average timestamp per frame for the video.
        /// </summary>
        public long AverageTimeStampsPerFrame
        {
            get { return averageTimeStampsPerFrame; }
            set { averageTimeStampsPerFrame = value; }
        }

        /// <summary>
        /// Defines the highest possible opacity.
        /// </summary>
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
                hash ^= opaqueFrames.GetHashCode();
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
        private int opaqueFrames = 1;
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
            opaqueFrames = 1;
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
        public void FromInfosFading(InfosFading origin)
        {
            this.Enabled = origin.Enabled;
            this.UseDefault = origin.UseDefault;
            this.AlwaysVisible = origin.AlwaysVisible;
            this.FadingFrames = origin.FadingFrames;
            this.OpaqueFrames = origin.OpaqueFrames;
            this.ReferenceTimestamp = origin.ReferenceTimestamp;
            this.AverageTimeStampsPerFrame = origin.AverageTimeStampsPerFrame;
            this.MasterFactor = origin.MasterFactor;
        }
        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Enabled", enabled.ToString().ToLower());
            w.WriteElementString("UseDefault", useDefault.ToString().ToLower());
            w.WriteElementString("AlwaysVisible", alwaysVisible.ToString().ToLower());
            w.WriteElementString("MasterFactor", masterFactor.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("OpaqueFrames", opaqueFrames.ToString());
            w.WriteElementString("Frames", fadingFrames.ToString());
        }
        public void ReadXml(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Enabled":
                        enabled = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "UseDefault":
                        useDefault = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "AlwaysVisible":
                        alwaysVisible = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "MasterFactor":
                        masterFactor = float.Parse(xmlReader.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "OpaqueFrames":
                        opaqueFrames = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Frames":
                        fadingFrames = xmlReader.ReadElementContentAsInt();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();

            fadingFrames = Math.Max(fadingFrames, 1);
        }
        #endregion

        /// <summary>
        /// Disable the Always visible flag that may have been 
        /// set from default preferences. 
        /// This should be used for detached drawings when the user 
        /// makes a manual adjustment to visibility.
        /// </summary>
        public void DisableAlwaysVisible()
        {
            // If the drawing is created while the user has the "always visible" option 
            // enabled in the default visibility preferences, we need to explicitly disable
            // it when the user change any visibility option. 
            // This is for drawings that don't have an explicit "custom parameters" dialog.
            useDefault = false;
            alwaysVisible = false;
        }

        /// <summary>
        /// Returns the opacity based on the fading configuration and the drawing insertion time.
        /// </summary>
        public double GetOpacityFactor(long timestamp)
        {
            if (useDefault)
            {
                InfosFading info = PreferencesManager.PlayerPreferences.DefaultFading;
                return ComputeOpacityFactor(referenceTimestamp, timestamp, info.alwaysVisible, info.opaqueFrames, info.fadingFrames, info.MasterFactor);
            }
            else
            {
                return ComputeOpacityFactor(referenceTimestamp, timestamp, alwaysVisible, opaqueFrames, fadingFrames, masterFactor);
            }
        }

        /// <summary>
        /// Returns an opacity suitable for trackable drawings.
        /// The relative timestamps is the time distance between the current video time and the closest tracked value.
        /// The goal is to have fading around tracked values, unless overriden by the custom mode.
        /// </summary>
        public double GetOpacityTrackable(long relativeTimestamps, long currentTimestamp)
        {
            // Opacity based on the drawing insertion frame.
            double baselineOpacity = GetOpacityFactor(currentTimestamp);

            // Negative relative time means the drawing has no tracking data whatsoever.
            if (relativeTimestamps < 0)
                return baselineOpacity;

            // Fading based on distance to the closest tracked frame.
            long fadingTimestamps = fadingFrames * averageTimeStampsPerFrame;
            double relativeOpacity = 1.0f - ((float)relativeTimestamps / (float)fadingTimestamps);
            relativeOpacity = Math.Max(0, relativeOpacity) * masterFactor;

            // Take the max, in order to honor the "Always visible" mode as well as opaque sections that go beyond the end of the tracked section.
            return Math.Max(baselineOpacity, relativeOpacity);
        }

        public bool IsVisible(long referenceTimestamp, long testTimestamp, int visibleFrames)
        {
            return ComputeOpacityFactor(referenceTimestamp, testTimestamp, (long)visibleFrames) > 0;
        }
        
        private double ComputeOpacityFactor(long referenceTimestamp, long testTimestamp, long fadingFrames)
        {
            long distanceTimestamps = Math.Abs(testTimestamp - referenceTimestamp);
            long fadingTimestamps = fadingFrames * averageTimeStampsPerFrame;
            return distanceTimestamps > fadingTimestamps ? 0.0f : 1.0f - ((double)distanceTimestamps / (double)fadingTimestamps);
        }

        private double ComputeOpacityFactor(long referenceTimestamp, long testTimestamp, bool alwaysVisible, long opaqueFrames, long fadingFrames, float masterFactor)
        {
            if (alwaysVisible)
                return 1.0f * masterFactor;

            long opaqueTimestamps = ((opaqueFrames-1) * averageTimeStampsPerFrame);
            long opaqueStart = referenceTimestamp;
            long opaqueEnd = opaqueStart + opaqueTimestamps;

            if ((testTimestamp >= opaqueStart) && (testTimestamp <= opaqueEnd))
                return 1.0f * masterFactor;

            long distanceTimestamps;
            if (testTimestamp < opaqueStart)
                distanceTimestamps = opaqueStart - testTimestamp;
            else
                distanceTimestamps = testTimestamp - opaqueEnd;

            long fadingTimestamps = fadingFrames * averageTimeStampsPerFrame;
            float factor = 1.0f - ((float)distanceTimestamps / (float)fadingTimestamps);
            factor = Math.Max(0, factor);

            return factor * masterFactor;
        }
    }
}
