#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing;

namespace Kinovea.Services
{
	/// <summary>
	/// DeviceCapability - one possible output for the device.
	/// Typically, webcams will expose several capabilities (ex: 640x480@30fps, 320x200@60fps)
	/// </summary>
	public class DeviceCapability
	{
		#region Properties
		public int NumberOfPixels
		{
			get {return m_FrameSize.Width * m_FrameSize.Height; }
		}
		public Size FrameSize
		{
			get { return m_FrameSize; }
		}        
		public int Framerate
		{
			get { return m_iFramerate; }
		}
        #endregion
		
		#region Members
		private Size m_FrameSize;
        private int m_iFramerate;
        #endregion
		
        public DeviceCapability(Size _size, int _framerate)
        {
        	m_FrameSize = _size;
        	m_iFramerate = _framerate;
        }
		public override string ToString()
		{
			return String.Format("{0}×{1} px @ {2} fps", m_FrameSize.Width, m_FrameSize.Height, m_iFramerate);
		}
		public override bool Equals(object obj)
		{
			bool equals = false;
			
			DeviceCapability dc = obj as DeviceCapability;
			if(dc != null)
			{
				equals = (m_FrameSize.Width == dc.FrameSize.Width && m_FrameSize.Height == dc.FrameSize.Height && m_iFramerate == dc.Framerate);
			}
			
			return equals;
		}
		public override int GetHashCode()
		{
			// Combine all relevant fields with XOR to get the Hash.
            int iHash = m_iFramerate.GetHashCode();
            iHash ^= m_FrameSize.GetHashCode();
            return iHash;
		}
	}
}
