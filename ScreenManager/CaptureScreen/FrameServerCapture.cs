#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameServerCapture encapsulate all the metadata and configuration for managing frames in a capture screen.
	/// </summary>
	public class FrameServerCapture : AbstractFrameServer
	{
		#region Properties
		public bool IsRunning
		{
			get {return m_VideoDevice.IsRunning;}
		}
		public bool IsRecording
		{
			get {return m_bIsRecording;}
		}
		public Size DecodingSize
		{
			get { return m_DecodingSize; }
		}
		#endregion
		
		#region Members
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private DelegateInvalidate m_DoInvalidate;					// To request a paint from screen.
		private int m_iDelayFrames = 0;							// Delay between what is captured and what is seen on screen.
		private List<Bitmap> m_FrameBuffer = new List<Bitmap>();	// Input buffer.
		private int m_iMaxSizeBufferFrames = 125;					// Input buffer size.
		private Size m_DecodingSize = new Size(720, 576);			// Default image size.
		private VideoCaptureDevice m_VideoDevice;					// Gives access to the physical device.
		private bool m_bPainting;									// 'true' between paint requests.
		private Stopwatch m_BufferWatch = new Stopwatch();		// For instrumentation only.
		private bool m_bIsRecording;
		private PlayerServer m_PlayerServer = new PlayerServer();
		#endregion
		
		#region Constructor
		public FrameServerCapture(DelegateInvalidate _delegate)
		{
			m_DoInvalidate = _delegate;	
		}
		#endregion
		
		#region Public methods
		public override void Draw(Graphics _canvas)
		{
			// Draw the current image on canvas according to conf.
			// This is called back from screen paint method.

			if(m_FrameBuffer != null && m_FrameBuffer.Count > 0)
			{
				int iCurrentFrameIndex = (m_FrameBuffer.Count - 1) - m_iDelayFrames;
				if (iCurrentFrameIndex < 0)
				{
					iCurrentFrameIndex = 0;
				}
				
				// Configure canvas.
				_canvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				_canvas.CompositingQuality = CompositingQuality.HighSpeed;
				_canvas.InterpolationMode = InterpolationMode.Bilinear;
				_canvas.SmoothingMode = SmoothingMode.None;
				
				try
				{
					// Draw delayed image.
					_canvas.DrawImage(m_FrameBuffer[iCurrentFrameIndex], _canvas.ClipBounds);
					
					// Ask LivePreview to draw itself
					if(m_iDelayFrames > 0)
					{
						Rectangle rSrc = new Rectangle(0, 0, m_FrameBuffer[m_FrameBuffer.Count-1].Width, m_FrameBuffer[m_FrameBuffer.Count-1].Height);
						Rectangle rDst = new Rectangle((int)_canvas.ClipBounds.Width/2, 0, (int)_canvas.ClipBounds.Width/2, (int)_canvas.ClipBounds.Height/2);
						_canvas.DrawImage(m_FrameBuffer[m_FrameBuffer.Count-1], rDst);
					}
				}
				catch (Exception exp)
				{
					log.Error("Unknown error while painting image.");
					log.Error(exp.StackTrace);
				}				
			}
			
			m_bPainting = false;
		}
		public void SetDevice(FilterInfoCollection _devices, int _iSelected)
		{
			m_VideoDevice = new VideoCaptureDevice( _devices[_iSelected].MonikerString );
			m_VideoDevice.NewFrame += new NewFrameEventHandler( VideoDevice_NewFrame );
				
			// use default frame rate from device.
			m_VideoDevice.DesiredFrameRate = 0;
				
			log.Debug(String.Format("Video Device : MonikerString:{0}, Name:{1}",_devices[_iSelected].MonikerString, _devices[_iSelected].Name));
		}
		public void TogglePlay()
		{
			if(m_VideoDevice.IsRunning)
			{
				SignalToStop();
			}
			else
			{
				SignalToStart();
			}
		}
		public void SignalToStop()
		{
			log.Debug("Stopping capture.");
			m_VideoDevice.SignalToStop();
		}
		public void ToggleRecord()
		{
			if(m_bIsRecording)
			{
				// Stop recording
				m_bIsRecording = false;
			}
			else
			{
				if(!m_VideoDevice.IsRunning)
				{
					SignalToStart();
				}
				
				m_bIsRecording = true;
				
				// Ask ffmpegHelper to open a recording context.
				// (on which file name ?)
				
			}
		}
		#endregion
		
		#region Private Methods
		/// <summary>
		/// VideoDevice_NewFrame. Callback method of the Video Device. 
		/// Called when a new frame is made available by the driver.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// Note: We are still in the worker thread. Don't touch UI directly.
			
			// Store the new frame in the buffer.
			m_FrameBuffer.Add((Bitmap)eventArgs.Frame.Clone());
			
			// Rotate the buffer, removing the oldest image.
			if(m_FrameBuffer.Count > m_iMaxSizeBufferFrames)
			{
				m_FrameBuffer.RemoveAt(0);
			}
		
			// If first frame, set up the size of video.
			if(m_FrameBuffer.Count == 1)
    		{
				m_DecodingSize = new Size(m_FrameBuffer[0].Width, m_FrameBuffer[0].Height);
    		}

			//If recording, append the new frame to file.
			if(m_bIsRecording)
			{
				
			}
			
			#region Instrumentation
			//log.Debug(String.Format("Bufferization:{0} FPS", 1000/m_BufferWatch.ElapsedMilliseconds));
	    	//m_BufferWatch.Reset();
	    	//m_BufferWatch.Start();
			#endregion
	    	
			// Display the frame if possible.
			if(!m_bPainting)
			{
				m_bPainting = true;
	    		m_DoInvalidate();
			}
		}
		private void SignalToStart()
		{
			m_FrameBuffer.Clear();
			m_BufferWatch.Start();
			m_VideoDevice.Start();
		}
		#endregion
	}
}
