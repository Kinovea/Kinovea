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

#region Using directives
using Kinovea.ScreenManager.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.VideoFiles;
using System.Globalization;


#endregion

namespace Kinovea.ScreenManager
{
	public partial class CaptureScreenUserInterface : UserControl, IFrameServerContainer
	{
		#region Properties
		public int DrawtimeFilterType
		{
			get
			{
				if(m_bDrawtimeFiltered)
				{
					return m_DrawingFilterOutput.VideoFilterType;
				}
				else
				{
					return -1;
				}
			}
		}
		#endregion

		#region Members
		private ICaptureScreenUIHandler m_ScreenUIHandler;
		private FrameServerCapture m_FrameServer;
		
		// General
		private PreferencesManager m_PrefManager = PreferencesManager.Instance();
		private bool m_bIsIdle = true;
		private bool m_bTryingToConnect;

		// Image
		private bool m_bStretchModeOn;			// This is just a toggle to know what to do on double click.
		private bool m_bShowImageBorder;
		private static readonly Pen m_PenImageBorder = Pens.SteelBlue;
		
		// Keyframes, Drawings, etc.
		private DrawingToolType m_ActiveTool;
		private AbstractDrawingTool[] m_DrawingTools;
		private ColorProfile m_ColorProfile = new ColorProfile();
		private bool m_bDocked = true;
		private bool m_bTextEdit;
		private bool m_bMeasuring;
		//private Timer m_DeviceDetector = new Timer();

		// Video Filters Management
		private bool m_bDrawtimeFiltered;
		private DrawtimeFilterOutput m_DrawingFilterOutput;
        private String m_CurrentCaptureDevice = "";
        private DateTime m_CaptureDuration;
		
		#region Context Menus
		private ContextMenuStrip popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuSavePic = new ToolStripMenuItem();
		private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();

		private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
		private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
		private ToolStripSeparator mnuSepDrawing = new ToolStripSeparator();
		private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
		private ToolStripMenuItem mnuShowMeasure = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSealMeasure = new ToolStripMenuItem();
		
		private ContextMenuStrip popMenuMagnifier = new ContextMenuStrip();
		private ToolStripMenuItem mnuMagnifier150 = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifier175 = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifier200 = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifier225 = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifier250 = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifierDirect = new ToolStripMenuItem();
		private ToolStripMenuItem mnuMagnifierQuit = new ToolStripMenuItem();

		private ContextMenuStrip popMenuGrids = new ContextMenuStrip();
		private ToolStripMenuItem mnuGridsConfigure = new ToolStripMenuItem();
		private ToolStripMenuItem mnuGridsHide = new ToolStripMenuItem();
		#endregion

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor
		public CaptureScreenUserInterface(FrameServerCapture _FrameServer, ICaptureScreenUIHandler _screenUIHandler)
		{
			log.Debug("Constructing the CaptureScreen user interface.");
			m_ScreenUIHandler = _screenUIHandler;
			m_FrameServer = _FrameServer;
			m_FrameServer.SetContainer(this);
			m_FrameServer.Metadata = new Metadata(new GetTimeCode(TimeStampsToTimecode), null);

			// Initialize UI.
			InitializeComponent();
			this.Dock = DockStyle.Fill;
			ShowHideResizers(false);
			InitializeDrawingTools();
			InitializeMetadata();
            InitializeDeviceList(m_ScreenUIHandler.CaptureScreenUI_CaptureDevices());
			BuildContextMenus();
			m_bDocked = true;
			
			tmrCaptureDeviceDetector.Start();
		}
		#endregion
		
		#region IFrameServerContainer implementation
		public void DoInvalidate()
		{
			pbSurfaceScreen.Invalidate();
		}
		public void DoInitDecodingSize()
		{
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetImageSize(m_FrameServer.DecodingSize);
		}
		public void DoUpdateCapturedVideos()
		{
			// Update the list of Captured Videos.
			// Similar to OrganizeKeyframe in PlayerScreen.
			
			pnlThumbnails.Controls.Clear();
			
			if(m_FrameServer.RecentlyCapturedVideos.Count > 0)
			{
				int iBoxIndex = 0;
				int iPixelsOffset = 0;
				int iPixelsSpacing = 20;
				
				foreach (CapturedVideo cv in m_FrameServer.RecentlyCapturedVideos)
				{
					CapturedVideoBox box = new CapturedVideoBox(cv);
					SetupDefaultThumbBox(box);
					
					// Finish the setup
					box.Left = iPixelsOffset + iPixelsSpacing;
					box.pbThumbnail.Image = cv.Thumbnail;
					//box.CloseThumb += new KeyframeBox.CloseThumbHandler(ThumbBoxClose);
					//box.ClickThumb += new KeyframeBox.ClickThumbHandler(ThumbBoxClick);
					
					iPixelsOffset += (iPixelsSpacing + box.Width);

					pnlThumbnails.Controls.Add(box);

					iBoxIndex++;
				}
				
				UndockKeyframePanel();
				pnlThumbnails.Refresh();
			}
			else
			{
				DockKeyframePanel();
			}

		}
		#endregion
		
		#region Public Methods
		public void PostTryConnection()
		{
			if(m_FrameServer.IsConnected)
			{
				buttonPlay_Click(null, EventArgs.Empty);
			}
		}
		public void DisplayAsActiveScreen(bool _bActive)
		{
			// Called from ScreenManager.
			ShowBorder(_bActive);
		}
		public void RefreshUICulture()
		{
			ReloadTooltipsCulture();
			ReloadMenusCulture();
			// Refresh image to update grids colors, etc.
			pbSurfaceScreen.Invalidate();
		}
		public void SetDrawingtimeFilterOutput(DrawtimeFilterOutput _dfo)
		{
			// A video filter just finished and is passing us its output object.
			// It is used as a communication channel between the filter and the player.
			// Depending on the filter type, we may need to switch to a special mode,
			// keep track of old pre-filter parameters,
			// delegate the draw to the filter, etc...
			
			if(_dfo.Active)
			{
				m_bDrawtimeFiltered = true;
				m_DrawingFilterOutput = _dfo;
				
				// Disable playing and drawing.
				DisablePlayAndDraw();
				
				// Disable all player controls
				EnableDisableAllPlayingControls(false);
				EnableDisableDrawingTools(false);
				
				// TODO: memorize current state (keyframe docked) and recall it when quiting filtered mode.
				DockKeyframePanel();
				m_bStretchModeOn = true;
				StretchSqueezeSurface();
			}
			else
			{
				m_bDrawtimeFiltered = false;
				m_DrawingFilterOutput = null;

				EnableDisableAllPlayingControls(true);
				EnableDisableDrawingTools(true);
				
				// TODO:recall saved state.
			}
		}
		public bool OnKeyPress(Keys _keycode)
		{
			bool bWasHandled = false;
			
			// Method called from the Screen Manager's PreFilterMessage.
			switch (_keycode)
			{
                case Keys.Space:
                    {
                        OnRecord();
                        bWasHandled = true;
                        break;
                    }
				case Keys.Escape:
					{
						DisablePlayAndDraw();
						pbSurfaceScreen.Invalidate();
						bWasHandled = true;
						break;
					}
				
				case Keys.Add:
					{
						IncreaseDirectZoom();
						bWasHandled = true;
						break;
					}
				case Keys.Subtract:
					{
						// Decrease Zoom.
						DecreaseDirectZoom();
						bWasHandled = true;
						break;
					}
				case Keys.F11:
					{
						ToggleStretchMode();
						bWasHandled = true;
						break;
					}
				case Keys.Delete:
					{
						// Remove selected Drawing
						// Note: Should only work if the Drawing is currently being moved...
						DeleteSelectedDrawing();
						
						bWasHandled = true;
						break;
					}
				default:
					break;
			}

			return bWasHandled;
		}
		public void BeforeClose()
		{
			// This screen is about to be closed.
			tmrCaptureDeviceDetector.Stop();
			tmrCaptureDeviceDetector.Dispose();
		}
		#endregion
		
