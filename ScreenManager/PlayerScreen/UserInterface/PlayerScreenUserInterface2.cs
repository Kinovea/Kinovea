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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.VideoFiles;

#endregion

namespace Kinovea.ScreenManager
{
	public enum DrawingToolType
	{
		Pointer,
		Line2D,
		Cross2D,
		Angle2D,
		Pencil,
		Text,
		Chrono,
		NumberOfDrawingTools
	};
	
	public partial class PlayerScreenUserInterface : UserControl
	{
		#region Imports Win32
		[DllImport("winmm.dll", SetLastError = true)]
		private static extern uint timeSetEvent(int msDelay, int msResolution, TimerEventHandler handler, ref int userCtx, int eventType);

		[DllImport("winmm.dll", SetLastError = true)]
		private static extern uint timeKillEvent(uint timerEventId);

		const int TIME_PERIODIC         = 0x01;
		const int TIME_KILL_SYNCHRONOUS = 0x0100;

		[DllImport("user32.dll")]
		static extern uint GetDoubleClickTime();
		#endregion

		#region Internal delegates for async methods
		private delegate void TimerEventHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2);
		private delegate void CallbackPlayLoop();
        private Kinovea.ScreenManager.PlayerScreenUserInterface.TimerEventHandler m_CallbackTimerEventHandler;
        private Kinovea.ScreenManager.PlayerScreenUserInterface.CallbackPlayLoop m_CallbackPlayLoop;
		#endregion

		#region Enums
		private enum PlayingMode
		{
			Once,
			Loop,
			Bounce
		}
		#endregion

		#region Properties
		public bool IsCurrentlyPlaying
		{
			get { return m_bIsCurrentlyPlaying; }
		}
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
		public int SlowmotionPercentage
		{
			get { return m_iSlowmotionPercentage; }
		}
		public bool Synched
		{
			//get { return m_bSynched; }
			set 
			{ 
				m_bSynched = value;
				
				if(!m_bSynched)
				{
					m_iSyncPosition = 0;
					trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
					trkFrame.Invalidate();
					UpdateCurrentPositionLabel();
				}
			}
		}
		public Int64 SyncPosition
		{
			// The absolute ts of the sync point for this video.
			get { return m_iSyncPosition; }
			set 
			{ 
				m_iSyncPosition = value; 
				trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
				trkFrame.Invalidate();
				UpdateCurrentPositionLabel();
			}
		}
		public Int64 SyncCurrentPosition
		{
			// The current ts, relative to the selection.
			get { return m_iCurrentPosition - m_iSelStart; }
		}	
		#endregion

		#region Members
		private IPlayerScreenUIHandler m_PlayerScreenUIHandler;
		private FrameServerPlayer m_FrameServer;
		
		// General
		private PreferencesManager m_PrefManager = PreferencesManager.Instance();
		
		// Playback current state
		private bool m_bIsCurrentlyPlaying;
		private int m_iFramesToDecode = 1;
		private bool m_bSeekToStart;
		private bool m_bSynched;
		private Int64 m_iSyncPosition;
		private uint m_IdMultimediaTimer;
        private Kinovea.ScreenManager.PlayerScreenUserInterface.PlayingMode m_ePlayingMode = PlayingMode.Loop;
		private int m_iDroppedFrames;                  // For debug purposes only.
		private int m_iDecodedFrames;
		private int m_iSlowmotionPercentage = 100;
		private bool m_bIsIdle = true;

		// Image
		private bool m_bStretchModeOn;
		private bool m_bShowImageBorder;
		private static readonly Pen m_PenImageBorder = Pens.SteelBlue;
		
		// Selection (All values in TimeStamps)
		// trkSelection.minimum and maximum are also in absolute timestamps.
		private Int64 m_iTotalDuration = 100;
		private Int64 m_iSelStart;          	// Valeur absolue, par défaut égale à m_iStartingPosition. (pas 0)
		private Int64 m_iSelEnd = 99;          // Value absolue
		private Int64 m_iSelDuration = 100;
		private Int64 m_iCurrentPosition;    	// Valeur absolue dans l'ensemble des timestamps.
		private Int64 m_iStartingPosition;   	// Valeur absolue correspond au timestamp de la première frame.
		private bool m_bHandlersLocked;
		
		// Keyframes, Drawings, etc.
		private int m_iActiveKeyFrameIndex = -1;	// The index of the keyframe we are on, or -1 if not a KF.
		private DrawingToolType m_ActiveTool;
		private AbstractDrawingTool[] m_DrawingTools;
		private ColorProfile m_ColorProfile = new ColorProfile();
		private formKeyframeComments m_KeyframeCommentsHub;
		private bool m_bDocked = true;
		private bool m_bTextEdit;
		private bool m_bMeasuring;

		// Video Filters Management
		private bool m_bDrawtimeFiltered;
		private DrawtimeFilterOutput m_DrawingFilterOutput;
		
		// Others
		private Magnifier m_Magnifier = new Magnifier();
		private Double m_fHighSpeedFactor = 1.0f;           	// When capture fps is different from Playing fps.
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
		
		#region Context Menus
		private ContextMenuStrip popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuPlayPause = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSetCaptureSpeed = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSavePic = new ToolStripMenuItem();
		private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();

		private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
		private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
		private ToolStripMenuItem mnuConfigureFading = new ToolStripMenuItem();
		private ToolStripMenuItem mnuTrackTrajectory = new ToolStripMenuItem();
		private ToolStripMenuItem mnuGotoKeyframe = new ToolStripMenuItem();
		private ToolStripSeparator mnuSepDrawing = new ToolStripSeparator();
		private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
		private ToolStripMenuItem mnuShowMeasure = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSealMeasure = new ToolStripMenuItem();
		
		private ContextMenuStrip popMenuTrack = new ContextMenuStrip();
		private ToolStripMenuItem mnuRestartTracking = new ToolStripMenuItem();
		private ToolStripMenuItem mnuStopTracking = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDeleteTrajectory = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDeleteEndOfTrajectory = new ToolStripMenuItem();
		private ToolStripMenuItem mnuConfigureTrajectory = new ToolStripMenuItem();

		private ContextMenuStrip popMenuChrono = new ContextMenuStrip();
		private ToolStripMenuItem mnuChronoStart = new ToolStripMenuItem();
		private ToolStripMenuItem mnuChronoStop = new ToolStripMenuItem();
		private ToolStripMenuItem mnuChronoHide = new ToolStripMenuItem();
		private ToolStripMenuItem mnuChronoCountdown = new ToolStripMenuItem();
		private ToolStripMenuItem mnuChronoDelete = new ToolStripMenuItem();
		private ToolStripMenuItem mnuChronoConfigure = new ToolStripMenuItem();
		
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

