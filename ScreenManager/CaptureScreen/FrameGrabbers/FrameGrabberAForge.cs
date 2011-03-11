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

using AForge.Video;
using AForge.Video.DirectShow;
using Kinovea.Services;

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
		public override double FramesInterval
		{
			get { return m_FramesInterval; }
		}
		public override Size FrameSize
		{
			// This may not be used because the user may want to bypass and force an aspect ratio.
			// In this case, only the FrameServerCapture is aware of the final image size.
			get { return m_FrameSize; }
		}
		public override DeviceCapability SelectedCapability
		{
			get { return m_CurrentVideoDevice.SelectedCapability; }
		}
		#endregion
			
		#region Members
		private VideoCaptureDevice m_VideoDevice;
		private DeviceDescriptor m_CurrentVideoDevice;
		private IFrameGrabberContainer m_Container;	// FrameServerCapture seen through a limited interface.
		private FrameBuffer m_FrameBuffer;
		private bool m_bIsConnected;
		private bool m_bIsGrabbing;
		private bool m_bSizeKnown;
		private bool m_bSizeChanged;
		private double m_FramesInterval = -1;
		private Size m_FrameSize;
		private int m_iConnectionsAttempts;
		private int m_iGrabbedSinceLastCheck;
		private int m_iConnectionsWithoutFrames;
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
			// Ask the user which device he wants to use or which size/framerate.
			formDevicePicker fdp = new formDevicePicker(ListDevices(), m_CurrentVideoDevice);
			if(fdp.ShowDialog() == DialogResult.OK)
			{
				DeviceDescriptor dev = fdp.SelectedDevice;
				if(dev != null)
				{
					if(dev.Identification == m_CurrentVideoDevice.Identification)
					{
						// Device unchanged.
						DeviceCapability cap = fdp.SelectedCapability;
						if(cap != null)
						{
							if(!cap.Equals(m_CurrentVideoDevice.SelectedCapability))
							{
								// Changed capability.
								m_CurrentVideoDevice.SelectedCapability = cap;
								PreferencesManager pm = PreferencesManager.Instance();
								pm.UpdateSelectedCapability(m_CurrentVideoDevice.Identification, cap);
								
								if(m_bIsGrabbing)
								{
									m_VideoDevice.Stop();
								}
								
								m_VideoDevice.DesiredFrameSize = cap.FrameSize;
								m_VideoDevice.DesiredFrameRate = cap.Framerate;
								
								m_FrameSize = cap.FrameSize;
								m_FramesInterval = 1000 / (double)cap.Framerate;
					
								log.Debug(String.Format("Picked new capability: {0}", cap.ToString()));
								
								m_bSizeChanged = true;
								
								if(m_bIsGrabbing)
								{
									m_VideoDevice.Start();
								}
							}
						}
					}
					else
					{
						// Changed device
						m_CurrentVideoDevice = dev;
						ConnectToDevice(m_CurrentVideoDevice);
						m_Container.Connected();
					}
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
	        	
	        	FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
	        	
	        	if(videoDevices != null && videoDevices.Count > 0)
	        	{
	        		ConnectToDevice(videoDevices, 0);
	        	}
	        	
	        	m_iGrabbedSinceLastCheck = 0;
	        	
	        	if(m_bIsConnected)
	        	{
	        		m_iConnectionsWithoutFrames++;
	        	}
	        	
	        	if(!m_bIsConnected && m_iConnectionsAttempts == 2)
	        	{
	        		m_Container.AlertCannotConnect();
	        	}
			}
		}
		public override void CheckDeviceConnection()
		{
			//--------------------------------------------------------------------------------------------------
			// Try to check if we're still connected to the video source.
			// The problem is that we are not notified when the source disconnects.
			//
			// The only way we have to detect is to count the number of frames we received since last check.
			// If we are supposed to do grabbing and we received nothing, we are in one of two conditions:
			// 1. The device has been disconnected.
			// 2. We are in STOP state of Play/Edit mode, but the device is still connected.
			//
			// The problem is that even in condition 1, we may still succeed in reconnecting.
			// But, if we detect that we CONSTANTLY succeed in reconnecting but there are still no frames coming,
			// we are probably in condition 2. Thus we'll stop trying to disconnect/reconnect, and just wait
			// for the source to start sending frames again.
			//
			// Note:
			// This prevents working with very slow capturing devices (less than one frame per second).
			// This doesn't work if we are not currently grabbing.
			//--------------------------------------------------------------------------------------------------
			if(m_iConnectionsWithoutFrames < 2)
			{
				if(m_bIsGrabbing && m_iGrabbedSinceLastCheck == 0)
				{
					log.Debug(String.Format("Device has been disconnected."));
					m_VideoDevice.SignalToStop();
					m_VideoDevice.WaitForStop();
					
					m_bIsGrabbing = false;
					m_bIsConnected = false;
					m_Container.AlertConnectionLost();
					 
					// Set connection attempts so we don't show the initial error message.
					m_iConnectionsAttempts = 2;
				}
				else
				{
					//log.Debug(String.Format("Device is still connected, or we are not grabbing and can't check."));
				}
			}
			else
			{
				//log.Debug(String.Format("Device has succeeded in reconnecting twice, but still doesn't send frames."));
				//log.Debug(String.Format("This probably means it is on STOP state, we will wait for frames without disconnecting."));
			}
			
			m_iGrabbedSinceLastCheck = 0;
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
			if(m_VideoDevice != null)
			{
				log.Debug("Pausing frame grabbing.");
				
				if(m_bIsConnected && m_bIsGrabbing)
				{
					m_VideoDevice.Stop();
				}
				
				m_bIsGrabbing = false;
				m_iGrabbedSinceLastCheck = 0;
			}
		}
		public override void BeforeClose()
		{
			Disconnect();
		}
		#endregion
		
		#region Private methods
		private List<DeviceDescriptor> ListDevices()
		{
			// List all the devices currently connected.
			FilterInfoCollection videoDevices = new FilterInfoCollection( FilterCategory.VideoInputDevice );
			
			List<DeviceDescriptor> devices = new List<DeviceDescriptor>();
			
			foreach(FilterInfo fi in videoDevices)
			{
				devices.Add(new DeviceDescriptor(fi.Name, fi.MonikerString));				
			}

			return devices;
		}
		private void ConnectToDevice(FilterInfoCollection _devices, int _iSelected)
		{
			m_CurrentVideoDevice = new DeviceDescriptor(_devices[_iSelected].Name, _devices[_iSelected].MonikerString);
			if(m_CurrentVideoDevice != null)
			{
				log.Debug(String.Format("Connecting to device: index: {0}, name: {1}, moniker string:{2}", 
			                        _iSelected, m_CurrentVideoDevice.Name, m_CurrentVideoDevice.Identification));
			
				ConnectToDevice(m_CurrentVideoDevice);
				m_Container.Connected();
			}
			else
			{
				log.Error(String.Format("Couldn't create the DeviceIdentifier: index: {0}, name: {1}, moniker string:{2}", 
			                        _iSelected, _devices[_iSelected].Name, _devices[_iSelected].MonikerString));
			}
		}
		private void ConnectToDevice(DeviceDescriptor _device)
		{
			Disconnect();
			
			log.Debug(String.Format("Connecting to device. {0}", _device.Name));
				
			m_VideoDevice = new VideoCaptureDevice(_device.Identification);
			if(m_VideoDevice != null)
			{
				if((m_VideoDevice.VideoCapabilities != null) && (m_VideoDevice.VideoCapabilities.Length > 0))
				{
					// Import the capabilities of the device.
					foreach(VideoCapabilities vc in m_VideoDevice.VideoCapabilities)
					{
						DeviceCapability dc = new DeviceCapability(vc.FrameSize, vc.FrameRate);
						_device.Capabilities.Add(dc);
						
						log.Debug(String.Format("Device Capability. {0}", dc.ToString()));
					}
					
					DeviceCapability selectedCapability = null;
					
					// Check if we already know this device and have a preferred configuration.
					PreferencesManager pm = PreferencesManager.Instance();
					foreach(DeviceConfiguration conf in pm.DeviceConfigurations)
					{
						if(conf.id == _device.Identification)
						{							
							// Try to find the previously selected capability.
							selectedCapability = _device.GetCapabilityFromSpecs(conf.cap);
							if(selectedCapability != null)
								log.Debug(String.Format("Picking capability from preferences: {0}", selectedCapability.ToString()));
						}
					}

					if(selectedCapability == null)
					{
						// Pick the one with max frame size.
						selectedCapability = _device.GetBestSizeCapability();
						log.Debug(String.Format("Picking a default capability (best size): {0}", selectedCapability.ToString()));
						pm.UpdateSelectedCapability(_device.Identification, selectedCapability);
					}
					
					_device.SelectedCapability = selectedCapability;
					m_VideoDevice.DesiredFrameSize = selectedCapability.FrameSize;
					m_FrameSize = selectedCapability.FrameSize;
					m_FramesInterval = 1000 / (double)selectedCapability.Framerate;
				}
				else
				{
					m_VideoDevice.DesiredFrameRate = 0;
				}
				
				m_VideoDevice.NewFrame += new NewFrameEventHandler( VideoDevice_NewFrame );
				m_VideoDevice.VideoSourceError += new VideoSourceErrorEventHandler( VideoDevice_VideoSourceError );
				
				m_bIsConnected = true;
			}
			else
			{
				log.Error("Couldn't create the VideoCaptureDevice.");
			}

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
				m_VideoDevice.VideoSourceError -= new VideoSourceErrorEventHandler( VideoDevice_VideoSourceError );
				m_FrameBuffer.Clear();
				
				m_bIsConnected = false;
			}
		}
		private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// A new frame has been grabbed, push it to the buffer and notifies the frame server.
			if(!m_bSizeKnown || m_bSizeChanged)
			{
				m_bSizeKnown = true;
				m_bSizeChanged = false;
				Size sz = eventArgs.Frame.Size;
				
				m_Container.SetImageSize(sz);
				log.Debug(String.Format("Device infos : {0}. Received frame : {1}", m_FrameSize, sz));
				
				// Update the "official" size (used for saving context.)
				m_FrameSize = sz;
			}
			
			m_iConnectionsWithoutFrames = 0;
			m_iGrabbedSinceLastCheck++;
			m_FrameBuffer.Write(eventArgs.Frame);
			m_Container.FrameGrabbed();
		}
		private void VideoDevice_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
		{
			log.Error(String.Format("Error happened to device."));
		}
		#endregion
	}
}
