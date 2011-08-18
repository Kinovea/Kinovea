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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameServerCapture encapsulates all the metadata and configuration for managing frames in a capture screen.
	/// This is the object that maintains the interface with file level operations done by VideoFile class.
	/// </summary>
	public class FrameServerCapture : AbstractFrameServer, IFrameGrabberContainer
	{
		#region Properties
		public string Status
		{
			get
			{
				if(m_FrameGrabber.IsConnected)
				{
					string bufferFill = String.Format(ScreenManagerLang.statusBufferFill, m_FrameBuffer.FillPercentage);
					string status = String.Format("{0} - {1} ({2})",
					                              m_FrameGrabber.DeviceName, 
					                              m_FrameGrabber.SelectedCapability.ToString(), 
					                              bufferFill);
					return status;		
				}
				else
				{
					return ScreenManagerLang.statusEmptyScreen;	
				}
			}
		}
		// Capture device.
		public bool IsConnected
		{
			get { return m_FrameGrabber.IsConnected; }
		}
		public bool IsGrabbing
		{
			get {return m_FrameGrabber.IsGrabbing;}
		}
		public Size ImageSize
		{
			get { return m_ImageSize; }
		}
		public bool IsRecording
		{
			get {return m_bIsRecording;}
		}
		public string DeviceName
		{
			get { return m_FrameGrabber.DeviceName; }
		}
		public bool Shared
		{
			set {m_bShared = value;}
		}
		
		// Image, Drawings and other screens overlays.
		public Metadata Metadata
		{
			get { return m_Metadata; }
			set { m_Metadata = value; }
		}
		public Magnifier Magnifier
		{
			get { return m_Magnifier; }
			set { m_Magnifier = value; }
		}
		public CoordinateSystem CoordinateSystem
		{
			get { return m_CoordinateSystem; }
		}
		
		// Saving to disk.
		public List<CapturedVideo> RecentlyCapturedVideos
		{
			get { return m_RecentlyCapturedVideos; }	
		}
		public string CurrentCaptureFilePath
		{
			get { return m_CurrentCaptureFilePath; }
			set { m_CurrentCaptureFilePath = value; }
		}
		public ImageAspectRatio AspectRatio
		{
			get { return m_AspectRatio; }
			set 
			{ 
				SetAspectRatio(value, m_FrameGrabber.FrameSize);
			}
		}
		#endregion
		
		#region Members
		private IFrameServerCaptureContainer m_Container;	// CaptureScreenUserInterface seen through a limited interface.
		
		// Threading
		private Control m_DummyControl = new Control();
		private readonly object m_Locker = new object();
		private event EventHandler m_EventFrameGrabbed;
		
		// Grabbing frames
		private AbstractFrameGrabber m_FrameGrabber;
		private FrameBuffer m_FrameBuffer = new FrameBuffer();
		private Bitmap m_ImageToDisplay;
		private Size m_ImageSize = new Size(720, 576);		
		private ImageAspectRatio m_AspectRatio = ImageAspectRatio.Auto;
		private int m_iFrameIndex;		// The "age" we pull from, in the circular buffer.
		private int m_iCurrentBufferFill;
		
		// Image, drawings and other screens overlays.
		private bool m_bPainting;									// 'true' between paint requests.
		private Metadata m_Metadata;
		private Magnifier m_Magnifier = new Magnifier();
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
		
		// Saving to disk
		private bool m_bIsRecording;
		private VideoRecorder m_VideoRecorder;
		
		// Captured video thumbnails.
		private string m_CurrentCaptureFilePath;
		private List<CapturedVideo> m_RecentlyCapturedVideos = new List<CapturedVideo>();
		
		
		// General
		private Stopwatch m_Stopwatch = new Stopwatch();
		private bool m_bShared;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FrameServerCapture()
		{
			m_FrameGrabber = new FrameGrabberAForge(this, m_FrameBuffer);
			m_AspectRatio = PreferencesManager.Instance().AspectRatio;
			
			IntPtr forceHandleCreation = m_DummyControl.Handle; // Needed to show that the main thread "owns" this Control.
			m_EventFrameGrabbed = FrameGrabbed_Invoked;
		}
		#endregion
		
		#region Implementation of IFrameGrabberContainer
		public void Connected()
		{
			log.Debug("Screen connected.");
			StartGrabbing();
		}
		public void FrameGrabbed()
		{
			// We are still in the grabbing thread. 
			// We must return as fast as possible to avoid slowing down the grabbing.
			// We use a Control object to merge back into the main thread, we'll do the work there.
			m_DummyControl.BeginInvoke(m_EventFrameGrabbed);
		}
		public void SetImageSize(Size _size)
		{
			// This method is still in the grabbing thread. 
			// (NO UI calls, must use BeginInvoke).
			if(_size != Size.Empty)
			{
				SetAspectRatio(m_AspectRatio, _size);
				log.Debug(String.Format("Image size specified. {0}", m_ImageSize));				
			}
			else
			{
				m_ImageToDisplay = null;
				m_ImageSize = new Size(720, 576);
				m_FrameBuffer.UpdateFrameSize(m_ImageSize);
			}
		}	
		#endregion
		
		#region Public methods
		public void SetContainer(IFrameServerCaptureContainer _container)
		{
			m_Container = _container;
		}
		public void PromptDeviceSelector()
		{
			m_FrameGrabber.PromptDeviceSelector();
		}
		public void NegociateDevice()
		{
			m_FrameGrabber.NegociateDevice();
		}
		public void HeartBeat()
		{
			// Heartbeat called regularly by the UI to ensure the grabber is still alive.
			
			// This runs on the UI thread and is not accurate.
			// Do not use it for measures needing accuracy, like framerate estimation.
			
			m_FrameGrabber.CheckDeviceConnection();
		}
		public void StartGrabbing()
		{
			m_FrameGrabber.StartGrabbing();
			m_Container.DisplayAsGrabbing(true);
		}
		public void PauseGrabbing()
		{
			m_FrameGrabber.PauseGrabbing();
			m_Container.DisplayAsGrabbing(false);
		}
		public void BeforeClose()
		{
			if(m_bIsRecording)
			{
				StopRecording();
			}
			m_FrameGrabber.BeforeClose();
		}
		public override void Draw(Graphics _canvas)
		{
			// Draw the current image on canvas according to conf.
			// This is called back from UI paint method.
			if(m_FrameGrabber.IsConnected)
			{
				if(m_ImageToDisplay != null)
				{
					try
					{
						Size outputSize = new Size((int)_canvas.ClipBounds.Width, (int)_canvas.ClipBounds.Height);
						FlushOnGraphics(m_ImageToDisplay, _canvas, outputSize);
					}
					catch (Exception exp)
					{
						log.Error("Error while painting image.");
						log.Error(exp.Message);
						log.Error(exp.StackTrace);
					}
				}
			}
			
			m_bPainting = false;
		}
		public Bitmap GetFlushedImage()
		{
			// Returns a standalone image with all drawings flushed.
			// This can be used by snapshot or movie saving.
			// We don't use the screen size, but the original video size (differs from PlayerScreen.)
			// This always represents the image that is drawn on screen, not the last image grabbed by the device.
			Bitmap output = new Bitmap(m_ImageSize.Width, m_ImageSize.Height, PixelFormat.Format24bppRgb);
			
			try
			{
				if(m_ImageToDisplay != null)
				{
					output.SetResolution(m_ImageToDisplay.HorizontalResolution, m_ImageToDisplay.VerticalResolution);
					FlushOnGraphics(m_ImageToDisplay, Graphics.FromImage(output), output.Size);
				}	
			}
			catch(Exception)
			{
				log.ErrorFormat("Exception while trying to get flushed image. Returning blank image.");
			}
			
			
			return output;
		}
		public bool StartRecording(string _filepath)
		{
			bool bRecordingStarted = false;
			log.Debug("Start recording images to file.");
			
			// Restart capturing if needed.
			if(!m_FrameGrabber.IsGrabbing)
			{
				m_FrameGrabber.StartGrabbing();
			}
			
			// Prepare the recorder
			m_VideoRecorder = new VideoRecorder();
			double interval = (m_FrameGrabber.FramesInterval > 0) ? m_FrameGrabber.FramesInterval : 40;
			SaveResult result = m_VideoRecorder.Initialize(_filepath, interval, m_FrameGrabber.FrameSize);
			
			if(result == SaveResult.Success)
			{
				// The frames will be pushed to the file upon receiving the FrameGrabbed event.
				m_bIsRecording = true;
				bRecordingStarted = true;
			}
			else
			{
				m_bIsRecording = false;
				DisplayError(result);
			}
			
			return bRecordingStarted;
		}
		public void StopRecording()
		{
			m_bIsRecording = false;
			log.Debug("Stop recording images to file.");
			
			if(m_VideoRecorder != null)
			{
				// Add a VideofileBox with a thumbnail of this video.
				if(m_VideoRecorder.CaptureThumb != null)
				{
					CapturedVideo cv = new CapturedVideo(m_CurrentCaptureFilePath, m_VideoRecorder.CaptureThumb);
					m_RecentlyCapturedVideos.Add(cv);
					m_VideoRecorder.CaptureThumb.Dispose();
					m_Container.DoUpdateCapturedVideos();
				}
			
				// Terminate the recording thread and release resources. This will treat any outstanding frames in the queue.
				m_VideoRecorder.Dispose();
			}

			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			
			// Ask the Explorer tree to refresh itself, (but not the thumbnails pane.)
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.RefreshFileExplorer != null)
            {
                dp.RefreshFileExplorer(false);
            }
		}
		public int DelayChanged(int percentage)
		{
			// Set the new delay, and give back the value in seconds.
			// The value given back is just an integer, not a double, because we don't have that much precision.
			// The frame rate is roughly estimated from frame received by seconds,
			// and there is a latency inherent to the camcorder that we can't know.
			m_iFrameIndex = (int)(((double)m_FrameBuffer.Capacity / 100.0) * percentage);
			
			double interval = (m_FrameGrabber.FramesInterval > 0)?(double)m_FrameGrabber.FramesInterval:40.0;
			int delay = (int)(((double)m_iFrameIndex * interval) / 1000);
			
			// Re-adjust frame for the special case of no delay at all.
			// (it's not always easy to drag all the way left to the real 0 spot).
			if(delay < 1)
				m_iFrameIndex = 0;
			
			// Explicitely call the refresh if we are not currently grabbing.
			if(!m_FrameGrabber.IsGrabbing)
			{
				m_ImageToDisplay = m_FrameBuffer.ReadAt(m_iFrameIndex);
				if(!m_bPainting)
				{
					m_bPainting = true;
					m_Container.DoInvalidate();
				}
			}
			
			return delay;
		}
		public void UpdateMemoryCapacity()
		{
			m_FrameBuffer.UpdateMemoryCapacity(m_bShared);
		}
		#endregion
		
		#region Final image creation
		private void FlushOnGraphics(Bitmap _image, Graphics _canvas, Size _outputSize)
		{
			// Configure canvas.
			_canvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			_canvas.CompositingQuality = CompositingQuality.HighSpeed;
			_canvas.InterpolationMode = InterpolationMode.Bilinear;
			_canvas.SmoothingMode = SmoothingMode.None;
			
			// Draw image.
			Rectangle rDst;			
			rDst = new Rectangle(0, 0, _outputSize.Width, _outputSize.Height);
			
			RectangleF rSrc;
			if (m_CoordinateSystem.Zooming)
			{
				rSrc = m_CoordinateSystem.ZoomWindow;
			}
			else
			{
				rSrc = new Rectangle(0, 0, _image.Width, _image.Height);
			}
			
			_canvas.DrawImage(_image, rDst, rSrc, GraphicsUnit.Pixel);
			
			FlushDrawingsOnGraphics(_canvas);	
			
			// .Magnifier
			// TODO: handle miroring.
			if (m_Magnifier.Mode != MagnifierMode.NotVisible)
			{
				m_Magnifier.Draw(_image, _canvas, 1.0, false);
			}
		}
		private void FlushDrawingsOnGraphics(Graphics _canvas)
		{
			// Commit drawings on image.
			_canvas.SmoothingMode = SmoothingMode.AntiAlias;

			foreach(AbstractDrawing ad in m_Metadata.ExtraDrawings)
			{
                ad.Draw(_canvas, m_CoordinateSystem, false, 0);
			}
			
			// In capture mode, all drawings are gathered in a virtual key image at m_Metadata[0].
			// Draw all drawings in reverse order to get first object on the top of Z-order.
			for (int i = m_Metadata[0].Drawings.Count - 1; i >= 0; i--)
			{
				bool bSelected = (i == m_Metadata.SelectedDrawing);
                m_Metadata[0].Drawings[i].Draw(_canvas, m_CoordinateSystem, bSelected, 0);
			}
		}
		#endregion
		
		#region Error messages
		public void AlertCannotConnect()
		{
			// Couldn't find device. Signal to user.
			MessageBox.Show(
				ScreenManagerLang.Error_Capture_CannotConnect_Text.Replace("\\n", "\n"),
               	ScreenManagerLang.Error_Capture_CannotConnect_Title,
               	MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
		}
		public void AlertConnectionLost()
		{
			// Device stopped sending frames.
			m_Container.AlertDisconnected();
		}
		private void DisplayError(SaveResult _result)
		{
			switch(_result)
        	{
                case SaveResult.FileHeaderNotWritten:
                case SaveResult.FileNotOpened:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_FileError);
                    break;
                
                case SaveResult.EncoderNotFound:
                case SaveResult.EncoderNotOpened:
                case SaveResult.EncoderParametersNotAllocated:
                case SaveResult.EncoderParametersNotSet:
                case SaveResult.InputFrameNotAllocated:
                case SaveResult.MuxerNotFound:
                case SaveResult.MuxerParametersNotAllocated:
                case SaveResult.MuxerParametersNotSet:
                case SaveResult.VideoStreamNotCreated:
                case SaveResult.UnknownError:
                default:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_LowLevelError);
                    break;
        	}
		}
		private void DisplayErrorMessage(string _err)
        {
        	MessageBox.Show(
        		_err.Replace("\\n", "\n"),
               	ScreenManagerLang.Error_SaveMovie_Title,
               	MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
		#endregion
	
		#region Private Methods
		private void SetAspectRatio(ImageAspectRatio _aspectRatio, Size _size)
		{
			m_AspectRatio = _aspectRatio;
			
			if(m_FrameGrabber.IsConnected)
			{
				int newHeight;
				
				switch(_aspectRatio)
				{
					case ImageAspectRatio.Auto:
					default:
						newHeight = _size.Height;
						break;
					case ImageAspectRatio.Force43:
						newHeight = (_size.Width / 4) * 3;
						break;
					case ImageAspectRatio.Force169:
						newHeight = (_size.Width / 16) * 9;
						break;
				}
				
				m_ImageSize = new Size(_size.Width, newHeight);
				m_CoordinateSystem.SetOriginalSize(m_ImageSize);
				m_Container.DoInitDecodingSize();
				m_Metadata.ImageSize = m_ImageSize;
				m_FrameBuffer.UpdateFrameSize(m_ImageSize);
			}
		}
		private void FrameGrabbed_Invoked(object sender, EventArgs e)
		{
            // We are back in the Main thread.

			// Get the raw frame we will be displaying/saving.
			m_ImageToDisplay = m_FrameBuffer.ReadAt(m_iFrameIndex);
		
			if(m_bIsRecording && m_VideoRecorder != null && m_VideoRecorder.Initialized)
			{
				// The recorder runs in its own thread.
				// We need to make a full copy of the frame because m_ImageToDisplay may change before we actually save it.
				// TODO: drop mechanism in case the frame queue grows too big.
				if(!m_VideoRecorder.Cancelling)
				{
					Bitmap bmp = GetFlushedImage();
					m_VideoRecorder.EnqueueFrame(bmp);
				}
				else
				{
					StopRecording();
					m_Container.DisplayAsRecording(false);
					DisplayError(m_VideoRecorder.CancelReason);
				}
			}
			
			// Ask a refresh.
			if(!m_bPainting)
			{
				m_bPainting = true;
				m_Container.DoInvalidate();
			}
			
			// Update status bar if needed.
			if(m_iCurrentBufferFill != m_FrameBuffer.FillPercentage)
			{
				m_iCurrentBufferFill = m_FrameBuffer.FillPercentage;
				m_Container.DoUpdateStatusBar();
			}
		}
		#endregion
	}
}