		#region Various Inits & Setups
		private void InitializeDrawingTools()
        {
			// Create Drawing Tools
			m_DrawingTools = new AbstractDrawingTool[(int)DrawingToolType.NumberOfDrawingTools];
			
			m_DrawingTools[(int)DrawingToolType.Pointer] = new DrawingToolPointer();
			m_DrawingTools[(int)DrawingToolType.Line2D] = new DrawingToolLine2D();
			m_DrawingTools[(int)DrawingToolType.Cross2D] = new DrawingToolCross2D();
			m_DrawingTools[(int)DrawingToolType.Angle2D] = new DrawingToolAngle2D();
			m_DrawingTools[(int)DrawingToolType.Pencil] = new DrawingToolPencil();
			m_DrawingTools[(int)DrawingToolType.Text] = new DrawingToolText();
			m_DrawingTools[(int)DrawingToolType.Chrono] = new DrawingToolChrono();
			
			m_ActiveTool = DrawingToolType.Pointer;
        }
		private void InitializeMetadata()
		{
			// In capture, there is always a single keyframe.
			// It is used to hold motion guides.
			Keyframe kf = new Keyframe(m_FrameServer.Metadata);
			kf.Position = 0;
			
			m_FrameServer.Metadata.Add(kf);
		}
        private void InitializeDeviceList(List<String> dev_list)
        {
            foreach(String dev in dev_list)
            {
                cmbDeviceList.Items.Add(dev);
            }
        }
		private void ResetData()
		{
			m_bStretchModeOn = false;
			m_bShowImageBorder = false;
			m_ActiveTool = DrawingToolType.Pointer;
			m_ColorProfile.Load(PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder") + "\\current.xml");
			
			m_bDocked = true;
			m_bTextEdit = false;
			m_bMeasuring = false;
			
			m_bDrawtimeFiltered = false;;
			m_FrameServer.CoordinateSystem.Reset();
			
			m_FrameServer.Magnifier.ResetData();	
		}
		private void ShowHideResizers(bool _bShow)
		{
			ImageResizerNE.Visible = _bShow;
			ImageResizerNW.Visible = _bShow;
			ImageResizerSE.Visible = _bShow;
			ImageResizerSW.Visible = _bShow;
		}
		private void BuildContextMenus()
		{
			// Attach the event handlers and build the menus.
			
			// 1. Default context menu.
			mnuSavePic.Click += new EventHandler(btnSnapShot_Click);
			mnuCloseScreen.Click += new EventHandler(btnClose_Click);
			popMenu.Items.AddRange(new ToolStripItem[] { mnuSavePic, new ToolStripSeparator(), mnuCloseScreen });

			// 2. Drawings context menu (Configure, Delete, Track this)
			mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);
			mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);
			mnuShowMeasure.Click += new EventHandler(mnuShowMeasure_Click);
			mnuSealMeasure.Click += new EventHandler(mnuSealMeasure_Click);
			popMenuDrawings.Items.AddRange(new ToolStripItem[] { mnuConfigureDrawing, new ToolStripSeparator(), mnuShowMeasure, mnuSealMeasure, mnuSepDrawing, mnuDeleteDrawing });

			// 5. Magnifier
			mnuMagnifier150.Click += new EventHandler(mnuMagnifier150_Click);
			mnuMagnifier175.Click += new EventHandler(mnuMagnifier175_Click);
			mnuMagnifier175.Checked = true;
			mnuMagnifier200.Click += new EventHandler(mnuMagnifier200_Click);
			mnuMagnifier225.Click += new EventHandler(mnuMagnifier225_Click);
			mnuMagnifier250.Click += new EventHandler(mnuMagnifier250_Click);
			mnuMagnifierDirect.Click += new EventHandler(mnuMagnifierDirect_Click);
			mnuMagnifierQuit.Click += new EventHandler(mnuMagnifierQuit_Click);
			popMenuMagnifier.Items.AddRange(new ToolStripItem[] { mnuMagnifier150, mnuMagnifier175, mnuMagnifier200, mnuMagnifier225, mnuMagnifier250, new ToolStripSeparator(), mnuMagnifierDirect, mnuMagnifierQuit });
			
			// 6. Grids
			mnuGridsConfigure.Click += new EventHandler(mnuGridsConfigure_Click);
			mnuGridsHide.Click += new EventHandler(mnuGridsHide_Click);
			popMenuGrids.Items.AddRange(new ToolStripItem[] { mnuGridsConfigure, mnuGridsHide });
			
			// Default :
			this.ContextMenuStrip = popMenu;
			
			// Load texts
			ReloadMenusCulture();
		}
		#endregion
		
		#region Misc Events
		private void btnClose_Click(object sender, EventArgs e)
		{
			// Propagate to PlayerScreen which will report to ScreenManager.
			m_ScreenUIHandler.ScreenUI_CloseAsked();
		}
		private void PanelVideoControls_MouseEnter(object sender, EventArgs e)
		{
			// Set focus to enable mouse scroll
			panelVideoControls.Focus();
		}
		#endregion
		
		#region Misc private helpers
		private void OnPoke()
		{
			//------------------------------------------------------------------------------
			// This function is a hub event handler for all button press, mouse clicks, etc.
			// Signal itself as the active screen to the ScreenManager
			//---------------------------------------------------------------------
			
			m_ScreenUIHandler.ScreenUI_SetAsActiveScreen();
			
			// 1. Ensure no DrawingText is in edit mode.
			//m_FrameServer.Metadata.AllDrawingTextToNormalMode();

			// 2. Return to the pointer tool, except if Pencil
			/*if (m_ActiveTool != DrawingToolType.Pencil)
			{
				m_ActiveTool = DrawingToolType.Pointer;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, -1));
			}

			// 3. Dock Keyf panel if nothing to see.
			//if (m_FrameServer.Metadata.Count < 1)
			//TODO other test.
			{
				DockKeyframePanel();
			}*/
		}
		private string TimeStampsToTimecode(long _iTimeStamp, TimeCodeFormat _timeCodeFormat, bool _bSynched)
		{
			return "todo";
			/*
			
			//-------------------------
			// Input    : TimeStamp (might be a duration. If starting ts isn't 0, it should already be shifted.)
			// Output   : time in a specific format
			//-------------------------

			TimeCodeFormat tcf;
			if (_timeCodeFormat == TimeCodeFormat.Unknown)
			{
				tcf = m_PrefManager.TimeCodeFormat;
			}
			else
			{
				tcf = _timeCodeFormat;
			}

			long iTimeStamp;
			if (_bSynched)
			{
				iTimeStamp = _iTimeStamp - m_iSyncPosition;
			}
			else
			{
				iTimeStamp = _iTimeStamp;
			}

			// timestamp to milliseconds. (Needed for most formats)
			double iSeconds;

			// TODO - other source for time ammount (time since capture started.)
			/*if (m_FrameServer.VideoFile.Loaded)
				iSeconds = (double)iTimeStamp / m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds;
			else* /
				iSeconds = 0;

			// m_fSlowFactor is different from 1.0f only when user specify that the capture fps
			// was different than the playing fps. We readjust time.
			double iMilliseconds = (iSeconds * 1000) / m_fHighSpeedFactor;
			
			// If there are more than 100 frames per seconds, we display milliseconds.
			// This can happen when the user manually tune the input fps.
			//bool bShowThousandth = (m_fHighSpeedFactor *  m_FrameServer.VideoFile.Infos.fFps >= 100);
			bool bShowThousandth;
			
			string outputTimeCode;
			switch (tcf)
			{
				case TimeCodeFormat.ClassicTime:
					outputTimeCode = TimeHelper.MillisecondsToTimecode((long)iMilliseconds, bShowThousandth);
					break;
				case TimeCodeFormat.Frames:
					if (m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame != 0)
					{
						outputTimeCode = String.Format("{0}", (int)((double)iTimeStamp / m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame) + 1);
					}
					else
					{
						outputTimeCode = String.Format("0");
					}
					break;
				case TimeCodeFormat.TenThousandthOfHours:
					// 1 Ten Thousandth of Hour = 360 ms.
					double fTth = ((double)iMilliseconds / 360.0);
					outputTimeCode = String.Format("{0}:{1:00}", (int)fTth, Math.Floor((fTth - (int)fTth)*100));
					break;
				case TimeCodeFormat.HundredthOfMinutes:
					// 1 Hundredth of minute = 600 ms.
					double fCtm = ((double)iMilliseconds / 600.0);
					outputTimeCode = String.Format("{0}:{1:00}", (int)fCtm, Math.Floor((fCtm - (int)fCtm) * 100));
					break;
				case TimeCodeFormat.TimeAndFrames:
					String timeString = TimeHelper.MillisecondsToTimecode((long)iMilliseconds, bShowThousandth);
					String frameString;
					if (m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame != 0)
					{
						frameString = String.Format("{0}", (int)((double)iTimeStamp / m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame) + 1);
					}
					else
					{
						frameString = String.Format("0");
					}
					outputTimeCode = String.Format("{0} ({1})", timeString, frameString);
					break;
				case TimeCodeFormat.Timestamps:
					outputTimeCode = String.Format("{0}", (int)iTimeStamp);
					break;
					default :
						outputTimeCode = TimeHelper.MillisecondsToTimecode((long)iMilliseconds, bShowThousandth);
					break;
			}

			return outputTimeCode;*/
		}
		private int TimeStampsToPixels(long _iValue, long _iOldMax, long _iNewMax)
		{
			// Rescaling général.
			return (int)((double)((double)_iValue * (double)_iNewMax) / (double)_iOldMax);
		}
		private int PixelsToTimeStamps(long _iValue, long _iOldMax, long _iNewMax)
		{
			return (int)(Math.Round((double)((double)_iValue * (double)_iNewMax) / (double)_iOldMax));
		}
		private int Rescale(long _iValue, long _iOldMax, long _iNewMax)
		{
			// Generic rescale.
			return (int)((double)((double)_iValue * (double)_iNewMax) / (double)_iOldMax);
		}
		private void DoDrawingUndrawn()
		{
			//--------------------------------------------------------
			// this function is called after we undo a drawing action.
			// Called from CommandAddDrawing.Unexecute() through a delegate.
			//--------------------------------------------------------

			// Return to the pointer tool unless we were drawing.
			if (m_ActiveTool != DrawingToolType.Pencil)
			{
				m_ActiveTool = DrawingToolType.Pointer;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			}
		}
		#endregion
		
		#region Video Controls

