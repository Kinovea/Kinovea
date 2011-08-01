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
		public float MasterFactor
		{
			get { return m_fMasterFactor; }
			set { m_fMasterFactor = value; }
		}
        #endregion

        #region Members
        private bool m_bEnabled;
        private bool m_bUseDefault;
        private bool m_bAlwaysVisible;
        private int m_iFadingFrames;
        private long m_iReferenceTimestamp;
        private long m_iAverageTimeStampsPerFrame;
        private float m_fMasterFactor = 1.0f;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            m_fMasterFactor = 1.0f;

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
            this.MasterFactor = _origin.MasterFactor;
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteElementString("Enabled", m_bEnabled ? "true" : "false");
            _xmlWriter.WriteElementString("Frames", m_iFadingFrames.ToString());
            _xmlWriter.WriteElementString("AlwaysVisible", m_bAlwaysVisible ? "true" : "false");
            _xmlWriter.WriteElementString("UseDefault", m_bUseDefault ? "true" : "false");
        }
        public void ReadXml(XmlReader _xmlReader)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Enabled":
				        m_bEnabled = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
					case "Frames":
				        m_iFadingFrames = _xmlReader.ReadElementContentAsInt();
                        break;
					case "UseDefault":
				        m_bUseDefault = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
                    case "AlwaysVisible":
						m_bAlwaysVisible = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
						break;
				    default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
			
			// Sanity check.
            if (m_iFadingFrames < 1) m_iFadingFrames = 1;
        }
        #endregion

        public double GetOpacityFactor(long _iTimestamp)
        {
            double fOpacityFactor = 0.0f;

            if (!m_bEnabled)
            {
                // No fading.
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
                InfosFading info = PreferencesManager.Instance().DefaultFading;
                if(info.AlwaysVisible)
                {
                	fOpacityFactor = 1.0f;
                }
                else
                {
                	fOpacityFactor = ComputeOpacityFactor(m_iReferenceTimestamp, _iTimestamp, info.FadingFrames);
                }
            }
            else if (m_bAlwaysVisible)
            {
                // infinite fading. (= persisting drawing)
                fOpacityFactor = 1.0f;
            }
            else
            {
                // Custom value.
                fOpacityFactor = ComputeOpacityFactor(m_iReferenceTimestamp, _iTimestamp, m_iFadingFrames);
            }

            return fOpacityFactor * m_fMasterFactor;
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
