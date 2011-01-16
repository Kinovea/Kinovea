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
		public DeviceDescriptor SelectedDevice
		{
			get 
			{ 
				DeviceDescriptor selected = null;
				if(cmbOtherDevices.SelectedIndex >= 0)
				{
					selected = cmbOtherDevices.Items[cmbOtherDevices.SelectedIndex] as DeviceDescriptor;	
				}
				return selected;
			}
		}
		public DeviceCapability SelectedCapability
		{
			get 
			{ 
				DeviceCapability selected = null;
				if(cmbCapabilities.SelectedIndex >= 0)
				{
					selected = cmbCapabilities.Items[cmbCapabilities.SelectedIndex] as DeviceCapability;	
				}
				return selected;
			}
		}
		
		#endregion
		
		public formDevicePicker(List<DeviceDescriptor> _devices, DeviceDescriptor _currentDevice)
		{
			InitializeComponent();
			
			this.Text = "   " + ScreenManagerLang.ToolTip_DevicePicker;
			this.btnApply.Text = ScreenManagerLang.Generic_Apply;
			this.btnCancel.Text = ScreenManagerLang.Generic_Cancel;
			this.gpCurrentDevice.Text = ScreenManagerLang.dlgDevicePicker_CurrentDevice;
			this.gpOtherDevices.Text = ScreenManagerLang.dlgDevicePicker_SelectAnother;
			this.lblConfig.Text = ScreenManagerLang.Generic_Configuration;
			
			// Populate current.
			lblCurrentlySelected.Text =  _currentDevice.Name;
			int selectedCap = 0;
			for(int i = 0;i<_currentDevice.Capabilities.Count;i++)
			{
				DeviceCapability dc = _currentDevice.Capabilities[i];
				cmbCapabilities.Items.Add(dc);
				if(dc == _currentDevice.SelectedCapability)
				{
					selectedCap = i;
				}
			}
			cmbCapabilities.SelectedIndex = selectedCap;
			
			// Populate other devices.
			int selectedDev = 0;
			for(int i = 0;i<_devices.Count;i++)
			{
				DeviceDescriptor dd = _devices[i];
				cmbOtherDevices.Items.Add(dd);
				
				if(dd.Identification == _currentDevice.Identification)
				{
					selectedDev = i;
				}
			}
			
			cmbOtherDevices.SelectedIndex = selectedDev;
			
			gpOtherDevices.Enabled = _devices.Count > 1;
		}
	}
}