		// Debug
		private bool m_bShowInfos;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor
		public PlayerScreenUserInterface(FrameServerPlayer _FrameServer, IPlayerScreenUIHandler _PlayerScreenUIHandler)
		{
			log.Debug("Constructing the PlayerScreen user interface.");
			
			m_PlayerScreenUIHandler = _PlayerScreenUIHandler;
			m_FrameServer = _FrameServer;
			m_FrameServer.Metadata = new Metadata(new GetTimeCode(TimeStampsToTimecode), new ShowClosestFrame(OnShowClosestFrame));
			
			InitializeComponent();
			BuildContextMenus();
			InitializeDrawingTools();
			
			// From prefs or command line.
			m_ColorProfile.Load(PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder") + "\\current.xml");
			
			CommandLineArgumentManager clam = CommandLineArgumentManager.Instance();
			if(!clam.SpeedConsumed)
			{ 
				sldrSpeed.Value = clam.SpeedPercentage;
				clam.SpeedConsumed = true;
			}
			
			// Most members and controls should be initialized with the right value.
			// So we don't need to do an extra ResetData here.
			
			// Controls that renders differently between run time and design time. 
			this.Dock = DockStyle.Fill;
			ShowHideResizers(false);
			SetupPrimarySelectionPanel();
			SetupKeyframeCommentsHub();
			pnlThumbnails.Controls.Clear();
			DockKeyframePanel();

			// Internal delegates
			m_CallbackTimerEventHandler = new TimerEventHandler(MultimediaTimerTick);
			m_CallbackPlayLoop = new CallbackPlayLoop(PlayLoop);

			//SetupDebugPanel();
		}
		#endregion
		
		#region Public Methods
		public void ResetToEmptyState()
		{
			// Called when we load a new video over an already loaded screen.
			// also recalled if the video loaded but the first frame cannot be displayed.

			// 1. Reset all data.
			m_FrameServer.Unload();
			ResetData();
			
			// 2. Reset all interface.
			ShowHideResizers(false);
			SetupPrimarySelectionPanel();
			pnlThumbnails.Controls.Clear();
			DockKeyframePanel();
			UpdateKeyframesMarkers();
			trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
			trkFrame.Invalidate();
			EnableDisableAllPlayingControls(true);
			EnableDisableDrawingTools(true);
			buttonPlay.BackgroundImage = Resources.liqplay17;
			sldrSpeed.Value = 100;
			sldrSpeed.Enabled = false;
			lblFileName.Text = "";
			m_KeyframeCommentsHub.Hide();
		}
		public int PostLoadProcess()
		{
			//---------------------------------------------------------------------------
			// Configure the interface according to he video and try to read first frame.
			// Called from CommandLoadMovie when VideoFile.Load() is successful.
			//---------------------------------------------------------------------------
			
			int iPostLoadResult = 0;

			// By default the filename of metadata will be the one of the video.
			m_FrameServer.Metadata.FullPath = m_FrameServer.VideoFile.FilePath;
			
			// Try to get MetaData from file.
			DemuxMetadata();
			
			// Try to display first frame.
			ReadResult readFrameResult  = ShowNextFrame(-1, true);
			UpdateNavigationCursor();

			if (readFrameResult != ReadResult.Success)
			{
				iPostLoadResult = -1;
				m_FrameServer.Unload();
				log.Error("First frame couldn't be loaded - aborting");
			}
			else
			{
				if (m_iCurrentPosition < 0)
				{
					// First frame loaded but inconsistency. (Seen with some AVCHD)
					log.Error(String.Format("First frame loaded but negative timestamp ({0}) - aborting", m_iCurrentPosition));
					iPostLoadResult = -2;
					m_FrameServer.Unload();
				}
				else
				{
					//---------------------------------------------------------------------------------------
					// First frame loaded finely.
					//
					// We will now update the internal data of the screen ui and
					// set up the various child controls (like the timelines).
					// Call order matters.
					// Some bugs come from variations between what the file infos advertised 
					// and the reality.
					// We fix what we can with the help of data read from the first frame or
					// from the analysis mode switch if successful.
					//---------------------------------------------------------------------------------------
					
					iPostLoadResult = 0;
					pbSurfaceScreen.Invalidate();

					//--------------------------------------------------------
					// 1. Internal data : timestamps. Controls : trkSelection.
					//
					// - Set tentatives timestamps from infos read in the file and first frame load.
					// - Try to switch to analysis mode.
					// - Update the tentative timestamps with more accurate data gotten from analysis mode.
					//--------------------------------------------------------
									
					//-----------------------------------------------------------------------------
					// [2008-04-26] Time stamp non 0 :Assez courant en fait.
					// La première frame peut avoir un timestamp à 1 au lieu de 0 selon l'encodeur.
					// Sans que cela soit répercuté sur iFirstTimeStamp...
					// On fixe à la main.
					//-----------------------------------------------------------------------------
					m_FrameServer.VideoFile.Infos.iFirstTimeStamp = m_iCurrentPosition;
					m_iStartingPosition = m_iCurrentPosition;
					m_iTotalDuration = m_FrameServer.VideoFile.Infos.iDurationTimeStamps;
					
					double fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
					m_iSelStart     = m_iStartingPosition;
					m_iSelEnd       = (long)((double)(m_iTotalDuration + m_iStartingPosition) - fAverageTimeStampsPerFrame);
					m_iSelDuration  = m_iTotalDuration;
					
					// TODO Remove following call
					//trkSelection.UpdateInternalState(m_iSelStart, m_iSelEnd, m_iSelStart, m_iSelEnd, m_iSelStart);

					// Switch to analysis mode if possible.
					// This will update the selection sentinels (m_iSelStart, m_iSelEnd) with more robust data.					
					ImportSelectionToMemory(false);

					m_iCurrentPosition = m_iSelStart;
					m_FrameServer.VideoFile.Infos.iFirstTimeStamp = m_iCurrentPosition;
					m_iStartingPosition = m_iCurrentPosition;
					m_iTotalDuration = m_iSelDuration;
					
					// Update the control.
					// FIXME - already done in ImportSelectionToMemory ?
					SetupPrimarySelectionPanel();
					
					//---------------------------------------------------
					// 2. Other various infos.
					//---------------------------------------------------
					m_iDecodedFrames = 1;
					m_iDroppedFrames = 0;
					m_bSeekToStart = false;
					
					m_FrameServer.SetupMetadata();
					((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetImageSize(m_FrameServer.Metadata.ImageSize);
					
					UpdateFilenameLabel();
					sldrSpeed.Enabled = true;

					//---------------------------------------------------
					// 3. Screen position and size.
					//---------------------------------------------------
					m_FrameServer.CoordinateSystem.SetOriginalSize(m_FrameServer.Metadata.ImageSize);
					m_FrameServer.CoordinateSystem.ReinitZoom();
					SetUpForNewMovie();
					m_KeyframeCommentsHub.UserActivated = false;

					//------------------------------------------------------------
					// 4. If metadata demux failed,
					// check if there is an brother analysis file in the directory
					//------------------------------------------------------------
					if (!m_FrameServer.Metadata.HasData)
					{
						LookForLinkedAnalysis();
					}
					
					// Do the post import whether the data come from external file or included xml.
					if (m_FrameServer.Metadata.HasData)
					{
						PostImportMetadata();
					}

					UpdateKeyframesMarkers();

					// Debug
					if (m_bShowInfos) { UpdateDebugInfos(); }
				}
			}
			
			return iPostLoadResult;
		}
		public void PostImportMetadata()
		{
			//----------------------------------------------------------
			// Analysis file or stream was imported into metadata.
			// Now we need to load each frames and do some scaling.
			//
			// Public because accessed from : 
			// 	ScreenManager upon loading standalone analysis.
			//----------------------------------------------------------

			// TODO - progress bar ?

			int iOutOfRange = -1;
			int iCurrentKeyframe = -1;

			foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
			{
				iCurrentKeyframe++;

				if (kf.Position < (m_FrameServer.VideoFile.Infos.iFirstTimeStamp + m_FrameServer.VideoFile.Infos.iDurationTimeStamps))
				{
					// Goto frame.
					m_iFramesToDecode = 1;
					ShowNextFrame(kf.Position, true);
					UpdateNavigationCursor();
					UpdateCurrentPositionLabel();
					trkSelection.SelPos = trkFrame.Position;

					// Readjust and complete the Keyframe
					kf.Position = m_iCurrentPosition;
					kf.ImportImage(m_FrameServer.VideoFile.CurrentImage);
					kf.GenerateDisabledThumbnail();

					// EditBoxes
					foreach (AbstractDrawing ad in kf.Drawings)
					{
						if (ad is DrawingText)
						{
							((DrawingText)ad).ContainerScreen = pbSurfaceScreen;
							panelCenter.Controls.Add(((DrawingText)ad).EditBox);
							((DrawingText)ad).EditBox.BringToFront();
						}
					}
				}
				else
				{
					// TODO - Alert box to inform that some images couldn't be matched.
					if (iOutOfRange < 0)
					{
						iOutOfRange = iCurrentKeyframe;
					}
				}
			}

			if (iOutOfRange != -1)
			{
				// Some keyframes were out of range. remove them.
				m_FrameServer.Metadata.Keyframes.RemoveRange(iOutOfRange, m_FrameServer.Metadata.Keyframes.Count - iOutOfRange);
			}

			UpdateFilenameLabel();
			OrganizeKeyframes();
			if(m_FrameServer.Metadata.Count > 0)
			{
				UndockKeyframePanel();
			}
			
			// Goto selection start and refresh.
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iSelStart, true);
			UpdateNavigationCursor();
			ActivateKeyframe(m_iCurrentPosition);

			m_FrameServer.SetupMetadata();
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetImageSize(m_FrameServer.Metadata.ImageSize);

			pbSurfaceScreen.Invalidate();
		}
		public void DisplayAsActiveScreen(bool _bActive)
		{
			// Actually called from ScreenManager.
			ShowBorder(_bActive);
		}
		public void StopPlaying()
		{
			StopPlaying(true);
		}
		public void SetCurrentFrame(Int64 _iFrame)
		{
			// Called during static sync.
			// Common position changed, we get a new frame to jump to.
			// target frame may be over the total.

			if (m_FrameServer.VideoFile.Loaded)
			{
				m_iFramesToDecode = 1;

				if (_iFrame == -1)
				{
					// Special case for +1 frame.
					if (m_iCurrentPosition < m_iSelEnd)
					{
						ShowNextFrame(-1, true);
					}
				}
				else
				{
					m_iCurrentPosition = _iFrame * m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame;
					m_iCurrentPosition += m_iSelStart;
					
					if (m_iCurrentPosition > m_iSelEnd) m_iCurrentPosition = m_iSelEnd;
					
					ShowNextFrame(m_iCurrentPosition, true);
				}

				UpdateNavigationCursor();
				UpdateCurrentPositionLabel();
				ActivateKeyframe(m_iCurrentPosition);

				trkSelection.SelPos = trkFrame.Position;

				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		public void RefreshImage()
		{
			// For cases where surfaceScreen.Invalidate() is not enough.
			if (m_FrameServer.VideoFile.Loaded)
			{
				ShowNextFrame(m_iCurrentPosition, true);
			}
		}
		public void RefreshUICulture()
		{
			// Labels
			lblSelStartSelection.AutoSize = true;
			lblSelDuration.AutoSize = true;

			lblWorkingZone.Text = ScreenManagerLang.lblWorkingZone_Text;
			UpdateSpeedLabel();
			UpdateSelectionLabels();
			UpdateCurrentPositionLabel();
			
			lblSpeedTuner.Left = lblTimeCode.Left + lblTimeCode.Width + 8;
			sldrSpeed.Left = lblSpeedTuner.Left + lblSpeedTuner.Width + 8;
			
			ReloadTooltipsCulture();
			ReloadMenusCulture();
			m_KeyframeCommentsHub.RefreshUICulture();

			// Because this method is called when we change the general preferences,
			// we can use it to update data too.
			
			// Keyframes positions.
			if (m_FrameServer.Metadata.Count > 0)
			{
				EnableDisableKeyframes();
			}

			// Refresh image to update timecode in chronos, grids colors, default fading, etc.
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
				case Keys.Return:
					{
						OnButtonPlay();
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
				case Keys.Left:
					{
						if ((ModifierKeys & Keys.Control) == Keys.Control)
						{
							// Previous keyframe
							GotoPreviousKeyframe();
						}
						else
						{
							if (((ModifierKeys & Keys.Shift) == Keys.Shift) && m_iCurrentPosition <= m_iSelStart)
							{
								// Shift + Left on first = loop backward.
								buttonGotoLast_Click(null, EventArgs.Empty);
							}
							else
							{
								// Previous frame
								buttonGotoPrevious_Click(null, EventArgs.Empty);
							}
						}
						bWasHandled = true;
						break;
					}
				case Keys.Right:
					{
						if ((ModifierKeys & Keys.Control) == Keys.Control)
						{
							// Next keyframe
							GotoNextKeyframe();
						}
						else
						{
							// Next frame
							buttonGotoNext_Click(null, EventArgs.Empty);
						}
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
				case Keys.F6:
					{
						AddKeyframe();
						bWasHandled = true;
						break;
					}
				case Keys.F7:
					{
						// Unused.
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
						if ((ModifierKeys & Keys.Control) == Keys.Control)
						{
							// Remove Keyframe
							if (m_iActiveKeyFrameIndex >= 0)
							{
								RemoveKeyframe(m_iActiveKeyFrameIndex);
							}
						}
						else
						{
							// Remove selected Drawing
							// Note: Should only work if the Drawing is currently being moved...
							DeleteSelectedDrawing();
						}
						bWasHandled = true;
						break;
					}
				case Keys.End:
					{
						buttonGotoLast_Click(null, EventArgs.Empty);
						bWasHandled = true;
						break;
					}
				case Keys.Home:
					{
						buttonGotoFirst_Click(null, EventArgs.Empty);
						bWasHandled = true;
						break;
					}
				case Keys.Down:
				case Keys.Up:
					{
						sldrSpeed_KeyDown(null, new KeyEventArgs(_keycode));
						bWasHandled = true;
						break;
					}
				default:
					break;
			}

			return bWasHandled;
		}
		public void UpdateImageSize()
		{
			Size imageSize = new Size(m_FrameServer.VideoFile.Infos.iDecodingWidth, m_FrameServer.VideoFile.Infos.iDecodingHeight);
			
			m_FrameServer.Metadata.ImageSize = imageSize;
			
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).SetImageSize(m_FrameServer.Metadata.ImageSize);
			
			m_FrameServer.CoordinateSystem.SetOriginalSize(m_FrameServer.Metadata.ImageSize);
			m_FrameServer.CoordinateSystem.ReinitZoom();
						
			StretchSqueezeSurface();
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
		private void ResetData()
		{
			m_iFramesToDecode = 1;
			
			m_iSlowmotionPercentage = 100;
			m_bDrawtimeFiltered = false;
			m_bIsCurrentlyPlaying = false;
			m_bSeekToStart = false;
			m_bSynched = false;
			m_iSyncPosition = 0;
			m_ePlayingMode = PlayingMode.Loop;
			m_bStretchModeOn = false;
			m_FrameServer.CoordinateSystem.Reset();
			
		
			m_bShowImageBorder = false;
		
			SetupPrimarySelectionData(); 	// Should not be necessary when every data is coming from m_FrameServer.
		
			m_bHandlersLocked = false;
		
			m_iActiveKeyFrameIndex = -1;
			m_ActiveTool = DrawingToolType.Pointer;
		
			m_ColorProfile.Load(PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder") + "\\current.xml");
			
			m_bDocked = true;
			m_bTextEdit = false;
			m_bMeasuring = false;
			
			m_bDrawtimeFiltered = false;;
		
			m_Magnifier.ResetData();
			
			m_fHighSpeedFactor = 1.0f;			
		}
		private void DemuxMetadata()
		{	
			// Try to find metadata muxed inside the file and load it.
			
			String metadata = m_FrameServer.VideoFile.ReadMetadata();
			
			if (metadata != null)
			{
				// TODO - save previous metadata for undo.
				m_FrameServer.Metadata = Metadata.FromXmlString(	metadata, 
				                                                	m_FrameServer.VideoFile.Infos.iDecodingWidth, 
				                                                	m_FrameServer.VideoFile.Infos.iDecodingHeight, 
				                                                	m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame, 
				                                                	m_FrameServer.VideoFile.FilePath, 
				                                                	new GetTimeCode(TimeStampsToTimecode), 
				                                                	new ShowClosestFrame(OnShowClosestFrame));
				UpdateKeyframesMarkers();
				OrganizeKeyframes();
			}
		}
		private void SetupPrimarySelectionData()
		{
			// Setup data
			if (m_FrameServer.VideoFile.Loaded)
			{
				double  fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
				m_iSelStart = m_iStartingPosition;
				m_iSelEnd = (long)((double)(m_iTotalDuration + m_iStartingPosition) - fAverageTimeStampsPerFrame);
				m_iSelDuration = m_iTotalDuration;
			}
			else
			{
				m_iSelStart = 0;
				m_iSelEnd = 99;
				m_iSelDuration = 100;
				m_iTotalDuration = 100;
				
				m_iCurrentPosition = 0;
				m_iStartingPosition = 0;
			}
		}
		private void SetupPrimarySelectionPanel()
		{
			// Setup controls & labels.
			// Update internal state only, doesn't trigger the events.
			trkSelection.UpdateInternalState(m_iSelStart, m_iSelEnd, m_iSelStart, m_iSelEnd, m_iSelStart);
			UpdateSelectionLabels();
		}
		private void SetUpForNewMovie()
		{
			// Problem: The screensurface hasn't got its final size...
			// So it doesn't make much sense to call it here...
			ShowHideResizers(true);
			StretchSqueezeSurface();
			
			// Since it hadn't its final size, we don't really know if the pic is too large...
			m_bStretchModeOn = false;
			OnPoke();
		}
		private void SetupKeyframeCommentsHub()
		{
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.MakeTopMost != null)
			{
				m_KeyframeCommentsHub = new formKeyframeComments(this);
				dp.MakeTopMost(m_KeyframeCommentsHub);
			}
		}
		private void LookForLinkedAnalysis()
		{
			// Look for an Anlaysis with the same file name in the same directory.

			// Complete path of hypothetical Analysis.
			string kvaFile = Path.GetDirectoryName(m_FrameServer.VideoFile.FilePath);
			kvaFile = kvaFile + "\\" + Path.GetFileNameWithoutExtension(m_FrameServer.VideoFile.FilePath) + ".kva";
			
			if (File.Exists(kvaFile))
			{
				m_FrameServer.Metadata.LoadFromFile(kvaFile);
			}			
		}
		private void UpdateFilenameLabel()
		{
			lblFileName.Text = Path.GetFileName(m_FrameServer.VideoFile.FilePath);
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
			mnuPlayPause.Click += new EventHandler(buttonPlay_Click);
			mnuSetCaptureSpeed.Click += new EventHandler(mnuSetCaptureSpeed_Click);
			mnuSavePic.Click += new EventHandler(btnSnapShot_Click);
			mnuCloseScreen.Click += new EventHandler(btnClose_Click);
			popMenu.Items.AddRange(new ToolStripItem[] { mnuPlayPause, mnuSetCaptureSpeed, mnuSavePic, new ToolStripSeparator(), mnuCloseScreen });

			// 2. Drawings context menu (Configure, Delete, Track this)
			mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);
			mnuConfigureFading.Click += new EventHandler(mnuConfigureFading_Click);
			mnuTrackTrajectory.Click += new EventHandler(mnuTrackTrajectory_Click);
			mnuGotoKeyframe.Click += new EventHandler(mnuGotoKeyframe_Click);
			mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);
			mnuShowMeasure.Click += new EventHandler(mnuShowMeasure_Click);
			mnuSealMeasure.Click += new EventHandler(mnuSealMeasure_Click);
			popMenuDrawings.Items.AddRange(new ToolStripItem[] { mnuConfigureDrawing, mnuConfigureFading, new ToolStripSeparator(), mnuTrackTrajectory, mnuShowMeasure, mnuSealMeasure, mnuGotoKeyframe, mnuSepDrawing, mnuDeleteDrawing });

			// 3. Tracking pop menu (Restart, Stop tracking)
			mnuStopTracking.Click += new EventHandler(mnuStopTracking_Click);
			mnuStopTracking.Visible = false;
			mnuRestartTracking.Click += new EventHandler(mnuRestartTracking_Click);
			mnuRestartTracking.Visible = false;
			mnuDeleteTrajectory.Click += new EventHandler(mnuDeleteTrajectory_Click);
			mnuDeleteEndOfTrajectory.Click += new EventHandler(mnuDeleteEndOfTrajectory_Click);
			mnuConfigureTrajectory.Click += new EventHandler(mnuConfigureTrajectory_Click);
			popMenuTrack.Items.AddRange(new ToolStripItem[] { mnuConfigureTrajectory, new ToolStripSeparator(), mnuStopTracking, mnuRestartTracking, new ToolStripSeparator(), mnuDeleteEndOfTrajectory, mnuDeleteTrajectory });

			// 4. Chrono pop menu (Start, Stop, Hide, etc.)
			mnuChronoConfigure.Click += new EventHandler(mnuChronoConfigure_Click);
			mnuChronoStart.Click += new EventHandler(mnuChronoStart_Click);
			mnuChronoStop.Click += new EventHandler(mnuChronoStop_Click);
			mnuChronoCountdown.Click += new EventHandler(mnuChronoCountdown_Click);
			mnuChronoCountdown.Checked = false;
			mnuChronoCountdown.Enabled = false;
			mnuChronoHide.Click += new EventHandler(mnuChronoHide_Click);
			mnuChronoDelete.Click += new EventHandler(mnuChronoDelete_Click);
			popMenuChrono.Items.AddRange(new ToolStripItem[] { mnuChronoConfigure, new ToolStripSeparator(), mnuChronoStart, mnuChronoStop, mnuChronoCountdown, new ToolStripSeparator(), mnuChronoHide, mnuChronoDelete, });

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
		private void SetupDebugPanel()
		{
			m_bShowInfos = true;
			panelDebug.Left = 0;
			panelDebug.Width = 180;
			panelDebug.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			panelDebug.BackColor = Color.Black;
		}
		#endregion
		
		#region Misc Events
		private void btnClose_Click(object sender, EventArgs e)
		{
			// Propagate to PlayerScreen which will report to ScreenManager.			
			m_PlayerScreenUIHandler.ScreenUI_CloseAsked();
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
			
			m_PlayerScreenUIHandler.ScreenUI_SetAsActiveScreen();
			
			// 1. Ensure no DrawingText is in edit mode.
			m_FrameServer.Metadata.AllDrawingTextToNormalMode();

			// 2. Return to the pointer tool, except if Pencil
			if (m_ActiveTool != DrawingToolType.Pencil)
			{
				m_ActiveTool = DrawingToolType.Pointer;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, -1));
			}

			// 3. Dock Keyf panel if nothing to see.
			if (m_FrameServer.Metadata.Count < 1)
			{
				DockKeyframePanel();
			}
		}
		private string TimeStampsToTimecode(long _iTimeStamp, TimeCodeFormat _timeCodeFormat, bool _bSynched)
		{
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

			if (m_FrameServer.VideoFile.Loaded)
				iSeconds = (double)iTimeStamp / m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds;
			else
				iSeconds = 0;

			// m_fSlowFactor is different from 1.0f only when user specify that the capture fps
			// was different than the playing fps. We readjust time.
			double iMilliseconds = (iSeconds * 1000) / m_fHighSpeedFactor;
			
			// If there are more than 100 frames per seconds, we display milliseconds.
			// This can happen when the user manually tune the input fps.
			bool bShowThousandth = (m_fHighSpeedFactor *  m_FrameServer.VideoFile.Infos.fFps >= 100);
			
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

			return outputTimeCode;
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
			// Called from CommandAddDrawing.Unexecute().
			//--------------------------------------------------------

			// Return to the pointer tool unless we were drawing.
			if (m_ActiveTool != DrawingToolType.Pencil)
			{
				m_ActiveTool = DrawingToolType.Pointer;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			}
		}
		#endregion

		#region Debug Helpers
		private void UpdateDebugInfos()
		{
			panelDebug.Visible = true;

			dbgDurationTimeStamps.Text = String.Format("TotalDuration (ts): {0:0}", m_iTotalDuration);
			dbgFFps.Text = String.Format("Fps Avg (f): {0:0.00}", m_FrameServer.VideoFile.Infos.fFps);
			dbgSelectionStart.Text = String.Format("SelStart (ts): {0:0}", m_iSelStart);
			dbgSelectionEnd.Text = String.Format("SelEnd (ts): {0:0}", m_iSelEnd);
			dbgSelectionDuration.Text = String.Format("SelDuration (ts): {0:0}", m_iSelDuration);
			dbgCurrentPositionAbs.Text = String.Format("CurrentPosition (abs, ts): {0:0}", m_iCurrentPosition);
			dbgCurrentPositionRel.Text = String.Format("CurrentPosition (rel, ts): {0:0}", m_iCurrentPosition-m_iSelStart);
			dbgStartOffset.Text = String.Format("StartOffset (ts): {0:0}", m_FrameServer.VideoFile.Infos.iFirstTimeStamp);
			dbgDrops.Text = String.Format("Drops (f): {0:0}", m_iDroppedFrames);

			dbgCurrentFrame.Text = String.Format("CurrentFrame (f): {0}", m_FrameServer.VideoFile.Selection.iCurrentFrame);
			dbgDurationFrames.Text = String.Format("Duration (f) : {0}", m_FrameServer.VideoFile.Selection.iDurationFrame);

			panelDebug.Invalidate();
		}
		private void panelDebug_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			UpdateDebugInfos();
			log.Debug("");
			log.Debug("Timestamps Full Dump");
			log.Debug("--------------------");
			double fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
			log.Debug("Average ts per frame     : " + fAverageTimeStampsPerFrame);
			log.Debug("");
			log.Debug("m_iStartingPosition      : " + m_iStartingPosition);
			log.Debug("m_iTotalDuration         : " + m_iTotalDuration);
			log.Debug("m_iCurrentPosition       : " + m_iCurrentPosition);
			log.Debug("");
			log.Debug("m_iSelStart              : " + m_iSelStart);
			log.Debug("m_iSelEnd                : " + m_iSelEnd);
			log.Debug("m_iSelDuration           : " + m_iSelDuration);
			log.Debug("");
			log.Debug("trkSelection.Minimum     : " + trkSelection.Minimum);
			log.Debug("trkSelection.Maximum     : " + trkSelection.Maximum);
			log.Debug("trkSelection.SelStart    : " + trkSelection.SelStart);
			log.Debug("trkSelection.SelEnd      : " + trkSelection.SelEnd);
			log.Debug("trkSelection.SelPos      : " + trkSelection.SelPos);
			log.Debug("");
			log.Debug("trkFrame.Minimum         : " + trkFrame.Minimum);
			log.Debug("trkFrame.Maximum         : " + trkFrame.Maximum);
			log.Debug("trkFrame.Position        : " + trkFrame.Position);
		}
		#endregion

		#region Video Controls

		#region Playback Controls
		public void buttonGotoFirst_Click(object sender, EventArgs e)
		{
			// Jump to start.
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();
				
				m_iFramesToDecode = 1;
				ShowNextFrame(m_iSelStart, true);
				
				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);

				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		public void buttonGotoPrevious_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();

				//---------------------------------------------------------------------------
				// Si on est en dehors de la zone primaire, ou qu'on va en sortir,
				// se replacer au début de celle-ci.
				//---------------------------------------------------------------------------
				if ((m_iCurrentPosition <= m_iSelStart) || (m_iCurrentPosition > m_iSelEnd))
				{
					m_iFramesToDecode = 1;
					ShowNextFrame(m_iSelStart, true);
				}
				else
				{
					//--------------------------------------------------------------------------------------
					// Il est possible que la frame sur laquelle on est positionné possède un décalage en TimeStamps
					// avec la précédente qui soit supérieur à la moyenne.
					//
					// Dans un tel cas, on va demander un timestamp entre les deux frames.
					// Après seek+decode, en arrivant sur la vraie frame précédente,
					// on ne sera toujours pas rendu et on va faire un nouveau décode,
					// qui va nous ramener sur la frame courante.
					// -> On ne pourra pas reculer de la frame courante.
					//
					// -> Detecter un tel cas, et forcer un jump arrière plus grand.
					// Peut-être long si le seek nous fait tomber très loin en arrière ET
					// que l'intervalle entre les deux frames est très supérieur à la normale.(necessite plusieurs tentatives)
					// (Devrait rester rare).
					//--------------------------------------------------------------------------------------

					//double  fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
					//Int64   iAverageTimeStampsPerFrame = (Int64)Math.Round(fAverageTimeStampsPerFrame);

					/*Int64 iOldCurrentPosition = m_iCurrentPosition;
                    int iBackJump = 1;


                    //problème sur certains fichiers.
                    //while (m_iCurrentPosition >= iOldCurrentPosition)
                    {
                        ShowNextFrame(m_iCurrentPosition - (iBackJump * iAverageTimeStampsPerFrame), true);
                        iBackJump++;
                    }*/

					m_iFramesToDecode = -1;
					ShowNextFrame(-1, true);
					m_iFramesToDecode = 1;
				}
				
				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);
				
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
			
		}
		private void buttonPlay_Click(object sender, EventArgs e)
		{
			//----------------------------------------------------------------------------
			// L'appui sur le bouton play ne fait qu'activer ou désactiver le Timer
			// La lecture est ensuite automatique et c'est dans la fonction du Timer
			// que l'on gère la NextFrame à afficher en fonction du ralentit,
			// du mode de bouclage etc...
			//----------------------------------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				OnButtonPlay();
			}
		}
		public void buttonGotoNext_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();
				m_iFramesToDecode = 1;

				//---------------------------------------------------------------------------
				// Si on est en dehors de la zone primaire, ou qu'on va en sortir,
				// se replacer au début de celle-ci.
				//---------------------------------------------------------------------------
				if ((m_iCurrentPosition < m_iSelStart) || (m_iCurrentPosition >= m_iSelEnd))
				{
					ShowNextFrame(m_iSelStart, true);
				}
				else
				{
					ShowNextFrame(-1, true);
				}

				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
			
		}
		public void buttonGotoLast_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();

				m_iFramesToDecode = 1;
				ShowNextFrame(m_iSelEnd, true);

				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		public void OnButtonPlay()
		{
			//--------------------------------------------------------------
			// This function is accessed from ScreenManager.
			// Eventually from a worker thread. (no SetAsActiveScreen here).
			//--------------------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				if (m_bIsCurrentlyPlaying)
				{
					// Go into Pause mode.
					StopPlaying();
					buttonPlay.BackgroundImage = Resources.liqplay17;
					m_bIsCurrentlyPlaying = false;
					ActivateKeyframe(m_iCurrentPosition);
				}
				else
				{
					// Go into Play mode
					buttonPlay.BackgroundImage = Resources.liqpause6;
					Application.Idle += new EventHandler(this.IdleDetector);
					StartMultimediaTimer(GetPlaybackFrameInterval());
					m_bIsCurrentlyPlaying = true;
				}
			}
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
				if (iScrollOffset > 0)
				{
					buttonGotoNext_Click(null, EventArgs.Empty);
				}
				else
				{
					if (((ModifierKeys & Keys.Shift) == Keys.Shift) && m_iCurrentPosition <= m_iSelStart)
					{
						// Shift + Left on first = loop backward.
						buttonGotoLast_Click(null, EventArgs.Empty);
					}
					else
					{
						buttonGotoPrevious_Click(null, EventArgs.Empty);
					}
				}
			}
		}
		#endregion

		#region Working Zone Selection
		private void trkSelection_SelectionChanging(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				StopPlaying();

				// Update selection timestamps and labels.
				UpdateSelectionDataFromControl();
				UpdateSelectionLabels();

				// Update the frame tracker internal timestamps (including position if needed).
				trkFrame.Remap(m_iSelStart, m_iSelEnd);
				
				// Update the position but don't trigger a refresh.
				trkSelection.UpdatePositionValueOnly(trkFrame.Position);

				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		private void trkSelection_SelectionChanged(object sender, EventArgs e)
		{
			// Actual update.
			if (m_FrameServer.VideoFile.Loaded)
			{
				UpdateSelectionDataFromControl();
				ImportSelectionToMemory(false);

				m_FrameServer.Metadata.SelectionStart = m_iSelStart;
				UpdateKeyframesMarkers();

				OnPoke();
				m_PlayerScreenUIHandler.PlayerScreenUI_SelectionChanged(false);

				// Update the visible frame if needed.
				UpdateFramePrimarySelection();
				EnableDisableKeyframes();
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void trkSelection_TargetAcquired(object sender, EventArgs e)
		{
			// User clicked inside selection: Jump to position.
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();
				m_iFramesToDecode = 1;

				ShowNextFrame(trkSelection.SelTarget, true);
				m_iCurrentPosition = trkSelection.SelTarget + trkSelection.Minimum;

				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
			
		}
		private void btn_HandlersLock_Click(object sender, EventArgs e)
		{
			// Lock the selection handlers.
			if (m_FrameServer.VideoFile.Loaded)
			{
				m_bHandlersLocked = !m_bHandlersLocked;
				trkSelection.SelLocked = m_bHandlersLocked;

				// Update UI accordingly.
				if (m_bHandlersLocked)
				{
					btn_HandlersLock.Image = Resources.primselec_locked3;
					toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionUnlock);
				}
				else
				{
					btn_HandlersLock.Image = Resources.primselec_unlocked3;
					toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionLock);
				}
			}
		}
		private void btnSetHandlerLeft_Click(object sender, EventArgs e)
		{
			// Set the left handler of the selection at the current frame.
			if (m_FrameServer.VideoFile.Loaded && !m_bHandlersLocked)
			{
				trkSelection.SelStart = m_iCurrentPosition;
				UpdateSelectionDataFromControl();
				UpdateSelectionLabels();
				trkFrame.Remap(m_iSelStart,m_iSelEnd);
				ImportSelectionToMemory(false);
			}
		}
		private void btnSetHandlerRight_Click(object sender, EventArgs e)
		{
			// Set the right handler of the selection at the current frame.
			if (m_FrameServer.VideoFile.Loaded && !m_bHandlersLocked)
			{
				trkSelection.SelEnd = m_iCurrentPosition;
				UpdateSelectionDataFromControl();
				UpdateSelectionLabels();
				trkFrame.Remap(m_iSelStart,m_iSelEnd);
				ImportSelectionToMemory(false);
			}
		}
		private void btnHandlersReset_Click(object sender, EventArgs e)
		{
			// Reset both selection sentinels to their max values. 
			if (m_FrameServer.VideoFile.Loaded && !m_bHandlersLocked)
			{
				trkSelection.Reset();
				UpdateSelectionDataFromControl();
				
				// We need to force the reloading of all frames.
				ImportSelectionToMemory(true);
				
				// Update everything as if we moved the handlers manually.
				m_FrameServer.Metadata.SelectionStart = m_iSelStart;
				UpdateKeyframesMarkers();
				OnPoke();
				m_PlayerScreenUIHandler.PlayerScreenUI_SelectionChanged(false);

				// Update current image and keyframe  status.
				UpdateFramePrimarySelection();
				EnableDisableKeyframes();
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		
		private void UpdateFramePrimarySelection()
		{
			//--------------------------------------------------------------
			// Update the visible image to reflect the new selection.
			// Cheks that the previous current frame is still within selection,
			// jumps to closest sentinel otherwise.
			//--------------------------------------------------------------
			
			if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
			{
				// In analysis mode, we always refresh the current frame.
				ShowNextFrame(m_FrameServer.VideoFile.Selection.iCurrentFrame, true);
			}
			else
			{
				if ((m_iCurrentPosition >= m_iSelStart) && (m_iCurrentPosition <= m_iSelEnd))
				{
					// Nothing more to do.
				}
				else
				{
					m_iFramesToDecode = 1;

					// Currently visible frame is not in selection, force refresh. 
					if (m_iCurrentPosition < m_iSelStart)
					{
						// Was before start: goto start.
						ShowNextFrame(m_iSelStart, true);
					}
					else
					{
						// Was after end: goto end.
						ShowNextFrame(m_iSelEnd, true);
					}
				}
			}

			UpdateNavigationCursor();
			if (m_bShowInfos) UpdateDebugInfos();
		}
		private void UpdateSelectionLabels()
		{
			lblSelStartSelection.Text = ScreenManagerLang.lblSelStartSelection_Text + " : " + TimeStampsToTimecode(m_iSelStart - m_iStartingPosition, m_PrefManager.TimeCodeFormat, false);
			lblSelDuration.Text = ScreenManagerLang.lblSelDuration_Text + " : " + TimeStampsToTimecode(m_iSelDuration, m_PrefManager.TimeCodeFormat, false);
		}
		private void UpdateSelectionDataFromControl()
		{
			// Update WorkingZone data according to control.
			if ((m_iSelStart != trkSelection.SelStart) || (m_iSelEnd != trkSelection.SelEnd))
			{
				m_iSelStart = trkSelection.SelStart;
				m_iSelEnd = trkSelection.SelEnd;
				double fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
				m_iSelDuration = m_iSelEnd - m_iSelStart + (long)fAverageTimeStampsPerFrame;
			}
		}
		#endregion
		
		#region Frame Tracker
		private void trkFrame_PositionChanging(object sender, long _iPosition)
		{
			//---------------------------------------------------
			// Appelée lors de déplacement de type MouseMove
			// UNIQUEMENT si mode Analyse.
			//---------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				//SetAsActiveScreen();
				//StopPlaying();

				// Mettre à jour l'image, mais ne pas toucher au curseur.
				UpdateFrameCurrentPosition(false);
				UpdateCurrentPositionLabel();

				// May be expensive ?
				ActivateKeyframe(m_iCurrentPosition);

				// Mise à jour de l'indicateur sur le frame
				//trkSelection.SelPos = trkFrame.Position;
			}
		}
		private void trkFrame_PositionChanged(object sender, long _iPosition)
		{
			//---------------------------------------------------
			// Appelée uniquement lors de déplacement automatique
			// MouseUp, DoubleClick
			//---------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();

				// Mettre à jour l'image, ET le CURSEUR.
				UpdateFrameCurrentPosition(true);
				UpdateCurrentPositionLabel();
				ActivateKeyframe(m_iCurrentPosition);

				// Mise à jour de l'indicateur sur le frame
				trkSelection.SelPos = trkFrame.Position;
			}
		}
		private void UpdateFrameCurrentPosition(bool _bUpdateNavCursor)
		{
			//--------------------------------------------------------------
			// Affiche l'image correspondant à la position courante dans la selection primaire
			// Sur intervention manuelle de l'utilisateur ou au chargement.
			// ( = Le curseur a bougé, afficher l'image)
			//--------------------------------------------------------------

			if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 0)
			{
				this.Cursor = Cursors.WaitCursor;
			}

			m_iCurrentPosition = trkFrame.Position;
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iCurrentPosition, true);

			if (_bUpdateNavCursor) { UpdateNavigationCursor();}
			if (m_bShowInfos) { UpdateDebugInfos(); }

			if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 0)
			{
				this.Cursor = Cursors.Default;
			}
			
		}
		private void UpdateCurrentPositionLabel()
		{
			//-----------------------------------------------------------------
			// Format d'affichage : Standard TimeCode.
			// Heures:Minutes:Secondes.Frames
			// Position relative à la Selection Primaire / Zone de travail
			//-----------------------------------------------------------------
			string timecode = TimeStampsToTimecode(m_iCurrentPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, m_bSynched);
			lblTimeCode.Text = ScreenManagerLang.lblTimeCode_Text + " : " + timecode;
			lblTimeCode.Invalidate();
		}
		private void UpdateNavigationCursor()
		{
			// Update cursor position after Resize, ShowNextFrame, Working Zone change.
			trkFrame.Position = m_iCurrentPosition;
			trkSelection.SelPos = trkFrame.Position;
			UpdateCurrentPositionLabel();
		}
		private void UpdateKeyframesMarkers()
		{
			long[] ts = new long[m_FrameServer.Metadata.Keyframes.Count];
			for (int i = 0; i < ts.Length; i++)
			{
				// Selection Start will be taken care of directly in the FrameTracker.
				ts[i] = m_FrameServer.Metadata.Keyframes[i].Position;
			}
			trkFrame.UpdateKeyframesMarkers(ts);
		}
		#endregion

		#region Speed Slider
		private void sldrSpeed_ValueChanged(object sender, EventArgs e)
		{
			m_iSlowmotionPercentage = sldrSpeed.Value > 0 ? sldrSpeed.Value : 1;
			
			if (m_FrameServer.VideoFile.Loaded)
			{
				// Reset timer with new value.
				if (m_bIsCurrentlyPlaying)
				{
					StopMultimediaTimer();
					StartMultimediaTimer(GetPlaybackFrameInterval());
				}

				// Impacts synchro.
				m_PlayerScreenUIHandler.PlayerScreenUI_IsReady(true);
			}

			UpdateSpeedLabel();
		}
		private void sldrSpeed_KeyDown(object sender, KeyEventArgs e)
		{
			// Increase/Decrease speed on UP/DOWN Arrows. 
			
			if (m_FrameServer.VideoFile.Loaded)
			{
				if (e.KeyCode == Keys.Down)
				{
					// If Control is pressed, jump to the next 25% spot.
					if( (ModifierKeys & Keys.Control) == Keys.Control)
					{
						sldrSpeed.Value = 25 * ((sldrSpeed.Value-1) / 25);
					}
					else if (sldrSpeed.Value >= sldrSpeed.Minimum + sldrSpeed.SmallChange)
					{
						sldrSpeed.Value -= sldrSpeed.SmallChange;
					}

					e.Handled = true;
				}

				if (e.KeyCode == Keys.Up)
				{
					// If Control is pressed, jump to the next 25% spot.
					if( (ModifierKeys & Keys.Control) == Keys.Control)
					{
						sldrSpeed.Value = 25 * ((sldrSpeed.Value / 25) + 1);
					}
					else if (sldrSpeed.Value <= sldrSpeed.Maximum - sldrSpeed.SmallChange)
					{
						sldrSpeed.Value+=sldrSpeed.SmallChange;
					}
					e.Handled = true;
				}
			}
		}
		private void lblSpeedTuner_DoubleClick(object sender, EventArgs e)
		{
			// Double click on the speed label : Back to 100%
			sldrSpeed.Value = sldrSpeed.StickyValue;
		}
		private void UpdateSpeedLabel()
		{
			lblSpeedTuner.Text = ScreenManagerLang.lblSpeedTuner_Text + " " + m_iSlowmotionPercentage + "%";
		}
		#endregion

		#region Loop Mode
		private void buttonPlayingMode_Click(object sender, EventArgs e)
		{
			// Playback mode ('Once' or 'Loop').
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();

				if (m_ePlayingMode == PlayingMode.Once)
				{
					// Was in 'Once' mode, switch to 'Loop' mode.
					m_ePlayingMode = PlayingMode.Loop;
					buttonPlayingMode.Image = Resources.playmodeloop;
					toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Loop);
				}
				else if (m_ePlayingMode == PlayingMode.Loop)
				{
					// Was in 'Loop' mode, switch to 'Once' mode.
					m_ePlayingMode = PlayingMode.Once;
					buttonPlayingMode.Image = Resources.playmodeonce;
					toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Once);
				}
			}
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
			
			if (m_FrameServer.Loaded)
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
				int iTargetHeight = (int)((double)m_FrameServer.VideoFile.Infos.iDecodingHeight * m_FrameServer.CoordinateSystem.Stretch);
				int iTargetWidth = (int)((double)m_FrameServer.VideoFile.Infos.iDecodingWidth * m_FrameServer.CoordinateSystem.Stretch);
				
				if (iTargetHeight > panelCenter.Height || iTargetWidth > panelCenter.Width)
				{
					if (m_FrameServer.CoordinateSystem.Stretch > 1.0)
					{
						m_bStretchModeOn = true;
					}
				}
				
				if ((m_bStretchModeOn) || (m_FrameServer.VideoFile.Infos.iDecodingWidth > panelCenter.Width) || (m_FrameServer.VideoFile.Infos.iDecodingHeight > panelCenter.Height))
				{
					//-------------------------------------------------------------------------------
					// Maximiser :
					//Redimensionner l'image selon la dimension la plus proche de la taille du panel.
					//-------------------------------------------------------------------------------
					float WidthRatio = (float)m_FrameServer.VideoFile.Infos.iDecodingWidth / panelCenter.Width;
					float HeightRatio = (float)m_FrameServer.VideoFile.Infos.iDecodingHeight / panelCenter.Height;
					
					if (WidthRatio > HeightRatio)
					{
						pbSurfaceScreen.Width = panelCenter.Width;
						pbSurfaceScreen.Height = (int)((float)m_FrameServer.VideoFile.Infos.iDecodingHeight / WidthRatio);
						
						m_FrameServer.CoordinateSystem.Stretch = (1 / WidthRatio);
					}
					else
					{
						pbSurfaceScreen.Width = (int)((float)m_FrameServer.VideoFile.Infos.iDecodingWidth / HeightRatio);
						pbSurfaceScreen.Height = panelCenter.Height;
						
						m_FrameServer.CoordinateSystem.Stretch = (1 / HeightRatio);
					}
				}
				else
				{
					
					pbSurfaceScreen.Width = (int)((double)m_FrameServer.VideoFile.Infos.iDecodingWidth * m_FrameServer.CoordinateSystem.Stretch);
					pbSurfaceScreen.Height = (int)((double)m_FrameServer.VideoFile.Infos.iDecodingHeight * m_FrameServer.CoordinateSystem.Stretch);
				}
				
				//recentrer
				pbSurfaceScreen.Left = (panelCenter.Width / 2) - (pbSurfaceScreen.Width / 2);
				pbSurfaceScreen.Top = (panelCenter.Height / 2) - (pbSurfaceScreen.Height / 2);
				
				// Repositionement des Resizers.
				ReplaceResizers();
				
				// Redéfinir les plans & grilles 3D
				Size imageSize = new Size(m_FrameServer.VideoFile.Infos.iDecodingWidth, m_FrameServer.VideoFile.Infos.iDecodingHeight);
				m_FrameServer.Metadata.Plane.SetLocations(imageSize, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Location);
				m_FrameServer.Metadata.Grid.SetLocations(imageSize, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Location);
			}
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
			if (_iTargetHeight > m_FrameServer.VideoFile.Infos.iDecodingHeight &&
			    _iTargetHeight < panelCenter.Height &&
			    _iTargetWidth > m_FrameServer.VideoFile.Infos.iDecodingWidth &&
			    _iTargetWidth < panelCenter.Width)
			{
				double fHeightFactor = ((_iTargetHeight) / (double)m_FrameServer.VideoFile.Infos.iDecodingHeight);
				double fWidthFactor = ((_iTargetWidth) / (double)m_FrameServer.VideoFile.Infos.iDecodingWidth);

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
		private void StartMultimediaTimer(int _interval)
		{

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

			int myData = 0;	// dummy data
			m_IdMultimediaTimer = timeSetEvent( _interval,                              // Délai en ms.
			                                   _interval,                              // Resolution en ms.
			                                   m_CallbackTimerEventHandler,            // event handler du tick.
			                                   ref myData,                             // ?
			                                   TIME_PERIODIC | TIME_KILL_SYNCHRONOUS); // Type d'event (1=periodic)
			
			log.Debug("Multimedia timer started.");
			
			// Deactivate all keyframes during playing.
			ActivateKeyframe(-1);
		}
		private void StopMultimediaTimer()
		{
			if (m_IdMultimediaTimer != 0)
			{
				timeKillEvent(m_IdMultimediaTimer);
				Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
				log.Debug("Multimedia timer stopped.");
			}
		}
		private void MultimediaTimerTick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
		{
			// We comes here more often than we should, by bunches.
			if (m_FrameServer.VideoFile.Loaded)
			{
				BeginInvoke(m_CallbackPlayLoop);
			}
		}
		private void PlayLoop()
		{
			//--------------------------------------------------------------
			// Fonction appellée par l'eventhandler du timer, à chaque tick.
			// de façon asynchrone si besoin.
			//--------------------------------------------------------------
			
			//-----------------------------------------------------------------------------
			// Attention, comme la fonction est assez longue et qu'elle met à jour l'UI,
			// Il y a un risque de UI unresponsive si les BeginInvokes sont trop fréquents.
			// tout le temps sera passé ici, et on ne pourra plus répondre aux évents
			// 
			// Solution : n'effectuer le traitement long que si la form est idle.
			// ca va dropper des frames, mais on pourra toujours utiliser l'appli.
			// Par contre on doit quand même mettre à jour NextFrame.
			//-----------------------------------------------------------------------------

			bool bStopAtEnd = false;

			//----------------------------------------------------------------------------
			// En prévision de l'appel à ShowNextFrame, on vérifie qu'on ne va pas sortir.
			// Si c'est le cas, on stoppe la lecture pour rewind.
			// m_iFramesToDecode est toujours strictement positif. (Car on est en Play)
			//----------------------------------------------------------------------------
			long    TargetPosition = m_iCurrentPosition + (m_iFramesToDecode * m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame);

			if ((TargetPosition > m_iSelEnd) || (TargetPosition >= (m_iStartingPosition + m_iTotalDuration)))
			{
				// Si mode Play Once, On stoppe la lecture.
				if (m_ePlayingMode == PlayingMode.Once)
				{
					StopPlaying();
					bStopAtEnd = true;
				}
				else
				{
					// On ne stoppe que le timer,
					// on reprendra directement la lecture si tout est ok.
					StopMultimediaTimer();
					m_bSeekToStart = true;
				}

				//Close Tracks
				m_FrameServer.Metadata.StopAllTracking();
			}

			//-----------------------------------------
			// Moving playhead and rendering mechanics.
			//-----------------------------------------
			if (m_bIsIdle || m_bSeekToStart || bStopAtEnd)
			{
				if (m_bSeekToStart)
				{
					// Play loop was stopped while moving back to begining.
					// -> Back to playing loop.
					if (ShowNextFrame(m_iSelStart, true) == 0)
					{
						StartMultimediaTimer(GetPlaybackFrameInterval());
					}
					else
					{
						// Error on first frame.
						StopPlaying();
					}
					m_bSeekToStart = false;
				}
				else if (bStopAtEnd)
				{
					//--------------------------------------------------------------------------
					// Ne rien faire, la lecture à été stoppée suite à l'arrivée sur la dernière
					// frame (ou presque) et la lecture est en mode 'Once'
					//--------------------------------------------------------------------------
				}
				else
				{
					//-------------------------------------------------------------
					// Lancer la demande de la prochaine frame.
					// (éventuellement plusieurs si accumulation.
					// Nb de frames à décoder : a été placé dans m_iFramesToDecode,
					// lors des passages succéssifs dans le Timer_Tick.
					//-------------------------------------------------------------
					ShowNextFrame(-1, true);
				}

				UpdateNavigationCursor();
				
				if (m_bShowInfos) { UpdateDebugInfos(); }

				// Empty frame queue.
				m_iFramesToDecode = 1;
			}
			else
			{
				//-------------------------------------------------------------------------------
				// Not Idle.
				// Cannot decode frame now.
				// Enqueue frames.
				//
				// Side effect: queue will always stabilize right under the treshold.
				//-------------------------------------------------------------------------------
				
				// If we a re currently tracking a point, do not try to keep with the framerate.
				bool bTracking = false;
				if (m_FrameServer.Metadata.SelectedTrack >= 0)
				{
					Track trk = m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack];
					if (trk.Status == Track.TrackStatus.Edit && m_iCurrentPosition >= trk.BeginTimeStamp && m_iCurrentPosition <= trk.EndTimeStamp )
					{
						bTracking = true;
					}
				}

				if (!bTracking)
				{
					m_iFramesToDecode++;
					m_iDroppedFrames++;
				}
				
				//-------------------------------------------------------------------------------
				// Mécanisme de sécurité.
				//
				// Si le nb de drops augmente alors que la vitesse de défilement n'a pas été touchée
				// On à atteint le seuil de non retour :
				// Les images prennent plus de temps à décoder/afficher que l'intervalle du timer.
				// -> Diminuer automatiquement la vitesse.
				//-------------------------------------------------------------------------------
				if (m_iFramesToDecode > 6)
				{
					m_iFramesToDecode = 0;
					if (sldrSpeed.Value >= sldrSpeed.Minimum + sldrSpeed.LargeChange) { sldrSpeed.Value -= sldrSpeed.LargeChange; }
				}
			}
		}
		private void IdleDetector(object sender, EventArgs e)
		{
			m_bIsIdle = true;
		}
		private ReadResult ShowNextFrame(Int64 _iSeekTarget, bool _bAllowUIUpdate)
		{
			//---------------------------------------------------------------------------
			// Demande au PlayerServer de remplir la bmp avec la prochaine frame requise.
			// 2 paramètres, dépendant du contexte.
			//
			// Si _iSeekTarget = -1, utilise m_iFramesToDecode.
			// Sinon, utilise directement _iSeekTarget.
			// m_iFramesToDecode peut être négatif quand on recule.
			//---------------------------------------------------------------------------
			m_bIsIdle = false;

			ReadResult res = m_FrameServer.VideoFile.ReadFrame((long)_iSeekTarget, m_iFramesToDecode);

			if (res == ReadResult.Success)
			{
				m_iDecodedFrames++;
				m_iCurrentPosition = m_FrameServer.VideoFile.Selection.iCurrentTimeStamp;

				// Compute or stop tracking
				if (m_FrameServer.Metadata.Tracks.Count > 0)
				{
					if (_iSeekTarget >= 0 || m_iFramesToDecode > 1)
					{
						m_FrameServer.Metadata.StopAllTracking();
					}
					else
					{
						foreach (Track t in m_FrameServer.Metadata.Tracks)
						{
							if (t.Status == Track.TrackStatus.Edit)
							{
								t.TrackCurrentPosition(m_iCurrentPosition, m_FrameServer.VideoFile.CurrentImage);
							}
						}
					}
				}

				// Display image
				if(_bAllowUIUpdate) pbSurfaceScreen.Invalidate();
			}
			else
			{
				switch (res)
				{
					case ReadResult.MovieNotLoaded:
					{
						// This will be a silent error.
						break;
					}
					case ReadResult.MemoryNotAllocated:
						{
							// SHOW_NEXT_FRAME_ALLOC_ERROR
							StopPlaying(_bAllowUIUpdate);
							
							// This will be a silent error.
							// It is very low level and seem to always come in pair with another error
							// for which we'll show the dialog.
							break;
						}
					case ReadResult.FrameNotRead:
						{
							//------------------------------------------------------------------------------------
							// SHOW_NEXT_FRAME_READFRAME_ERROR
							// Frame bloquante ou fin de fichier.
							// On fait une demande de jump jusqu'à la fin de la selection.
							// Au prochain tick du timer, on prendra la décision d'arrêter la vidéo
							// ou pas en fonction du PlayingMode. (et on se replacera en début de selection)
							//
							// Possibilité que cette même frame ne soit plus bloquante lors des passages suivants.
							//------------------------------------------------------------------------------------
							m_iCurrentPosition = m_iSelEnd;
							if(_bAllowUIUpdate)
								trkSelection.SelPos = m_iCurrentPosition;
							
							//Close Tracks
							m_FrameServer.Metadata.StopAllTracking();
							
							break;
						}
					case ReadResult.ImageNotConverted:
						{
							//-------------------------------------
							// SHOW_NEXT_FRAME_IMAGE_CONVERT_ERROR
							// La Bitmap n'a pas pu être créé à partir des octets
							// (format d'image non standard.)
							//-------------------------------------
							StopPlaying(_bAllowUIUpdate);
							break;
						}
					default:
						{
							//------------------------------------------------
							// Erreur imprévue (donc grave) :
							// on reverse le compteur et on arrète la lecture.
							//------------------------------------------------
							StopPlaying(_bAllowUIUpdate);
							
							break;
						}
				}
				
			}

			return res;
		}
		private void StopPlaying(bool _bAllowUIUpdate)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				if (m_bIsCurrentlyPlaying)
				{
					StopMultimediaTimer();
					m_bIsCurrentlyPlaying = false;
					Application.Idle -= new EventHandler(this.IdleDetector);
					m_iFramesToDecode = 0;

					if (_bAllowUIUpdate)
					{
						buttonPlay.BackgroundImage = Resources.liqplay17;
						pbSurfaceScreen.Invalidate();
					}
				}
			}
		}
		
		private void mnuSetCaptureSpeed_Click(object sender, EventArgs e)
		{
			DisplayConfigureSpeedBox(false);
		}
		private void lblTimeCode_DoubleClick(object sender, EventArgs e)
		{
			// Same as mnuSetCaptureSpeed_Click but different location.
			DisplayConfigureSpeedBox(true);
		}
		private void DisplayConfigureSpeedBox(bool _center)
		{
			//--------------------------------------------------------------------
			// Display the dialog box that let the user specify the capture speed.
			// Used to adpat time for high speed cameras.
			//--------------------------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}

				formConfigureSpeed fcs = new formConfigureSpeed(m_FrameServer.VideoFile.Infos.fFps, m_fHighSpeedFactor);
				if (_center)
				{
					fcs.StartPosition = FormStartPosition.CenterScreen;
				}
				else
				{
					fcs.StartPosition = FormStartPosition.Manual;
					LocateForm(fcs);
					//fcs.Location = new Point(Cursor.Position.X - fcs.Width / 2, Cursor.Position.Y - 50);
				}
				
				if (fcs.ShowDialog() == DialogResult.OK)
				{
					m_fHighSpeedFactor = fcs.SlowFactor;						
				}
					
				fcs.Dispose();

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}

				// Update times.
				UpdateSelectionLabels();
				UpdateCurrentPositionLabel();
				pbSurfaceScreen.Invalidate();
			}
		}
		private int GetPlaybackFrameInterval()
		{
			// Returns the playback interval between frames in Milliseconds, taking slow motion slider into account.
			// m_iSlowmotionPercentage must be > 0.
			if (m_FrameServer.VideoFile.Loaded && m_FrameServer.VideoFile.Infos.iFrameInterval > 0 && m_iSlowmotionPercentage > 0)
			{
				return (int)((double)m_FrameServer.VideoFile.Infos.iFrameInterval / ((double)m_iSlowmotionPercentage / 100.0));
			}
			else
			{
				return 40;
			}
		}
		#endregion
		
		#region Culture
		private void ReloadMenusCulture()
		{
			// Reload the text for each menu.
			// this is done at construction time and at RefreshUICulture time.
			
			// 1. Default context menu.
			mnuPlayPause.Text = ScreenManagerLang.mnuPlayPause;
			mnuSetCaptureSpeed.Text = ScreenManagerLang.mnuSetCaptureSpeed;
			mnuSavePic.Text = ScreenManagerLang.mnuSavePic;
			mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
			
			// 2. Drawings context menu (Configure, Delete, Track this)
			mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;
			mnuConfigureFading.Text = ScreenManagerLang.mnuConfigureFading;
			mnuTrackTrajectory.Text = ScreenManagerLang.mnuTrackTrajectory;
			mnuGotoKeyframe.Text = ScreenManagerLang.mnuGotoKeyframe;
			mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
			mnuShowMeasure.Text = ScreenManagerLang.mnuShowMeasure;
			mnuSealMeasure.Text = ScreenManagerLang.mnuSealMeasure;
			
			// 3. Tracking pop menu (Restart, Stop tracking)
			mnuStopTracking.Text = ScreenManagerLang.mnuStopTracking;
			mnuRestartTracking.Text = ScreenManagerLang.mnuRestartTracking;
			mnuDeleteTrajectory.Text = ScreenManagerLang.mnuDeleteTrajectory;
			mnuDeleteEndOfTrajectory.Text = ScreenManagerLang.mnuDeleteEndOfTrajectory;
			mnuConfigureTrajectory.Text = ScreenManagerLang.Generic_Configuration;
			
			// 4. Chrono pop menu (Start, Stop, Hide, etc.)
			mnuChronoConfigure.Text = ScreenManagerLang.Generic_Configuration;
			mnuChronoStart.Text = ScreenManagerLang.mnuChronoStart;
			mnuChronoStop.Text = ScreenManagerLang.mnuChronoStop;
			mnuChronoHide.Text = ScreenManagerLang.mnuChronoHide;
			mnuChronoCountdown.Text = ScreenManagerLang.mnuChronoCountdown;
			mnuChronoDelete.Text = ScreenManagerLang.mnuChronoDelete;
				
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
			toolTips.SetToolTip(buttonGotoPrevious, ScreenManagerLang.ToolTip_Back);
			toolTips.SetToolTip(buttonGotoNext, ScreenManagerLang.ToolTip_Next);
			toolTips.SetToolTip(buttonGotoFirst, ScreenManagerLang.ToolTip_First);
			toolTips.SetToolTip(buttonGotoLast, ScreenManagerLang.ToolTip_Last);
			if (m_ePlayingMode == PlayingMode.Once)
			{
				toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Once);
			}
			else
			{
				toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Loop);
			}
			
			// Export buttons
			toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.ToolTip_Snapshot);
			toolTips.SetToolTip(btnRafale, ScreenManagerLang.ToolTip_Rafale);
			toolTips.SetToolTip(btnDiaporama, ScreenManagerLang.ToolTip_SaveDiaporama);
			toolTips.SetToolTip(btnPdf, ScreenManagerLang.dlgExportToPDF_Title);
			toolTips.SetToolTip(btnSaveVideo, ScreenManagerLang.dlgSaveVideoTitle);
			toolTips.SetToolTip(btnPausedVideo, ScreenManagerLang.ToolTip_SavePausedVideo);
			
			// Working zone and sliders.
			if (m_bHandlersLocked)
			{
				toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionUnlock);
			}
			else
			{
				toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionLock);	
			}
			toolTips.SetToolTip(btnSetHandlerLeft, ScreenManagerLang.ToolTip_SetHandlerLeft);
			toolTips.SetToolTip(btnSetHandlerRight, ScreenManagerLang.ToolTip_SetHandlerRight);
			toolTips.SetToolTip(btnHandlersReset, ScreenManagerLang.ToolTip_ResetWorkingZone);
			trkSelection.ToolTip = ScreenManagerLang.ToolTip_trkSelection;
			sldrSpeed.ToolTip = ScreenManagerLang.ToolTip_sldrSpeed;

			// Drawing tools
			toolTips.SetToolTip(btnAddKeyframe, ScreenManagerLang.ToolTip_AddKeyframe);
			toolTips.SetToolTip(btnDrawingToolPointer, ScreenManagerLang.ToolTip_DrawingToolPointer);
			toolTips.SetToolTip(btnDrawingToolText, ScreenManagerLang.ToolTip_DrawingToolText);
			toolTips.SetToolTip(btnDrawingToolPencil, ScreenManagerLang.ToolTip_DrawingToolPencil);
			toolTips.SetToolTip(btnDrawingToolLine2D, ScreenManagerLang.ToolTip_DrawingToolLine2D);
			toolTips.SetToolTip(btnDrawingToolCross2D, ScreenManagerLang.ToolTip_DrawingToolCross2D);
			toolTips.SetToolTip(btnDrawingToolAngle2D, ScreenManagerLang.ToolTip_DrawingToolAngle2D);
			toolTips.SetToolTip(btnShowComments, ScreenManagerLang.ToolTip_ShowComments);
			toolTips.SetToolTip(btnColorProfile, ScreenManagerLang.ToolTip_ColorProfile);
			toolTips.SetToolTip(btnDrawingToolChrono, ScreenManagerLang.ToolTip_DrawingToolChrono);
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
			if(m_FrameServer.VideoFile != null)
			{
				if (m_FrameServer.VideoFile.Loaded)
				{
					if (e.Button == MouseButtons.Left)
					{
						// Magnifier can be moved even when the video is playing.
						// TODO - Grids should be able to do the same.
						// But the z order in the PointerTool MouseDown would have to be taken care of.
						
						bool bWasPlaying = false;
						
						if (m_bIsCurrentlyPlaying)
						{
							if ( (m_ActiveTool == DrawingToolType.Pointer)      &&
							    (m_Magnifier.Mode != MagnifierMode.NotVisible) &&
							    (m_Magnifier.IsOnObject(e)))
							{
								m_Magnifier.OnMouseDown(e);
							}
							else
							{
								// MouseDown while playing: Halt the video.
								StopPlaying();
								ActivateKeyframe(m_iCurrentPosition);
								bWasPlaying = true;
							}
						}
						
						
						if (!m_bIsCurrentlyPlaying)
						{
							//-------------------------------------
							// Action begins:
							// Move or set magnifier
							// Move or set Drawing
							// Move or set Chrono / Track
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
								
								if (m_Magnifier.Mode == MagnifierMode.Indirect)
								{
									bMovingMagnifier = m_Magnifier.OnMouseDown(e);
								}
								
								if (!bMovingMagnifier)
								{
									// Magnifier wasn't hit or is not in use,
									// try drawings (including chronos, grids, etc.)
									bDrawingHit = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseDown(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition, m_PrefManager.DefaultFading.Enabled);
								}
								
								if (!bDrawingHit && !bWasPlaying)
								{
									// MouseDown in arbitrary location and we were halted already.
									
									// We cannot restart the video here because this MouseDown may actually be the start
									// of a double click. (expand screen)
								}
							}
							else if (m_ActiveTool == DrawingToolType.Chrono)
							{
								// Creating a new Chrono.
								m_FrameServer.Metadata.SelectedTrack = -1;
								
								// Add a Chrono.
								DrawingToolChrono dtc = (DrawingToolChrono)m_DrawingTools[(int)m_ActiveTool];
								m_FrameServer.Metadata.Chronos.Insert(0, (DrawingChrono)dtc.GetNewDrawing(descaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame));
								m_FrameServer.Metadata.SelectedChrono = 0;
								
								// Complete Setup
								m_FrameServer.Metadata.Chronos[0].ParentMetadata = m_FrameServer.Metadata;
								m_ColorProfile.SetupDrawing(m_FrameServer.Metadata.Chronos[0], DrawingToolType.Chrono);
							}
							else
							{
								//-----------------------
								// Creating a new Drawing
								//-----------------------
								m_FrameServer.Metadata.SelectedTrack = -1;
								m_FrameServer.Metadata.SelectedChrono = -1;
								
								// Add a KeyFrame here if it doesn't exist.
								AddKeyframe();
								
								if (m_ActiveTool != DrawingToolType.Text)
								{
									// Add an instance of a drawing from the active tool to the current keyframe.
									// The drawing is initialized with the current mouse coordinates.
									AbstractDrawing ad = m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame);
									
									m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(ad);
									m_FrameServer.Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
									m_FrameServer.Metadata.SelectedDrawing = 0;
									
									// Color
									m_ColorProfile.SetupDrawing(ad, m_ActiveTool);
									
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
									foreach (AbstractDrawing ad in m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings)
									{
										if (ad is DrawingText)
										{
											int hitRes = ad.HitTest(descaledMouse, m_iCurrentPosition);
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
										m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame));
										m_FrameServer.Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
										m_FrameServer.Metadata.SelectedDrawing = 0;
										
										DrawingText dt = (DrawingText)m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings[0];
										
										dt.ContainerScreen = pbSurfaceScreen;
										dt.RelocateEditbox(m_FrameServer.CoordinateSystem.Stretch * m_FrameServer.CoordinateSystem.Zoom, m_FrameServer.CoordinateSystem.Location);
										dt.EditMode = true;
										panelCenter.Controls.Add(dt.EditBox);
										dt.EditBox.BringToFront();
										dt.EditBox.Focus();
										m_ColorProfile.SetupDrawing(dt, DrawingToolType.Text);
										//dt.SetupColor(m_ColorProfile);
									}
								}
							}
						}
					}
					else if (e.Button == MouseButtons.Right)
					{
						// Show the right Pop Menu depending on context.
						// (Drawing, Trajectory, Chronometer, Grids, Magnifier, Nothing)
						
						Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
						
						if (!m_bIsCurrentlyPlaying)
						{
							m_FrameServer.Metadata.UnselectAll();
							
							//m_bWasRightClicking = true;
							
							if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition))
							{
								// If we are on a Cross2D, we activate the menu to let the user Track it.
								AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing];
								
								// We use temp variables because ToolStripMenuItem.Visible always returns false...
								bool isCross = (ad is DrawingCross2D);
								bool isLine = (ad is DrawingLine2D);
								bool fadingVisible = m_PrefManager.DefaultFading.Enabled;
								bool gotoVisible = (m_PrefManager.DefaultFading.Enabled && (ad.infosFading.ReferenceTimestamp != m_iCurrentPosition));
								
								mnuTrackTrajectory.Visible = isCross;
								mnuTrackTrajectory.Enabled = (ad.infosFading.ReferenceTimestamp == m_iCurrentPosition);
								mnuConfigureFading.Visible = fadingVisible;
								mnuGotoKeyframe.Visible = gotoVisible;
								mnuShowMeasure.Visible = isLine;
								mnuSealMeasure.Visible = isLine;
								
								mnuSepDrawing.Visible = isCross || gotoVisible || isLine;
								
								// "Color & Size" or "Color" depending on drawing type.
								SetPopupConfigureParams(ad);
								
								this.ContextMenuStrip = popMenuDrawings;
							}
							else if (m_FrameServer.Metadata.IsOnChronometer(descaledMouse, m_iCurrentPosition))
							{
								// We can only toggle to countdown if we already have a stop.
								mnuChronoCountdown.Enabled = m_FrameServer.Metadata.Chronos[m_FrameServer.Metadata.SelectedChrono].HasTimeStop;
								mnuChronoCountdown.Checked = m_FrameServer.Metadata.Chronos[m_FrameServer.Metadata.SelectedChrono].CountDown;
								this.ContextMenuStrip = popMenuChrono;
							}
							else if (m_FrameServer.Metadata.IsOnTrack(descaledMouse, m_iCurrentPosition))
							{
								// Configure the "Tracking" Pop Menu
								if (m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].Status == Track.TrackStatus.Edit)
								{
									mnuStopTracking.Visible = true;
									mnuRestartTracking.Visible = false;
								}
								else
								{
									mnuStopTracking.Visible = false;
									mnuRestartTracking.Visible = true;
								}
								
								this.ContextMenuStrip = popMenuTrack;
							}
							else if (m_FrameServer.Metadata.IsOnGrid(descaledMouse))
							{
								this.ContextMenuStrip = popMenuGrids;
							}
							else if (m_Magnifier.Mode == MagnifierMode.Indirect && m_Magnifier.IsOnObject(e))
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
						else
						{
							this.ContextMenuStrip = popMenu;
						}
					}
					
					pbSurfaceScreen.Invalidate();
				}
			}
		}
		private void SurfaceScreen_MouseMove(object sender, MouseEventArgs e)
		{
			// We must keep the same Z order.
			// 1:Magnifier, 2:Drawings, 3:Chronos/Tracks, 4:Grids.
			// When creating a drawing, the active tool will stay on this drawing until its setup is over.
			// After the drawing is created, we either fall back to Pointer tool or stay on the same tool.

			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded)
			{
				if (e.Button == MouseButtons.None && m_Magnifier.Mode == MagnifierMode.Direct)
				{
					m_Magnifier.MouseX = e.X;
					m_Magnifier.MouseY = e.Y;
					
					if (!m_bIsCurrentlyPlaying)
					{
						pbSurfaceScreen.Invalidate();
					}
				}
				else if (e.Button == MouseButtons.Left)
				{
					if (m_ActiveTool != DrawingToolType.Pointer && m_ActiveTool != DrawingToolType.Chrono)
					{
						if (m_iActiveKeyFrameIndex >= 0 && !m_bIsCurrentlyPlaying)
						{
							// Currently setting the second point of a Drawing.
							m_DrawingTools[(int)m_ActiveTool].OnMouseMove(m_FrameServer.Metadata[m_iActiveKeyFrameIndex], m_FrameServer.CoordinateSystem.Untransform(new Point(e.X, e.Y)));
						}
					}
					else
					{
						bool bMovingMagnifier = false;
						if (m_Magnifier.Mode == MagnifierMode.Indirect)
						{
							bMovingMagnifier = m_Magnifier.OnMouseMove(e);
						}
						
						if (!bMovingMagnifier && m_ActiveTool == DrawingToolType.Pointer)
						{
							if (!m_bIsCurrentlyPlaying)
							{
								Point descaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
								
								// Magnifier is not being moved or is invisible, try drawings through pointer tool.
								// (including chronos, tracks and grids)
								bool bMovingObject = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseMove(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, descaledMouse, m_FrameServer.CoordinateSystem.Location, ModifierKeys);
								
								if (!bMovingObject && m_FrameServer.CoordinateSystem.Zooming)
								{
									// User is not moving anything and we are zooming : move the zoom window.
								
									// Get mouse deltas (descaled=in image coords).
									double fDeltaX = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.X;
									double fDeltaY = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.Y;
									
									if(m_FrameServer.Metadata.Mirrored)
									{
										fDeltaX = -fDeltaX;
									}
									
									m_FrameServer.CoordinateSystem.MoveZoomWindow(fDeltaX, fDeltaY);
								}
							}
						}
					}
					
					if (!m_bIsCurrentlyPlaying)
					{
						pbSurfaceScreen.Invalidate();
					}
				}
			}
		}
		private void SurfaceScreen_MouseUp(object sender, MouseEventArgs e)
		{
			// End of an action.
			// Depending on the active tool we have various things to do.
			
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded && e.Button == MouseButtons.Left)
			{
				if (m_ActiveTool == DrawingToolType.Pointer)
				{
					OnPoke();
					
					// Update tracks with current image and pos.
					if (m_FrameServer.Metadata.SelectedTrack >= 0 && m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].Status == Track.TrackStatus.Edit)
					{
						m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].UpdateCurrentPos(m_FrameServer.VideoFile.CurrentImage);
					}
				}
				
				m_Magnifier.OnMouseUp(e);
				
				// Memorize the action we just finished to enable undo.
				if(m_ActiveTool == DrawingToolType.Chrono)
				{
					IUndoableCommand cac = new CommandAddChrono(DoInvalidate, DoDrawingUndrawn, m_FrameServer.Metadata);
					CommandManager cm = CommandManager.Instance();
					cm.LaunchUndoableCommand(cac);
				}
				else if (m_ActiveTool != DrawingToolType.Pointer && m_iActiveKeyFrameIndex >= 0)
				{
					// Record the adding unless we are editing a text box.
					if (!m_bTextEdit)
					{
						IUndoableCommand cad = new CommandAddDrawing(DoInvalidate, DoDrawingUndrawn, m_FrameServer.Metadata, m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Position);
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
				
				if (m_iActiveKeyFrameIndex >= 0)
				{
					m_FrameServer.Metadata.SelectedDrawingFrame = -1;
					m_FrameServer.Metadata.SelectedDrawing = -1;
				}
				
				pbSurfaceScreen.Invalidate();
			}
		}
		private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded && e.Button == MouseButtons.Left)
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
				if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition))
				{
					AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing];
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
				else if (m_FrameServer.Metadata.IsOnChronometer(descaledMouse, m_iCurrentPosition))
				{
					mnuChronoConfigure_Click(null, EventArgs.Empty);
				}
				else if (m_FrameServer.Metadata.IsOnTrack(descaledMouse, m_iCurrentPosition))
				{
					mnuConfigureTrajectory_Click(null, EventArgs.Empty);
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
			//-------------------------------------------------------------------
			// We always draw at full SurfaceScreen size.
			// It is the SurfaceScreen itself that is resized if needed.
			//-------------------------------------------------------------------
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded)
			{
				if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
				{
					m_DrawingFilterOutput.Draw(e.Graphics, pbSurfaceScreen.Size, m_DrawingFilterOutput.InputFrames, m_DrawingFilterOutput.PrivateData);
				}
				else if(m_FrameServer.VideoFile.CurrentImage != null)
				{
					try
					{
						// If we are on a keyframe, see if it has any drawing.
						int iKeyFrameIndex = -1;
						if (m_iActiveKeyFrameIndex >= 0)
						{
							if (m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
							{
								iKeyFrameIndex = m_iActiveKeyFrameIndex;
							}
						}
						
						FlushOnGraphics(m_FrameServer.VideoFile.CurrentImage, e.Graphics, pbSurfaceScreen.Size, iKeyFrameIndex, m_iCurrentPosition);
					}
					catch (System.InvalidOperationException)
					{
						log.Error("Error while painting image. Object is currently in use elsewhere... ATI Drivers ?");
					}
					catch (Exception exp)
					{
						log.Error("Error while painting image.");
						log.Error(exp.Message);
						log.Error(exp.StackTrace);
					}
					finally
					{
						// Nothing more to do.
					}
				}
				else
				{
					log.Debug("Painting screen - no image to display.");
				}
				
				// Draw Selection Border if needed.
				if (m_bShowImageBorder)
				{
					DrawImageBorder(e.Graphics);
				}
			}
		}
		private void SurfaceScreen_MouseEnter(object sender, EventArgs e)
		{
			// Set focus to surfacescreen to enable mouse scroll
			
			// But only if there is no Text edition going on.
			bool bEditing = false;
			if(m_FrameServer.Metadata.Count > m_iActiveKeyFrameIndex && m_iActiveKeyFrameIndex >= 0)
			{
				foreach (AbstractDrawing ad in m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings)
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
			}
			
			if(!bEditing)
			{
				pbSurfaceScreen.Focus();
			}
			
		}
		private void FlushOnGraphics(Bitmap _sourceImage, Graphics g, Size _iNewSize, int _iKeyFrameIndex, long _iPosition)
		{
			// This function is used both by the main rendering loop and by image export functions.

			// Notes on performances:
			// - The fastest pixel format to render is Format32bppPArgb. (But we can't directly decode from FFMpeg into that)
			// - The global performance depends on the size of the *source* image. Not destination.
			//   (rendering 1 pixel from an HD source will still be slow)
			// - Using a matrix transform instead of the buit in interpolation doesn't seem to do much.
			// - InterpolationMode has a sensible effect. but can look ugly for lowest values.
			// - Using unmanaged BitBlt or StretchBlt doesn't seem to do much... (!?)
			// - the scaling and interpolation better be done directly from ffmpeg. (cut on memory usage too)
			// - furthermore ffmpeg has a mode called 'FastBilinear' that seems more promising.

			#if TRACE
			//m_RenderingWatch.Reset();
			//m_RenderingWatch.Start();
			#endif
			
			// 1. Image
			g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.InterpolationMode = InterpolationMode.Bilinear;
			g.SmoothingMode = SmoothingMode.None;
			
			// TODO - matrix transform.
			// - Focus region
			// - Rotate 90°/-90°
			// - Mirror
			
			Rectangle rDst;
			if(m_FrameServer.Metadata.Mirrored)
			{
				rDst = new Rectangle(_iNewSize.Width, 0, -_iNewSize.Width, _iNewSize.Height);
			}
			else
			{
				rDst = new Rectangle(0, 0, _iNewSize.Width, _iNewSize.Height);
			}
			
			Rectangle rSrc;
			if (m_FrameServer.CoordinateSystem.Zooming)
			{
				rSrc = m_FrameServer.CoordinateSystem.ZoomWindow;
			}
			else
			{
				rSrc = new Rectangle(0, 0, _sourceImage.Width, _sourceImage.Height);
			}
			
			g.DrawImage(_sourceImage, rDst, rSrc, GraphicsUnit.Pixel);
			

			#region other perf tests
			// 1.b. Testing Crop - Does not impact performances.
			//Rectangle destRect = new Rectangle(0, 0, _iNewSize.Width, _iNewSize.Height);
			//g.DrawImage(_sourceImage, destRect, m_CropRectangle, GraphicsUnit.Pixel);

			// 1.c. Testing Matrix transform to improve perfs.
			//Matrix mx = new Matrix((float)m_FrameServer.CoordinateSystem.Stretch, 0, 0, (float)m_FrameServer.CoordinateSystem.Stretch, 0, 0);
			//Matrix mx = new Matrix((float)10, 0, 0, (float)10, 0, 0);
			//g.Transform = mx;
			//g.DrawImageUnscaled(_sourceImage, 0, 0);


			// 1.e. Through unmanaged code.
			/*Graphics imgGraphics = Graphics.FromImage(_sourceImage);
            
            IntPtr hdc_screen = g.GetHdc();
            IntPtr hdc_image = imgGraphics.GetHdc();

            BitBlt(hdc_screen, 100, 0, 100, 100, hdc_image, 0, 0, 0xCC0020);

            g.ReleaseHdc(hdc_screen);
            imgGraphics.ReleaseHdc(hdc_image);
            imgGraphics.Dispose();*/

			//--------------------------------------------------------------
			/*IntPtr hbmp = _sourceImage.GetHbitmap();

            IntPtr pTarget = g.GetHdc();
            IntPtr pSource = CreateCompatibleDC(pTarget);
            IntPtr pOrig = SelectObject(pSource, hbmp);

            //StretchBlt(pTarget, 0, 0, _iNewSize.Width, _iNewSize.Height,
            //           pSource, 0, 0, _sourceImage.Width, _sourceImage.Height,
             //          TernaryRasterOperations.SRCCOPY);

            BitBlt( pTarget, 0, 0, _sourceImage.Width, _sourceImage.Height,
                    pSource, 0, 0, TernaryRasterOperations.SRCCOPY);

            IntPtr pNew = SelectObject(pSource, pOrig);
            DeleteObject(pNew);
            DeleteDC(pSource);
            
            g.ReleaseHdc(pTarget);*/
			#endregion

			FlushDrawingsOnGraphics(g, _iKeyFrameIndex, _iPosition, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Zoom, m_FrameServer.CoordinateSystem.Location);

			// .Magnifier
			if (m_Magnifier.Mode != MagnifierMode.NotVisible)
			{
				// Mirrored ?

				m_Magnifier.Draw(_sourceImage, g, m_FrameServer.CoordinateSystem.Stretch);
			}
		}
		private void FlushDrawingsOnGraphics(Graphics _canvas, int _iKeyFrameIndex, long _iPosition, double _fStretchFactor, double _fDirectZoomFactor, Point _DirectZoomTopLeft)
		{
			// Prepare for drawings
			_canvas.SmoothingMode = SmoothingMode.AntiAlias;

			// 1. 2D Grid
			if (m_FrameServer.Metadata.Grid.Visible)
			{
				m_FrameServer.Metadata.Grid.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _DirectZoomTopLeft);
			}

			// 2. 3D Plane
			if (m_FrameServer.Metadata.Plane.Visible)
			{
				m_FrameServer.Metadata.Plane.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _DirectZoomTopLeft);
			}

			// 3. Regular drawings.
			if (m_PrefManager.DefaultFading.Enabled)
			{
				if ((m_bIsCurrentlyPlaying && m_PrefManager.DrawOnPlay) || !m_bIsCurrentlyPlaying)
				{
					// If fading is on, we ask all drawings to draw themselves with their respective
					// fading factor for this position.

					int[] zOrder = m_FrameServer.Metadata.GetKeyframesZOrder(_iPosition);

					// Draw in reverse keyframes z order so the closest next keyframe gets drawn on top (last).
					for (int ikf = zOrder.Length-1; ikf >= 0 ; ikf--)
					{
						Keyframe kf = m_FrameServer.Metadata.Keyframes[zOrder[ikf]];
						for (int idr = kf.Drawings.Count - 1; idr >= 0; idr--)
						{
							bool bSelected = (zOrder[ikf] == m_FrameServer.Metadata.SelectedDrawingFrame && idr == m_FrameServer.Metadata.SelectedDrawing);
							kf.Drawings[idr].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, bSelected, _iPosition, _DirectZoomTopLeft);
						}
					}
				}
			}
			else if (_iKeyFrameIndex >= 0)
			{
				// if fading is off, only draw the current keyframe.
				// Draw all drawings in reverse order to get first object on the top of Z-order.
				for (int i = m_FrameServer.Metadata[_iKeyFrameIndex].Drawings.Count - 1; i >= 0; i--)
				{
					bool bSelected = (_iKeyFrameIndex == m_FrameServer.Metadata.SelectedDrawingFrame && i == m_FrameServer.Metadata.SelectedDrawing);
					m_FrameServer.Metadata[_iKeyFrameIndex].Drawings[i].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, bSelected, _iPosition, _DirectZoomTopLeft);
				}
			}
			else
			{
				// This is not a Keyframe, and fading is off.
				// Hence, there is no drawings to draw here.
			}

			// 4. Tracks
			if (m_FrameServer.Metadata.Tracks.Count > 0)
			{
				foreach (Track t in m_FrameServer.Metadata.Tracks)
				{
					t.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _iPosition, _DirectZoomTopLeft);
				}
			}

			// 5. Chronos
			if (m_FrameServer.Metadata.Chronos.Count > 0)
			{
				for (int i = 0; i < m_FrameServer.Metadata.Chronos.Count; i++)
				{
					m_FrameServer.Metadata.Chronos[i].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, (i == m_FrameServer.Metadata.SelectedChrono), _iPosition, _DirectZoomTopLeft);
				}
			}
		}
		private void DoInvalidate()
		{
			pbSurfaceScreen.Invalidate();
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
		private void btnAddKeyframe_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				AddKeyframe();

				// Set as active screen is done afterwards, so the export as pdf menu is activated
				// even if we had no keyframes yet.
				OnPoke();
			}
		}
		private void OrganizeKeyframes()
		{
			// Should only be called when adding/removing a Thumbnail
			
			pnlThumbnails.Controls.Clear();

			if (m_FrameServer.Metadata.Count > 0)
			{
				int iKeyframeIndex = 0;
				int iPixelsOffset = 0;
				int iPixelsSpacing = 20;

				foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
				{
					KeyframeBox box = new KeyframeBox(kf);
					SetupDefaultThumbBox(box);
					
					// Finish the setup
					box.Left = iPixelsOffset + iPixelsSpacing;

					box.pbThumbnail.Image = kf.Thumbnail;
					box.UpdateTitle(kf.Title);
					box.Tag = iKeyframeIndex;
					box.pbThumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
					
					box.CloseThumb += new KeyframeBox.CloseThumbHandler(ThumbBoxClose);
					box.ClickThumb += new KeyframeBox.ClickThumbHandler(ThumbBoxClick);
					box.ClickInfos += new KeyframeBox.ClickInfosHandler(ThumbBoxInfosClick);
					
					// TODO - Titre de la Keyframe en ToolTip.
					iPixelsOffset += (iPixelsSpacing + box.Width);

					pnlThumbnails.Controls.Add(box);

					iKeyframeIndex++;
				}
				EnableDisableKeyframes();
				pnlThumbnails.Refresh();
				pbSurfaceScreen.Invalidate(); // Because of trajectories.
			}
			else
			{
				DockKeyframePanel();
				m_iActiveKeyFrameIndex = -1;
			}
		}
		private void SetupDefaultThumbBox(KeyframeBox _ThumbBox)
		{
			_ThumbBox.Top = 10;
			_ThumbBox.Cursor = Cursors.Hand;
		}
		private void ActivateKeyframe(long _iPosition)
		{
			ActivateKeyframe(_iPosition, true);
		}
		private void ActivateKeyframe(long _iPosition, bool _bAllowUIUpdate)
		{
			//--------------------------------------------------------------
			// Black border every keyframe, unless it is at the given position.
			// This method might be called with -1 to force complete blackout.
			//--------------------------------------------------------------

			// This method is called on each frame during frametracker browsing
			// keep it fast or fix the strategy.

			m_iActiveKeyFrameIndex = -1;

			// We leverage the fact that pnlThumbnail is exclusively populated with thumboxes.
			for (int i = 0; i < pnlThumbnails.Controls.Count; i++)
			{
				if (m_FrameServer.Metadata[i].Position == _iPosition)
				{
					m_iActiveKeyFrameIndex = i;
					if(_bAllowUIUpdate)
						((KeyframeBox)pnlThumbnails.Controls[i]).DisplayAsSelected(true);

					// Make sure the thumbnail is always in the visible area by auto scrolling.
					if(_bAllowUIUpdate) pnlThumbnails.ScrollControlIntoView(pnlThumbnails.Controls[i]);
				}
				else
				{
					if(_bAllowUIUpdate)
						((KeyframeBox)pnlThumbnails.Controls[i]).DisplayAsSelected(false);
				}
			}

			if (_bAllowUIUpdate && m_KeyframeCommentsHub.UserActivated && m_iActiveKeyFrameIndex >= 0)
			{
				m_KeyframeCommentsHub.UpdateContent(m_FrameServer.Metadata[m_iActiveKeyFrameIndex]);
				m_KeyframeCommentsHub.Visible = true;
			}
			else
			{
				m_KeyframeCommentsHub.Visible = false;
			}
		}
		private void EnableDisableKeyframes()
		{
			// public : called from formKeyFrameComments. (fixme ?)

			// Enable Keyframes that are within Working Zone, Disable others.

			// We leverage the fact that pnlThumbnail is exclusively populated with thumboxes.
			for (int i = 0; i < pnlThumbnails.Controls.Count; i++)
			{
				KeyframeBox tb = pnlThumbnails.Controls[i] as KeyframeBox;
				if(tb != null)
				{
					m_FrameServer.Metadata[i].TimeCode = TimeStampsToTimecode(m_FrameServer.Metadata[i].Position - m_iSelStart, m_PrefManager.TimeCodeFormat, false);
					
					// Enable thumbs that are within Working Zone, grey out others.
					if (m_FrameServer.Metadata[i].Position >= m_iSelStart && m_FrameServer.Metadata[i].Position <= m_iSelEnd)
					{
						m_FrameServer.Metadata[i].Disabled = false;
						
						tb.Enabled = true;
						tb.pbThumbnail.Image = m_FrameServer.Metadata[i].Thumbnail;	
					}
					else
					{
						m_FrameServer.Metadata[i].Disabled = true;
						
						tb.Enabled = false;
						tb.pbThumbnail.Image = m_FrameServer.Metadata[i].DisabledThumbnail;
					}

					tb.UpdateTitle(m_FrameServer.Metadata[i].Title);
				}
			}
		}
		public void OnKeyframesTitleChanged()
		{
			// Called when title changed.

			// Update trajectories.
			m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();

			// Update thumb boxes.
			EnableDisableKeyframes();

			pbSurfaceScreen.Invalidate();

		}
		private void GotoNextKeyframe()
		{
			if (m_FrameServer.Metadata.Count > 1)
			{
				int iNextKeyframe = -1;
				for (int i = 0; i < m_FrameServer.Metadata.Count; i++)
				{
					if (m_iCurrentPosition < m_FrameServer.Metadata[i].Position)
					{
						iNextKeyframe = i;
						break;
					}
				}

				if (iNextKeyframe >= 0 && m_FrameServer.Metadata[iNextKeyframe].Position <= m_iSelEnd)
				{
					ThumbBoxClick(pnlThumbnails.Controls[iNextKeyframe], EventArgs.Empty);
				}
				
			}
		}
		private void GotoPreviousKeyframe()
		{
			if (m_FrameServer.Metadata.Count > 0)
			{
				int iPrevKeyframe = -1;
				for (int i = m_FrameServer.Metadata.Count - 1; i >= 0; i--)
				{
					if (m_iCurrentPosition > m_FrameServer.Metadata[i].Position)
					{
						iPrevKeyframe = i;
						break;
					}
				}

				if (iPrevKeyframe >= 0 && m_FrameServer.Metadata[iPrevKeyframe].Position >= m_iSelStart)
				{
					ThumbBoxClick(pnlThumbnails.Controls[iPrevKeyframe], EventArgs.Empty);
				}

			}
		}

		private void AddKeyframe()
		{
			int i = 0;
			// Check if it's not already registered.
			bool bAlreadyKeyFrame = false;
			for (i = 0; i < m_FrameServer.Metadata.Count; i++)
			{
				if (m_FrameServer.Metadata[i].Position == m_iCurrentPosition)
				{
					bAlreadyKeyFrame = true;
					m_iActiveKeyFrameIndex = i;
				}
			}

			// Add it to the list.
			if (!bAlreadyKeyFrame)
			{
				IUndoableCommand cak = new CommandAddKeyframe(this, m_FrameServer.Metadata, m_iCurrentPosition);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cak);
				
				// If it is the very first key frame, we raise the KF panel.
				// Otherwise we keep whatever choice the user made.
				if(m_FrameServer.Metadata.Count == 1)
				{
					UndockKeyframePanel();
				}
			}
		}
		public void OnAddKeyframe(long _iPosition)
		{
			// Public because called from CommandAddKeyframe.Execute()
			// Title becomes the current timecode. (relative to sel start or sel minimum ?)
			
			Keyframe kf = new Keyframe(_iPosition, TimeStampsToTimecode(_iPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, m_bSynched), m_FrameServer.VideoFile.CurrentImage, m_FrameServer.Metadata);
			
			if (_iPosition != m_iCurrentPosition)
			{
				// Move to the required Keyframe.
				// Should only happen when Undoing a DeleteKeyframe.
				m_iFramesToDecode = 1;
				ShowNextFrame(_iPosition, true);
				UpdateNavigationCursor();
				UpdateCurrentPositionLabel();
				trkSelection.SelPos = trkFrame.Position;

				// Readjust and complete the Keyframe
				kf.ImportImage(m_FrameServer.VideoFile.CurrentImage);
			}

			m_FrameServer.Metadata.Add(kf);

			// Keep the list sorted
			m_FrameServer.Metadata.Sort();

			m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
			UpdateKeyframesMarkers();

			// Refresh Keyframes preview.
			OrganizeKeyframes();

			// B&W conversion can be lengthly. We do it after showing the result.
			kf.GenerateDisabledThumbnail();

			if (!m_bIsCurrentlyPlaying)
			{
				ActivateKeyframe(m_iCurrentPosition);
			}
			
		}
		private void RemoveKeyframe(int _iKeyframeIndex)
		{
			IUndoableCommand cdk = new CommandDeleteKeyframe(this, m_FrameServer.Metadata, m_FrameServer.Metadata[_iKeyframeIndex].Position);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdk);

			//OnRemoveKeyframe(_iKeyframeIndex);
		}
		public void OnRemoveKeyframe(int _iKeyframeIndex)
		{
			if (_iKeyframeIndex == m_iActiveKeyFrameIndex)
			{
				// Removing active frame
				m_iActiveKeyFrameIndex = -1;
			}
			else if (_iKeyframeIndex < m_iActiveKeyFrameIndex)
			{
				if (m_iActiveKeyFrameIndex > 0)
				{
					// Active keyframe index shift
					m_iActiveKeyFrameIndex--;
				}
			}

			m_FrameServer.Metadata.RemoveAt(_iKeyframeIndex);
			m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
			UpdateKeyframesMarkers();
			OrganizeKeyframes();
			pbSurfaceScreen.Invalidate();
		}
		public void UpdateKeyframes()
		{
			// Primary selection has been image-adjusted,
			// some keyframes may have been impacted.

			bool bAtLeastOne = false;

			foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
			{
				if (kf.Position >= m_iSelStart && kf.Position <= m_iSelEnd)
				{
					kf.ImportImage(m_FrameServer.VideoFile.FrameList[(int)m_FrameServer.VideoFile.GetFrameNumber(kf.Position)].BmpImage);
					kf.GenerateDisabledThumbnail();
					bAtLeastOne = true;
				}
				else
				{
					// Outside selection : couldn't possibly be impacted.
				}
			}

			if (bAtLeastOne)
				OrganizeKeyframes();

		}
		private void pnlThumbnails_DoubleClick(object sender, EventArgs e)
		{
			// On double click in the thumbs panel : Add a keyframe at current pos.
			AddKeyframe();
			OnPoke();
		}

		#region ThumbBox event Handlers
		private void ThumbBoxClose(object sender, EventArgs e)
		{
			RemoveKeyframe((int)((KeyframeBox)sender).Tag);

			// Set as active screen is done after in case we don't have any keyframes left.
			OnPoke();
		}
		private void ThumbBoxClick(object sender, EventArgs e)
		{
			// Move to the right spot.
			OnPoke();
			StopPlaying();

			long iTargetPosition = m_FrameServer.Metadata[(int)((KeyframeBox)sender).Tag].Position;

			trkSelection.SelPos = iTargetPosition;
			m_iFramesToDecode = 1;


			ShowNextFrame(iTargetPosition, true);
			m_iCurrentPosition = iTargetPosition;

			UpdateNavigationCursor();
			if (m_bShowInfos) { UpdateDebugInfos(); }

			// On active sur la position réelle, au cas où on ne soit pas sur la frame demandée.
			// par ex, si la kf cliquée est hors zone
			ActivateKeyframe(m_iCurrentPosition);
		}
		private void ThumbBoxInfosClick(object sender, EventArgs e)
		{
			ThumbBoxClick(sender, e);
			m_KeyframeCommentsHub.UserActivated = true;
			ActivateKeyframe(m_iCurrentPosition);
		}
		#endregion

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
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Line2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorLine2D, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolPointer_Click(object sender, EventArgs e)
		{
			OnPoke();
			m_ActiveTool = DrawingToolType.Pointer;
			SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			if (m_FrameServer.Metadata.Count < 1)
			{
				DockKeyframePanel();
			}
		}
		private void btnDrawingToolCross2D_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Cross2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorCross2D, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolAngle2D_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Angle2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorAngle2D, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolPencil_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Pencil;
				UpdateCursor();
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolChrono_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Chrono;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorChrono, 0));
			}
		}
		private void btnMagnifier_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				m_ActiveTool = DrawingToolType.Pointer;

				// Magnifier is half way between a persisting tool (like trackers and chronometers).
				// and a mode like grid and 3dplane.
				if (m_Magnifier.Mode == MagnifierMode.NotVisible)
				{
					UnzoomDirectZoom();
					m_Magnifier.Mode = MagnifierMode.Direct;
					btnMagnifier.BackgroundImage = Resources.magnifierActive2;
					SetCursor(Cursors.Cross);
				}
				else if (m_Magnifier.Mode == MagnifierMode.Direct)
				{
					// Revert to no magnification.
					UnzoomDirectZoom();
					m_Magnifier.Mode = MagnifierMode.NotVisible;
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
			m_Magnifier.Mode = MagnifierMode.NotVisible;
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
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				OnPoke();
				m_ActiveTool = DrawingToolType.Text;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnShowComments_Click(object sender, EventArgs e)
		{
			OnPoke();

			if (m_FrameServer.VideoFile.Loaded)
			{
				// As of now, Keyframes infobox should display when we are on a keyframe
				m_KeyframeCommentsHub.UserActivated = true;

				if (m_iActiveKeyFrameIndex < 0)
				{
					// We are not on a keyframe but user asked to show the infos...
					// did he want to create a keyframe here and put some infos,
					// or did he only want to activate the infobox for next keyframes ?
					//
					// Since he clicked on the DrawingTools bar, we will act as if it was a Drawing,
					// and add a keyframe here in case there isn't already one.
					AddKeyframe();
				}

				m_KeyframeCommentsHub.UpdateContent(m_FrameServer.Metadata[m_iActiveKeyFrameIndex]);
				m_KeyframeCommentsHub.Visible = true;
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
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureDrawing fcd = new formConfigureDrawing(m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				LocateForm(fcd);
				fcd.ShowDialog();
				fcd.Dispose();
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		private void mnuConfigureFading_Click(object sender, EventArgs e)
		{
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureFading fcf = new formConfigureFading(m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				LocateForm(fcf);
				fcf.ShowDialog();
				fcf.Dispose();
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		private void mnuTrackTrajectory_Click(object sender, EventArgs e)
		{
			//---------------------------------------
			// Turn a Cross2D into a Track.
			// Cross2D was selected upon Right Click.
			//---------------------------------------

			// We force the user to be on the right frame.
			if (m_iActiveKeyFrameIndex >= 0 && m_iActiveKeyFrameIndex == m_FrameServer.Metadata.SelectedDrawingFrame)
			{
				int iSelectedDrawing = m_FrameServer.Metadata.SelectedDrawing;

				if (iSelectedDrawing >= 0)
				{
					// TODO - link to CommandAddTrajectory.
					// TODO - always insert at 0.

					// Ajouter un Track sur ce point.
					DrawingCross2D dc = (DrawingCross2D)m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings[iSelectedDrawing];
					m_FrameServer.Metadata.Tracks.Add(new Track(dc.CenterPoint.X, dc.CenterPoint.Y, m_iCurrentPosition, m_FrameServer.VideoFile.CurrentImage));

					// Complete Setup
					m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.Tracks.Count - 1].m_ShowClosestFrame = new ShowClosestFrame(OnShowClosestFrame);
					m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.Tracks.Count - 1].ParentMetadata = m_FrameServer.Metadata;
					m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.Tracks.Count - 1].MainColor = dc.PenColor;

					m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.Tracks.Count - 1].Status = Track.TrackStatus.Edit;
					m_FrameServer.Metadata.SelectedTrack = m_FrameServer.Metadata.Tracks.Count - 1;

					// Supprimer le point en tant que Drawing ?
					m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.RemoveAt(iSelectedDrawing);
					
					m_FrameServer.Metadata.SelectedDrawingFrame = -1;
					m_FrameServer.Metadata.SelectedDrawing = -1;

					// Return to the pointer tool.
					m_ActiveTool = DrawingToolType.Pointer;
					SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
				}
			}
			pbSurfaceScreen.Invalidate();
		}
		private void mnuGotoKeyframe_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				long iPosition = m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing].infosFading.ReferenceTimestamp;

				m_iFramesToDecode = 1;
				ShowNextFrame(iPosition, true);
				UpdateNavigationCursor();
				UpdateCurrentPositionLabel();
				trkSelection.SelPos = trkFrame.Position;
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void mnuShowMeasure_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the measure for this line.
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing] as DrawingLine2D;
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
			
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing] as DrawingLine2D;
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
			if (m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				IUndoableCommand cdd = new CommandDeleteDrawing(DoInvalidate, m_FrameServer.Metadata, m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Position, m_FrameServer.Metadata.SelectedDrawing);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cdd);
				pbSurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		#endregion
		
		#region Tracking Menus
		private void mnuStopTracking_Click(object sender, EventArgs e)
		{
			m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].StopTracking();
			pbSurfaceScreen.Invalidate();
		}
		private void mnuDeleteEndOfTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdeot = new CommandDeleteEndOfTrack(this, m_FrameServer.Metadata, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdeot);

			//m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].ChopTrajectory(m_iCurrentPosition);
			pbSurfaceScreen.Invalidate();
		}
		private void mnuRestartTracking_Click(object sender, EventArgs e)
		{
			m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack].RestartTracking();
			pbSurfaceScreen.Invalidate();
		}
		private void mnuDeleteTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteTrack(this, m_FrameServer.Metadata);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdc);
		}
		private void mnuConfigureTrajectory_Click(object sender, EventArgs e)
		{
			// Change this trajectory display.
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.DeactivateKeyboardHandler != null)
			{
				dp.DeactivateKeyboardHandler();
			}

			formConfigureTrajectoryDisplay fctd = new formConfigureTrajectoryDisplay(m_FrameServer.Metadata.Tracks[m_FrameServer.Metadata.SelectedTrack], pbSurfaceScreen);
			fctd.StartPosition = FormStartPosition.CenterScreen;
			fctd.ShowDialog();
			fctd.Dispose();

			if (dp.ActivateKeyboardHandler != null)
			{
				dp.ActivateKeyboardHandler();
			}
		}
		private void OnShowClosestFrame(Point _mouse, long _iBeginTimestamp, List<TrackPosition> _positions, int _iPixelTotalDistance, bool _b2DOnly)
		{
			//--------------------------------------------------------------------------
			// This is where the interactivity of the trajectory is done.
			// The user has draged or clicked the trajectory, we find the closest point
			// and we update to the corresponding frame.
			//--------------------------------------------------------------------------


			// Calcul de la distance 3D (x,y,t) de chaque point du track.
			// coordonnées descalées.

			double minDistance = double.MaxValue;
			int iClosestPoint = 0;

			if (_b2DOnly)
			{
				// Check the closest location on screen.
				for (int i = 0; i < _positions.Count; i++)
				{
					double dist = Math.Sqrt(((_mouse.X - _positions[i].X) * (_mouse.X - _positions[i].X))
					                        + ((_mouse.Y - _positions[i].Y) * (_mouse.Y - _positions[i].Y)));


					if (dist < minDistance)
					{
						minDistance = dist;
						iClosestPoint = i;
					}
				}
			}
			else
			{
				// Check closest location on screen, but giving priority to the one also close in time.
				// = distance in 3D.
				// Distance on t is not in the same unit as distance on x and y.
				// So first step is to normalize t.

				// _iPixelTotalDistance should be the flat distance (distance from topleft to bottomright)
				// not the added distances of each segments, otherwise it will be biased towards time.

				long TimeTotalDistance = _positions[_positions.Count -1].T - _positions[0].T;
				double scaleFactor = (double)TimeTotalDistance / (double)_iPixelTotalDistance;

				for (int i = 0; i < _positions.Count; i++)
				{
					double fTimeDistance = (double)(m_iCurrentPosition - _iBeginTimestamp - _positions[i].T);

					double dist = Math.Sqrt(((_mouse.X - _positions[i].X) * (_mouse.X - _positions[i].X))
					                        + ((_mouse.Y - _positions[i].Y) * (_mouse.Y - _positions[i].Y))
					                        + ((long)(fTimeDistance / scaleFactor) * (long)(fTimeDistance / scaleFactor)));

					if (dist < minDistance)
					{
						minDistance = dist;
						iClosestPoint = i;
					}
				}

			}

			// move to corresponding timestamp.
			m_iFramesToDecode = 1;
			ShowNextFrame(_positions[iClosestPoint].T + _iBeginTimestamp, true);
			UpdateNavigationCursor();
			UpdateCurrentPositionLabel();
			trkSelection.SelPos = trkFrame.Position;
		}
		#endregion

		#region Chronometers Menus
		private void mnuChronoStart_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_FrameServer.Metadata, DrawingChrono.ChronoModificationType.TimeStart, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoStop_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_FrameServer.Metadata, DrawingChrono.ChronoModificationType.TimeStop, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoHide_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_FrameServer.Metadata, DrawingChrono.ChronoModificationType.TimeHide, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoCountdown_Click(object sender, EventArgs e)
		{
			// This menu should only be accessible if we have a "Stop" value.
			mnuChronoCountdown.Checked = !mnuChronoCountdown.Checked;
			
			IUndoableCommand cmc = new CommandModifyChrono(this, m_FrameServer.Metadata, DrawingChrono.ChronoModificationType.Countdown, (mnuChronoCountdown.Checked == true)?1:0);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
			
			pbSurfaceScreen.Invalidate();
		}
		private void mnuChronoDelete_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteChrono(this, m_FrameServer.Metadata);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdc);
		}
		private void mnuChronoConfigure_Click(object sender, EventArgs e)
		{
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.DeactivateKeyboardHandler != null)
			{
				dp.DeactivateKeyboardHandler();
			}
			
			// Change this chrono display.
			formConfigureChrono fcc = new formConfigureChrono(m_FrameServer.Metadata.Chronos[m_FrameServer.Metadata.SelectedChrono], pbSurfaceScreen);
			LocateForm(fcc);
			fcc.ShowDialog();
			fcc.Dispose();
			pbSurfaceScreen.Invalidate();

			if (dp.ActivateKeyboardHandler != null)
			{
				dp.ActivateKeyboardHandler();
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
			m_FrameServer.CoordinateSystem.Zoom = m_Magnifier.ZoomFactor;
			m_FrameServer.CoordinateSystem.RelocateZoomWindow(m_Magnifier.MagnifiedCenter);
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
			m_Magnifier.ZoomFactor = _fValue;
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
			if (m_Magnifier.Mode != MagnifierMode.NotVisible)
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
			if (m_FrameServer.CoordinateSystem.Zooming)
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
			StopPlaying();
			m_ActiveTool = DrawingToolType.Pointer;
			SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			DisableMagnifier();
			UnzoomDirectZoom();
		}
		private void EnableDisableAllPlayingControls(bool _bEnable)
		{
			btnSetHandlerLeft.Enabled = _bEnable;
			btnSetHandlerRight.Enabled = _bEnable;
			btnHandlersReset.Enabled = _bEnable;
			btn_HandlersLock.Enabled = _bEnable;
			
			buttonGotoFirst.Enabled = _bEnable;
			buttonGotoLast.Enabled = _bEnable;
			buttonGotoNext.Enabled = _bEnable;
			buttonGotoPrevious.Enabled = _bEnable;
			buttonPlay.Enabled = _bEnable;
			buttonPlayingMode.Enabled = _bEnable;
			
			lblSpeedTuner.Enabled = _bEnable;
			trkFrame.EnableDisable(_bEnable);
			trkSelection.EnableDisable(_bEnable);
			sldrSpeed.Enabled = _bEnable;
			
			btnRafale.Enabled = _bEnable;
			btnSaveVideo.Enabled = _bEnable;
			btnDiaporama.Enabled = _bEnable;
			btnPausedVideo.Enabled = _bEnable;
		}
		private void EnableDisableDrawingTools(bool _bEnable)
		{
			btnShowComments.Enabled = _bEnable;
			btn3dplane.Enabled = _bEnable;
			btnDrawingToolAngle2D.Enabled = _bEnable;
			btnDrawingToolChrono.Enabled = _bEnable;
			btnDrawingToolCross2D.Enabled = _bEnable;
			btnDrawingToolLine2D.Enabled = _bEnable;
			btnDrawingToolPencil.Enabled = _bEnable;
			btnDrawingToolPointer.Enabled = _bEnable;
			btnDrawingToolText.Enabled = _bEnable;
			btnMagnifier.Enabled = _bEnable;
			btnColorProfile.Enabled = _bEnable;
			btnAddKeyframe.Enabled = _bEnable;
		}
		#endregion
		
		#region Importing selection to memory
		public void ImportSelectionToMemory(bool _bForceReload)
		{
			//-------------------------------------------------------------------------------------
			// Switch the current selection to memory if possible.
			// Called at video load after first frame load, recalling a screen memo on undo,
			// and when the user manually modifies the selection.
			// At this point the selection sentinels (m_iSelStart and m_iSelEnd) must be good.
			// They would have been positionned from file data or from trkSelection pixel mapping.
			// The internal data of the trkSelection should also already have been updated.
			//
			// After the selection is imported, we may have different values than before
			// regarding selection sentinels because: 
			// - the video ending timestamp may have been misadvertised in the file,
			// - the timestamps may not be linear so the mapping with the trkSelection isn't perfect.
			// We check and fix these discrepancies.
			//
			// Public because accessed from PlayerServer.Deinterlace property 
			//-------------------------------------------------------------------------------------
			if (m_FrameServer.VideoFile.Loaded)
			{
				if (m_FrameServer.VideoFile.CanExtractToMemory(m_iSelStart, m_iSelEnd, m_PrefManager.WorkingZoneSeconds, m_PrefManager.WorkingZoneMemory))
				{
					StopPlaying();
					
					formFramesImport ffi = new formFramesImport(m_FrameServer.VideoFile, m_iSelStart, m_iSelEnd, _bForceReload);
					ffi.ShowDialog();
					
					if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 0)
					{
						// It didn't work. (Operation canceled, or failed).
						log.Debug("Extract to memory canceled or failed, reload first frame.");
						m_iFramesToDecode = 1;
						ShowNextFrame(m_iSelStart, true);
						UpdateNavigationCursor();
					}
					
					ffi.Dispose();
					
				}
				else if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
				{
					// Exiting Analysis mode.
					// TODO - free memory for images now ?
					m_FrameServer.VideoFile.Selection.iAnalysisMode = 0;
				}

				// Here, we may have changed mode.
				if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
				{
					// We now have solid facts. Update all variables with them.
					m_iSelStart = m_FrameServer.VideoFile.GetTimeStamp(0);
					m_iSelEnd = m_FrameServer.VideoFile.GetTimeStamp(m_FrameServer.VideoFile.Selection.iDurationFrame - 1);
					double fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
					m_iSelDuration = (long)((double)(m_iSelEnd - m_iSelStart) + fAverageTimeStampsPerFrame);

					if(trkSelection.SelStart != m_iSelStart) trkSelection.SelStart = m_iSelStart;
					if(trkSelection.SelEnd != m_iSelEnd) trkSelection.SelEnd = m_iSelEnd;
					
					// Remap frame tracker with solid data.
					trkFrame.Remap(m_iSelStart, m_iSelEnd);
					trkFrame.ReportOnMouseMove = true;

					// Display first frame.
					m_iFramesToDecode = 1;
					ShowNextFrame(m_iSelStart, true);
					UpdateNavigationCursor();
				}
				else
				{
					/*
					m_iSelStart = trkSelection.SelStart;
					// Hack : If we changed the trkSelection.SelEnd before the trkSelection.SelStart
					// (As we do when we first load the video), the selstart will not take into account
					// a possible shift of unreadable first frames.
					// We make the ad-hoc modif here.
					if (m_iSelStart < m_iStartingPosition) m_iSelStart = m_iStartingPosition;
				
					m_iSelEnd = trkSelection.SelEnd;
					*/
					
					double fAverageTimeStampsPerFrame = m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds / m_FrameServer.VideoFile.Infos.fFps;
					m_iSelDuration = (long)((double)(m_iSelEnd - m_iSelStart) + fAverageTimeStampsPerFrame);

					// Remap frame tracker.
					trkFrame.Remap(m_iSelStart, m_iSelEnd);
					trkFrame.ReportOnMouseMove = false;
				}

				UpdateSelectionLabels();
				OnPoke();

				m_PlayerScreenUIHandler.PlayerScreenUI_SelectionChanged(true);
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		#endregion
		
		#region Export video and frames
		private void btnSnapShot_Click(object sender, EventArgs e)
		{
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
			}
		}
		private void btnRafale_Click(object sender, EventArgs e)
		{
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
			}
		}
		public void SaveImageSequence(BackgroundWorker bgWorker, string _FilePath, Int64 _iIntervalTimeStamps, bool _bBlendDrawings, bool _bKeyframesOnly, int iEstimatedTotal)
		{
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

			pbSurfaceScreen.Invalidate();
		}
		private void btnVideo_Click(object sender, EventArgs e)
		{
			StopPlaying();
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.DeactivateKeyboardHandler != null)
			{
				dp.DeactivateKeyboardHandler();
			}	
			
			Save();
				
			if (dp.ActivateKeyboardHandler != null)
			{
				dp.ActivateKeyboardHandler();
			}
		}
		private void btnDiaporama_Click(object sender, EventArgs e)
		{
			bool bDiapo = sender == btnDiaporama;
			
			if(m_FrameServer.Metadata.Keyframes.Count < 1)
			{
				if(bDiapo)
				{
					MessageBox.Show(ScreenManagerLang.Error_SaveDiaporama_NoKeyframes.Replace("\\n", "\n"),
				                ScreenManagerLang.Error_SaveDiaporama,
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);
				}
				else
				{
					MessageBox.Show(ScreenManagerLang.Error_SavePausedVideo_NoKeyframes.Replace("\\n", "\n"),
				                ScreenManagerLang.Error_SavePausedVideo,
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);
				}
			}
			else if ((m_FrameServer.VideoFile.Loaded) && (m_FrameServer.VideoFile.CurrentImage != null))
			{
				StopPlaying();
			
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}
	
				m_FrameServer.SaveDiaporama(m_iSelStart, m_iSelEnd, new DelegateGetOutputBitmap(GetOutputBitmap), bDiapo);

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}
			}
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
		public void Save()
		{
            // Todo:
            // Eventually, this call should be done directly by PlayerScreen, without passing through the UI.
			// This will be possible when m_FrameServer.Metadata, m_iSelStart, m_iSelEnd are encapsulated in m_FrameServer
			// and when PlaybackFrameInterval, m_iSlowmotionPercentage, GetOutputBitmap are available publically.
			
			m_FrameServer.Save(	GetPlaybackFrameInterval(), 
			                   		m_iSlowmotionPercentage, 
			                   		m_iSelStart, 
			                       	m_iSelEnd,
			                       	new DelegateGetOutputBitmap(GetOutputBitmap));
		}
		private bool GetOutputBitmap(Graphics _canvas, long _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly)
		{
			// Used by the VideoFile for SaveMovie.
			// The image to save was already retrieved (from stream or analysis array)
			// This image is already drawn on _canvas.
			// Here we we flush the drawings on it if needed.
			// We return a boolean indicating if it's a key image or not.
			// This can then be used by the caller.

			int iKeyFrameIndex = -1;
			int iCurrentKeyframe = 0;
			bool bFound = false;
			while (!bFound && iCurrentKeyframe < m_FrameServer.Metadata.Count)
			{
				if (m_FrameServer.Metadata[iCurrentKeyframe].Position == _iTimestamp)
				{
					bFound = true;
					iKeyFrameIndex = iCurrentKeyframe;
				}
				else
				{
					iCurrentKeyframe++;
				}
			}

			if (!_bKeyframesOnly || iKeyFrameIndex >= 0)
			{				
				if (_bFlushDrawings)
				{
					FlushDrawingsOnGraphics(_canvas, iKeyFrameIndex, _iTimestamp, 1.0f, 1.0f, new Point(0,0));
				}
			}

			return (iKeyFrameIndex >= 0);
		}
		private Bitmap GetFlushedImage()
		{
			// Returns an image with all drawings flushed, including
			// grids, chronos, magnifier, etc.
			// image should be at same strech factor than the one visible on screen.

			Size iNewSize = new Size((int)((double)m_FrameServer.VideoFile.CurrentImage.Width * m_FrameServer.CoordinateSystem.Stretch), (int)((double)m_FrameServer.VideoFile.CurrentImage.Height * m_FrameServer.CoordinateSystem.Stretch));
			Bitmap output = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
			output.SetResolution(m_FrameServer.VideoFile.CurrentImage.HorizontalResolution, m_FrameServer.VideoFile.CurrentImage.VerticalResolution);

			if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
			{
				m_DrawingFilterOutput.Draw(Graphics.FromImage(output), iNewSize, m_DrawingFilterOutput.InputFrames, m_DrawingFilterOutput.PrivateData);
			}
			else
			{
				int iKeyFrameIndex = -1;
				if (m_iActiveKeyFrameIndex >= 0 && m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
				{
					iKeyFrameIndex = m_iActiveKeyFrameIndex;
				}
				
				FlushOnGraphics(m_FrameServer.VideoFile.CurrentImage, Graphics.FromImage(output), iNewSize, iKeyFrameIndex, m_iCurrentPosition);
			}
			
			return output;
		}
		private string BuildFilename(string _FilePath, Int64 _position, TimeCodeFormat _timeCodeFormat)
		{
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

		#region Memo & Reset
		public MemoPlayerScreen GetMemo()
		{
			return new MemoPlayerScreen(m_iSelStart, m_iSelEnd);
		}
		public void ResetSelectionImages(MemoPlayerScreen _memo)
		{
			// This is typically called when undoing image adjustments.
			// We do not actually undo the adjustment because we don't have the original data anymore.
			// We emulate it by reloading the selection.
			
			// Memorize the current selection boundaries.
			MemoPlayerScreen mps = new MemoPlayerScreen(m_iSelStart, m_iSelEnd);

			// Reset the selection to whatever it was when we did the image adjustment.
			m_iSelStart = _memo.SelStart;
			m_iSelEnd = _memo.SelEnd;

			// Undo all adjustments made on this portion.
			ImportSelectionToMemory(true);
			UpdateKeyframes();

			// Reset to the current selection.
			m_iSelStart = mps.SelStart;
			m_iSelEnd = mps.SelEnd;
		}
		#endregion

	}
}
