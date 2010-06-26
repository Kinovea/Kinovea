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
using Kinovea.ScreenManager.Languages;
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
			
			this.Text = "   " + ScreenManagerLang.ToolTip_DevicePicker;
			this.btnApply.Text = ScreenManagerLang.Generic_Apply;
			this.btnCancel.Text = ScreenManagerLang.Generic_Cancel;
			
			lblCurrentlySelected.Text = ScreenManagerLang.dlgDevicePicker_CurrentlySelected + " " + _currentDevice.Name;
			lblSelectAnother.Text = ScreenManagerLang.dlgDevicePicker_SelectAnother;
			
			// Populate the list.
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
