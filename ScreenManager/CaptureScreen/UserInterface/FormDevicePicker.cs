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

using Kinovea.ScreenManager.Languages;
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;

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
		public string SelectedUrl
		{
			get { return cmbUrl.Text;}
		}
		public NetworkCameraFormat SelectedFormat
		{
			get
			{
				NetworkCameraFormat selected = NetworkCameraFormat.JPEG;
				if(cmbStreamType.SelectedIndex >= 0)
				{
					selected = (NetworkCameraFormat)cmbStreamType.SelectedIndex;
				}
				return selected;
			}
		}
		
		#endregion
		
		#region Members
		private DeviceDescriptor m_CurrentDevice;
		private PromptDevicePropertyPage m_PromptDevicePropertyPage;
		#endregion
		
		public formDevicePicker(List<DeviceDescriptor> _devices, DeviceDescriptor _currentDevice, PromptDevicePropertyPage _PromptDevicePropertyPage)
		{
			m_PromptDevicePropertyPage = _PromptDevicePropertyPage;
			m_CurrentDevice = _currentDevice;
			
			InitializeComponent();
			
			this.Text = "   " + ScreenManagerLang.ToolTip_DevicePicker;
			btnApply.Text = ScreenManagerLang.Generic_Apply;
			btnCancel.Text = ScreenManagerLang.Generic_Cancel;
			gpCurrentDevice.Text = ScreenManagerLang.dlgDevicePicker_CurrentDevice;
			gpOtherDevices.Text = ScreenManagerLang.dlgDevicePicker_SelectAnother;
			lblUrl.Text = ScreenManagerLang.dlgDevicePicker_Url;
			lblStreamType.Text = ScreenManagerLang.dlgDevicePicker_Type;
			lblConfig.Text = ScreenManagerLang.Generic_Configuration + " :";
			lblNoConf.Text = ScreenManagerLang.dlgDevicePicker_NoConf;
			btnDeviceProperties.Text = ScreenManagerLang.dlgDevicePicker_DeviceProperties;
			lblNoConf.Top = lblConfig.Top;
			lblNoConf.Left = lblConfig.Right;
			lblUrl.Location = lblConfig.Location;
			cmbUrl.Top = cmbCapabilities.Top - 3;
			cmbUrl.Left = lblCurrentlySelected.Left;
			cmbUrl.Width = cmbOtherDevices.Right - cmbUrl.Left;
			cmbStreamType.Top = btnDeviceProperties.Top - 3;
			cmbStreamType.Left = lblCurrentlySelected.Left;
			
			// Populate current device.
			if(_currentDevice == null)
			{
				// No device. This can happen if there is no capture device connected.
				lblCurrentlySelected.Text = ScreenManagerLang.Capture_CameraNotFound;
			}
			else
			{
				lblCurrentlySelected.Text =  _currentDevice.Name;
			}
			
			DisplayConfControls(_currentDevice);
			
			// Populate other devices.
			int selectedDev = 0;
			for(int i = 0;i<_devices.Count;i++)
			{
				DeviceDescriptor dd = _devices[i];
				cmbOtherDevices.Items.Add(dd);
				
				if(_currentDevice == null)
				{
					selectedDev = 0;
				}
				else if(dd.Identification == _currentDevice.Identification)
				{
					selectedDev = i;
				}
			}
			
			cmbOtherDevices.SelectedIndex = selectedDev;
			gpOtherDevices.Enabled = _devices.Count > 1;
		}
		private void DisplayConfControls(DeviceDescriptor _currentDevice)
		{
			if(_currentDevice != null)
			{
				lblConfig.Visible = !_currentDevice.Network;
				lblNoConf.Visible = !_currentDevice.Network;
				btnDeviceProperties.Visible = !_currentDevice.Network;
				cmbCapabilities.Visible = !_currentDevice.Network;
				
				lblUrl.Visible = _currentDevice.Network;
				lblStreamType.Visible = _currentDevice.Network;
				cmbUrl.Visible = _currentDevice.Network;
				cmbStreamType.Visible = _currentDevice.Network;
				
				if(_currentDevice.Network)
				{
					btnCamcorder.Image = Resources.camera_network2;
					PreferencesManager pm = PreferencesManager.Instance();
					
					// Recently used cameras.
					cmbUrl.Text = _currentDevice.NetworkCameraUrl;
					if(pm.RecentNetworkCameras.Count > 0)
					{
						foreach(string url in pm.RecentNetworkCameras)
						{
							cmbUrl.Items.Add(url);
						}
					}
					else
					{
						cmbUrl.Items.Add(_currentDevice.NetworkCameraUrl);
					}
					
					// Type of streams supported.
					cmbStreamType.Items.Add("JPEG");
					cmbStreamType.Items.Add("MJPEG");
					if(_currentDevice.NetworkCameraFormat == NetworkCameraFormat.JPEG)
					{
						cmbStreamType.SelectedIndex = 0;
					}
					else if(_currentDevice.NetworkCameraFormat == NetworkCameraFormat.MJPEG)
					{
						cmbStreamType.SelectedIndex = 1;
					}
					else
					{
						_currentDevice.NetworkCameraFormat = NetworkCameraFormat.JPEG;
						cmbStreamType.SelectedIndex = 0;
					}
				}
				else
				{
					btnCamcorder.Image = Resources.camera_selected;
					
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
					
					if(_currentDevice.Capabilities.Count > 0)
					{
						cmbCapabilities.SelectedIndex = selectedCap;
						lblNoConf.Visible = false;
						cmbCapabilities.Visible = true;
					}
					else
					{
						lblNoConf.Visible = true;
						cmbCapabilities.Visible = false;
					}
				}	
			}
			else
			{
				btnCamcorder.Image = Resources.camera_notfound;
				
				// No device currently selected.
				lblConfig.Visible = false;
				lblNoConf.Visible = false;
				btnDeviceProperties.Visible = false;
				cmbCapabilities.Visible = false;
				
				lblUrl.Visible = false;
				lblStreamType.Visible = false;
				cmbUrl.Visible = false;
				cmbStreamType.Visible = false;
			}
		}
		private void cmbOtherDevices_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Enable / disable the configuration picker if we change device,
			// so the user doesn't think he could change both at the same time.
			DeviceDescriptor selected = cmbOtherDevices.Items[cmbOtherDevices.SelectedIndex] as DeviceDescriptor;
			if(m_CurrentDevice == null)
			{
				gpCurrentDevice.Enabled = false;	
			}
			else if(m_CurrentDevice.Network)
			{
				gpCurrentDevice.Enabled = selected.Network;
			}
			else
			{
				gpCurrentDevice.Enabled = !selected.Empty && !selected.Network && (selected.Identification == m_CurrentDevice.Identification);
			} 
		}
		
		private void btnDeviceProperties_Click(object sender, EventArgs e)
		{
			// Ask the API to display the device property page.
			// This page is implemented by the driver.
			if(m_PromptDevicePropertyPage != null)
			{
				m_PromptDevicePropertyPage(this.Handle);
			}
		}
	}
}
