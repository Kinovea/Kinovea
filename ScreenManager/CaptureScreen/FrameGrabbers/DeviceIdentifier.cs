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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractDevice, a class to represent a device identification.
	/// </summary>
	public class DeviceIdentifier
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
		#endregion
		
		#region Members
		private string m_Identification;
		private string m_Name;
		#endregion
		
		#region Constructor
		public DeviceIdentifier(string _name, string _identification)
		{
			m_Name = _name;
			m_Identification = _identification;
		}
		#endregion
	
		public override string ToString()
		{
			return m_Name;
		}
	
	}
}
