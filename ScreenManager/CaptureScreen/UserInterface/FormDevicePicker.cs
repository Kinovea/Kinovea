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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Description of FormDevicePicker.
	/// </summary>
	public partial class formDevicePicker : Form
	{
		#region Properties
		public DeviceIdentifier SelectedDevice
		{
			get 
			{ 
				DeviceIdentifier selected = null;
				if(lstOtherDevices.SelectedIndex >= 0)
				{
					selected = lstOtherDevices.Items[lstOtherDevices.SelectedIndex] as DeviceIdentifier;	
				}

				return selected;
			}
		}
		#endregion
		
		
		
		public formDevicePicker(List<DeviceIdentifier> _devices, DeviceIdentifier _currentDevice)
		{
			InitializeComponent();
			
			// TODO: i18n.
			
			// Current
			lblCurrentlySelected.Text = String.Format("Currently selected: {0}", _currentDevice.Name);
			
			foreach(DeviceIdentifier di in _devices)
			{
				if(di.Identification != _currentDevice.Identification)
				{
					lstOtherDevices.Items.Add(di);
				}
			}
		}
				
		private void lstOtherDevices_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnApply.Enabled = true;
		}
	}
}
