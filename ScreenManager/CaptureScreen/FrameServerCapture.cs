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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
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

		#endregion
		
		#region Members
		private IFrameServerCaptureContainer m_Container;	// CaptureScreenUserInterface seen through a limited interface.
		
		// Grabbing frames
		private AbstractFrameGrabber m_FrameGrabber;
		private FrameBuffer m_FrameBuffer = new FrameBuffer();
		private Bitmap m_MostRecentImage;
		private Size m_ImageSize = new Size(720, 576);		
		
		// Image, drawings and other screens overlays.
		private bool m_bPainting;									// 'true' between paint requests.
		private Metadata m_Metadata;
		private Magnifier m_Magnifier = new Magnifier();
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
		
		// Saving to disk
		private bool m_bIsRecording;
		private VideoFileWriter m_VideoFileWriter = new VideoFileWriter();
		
		// Captured video thumbnails.
		private string m_CurrentCaptureFilePath;
		private List<CapturedVideo> m_RecentlyCapturedVideos = new List<CapturedVideo>();
		private bool m_bCaptureThumbSet;
		private Bitmap m_CurrentCaptureBitmap;
		
		/*
		private int m_iDelayFrames = 0;							// Delay between what is captured and what is seen on screen.
		*/
		
		// General
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructor
		public FrameServerCapture()
		{
			m_FrameGrabber = new FrameGrabberAForge(this, m_FrameBuffer);
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
			//----------------------------------------------
			// NOTE : This method is in the GRABBING thread,
			// NO UI calls can be made directly from here.
			// must use BeginInvoke at some point.
			//----------------------------------------------
			
			// The frame grabber has just pushed a new frame to the buffer.
			
			// Consolidate this real-time frame locally.
			m_MostRecentImage = m_FrameBuffer.ReadFrameAt(0);
			
			// Ask a refresh. This could also be done with a timer,
			// but using the frame grabber event is convenient.
			if(!m_bPainting)
			{
				m_bPainting = true;
				m_Container.DoInvalidate();
			}
			
			// We also use this event to commit frame to disk during saving.
			// However, it is what is drawn on screen that will be pushed to the file,
			// not the frame the device just grabbed.
			if(m_bIsRecording)
			{
				// Is it necessary to make another copy of the frame ?
				Bitmap bmp = GetFlushedImage();
				m_VideoFileWriter.SaveFrame(bmp);
				
				if(!m_bCaptureThumbSet)
				{
					m_CurrentCaptureBitmap = bmp;
					m_bCaptureThumbSet = true;
				}
				else
				{
					bmp.Dispose();
				}
			}
		}
		public void SetImageSize(Size _size)
		{
			// This method is still in the grabbing thread. 
			// (NO UI calls, must use BeginInvoke).
			
			if(_size != Size.Empty)
			{
				log.Debug(String.Format("Image size specified. ({0})", _size.ToString()));
				m_ImageSize = new Size(_size.Width, _size.Height);
				m_CoordinateSystem.SetOriginalSize(m_ImageSize);
				m_Container.DoInitDecodingSize();
			}
			else
			{
				m_ImageSize = new Size(720, 576);	
			}
		}
		public void AlertCannotConnect()
		{
			// Couldn't find device. Signal to user.
			MessageBox.Show(
        		"Couldn't find any device to connect to.\nPlease make sure the device is properly connected.",
               	"Cannot connect to video device",
               	MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
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
		public void StartGrabbing()
		{
			//m_FrameBuffer.Clear();
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
				Bitmap image = GetImageToDisplay();
				
				if(image != null)
				{
					try
					{
						Size outputSize = new Size((int)_canvas.ClipBounds.Width, (int)_canvas.ClipBounds.Height);
						FlushOnGraphics(image, _canvas, outputSize);
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
			Bitmap source = GetImageToDisplay();	
			Bitmap output = new Bitmap(m_ImageSize.Width, m_ImageSize.Height, PixelFormat.Format24bppRgb);
			output.SetResolution(source.HorizontalResolution, source.VerticalResolution);
			FlushOnGraphics(source, Graphics.FromImage(output), output.Size);
			return output;
		}
		public void StartRecording(string filepath)
		{
			// Start recording.
			// We always record what is displayed on screen, not what is grabbed by the device.
			
			// Restart capturing if needed.
			if(!m_FrameGrabber.IsGrabbing)
			{
				m_FrameGrabber.StartGrabbing();
			}
			
			// Open a recording context.
			SaveResult result = m_VideoFileWriter.OpenSavingContext(filepath, null, -1, false);
			
			if(result == SaveResult.Success)
			{
				// The frames will be pushed to the file upon receiving the FrameGrabbed event.
				m_bCaptureThumbSet = false;
				m_bIsRecording = true;
			}
			else
			{
				m_VideoFileWriter.CloseSavingContext(false);
				m_bIsRecording = false;	
				DisplayError(result);
			}
		}
		public void StopRecording()
		{
			// Stop recording
			m_bIsRecording = false;
			
			// Close the recording context.
			m_VideoFileWriter.CloseSavingContext(true);
						
			//----------------------------------------------------------------------------
			// Add a VideofileBox (in the Keyframes panel) with a thumbnail of this video.
			// As for KeyframeBox, you'd be able to edit the filename.
			// double click = open it in a Playback screen.
			// time label would be the duration.
			// using the close button do not delete the file, it just hides it.
			//----------------------------------------------------------------------------
			CapturedVideo cv = new CapturedVideo(m_CurrentCaptureFilePath, m_CurrentCaptureBitmap);
			m_RecentlyCapturedVideos.Add(cv);
			m_CurrentCaptureBitmap.Dispose();
			m_Container.DoUpdateCapturedVideos();
		}
		#endregion
		
		#region Final image creation
		private Bitmap GetImageToDisplay()
		{
			// Get the right image to display, according to delay.
			// Drawings will then be drawn as overlays on this image.
			return m_MostRecentImage;
		}
		private void FlushOnGraphics(Bitmap _image, Graphics _canvas, Size _outputSize)
		{
			// Configure canvas.
			_canvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			_canvas.CompositingQuality = CompositingQuality.HighSpeed;
			_canvas.InterpolationMode = InterpolationMode.Bilinear;
			_canvas.SmoothingMode = SmoothingMode.None;
			
			// Draw image.
			Rectangle rDst;
			/*if(m_FrameServer.Metadata.Mirrored)
			{
				rDst = new Rectangle(_iNewSize.Width, 0, -_iNewSize.Width, _iNewSize.Height);
			}
			else
			{
				rDst = new Rectangle(0, 0, _iNewSize.Width, _iNewSize.Height);
			}*/
			
			rDst = new Rectangle(0, 0, _outputSize.Width, _outputSize.Height);
			
			RectangleF rSrc;
			if (m_CoordinateSystem.Zooming)
			{
				rSrc = m_CoordinateSystem.ZoomWindow;
			}
			else
			{
				rSrc = new Rectangle(0, 0, m_ImageSize.Width, m_ImageSize.Height);
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
			// In capture mode, all drawings are gathered in a virtual key image at m_Metadata[0].
			
			_canvas.SmoothingMode = SmoothingMode.AntiAlias;

			// 1. 2D Grid
			if (m_Metadata.Grid.Visible)
			{
				m_Metadata.Grid.Draw(_canvas, m_CoordinateSystem.Stretch * m_CoordinateSystem.Zoom, m_CoordinateSystem.Location);
			}

			// 2. 3D Plane
			if (m_Metadata.Plane.Visible)
			{
				m_Metadata.Plane.Draw(_canvas, m_CoordinateSystem.Stretch * m_CoordinateSystem.Zoom, m_CoordinateSystem.Location);
			}

			// Draw all drawings in reverse order to get first object on the top of Z-order.
			for (int i = m_Metadata[0].Drawings.Count - 1; i >= 0; i--)
			{
				bool bSelected = (i == m_Metadata.SelectedDrawing);
				m_Metadata[0].Drawings[i].Draw(_canvas, m_CoordinateSystem.Stretch * m_CoordinateSystem.Zoom, bSelected, 0, m_CoordinateSystem.Location);
			}
		}
		#endregion
		
		#region Saving to disk
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
	}
}
