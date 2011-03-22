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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameGrabberAForge - a FrameGrabber using DirectShow via AForge library.
	/// We define 3 type of sources:
	/// - capture sources. (Directshow devices)
	/// - network source. Built-in source to represent a network camera.
	/// - empty source. Built-in source when no capture sources have been found.
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
		private IVideoSource m_VideoSource;
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
		private PreferencesManager m_PrefsManager = PreferencesManager.Instance();
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
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.DeactivateKeyboardHandler != null)
            {
                dp.DeactivateKeyboardHandler();
			}
			
			bool reconnected = false;
			
			// Ask the user which device he wants to use or which size/framerate.
			formDevicePicker fdp = new formDevicePicker(ListDevices(), m_CurrentVideoDevice, new PromptDevicePropertyPage(DisplayDevicePropertyPage));
			
			if(fdp.ShowDialog() == DialogResult.OK)
			{
				DeviceDescriptor dev = fdp.SelectedDevice;
				
				if(dev == null || dev.Empty)
				{
					log.DebugFormat("Selected device is null or empty.");
					if(m_CurrentVideoDevice != null)
					{
						// From something to empty.
						Disconnect();
					}
				}
				else if(dev.Network)
				{
					if(m_CurrentVideoDevice == null || !m_CurrentVideoDevice.Network)
					{
						// From empty or non-network to network.
						log.DebugFormat("Selected network camera - connect with default parameters");
						reconnected = ConnectToDevice(dev);	
					}
					else
					{
						// From network to network.
						log.DebugFormat("Network camera - parameters changed - connect with new parameters");
						// Parameters were set on the dialog. We don't care if the parameters were actually changed.
						DeviceDescriptor netDevice = new DeviceDescriptor(ScreenManagerLang.Capture_NetworkCamera, fdp.SelectedUrl, fdp.SelectedFormat);
						reconnected = ConnectToDevice(netDevice);
					}
				} 
				else
				{
					if(m_CurrentVideoDevice == null || m_CurrentVideoDevice.Network || dev.Identification != m_CurrentVideoDevice.Identification)
					{
						// From network or different capture device to capture device.
						log.DebugFormat("Selected capture device");
						reconnected = ConnectToDevice(dev);
					}
					else
					{
						// From same capture device - caps changed.
						DeviceCapability cap = fdp.SelectedCapability;
						if(cap != null && !cap.Equals(m_CurrentVideoDevice.SelectedCapability))
						{							
							log.DebugFormat("Capture device, capability changed.");
							
							m_CurrentVideoDevice.SelectedCapability = cap;
							m_PrefsManager.UpdateSelectedCapability(m_CurrentVideoDevice.Identification, cap);
							
							if(m_bIsGrabbing)
							{
								m_VideoSource.Stop();
							}
							
							((VideoCaptureDevice)m_VideoSource).DesiredFrameSize = cap.FrameSize;
							((VideoCaptureDevice)m_VideoSource).DesiredFrameRate = cap.Framerate;
							
							m_FrameSize = cap.FrameSize;
							m_FramesInterval = 1000 / (double)cap.Framerate;
				
							log.Debug(String.Format("New capability: {0}", cap.ToString()));
							
							m_bSizeChanged = true;
							
							if(m_bIsGrabbing)
							{
								m_VideoSource.Start();
							}	
						}
					}
				}
				
				if(reconnected)
				{
					m_Container.Connected();
				}
				
			}
			
			fdp.Dispose();
			
			if(dp.ActivateKeyboardHandler != null)
            {
            	dp.ActivateKeyboardHandler();
            }
		}
		public override void NegociateDevice()
		{
			if(!m_bIsConnected)
			{
				log.Debug("Trying to connect to a Capture source.");
				
				m_iConnectionsAttempts++;
	        	
				// TODO: Detect if a device is already in use 
				// (by an other app or even just by the other screen).
	        	List<DeviceDescriptor> devices = ListDevices();
	        	if(devices.Count > 0)
	        	{
	        		DeviceDescriptor device = devices[0];
	        		if(!device.Empty)
					{
						if(ConnectToDevice(device))
						{
							m_Container.Connected();
						}
					}
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
			bool stayConnected = m_CurrentVideoDevice.Empty || m_CurrentVideoDevice.Network;
			if(!stayConnected && m_iConnectionsWithoutFrames < 2)
			{
				if(m_bIsGrabbing && m_iGrabbedSinceLastCheck == 0)
				{
					log.DebugFormat("{0} has been disconnected.", m_CurrentVideoDevice.Name);
					
					// Close properly.
					m_VideoSource.SignalToStop();
					m_VideoSource.WaitForStop();
					
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
			if(m_bIsConnected && m_VideoSource != null)
			{
				if(!m_bIsGrabbing)
				{
					m_VideoSource.Start();
				}
				
				m_bIsGrabbing = true;
				log.DebugFormat("Starting to grab frames from {0}.", m_CurrentVideoDevice.Name);
			}
		}
		public override void PauseGrabbing()
		{
			if(m_VideoSource != null)
			{
				log.Debug("Pausing frame grabbing.");
				
				if(m_bIsConnected && m_bIsGrabbing)
				{
					m_VideoSource.Stop();
				}
				
				m_bIsGrabbing = false;
				m_iGrabbedSinceLastCheck = 0;
			}
		}
		public override void BeforeClose()
		{
			Disconnect();
		}
		public void DisplayDevicePropertyPage(IntPtr _handle)
		{
			VideoCaptureDevice device = m_VideoSource as VideoCaptureDevice;
			if(device != null)
			{
				try
				{
					device.DisplayPropertyPage(_handle);
				}
				catch(Exception)
				{
					log.ErrorFormat("Error when trying to display device property page.");
				}	
			}
		}
		#endregion
		
		#region Private methods
		private List<DeviceDescriptor> ListDevices()
		{
			// List all the devices currently connected (+ special entries).
			List<DeviceDescriptor> devices = new List<DeviceDescriptor>();

			// Capture devices
			FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			foreach(FilterInfo fi in videoDevices)
			{
				devices.Add(new DeviceDescriptor(fi.Name, fi.MonikerString));				
			}
			
			if(devices.Count == 0)
			{
				// Special entry if no Directshow camera found.
				// We add this one so the network camera doesn't get connected by default.
				devices.Add(new DeviceDescriptor(ScreenManagerLang.Capture_CameraNotFound));
			}
			
			// Special entry for network cameras.
			devices.Add(new DeviceDescriptor(ScreenManagerLang.Capture_NetworkCamera, m_PrefsManager.NetworkCameraUrl, m_PrefsManager.NetworkCameraFormat));

			return devices;
		}
		private bool ConnectToDevice(DeviceDescriptor _device)
		{
			log.DebugFormat("Connecting to {0}", _device.Name);
			
			Disconnect();
			bool created = false;
			if(_device.Network)
			{
				// Network Camera. Connect to last used url.
				// The user will have to open the dialog again if parameters have changed or aren't good.
				if(_device.NetworkCameraFormat == NetworkCameraFormat.JPEG)
				{
					m_VideoSource = new JPEGStream(_device.NetworkCameraUrl);
				}
				else
				{
					m_VideoSource = new MJPEGStream(_device.NetworkCameraUrl);
				}
				m_PrefsManager.NetworkCameraFormat = _device.NetworkCameraFormat;
				m_PrefsManager.NetworkCameraUrl = _device.NetworkCameraUrl;
				m_PrefsManager.Export();
				
				created = true;
			}
			else
			{
				m_VideoSource = new VideoCaptureDevice(_device.Identification);
				VideoCaptureDevice captureDevice = m_VideoSource as VideoCaptureDevice;
				if(captureDevice != null)
				{
					if((captureDevice.VideoCapabilities != null) && (captureDevice.VideoCapabilities.Length > 0))
					{
						// Import the capabilities of the device.
						foreach(VideoCapabilities vc in captureDevice.VideoCapabilities)
						{
							DeviceCapability dc = new DeviceCapability(vc.FrameSize, vc.FrameRate);
							_device.Capabilities.Add(dc);
							
							log.Debug(String.Format("Device Capability. {0}", dc.ToString()));
						}
						
						DeviceCapability selectedCapability = null;
						
						// Check if we already know this device and have a preferred configuration.
						foreach(DeviceConfiguration conf in m_PrefsManager.DeviceConfigurations)
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
							m_PrefsManager.UpdateSelectedCapability(_device.Identification, selectedCapability);
						}
						
						_device.SelectedCapability = selectedCapability;
						captureDevice.DesiredFrameSize = selectedCapability.FrameSize;
						captureDevice.DesiredFrameRate = selectedCapability.Framerate;
						m_FrameSize = selectedCapability.FrameSize;
						m_FramesInterval = 1000 / (double)selectedCapability.Framerate;
					}
					else
					{
						captureDevice.DesiredFrameRate = 0;
					}
					
					created = true;
				}
			}
			
			if(created)
			{
				m_CurrentVideoDevice = _device;
				m_VideoSource.NewFrame += new NewFrameEventHandler( VideoDevice_NewFrame );
				m_VideoSource.VideoSourceError += new VideoSourceErrorEventHandler( VideoDevice_VideoSourceError );
				m_bIsConnected = true;
			}
			else
			{
				log.Error("Couldn't create the capture device.");
			}
			
			return created;
		}
		private void Disconnect()
		{
			// The screen is about to be closed, release resources.
			
			if(m_bIsConnected && m_VideoSource != null)
			{
				log.DebugFormat("disconnecting from {0}", m_CurrentVideoDevice.Name);
				
				// Reset
				m_bIsGrabbing = false;
				m_bSizeKnown = false;
				m_iConnectionsAttempts = 0;
				m_Container.SetImageSize(Size.Empty);
				
				m_VideoSource.Stop();
				m_VideoSource.NewFrame -= new NewFrameEventHandler( VideoDevice_NewFrame );
				m_VideoSource.VideoSourceError -= new VideoSourceErrorEventHandler( VideoDevice_VideoSourceError );
				
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
				log.DebugFormat("Device infos : {0}. Received frame : {1}", m_FrameSize, sz);
				
				// Update the "official" size (used for saving context.)
				m_FrameSize = sz;
				
				if(m_CurrentVideoDevice.Network)
				{
					// This source is now officially working. Save the parameters to prefs.
					m_PrefsManager.NetworkCameraUrl = m_CurrentVideoDevice.NetworkCameraUrl;
					m_PrefsManager.NetworkCameraFormat = m_CurrentVideoDevice.NetworkCameraFormat;
					m_PrefsManager.AddRecentCamera(m_CurrentVideoDevice.NetworkCameraUrl);
					m_PrefsManager.Export();
				}
			}
			
			m_iConnectionsWithoutFrames = 0;
			m_iGrabbedSinceLastCheck++;
			m_FrameBuffer.Write(eventArgs.Frame);
			m_Container.FrameGrabbed();
		}
		private void VideoDevice_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
		{
			log.ErrorFormat("Error happened to {0}.", m_CurrentVideoDevice.Name);
		}
		#endregion
	}
}