		#region Playback Controls
        private void OnRecord()
        {
            if (m_FrameServer.IsRecording)
            {
                // We will now be paused.
                buttonRecord.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.record;
                tmrCaptureDuration.Stop();
            }
            else
            {
                buttonRecord.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.stop;
                m_FrameServer.SetFilePrefix(txtRecordingName.Text);
                m_CaptureDuration = new DateTime();

                tmrCaptureDuration.Start();
            }
            m_FrameServer.ToggleRecord();
        }
		private void buttonRecord_Click(object sender, EventArgs e)
        {
            OnRecord();                
        }
		private void buttonPlay_Click(object sender, EventArgs e)
		{
			// Toggle play / pause capture.
			
			// Prepare the interface for after the change.
			if(m_FrameServer.IsRunning)
			{
				// We are running, we'll be paused.		   		
				buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqplay17;	
			}
		   	else
		   	{
		   		// We are paused, we'll be running.		   		
		   		pbSurfaceScreen.Visible = true;				
		   		m_FrameServer.CoordinateSystem.Stretch = 1.0f;
		   		ShowHideResizers(true);
	    		StretchSqueezeSurface();								
				buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqpause6;
		   	}

		   	// Actually do the toggle.
		   	m_FrameServer.TogglePlay();
		   	
			OnPoke();	
		}
		public void Common_MouseWheel(object sender, MouseEventArgs e)
		{
			// MouseWheel was recorded on one of the controls.
			int iScrollOffset = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				if (iScrollOffset > 0)
				{
					IncreaseDirectZoom();
				}
				else
				{
					DecreaseDirectZoom();
				}
			}
			else
			{
				// return in recent frame history ?	
			}
		}
		#endregion

		#region Frame Tracker
		private void trkFrame_PositionChanging(object sender, long _iPosition)
		{
			
		}
		private void trkFrame_PositionChanged(object sender, long _iPosition)
		{
			
		}
		private void UpdateCurrentPositionLabel()
		{
			// Here we will display the time in negative.
			
			/*
			//-----------------------------------------------------------------
			// Format d'affichage : Standard TimeCode.
			// Heures:Minutes:Secondes.Frames
			// Position relative à la Selection Primaire / Zone de travail
			//-----------------------------------------------------------------
			string timecode = TimeStampsToTimecode(m_iCurrentPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, false);
			lblTimeCode.Text = ScreenManagerLang.lblTimeCode_Text + " : " + timecode;
			lblTimeCode.Invalidate();
			*/
		}
		private void UpdateNavigationCursor()
		{
			// Update cursor position after Resize, ShowNextFrame, Working Zone change.
			//trkFrame.Position = m_iCurrentPosition;
			//UpdateCurrentPositionLabel();
		}
		#endregion

		#endregion

		#region Image Border
		private void ShowBorder(bool _bShow)
		{
			m_bShowImageBorder = _bShow;
			pbSurfaceScreen.Invalidate();
		}
		private void DrawImageBorder(Graphics _canvas)
		{
			// Draw the border around the screen to mark it as selected.
			// Called back from main drawing routine.
			_canvas.DrawRectangle(m_PenImageBorder, 0, 0, pbSurfaceScreen.Width - m_PenImageBorder.Width, pbSurfaceScreen.Height - m_PenImageBorder.Width);
		}
		#endregion

		#region Auto Stretch & Manual Resize
		private void StretchSqueezeSurface()
		{
			// Check if the image was loaded squeezed.
			// (happen when screen control isn't being fully expanded at video load time.)
			if(pbSurfaceScreen.Height < panelCenter.Height && m_FrameServer.CoordinateSystem.Stretch < 1.0)
			{
				m_FrameServer.CoordinateSystem.Stretch = 1.0;
			}
			
			//---------------------------------------------------------------
			// Check if the stretch factor is not going to outsize the panel.
			// If so, force maximized, unless screen is smaller than video.
			//---------------------------------------------------------------
			int iTargetHeight = (int)((double)m_FrameServer.DecodingSize.Height * m_FrameServer.CoordinateSystem.Stretch);
			int iTargetWidth = (int)((double)m_FrameServer.DecodingSize.Width * m_FrameServer.CoordinateSystem.Stretch);
			
			if (iTargetHeight > panelCenter.Height || iTargetWidth > panelCenter.Width)
			{
				if (m_FrameServer.CoordinateSystem.Stretch > 1.0)
				{
					m_bStretchModeOn = true;
				}
			}
			
			if ((m_bStretchModeOn) || (m_FrameServer.DecodingSize.Width > panelCenter.Width) || (m_FrameServer.DecodingSize.Height > panelCenter.Height))
			{
				//-------------------------------------------------------------------------------
				// Maximiser :
				//Redimensionner l'image selon la dimension la plus proche de la taille du panel.
				//-------------------------------------------------------------------------------
				float WidthRatio = (float)m_FrameServer.DecodingSize.Width / panelCenter.Width;
				float HeightRatio = (float)m_FrameServer.DecodingSize.Height / panelCenter.Height;
				
				if (WidthRatio > HeightRatio)
				{
					pbSurfaceScreen.Width = panelCenter.Width;
					pbSurfaceScreen.Height = (int)((float)m_FrameServer.DecodingSize.Height / WidthRatio);
					
					m_FrameServer.CoordinateSystem.Stretch = (1 / WidthRatio);
				}
				else
				{
					pbSurfaceScreen.Width = (int)((float)m_FrameServer.DecodingSize.Width / HeightRatio);
					pbSurfaceScreen.Height = panelCenter.Height;
					
					m_FrameServer.CoordinateSystem.Stretch = (1 / HeightRatio);
				}
			}
			else
			{
				
				pbSurfaceScreen.Width = (int)((double)m_FrameServer.DecodingSize.Width * m_FrameServer.CoordinateSystem.Stretch);
				pbSurfaceScreen.Height = (int)((double)m_FrameServer.DecodingSize.Height * m_FrameServer.CoordinateSystem.Stretch);
			}
			
			//recentrer
			pbSurfaceScreen.Left = (panelCenter.Width / 2) - (pbSurfaceScreen.Width / 2);
			pbSurfaceScreen.Top = (panelCenter.Height / 2) - (pbSurfaceScreen.Height / 2);
			ReplaceResizers();
			
			// Redéfinir les plans & grilles 3D
			Size imageSize = new Size(m_FrameServer.DecodingSize.Width, m_FrameServer.DecodingSize.Height);
			m_FrameServer.Metadata.Plane.SetLocations(imageSize, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Location);
			m_FrameServer.Metadata.Grid.SetLocations(imageSize, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Location);
		}
		private void ReplaceResizers()
		{
			ImageResizerSE.Left = pbSurfaceScreen.Left + pbSurfaceScreen.Width - (ImageResizerSE.Width / 2);
			ImageResizerSE.Top = pbSurfaceScreen.Top + pbSurfaceScreen.Height - (ImageResizerSE.Height / 2);

			ImageResizerSW.Left = pbSurfaceScreen.Left - (ImageResizerSW.Width / 2);
			ImageResizerSW.Top = pbSurfaceScreen.Top + pbSurfaceScreen.Height - (ImageResizerSW.Height / 2);

			ImageResizerNE.Left = pbSurfaceScreen.Left + pbSurfaceScreen.Width - (ImageResizerNE.Width / 2);
			ImageResizerNE.Top = pbSurfaceScreen.Top - (ImageResizerNE.Height / 2);

			ImageResizerNW.Left = pbSurfaceScreen.Left - (ImageResizerNW.Width / 2);
			ImageResizerNW.Top = pbSurfaceScreen.Top - (ImageResizerNW.Height / 2);
		}
		private void ToggleStretchMode()
		{
			if (!m_bStretchModeOn)
			{
				m_bStretchModeOn = true;
			}
			else
			{
				// Ne pas repasser en stretch mode à false si on est plus petit que l'image
				if (m_FrameServer.CoordinateSystem.Stretch >= 1)
				{
					m_FrameServer.CoordinateSystem.Stretch = 1;
					m_bStretchModeOn = false;
				}
			}
			StretchSqueezeSurface();
			pbSurfaceScreen.Invalidate();
		}
		private void ImageResizerSE_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight = (ImageResizerSE.Top - pbSurfaceScreen.Top + e.Y);
				int iTargetWidth = (ImageResizerSE.Left - pbSurfaceScreen.Left + e.X);
				ResizeImage(iTargetWidth, iTargetHeight);
			}
		}
		private void ImageResizerSW_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight = (ImageResizerSW.Top - pbSurfaceScreen.Top + e.Y);
				int iTargetWidth = pbSurfaceScreen.Width + (pbSurfaceScreen.Left - (ImageResizerSW.Left + e.X));
				ResizeImage(iTargetWidth, iTargetHeight);
			}
		}
		private void ImageResizerNW_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight = pbSurfaceScreen.Height + (pbSurfaceScreen.Top - (ImageResizerNW.Top + e.Y));
				int iTargetWidth = pbSurfaceScreen.Width + (pbSurfaceScreen.Left - (ImageResizerNW.Left + e.X));
				ResizeImage(iTargetWidth, iTargetHeight);
			}
		}
		private void ImageResizerNE_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight = pbSurfaceScreen.Height + (pbSurfaceScreen.Top - (ImageResizerNE.Top + e.Y));
				int iTargetWidth = (ImageResizerNE.Left - pbSurfaceScreen.Left + e.X);
				ResizeImage(iTargetWidth, iTargetHeight);
			}
		}
		private void ResizeImage(int _iTargetWidth, int _iTargetHeight)
		{
			//-------------------------------------------------------------------
			// Resize at the following condition:
			// Bigger than original image size, smaller than panel size.
			//-------------------------------------------------------------------
			if (_iTargetHeight > m_FrameServer.DecodingSize.Height &&
			    _iTargetHeight < panelCenter.Height &&
			    _iTargetWidth > m_FrameServer.DecodingSize.Width &&
			    _iTargetWidth < panelCenter.Width)
			{
				double fHeightFactor = ((_iTargetHeight) / (double)m_FrameServer.DecodingSize.Height);
				double fWidthFactor = ((_iTargetWidth) / (double)m_FrameServer.DecodingSize.Width);

				m_FrameServer.CoordinateSystem.Stretch = (fWidthFactor + fHeightFactor) / 2;
				m_bStretchModeOn = false;
				StretchSqueezeSurface();
				pbSurfaceScreen.Invalidate();
			}
		}
		private void Resizers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// Maximiser l'écran ou repasser à la taille originale.
			if (!m_bStretchModeOn)
			{
				m_bStretchModeOn = true;
			}
			else
			{
				m_FrameServer.CoordinateSystem.Stretch = 1;
				m_bStretchModeOn = false;
			}
			StretchSqueezeSurface();
			pbSurfaceScreen.Invalidate();
		}
		#endregion
		
		#region Timers & Playloop
		private void IdleDetector(object sender, EventArgs e)
		{
			m_bIsIdle = true;
		}
		#endregion
		
		#region Culture
		private void ReloadMenusCulture()
		{
			// Reload the text for each menu.
			// this is done at construction time and at RefreshUICulture time.
			
			// 1. Default context menu.
			mnuSavePic.Text = ScreenManagerLang.mnuSavePic;
			mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
			
			// 2. Drawings context menu (Configure, Delete, Track this)
			mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;
			mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
			mnuShowMeasure.Text = ScreenManagerLang.mnuShowMeasure;
			mnuSealMeasure.Text = ScreenManagerLang.mnuSealMeasure;
			
			// 5. Magnifier
			mnuMagnifier150.Text = ScreenManagerLang.mnuMagnifier150;
			mnuMagnifier175.Text = ScreenManagerLang.mnuMagnifier175;
			mnuMagnifier200.Text = ScreenManagerLang.mnuMagnifier200;
			mnuMagnifier225.Text = ScreenManagerLang.mnuMagnifier225;
			mnuMagnifier250.Text = ScreenManagerLang.mnuMagnifier250;
			mnuMagnifierDirect.Text = ScreenManagerLang.mnuMagnifierDirect;	
			mnuMagnifierQuit.Text = ScreenManagerLang.mnuMagnifierQuit;
				
			// 6. Grids
			mnuGridsConfigure.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;	
			mnuGridsHide.Text = ScreenManagerLang.mnuGridsHide;
		}
		private void ReloadTooltipsCulture()
		{
			// Video controls
			toolTips.SetToolTip(buttonPlay, ScreenManagerLang.ToolTip_Play);
			
			// Export buttons
			toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.ToolTip_Snapshot);
			toolTips.SetToolTip(btnRafale, ScreenManagerLang.ToolTip_Rafale);

			// Drawing tools
			toolTips.SetToolTip(btnDrawingToolPointer, ScreenManagerLang.ToolTip_DrawingToolPointer);
			toolTips.SetToolTip(btnDrawingToolText, ScreenManagerLang.ToolTip_DrawingToolText);
			toolTips.SetToolTip(btnDrawingToolPencil, ScreenManagerLang.ToolTip_DrawingToolPencil);
			toolTips.SetToolTip(btnDrawingToolLine2D, ScreenManagerLang.ToolTip_DrawingToolLine2D);
			toolTips.SetToolTip(btnDrawingToolCross2D, ScreenManagerLang.ToolTip_DrawingToolCross2D);
			toolTips.SetToolTip(btnDrawingToolAngle2D, ScreenManagerLang.ToolTip_DrawingToolAngle2D);
			toolTips.SetToolTip(btnColorProfile, ScreenManagerLang.ToolTip_ColorProfile);
			toolTips.SetToolTip(btnMagnifier, ScreenManagerLang.ToolTip_Magnifier);
			toolTips.SetToolTip(btn3dplane, ScreenManagerLang.mnu3DPlane);

		}
		private void SetPopupConfigureParams(AbstractDrawing _drawing)
		{
			// choose between "Color" and "Color & Size" popup menu.

			if (_drawing is DrawingAngle2D || _drawing is DrawingCross2D)
			{
				mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_Color;
			}
			else
			{
				mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;
			}
			
			// Check Show Measure menu
			if(_drawing is DrawingLine2D)
			{
				mnuShowMeasure.Checked = ((DrawingLine2D)_drawing).ShowMeasure;
			}
		}
		#endregion

		#region SurfaceScreen Events
		private void SurfaceScreen_MouseDown(object sender, MouseEventArgs e)
		{
			if(m_FrameServer.IsConnected)
			{
			
				if (e.Button == MouseButtons.Left)
				{
					if (m_FrameServer.IsConnected)
					{
						if ( (m_ActiveTool == DrawingToolType.Pointer)      &&
						    (m_FrameServer.Magnifier.Mode != MagnifierMode.NotVisible) &&
						    (m_FrameServer.Magnifier.IsOnObject(e)))
						{
							m_FrameServer.Magnifier.OnMouseDown(e);
						}
						else
						{
							//-------------------------------------
							// Action begins:
							// Move or set magnifier
							// Move or set Drawing
							// Move Grids
							//-------------------------------------
						
							Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
						
							// 1. Pass all DrawingText to normal mode
							m_FrameServer.Metadata.AllDrawingTextToNormalMode();
						
							if (m_ActiveTool == DrawingToolType.Pointer)
							{
								// 1. Manipulating an object or Magnifier
								bool bMovingMagnifier = false;
								bool bDrawingHit = false;
							
								// Show the grabbing hand cursor.
								SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 1));
							
								if (m_FrameServer.Magnifier.Mode == MagnifierMode.Indirect)
								{
									bMovingMagnifier = m_FrameServer.Magnifier.OnMouseDown(e);
								}
							
								if (!bMovingMagnifier)
								{
									// Magnifier wasn't hit or is not in use,
									// try drawings (including chronos, grids, etc.)
									bDrawingHit = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseDown(m_FrameServer.Metadata, 0, descaledMouse, 0, m_PrefManager.DefaultFading.Enabled);
								}
							}
							else
							{
								//-----------------------
								// Creating a new Drawing
								//-----------------------
								if (m_ActiveTool != DrawingToolType.Text)
								{
									// Add an instance of a drawing from the active tool to the current keyframe.
									// The drawing is initialized with the current mouse coordinates.
									AbstractDrawing ad = m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, 0, 1);
									
									m_FrameServer.Metadata[0].AddDrawing(ad);
									m_FrameServer.Metadata.SelectedDrawingFrame = 0;
									m_FrameServer.Metadata.SelectedDrawing = 0;
									
									// Color
									m_ColorProfile.SetupDrawing(ad, m_ActiveTool);
									
									// Special preparation if it's a line.
									DrawingLine2D line = ad as DrawingLine2D;
									if(line != null)
									{
										line.ParentMetadata = m_FrameServer.Metadata;
										line.ShowMeasure = m_bMeasuring;
									}
								}
								else
								{
									
									// We are using the Text Tool. This is a special case because
									// if we are on an existing Textbox, we just go into edit mode
									// otherwise, we add and setup a new textbox.
									bool bEdit = false;
									foreach (AbstractDrawing ad in m_FrameServer.Metadata[0].Drawings)
									{
										if (ad is DrawingText)
										{
											int hitRes = ad.HitTest(descaledMouse, 0);
											if (hitRes >= 0)
											{
												bEdit = true;
												((DrawingText)ad).EditMode = true;
											}
										}
									}
									
									// If we are not on an existing textbox : create new DrawingText.
									if (!bEdit)
									{
										m_FrameServer.Metadata[0].AddDrawing(m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, 0, 1));
										m_FrameServer.Metadata.SelectedDrawingFrame = 0;
										m_FrameServer.Metadata.SelectedDrawing = 0;
										
										DrawingText dt = (DrawingText)m_FrameServer.Metadata[0].Drawings[0];
										
										dt.ContainerScreen = pbSurfaceScreen;
										dt.RelocateEditbox(m_FrameServer.CoordinateSystem.Stretch * m_FrameServer.CoordinateSystem.Zoom, m_FrameServer.CoordinateSystem.Location);
										dt.EditMode = true;
										panelCenter.Controls.Add(dt.EditBox);
										dt.EditBox.BringToFront();
										dt.EditBox.Focus();
										m_ColorProfile.SetupDrawing(dt, DrawingToolType.Text);
									}
								}
							}
						}	
					}
				}
				else if (e.Button == MouseButtons.Right)
				{
					// Show the right Pop Menu depending on context.
					// (Drawing, Grids, Magnifier, Nothing)
					
					Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
					
					if (m_FrameServer.IsConnected)
					{
						m_FrameServer.Metadata.UnselectAll();
						
						if (m_FrameServer.Metadata.IsOnDrawing(0, descaledMouse, 0))
						{
							// If we are on a Cross2D, we activate the menu to let the user Track it.
							AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[0].Drawings[m_FrameServer.Metadata.SelectedDrawing];
							
							// We use temp variables because ToolStripMenuItem.Visible always returns false...
							bool isLine = (ad is DrawingLine2D);
							mnuShowMeasure.Visible = isLine;
							mnuSealMeasure.Visible = isLine;
							mnuSepDrawing.Visible = isLine;
							
							// "Color & Size" or "Color" depending on drawing type.
							SetPopupConfigureParams(ad);
							
							this.ContextMenuStrip = popMenuDrawings;
						}
						else if (m_FrameServer.Metadata.IsOnGrid(descaledMouse))
						{
							this.ContextMenuStrip = popMenuGrids;
						}
						else if (m_FrameServer.Magnifier.Mode == MagnifierMode.Indirect && m_FrameServer.Magnifier.IsOnObject(e))
						{
							this.ContextMenuStrip = popMenuMagnifier;
						}
						else if(m_ActiveTool != DrawingToolType.Pointer)
						{
							// Launch Preconfigure dialog.
							// = Updates the tool's entry of the main color profile.
							formConfigureDrawing fcd = new formConfigureDrawing(m_ActiveTool, m_ColorProfile);
							LocateForm(fcd);
							fcd.ShowDialog();
							fcd.Dispose();
							
							UpdateCursor();
						}
						else
						{
							// No drawing touched and no tool selected
							this.ContextMenuStrip = popMenu;
						}
					}
				}
					
				pbSurfaceScreen.Invalidate();
			}
		}
		private void SurfaceScreen_MouseMove(object sender, MouseEventArgs e)
		{
			// We must keep the same Z order.
			// 1:Magnifier, 2:Drawings, 3:Chronos/Tracks, 4:Grids.
			// When creating a drawing, the active tool will stay on this drawing until its setup is over.
			// After the drawing is created, we either fall back to Pointer tool or stay on the same tool.

			if(m_FrameServer.IsConnected)
			{
				if (e.Button == MouseButtons.None && m_FrameServer.Magnifier.Mode == MagnifierMode.Direct)
				{
					m_FrameServer.Magnifier.MouseX = e.X;
					m_FrameServer.Magnifier.MouseY = e.Y;
					pbSurfaceScreen.Invalidate();
				}
				else if (e.Button == MouseButtons.Left)
				{
					if (m_ActiveTool != DrawingToolType.Pointer)
					{
						// Currently setting the second point of a Drawing.
						m_DrawingTools[(int)m_ActiveTool].OnMouseMove(m_FrameServer.Metadata[0], m_FrameServer.CoordinateSystem.Untransform(new Point(e.X, e.Y)));
					}
					else
					{
						bool bMovingMagnifier = false;
						if (m_FrameServer.Magnifier.Mode == MagnifierMode.Indirect)
						{
							bMovingMagnifier = m_FrameServer.Magnifier.OnMouseMove(e);
						}
						
						if (!bMovingMagnifier && m_ActiveTool == DrawingToolType.Pointer)
						{
							// Moving an object.
							
							Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
							
							// Magnifier is not being moved or is invisible, try drawings through pointer tool.
							bool bMovingObject = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseMove(m_FrameServer.Metadata, 0, descaledMouse, m_FrameServer.CoordinateSystem.Location, ModifierKeys);
							
							if (!bMovingObject && m_FrameServer.CoordinateSystem.Zooming)
							{
								// User is not moving anything and we are zooming : move the zoom window.
								
								// Get mouse deltas (descaled=in image coords).
								double fDeltaX = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.X;
								double fDeltaY = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.Y;
								
								m_FrameServer.CoordinateSystem.MoveZoomWindow(fDeltaX, fDeltaY);
							}
						}
					}
				}
					
				/*if (!m_bIsCurrentlyPlaying)
				{
					pbSurfaceScreen.Invalidate();
				}*/
			}
		}
		private void SurfaceScreen_MouseUp(object sender, MouseEventArgs e)
		{
			// End of an action.
			// Depending on the active tool we have various things to do.
			
			if(m_FrameServer.IsConnected && e.Button == MouseButtons.Left)
			{
				if (m_ActiveTool == DrawingToolType.Pointer)
				{
					OnPoke();
				}
				
				m_FrameServer.Magnifier.OnMouseUp(e);
				
				// Memorize the action we just finished to enable undo.
				if (m_ActiveTool != DrawingToolType.Pointer)
				{
					// Record the adding unless we are editing a text box.
					if (!m_bTextEdit)
					{
						IUndoableCommand cad = new CommandAddDrawing(DoInvalidate, DoDrawingUndrawn, m_FrameServer.Metadata, m_FrameServer.Metadata[0].Position);
						CommandManager cm = CommandManager.Instance();
						cm.LaunchUndoableCommand(cad);
						
					}
					else
					{
						m_bTextEdit = false;
					}
				}
				
				// The fact that we stay on this tool or fall back to pointer tool, depends on the tool.
				m_ActiveTool = m_DrawingTools[(int)m_ActiveTool].OnMouseUp();
				
				if (m_ActiveTool == DrawingToolType.Pointer)
				{
					SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
					((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).OnMouseUp();
				}
				
				// Unselect drawings.
				m_FrameServer.Metadata.SelectedDrawingFrame = -1;
				m_FrameServer.Metadata.SelectedDrawing = -1;
								
				pbSurfaceScreen.Invalidate();
			}
		}
		private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if(m_FrameServer.IsConnected && e.Button == MouseButtons.Left)
			{
				OnPoke();
				
				Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
				m_FrameServer.Metadata.AllDrawingTextToNormalMode();
				m_FrameServer.Metadata.UnselectAll();
				
				//------------------------------------------------------------------------------------
				// - If on text, switch to edit mode.
				// - If on other drawing, launch the configuration dialog.
				// - Otherwise -> Maximize/Reduce image.
				//------------------------------------------------------------------------------------
				if (m_FrameServer.Metadata.IsOnDrawing(0, descaledMouse, 0))
				{
					AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[0].Drawings[m_FrameServer.Metadata.SelectedDrawing];
					if (ad is DrawingText)
					{
						((DrawingText)ad).EditMode = true;
						m_ActiveTool = DrawingToolType.Text;
						m_bTextEdit = true;
					}
					else
					{
						mnuConfigureDrawing_Click(null, EventArgs.Empty);
					}
				}
				else if (m_FrameServer.Metadata.IsOnGrid(descaledMouse))
				{
					mnuGridsConfigure_Click(null, EventArgs.Empty);
				}
				else
				{
					ToggleStretchMode();
				}
			}
		}
		private void SurfaceScreen_Paint(object sender, PaintEventArgs e)
		{
			// Draw the image.
			m_FrameServer.Draw(e.Graphics);
						
			// Draw selection Border if needed.
			if (m_bShowImageBorder)
			{
				DrawImageBorder(e.Graphics);
			}	
		}
		private void SurfaceScreen_MouseEnter(object sender, EventArgs e)
		{
			
			// Set focus to surfacescreen to enable mouse scroll
			
			// But only if there is no Text edition going on.
			bool bEditing = false;
			
			foreach (AbstractDrawing ad in m_FrameServer.Metadata[0].Drawings)
			{
				DrawingText dt = ad as DrawingText;
				if (dt != null)
				{
					if(dt.EditMode)
					{
						bEditing = true;
						break;
					}
				}
			}
			
			
			if(!bEditing)
			{
				pbSurfaceScreen.Focus();
			}
		}
		
		#endregion

		#region PanelCenter Events
		private void PanelCenter_MouseEnter(object sender, EventArgs e)
		{
			// Give focus to enable mouse scroll.
			panelCenter.Focus();
		}
		private void PanelCenter_MouseClick(object sender, MouseEventArgs e)
		{
			OnPoke();
		}
		private void PanelCenter_Resize(object sender, EventArgs e)
		{
			StretchSqueezeSurface();
			pbSurfaceScreen.Invalidate();
		}
		#endregion
		
		#region Keyframes Panel
		private void SetupDefaultThumbBox(UserControl _box)
		{
			_box.Top = 10;
			_box.Cursor = Cursors.Hand;
		}
		private void pnlThumbnails_MouseEnter(object sender, EventArgs e)
		{
			// Give focus to disable keyframe box editing.
			pnlThumbnails.Focus();
		}
		private void splitKeyframes_Resize(object sender, EventArgs e)
		{
			// Redo the dock/undock if needed to be at the right place.
			// (Could be handled by layout ?)
			if(m_bDocked)
			{
				DockKeyframePanel();
			}
			else
			{
				UndockKeyframePanel();
			}
		}
		private void SetupDefaultThumbBox(KeyframeBox _ThumbBox)
		{
			_ThumbBox.Top = 10;
			_ThumbBox.Cursor = Cursors.Hand;
		}
		
		public void OnKeyframesTitleChanged()
		{
			// Called when title changed.
			pbSurfaceScreen.Invalidate();
		}
		private void pnlThumbnails_DoubleClick(object sender, EventArgs e)
		{
			OnPoke();
		}

		#region Docking Undocking
		private void btnDockBottom_Click(object sender, EventArgs e)
		{
			if (m_bDocked)
			{
				UndockKeyframePanel();
			}
			else
			{
				DockKeyframePanel();
			}
		}
		private void splitKeyframes_Panel2_DoubleClick(object sender, EventArgs e)
		{
			// double click on the DrawingTools bar => expand/retract Keyframes panel
			btnDockBottom_Click(null, EventArgs.Empty);
		}
		private void DockKeyframePanel()
		{
			// hide the keyframes
			splitKeyframes.SplitterDistance = splitKeyframes.Height - 25;

			// change image
			btnDockBottom.BackgroundImage = Resources.undock16x16;

			// If there is 0 images, the arrow isn't visible.
			if (m_FrameServer.Metadata.Count == 0)
				btnDockBottom.Visible = false;

			// change status
			m_bDocked = true;
			
		}
		private void UndockKeyframePanel()
		{
			// show the keyframes
			splitKeyframes.SplitterDistance = splitKeyframes.Height - 140;

			// change image
			btnDockBottom.BackgroundImage = Resources.dock16x16;
			btnDockBottom.Visible = true;

			// change status
			m_bDocked = false;
		}
		private void PrepareKeyframesDock()
		{
			// If there's no keyframe, and we will be using a tool,
			// the keyframes dock should be raised.
			// This way we don't surprise the user when he click the screen and the image moves around.
			// (especially problematic when using the Pencil.
			
			// this is only done for the very first keyframe.
			if (m_FrameServer.Metadata.Count < 1)
			{
				UndockKeyframePanel();
			}
		}
		#endregion

		#endregion

		#region Drawings Toolbar Events
		private void btnDrawingToolLine2D_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Line2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorLine2D, 0));
			}
		}
		private void btnDrawingToolPointer_Click(object sender, EventArgs e)
		{
			OnPoke();
			m_ActiveTool = DrawingToolType.Pointer;
			SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
		}
		private void btnDrawingToolCross2D_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Cross2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorCross2D, 0));
			}
		}
		private void btnDrawingToolAngle2D_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Angle2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorAngle2D, 0));
			}
		}
		private void btnDrawingToolPencil_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Pencil;
				UpdateCursor();
			}
		}
		private void btnMagnifier_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.IsConnected)
			{
				m_ActiveTool = DrawingToolType.Pointer;

				// Magnifier is half way between a persisting tool (like trackers and chronometers).
				// and a mode like grid and 3dplane.
				if (m_FrameServer.Magnifier.Mode == MagnifierMode.NotVisible)
				{
					UnzoomDirectZoom();
					m_FrameServer.Magnifier.Mode = MagnifierMode.Direct;
					btnMagnifier.BackgroundImage = Resources.magnifierActive2;
					SetCursor(Cursors.Cross);
				}
				else if (m_FrameServer.Magnifier.Mode == MagnifierMode.Direct)
				{
					// Revert to no magnification.
					UnzoomDirectZoom();
					m_FrameServer.Magnifier.Mode = MagnifierMode.NotVisible;
					btnMagnifier.BackgroundImage = Resources.magnifier2;
					SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
					pbSurfaceScreen.Invalidate();
				}
				else
				{
					DisableMagnifier();
					pbSurfaceScreen.Invalidate();
				}
			}
		}
		private void DisableMagnifier()
		{
			// Revert to no magnification.
			m_FrameServer.Magnifier.Mode = MagnifierMode.NotVisible;
			btnMagnifier.BackgroundImage = Resources.magnifier2;
			SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
		}
		private void btn3dplane_Click(object sender, EventArgs e)
		{
			m_FrameServer.Metadata.Plane.Visible = !m_FrameServer.Metadata.Plane.Visible;
			m_ActiveTool = DrawingToolType.Pointer;
			OnPoke();
			pbSurfaceScreen.Invalidate();
		}
		private void UpdateCursor()
		{
			// Ther current cursor must be updated.

			// Get the cursor and use it.
			if (m_ActiveTool == DrawingToolType.Pencil)
			{
				int iCircleSize = (int)((double)m_ColorProfile.StylePencil.Size * m_FrameServer.CoordinateSystem.Stretch);
				Cursor c = m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorPencil, iCircleSize);
				SetCursor(c);
			}
			else if (m_ActiveTool == DrawingToolType.Cross2D)
			{
				Cursor c = m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorCross2D, 0);
				SetCursor(c);
			}
		}
		private void btnDrawingToolText_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Text;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			}
		}
		private void btnColorProfile_Click(object sender, EventArgs e)
		{
			OnPoke();

			// Load, save or modify current profile.
			formColorProfile fcp = new formColorProfile(m_ColorProfile);
			fcp.ShowDialog();
			fcp.Dispose();

			UpdateCursor();
		}
		private void SetCursor(Cursor _cur)
		{
			if (m_ActiveTool != DrawingToolType.Pointer)
			{
				panelCenter.Cursor = _cur;
			}
			else
			{
				panelCenter.Cursor = Cursors.Default;
			}

			pbSurfaceScreen.Cursor = _cur;
		}
		private void LocateForm(Form _form)
		{
			// A helper function to center the dialog boxes.
			if (Cursor.Position.X + (_form.Width / 2) >= SystemInformation.PrimaryMonitorSize.Width)
			{
				_form.StartPosition = FormStartPosition.CenterScreen;
			}
			else
			{
				_form.Location = new Point(Cursor.Position.X - (_form.Width / 2), Cursor.Position.Y - 20);
			}
		}
		#endregion

		#region Context Menus Events
		
		#region Drawings Menus
		private void mnuConfigureDrawing_Click(object sender, EventArgs e)
		{
			if(m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureDrawing fcd = new formConfigureDrawing(m_FrameServer.Metadata[0].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				LocateForm(fcd);
				fcd.ShowDialog();
				fcd.Dispose();
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		private void mnuConfigureFading_Click(object sender, EventArgs e)
		{
			if(m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureFading fcf = new formConfigureFading(m_FrameServer.Metadata[0].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				LocateForm(fcf);
				fcf.ShowDialog();
				fcf.Dispose();
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		
		private void mnuShowMeasure_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the measure for this line.
			if(m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_FrameServer.Metadata[0].Drawings[m_FrameServer.Metadata.SelectedDrawing] as DrawingLine2D;
				if(line!= null)
				{
					mnuShowMeasure.Checked = !mnuShowMeasure.Checked;
					line.ShowMeasure = mnuShowMeasure.Checked;
					m_bMeasuring = mnuShowMeasure.Checked;
					pbSurfaceScreen.Invalidate();
				}
			}
		}
		private void mnuSealMeasure_Click(object sender, EventArgs e)
		{
			// display a dialog that let the user specify how many real-world-units long is this line.
			
			if(m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_FrameServer.Metadata[0].Drawings[m_FrameServer.Metadata.SelectedDrawing] as DrawingLine2D;
				if(line!= null)
				{
					if(line.m_StartPoint.X != line.m_EndPoint.X || line.m_StartPoint.Y != line.m_EndPoint.Y)
					{
						if(!line.ShowMeasure)
							line.ShowMeasure = true;
						
						m_bMeasuring = true;
						
						DelegatesPool dp = DelegatesPool.Instance();
						if (dp.DeactivateKeyboardHandler != null)
						{
							dp.DeactivateKeyboardHandler();
						}

						formConfigureMeasure fcm = new formConfigureMeasure(m_FrameServer.Metadata, line);
						LocateForm(fcm);
						fcm.ShowDialog();
						fcm.Dispose();
						
						pbSurfaceScreen.Invalidate();
						this.ContextMenuStrip = popMenu;
						
						if (dp.ActivateKeyboardHandler != null)
						{
							dp.ActivateKeyboardHandler();
						}
					}
				}
			}
		}
		private void mnuDeleteDrawing_Click(object sender, EventArgs e)
		{
			DeleteSelectedDrawing();
			this.ContextMenuStrip = popMenu;
		}
		private void DeleteSelectedDrawing()
		{
			if (m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				IUndoableCommand cdd = new CommandDeleteDrawing(DoInvalidate, m_FrameServer.Metadata, m_FrameServer.Metadata[0].Position, m_FrameServer.Metadata.SelectedDrawing);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cdd);
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		#endregion
		
		#region Magnifier Menus
		private void mnuMagnifierQuit_Click(object sender, EventArgs e)
		{
			DisableMagnifier();
			pbSurfaceScreen.Invalidate();
		}
		private void mnuMagnifierDirect_Click(object sender, EventArgs e)
		{
			// Use position and magnification to Direct Zoom.
			// Go to direct zoom, at magnifier zoom factor, centered on same point as magnifier.
			m_FrameServer.CoordinateSystem.Zoom = m_FrameServer.Magnifier.ZoomFactor;
			m_FrameServer.CoordinateSystem.RelocateZoomWindow(m_FrameServer.Magnifier.MagnifiedCenter);
			DisableMagnifier();
			pbSurfaceScreen.Invalidate();
		}
		private void mnuMagnifier150_Click(object sender, EventArgs e)
		{
			SetMagnifier(mnuMagnifier150, 1.5);
		}
		private void mnuMagnifier175_Click(object sender, EventArgs e)
		{
			SetMagnifier(mnuMagnifier175, 1.75);
		}
		private void mnuMagnifier200_Click(object sender, EventArgs e)
		{
			SetMagnifier(mnuMagnifier200, 2.0);
		}
		private void mnuMagnifier225_Click(object sender, EventArgs e)
		{
			SetMagnifier(mnuMagnifier225, 2.25);
		}
		private void mnuMagnifier250_Click(object sender, EventArgs e)
		{
			SetMagnifier(mnuMagnifier250, 2.5);
		}
		private void SetMagnifier(ToolStripMenuItem _menu, double _fValue)
		{
			m_FrameServer.Magnifier.ZoomFactor = _fValue;
			UncheckMagnifierMenus();
			_menu.Checked = true;
			pbSurfaceScreen.Invalidate();
		}
		private void UncheckMagnifierMenus()
		{
			mnuMagnifier150.Checked = false;
			mnuMagnifier175.Checked = false;
			mnuMagnifier200.Checked = false;
			mnuMagnifier225.Checked = false;
			mnuMagnifier250.Checked = false;
		}
		#endregion

		#region Grids Menus
		private void mnuGridsConfigure_Click(object sender, EventArgs e)
		{
			formConfigureGrids fcg;

			if (m_FrameServer.Metadata.Plane.Selected)
			{
				m_FrameServer.Metadata.Plane.Selected = false;
				fcg = new formConfigureGrids(m_FrameServer.Metadata.Plane, pbSurfaceScreen);
				LocateForm(fcg);
				fcg.ShowDialog();
				fcg.Dispose();
			}
			else if (m_FrameServer.Metadata.Grid.Selected)
			{
				m_FrameServer.Metadata.Grid.Selected = false;
				fcg = new formConfigureGrids(m_FrameServer.Metadata.Grid, pbSurfaceScreen);
				LocateForm(fcg);
				fcg.ShowDialog();
				fcg.Dispose();
			}

			pbSurfaceScreen.Invalidate();
			
		}
		private void mnuGridsHide_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Metadata.Plane.Selected)
			{
				m_FrameServer.Metadata.Plane.Selected = false;
				m_FrameServer.Metadata.Plane.Visible = false;
			}
			else if (m_FrameServer.Metadata.Grid.Selected)
			{
				m_FrameServer.Metadata.Grid.Selected = false;
				m_FrameServer.Metadata.Grid.Visible = false;
			}

			pbSurfaceScreen.Invalidate();

			// Triggers an update to the menu.
			OnPoke();
		}
		#endregion

		#endregion
		
		#region DirectZoom
		private void UnzoomDirectZoom()
		{
			m_FrameServer.CoordinateSystem.ReinitZoom();
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetZoomLocation(m_FrameServer.CoordinateSystem.Location);
		}
		private void IncreaseDirectZoom()
		{
			if (m_FrameServer.Magnifier.Mode != MagnifierMode.NotVisible)
			{
				DisableMagnifier();
			}

			// Max zoom : 600%
			if (m_FrameServer.CoordinateSystem.Zoom < 6.0f)
			{
				m_FrameServer.CoordinateSystem.Zoom += 0.20f;
				RelocateDirectZoom();
				pbSurfaceScreen.Invalidate();
			}
		}
		private void DecreaseDirectZoom()
		{
			if (m_FrameServer.CoordinateSystem.Zoom > 1.0f)
			{
				m_FrameServer.CoordinateSystem.Zoom -= 0.20f;
				RelocateDirectZoom();
				pbSurfaceScreen.Invalidate();
			}
		}
		private void RelocateDirectZoom()
		{
			m_FrameServer.CoordinateSystem.RelocateZoomWindow();
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetZoomLocation(m_FrameServer.CoordinateSystem.Location);
		}
		#endregion

		#region VideoFilters Management
		private void DisablePlayAndDraw()
		{
			m_ActiveTool = DrawingToolType.Pointer;
			SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			DisableMagnifier();
			UnzoomDirectZoom();
		}
		private void EnableDisableAllPlayingControls(bool _bEnable)
		{
			buttonPlay.Enabled = _bEnable;
			btnRafale.Enabled = _bEnable;
			//trkFrame.Enabled = _bEnable;
		}
		private void EnableDisableDrawingTools(bool _bEnable)
		{
			btn3dplane.Enabled = _bEnable;
			btnDrawingToolAngle2D.Enabled = _bEnable;
			btnDrawingToolCross2D.Enabled = _bEnable;
			btnDrawingToolLine2D.Enabled = _bEnable;
			btnDrawingToolPencil.Enabled = _bEnable;
			btnDrawingToolPointer.Enabled = _bEnable;
			btnDrawingToolText.Enabled = _bEnable;
			btnMagnifier.Enabled = _bEnable;
			btnColorProfile.Enabled = _bEnable;
		}
		#endregion
		
		#region Export video and frames
		private void btnSnapShot_Click(object sender, EventArgs e)
		{
			/*
			// Export the current frame.
			if ((m_FrameServer.VideoFile.Loaded) && (m_FrameServer.VideoFile.CurrentImage != null))
			{
				StopPlaying();
				try
				{
					SaveFileDialog dlgSave = new SaveFileDialog();
					dlgSave.Title = ScreenManagerLang.dlgSaveTitle;
					dlgSave.RestoreDirectory = true;
					dlgSave.Filter = ScreenManagerLang.dlgSaveFilter;
					dlgSave.FilterIndex = 1;
					
					if(m_bDrawtimeFiltered && m_DrawingFilterOutput != null)
					{
						dlgSave.FileName = Path.GetFileNameWithoutExtension(m_FrameServer.VideoFile.FilePath);
					}
					else
					{
						dlgSave.FileName = BuildFilename(m_FrameServer.VideoFile.FilePath, m_iCurrentPosition, m_PrefManager.TimeCodeFormat);
					}
					
					if (dlgSave.ShowDialog() == DialogResult.OK)
					{
						
						// 1. Reconstruct the extension.
						// If the user let "file.00.00" as a filename, the extension is not appended automatically.
						string strImgNameLower = dlgSave.FileName.ToLower();
						string strImgName;
						if (strImgNameLower.EndsWith("jpg") || strImgNameLower.EndsWith("jpeg") || strImgNameLower.EndsWith("bmp") || strImgNameLower.EndsWith("png"))
						{
							// Ok, the user added the extension himself or he did not use the preformatting.
							strImgName = dlgSave.FileName;
						}
						else
						{
							// Get the extension
							string extension;
							switch (dlgSave.FilterIndex)
							{
								case 1:
									extension = ".jpg";
									break;
								case 2:
									extension = ".png";
									break;
								case 3:
									extension = ".bmp";
									break;
								default:
									extension = ".jpg";
									break;
							}
							strImgName = dlgSave.FileName + extension;
						}

						//2. Get image.
						Bitmap outputImage = GetFlushedImage();
						
						//3. Save the file.
						if (strImgName.ToLower().EndsWith("jpg"))
						{
							Bitmap OutputJpg = ConvertToJPG(outputImage);
							OutputJpg.Save(strImgName, ImageFormat.Jpeg);
							OutputJpg.Dispose();
						}
						else if (strImgName.ToLower().EndsWith("bmp"))
						{
							outputImage.Save(strImgName, ImageFormat.Bmp);
						}
						else if (strImgName.ToLower().EndsWith("png"))
						{
							outputImage.Save(strImgName, ImageFormat.Png);
						}

						outputImage.Dispose();
					}
				}
				catch (Exception exp)
				{
					log.Error(exp.StackTrace);
				}
			}*/
		}
		private void btnRafale_Click(object sender, EventArgs e)
		{
			/*
			//---------------------------------------------------------------------------------
			// Workflow:
			// 1. formRafaleExport  : configure the export, calls:
			// 2. FileSaveDialog    : choose the file name, then:
			// 3. formFrameExport   : Progress bar holder and updater, calls:
			// 4. SaveImageSequence (below) to perform the real work. (saving the pics)
			//---------------------------------------------------------------------------------

			if ((m_FrameServer.VideoFile.Loaded) && (m_FrameServer.VideoFile.CurrentImage != null))
			{
				StopPlaying();

				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}
				
				// Launch sequence saving configuration dialog
				formRafaleExport fre = new formRafaleExport(this, m_FrameServer.Metadata, m_FrameServer.VideoFile.FilePath, m_iSelDuration, m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds, m_FrameServer.VideoFile.Infos.fFps);
				fre.ShowDialog();
				fre.Dispose();

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}
			}*/
		}
		public void SaveImageSequence(BackgroundWorker bgWorker, string _FilePath, Int64 _iIntervalTimeStamps, bool _bBlendDrawings, bool _bKeyframesOnly, int iEstimatedTotal)
		{
			/*
			//---------------------------------------------------------------
			// Save image sequence.
			// (Method called back from the FormRafaleExport dialog box)
			//
			// We start at the first frame and use the interval in timestamps.
			// We append the timecode between the filename and the extension.
			//---------------------------------------------------------------

			//-------------------------------------------------------------
			// /!\ Cette fonction s'execute dans l'espace du WORKER THREAD.
			// Les fonctions appelées d'ici ne doivent pas toucher l'UI.
			// Les appels ici sont synchrones mais on peut remonter de
			// l'information par bgWorker_ProgressChanged().
			//-------------------------------------------------------------
			if (_bKeyframesOnly)
			{
				int iCurrent = 0;
				int iTotal = m_FrameServer.Metadata.Keyframes.Count;
				foreach(Keyframe kf in m_FrameServer.Metadata.Keyframes)
				{
					if (kf.Position >= m_iSelStart && kf.Position <= m_iSelEnd)
					{
						// Build the file name
						string fileName = Path.GetDirectoryName(_FilePath) + "\\" + BuildFilename(_FilePath, kf.Position, m_PrefManager.TimeCodeFormat) + Path.GetExtension(_FilePath);

						// Get the image
						Size iNewSize = new Size((int)((double)kf.FullFrame.Width * m_FrameServer.CoordinateSystem.Stretch), (int)((double)kf.FullFrame.Height * m_FrameServer.CoordinateSystem.Stretch));
						Bitmap outputImage = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
						outputImage.SetResolution(kf.FullFrame.HorizontalResolution, kf.FullFrame.VerticalResolution);
						Graphics g = Graphics.FromImage(outputImage);

						if (_bBlendDrawings)
						{
							FlushOnGraphics(kf.FullFrame, g, iNewSize, iCurrent, kf.Position);
						}
						else
						{
							// image only.
							g.DrawImage(kf.FullFrame, 0, 0, iNewSize.Width, iNewSize.Height);
						}

						// Save the file
						SaveImageFile(fileName, outputImage);
						outputImage.Dispose();
					}
					
					// Report to Progress Bar
					iCurrent++;
					bgWorker.ReportProgress(iCurrent, iTotal);
				}
			}
			else
			{
				// We are in the worker thread space.
				// We'll move the playhead and check for rafale period.

				m_iFramesToDecode = 1;
				ShowNextFrame(m_iSelStart, false);

				bool done = false;
				int iCurrent = 0;
				do
				{
					ActivateKeyframe(m_iCurrentPosition, false);

					// Build the file name
					string fileName = Path.GetDirectoryName(_FilePath) + "\\" + BuildFilename(_FilePath, m_iCurrentPosition, m_PrefManager.TimeCodeFormat) + Path.GetExtension(_FilePath);

					Size iNewSize = new Size((int)((double)m_FrameServer.VideoFile.CurrentImage.Width * m_FrameServer.CoordinateSystem.Stretch), (int)((double)m_FrameServer.VideoFile.CurrentImage.Height * m_FrameServer.CoordinateSystem.Stretch));
					Bitmap outputImage = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
					outputImage.SetResolution(m_FrameServer.VideoFile.CurrentImage.HorizontalResolution, m_FrameServer.VideoFile.CurrentImage.VerticalResolution);
					Graphics g = Graphics.FromImage(outputImage);

					if (_bBlendDrawings)
					{
						int iKeyFrameIndex = -1;
						if (m_iActiveKeyFrameIndex >= 0 && m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
						{
							iKeyFrameIndex = m_iActiveKeyFrameIndex;
						}

						FlushOnGraphics(m_FrameServer.VideoFile.CurrentImage, g, iNewSize, iKeyFrameIndex, m_iCurrentPosition);
					}
					else
					{
						// image only.
						g.DrawImage(m_FrameServer.VideoFile.CurrentImage, 0, 0, iNewSize.Width, iNewSize.Height);
					}

					// Save the file
					SaveImageFile(fileName, outputImage);
					outputImage.Dispose();

					// Report to Progress Bar
					iCurrent++;
					bgWorker.ReportProgress(iCurrent, iEstimatedTotal);


					// Go to next timestamp.
					if (m_iCurrentPosition + _iIntervalTimeStamps < m_iSelEnd)
					{
						m_iFramesToDecode = 1;
						ShowNextFrame(m_iCurrentPosition + _iIntervalTimeStamps, false);
					}
					else
					{
						done = true;
					}
				}
				while (!done);

				// Replace at selection start.
				m_iFramesToDecode = 1;
				ShowNextFrame(m_iSelStart, false);
				ActivateKeyframe(m_iCurrentPosition, false);
			}

			pbSurfaceScreen.Invalidate();*/
		}
		private void SaveImageFile(string _fileName, Bitmap _OutputImage)
		{
			if (_fileName.ToLower().EndsWith("jpg"))
			{
				Bitmap OutputJpg = ConvertToJPG(_OutputImage);
				OutputJpg.Save(_fileName, ImageFormat.Jpeg);
				OutputJpg.Dispose();
			}
			else if (_fileName.ToLower().EndsWith("bmp"))
			{
				_OutputImage.Save(_fileName, ImageFormat.Bmp);
			}
			else if (_fileName.ToLower().EndsWith("png"))
			{
				_OutputImage.Save(_fileName, ImageFormat.Png);
			}
			else
			{
				// the user may have put a filename in the form : "filename.ext"
				// where ext is unsupported. Or he misunderstood and put ".00.00"
				// We force format to jpg and we change back the extension to ".jpg".
				string fileName = Path.GetDirectoryName(_fileName) + "\\" + Path.GetFileNameWithoutExtension(_fileName) + ".jpg";

				Bitmap OutputJpg = ConvertToJPG(_OutputImage);
				OutputJpg.Save(fileName, ImageFormat.Jpeg);
				OutputJpg.Dispose();
			}
		}
		
		private string BuildFilename(string _FilePath, Int64 _position, TimeCodeFormat _timeCodeFormat)
		{
			return "todo";
			/*
			//-------------------------------------------------------
			// Build a file name, including extension
			// inserting the current timecode in the given file name.
			//-------------------------------------------------------

			TimeCodeFormat tcf;
			if(_timeCodeFormat == TimeCodeFormat.TimeAndFrames)
				tcf = TimeCodeFormat.ClassicTime;
			else
				tcf = _timeCodeFormat;
			
			// Timecode string (Not relative to sync position)
			string suffix = TimeStampsToTimecode(_position - m_iSelStart, tcf, false);
			string maxSuffix = TimeStampsToTimecode(m_iSelEnd - m_iSelStart, tcf, false);

			switch (tcf)
			{
				case TimeCodeFormat.Frames:
				case TimeCodeFormat.TenThousandthOfHours:
				case TimeCodeFormat.HundredthOfMinutes:
					
					int iZerosToPad = maxSuffix.Length - suffix.Length;
					for (int i = 0; i < iZerosToPad; i++)
					{
						// Add a leading zero.
						suffix = suffix.Insert(0, "0");
					}
					break;
				default:
					break;
			}

			// Reconstruct filename
			return Path.GetFileNameWithoutExtension(_FilePath) + "-" + suffix.Replace(':', '.');
			*/
		}
		private Bitmap ConvertToJPG(Bitmap _image)
		{
			// Intermediate MemoryStream for the conversion.
			MemoryStream memStr = new MemoryStream();

			//Get the list of available encoders
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			//find the encoder with the image/jpeg mime-type
			ImageCodecInfo ici = null;
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.MimeType == "image/jpeg")
				{
					ici = codec;
				}
			}

			if (ici != null)
			{
				//Create a collection of encoder parameters (we only need one in the collection)
				EncoderParameters ep = new EncoderParameters();
				ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

				_image.Save(memStr, ici, ep);
			}
			else
			{
				// No JPG encoder found (is that common ?) Use default system.
				_image.Save(memStr, ImageFormat.Jpeg);
			}

			return new Bitmap(memStr);
		}
		#endregion
        
		#region Capture device
        private void tmrCaptureDeviceDetector_Tick(object sender, EventArgs e)
        {
        	if(!m_FrameServer.IsConnected)
        	{
        		// Prevent reentry.
        		if(!m_bTryingToConnect)
        		{
        			m_bTryingToConnect = true;
        			
        			m_ScreenUIHandler.CaptureScreenUI_TryDeviceConnection(m_CurrentCaptureDevice);
        			
        			m_bTryingToConnect = false;
        		}
        	}
        }
        #endregion

        private void btnBrowserRecFolder_Click(object sender, EventArgs e)
        {
            // TODO: Set default path or some old path.
            FolderBrowserDialog folderDld = new FolderBrowserDialog();
            if (folderDld.ShowDialog() == DialogResult.OK)
            {
                m_FrameServer.SetFolderPath(folderDld.SelectedPath);
            }
        }

        private void cmbDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Todo: Add possibility to reconnect with another camera without closing the screen
            //       Now the Combo control is just disabled to prevent users from chainging


            if(cmbDeviceList.SelectedIndex >= 0)
            {
                m_CurrentCaptureDevice = (String)cmbDeviceList.Items[cmbDeviceList.SelectedIndex];
            } else {
                m_CurrentCaptureDevice = "";
            }
            cmbDeviceList.Enabled = false;
            tmrCaptureDeviceDetector_Tick(null, null);

        }

        private void tmrCaptureDuration_Tick(object sender, EventArgs e)
        {
            m_CaptureDuration =  m_CaptureDuration.AddMilliseconds((double)tmrCaptureDuration.Interval);

            lblSelDuration.Text = "Duration : " + m_CaptureDuration.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
	}
}
