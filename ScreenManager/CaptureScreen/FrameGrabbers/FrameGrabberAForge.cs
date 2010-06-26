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

using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameGrabberAForge - a FrameGrabber using DirectShow via AForge library.
	/// </summary>
	public class FrameGrabberAForge : AbstractFrameGrabber
	{
		#region Properties
		public override bool IsConnected
		{
			get { return m_bIsConnected; }
		}	
		public override bool IsGrabbing
		{
			get {return m_bIsGrabbing;}
		}
		public override string DeviceName
		{
			get { return m_CurrentVideoDevice.Name; }
		}
		#endregion
		
		#region Members
		private VideoCaptureDevice m_VideoDevice;
		private DeviceIdentifier m_CurrentVideoDevice;
		private IFrameGrabberContainer m_Container;	// FrameServerCapture seen through a limited interface.
		private FrameBuffer m_FrameBuffer;
		private bool m_bIsConnected;
		private bool m_bIsGrabbing;
		private bool m_bSizeKnown;
		private int m_iConnectionsAttempts;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FrameGrabberAForge(IFrameGrabberContainer _parent, FrameBuffer _buffer)
		{
			m_Container = _parent;
			m_FrameBuffer = _buffer;
		}
		#endregion
		
		#region AbstractFrameGrabber implementation
		public override void PromptDeviceSelector()
		{
			// Ask the user which device he wants to use.
			formDevicePicker fdp = new formDevicePicker(ListDevices(), m_CurrentVideoDevice);
			if(fdp.ShowDialog() == DialogResult.OK)
			{
				DeviceIdentifier selected = fdp.SelectedDevice;
				if(selected != null)
				{
					m_CurrentVideoDevice = selected;
					ConnectToDevice(m_CurrentVideoDevice);
					m_Container.Connected();
				}
			}
			fdp.Dispose();
		}
		public override void NegociateDevice()
		{
			if(!m_bIsConnected)
			{
				log.Debug("Try to connect to a Capture Device.");
				
				m_iConnectionsAttempts++;
	        	
				//----------------------------------------------------------------------------------------
				// TODO: Proper device negociation.
	        	// Apparently there's no way to know if a device is already used by an application.
	        	// Let's try to connect to the first device if any.
	        	// If the device is already streaming to another application, we will connect successfully, 
	        	// but the NewFrame event will never be raised for us.
	        	//----------------------------------------------------------------------------------------
	        	
	        	FilterInfoCollection videoDevices = new FilterInfoCollection( FilterCategory.VideoInputDevice );
	        	
	        	if(videoDevices.Count > 0)
	        	{
	        		ConnectToDevice(videoDevices, 0);
	        	}
	        	
	        	if(!m_bIsConnected && m_iConnectionsAttempts == 2)
	        	{
	        		m_Container.AlertCannotConnect();
	        	}
			}
		}
		public override void StartGrabbing()
		{
			if(m_bIsConnected && m_VideoDevice != null)
			{
				if(!m_bIsGrabbing)
				{
					m_VideoDevice.Start();
				}
				
				m_bIsGrabbing = true;
				log.Debug("Starting to grab frames from the capture device.");
			}
		}
		public override void PauseGrabbing()
		{
			if(m_bIsConnected && m_VideoDevice != null)
			{
				log.Debug("Pausing frame grabbing.");
				if(m_bIsGrabbing)
				{
					m_VideoDevice.Stop();
				}
				m_bIsGrabbing = false;
			}
		}
		public override void BeforeClose()
		{
			Disconnect();
		}
		#endregion
		
		#region Private methods
		private List<DeviceIdentifier> ListDevices()
		{
			// List all the devices currently connected.
			FilterInfoCollection videoDevices = new FilterInfoCollection( FilterCategory.VideoInputDevice );
			
			List<DeviceIdentifier> devices = new List<DeviceIdentifier>();
			
			foreach(FilterInfo fi in videoDevices)
			{
				devices.Add(new DeviceIdentifier(fi.Name, fi.MonikerString));				
			}

			return devices;
		}
		private void ConnectToDevice(FilterInfoCollection _devices, int _iSelected)
		{
			m_CurrentVideoDevice = new DeviceIdentifier(_devices[_iSelected].Name, _devices[_iSelected].MonikerString);
			
			log.Debug(String.Format("Connecting to device: index: {0}, name: {1}, moniker string:{2}", 
			                        _iSelected, m_CurrentVideoDevice.Name, m_CurrentVideoDevice.Identification));
			
			ConnectToDevice(m_CurrentVideoDevice);
			m_Container.Connected();
		}
		private void ConnectToDevice(DeviceIdentifier _device)
		{
			Disconnect();
			
			log.Debug(String.Format("Connecting to device. {0}", _device.Name));
				
			m_VideoDevice = new VideoCaptureDevice(_device.Identification);
			m_VideoDevice.DesiredFrameRate = 0;
			m_VideoDevice.NewFrame += new NewFrameEventHandler( VideoDevice_NewFrame );
			m_bIsConnected = true;
		}
		private void Disconnect()
		{
			// The screen is about to be closed, release resources.
			
			if(m_bIsConnected && m_VideoDevice != null)
			{
				log.Debug(String.Format("disconnecting from device. {0}", m_CurrentVideoDevice.Name));
				
				// Reset
				m_bIsGrabbing = false;
				m_bSizeKnown = false;
				m_iConnectionsAttempts = 0;
				m_Container.SetImageSize(Size.Empty);
				
				
				m_VideoDevice.Stop();
				m_VideoDevice.NewFrame -= new NewFrameEventHandler( VideoDevice_NewFrame );
				m_FrameBuffer.Dispose();
				
				m_bIsConnected = false;
			}
		}
		private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			
			// A new frame has been grabbed, push it to the buffer and notifies the frame server.
			if(!m_bSizeKnown)
			{
				m_bSizeKnown = true;
				m_Container.SetImageSize(eventArgs.Frame.Size);
			}
			
			m_FrameBuffer.PushFrame(eventArgs.Frame);
			m_Container.FrameGrabbed();
		}
		#endregion
	}
}
