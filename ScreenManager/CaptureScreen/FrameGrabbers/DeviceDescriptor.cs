#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Collections.Generic;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractDevice, a class to represent a device identification.
	/// </summary>
	public class DeviceDescriptor
	{
		#region Properties
		public string Name
		{
			get { return m_Name; }
		}
		public string Identification
		{
			get { return m_Identification; }
		}		
		public List<DeviceCapability> Capabilities
		{
			get { return m_Capabilities; }
			set { m_Capabilities = value; }
		}
		public DeviceCapability SelectedCapability
		{
			get { return m_SelectedCapability; }
			set { m_SelectedCapability = value; }
		}
		#endregion
		
		#region Members
		private string m_Identification;
		private string m_Name;
		private List<DeviceCapability> m_Capabilities = new List<DeviceCapability>();
		private DeviceCapability m_SelectedCapability;
		#endregion
		
		#region Constructor
		public DeviceDescriptor(string _name, string _identification)
		{
			m_Name = _name;
			m_Identification = _identification;
		}
		#endregion
	
		public DeviceCapability GetBestSizeCapability()
		{
			DeviceCapability bestCap = m_Capabilities[0];
			int maxPixels = bestCap.NumberOfPixels;
			
			for(int i = 1;i<m_Capabilities.Count;i++)
			{
				if(m_Capabilities[i].NumberOfPixels > maxPixels)
				{
					bestCap = m_Capabilities[i];
					maxPixels = bestCap.NumberOfPixels;	
				}
			}
			
			return bestCap;
		}
		public DeviceCapability GetCapabilityFromSpecs(DeviceCapability _cap)
		{
			DeviceCapability matchCap = null;
			
			foreach(DeviceCapability cap in m_Capabilities)
			{
				if(cap.Equals(_cap))
				{
					matchCap = cap;
				}
			}
			
			return matchCap;
		}
		public override string ToString()
		{
			return m_Name;
		}
	}
}
