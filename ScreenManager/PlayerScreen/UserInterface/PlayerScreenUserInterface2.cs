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
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;
using Kinovea.VideoFiles;

#endregion

namespace Kinovea.ScreenManager
{
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
		
		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
   										int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc,  TernaryRasterOperations dwRop);    

		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		static extern IntPtr CreateCompatibleDC(IntPtr hdc);
		
		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		
		[DllImport("gdi32.dll", ExactSpelling=true, SetLastError=true)]
		static extern bool DeleteObject(IntPtr hObject);

		public enum TernaryRasterOperations : uint {
		    SRCCOPY     = 0x00CC0020,
		    SRCPAINT    = 0x00EE0086,
		    SRCAND      = 0x008800C6,
		    SRCINVERT   = 0x00660046,
		    SRCERASE    = 0x00440328,
		    NOTSRCCOPY  = 0x00330008,
		    NOTSRCERASE = 0x001100A6,
		    MERGECOPY   = 0x00C000CA,
		    MERGEPAINT  = 0x00BB0226,
		    PATCOPY     = 0x00F00021,
		    PATPAINT    = 0x00FB0A09,
		    PATINVERT   = 0x005A0049,
		    DSTINVERT   = 0x00550009,
		    BLACKNESS   = 0x00000042,
		    WHITENESS   = 0x00FF0062
		}
		#endregion

		#region Internal delegates for async methods
		private delegate void TimerEventHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2);
		private delegate void PlayLoop();
        private TimerEventHandler m_TimerEventHandler;
        private PlayLoop m_PlayLoop;
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
		public double FrameInterval
		{
			get 
			{
				return (m_FrameServer.VideoFile.Infos.fFrameInterval / (m_fSlowmotionPercentage / 100));
			}
		}
		public double RealtimePercentage
		{
			// RealtimePercentage expresses the speed percentage relative to real time action.
			// It takes high speed camera into account.
			get 
			{ 
				return m_fSlowmotionPercentage / m_fHighSpeedFactor;
			}
			set
			{
				// This happens only in the context of synching 
				// when the other video changed its speed percentage (user or forced).
                // We must NOT trigger the event here, or it will impact the other screen in an infinite loop.
				// Compute back the slow motion percentage relative to the playback framerate.
				double fPlaybackPercentage = value * m_fHighSpeedFactor;
				if(fPlaybackPercentage > 200) fPlaybackPercentage = 200;
				sldrSpeed.Value = (int)fPlaybackPercentage;
				
				// If the other screen is in high speed context, we honor the decimal value.
				// (When it will be changed from this screen's slider, it will be an integer value).
				m_fSlowmotionPercentage = fPlaybackPercentage > 0 ? fPlaybackPercentage : 1;
				
				// Reset timer with new value.
				if (m_bIsCurrentlyPlaying)
				{
					StopMultimediaTimer();
					StartMultimediaTimer(GetPlaybackFrameInterval());
				}

				UpdateSpeedLabel();
			}
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
					UpdateCurrentPositionLabel();
					
					m_bSyncMerge = false;
					if(m_SyncMergeImage != null)
						m_SyncMergeImage.Dispose();
				}
				
				buttonPlayingMode.Enabled = !m_bSynched;
			}
		}
		public Int64 SelectionDuration
		{
			// The duration of the selection in ts.
			get { return m_iSelDuration; }	
		}
		public Int64 SyncPosition
		{
			// The absolute ts of the sync point for this video.
			get { return m_iSyncPosition; }
			set
			{
				m_iSyncPosition = value;
				trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
				UpdateCurrentPositionLabel();
			}
		}
		public Int64 SyncCurrentPosition
		{
			// The current ts, relative to the selection.
			get { return m_iCurrentPosition - m_iSelStart; }
		}
		public bool SyncMerge
		{
			// Idicates whether we should draw the other screen image on top of this one.
			get { return m_bSyncMerge; }
			set
			{
				m_bSyncMerge = value;
				
				m_FrameServer.CoordinateSystem.FreeMove = m_bSyncMerge;
				
				if(!m_bSyncMerge && m_SyncMergeImage != null)
				{
					m_SyncMergeImage.Dispose();
				}
				
				DoInvalidate();
			}
		}
		public bool DualSaveInProgress
        {
        	set { m_DualSaveInProgress = value; }
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
		private uint m_IdMultimediaTimer;
		private PlayingMode m_ePlayingMode = PlayingMode.Loop;
		private int m_iDroppedFrames;                  // For debug purposes only.
		private int m_iDecodedFrames;
		private double m_fSlowmotionPercentage = 100.0f;	// Always between 1 and 200 : this specific value is not impacted by high speed cameras.
		private bool m_bIsIdle = true;
		
		// Synchronisation
		private bool m_bSynched;
		private Int64 m_iSyncPosition;
		private bool m_bSyncMerge;
		private Bitmap m_SyncMergeImage;
		private ColorMatrix m_SyncMergeMatrix = new ColorMatrix();
		private ImageAttributes m_SyncMergeImgAttr = new ImageAttributes();
		private bool m_DualSaveInProgress;
		
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
		private AbstractDrawingTool m_ActiveTool;
		private DrawingToolPointer m_PointerTool;
		
		private formKeyframeComments m_KeyframeCommentsHub;
		private bool m_bDocked = true;
		private bool m_bTextEdit;
		private Point m_DescaledMouse;

		// Video Filters Management
		private bool m_bDrawtimeFiltered;
		private DrawtimeFilterOutput m_DrawingFilterOutput;
		
		// Others
		private Double m_fHighSpeedFactor = 1.0f;           	// When capture fps is different from Playing fps.
		private CoordinateSystem m_CoordinateSystem = new CoordinateSystem();
		private System.Windows.Forms.Timer m_DeselectionTimer = new System.Windows.Forms.Timer();
		private MessageToaster m_MessageToaster;
		
		#region Context Menus
		private ContextMenuStrip popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuDirectTrack = new ToolStripMenuItem();
		private ToolStripMenuItem mnuPlayPause = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSetCaptureSpeed = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSavePic = new ToolStripMenuItem();
		private ToolStripMenuItem mnuSendPic = new ToolStripMenuItem();
		private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();

		private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
		private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
		private ToolStripMenuItem mnuConfigureFading = new ToolStripMenuItem();
		private ToolStripMenuItem mnuConfigureOpacity = new ToolStripMenuItem();
		private ToolStripMenuItem mnuTrackTrajectory = new ToolStripMenuItem();
		private ToolStripMenuItem mnuGotoKeyframe = new ToolStripMenuItem();
		private ToolStripSeparator mnuSepDrawing = new ToolStripSeparator();
		private ToolStripSeparator mnuSepDrawing2 = new ToolStripSeparator();
		private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
		
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

		ToolStripButton m_btnAddKeyFrame;
		ToolStripButton m_btnShowComments;
		ToolStripButton m_btnToolPresets;
		
		// Debug
		private bool m_bShowInfos;
		private Stopwatch m_Stopwatch = new Stopwatch();
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
			SyncSetAlpha(0.5f);
			m_MessageToaster = new MessageToaster(pbSurfaceScreen);
			
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
			DockKeyframePanel(true);

			// Internal delegates
			m_TimerEventHandler = new TimerEventHandler(MultimediaTimer_Tick);
			m_PlayLoop = new PlayLoop(PlayLoop_Invoked);

			m_DeselectionTimer.Interval = 3000;
			m_DeselectionTimer.Tick += new EventHandler(DeselectionTimer_OnTick);

			EnableDisableActions(false);
			//SetupDebugPanel();
		}
		#endregion
		
		#region Public Methods
		public void ResetToEmptyState()
		{
			// Called when we load a new video over an already loaded screen.
			// also recalled if the video loaded but the first frame cannot be displayed.

			log.Debug("Reset screen to empty state.");
			
			// 1. Reset all data.
			m_FrameServer.Unload();
			ResetData();
			
			// 2. Reset all interface.
			ShowHideResizers(false);
			SetupPrimarySelectionPanel();
			pnlThumbnails.Controls.Clear();
			DockKeyframePanel(true);
			UpdateFramesMarkers();
			trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
			EnableDisableAllPlayingControls(true);
			EnableDisableDrawingTools(true);
			EnableDisableSnapshot(true);
			buttonPlay.Image = Resources.liqplay17;
			sldrSpeed.Value = 100;
			sldrSpeed.Enabled = false;
			lblFileName.Text = "";
			m_KeyframeCommentsHub.Hide();
			UpdatePlayingModeButton();
			
			m_PlayerScreenUIHandler.PlayerScreenUI_Reset();
		}
		public void EnableDisableActions(bool _bEnable)
		{
			// Called back after a load error.
			// Prevent any actions.
			if(!_bEnable)
				DisablePlayAndDraw();
			
			EnableDisableSnapshot(_bEnable);
			EnableDisableDrawingTools(_bEnable);
			
			if(_bEnable && m_FrameServer.Loaded && m_FrameServer.VideoFile.Infos.iDurationTimeStamps == 1)
			{
				// If we are in the special case of a one-frame video, disable playback controls.
				EnableDisableAllPlayingControls(false);
			}
			else
			{
				EnableDisableAllPlayingControls(_bEnable);				
			}
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
				log.Debug(String.Format("Timestamp after loading first frame : {0}", m_iCurrentPosition));
				
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
					DoInvalidate();

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
					m_PointerTool.SetImageSize(m_FrameServer.Metadata.ImageSize);
					
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

					UpdateFramesMarkers();
					
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
				DockKeyframePanel(false);
			}
			
			// Goto selection start and refresh.
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iSelStart, true);
			UpdateNavigationCursor();
			ActivateKeyframe(m_iCurrentPosition);

			m_FrameServer.SetupMetadata();
			m_PointerTool.SetImageSize(m_FrameServer.Metadata.ImageSize);

			DoInvalidate();
		}
		public void DisplayAsActiveScreen(bool _bActive)
		{
			// Called from ScreenManager.
			ShowBorder(_bActive);
		}
		public void StopPlaying()
		{
			StopPlaying(true);
		}
		public void SyncSetCurrentFrame(Int64 _iFrame, bool _bAllowUIUpdate)
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
						ShowNextFrame(-1, _bAllowUIUpdate);
					}
				}
				else
				{
					m_iCurrentPosition = _iFrame * m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame;
					m_iCurrentPosition += m_iSelStart;
					
					if (m_iCurrentPosition > m_iSelEnd) m_iCurrentPosition = m_iSelEnd;
					
					ShowNextFrame(m_iCurrentPosition, _bAllowUIUpdate);
				}

				if(_bAllowUIUpdate)
				{
					UpdateNavigationCursor();
					UpdateCurrentPositionLabel();
					ActivateKeyframe(m_iCurrentPosition);
					trkSelection.SelPos = trkFrame.Position;
					
					if (m_bShowInfos) { UpdateDebugInfos(); }
				}
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
			
			m_FrameServer.Metadata.CalibrationHelper.CurrentSpeedUnit = m_PrefManager.SpeedUnit;
			m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();

			// Refresh image to update timecode in chronos, grids colors, default fading, etc.
			DoInvalidate();
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
				DockKeyframePanel(true);
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
		public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
		{
			//if(m_SyncMergeImage != null)
			//	m_SyncMergeImage.Dispose();
			
			m_SyncMergeImage = _SyncMergeImage;
				
			if(_bUpdateUI)
			{
				// Ask for a repaint. We don't wait for the next frame to be drawn
				// because the user may be manually moving the other video.
				DoInvalidate();
			}
		}
		public bool OnKeyPress(Keys _keycode)
		{
			bool bWasHandled = false;
			
			// Disabled completely if no video.
			if (m_FrameServer.VideoFile.Loaded)
			{
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
							DoInvalidate();
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
			}

			return bWasHandled;
		}
		public void UpdateImageSize()
		{
			Size imageSize = new Size(m_FrameServer.VideoFile.Infos.iDecodingWidth, m_FrameServer.VideoFile.Infos.iDecodingHeight);
			
			m_FrameServer.Metadata.ImageSize = imageSize;
			
			m_PointerTool.SetImageSize(m_FrameServer.Metadata.ImageSize);
			
			m_FrameServer.CoordinateSystem.SetOriginalSize(m_FrameServer.Metadata.ImageSize);
			m_FrameServer.CoordinateSystem.ReinitZoom();
			
			StretchSqueezeSurface();
		}
		public void AddImageDrawing(string _filename, bool _bIsSvg)
		{
			// Add an image drawing from a file.
			// Mimick all the actions that are normally taken when we select a drawing tool and click on the image.
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded)
			{
				BeforeAddImageDrawing();
			
				if(File.Exists(_filename))
				{
					try
					{
						if(_bIsSvg)
						{
							DrawingSVG dsvg = new DrawingSVG(m_FrameServer.Metadata.ImageSize.Width,
							                                 m_FrameServer.Metadata.ImageSize.Height, 
							                                 m_iCurrentPosition, 
							                                 m_FrameServer.Metadata.AverageTimeStampsPerFrame, 
							                                 _filename);
						
    						m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(dsvg);
						}
						else
						{
							DrawingBitmap dbmp = new DrawingBitmap( m_FrameServer.Metadata.ImageSize.Width,
							                                 		m_FrameServer.Metadata.ImageSize.Height, 
							                                 		m_iCurrentPosition, 
							                                 		m_FrameServer.Metadata.AverageTimeStampsPerFrame, 
							                                 		_filename);
						
							m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(dbmp);	
						}
					}
					catch
					{
						// An error occurred during the creation.
						// example : external DTD an no network or invalid svg file.
						// TODO: inform the user.
					}
				}
				
				AfterAddImageDrawing();
			}	
		}
		public void AddImageDrawing(Bitmap _bmp)
		{
			// Add an image drawing from a bitmap.
			// Mimick all the actions that are normally taken when we select a drawing tool and click on the image.
			// TODO: use an actual DrawingTool class for this!?
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded)
			{
				BeforeAddImageDrawing();
			
				DrawingBitmap dbmp = new DrawingBitmap( m_FrameServer.Metadata.ImageSize.Width,
							                                 		m_FrameServer.Metadata.ImageSize.Height, 
							                                 		m_iCurrentPosition, 
							                                 		m_FrameServer.Metadata.AverageTimeStampsPerFrame, 
							                                 		_bmp);
						
				m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(dbmp);
				
				AfterAddImageDrawing();
			}
		}
		private void BeforeAddImageDrawing()
		{
			if(m_bIsCurrentlyPlaying)
			{
				StopPlaying();
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				ActivateKeyframe(m_iCurrentPosition);	
			}
					
			PrepareKeyframesDock();
			m_FrameServer.Metadata.AllDrawingTextToNormalMode();
			m_FrameServer.Metadata.SelectedExtraDrawing = -1;
			
			// Add a KeyFrame here if it doesn't exist.
			AddKeyframe();
			
		}
		private void AfterAddImageDrawing()
		{
			m_FrameServer.Metadata.SelectedDrawingFrame = -1;
			m_FrameServer.Metadata.SelectedDrawing = -1;
			
			m_ActiveTool = m_PointerTool;
			SetCursor(m_PointerTool.GetCursor(0));
			
			DoInvalidate();
		}
		#endregion
		
		#region Various Inits & Setups
		private void InitializeDrawingTools()
		{
			m_PointerTool = new DrawingToolPointer();
			m_ActiveTool = m_PointerTool;
			
			stripDrawingTools.Left = 3;
			EventHandler handler = new EventHandler(drawingTool_Click);
			
			// Special button: Add key image
			m_btnAddKeyFrame = CreateToolButton();
        	m_btnAddKeyFrame.Image = Resources.page_white_go;
        	m_btnAddKeyFrame.Click += new EventHandler(btnAddKeyframe_Click);
        	m_btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
        	stripDrawingTools.Items.Add(m_btnAddKeyFrame);
        	
			AddToolButton(m_PointerTool, handler);
			stripDrawingTools.Items.Add(new ToolStripSeparator());
			
			// Special button: Key image comments
			m_btnShowComments = CreateToolButton();
        	m_btnShowComments.Image = Resources.comments2;
        	m_btnShowComments.Click += new EventHandler(btnShowComments_Click);
        	m_btnShowComments.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
        	stripDrawingTools.Items.Add(m_btnShowComments);
        	
			AddToolButton(ToolManager.Label, handler);
			AddToolButton(ToolManager.Pencil, handler);
			AddToolButton(ToolManager.Line, handler);
			AddToolButton(ToolManager.Circle, handler);
			AddToolButton(ToolManager.CrossMark, handler);
			AddToolButton(ToolManager.Angle, handler);
			AddToolButton(ToolManager.Chrono, handler);
			AddToolButton(ToolManager.Plane, handler);
			AddToolButton(ToolManager.Magnifier, new EventHandler(btnMagnifier_Click));
						
			// Tool presets
			m_btnToolPresets = CreateToolButton();
        	m_btnToolPresets.Image = Resources.SwatchIcon3;
        	m_btnToolPresets.Click += new EventHandler(btnColorProfile_Click);
        	m_btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
        	stripDrawingTools.Items.Add(m_btnToolPresets);
		}
		private ToolStripButton CreateToolButton()
		{
			ToolStripButton btn = new ToolStripButton();
			btn.AutoSize = false;
        	btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
        	btn.ImageScaling = ToolStripItemImageScaling.None;
        	btn.Size = new Size(25, 25);
        	btn.AutoToolTip = false;
        	return btn;
		}
		private void AddToolButton(AbstractDrawingTool _tool, EventHandler _handler)
		{
			ToolStripButton btn = CreateToolButton();
        	btn.Image = _tool.Icon;
        	btn.Tag = _tool;
        	btn.Click += _handler;
        	btn.ToolTipText = _tool.DisplayName;
        	stripDrawingTools.Items.Add(btn);
		}
		private void ResetData()
		{
			m_iFramesToDecode = 1;
			
			m_fSlowmotionPercentage = 100.0;
			m_bDrawtimeFiltered = false;
			m_bIsCurrentlyPlaying = false;
			m_bSeekToStart = false;
			m_ePlayingMode = PlayingMode.Loop;
			m_bStretchModeOn = false;
			m_FrameServer.CoordinateSystem.Reset();
			
			// Sync
			m_bSynched = false;
			m_iSyncPosition = 0;
			m_bSyncMerge = false;
			if(m_SyncMergeImage != null)
				m_SyncMergeImage.Dispose();
			
			m_bShowImageBorder = false;
			
			SetupPrimarySelectionData(); 	// Should not be necessary when every data is coming from m_FrameServer.
			
			m_bHandlersLocked = false;
			
			m_iActiveKeyFrameIndex = -1;
			m_ActiveTool = m_PointerTool;
			
			m_bDocked = true;
			m_bTextEdit = false;
			DrawingToolLine2D.ShowMeasure = false;
			DrawingToolCross2D.ShowCoordinates = false;
			
			m_bDrawtimeFiltered = false;
			
			m_fHighSpeedFactor = 1.0f;
			UpdateSpeedLabel();
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
				UpdateFramesMarkers();
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
			mnuDirectTrack.Click += new EventHandler(mnuDirectTrack_Click);
			mnuDirectTrack.Image = Properties.Drawings.track;
			mnuPlayPause.Click += new EventHandler(buttonPlay_Click);
			mnuSetCaptureSpeed.Click += new EventHandler(mnuSetCaptureSpeed_Click);
			mnuSetCaptureSpeed.Image = Properties.Resources.camera_speed;
			mnuSavePic.Click += new EventHandler(btnSnapShot_Click);
			mnuSavePic.Image = Properties.Resources.picture_save;
			mnuSendPic.Click += new EventHandler(mnuSendPic_Click);
			mnuSendPic.Image = Properties.Resources.image;
			mnuCloseScreen.Click += new EventHandler(btnClose_Click);
			mnuCloseScreen.Image = Properties.Resources.film_close3;
			popMenu.Items.AddRange(new ToolStripItem[] { mnuDirectTrack, mnuSetCaptureSpeed, mnuSavePic, mnuSendPic, new ToolStripSeparator(), mnuCloseScreen });

			// 2. Drawings context menu (Configure, Delete, Track this)
			mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);
			mnuConfigureDrawing.Image = Properties.Drawings.configure;
			mnuConfigureFading.Click += new EventHandler(mnuConfigureFading_Click);
			mnuConfigureFading.Image = Properties.Drawings.persistence;
			mnuConfigureOpacity.Click += new EventHandler(mnuConfigureOpacity_Click);
			mnuConfigureOpacity.Image = Properties.Drawings.persistence;
			mnuTrackTrajectory.Click += new EventHandler(mnuTrackTrajectory_Click);
			mnuTrackTrajectory.Image = Properties.Drawings.track;
			mnuGotoKeyframe.Click += new EventHandler(mnuGotoKeyframe_Click);
			mnuGotoKeyframe.Image = Properties.Resources.page_white_go;
			mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);
			mnuDeleteDrawing.Image = Properties.Drawings.delete;
			
			// 3. Tracking pop menu (Restart, Stop tracking)
			mnuStopTracking.Click += new EventHandler(mnuStopTracking_Click);
			mnuStopTracking.Visible = false;
			mnuStopTracking.Image = Properties.Drawings.trackstop;
			mnuRestartTracking.Click += new EventHandler(mnuRestartTracking_Click);
			mnuRestartTracking.Visible = false;
			mnuRestartTracking.Image = Properties.Drawings.trackingplay;
			mnuDeleteTrajectory.Click += new EventHandler(mnuDeleteTrajectory_Click);
			mnuDeleteTrajectory.Image = Properties.Drawings.delete;
			mnuDeleteEndOfTrajectory.Click += new EventHandler(mnuDeleteEndOfTrajectory_Click);
			//mnuDeleteEndOfTrajectory.Image = Properties.Resources.track_trim2;
			mnuConfigureTrajectory.Click += new EventHandler(mnuConfigureTrajectory_Click);
			mnuConfigureTrajectory.Image = Properties.Drawings.configure;
			popMenuTrack.Items.AddRange(new ToolStripItem[] { mnuConfigureTrajectory, new ToolStripSeparator(), mnuStopTracking, mnuRestartTracking, new ToolStripSeparator(), mnuDeleteEndOfTrajectory, mnuDeleteTrajectory });

			// 4. Chrono pop menu (Start, Stop, Hide, etc.)
			mnuChronoConfigure.Click += new EventHandler(mnuChronoConfigure_Click);
			mnuChronoConfigure.Image = Properties.Drawings.configure;
			mnuChronoStart.Click += new EventHandler(mnuChronoStart_Click);
			mnuChronoStart.Image = Properties.Drawings.chronostart;
			mnuChronoStop.Click += new EventHandler(mnuChronoStop_Click);
			mnuChronoStop.Image = Properties.Drawings.chronostop;
			mnuChronoCountdown.Click += new EventHandler(mnuChronoCountdown_Click);
			mnuChronoCountdown.Checked = false;
			mnuChronoCountdown.Enabled = false;
			mnuChronoHide.Click += new EventHandler(mnuChronoHide_Click);
			mnuChronoHide.Image = Properties.Drawings.hide;
			mnuChronoDelete.Click += new EventHandler(mnuChronoDelete_Click);
			mnuChronoDelete.Image = Properties.Drawings.delete;
			popMenuChrono.Items.AddRange(new ToolStripItem[] { mnuChronoConfigure, new ToolStripSeparator(), mnuChronoStart, mnuChronoStop, mnuChronoCountdown, new ToolStripSeparator(), mnuChronoHide, mnuChronoDelete, });

			// 5. Magnifier
			mnuMagnifier150.Click += new EventHandler(mnuMagnifier150_Click);
			mnuMagnifier175.Click += new EventHandler(mnuMagnifier175_Click);
			mnuMagnifier175.Checked = true;
			mnuMagnifier200.Click += new EventHandler(mnuMagnifier200_Click);
			mnuMagnifier225.Click += new EventHandler(mnuMagnifier225_Click);
			mnuMagnifier250.Click += new EventHandler(mnuMagnifier250_Click);
			mnuMagnifierDirect.Click += new EventHandler(mnuMagnifierDirect_Click);
			mnuMagnifierDirect.Image = Properties.Resources.arrow_out;
			mnuMagnifierQuit.Click += new EventHandler(mnuMagnifierQuit_Click);
			mnuMagnifierQuit.Image = Properties.Resources.hide;
			popMenuMagnifier.Items.AddRange(new ToolStripItem[] { mnuMagnifier150, mnuMagnifier175, mnuMagnifier200, mnuMagnifier225, mnuMagnifier250, new ToolStripSeparator(), mnuMagnifierDirect, mnuMagnifierQuit });
			
			// 6. Grids
			mnuGridsConfigure.Click += new EventHandler(mnuGridsConfigure_Click);
			mnuGridsConfigure.Image = Properties.Drawings.configure;
			mnuGridsHide.Click += new EventHandler(mnuGridsHide_Click);
			mnuGridsHide.Image = Properties.Drawings.hide;
			popMenuGrids.Items.AddRange(new ToolStripItem[] { mnuGridsConfigure, mnuGridsHide });
			
			// The right context menu and its content will be choosen upon MouseDown.
			panelCenter.ContextMenuStrip = popMenu;
			
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
			// If we currently are in DrawTime filter, we just close this and return to normal playback.
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

			m_ActiveTool = m_ActiveTool.KeepToolFrameChanged ? m_ActiveTool : m_PointerTool;
			if(m_ActiveTool == m_PointerTool)
			{
				SetCursor(m_PointerTool.GetCursor(-1));
			}
			
			// 3. Dock Keyf panel if nothing to see.
			if (m_FrameServer.Metadata.Count < 1)
			{
				DockKeyframePanel(true);
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
			double fSeconds;

			if (m_FrameServer.VideoFile.Loaded)
				fSeconds = (double)iTimeStamp / m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds;
			else
				fSeconds = 0;

			// m_fSlowFactor is different from 1.0f only when user specify that the capture fps
			// was different than the playing fps. We readjust time.
			double fMilliseconds = (fSeconds * 1000) / m_fHighSpeedFactor;
			
			// If there are more than 100 frames per seconds, we display milliseconds.
			// This can happen when the user manually tune the input fps.
			bool bShowThousandth = (m_fHighSpeedFactor *  m_FrameServer.VideoFile.Infos.fFps >= 100);
			
			string outputTimeCode;
			switch (tcf)
			{
				case TimeCodeFormat.ClassicTime:
					outputTimeCode = TimeHelper.MillisecondsToTimecode(fMilliseconds, bShowThousandth, true);
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
				case TimeCodeFormat.Milliseconds:
					outputTimeCode = String.Format("{0}", (int)Math.Round(fMilliseconds));
					break;
				case TimeCodeFormat.TenThousandthOfHours:
					// 1 Ten Thousandth of Hour = 360 ms.
					double fTth = fMilliseconds / 360.0;
					outputTimeCode = String.Format("{0}:{1:00}", (int)fTth, Math.Floor((fTth - (int)fTth)*100));
					break;
				case TimeCodeFormat.HundredthOfMinutes:
					// 1 Hundredth of minute = 600 ms.
					double fCtm = fMilliseconds / 600.0;
					outputTimeCode = String.Format("{0}:{1:00}", (int)fCtm, Math.Floor((fCtm - (int)fCtm) * 100));
					break;
				case TimeCodeFormat.TimeAndFrames:
					String timeString = TimeHelper.MillisecondsToTimecode(fMilliseconds, bShowThousandth, true);
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
					outputTimeCode = TimeHelper.MillisecondsToTimecode(fMilliseconds, bShowThousandth, true);
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
		private void DoDrawingUndrawn()
		{
			//--------------------------------------------------------
			// this function is called after we undo a drawing action.
			// Called from CommandAddDrawing.Unexecute().
			//--------------------------------------------------------
			m_ActiveTool = m_ActiveTool.KeepToolFrameChanged ? m_ActiveTool : m_PointerTool;
			if(m_ActiveTool == m_PointerTool)
			{
				SetCursor(m_PointerTool.GetCursor(0));
			}
		}
		private void UpdateFramesMarkers()
		{
			// Updates the markers coordinates and redraw the trkFrame.
			trkFrame.UpdateMarkers(m_FrameServer.Metadata);
		}
		private void ShowBorder(bool _bShow)
		{
			m_bShowImageBorder = _bShow;
			DoInvalidate();
		}
		private void DrawImageBorder(Graphics _canvas)
		{
			// Draw the border around the screen to mark it as selected.
			// Called back from main drawing routine.
			_canvas.DrawRectangle(m_PenImageBorder, 0, 0, pbSurfaceScreen.Width - m_PenImageBorder.Width, pbSurfaceScreen.Height - m_PenImageBorder.Width);
		}
		private void DisablePlayAndDraw()
		{
			StopPlaying();
			m_ActiveTool = m_PointerTool;
			SetCursor(m_PointerTool.GetCursor(0));
			DisableMagnifier();
			UnzoomDirectZoom();
			m_FrameServer.Metadata.StopAllTracking();
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				
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
					long oldPos = m_iCurrentPosition;
					m_iFramesToDecode = -1;
					ShowNextFrame(-1, true);
					
					// If it didn't work, try going back two frames to unstuck the situation.
					// Todo: check if we're going to endup outside the working zone ?
					if(m_iCurrentPosition == oldPos)
					{
						log.Debug("Seeking to previous frame did not work. Moving backward 2 frames.");
						m_iFramesToDecode = -2;
						ShowNextFrame(-1, true);
					}
						
					// Reset to normal.
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				m_iFramesToDecode = 1;

				// If we are outside the primary zone or going to get out, seek to start.
				// We also only do the seek if we are after the m_iStartingPosition,
				// Sometimes, the second frame will have a time stamp inferior to the first,
				// which sort of breaks our sentinels.
				if (((m_iCurrentPosition < m_iSelStart) || (m_iCurrentPosition >= m_iSelEnd)) &&
				    (m_iCurrentPosition >= m_iStartingPosition))
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();

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
					m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
					buttonPlay.Image = Resources.liqplay17;
					m_bIsCurrentlyPlaying = false;
					ActivateKeyframe(m_iCurrentPosition);
					ToastPause();
				}
				else
				{
					// Go into Play mode
					buttonPlay.Image = Resources.liqpause6;
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
					if(m_bDrawtimeFiltered)
					{
						IncreaseDirectZoom();
					}
					else
					{
						buttonGotoNext_Click(null, EventArgs.Empty);
					}
				}
				else
				{
					if(m_bDrawtimeFiltered)
					{
						DecreaseDirectZoom();
					}
					else if (((ModifierKeys & Keys.Shift) == Keys.Shift) && m_iCurrentPosition <= m_iSelStart)
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();

				// Update selection timestamps and labels.
				UpdateSelectionDataFromControl();
				UpdateSelectionLabels();

				// Update the frame tracker internal timestamps (including position if needed).
				trkFrame.Remap(m_iSelStart, m_iSelEnd);
				
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

				AfterSelectionChanged();
			}
		}
		private void trkSelection_TargetAcquired(object sender, EventArgs e)
		{
			// User clicked inside selection: Jump to position.
			if (m_FrameServer.VideoFile.Loaded)
			{
				OnPoke();
				StopPlaying();
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				m_iFramesToDecode = 1;

				//ShowNextFrame(trkSelection.SelTarget, true);
				//m_iCurrentPosition = trkSelection.SelTarget + trkSelection.Minimum;
				ShowNextFrame(trkSelection.SelPos, true);
				m_iCurrentPosition = trkSelection.SelPos + trkSelection.Minimum;
				
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
				
				AfterSelectionChanged();
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
				
				AfterSelectionChanged();
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
				
				AfterSelectionChanged();
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
		private void AfterSelectionChanged()
		{
			// Update everything as if we moved the handlers manually.
			m_FrameServer.Metadata.SelectionStart = m_iSelStart;
			
			UpdateFramesMarkers();
			
			OnPoke();
			m_PlayerScreenUIHandler.PlayerScreenUI_SelectionChanged(false);

			// Update current image and keyframe  status.
			UpdateFramePrimarySelection();
			EnableDisableKeyframes();
			ActivateKeyframe(m_iCurrentPosition);	
		}
		#endregion
		
		#region Frame Tracker
		private void trkFrame_PositionChanging(object sender, long _iPosition)
		{
			// Called on user mouse move on frame tracker, if on analysis mode.
			if (m_FrameServer.VideoFile.Loaded)
			{
				// Update image but do not touch cursor, as the user is manipulating it.
				// If the position needs to be adjusted to an actual timestamp, it'll be done later.
				UpdateFrameCurrentPosition(false);
				UpdateCurrentPositionLabel();
				
				ActivateKeyframe(m_iCurrentPosition);

				// Update the selection hairline (?)
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();

				// Update image and cursor.
				UpdateFrameCurrentPosition(true);
				UpdateCurrentPositionLabel();
				ActivateKeyframe(m_iCurrentPosition);

				// Mise à jour de l'indicateur sur le frame
				trkSelection.SelPos = trkFrame.Position;
			}
		}
		private void UpdateFrameCurrentPosition(bool _bUpdateNavCursor)
		{
			// Displays the image corresponding to the current position within working zone.
			// Trigerred by user (or first load). i.e: cursor moved, show frame.
			if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 0)
			{
				// We may have to decode a few images so show hourglass.
				this.Cursor = Cursors.WaitCursor;
			}

			m_iCurrentPosition = trkFrame.Position;
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iCurrentPosition, true);

			if (_bUpdateNavCursor)
			{
				// This may readjust the cursor in case the mouse wasn't on a valid timestamp value.
				UpdateNavigationCursor();
			}
			if (m_bShowInfos) { UpdateDebugInfos(); }

			if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 0)
			{
				this.Cursor = Cursors.Default;
			}
		}
		private void UpdateCurrentPositionLabel()
		{
			// Position is relative to working zone.
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
		#endregion

		#region Speed Slider
		private void sldrSpeed_ValueChanged(object sender, EventArgs e)
		{
			m_fSlowmotionPercentage = sldrSpeed.Value > 0 ? sldrSpeed.Value : 1;
			
			if (m_FrameServer.VideoFile.Loaded)
			{
				// Reset timer with new value.
				if (m_bIsCurrentlyPlaying)
				{
					StopMultimediaTimer();
					StartMultimediaTimer(GetPlaybackFrameInterval());
				}

				// Impacts synchro.
				m_PlayerScreenUIHandler.PlayerScreenUI_SpeedChanged(true);
			}

			UpdateSpeedLabel();
		}
		private void sldrSpeed_KeyDown(object sender, KeyEventArgs e)
		{
			// Increase/Decrease speed on UP/DOWN Arrows.
			if (m_FrameServer.VideoFile.Loaded)
			{
				int jumpFactor = 25;
				if( (ModifierKeys & Keys.Control) == Keys.Control)
				{
					jumpFactor = 1;
				}
				else if((ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					jumpFactor = 10;
				}
			
				if (e.KeyCode == Keys.Down)
				{
					sldrSpeed.ForceValue(jumpFactor * ((sldrSpeed.Value-1) / jumpFactor));
					e.Handled = true;
				}
				else if (e.KeyCode == Keys.Up)
				{
					sldrSpeed.ForceValue(jumpFactor * ((sldrSpeed.Value / jumpFactor) + 1));
					e.Handled = true;
				}
			}
		}
		private void lblSpeedTuner_DoubleClick(object sender, EventArgs e)
		{
			// Double click on the speed label : Back to 100%
			sldrSpeed.ForceValue(sldrSpeed.StickyValue);
		}
		private void UpdateSpeedLabel()
		{
			if(m_fHighSpeedFactor != 1.0)
			{
				double fRealtimePercentage = (double)m_fSlowmotionPercentage / m_fHighSpeedFactor;
				lblSpeedTuner.Text = String.Format("{0} {1:0.00}%", ScreenManagerLang.lblSpeedTuner_Text, fRealtimePercentage);
			}
			else
			{
				if(m_fSlowmotionPercentage % 1 == 0)
				{
					lblSpeedTuner.Text = ScreenManagerLang.lblSpeedTuner_Text + " " + m_fSlowmotionPercentage + "%";
				}
				else
				{
					// Special case when the speed percentage is coming from the other screen and is not an integer.
					lblSpeedTuner.Text = String.Format("{0} {1:0.00}%", ScreenManagerLang.lblSpeedTuner_Text, m_fSlowmotionPercentage);
				}
			}			
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
					m_ePlayingMode = PlayingMode.Loop;
				}
				else if (m_ePlayingMode == PlayingMode.Loop)
				{
					m_ePlayingMode = PlayingMode.Once;
				}
				
				UpdatePlayingModeButton();
			}
		}
		private void UpdatePlayingModeButton()
		{
			if (m_ePlayingMode == PlayingMode.Once)
			{
				buttonPlayingMode.Image = Resources.playmodeonce;
				toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Once);		
			}
			else if(m_ePlayingMode == PlayingMode.Loop)
			{
				buttonPlayingMode.Image = Resources.playmodeloop;
				toolTips.SetToolTip(buttonPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Loop);	
			}
		}
		#endregion

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
				
				// Center
				pbSurfaceScreen.Left = (panelCenter.Width / 2) - (pbSurfaceScreen.Width / 2);
				pbSurfaceScreen.Top = (panelCenter.Height / 2) - (pbSurfaceScreen.Height / 2);
				ReplaceResizers();
				
				// Redefine grids.
				Size imageSize = new Size(m_FrameServer.VideoFile.Infos.iDecodingWidth, m_FrameServer.VideoFile.Infos.iDecodingHeight);
				
				m_FrameServer.Metadata.SetLocations(imageSize, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Location);
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
			m_FrameServer.Metadata.ResizeFinished();
			DoInvalidate();
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
				DoInvalidate();
			}
		}
		private void Resizers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ToggleStretchMode();
		}
		private void Resizers_MouseUp(object sender, MouseEventArgs e)
		{
			m_FrameServer.Metadata.ResizeFinished();
			DoInvalidate();
		}
		#endregion
		
		#region Timers & Playloop
		private void StartMultimediaTimer(double _interval)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

			int myData = 0;	// dummy data
			m_IdMultimediaTimer = timeSetEvent( (int)_interval,                              // Délai en ms.
			                                   (int)_interval,                              // Resolution en ms.
			                                   m_TimerEventHandler,            // event handler du tick.
			                                   ref myData,                             // ?
			                                   TIME_PERIODIC | TIME_KILL_SYNCHRONOUS); // Type d'event (1=periodic)
			
			log.Debug("PlayerScreen multimedia timer started.");
			
			// Deactivate all keyframes during playing.
			ActivateKeyframe(-1);
		}
		private void StopMultimediaTimer()
		{
			if (m_IdMultimediaTimer != 0)
			{
				timeKillEvent(m_IdMultimediaTimer);
				Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
				log.Debug("PlayerScreen multimedia timer stopped.");
			}
		}
		private void MultimediaTimer_Tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
		{
			// We comes here more often than we should, by bunches.
			if (m_FrameServer.VideoFile.Loaded)
			{
				BeginInvoke(m_PlayLoop);
			}
		}
		private void PlayLoop_Invoked()
		{
			//--------------------------------------------------------------
			// Function called by main timer event handler, on each tick.
			// Asynchronously if needed.
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

			/*m_Stopwatch.Stop();
			log.Debug(String.Format("Back in Playloop. Elapsed: {0} ms.", m_Stopwatch.ElapsedMilliseconds));
			
			m_Stopwatch.Reset();
			m_Stopwatch.Start();*/
			
			bool bStopAtEnd = false;
			//----------------------------------------------------------------------------
			// En prévision de l'appel à ShowNextFrame, on vérifie qu'on ne va pas sortir.
			// Si c'est le cas, on stoppe la lecture pour rewind.
			// m_iFramesToDecode est toujours strictement positif. (Car on est en Play)
			//----------------------------------------------------------------------------
			long iTargetPosition = m_iCurrentPosition + (m_iFramesToDecode * m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame);

			if ((iTargetPosition > m_iSelEnd) || (iTargetPosition >= (m_iStartingPosition + m_iTotalDuration)))
			{
				log.Debug("End of video reached");
				
				// We have reached the end of the video.
				if (m_ePlayingMode == PlayingMode.Once)
				{
					StopPlaying();
					bStopAtEnd = true;
				}
				else
				{
					if(m_bSynched)
					{
						// Go into Pause mode.
						StopPlaying();
					}
					else
					{
						// If auto rewind, only stops timer,
						// playback will restart automatically if everything is ok.
						StopMultimediaTimer();
					}
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
					// Rewind to begining.
					if (ShowNextFrame(m_iSelStart, true) == 0)
					{
						if(m_bSynched)
						{
							//log.Debug("Stopping on frame [0] after auto rewind.");
							// Stop on frame [0]. Will be restarted by the dynamic sync when needed.
						}
						else
						{
							// Auto restart timer if everything went fine.
							StartMultimediaTimer(GetPlaybackFrameInterval());
						}
					}
					else
					{
						log.Debug("Error while decoding first frame after auto rewind.");
						StopPlaying();
					}
					m_bSeekToStart = false;
				}
				else if (bStopAtEnd)
				{
					// Nothing to do. Playback was stopped on last frame.
				}
				else
				{
					// Not rewinding and not stopped on last frame.
					// display next frame(s) in queue.
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
				foreach(AbstractDrawing ad in m_FrameServer.Metadata.ExtraDrawings)
				{
					Track t = ad as Track;
					if( t != null && t.Status == Track.TrackStatus.Edit)
					{
						bTracking = true;
						break;
					}
				}

				if (!bTracking)
				{
					m_iFramesToDecode++;
					m_iDroppedFrames++;
					//log.Debug(String.Format("Dropping. Total :{0} frames.", m_iDroppedFrames));
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
					if (sldrSpeed.Value >= sldrSpeed.Minimum + sldrSpeed.LargeChange)
					{
						sldrSpeed.ForceValue(sldrSpeed.Value - sldrSpeed.LargeChange);
					}
				}
			}
			
			//m_Stopwatch.Stop();
			//log.Debug(String.Format("Exiting Playloop. Took: {0} ms.", m_Stopwatch.ElapsedMilliseconds));
		}
		private void IdleDetector(object sender, EventArgs e)
		{
			//log.Debug("back to idle");
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
			
			//m_Stopwatch.Reset();
			//m_Stopwatch.Start();
				
			ReadResult res = m_FrameServer.VideoFile.ReadFrame((long)_iSeekTarget, m_iFramesToDecode);
			
			if (res == ReadResult.Success)
			{
				m_iDecodedFrames++;
				m_iCurrentPosition = m_FrameServer.VideoFile.Selection.iCurrentTimeStamp;
				
				// Compute or stop tracking
				if (m_FrameServer.Metadata.HasTrack())
				{
					if (_iSeekTarget >= 0 || m_iFramesToDecode > 1)
					{
						// Tracking works frame to frame.
						// If playhead jumped several frames at once or moved back, we force-stop tracking.
						m_FrameServer.Metadata.StopAllTracking();
					}
					else
					{
						foreach(AbstractDrawing ad in m_FrameServer.Metadata.ExtraDrawings)
						{
							Track t = ad as Track;
							if (t != null && t.Status == Track.TrackStatus.Edit)
							{
								t.TrackCurrentPosition(m_iCurrentPosition, m_FrameServer.VideoFile.CurrentImage);
							}
						}
					}
					UpdateFramesMarkers();
				}
				
				// Display image
				if(_bAllowUIUpdate) DoInvalidate();
				
				// Report image for synchro and merge.
				ReportForSyncMerge();
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
							{
								trkSelection.SelPos = m_iCurrentPosition;
								DoInvalidate();
							}
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
			
			//m_Stopwatch.Stop();
			//log.Debug(String.Format("ShowNextFrame: {0} ms.", m_Stopwatch.ElapsedMilliseconds));
			
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
						buttonPlay.Image = Resources.liqplay17;
						DoInvalidate();
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
		public void DisplayConfigureSpeedBox(bool _center)
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
					ScreenManagerKernel.LocateForm(fcs);
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
				UpdateSpeedLabel();
				m_PlayerScreenUIHandler.PlayerScreenUI_SpeedChanged(true);
				m_FrameServer.Metadata.CalibrationHelper.FramesPerSeconds = m_FrameServer.VideoFile.Infos.fFps * m_fHighSpeedFactor;
				DoInvalidate();
			}
		}
		private double GetPlaybackFrameInterval()
		{
			// Returns the playback interval between frames in Milliseconds, taking slow motion slider into account.
			// m_iSlowmotionPercentage must be > 0.
			if (m_FrameServer.VideoFile.Loaded && m_FrameServer.VideoFile.Infos.fFrameInterval > 0 && m_fSlowmotionPercentage > 0)
			{
				return (m_FrameServer.VideoFile.Infos.fFrameInterval / ((double)m_fSlowmotionPercentage / 100.0));
			}
			else
			{
				return 40;
			}
		}
		
		private void DeselectionTimer_OnTick(object sender, EventArgs e) 
		{
			// Deselect the currently selected drawing.
			// This is used for drawings that must show extra stuff for being transformed, but we 
			// don't want to show the extra stuff all the time for clarity.
			
			m_FrameServer.Metadata.SelectedDrawingFrame = -1;
			m_FrameServer.Metadata.SelectedDrawing = -1;
			log.Debug("Deselection timer fired.");
			m_DeselectionTimer.Stop();
			DoInvalidate();
		}

		#endregion
		
		#region Culture
		private void ReloadMenusCulture()
		{
			// Reload the text for each menu.
			// this is done at construction time and at RefreshUICulture time.
			
			// 1. Default context menu.
			mnuDirectTrack.Text = ScreenManagerLang.mnuTrackTrajectory;
			mnuPlayPause.Text = ScreenManagerLang.mnuPlayPause;
			mnuSetCaptureSpeed.Text = ScreenManagerLang.mnuSetCaptureSpeed;
			mnuSavePic.Text = ScreenManagerLang.Generic_SaveImage;
			mnuSendPic.Text = ScreenManagerLang.mnuSendPic;
			mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
			
			// 2. Drawings context menu.
			mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;
			mnuConfigureFading.Text = ScreenManagerLang.mnuConfigureFading;
			mnuConfigureOpacity.Text = ScreenManagerLang.Generic_Opacity;
			mnuTrackTrajectory.Text = ScreenManagerLang.mnuTrackTrajectory;
			mnuGotoKeyframe.Text = ScreenManagerLang.mnuGotoKeyframe;
			mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
			
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
			toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.Generic_SaveImage);
			toolTips.SetToolTip(btnRafale, ScreenManagerLang.ToolTip_Rafale);
			toolTips.SetToolTip(btnDiaporama, ScreenManagerLang.ToolTip_SaveDiaporama);
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
			foreach(ToolStripItem tsi in stripDrawingTools.Items)
			{
				if(tsi is ToolStripButton)
				{
					AbstractDrawingTool tool = tsi.Tag as AbstractDrawingTool;
					if(tool != null)
					{
						tsi.ToolTipText = tool.DisplayName;
					}
				}
			}
			
			m_btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
			m_btnShowComments.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
			m_btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
		}
		#endregion

		#region SurfaceScreen Events
		private void SurfaceScreen_MouseDown(object sender, MouseEventArgs e)
		{
			if(m_FrameServer.VideoFile != null)
			{
				if (m_FrameServer.VideoFile.Loaded)
				{
					m_DeselectionTimer.Stop();
					
					if (e.Button == MouseButtons.Left)
					{
						// Magnifier can be moved even when the video is playing.
						// TODO - Grids should be able to do the same.
						// But the z order in the PointerTool MouseDown would have to be taken care of.
						
						bool bWasPlaying = false;
						
						if (m_bIsCurrentlyPlaying)
						{
							if ( (m_ActiveTool == m_PointerTool)      &&
							    (m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.NotVisible) &&
							    (m_FrameServer.Metadata.Magnifier.IsOnObject(e)))
							{
								m_FrameServer.Metadata.Magnifier.OnMouseDown(e);
							}
							else
							{
								// MouseDown while playing: Halt the video.
								StopPlaying();
								m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
								ActivateKeyframe(m_iCurrentPosition);
								bWasPlaying = true;
								ToastPause();
							}
						}
						
						
						if (!m_bIsCurrentlyPlaying && !m_bDrawtimeFiltered)
						{
							//-------------------------------------
							// Action begins:
							// Move or set magnifier
							// Move or set Drawing
							// Move or set Chrono / Track
							// Move Grids
							//-------------------------------------
							
							m_DescaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
							
							// 1. Pass all DrawingText to normal mode
							m_FrameServer.Metadata.AllDrawingTextToNormalMode();
							
							if (m_ActiveTool == m_PointerTool)
							{
								// 1. Manipulating an object or Magnifier
								bool bMovingMagnifier = false;
								bool bDrawingHit = false;
								
								// Show the grabbing hand cursor.
								SetCursor(m_PointerTool.GetCursor(1));
								
								if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Indirect)
								{
									bMovingMagnifier = m_FrameServer.Metadata.Magnifier.OnMouseDown(e);
								}
								
								if (!bMovingMagnifier)
								{
									// Magnifier wasn't hit or is not in use,
									// try drawings (including chronos, grids, etc.)
									bDrawingHit = m_PointerTool.OnMouseDown(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition, m_PrefManager.DefaultFading.Enabled);
								}
								
								if (!bDrawingHit && !bWasPlaying)
								{
									// MouseDown in arbitrary location and we were halted already.
									
									// We cannot restart the video here because this MouseDown may actually be the start
									// of a double click. (expand screen)
								}
							}
							else if (m_ActiveTool == ToolManager.Chrono)
							{
								// Add a Chrono.
								DrawingChrono chrono = (DrawingChrono)m_ActiveTool.GetNewDrawing(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame);
								m_FrameServer.Metadata.AddChrono(chrono);
								m_ActiveTool = m_PointerTool;
							}
							else
							{
								//-----------------------
								// Creating a new Drawing
								//-----------------------
								m_FrameServer.Metadata.SelectedExtraDrawing = -1;
								
								// Add a KeyFrame here if it doesn't exist.
								AddKeyframe();
								
								if (m_ActiveTool != ToolManager.Label)
								{
									// Add an instance of a drawing from the active tool to the current keyframe.
									// The drawing is initialized with the current mouse coordinates.
									AbstractDrawing ad = m_ActiveTool.GetNewDrawing(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame);
									
									m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(ad);
									m_FrameServer.Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
									m_FrameServer.Metadata.SelectedDrawing = 0;
									
									if(ad is DrawingLine2D)
									{
										((DrawingLine2D)ad).ParentMetadata = m_FrameServer.Metadata;
										((DrawingLine2D)ad).ShowMeasure = DrawingToolLine2D.ShowMeasure;
									}
									else if(ad is DrawingCross2D)
									{
										((DrawingCross2D)ad).ParentMetadata = m_FrameServer.Metadata;
										((DrawingCross2D)ad).ShowCoordinates = DrawingToolCross2D.ShowCoordinates;
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
											int hitRes = ad.HitTest(m_DescaledMouse, m_iCurrentPosition);
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
										m_FrameServer.Metadata[m_iActiveKeyFrameIndex].AddDrawing(m_ActiveTool.GetNewDrawing(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame));
										m_FrameServer.Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
										m_FrameServer.Metadata.SelectedDrawing = 0;
										
										DrawingText dt = (DrawingText)m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings[0];
										
										dt.ContainerScreen = pbSurfaceScreen;
										dt.RelocateEditbox(m_FrameServer.CoordinateSystem.Stretch * m_FrameServer.CoordinateSystem.Zoom, m_FrameServer.CoordinateSystem.Location);
										dt.EditMode = true;
										panelCenter.Controls.Add(dt.EditBox);
										dt.EditBox.BringToFront();
										dt.EditBox.Focus();
									}
								}
							}
						}
					}
					else if (e.Button == MouseButtons.Right)
					{
						// Show the right Pop Menu depending on context.
						// (Drawing, Trajectory, Chronometer, Grids, Magnifier, Nothing)
						
						m_DescaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
						
						if (!m_bIsCurrentlyPlaying)
						{
							m_FrameServer.Metadata.UnselectAll();
							AbstractDrawing hitDrawing = null;
								
							if(m_bDrawtimeFiltered)
							{
								mnuDirectTrack.Visible = false;
								mnuSendPic.Visible = false;
								panelCenter.ContextMenuStrip = popMenu;
							}
							else if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
							{
								// Rebuild the context menu according to the capabilities of the drawing we are on.
								
								AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing];
								if(ad != null)
								{
									popMenuDrawings.Items.Clear();
									
									// Generic context menu from drawing capabilities.
									if((ad.Caps & AbstractDrawing.Capabilities.ConfigureColor) == AbstractDrawing.Capabilities.ConfigureColor)
									{
									   	mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_Color;
									   	popMenuDrawings.Items.Add(mnuConfigureDrawing);
									}
									   
									if((ad.Caps & AbstractDrawing.Capabilities.ConfigureColorSize) == AbstractDrawing.Capabilities.ConfigureColorSize)
									{
										mnuConfigureDrawing.Text = ScreenManagerLang.mnuConfigureDrawing_ColorSize;
									   	popMenuDrawings.Items.Add(mnuConfigureDrawing);
									}
										
									if(m_PrefManager.DefaultFading.Enabled && ((ad.Caps & AbstractDrawing.Capabilities.Fading) == AbstractDrawing.Capabilities.Fading))
									{
										popMenuDrawings.Items.Add(mnuConfigureFading);
									}
									
									if((ad.Caps & AbstractDrawing.Capabilities.Opacity) == AbstractDrawing.Capabilities.Opacity)
									{
										popMenuDrawings.Items.Add(mnuConfigureOpacity);
									}
									
									popMenuDrawings.Items.Add(mnuSepDrawing);

									// Specific menus. Hosted by the drawing itself.
									bool hasExtraMenu = (ad.ContextMenu != null && ad.ContextMenu.Count > 0);
									if(hasExtraMenu)
									{
										foreach(ToolStripMenuItem tsmi in ad.ContextMenu)
										{
											tsmi.Tag = (DelegateScreenInvalidate)DoInvalidate;	// Inject dependency on this screen's invalidate method.
											popMenuDrawings.Items.Add(tsmi);
										}
									}
									
									bool gotoVisible = (m_PrefManager.DefaultFading.Enabled && (ad.infosFading.ReferenceTimestamp != m_iCurrentPosition));
									if(gotoVisible)
										popMenuDrawings.Items.Add(mnuGotoKeyframe);
									
									if(hasExtraMenu || gotoVisible)
										popMenuDrawings.Items.Add(mnuSepDrawing2);
										
									// Generic delete
									popMenuDrawings.Items.Add(mnuDeleteDrawing);
									
									// Set this menu as the context menu.
									panelCenter.ContextMenuStrip = popMenuDrawings;
								}
							} 
							else if( (hitDrawing = m_FrameServer.Metadata.IsOnExtraDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
							{ 
								// Unlike attached drawings, each extra drawing type has its own context menu for now.
								
								if(hitDrawing is DrawingChrono)
								{
									// Toggle to countdown is active only if we have a stop time.
									mnuChronoCountdown.Enabled = ((DrawingChrono)hitDrawing).HasTimeStop;
									mnuChronoCountdown.Checked = ((DrawingChrono)hitDrawing).CountDown;
									panelCenter.ContextMenuStrip = popMenuChrono;
								}
								else if(hitDrawing is Plane3D)
								{
									panelCenter.ContextMenuStrip = popMenuGrids;
								}
								else if(hitDrawing is Track)
								{
									if (((Track)hitDrawing).Status == Track.TrackStatus.Edit)
									{
										mnuStopTracking.Visible = true;
										mnuRestartTracking.Visible = false;
									}
									else
									{
										mnuStopTracking.Visible = false;
										mnuRestartTracking.Visible = true;
									}	
									
									panelCenter.ContextMenuStrip = popMenuTrack;
								}
								
							}
							else if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Indirect && m_FrameServer.Metadata.Magnifier.IsOnObject(e))
							{
								panelCenter.ContextMenuStrip = popMenuMagnifier;
							}
							else if(m_ActiveTool != m_PointerTool)
							{
								// Launch FormToolPreset.
								FormToolPresets ftp = new FormToolPresets(m_ActiveTool);
								ScreenManagerKernel.LocateForm(ftp);
								ftp.ShowDialog();
								ftp.Dispose();
								UpdateCursor();
							}
							else
							{
								// No drawing touched and no tool selected, but not currently playing. Default menu.
								mnuDirectTrack.Visible = true;
								mnuSendPic.Visible = m_bSynched;
								panelCenter.ContextMenuStrip = popMenu;
							}
						}
						else
						{
							// Currently playing.
							mnuDirectTrack.Visible = false;
							mnuSendPic.Visible = false;
							panelCenter.ContextMenuStrip = popMenu;
						}
					}
					
					DoInvalidate();
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
				if (e.Button == MouseButtons.None && m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
				{
					m_FrameServer.Metadata.Magnifier.MouseX = e.X;
					m_FrameServer.Metadata.Magnifier.MouseY = e.Y;
					
					if (!m_bIsCurrentlyPlaying)
					{
						DoInvalidate();
					}
				}
				else if (e.Button == MouseButtons.Left)
				{
					if (m_ActiveTool != m_PointerTool)
					{
						// Tools that are not IInitializable should reset to Pointer tool after creation.
						if (m_iActiveKeyFrameIndex >= 0 && !m_bIsCurrentlyPlaying)
						{
							// Currently setting the second point of a Drawing.
							IInitializable initializableDrawing = m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings[0] as IInitializable;
							if(initializableDrawing != null)
							{
								initializableDrawing.ContinueSetup(m_FrameServer.CoordinateSystem.Untransform(new Point(e.X, e.Y)));
							}
						}
					}
					else
					{
						bool bMovingMagnifier = false;
						if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Indirect)
						{
							bMovingMagnifier = m_FrameServer.Metadata.Magnifier.OnMouseMove(e);
						}
						
						if (!bMovingMagnifier && m_ActiveTool == m_PointerTool)
						{
							if (!m_bIsCurrentlyPlaying)
							{
								m_DescaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
								
								// Magnifier is not being moved or is invisible, try drawings through pointer tool.
								// (including chronos, tracks and grids)
								bool bMovingObject = m_PointerTool.OnMouseMove(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, m_DescaledMouse, m_FrameServer.CoordinateSystem.Location, ModifierKeys);
								
								if (!bMovingObject)
								{
									// User is not moving anything: move the whole image.
									// This may not have any effect if we try to move outside the original size and not in "free move" mode.
									
									// Get mouse deltas (descaled=in image coords).
									double fDeltaX = (double)m_PointerTool.MouseDelta.X;
									double fDeltaY = (double)m_PointerTool.MouseDelta.Y;
									
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
						DoInvalidate();
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
				if (m_ActiveTool == m_PointerTool)
				{
					OnPoke();
					
					// Update tracks with current image and pos.
					m_FrameServer.Metadata.UpdateTrackPoint(m_FrameServer.VideoFile.CurrentImage);
					
					// Report for synchro and merge to update image in the other screen.
					ReportForSyncMerge();
				}
				
				m_FrameServer.Metadata.Magnifier.OnMouseUp(e);
				
				// Memorize the action we just finished to enable undo.
				if(m_ActiveTool == ToolManager.Chrono)
				{
					IUndoableCommand cac = new CommandAddChrono(DoInvalidate, DoDrawingUndrawn, m_FrameServer.Metadata);
					CommandManager cm = CommandManager.Instance();
					cm.LaunchUndoableCommand(cac);
				}
				else if (m_ActiveTool != m_PointerTool && m_iActiveKeyFrameIndex >= 0)
				{
					// Record the adding unless we are editing a text box.
					if (!m_bTextEdit)
					{
						IUndoableCommand cad = new CommandAddDrawing(DoInvalidate, DoDrawingUndrawn, m_FrameServer.Metadata, m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Position);
						CommandManager cm = CommandManager.Instance();
						cm.LaunchUndoableCommand(cad);
						
						// Deselect the drawing we just added.
						m_FrameServer.Metadata.SelectedDrawingFrame = -1;
						m_FrameServer.Metadata.SelectedDrawing = -1;
					}
					else
					{
						m_bTextEdit = false;
					}
				}
				
				// The fact that we stay on this tool or fall back to pointer tool, depends on the tool.
				m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;
				
				if (m_ActiveTool == m_PointerTool)
				{
					SetCursor(m_PointerTool.GetCursor(0));
					m_PointerTool.OnMouseUp();
					
					// If we were resizing an SVG drawing, trigger a render.
					// TODO: this is currently triggered on every mouse up, not only on resize !
					int selectedFrame = m_FrameServer.Metadata.SelectedDrawingFrame;
					int selectedDrawing = m_FrameServer.Metadata.SelectedDrawing;
					if(selectedFrame != -1 && selectedDrawing  != -1)
					{
						DrawingSVG d = m_FrameServer.Metadata.Keyframes[selectedFrame].Drawings[selectedDrawing] as DrawingSVG;
						if(d != null)
						{
							d.ResizeFinished();
						}
					}
				}
				
				if (m_FrameServer.Metadata.SelectedDrawingFrame != -1 && m_FrameServer.Metadata.SelectedDrawing != -1)
				{
					m_DeselectionTimer.Start();					
				}
				
				DoInvalidate();
			}
		}
		private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if(m_FrameServer.VideoFile != null &&
			   m_FrameServer.VideoFile.Loaded &&
			   e.Button == MouseButtons.Left &&
			   m_ActiveTool == m_PointerTool)
			{
				OnPoke();
				
				m_DescaledMouse = m_FrameServer.CoordinateSystem.Untransform(e.Location);
				m_FrameServer.Metadata.AllDrawingTextToNormalMode();
				m_FrameServer.Metadata.UnselectAll();
				
				AbstractDrawing hitDrawing = null;
				
				//------------------------------------------------------------------------------------
				// - If on text, switch to edit mode.
				// - If on other drawing, launch the configuration dialog.
				// - Otherwise -> Maximize/Reduce image.
				//------------------------------------------------------------------------------------
				if(m_bDrawtimeFiltered)
				{
					ToggleStretchMode();	
				}
				else if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
				{
					// Double click on a drawing:
					// turn text tool into edit mode, launch config for others, SVG don't have a config.
					AbstractDrawing ad = m_FrameServer.Metadata.Keyframes[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing];
					if (ad is DrawingText)
					{
						((DrawingText)ad).EditMode = true;
						m_ActiveTool = ToolManager.Label;
						m_bTextEdit = true;
					}
					else if(ad is DrawingSVG || ad is DrawingBitmap)
					{
						mnuConfigureOpacity_Click(null, EventArgs.Empty);
					}
					else
					{
						mnuConfigureDrawing_Click(null, EventArgs.Empty);
					}
				}
				else if((hitDrawing = m_FrameServer.Metadata.IsOnExtraDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
				{
					if(hitDrawing is DrawingChrono)
					{
						mnuChronoConfigure_Click(null, EventArgs.Empty);	
					}
					else if(hitDrawing is Plane3D)
					{
						mnuGridsConfigure_Click(null, EventArgs.Empty);
					}
					else if(hitDrawing is Track)
					{
						mnuConfigureTrajectory_Click(null, EventArgs.Empty);	
					}
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
			if(m_FrameServer.VideoFile != null && m_FrameServer.VideoFile.Loaded && !m_DualSaveInProgress)
			{
				if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
				{
					m_DrawingFilterOutput.Draw(e.Graphics, pbSurfaceScreen.Size, m_DrawingFilterOutput.PrivateData);
				}
				else if(m_FrameServer.VideoFile.CurrentImage != null)
				{
					try
					{
						//m_Stopwatch.Reset();
						//m_Stopwatch.Start();
						
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
						
						if(m_MessageToaster.Enabled)
						{
							m_MessageToaster.Draw(e.Graphics);
						}
						
						//m_Stopwatch.Stop();
            			//log.Debug(String.Format("Paint: {0} ms.", m_Stopwatch.ElapsedMilliseconds));
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
			// - The global performance depends on the size of the *source* image. Not destination.
			//   (rendering 1 pixel from an HD source will still be slow)
			// - Using a matrix transform instead of the buit in interpolation doesn't seem to do much.
			// - InterpolationMode has a sensible effect. but can look ugly for lowest values.
			// - Using unmanaged BitBlt or StretchBlt doesn't seem to do much... (!?)
			// - the scaling and interpolation better be done directly from ffmpeg. (cut on memory usage too)
			// - furthermore ffmpeg has a mode called 'FastBilinear' that seems more promising.
			
			// 1. Image
			g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.InterpolationMode = InterpolationMode.Bilinear;
			g.SmoothingMode = SmoothingMode.None;
			
			// TODO - matrix transform.
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
			
			g.DrawImage(_sourceImage, rDst, m_FrameServer.CoordinateSystem.ZoomWindow, GraphicsUnit.Pixel);
			
			// Testing Key images overlay.
			// Creates a ghost image of the last keyframe superposed with the current image.
			// We can only do it in analysis mode to get the key image bitmap.
			/*if(m_FrameServer.VideoFile.Selection.iAnalysisMode == 1 && m_FrameServer.Metadata.Keyframes.Count > 0)
			{
				// Look for the closest key image before.
				int iImageMerge = -1 ;
				long iBestDistance = long.MaxValue;	
				for(int i=0; i<m_FrameServer.Metadata.Keyframes.Count;i++)
				{
					long iDistance = _iPosition - m_FrameServer.Metadata.Keyframes[i].Position;
					if(iDistance >=0 && iDistance < iBestDistance)
					{
						iBestDistance = iDistance;
						iImageMerge = i;
					}
				}
				
				// Merge images.
				int iFrameIndex = (int)m_FrameServer.VideoFile.GetFrameNumber(m_FrameServer.Metadata.Keyframes[iImageMerge].Position);
				Bitmap mergeImage = m_FrameServer.VideoFile.FrameList[iFrameIndex].BmpImage;
				g.DrawImage(mergeImage, rDst, 0, 0, _sourceImage.Width, _sourceImage.Height, GraphicsUnit.Pixel, m_SyncMergeImgAttr);
			}*/
			
			// .Sync superposition.
			if(m_bSynched && m_bSyncMerge && m_SyncMergeImage != null)
			{
				// The mirroring, if any, will have been done already and applied to the sync image.
				// (because to draw the other image, we take account its own mirroring option,
				// not the option of the original video in this screen.)
				Rectangle rSyncDst = new Rectangle(0, 0, _iNewSize.Width, _iNewSize.Height);
				g.DrawImage(m_SyncMergeImage, rSyncDst, 0, 0, m_SyncMergeImage.Width, m_SyncMergeImage.Height, GraphicsUnit.Pixel, m_SyncMergeImgAttr);
			}
			
			if ((m_bIsCurrentlyPlaying && m_PrefManager.DrawOnPlay) || !m_bIsCurrentlyPlaying)
			{
				FlushDrawingsOnGraphics(g, _iKeyFrameIndex, _iPosition, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.CoordinateSystem.Zoom, m_FrameServer.CoordinateSystem.Location);
				FlushMagnifierOnGraphics(_sourceImage, g);
			}
		}
		private void FlushDrawingsOnGraphics(Graphics _canvas, int _iKeyFrameIndex, long _iPosition, double _fStretchFactor, double _fDirectZoomFactor, Point _DirectZoomTopLeft)
		{
			// Prepare for drawings
			_canvas.SmoothingMode = SmoothingMode.AntiAlias;

			// 1. Extra (non attached to any key image).
			for (int i = 0; i < m_FrameServer.Metadata.ExtraDrawings.Count; i++)
			{
				m_FrameServer.Metadata.ExtraDrawings[i].Draw(_canvas, 
				                                             _fStretchFactor * _fDirectZoomFactor, 
				                                             (i == m_FrameServer.Metadata.SelectedExtraDrawing), 
				                                             _iPosition, 
				                                             _DirectZoomTopLeft);
			}
			
			// 2. Drawings attached to key images.
			if (m_PrefManager.DefaultFading.Enabled)
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
		}
		private void FlushMagnifierOnGraphics(Bitmap _sourceImage, Graphics g)
		{
			// Note: the Graphics object must not be the one extracted from the image itself.
			// If needed, clone the image.
			if (_sourceImage != null && m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.NotVisible)
			{
				m_FrameServer.Metadata.Magnifier.Draw(_sourceImage, g, m_FrameServer.CoordinateSystem.Stretch, m_FrameServer.Metadata.Mirrored);
			}
		}
		private void DoInvalidate()
		{
			// This function should be the single point where we call for rendering.
			// Here we can decide to render directly on the surface or go through the Windows message pump.
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
			DoInvalidate();
		}
		private void PanelCenter_MouseDown(object sender, MouseEventArgs e)
		{
			mnuDirectTrack.Visible = false;
			mnuSendPic.Visible = m_bSynched;
			panelCenter.ContextMenuStrip = popMenu;
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
			DockKeyframePanel(m_bDocked);
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

					box.UpdateTitle(kf.Title);
					box.Tag = iKeyframeIndex;
					box.pbThumbnail.SizeMode = PictureBoxSizeMode.StretchImage;
					
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
			}
			else
			{
				DockKeyframePanel(true);
				m_iActiveKeyFrameIndex = -1;
			}
			
			UpdateFramesMarkers();
			DoInvalidate(); // Because of trajectories with keyframes labels.
		}
		private void SetupDefaultThumbBox(UserControl _box)
		{
			_box.Top = 10;
			_box.Cursor = Cursors.Hand;
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

			DoInvalidate();

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
					DockKeyframePanel(false);
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
				trkSelection.SelPos = trkFrame.Position;

				// Readjust and complete the Keyframe
				kf.ImportImage(m_FrameServer.VideoFile.CurrentImage);
			}

			m_FrameServer.Metadata.Add(kf);

			// Keep the list sorted
			m_FrameServer.Metadata.Sort();
			m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();

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
			OrganizeKeyframes();
			DoInvalidate();
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
			if (m_FrameServer.VideoFile.Loaded)
			{
				// On double click in the thumbs panel : Add a keyframe at current pos.
				AddKeyframe();
				OnPoke();
			}
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
			m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();

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
			DockKeyframePanel(!m_bDocked);
		}
		private void splitKeyframes_Panel2_DoubleClick(object sender, EventArgs e)
		{
			DockKeyframePanel(!m_bDocked);
		}
		private void DockKeyframePanel(bool _bDock)
		{
			if(_bDock)
			{
				// hide the keyframes, change image.
				splitKeyframes.SplitterDistance = splitKeyframes.Height - 25;
				btnDockBottom.BackgroundImage = Resources.undock16x16;
				btnDockBottom.Visible = m_FrameServer.Metadata.Count > 0;
			}
			else
			{
				// show the keyframes, change image.
				splitKeyframes.SplitterDistance = splitKeyframes.Height - 140;
				btnDockBottom.BackgroundImage = Resources.dock16x16;
				btnDockBottom.Visible = true;
			}
			
			m_bDocked = _bDock;
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
				DockKeyframePanel(false);
			}
		}
		#endregion

		#endregion

		#region Drawings Toolbar Events
		private void btnDrawingToolPointer_Click(object sender, EventArgs e)
		{
			OnPoke();
			m_ActiveTool = m_PointerTool;
			SetCursor(m_PointerTool.GetCursor(0));
			if (m_FrameServer.Metadata.Count < 1)
			{
				DockKeyframePanel(true);
			}
		}
		private void drawingTool_Click(object sender, EventArgs e)
		{
			// User clicked on a drawing tool button. A reference to the tool is stored in .Tag
			// Set this tool as the active tool (waiting for the actual use) and set the cursor accordingly.
			
			// Deactivate magnifier if not commited.
			if(m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
			{
				DisableMagnifier();
			}
			
			OnPoke();
			
			AbstractDrawingTool tool = ((ToolStripItem)sender).Tag as AbstractDrawingTool;
			if(tool != null)
			{
				if(tool == ToolManager.Plane)
				{
					m_ActiveTool = m_PointerTool;
					m_FrameServer.Metadata.Plane.Visible = !m_FrameServer.Metadata.Plane.Visible;
				}
				else
				{
					m_ActiveTool = tool;
				}
			}
			else
			{
				m_ActiveTool = m_PointerTool;
			}
			
			
			UpdateCursor();
			
			// Ensure there's a key image at this position, unless the tool creates unattached drawings.
			if(m_ActiveTool == m_PointerTool && m_FrameServer.Metadata.Count < 1)
			{
				DockKeyframePanel(true);
			}
			else if(m_ActiveTool.Attached)
			{
				PrepareKeyframesDock();
			}
			
			pbSurfaceScreen.Invalidate();
		}
		private void btnMagnifier_Click(object sender, EventArgs e)
		{
			if (m_FrameServer.VideoFile.Loaded)
			{
				m_ActiveTool = m_PointerTool;

				// Magnifier is half way between a persisting tool (like trackers and chronometers).
				// and a mode like grid and 3dplane.
				if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.NotVisible)
				{
					UnzoomDirectZoom();
					m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.Direct;
					//btnMagnifier.Image = Drawings.magnifieractive;
					SetCursor(Cursors.Cross);
				}
				else if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
				{
					// Revert to no magnification.
					UnzoomDirectZoom();
					m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.NotVisible;
					//btnMagnifier.Image = Drawings.magnifier;
					SetCursor(m_PointerTool.GetCursor(0));
					DoInvalidate();
				}
				else
				{
					DisableMagnifier();
					DoInvalidate();
				}
			}
		}
		private void btnShowComments_Click(object sender, EventArgs e)
		{
			OnPoke();

			if (m_FrameServer.VideoFile.Loaded)
			{
				// If the video is currently playing, the comments are not visible.
				// We stop the video and show them.
				bool bWasPlaying = m_bIsCurrentlyPlaying;
				if (m_bIsCurrentlyPlaying)
				{
					StopPlaying();
					m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
					ActivateKeyframe(m_iCurrentPosition);
				}
				
				if(m_iActiveKeyFrameIndex < 0 || !m_KeyframeCommentsHub.UserActivated || bWasPlaying)
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
				else
				{
					m_KeyframeCommentsHub.UserActivated = false;
					m_KeyframeCommentsHub.Visible = false;
				}
				
			}
		}
		private void btnColorProfile_Click(object sender, EventArgs e)
		{
			OnPoke();

			// Load, save or modify current profile.
			FormToolPresets ftp = new FormToolPresets();
			ScreenManagerKernel.LocateForm(ftp);
			ftp.ShowDialog();
			ftp.Dispose();

			UpdateCursor();
			DoInvalidate();
		}
		private void UpdateCursor()
		{
			if(m_ActiveTool == m_PointerTool)
			{
				SetCursor(m_PointerTool.GetCursor(0));
			}
			else
			{
				SetCursor(m_ActiveTool.GetCursor(m_FrameServer.CoordinateSystem.Stretch));
			}
		}
		private void SetCursor(Cursor _cur)
		{
			pbSurfaceScreen.Cursor = _cur;
		}
		#endregion

		#region Context Menus Events
		
		#region Main
		private void mnuDirectTrack_Click(object sender, EventArgs e)
		{
			// Track the point. No Cross2D was selected.
			// m_DescaledMouse would have been set during the MouseDown event.
			Track trk = new Track(m_DescaledMouse.X, m_DescaledMouse.Y, m_iCurrentPosition, m_FrameServer.VideoFile.CurrentImage, m_FrameServer.VideoFile.CurrentImage.Size);
			m_FrameServer.Metadata.AddTrack(trk, OnShowClosestFrame, Color.CornflowerBlue); // todo: get from track tool.
			
			// Return to the pointer tool.
			m_ActiveTool = m_PointerTool;
			SetCursor(m_PointerTool.GetCursor(0));
			
			DoInvalidate();
		}
		private void mnuSendPic_Click(object sender, EventArgs e)
		{
			// Send the current image to the other screen for conversion into an observational reference.
			if(m_bSynched && m_FrameServer.VideoFile.CurrentImage != null)
			{
				Bitmap img = CloneTransformedImage();
				m_PlayerScreenUIHandler.PlayerScreenUI_SendImage(img);	
			}
		}
		#endregion
		
		#region Drawings Menus
		private void mnuConfigureDrawing_Click(object sender, EventArgs e)
		{
			// Generic menu for all drawings with the Color or ColorSize capability.
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				IDecorable decorableDrawing = m_FrameServer.Metadata[0].Drawings[m_FrameServer.Metadata.SelectedDrawing] as IDecorable;
				if(decorableDrawing != null && decorableDrawing.DrawingStyle.Elements.Count > 0)
				{
					FormConfigureDrawing2 fcd = new FormConfigureDrawing2(decorableDrawing.DrawingStyle, DoInvalidate);
					ScreenManagerKernel.LocateForm(fcd);
					fcd.ShowDialog();
					fcd.Dispose();
					DoInvalidate();
				}
			}
		}
		private void mnuConfigureFading_Click(object sender, EventArgs e)
		{
			// Generic menu for all drawings with the Fading capability.
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureFading fcf = new formConfigureFading(m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				ScreenManagerKernel.LocateForm(fcf);
				fcf.ShowDialog();
				fcf.Dispose();
				DoInvalidate();
			}
		}
		private void mnuConfigureOpacity_Click(object sender, EventArgs e)
		{
			// Generic menu for all drawings with the Opacity capability.
			if(m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				formConfigureOpacity fco = new formConfigureOpacity(m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing], pbSurfaceScreen);
				ScreenManagerKernel.LocateForm(fco);
				fco.ShowDialog();
				fco.Dispose();
				DoInvalidate();
			}
		}
		private void mnuGotoKeyframe_Click(object sender, EventArgs e)
		{
			// Generic menu for all drawings when we are not on their attachement key frame.
			if (m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				long iPosition = m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Drawings[m_FrameServer.Metadata.SelectedDrawing].infosFading.ReferenceTimestamp;

				m_iFramesToDecode = 1;
				ShowNextFrame(iPosition, true);
				UpdateNavigationCursor();
				trkSelection.SelPos = trkFrame.Position;
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void mnuDeleteDrawing_Click(object sender, EventArgs e)
		{
			// Generic menu for all attached drawings.
			DeleteSelectedDrawing();
		}
		private void DeleteSelectedDrawing()
		{
			if (m_FrameServer.Metadata.SelectedDrawingFrame >= 0 && m_FrameServer.Metadata.SelectedDrawing >= 0)
			{
				IUndoableCommand cdd = new CommandDeleteDrawing(DoInvalidate, m_FrameServer.Metadata, m_FrameServer.Metadata[m_FrameServer.Metadata.SelectedDrawingFrame].Position, m_FrameServer.Metadata.SelectedDrawing);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cdd);
				DoInvalidate();
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
					// Add track on this point.
					DrawingCross2D dc = m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings[iSelectedDrawing] as DrawingCross2D;
					if(dc != null)
					{
						Track trk = new Track(dc.CenterPoint.X, dc.CenterPoint.Y, m_iCurrentPosition, m_FrameServer.VideoFile.CurrentImage, m_FrameServer.VideoFile.CurrentImage.Size);
						m_FrameServer.Metadata.AddTrack(trk, OnShowClosestFrame, dc.PenColor);
						
						// Suppress the point as a Drawing (?)
						m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.RemoveAt(iSelectedDrawing);
						m_FrameServer.Metadata.SelectedDrawingFrame = -1;
						m_FrameServer.Metadata.SelectedDrawing = -1;
	
						// Return to the pointer tool.
						m_ActiveTool = m_PointerTool;
						SetCursor(m_PointerTool.GetCursor(0));
					}
				}
			}
			DoInvalidate();
		}
		#endregion
		
		#region Tracking Menus
		private void mnuStopTracking_Click(object sender, EventArgs e)
		{
			Track trk = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as Track;
			if(trk != null)
			{
				trk.StopTracking();
			}
			DoInvalidate();
		}
		private void mnuDeleteEndOfTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdeot = new CommandDeleteEndOfTrack(this, m_FrameServer.Metadata, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdeot);

			DoInvalidate();
			UpdateFramesMarkers();
		}
		private void mnuRestartTracking_Click(object sender, EventArgs e)
		{
			Track trk = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as Track;
			if(trk != null)
			{
				trk.RestartTracking();
			}
			DoInvalidate();
		}
		private void mnuDeleteTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteTrack(this, m_FrameServer.Metadata);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdc);
			
			UpdateFramesMarkers();
			
			// Trigger a refresh of the export to spreadsheet menu, 
			// in case we don't have any more trajectory left to export.
			OnPoke();
		}
		private void mnuConfigureTrajectory_Click(object sender, EventArgs e)
		{
			Track trk = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as Track;
			if(trk != null)
			{
				// Change this trajectory display.
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}
	
				formConfigureTrajectoryDisplay fctd = new formConfigureTrajectoryDisplay(trk, DoInvalidate);
				fctd.StartPosition = FormStartPosition.CenterScreen;
				fctd.ShowDialog();
				fctd.Dispose();
	
				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}
			}
		}
		private void OnShowClosestFrame(Point _mouse, long _iBeginTimestamp, List<AbstractTrackPoint> _positions, int _iPixelTotalDistance, bool _b2DOnly)
		{
			//--------------------------------------------------------------------------
			// This is where the interactivity of the trajectory is done.
			// The user has draged or clicked the trajectory, we find the closest point
			// and we update to the corresponding frame.
			//--------------------------------------------------------------------------


			// Compute the 3D distance (x,y,t) of each point in the path.
			// unscaled coordinates.

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
			UpdateFramesMarkers();
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
			
			DoInvalidate();
		}
		private void mnuChronoDelete_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteChrono(this, m_FrameServer.Metadata);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdc);
			
			UpdateFramesMarkers();
		}
		private void mnuChronoConfigure_Click(object sender, EventArgs e)
		{
			DrawingChrono dc = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as DrawingChrono;
			if(dc != null)
			{
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}
				
				// Change this chrono display.
				formConfigureChrono fcc = new formConfigureChrono(dc, DoInvalidate);
				ScreenManagerKernel.LocateForm(fcc);
				fcc.ShowDialog();
				fcc.Dispose();
				DoInvalidate();
	
				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}	
			}
		}
		#endregion

		#region Magnifier Menus
		private void mnuMagnifierQuit_Click(object sender, EventArgs e)
		{
			DisableMagnifier();
			DoInvalidate();
		}
		private void mnuMagnifierDirect_Click(object sender, EventArgs e)
		{
			// Use position and magnification to Direct Zoom.
			// Go to direct zoom, at magnifier zoom factor, centered on same point as magnifier.
			m_FrameServer.CoordinateSystem.Zoom = m_FrameServer.Metadata.Magnifier.ZoomFactor;
			m_FrameServer.CoordinateSystem.RelocateZoomWindow(m_FrameServer.Metadata.Magnifier.MagnifiedCenter);
			DisableMagnifier();
			m_FrameServer.Metadata.ResizeFinished();
			ToastZoom();
			DoInvalidate();
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
			m_FrameServer.Metadata.Magnifier.ZoomFactor = _fValue;
			UncheckMagnifierMenus();
			_menu.Checked = true;
			DoInvalidate();
		}
		private void UncheckMagnifierMenus()
		{
			mnuMagnifier150.Checked = false;
			mnuMagnifier175.Checked = false;
			mnuMagnifier200.Checked = false;
			mnuMagnifier225.Checked = false;
			mnuMagnifier250.Checked = false;
		}
		private void DisableMagnifier()
		{
			// Revert to no magnification.
			m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.NotVisible;
			//btnMagnifier.Image = Drawings.magnifier;
			SetCursor(m_PointerTool.GetCursor(0));
		}
		#endregion

		#region Grids Menus
		private void mnuGridsConfigure_Click(object sender, EventArgs e)
		{
			Plane3D grid = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as Plane3D;
			if(grid != null)
			{
				formConfigureGrids fcg;
				grid.Selected = false;
				fcg = new formConfigureGrids(grid, pbSurfaceScreen);
				ScreenManagerKernel.LocateForm(fcg);
				fcg.ShowDialog();
				fcg.Dispose();
			}
			
			DoInvalidate();
		}
		private void mnuGridsHide_Click(object sender, EventArgs e)
		{
			Plane3D grid = m_FrameServer.Metadata.ExtraDrawings[m_FrameServer.Metadata.SelectedExtraDrawing] as Plane3D;
			if(grid != null)
			{
				grid.Visible = false;	
				DoInvalidate();

				// Triggers an update to the main menu.
				OnPoke();
			}
		}
		#endregion

		#endregion
		
		#region DirectZoom
		private void UnzoomDirectZoom()
		{
			m_FrameServer.CoordinateSystem.ReinitZoom();
			m_PointerTool.SetZoomLocation(m_FrameServer.CoordinateSystem.Location);
			m_FrameServer.Metadata.ResizeFinished();
		}
		private void IncreaseDirectZoom()
		{
			if (m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.NotVisible)
			{
				DisableMagnifier();
			}

			if(m_bDrawtimeFiltered && m_DrawingFilterOutput.IncreaseZoom != null)
			{
				m_DrawingFilterOutput.IncreaseZoom(m_DrawingFilterOutput.PrivateData);
			}
			else
			{
				// Max zoom : 600%
				if (m_FrameServer.CoordinateSystem.Zoom < 6.0f)
				{
					m_FrameServer.CoordinateSystem.Zoom += 0.10f;
					RelocateDirectZoom();
					m_FrameServer.Metadata.ResizeFinished();
					ToastZoom();
					ReportForSyncMerge();
				}	
			}
			
			DoInvalidate();
		}
		private void DecreaseDirectZoom()
		{
			if(m_bDrawtimeFiltered && m_DrawingFilterOutput.DecreaseZoom != null)
			{
				m_DrawingFilterOutput.DecreaseZoom(m_DrawingFilterOutput.PrivateData);
			}
			else if (m_FrameServer.CoordinateSystem.Zooming)
			{
				if (m_FrameServer.CoordinateSystem.Zoom > 1.1f)
				{
					m_FrameServer.CoordinateSystem.Zoom -= 0.10f;
				}
				else
				{
					m_FrameServer.CoordinateSystem.Zoom = 1.0f;	
				}

				RelocateDirectZoom();
				m_FrameServer.Metadata.ResizeFinished();
				ToastZoom();
				ReportForSyncMerge();
			}
			
			DoInvalidate();
		}
		private void RelocateDirectZoom()
		{
			m_FrameServer.CoordinateSystem.RelocateZoomWindow();
			m_PointerTool.SetZoomLocation(m_FrameServer.CoordinateSystem.Location);
		}
		#endregion
		
		#region Toasts
		private void ToastZoom()
		{
			m_MessageToaster.SetDuration(750);
			int percentage = (int)(m_FrameServer.CoordinateSystem.Zoom * 100);
			m_MessageToaster.Show(String.Format(ScreenManagerLang.Toast_Zoom, percentage.ToString()));
		}
		private void ToastPause()
		{
			m_MessageToaster.SetDuration(750);
			m_MessageToaster.Show(ScreenManagerLang.Toast_Pause);
		}
		#endregion

		#region Synchronisation specifics
		private void SyncSetAlpha(float _alpha)
		{
			m_SyncMergeMatrix.Matrix00 = 1.0f;
			m_SyncMergeMatrix.Matrix11 = 1.0f;
			m_SyncMergeMatrix.Matrix22 = 1.0f;
			m_SyncMergeMatrix.Matrix33 = _alpha;
			m_SyncMergeMatrix.Matrix44 = 1.0f;
			m_SyncMergeImgAttr.SetColorMatrix(m_SyncMergeMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		}
		private void ReportForSyncMerge()
		{
			// We have to re-apply the transformations here, because when drawing in this screen we draw directly on the canvas.
			// (there is no intermediate image that we could reuse here, this might be a future optimization).
			// We need to clone it anyway, so we might aswell do the transform.
			if(m_bSynched && m_FrameServer.VideoFile.CurrentImage != null)
			{
				Bitmap img = CloneTransformedImage();
				m_PlayerScreenUIHandler.PlayerScreenUI_ImageChanged(img);
			}
		}
		private Bitmap CloneTransformedImage()
		{
			Size imgSize = new Size(m_FrameServer.VideoFile.CurrentImage.Size.Width, m_FrameServer.VideoFile.CurrentImage.Size.Height);
			Bitmap img = new Bitmap(imgSize.Width, imgSize.Height);
			Graphics g = Graphics.FromImage(img);
			
			Rectangle rDst;
			if(m_FrameServer.Metadata.Mirrored)
			{
				rDst = new Rectangle(imgSize.Width, 0, -imgSize.Width, imgSize.Height);
			}
			else
			{
				rDst = new Rectangle(0, 0, imgSize.Width, imgSize.Height);
			}
			
			g.DrawImage(m_FrameServer.VideoFile.CurrentImage, rDst, m_FrameServer.CoordinateSystem.ZoomWindow, GraphicsUnit.Pixel);
			return img;
		}
		#endregion
		
		#region VideoFilters Management
		private void EnableDisableAllPlayingControls(bool _bEnable)
		{
			// Disable playback controls and some other controls for the case
			// of a one-frame rendering. (mosaic, single image)
			
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
			sldrSpeed.EnableDisable(_bEnable);
			trkFrame.Enabled = _bEnable;
			trkSelection.Enabled = _bEnable;
			sldrSpeed.Enabled = _bEnable;
			
			btnRafale.Enabled = _bEnable;
			btnSaveVideo.Enabled = _bEnable;
			btnDiaporama.Enabled = _bEnable;
			btnPausedVideo.Enabled = _bEnable;
			
			mnuPlayPause.Visible = _bEnable;
			mnuDirectTrack.Visible = _bEnable;
		}
		private void EnableDisableSnapshot(bool _bEnable)
		{
			btnSnapShot.Enabled = _bEnable;
		}
		private void EnableDisableDrawingTools(bool _bEnable)
		{
			foreach(ToolStripItem tsi in stripDrawingTools.Items)
			{
				tsi.Enabled = _bEnable;
			}
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
					m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
					
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				try
				{
					SaveFileDialog dlgSave = new SaveFileDialog();
					dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
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

						//2. Get image and save it to the file.
						Bitmap outputImage = GetFlushedImage();
						ImageHelper.Save(strImgName, outputImage);						
						outputImage.Dispose();
						m_FrameServer.AfterSave();
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();

				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}
				
				// Launch sequence saving configuration dialog
				formRafaleExport fre = new formRafaleExport(this, m_FrameServer.Metadata, m_FrameServer.VideoFile.FilePath, m_iSelDuration, m_FrameServer.VideoFile.Infos.fAverageTimeStampsPerSeconds, m_FrameServer.VideoFile.Infos.fFps);
				fre.ShowDialog();
				fre.Dispose();
				m_FrameServer.AfterSave();

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
						ImageHelper.Save(fileName, outputImage);
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
					ImageHelper.Save(fileName, outputImage);
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

			DoInvalidate();
		}
		private void btnVideo_Click(object sender, EventArgs e)
		{
			if(m_FrameServer.Loaded)
			{
				StopPlaying();
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
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
				m_PlayerScreenUIHandler.PlayerScreenUI_PauseAsked();
				
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
		public void Save()
		{
			// Todo:
			// Eventually, this call should be done directly by PlayerScreen, without passing through the UI.
			// This will be possible when m_FrameServer.Metadata, m_iSelStart, m_iSelEnd are encapsulated in m_FrameServer
			// and when PlaybackFrameInterval, m_iSlowmotionPercentage, GetOutputBitmap are available publically.
			
			m_FrameServer.Save(	GetPlaybackFrameInterval(),
			                   m_fSlowmotionPercentage,
			                   m_iSelStart,
			                   m_iSelEnd,
			                   new DelegateGetOutputBitmap(GetOutputBitmap));
		}
		public long GetOutputBitmap(Graphics _canvas, Bitmap _sourceImage, long _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly)
		{
			// Used by the VideoFile for SaveMovie.
			// The image to save was already retrieved (from stream or analysis array)
			// This image is already drawn on _canvas.
			// Here we we flush the drawings on it if needed.
			// We return the distance to the closest key image.
			// This can then be used by the caller.

			// 1. Look for the closest key image.
			long iClosestKeyImageDistance = long.MaxValue;	
			int iKeyFrameIndex = -1;
			for(int i=0; i<m_FrameServer.Metadata.Keyframes.Count;i++)
			{
				long iDistance = Math.Abs(_iTimestamp - m_FrameServer.Metadata.Keyframes[i].Position);
				if(iDistance < iClosestKeyImageDistance)
				{
					iClosestKeyImageDistance = iDistance;
					iKeyFrameIndex = i;
				}
			}

			// 2. Invalidate the distance if we wanted only key images, and we are not on one.
			// Or if there is no key image at all.
			if ( (_bKeyframesOnly && iClosestKeyImageDistance != 0) || (iClosestKeyImageDistance == long.MaxValue))
			{
				iClosestKeyImageDistance = -1;
			}
			
			// 3. Flush drawings if needed.
			if(_bFlushDrawings)
			{
				Bitmap rawImage = null;
				
				if(m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.NotVisible)
				{
					// For the magnifier, we must clone the image since the graphics object has been 
					// extracted from the image itself (painting fails if we reuse the uncloned image).
					// And we must clone it before the drawings are flushed on it.
					rawImage = AForge.Imaging.Image.Clone(_sourceImage);
				}
				
				if (_bKeyframesOnly)
				{
					if(iClosestKeyImageDistance == 0)
					{
						FlushDrawingsOnGraphics(_canvas, iKeyFrameIndex, _iTimestamp, 1.0f, 1.0f, new Point(0,0));
						FlushMagnifierOnGraphics(rawImage, _canvas);
					}
				}
				else
				{
					if(iClosestKeyImageDistance == 0)
					{
						FlushDrawingsOnGraphics(_canvas, iKeyFrameIndex, _iTimestamp, 1.0f, 1.0f, new Point(0,0));	
					}
					else
					{
						FlushDrawingsOnGraphics(_canvas, -1, _iTimestamp, 1.0f, 1.0f, new Point(0,0));
					}
					
					FlushMagnifierOnGraphics(rawImage, _canvas);
				}	
			}

			return iClosestKeyImageDistance;
		}
		public Bitmap GetFlushedImage()
		{
			// Returns an image with all drawings flushed, including
			// grids, chronos, magnifier, etc.
			// image should be at same strech factor than the one visible on screen.
			Size iNewSize = new Size((int)((double)m_FrameServer.VideoFile.CurrentImage.Width * m_FrameServer.CoordinateSystem.Stretch), (int)((double)m_FrameServer.VideoFile.CurrentImage.Height * m_FrameServer.CoordinateSystem.Stretch));
			Bitmap output = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
			output.SetResolution(m_FrameServer.VideoFile.CurrentImage.HorizontalResolution, m_FrameServer.VideoFile.CurrentImage.VerticalResolution);
			
			if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
			{
				m_DrawingFilterOutput.Draw(Graphics.FromImage(output), iNewSize, m_DrawingFilterOutput.PrivateData);
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
				case TimeCodeFormat.Milliseconds:
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
