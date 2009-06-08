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
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Videa.Services
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
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }
        public bool UseDefault
        {
            get { return m_bUseDefault; }
            set { m_bUseDefault = value; }
        }
        public bool AlwaysVisible
        {
            get { return m_bAlwaysVisible; }
            set { m_bAlwaysVisible = value; }
        }
        public int FadingFrames
        {
            get { return m_iFadingFrames; }
            set { m_iFadingFrames = value; }
        }
        public long ReferenceTimestamp
        {
            get { return m_iReferenceTimestamp; }
            set { m_iReferenceTimestamp = value; }
        }
        public long AverageTimeStampsPerFrame
        {
            get { return m_iAverageTimeStampsPerFrame; }
            set { m_iAverageTimeStampsPerFrame = value; }
        }
        #endregion

        #region Members
        private bool m_bEnabled;
        private bool m_bUseDefault;
        private bool m_bAlwaysVisible;
        private int m_iFadingFrames;
        private long m_iReferenceTimestamp;
        private long m_iAverageTimeStampsPerFrame;
        #endregion

        #region Construction
        public InfosFading()
        {
            // this constructor is directly used only by the Preference manager 
            // to create the default fading values.

            m_bEnabled = true;
            m_bUseDefault = true;
            m_bAlwaysVisible = false;
            m_iFadingFrames = 20;
            m_iReferenceTimestamp = 0;
            m_iAverageTimeStampsPerFrame = 0;

        }
        public InfosFading(long _iReferenceTimestamp, long _iAverageTimeStampsPerFrame)
        {
            // This constructor is used by all drawings to get the default values.
            FromInfosFading(PreferencesManager.Instance().DefaultFading);
            m_iReferenceTimestamp = _iReferenceTimestamp;
            m_iAverageTimeStampsPerFrame = _iAverageTimeStampsPerFrame;
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
        }
        public void ToXml(XmlTextWriter _xmlWriter, bool _bMainDefault)
        {
            // Persist this particular Fading infos to xml.
            // if _bMainDefault is true, this is the preferences object.
            // then we don't need to persist everything.

            _xmlWriter.WriteStartElement("InfosFading");
            
            _xmlWriter.WriteStartElement("Enabled");
            _xmlWriter.WriteString(m_bEnabled.ToString());
            _xmlWriter.WriteEndElement();

            _xmlWriter.WriteStartElement("Frames");
            _xmlWriter.WriteString(m_iFadingFrames.ToString());
            _xmlWriter.WriteEndElement();

            if (!_bMainDefault)
            {
                _xmlWriter.WriteStartElement("UseDefault");
                _xmlWriter.WriteString(m_bUseDefault.ToString());
                _xmlWriter.WriteEndElement();

                _xmlWriter.WriteStartElement("AlwaysVisible");
                _xmlWriter.WriteString(m_bAlwaysVisible.ToString());
                _xmlWriter.WriteEndElement();

                // We shouldn't have to write the reference timestamp and avg fpts...
            }

            // </InfosFading>
            _xmlWriter.WriteEndElement();
        }
        public void FromXml(XmlReader _xmlReader)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Enabled")
                    {
                        m_bEnabled = bool.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "Frames")
                    {
                        m_iFadingFrames = int.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "UseDefault")
                    {
                        m_bUseDefault = bool.Parse(_xmlReader.ReadString());
                    }
                    else if (_xmlReader.Name == "AlwaysVisible")
                    {
                        m_bAlwaysVisible = bool.Parse(_xmlReader.ReadString());
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "InfosFading")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            // Sanity check.
            if (m_iFadingFrames < 1) m_iFadingFrames = 1;
        }
        #endregion

        public double GetOpacityFactor(long _iTimestamp)
        {
            double fOpacityFactor = 0.0f;

            if (!m_bEnabled)
            {
                // No fading. (= only if on the Key frame)
                if (_iTimestamp == m_iReferenceTimestamp)
                {
                    fOpacityFactor = 1.0f;
                }
                else
                {
                    fOpacityFactor = 0.0f;
                }
            }
            else if (m_bUseDefault)
            {
                // Default value
                fOpacityFactor = ComputeOpacityFactor(_iTimestamp, PreferencesManager.Instance().DefaultFading.FadingFrames);
            }
            else if (m_bAlwaysVisible)
            {
                // infinite fading. (= persisting drawing)
                fOpacityFactor = 1.0f;
            }
            else
            {
                // Custom value.
                fOpacityFactor = ComputeOpacityFactor(_iTimestamp, m_iFadingFrames);
            }

            return fOpacityFactor;
        }
        private double ComputeOpacityFactor(long _iTimestamp, long iFadingFrames)
        {
            double fOpacityFactor = 0.0f;

            long iDistanceTimestamps = Math.Abs(_iTimestamp - m_iReferenceTimestamp);
            long iFadingTimestamps = iFadingFrames * m_iAverageTimeStampsPerFrame;

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
