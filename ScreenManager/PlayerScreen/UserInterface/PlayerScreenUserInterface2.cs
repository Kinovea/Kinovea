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
//using CPI.Plot3D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Resources;
using System.Runtime.InteropServices;
using VideaPlayerServer;
using Videa.Services;
#endregion

namespace Videa.ScreenManager
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

		/*
        [DllImport("gdi32.dll")]
        public static extern long BitBlt (System.IntPtr a, int b, int c, int d, int e, System.IntPtr f, int g, int h, int i);

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020, // dest = source
            SRCPAINT = 0x00EE0086, // dest = source OR dest
            SRCAND = 0x008800C6, // dest = source AND dest
            SRCINVERT = 0x00660046, // dest = source XOR dest
            SRCERASE = 0x00440328, // dest = source AND (NOT dest )
            NOTSRCCOPY = 0x00330008, // dest = (NOT source)
            NOTSRCERASE = 0x001100A6, // dest = (NOT src) AND (NOT dest)
            MERGECOPY = 0x00C000CA, // dest = (source AND pattern)
            MERGEPAINT = 0x00BB0226, // dest = (NOT source) OR dest
            PATCOPY = 0x00F00021, // dest = pattern
            PATPAINT = 0x00FB0A09, // dest = DPSnoo
            PATINVERT = 0x005A0049, // dest = pattern XOR dest
            DSTINVERT = 0x00550009, // dest = (NOT dest)
            BLACKNESS = 0x00000042, // dest = BLACK
            WHITENESS = 0x00FF0062, // dest = WHITE
        };
        public enum StretchMode
        {
            STRETCH_ANDSCANS = 1,
            STRETCH_ORSCANS = 2,
            STRETCH_DELETESCANS = 3,
            STRETCH_HALFTONE = 4,
        }

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        static extern bool StretchBlt(  IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
                                        int nWidthDest, int nHeightDest,
                                        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
                                        TernaryRasterOperations dwRop);
        
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool SetStretchBltMode(IntPtr hdc, StretchMode iStretchMode);
		 */

		#endregion

		#region Délégués

		// 1. Affectées et accédées depuis PlayerScreen.cs
		public delegate void DelegateCloseMeUI();		
		public delegate void DelegateSetMeAsActiveScreenUI();

		public delegate void ReportReady(bool _bIntervalOnly);
		public delegate void ReportSelectionChanged(bool _bInitialization);

		public DelegateCloseMeUI				m_CloseMeUI;
		public DelegateSetMeAsActiveScreenUI 	m_SetMeAsActiveScreenUI;
		
		public ReportReady                 	m_ReportReady;
		public ReportSelectionChanged      	m_ReportSelectionChanged;

		// 2. Internes
		private delegate void TimerEventHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2);
		private delegate void CallbackPlayLoop();
		private delegate void ProxySetAsActiveScreen();
		
		private TimerEventHandler m_CallbackTimerEventHandler;
		private CallbackPlayLoop m_CallbackPlayLoop;
		private ProxySetAsActiveScreen m_ProxySetAsActiveScreen;
		
		#endregion

		#region Structs
		public struct DropWatcher
		{
			public int iLastDropCount;
			public int iLastFrameInterval;
		};
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
		public Metadata Metadata
		{
			get { return m_Metadata; }
		}
		public int SlowmotionPercentage
		{
			get { return m_iSlowmotionPercentage; }
		}
		public bool IsCurrentlyPlaying
		{
			get { return m_bIsCurrentlyPlaying; }
		}

		// Pseudo Filters (Modifies Rendering)
		public bool Deinterlaced
		{
			get { return m_PlayerServer.m_InfosVideo.bDeinterlaced;}
			set
			{
				m_PlayerServer.m_InfosVideo.bDeinterlaced = value;

				// If there was a selection it must be imported again.
				// (This means we'll loose color adjustments.)
				if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1)
				{
					SwitchToAnalysisMode(true);
				}
			}
		}
		public bool Mirrored
		{
			get { return m_Metadata.Mirrored; }
			set { m_Metadata.Mirrored = value; }
		}
		public bool ShowGrid
		{
			get { return m_Metadata.Grid.Visible; }
			set { m_Metadata.Grid.Visible = value; }
		}
		public bool Show3DPlane
		{
			get { return m_Metadata.Plane.Visible; }
			set { m_Metadata.Plane.Visible = value; }
		}
		
		// Synchro
		public bool Synched
		{
			get { return m_bSynched; }
			set { m_bSynched = value; }
		}
		public Int64 SyncPosition
		{
			get { return m_iSyncPosition; }
			set { m_iSyncPosition = value; }
		}
		
		// Slowness (when video wasn't captured realtime)
		public double SlowFactor
		{
			get { return m_fSlowFactor; }
			set { m_fSlowFactor = value; }
		}
		
		// DrawtimeFilterType
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

		// Low level routines.
		public PlayerServer m_PlayerServer = new PlayerServer();

		// General
		private ResourceManager m_ResourceManager;
		private PreferencesManager m_PrefManager;
		private string m_FullPath = "";
		
		// Playback current state
		private bool m_bIsCurrentlyPlaying;
		private int m_iFramesToDecode = 1;
		private bool m_bSeekToStart;
		private bool m_bSynched;
		private Int64 m_iSyncPosition;
		private uint m_IdMultimediaTimer;
		private PlayingMode m_ePlayingMode = PlayingMode.Loop;
		private int m_iDroppedFrames;                  // For debug purposes only.
		private int m_iDecodedFrames;
		private int m_iSlowmotionPercentage = 100;
		private bool m_bIsIdle = true;

		// Image
		private bool m_bStretchModeOn;
		private double m_fStretchFactor = 1.0f;
		private Rectangle m_CropRectangle;
		private Panel panelCenter
		{
			get { return _panelCenter; }
			//set { _panelCenter = value; }
		}
		private PictureBox m_SurfaceScreen
		{
			get { return _surfaceScreen; }
		}
		private bool m_bShowImageBorder;
		private static readonly Pen m_PenImageBorder = Pens.SteelBlue;
		
		// Selection (All values in TimeStamps)
		// trkSelection.minimum and maximum are also in absolute timestamps.
		private Int64 m_iTotalDuration;
		private Int64 m_iSelStart;          // Valeur absolue, par défaut égale à m_iStartingPosition. (pas 0)
		private Int64 m_iSelEnd;            // Value absolue
		private Int64 m_iSelDuration;
		private Int64 m_iCurrentPosition;    // Valeur absolue dans l'ensemble des timestamps.
		private Int64 m_iStartingPosition;   // Valeur absolue correspond au timestamp de la première frame.
		private bool m_bHandlersLocked;
		private bool m_bResetingHandlers;
		
		// Keyframes, Drawings, etc.
		private Metadata m_Metadata;
		private int m_iActiveKeyFrameIndex;
		private DrawingToolType m_ActiveTool;
		private AbstractDrawingTool[] m_DrawingTools;
		private ColorProfile m_ColorProfile;
		private formKeyframeComments m_KeyframeCommentsHub;
		private bool m_bDocked;
		private bool m_bTextEdit;
		private bool m_bMeasuring;

		// Video Filters Management
		private bool m_bDrawtimeFiltered;
		private DrawtimeFilterOutput m_DrawingFilterOutput;
		
		// Others
		private Magnifier m_Magnifier;
		private double m_fDirectZoomFactor;       // Direct zoom (CTRL+/-)
		private Rectangle m_DirectZoomWindow = new Rectangle(0, 0, 0, 0);
		private Double m_fSlowFactor = 1.0f;             // Only for when capture fps is different from Playing fps.
		
		#region Context Menus
		private ContextMenuStrip  popMenu;
		private ToolStripMenuItem mnuPlayPause;
		private ToolStripMenuItem mnuSetSelectionEnd;
		private ToolStripMenuItem mnuSetSelectionStart;
		private ToolStripMenuItem mnuLockSelection;
		private ToolStripMenuItem mnuSetCaptureSpeed;
		private ToolStripMenuItem mnuSavePic;
		private ToolStripMenuItem mnuCloseScreen;

		private ContextMenuStrip popMenuDrawings;
		private ToolStripMenuItem mnuConfigureDrawing;
		private ToolStripMenuItem mnuConfigureFading;
		private ToolStripMenuItem mnuTrackTrajectory;
		private ToolStripMenuItem mnuGotoKeyframe;
		private ToolStripSeparator mnuSepDrawing;
		private ToolStripMenuItem mnuDeleteDrawing;
		private ToolStripMenuItem mnuShowMeasure;
		private ToolStripMenuItem mnuSealMeasure;
		
		private ContextMenuStrip popMenuTrack;
		private ToolStripMenuItem mnuRestartTracking;
		private ToolStripMenuItem mnuStopTracking;
		private ToolStripMenuItem mnuDeleteTrajectory;
		private ToolStripMenuItem mnuDeleteEndOfTrajectory;
		private ToolStripMenuItem mnuConfigureTrajectory;

		private ContextMenuStrip popMenuChrono;
		private ToolStripMenuItem mnuChronoStart;
		private ToolStripMenuItem mnuChronoStop;
		private ToolStripMenuItem mnuChronoHide;
		private ToolStripMenuItem mnuChronoCountdown;
		private ToolStripMenuItem mnuChronoDelete;
		private ToolStripMenuItem mnuChronoConfigure;

		private ContextMenuStrip popMenuMagnifier;
		private ToolStripMenuItem mnuMagnifier150;
		private ToolStripMenuItem mnuMagnifier175;
		private ToolStripMenuItem mnuMagnifier200;
		private ToolStripMenuItem mnuMagnifier225;
		private ToolStripMenuItem mnuMagnifier250;
		private ToolStripMenuItem mnuMagnifierDirect;
		private ToolStripMenuItem mnuMagnifierQuit;

		private ContextMenuStrip popMenuGrids;
		private ToolStripMenuItem mnuGridsConfigure;
		private ToolStripMenuItem mnuGridsHide;
		#endregion

		// Debug
		private bool m_bShowInfos;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		#if TRACE
		//-------------------------------------------------------------------
		// Debug only :
		// Performance Counters needs Admin rights.
		//-------------------------------------------------------------------
		//private PerformanceCounter m_RamCounter;
		/*private PerformanceCounter m_RenderedPerSecond;
        private PerformanceCounter m_DecodedPerSecond;
        private PerformanceCounter m_DropsPerSecond;
        private Stopwatch m_DecodeWatch;
        private Stopwatch m_RenderingWatch;
        private Stopwatch m_NotIdleWatch;*/
		#endif

		#endregion

		#region Constructor
		public PlayerScreenUserInterface(ResourceManager _resManager)
		{
			log.Debug("Constructing the PlayerScreen user interface.");
			
			InitializeComponent();
			
			Dock = DockStyle.Fill;
			
			// log.Debug("General settings initialization.");
			m_ResourceManager = _resManager;
			m_PrefManager = PreferencesManager.Instance();
			lblFileName.Text = "";
			SetupDirectZoom();
			HideResizers();

			// log.Debug("Selection settings initialization.");
			InitTimestamps();
			SetupPrimarySelectionPanel();
			SetupNavigationPanel();
			SetupSpeedTunerPanel();

			// log.Debug("Keyframes and drawings tools settings initialization.");
			SetupKeyframesAndDrawingTools();

			// log.Debug("Menus initialization.");
			BuildContextMenus();
			
			// Délégués internes
			m_CallbackTimerEventHandler = new TimerEventHandler(MultimediaTimerTick);
			m_CallbackPlayLoop = new CallbackPlayLoop(PlayLoop);
			m_ProxySetAsActiveScreen = new ProxySetAsActiveScreen(SetAsActiveScreen);

			#region Instrumentation
			
			// Advanced infos panel
			SetupDebugPanel(false);

			#if TRACE
			//m_RamCounter = new PerformanceCounter("Memory", "Available MBytes");
			/*if (!PerformanceCounterCategory.Exists("KinoveaCounters3"))
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                // 1. Frames Rendering Per Second
                CounterCreationData renderedPerSecond = new CounterCreationData();
                renderedPerSecond.CounterName = "Frames/s (Rendered)";
                renderedPerSecond.CounterHelp = "Number of frames rendered per second";
                renderedPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(renderedPerSecond);

                // 2. Frames Decoding Per Second
                CounterCreationData decodedPerSecond = new CounterCreationData();
                decodedPerSecond.CounterName = "Frames/s (Decoded)";
                decodedPerSecond.CounterHelp = "Number of frames decoded per second";
                decodedPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(decodedPerSecond);

                // 3. Drops Per Second
                CounterCreationData dropsPerSecond = new CounterCreationData();
                dropsPerSecond.CounterName = "Drops/s";
                dropsPerSecond.CounterHelp = "Number of drops per second";
                dropsPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(dropsPerSecond);

                PerformanceCounterCategory.Create("KinoveaCounters3", "Kinovea Instrumentation", counters);
            }*/

			// Create counters.
			/*m_RenderedPerSecond = new PerformanceCounter();
            m_RenderedPerSecond.CategoryName = "KinoveaCounters3";
            m_RenderedPerSecond.CounterName = "Frames/s (Rendered)";
            m_RenderedPerSecond.MachineName = ".";
            m_RenderedPerSecond.ReadOnly = false;

            m_DecodedPerSecond = new PerformanceCounter();
            m_DecodedPerSecond.CategoryName = "KinoveaCounters3";
            m_DecodedPerSecond.CounterName = "Frames/s (Decoded)";
            m_DecodedPerSecond.MachineName = ".";
            m_DecodedPerSecond.ReadOnly = false;

            m_DropsPerSecond = new PerformanceCounter();
            m_DropsPerSecond.CategoryName = "KinoveaCounters3";
            m_DropsPerSecond.CounterName = "Drops/s";
            m_DropsPerSecond.MachineName = ".";
            m_DropsPerSecond.ReadOnly = false;

            m_DecodeWatch = new Stopwatch();
            m_NotIdleWatch = new Stopwatch();
            m_RenderingWatch = new Stopwatch();*/
			#endif
			#endregion
		}
		#endregion

		#region Various Inits & Setups
		public int PostLoadProcess(int _iMovieLoadResult, string _FilePath)
		{
			//---------------------------------------------------------------------------------------------------
			// On passe ici, que le chargement de la vidéo ait été un succès ou pas.
			// Si c'est le cas, on tente de charger la première image.
			// Le succès final du chargement de la vidéo est établi si la première image est affichée sans erreur
			//---------------------------------------------------------------------------------------------------
			int iPostLoadResult = 0;

			m_fSlowFactor = 1.0f;
			HideResizers();
			SetupKeyframesPanel();
			m_Metadata.Reset();// unloadmovie complet ?
			m_bDrawtimeFiltered = false;
			EnableDisableAllPlayingControls(true);
	        EnableDisableDrawingTools(true);

			trkSelection.Minimum = 0;
			trkSelection.Maximum = 100;

			m_bResetingHandlers     = true;
			trkSelection.SelStart = trkSelection.Minimum;
			trkSelection.SelEnd = trkSelection.Maximum;
			m_bResetingHandlers     = false;

			if (_iMovieLoadResult == 0)
			{
				//-------------------------
				// Préparation à la lecture
				//-------------------------
				EnableControls();
				m_iCurrentPosition = 0;
				m_iStartingPosition = 0;

				m_FullPath = _FilePath;

				//------------------------------------------
				// Tentative de récupération des meta données
				// Ne charge pas les images.
				//------------------------------------------
				ParseMetadata(m_PlayerServer.GetMetadata());

				//--------------------------
				// Affichage Première frame.
				//--------------------------
				m_iFramesToDecode = 1;
				int iShowFrameResult  = ShowNextFrame(-1, true);
				UpdateNavigationCursor();

				//--------------
				// Sanity Check
				//--------------
				if (iShowFrameResult != 0)
				{
					//-------------------------------------------------
					// La première image n'a put être chargée.
					// Cas le plus courant : taille image non standard.
					//-------------------------------------------------
					iPostLoadResult = -1;
					m_PlayerServer.m_InfosVideo.iDurationTimeStamps = 0;
					m_PlayerServer.m_InfosVideo.iFirstTimeStamp = 0;
					InitTimestamps();
					log.Error("First frame couldn't be loaded - aborting");
				}
				else
				{
					if (m_iCurrentPosition < 0)
					{
						//-------------------------------------------
						// Première image chargée, mais incohérence.
						// Exemple : AVCHD entrelacé.
						// (fichiers .m2ts des dernières caméras HDV)
						//-------------------------------------------
						log.Error(String.Format("First frame loaded but negative timestamp ({0}) - aborting", m_iCurrentPosition));
						iPostLoadResult = -2;

						m_PlayerServer.m_InfosVideo.iDurationTimeStamps = 0;
						m_PlayerServer.m_InfosVideo.iFirstTimeStamp = 0;
						InitTimestamps();
					}
					else
					{
						//---------------------------------------------------------------------------------------
						// Notes:
						// L'ordre d'appel est important.
						// Les problèmes viennent des différences possibles entre les données lues dans les infos
						// globales et les données lues au décodage réel.
						// Les timestamps de la première frame et la durée en timestamps peuvent être faux.
						// On réajuste comme on peut à l'aide des données lues dans la première frame et
						// dans la dernière frame si on est passé en mode analyse.
						//---------------------------------------------------------------------------------------

						iPostLoadResult = 0;
						m_SurfaceScreen.Invalidate();

						//---------------------------------------------------
						// 1. Timestamps et panels de Selections / FrameTrack
						//---------------------------------------------------
						log.Debug(String.Format("First frame loaded. Timestamp : {0}", m_iCurrentPosition));
						log.Debug(String.Format("Announced first timestamp : {0}", m_PlayerServer.m_InfosVideo.iFirstTimeStamp));
						
						//-----------------------------------------------------------------------------
						// [2008-04-26] Time stamp non 0 :Assez courant en fait.
						// La première frame peut avoir un timestamp à 1 au lieu de 0 selon l'encodeur.
						// Sans que cela soit répercuté sur iFirstTimeStamp...
						// On fixe à la main.
						//-----------------------------------------------------------------------------
						m_PlayerServer.m_InfosVideo.iFirstTimeStamp = m_iCurrentPosition;
						m_iStartingPosition = m_iCurrentPosition;
						m_iTotalDuration = m_PlayerServer.m_InfosVideo.iDurationTimeStamps;

						// Set temporary values.
						double fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
						m_iSelStart     = m_iStartingPosition;
						m_iSelEnd       = (long)((double)(m_iTotalDuration + m_iStartingPosition) - fAverageTimeStampsPerFrame);
						m_iSelDuration  = m_iTotalDuration;
						trkSelection.Minimum = m_iSelStart;
						trkSelection.Maximum = m_iSelEnd;
						m_bResetingHandlers = true;
						trkSelection.SelStart = trkSelection.Minimum;
						trkSelection.SelEnd = trkSelection.Maximum;
						m_bResetingHandlers = false;

						// On switche en mode analyse si possible.
						SwitchToAnalysisMode(false);

						// We now have solid facts for m_iSelStart, m_iSelEnd and m_iSelDuration.
						// Let's update all variables with them.
						m_iCurrentPosition = m_iSelStart;
						m_PlayerServer.m_InfosVideo.iFirstTimeStamp = m_iCurrentPosition;
						m_iStartingPosition = m_iCurrentPosition;
						m_iTotalDuration = m_iSelDuration;
						trkSelection.Minimum = m_iSelStart;
						trkSelection.Maximum = m_iSelEnd;
						m_bResetingHandlers = true;
						trkSelection.SelStart = trkSelection.Minimum;
						trkSelection.SelEnd = trkSelection.Maximum;
						m_bResetingHandlers = false;


						trkSelection.SelPos = m_iCurrentPosition;
						trkSelection.UpdateSelectedZone();
						UpdatePrimarySelectionPanelInfos();

						SetupNavigationPanel();
						SetupSpeedTunerPanel();

						//---------------------------------------------------
						// 2. Autres infos diverses
						//---------------------------------------------------
						m_iDecodedFrames = 1;
						m_iDroppedFrames = 0;
						m_bSeekToStart = false;
						
						
						// Setup Metadata global infos in case we want to flush it to a file (or mux).
						// (Might have already been stored if muxed metadata)
						Size imageSize = new Size(m_PlayerServer.m_InfosVideo.iDecodingWidth, m_PlayerServer.m_InfosVideo.iDecodingHeight);
						m_Metadata.ImageSize = imageSize;
						m_Metadata.AverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame;
						m_Metadata.FirstTimeStamp = m_PlayerServer.m_InfosVideo.iFirstTimeStamp;
						m_Metadata.Plane.SetLocations(imageSize, 1.0, new Point(0,0));
						m_Metadata.Grid.SetLocations(imageSize, 1.0, new Point(0,0));
						((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).ImgSize = imageSize;
						
						//Inits diverses
						UpdateFilenameLabel();
						m_CropRectangle = new Rectangle(imageSize.Width / 4, imageSize.Height / 4, imageSize.Width / 2, imageSize.Height/2);
						

						//---------------------------------------------------
						// 3. Positionnement de l'écran.
						//---------------------------------------------------
						SetUpForNewMovie();
						m_KeyframeCommentsHub.UserActivated = false;

						//--------------------------------------------------------------------------
						// 4. Check if there is an Analysis with the same name in the same directory
						//--------------------------------------------------------------------------
						if (!m_Metadata.HasData)
						{
							LookForLinkedAnalysis();
						}
						
						// Do the post import wether the data come from external file or included xml.
						if (m_Metadata.HasData)
						{
							PostImportAnalysis();
						}

						UpdateKeyframesMarkers();

						// Debug
						if (m_bShowInfos) { UpdateDebugInfos(); }
					}
				}
			}
			else
			{
				DisableControls();
			}

			return iPostLoadResult;
		}
		private void ParseMetadata(String _metadata)
		{
			//---------------------------------------------------------------------
			// Utilisé uniquement dans le cadre de la récupération des metadonnées
			// directement depuis le fichier vidéo. (mux)
			// Pour l'import XML, voir ScreenManager.LoadAnalysis()
			//---------------------------------------------------------------------
			if (_metadata != null)
			{
				// TODO - save previous metadata for undo.
				m_Metadata = Metadata.FromXmlString(_metadata, m_PlayerServer.m_InfosVideo.iDecodingWidth, m_PlayerServer.m_InfosVideo.iDecodingHeight, m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame, m_FullPath, new GetTimeCode(TimeStampsToTimecode), new ShowClosestFrame(OnShowClosestFrame));
				UpdateKeyframesMarkers();
				OrganizeKeyframes();
			}
		}
		public void PostImportAnalysis()
		{
			//----------------------------------------------------------
			// Analysis file was imported into metadata.
			// We still need to load each frames and do some scaling.
			//----------------------------------------------------------

			// Public because accessed from : ScreenManager upon loading analysis.

			// TODO - progress bar ?

			int iOutOfRange = -1;
			int iCurrentKeyframe = -1;

			foreach (Keyframe kf in m_Metadata.Keyframes)
			{
				iCurrentKeyframe++;

				// Récupérer l'image
				if (kf.Position < (m_PlayerServer.m_InfosVideo.iFirstTimeStamp + m_PlayerServer.m_InfosVideo.iDurationTimeStamps))
				{
					m_iFramesToDecode = 1;
					ShowNextFrame(kf.Position, true);
					UpdateNavigationCursor();
					UpdateCurrentPositionInfos();
					trkSelection.SelPos = trkFrame.Position;

					// Readjust and complete the Keyframe
					kf.Position = m_iCurrentPosition;
					kf.ImportImage(m_PlayerServer.m_BmpImage);
					kf.GenerateDisabledThumbnail();

					// EditBoxes
					foreach (AbstractDrawing ad in kf.Drawings)
					{
						if (ad is DrawingText)
						{
							((DrawingText)ad).ContainerScreen = m_SurfaceScreen;
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
				m_Metadata.Keyframes.RemoveRange(iOutOfRange, m_Metadata.Keyframes.Count - iOutOfRange);
			}

			UpdateFilenameLabel();
			OrganizeKeyframes();
			if(m_Metadata.Count > 0)
			{
				UndockKeyframePanel();
			}
			
			// Se replacer en début de sélection et mettre à jour
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iSelStart, true);
			UpdateNavigationCursor();
			ActivateKeyframe(m_iCurrentPosition);

			Size sz = new Size(m_PlayerServer.m_InfosVideo.iDecodingWidth, m_PlayerServer.m_InfosVideo.iDecodingHeight);
			m_Metadata.ImageSize = sz;
			m_Metadata.Plane.SetLocations(sz, 1.0, new Point(0,0));
			m_Metadata.Grid.SetLocations(sz, 1.0, new Point(0, 0));
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).ImgSize = sz;

			// Analysis is now fully initialized.
			m_Metadata.CleanupHash();
			
			m_SurfaceScreen.Invalidate();
		}
		private void EnableControls()
		{
			//trkSelection.Enabled = true;
			//trkFrame.Enabled = true;
			sldrSpeed.Enabled = true;
		}
		private void SetupPrimarySelectionPanel()
		{
			//--------------------------
			// Setup data
			//--------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				double  fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
				m_iSelStart = m_iStartingPosition;
				m_iSelEnd = (long)((double)(m_iTotalDuration + m_iStartingPosition) - fAverageTimeStampsPerFrame);
				m_iSelDuration = m_iTotalDuration;
			}
			else
			{
				m_iTotalDuration = 100;
				m_iSelStart = 0;
				m_iSelEnd = 99;
				m_iSelDuration = 100;
			}

			//--------------------------
			// Setup contrôle
			//--------------------------
			trkSelection.Minimum = m_iSelStart;
			trkSelection.Maximum = m_iSelEnd;

			// /!\ La ligne suivante déclenche un SelectionChanged qui va déboucher sur un Switch To Analysis Mode
			// Ce qui peut entrâiner la modification de m_iSelEnd...
			trkSelection.SelStart   = m_iSelStart;
			trkSelection.SelEnd     = m_iSelEnd;
			trkSelection.SelPos     = m_iCurrentPosition;

			//--------------------------
			// Setup Labels
			//--------------------------
			UpdatePrimarySelectionPanelInfos();
		}
		private void SetupNavigationPanel()
		{
			//--------------------------
			// Setup data
			//--------------------------
			if (!m_PlayerServer.m_bIsMovieLoaded)
			{
				m_iCurrentPosition = 0;
			}

			//--------------------------
			// Setup contrôle
			//--------------------------
			trkFrame.Minimum = m_iSelStart;
			trkFrame.Maximum = m_iSelEnd;
			trkFrame.Position = m_iCurrentPosition;
			
			// Don't touch, should have been taken care of during SwitchToAnalysisMode.
			//trkFrame.ReportOnMouseMove = false;

			//--------------------------
			// Setup Labels
			//--------------------------
			UpdateCurrentPositionInfos();
			
		}
		private void SetupSpeedTunerPanel()
		{
			sldrSpeed.Minimum = 1;
			sldrSpeed.Maximum = 200;
			sldrSpeed.Value = 100;
			sldrSpeed.StickyValue = 100;
			m_iSlowmotionPercentage = sldrSpeed.Value;
		}
		private void SetUpForNewMovie()
		{
			SetupDirectZoom();

			// Problem: The screensurface hasn't got its final size...
			// So it doesn't make much sense to call it here...
			m_fStretchFactor = 1;
			ShowResizers();
			StretchSqueezeSurface();
			// Since it hadn't its final size, we don't really know if the pic is too large...
			m_bStretchModeOn = false;

			StopPlaying();

			if (m_bIsCurrentlyPlaying)
			{
				buttonPlay.BackgroundImage = Videa.ScreenManager.Properties.Resources.liqplay17;
				m_bIsCurrentlyPlaying = false;
			}

			m_Metadata.Plane.Visible = false;
			m_Metadata.Grid.Visible = false;
			
			SetAsActiveScreen();
		}
		private void SetupKeyframeCommentsHub()
		{
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.MakeTopMost != null)
			{
				// Is it ok to use the "this" keyword here ?
				m_KeyframeCommentsHub = new formKeyframeComments(this);
				dp.MakeTopMost(m_KeyframeCommentsHub);
			}
		}
		private void LookForLinkedAnalysis()
		{
			if (m_Metadata.Count == 0)
			{
				// If there is an Anlaysis with the same file name in the same directory,
				// we'll directly load it.

				// Figure out the complete path of hypothetical Analysis
				string kvaFile = Path.GetDirectoryName(m_FullPath);
				kvaFile = kvaFile + "\\" + Path.GetFileNameWithoutExtension(m_FullPath) + ".kva";
				if (File.Exists(kvaFile))
				{
					m_Metadata.LoadFromFile(kvaFile);
				}
				else
				{
					// By default the filename will be the one of the video
					m_Metadata.FullPath = m_FullPath;
				}
			}
			else
			{
				// Meta data has probably been loaded from within the file itself.
			}
		}
		public void DisableControls()
		{
			// Public because accessed from CommandLoadMovie if failed. (Fixme ?)

			//appelé sur Unload, remettre tout à zéro.
			InitTimestamps();
			SetupPrimarySelectionPanel();
			SetupNavigationPanel();
			SetupSpeedTunerPanel();

			//trkSelection.Enabled = false;
			//trkFrame.Enabled = false;
			sldrSpeed.Enabled = false;

			pnlThumbnails.Controls.Clear();

			_panelCenter.Refresh();
			_surfaceScreen.Refresh();
		}
		private void UpdateFilenameLabel()
		{
			lblFileName.Text = Path.GetFileName(m_FullPath);
		}
		private void InitTimestamps()
		{
			m_iTotalDuration = 0;
			m_iSelStart = 0;
			m_iSelEnd = 0;
			m_iSelDuration = 0;
			m_iCurrentPosition = 0;
			m_iStartingPosition = 0;
			m_bHandlersLocked = false;
			m_bResetingHandlers = false;
			m_fSlowFactor = 1.0f;
		}
		private void SetupKeyframesPanel()
		{
			m_Metadata.Clear();
			OrganizeKeyframes();
		}
		private void ShowResizers()
		{
			ImageResizerNE.Visible = true;
			ImageResizerNW.Visible = true;
			ImageResizerSE.Visible = true;
			ImageResizerSW.Visible = true;
		}
		private void HideResizers()
		{
			ImageResizerNE.Visible = false;
			ImageResizerNW.Visible = false;
			ImageResizerSE.Visible = false;
			ImageResizerSW.Visible = false;
		}
		private void BuildContextMenus()
		{
			#region 1. pop Menu by default. (Working Zone, Start Stop, etc.)
			popMenu = new ContextMenuStrip();

			// Séparateurs
			ToolStripSeparator mnuSep = new ToolStripSeparator();
			ToolStripSeparator mnuSep2 = new ToolStripSeparator();
			ToolStripSeparator mnuSep3 = new ToolStripSeparator();

			// Lecture / Pause
			mnuPlayPause = new ToolStripMenuItem();
			mnuPlayPause.Tag = new ItemResourceInfo(m_ResourceManager, "mnuPlayPause");
			mnuPlayPause.Text = ((ItemResourceInfo)mnuPlayPause.Tag).resManager.GetString(((ItemResourceInfo)mnuPlayPause.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			//mnuPlayPause.ShortcutKeys = System.Windows.Forms.Keys.Space;
			mnuPlayPause.Click += new EventHandler(buttonPlay_Click);
			
			// Set Left
			mnuSetSelectionStart = new ToolStripMenuItem();
			mnuSetSelectionStart.Tag = new ItemResourceInfo(m_ResourceManager, "mnuSetSelectionStart");
			mnuSetSelectionStart.Text = ((ItemResourceInfo)mnuSetSelectionStart.Tag).resManager.GetString(((ItemResourceInfo)mnuSetSelectionStart.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetSelectionStart.Click += new EventHandler(btnSetHandlerLeft_Click);
			mnuSetSelectionStart.Visible = false;

			// Set Right
			mnuSetSelectionEnd = new ToolStripMenuItem();
			mnuSetSelectionEnd.Tag = new ItemResourceInfo(m_ResourceManager, "mnuSetSelectionEnd");
			mnuSetSelectionEnd.Text = ((ItemResourceInfo)mnuSetSelectionEnd.Tag).resManager.GetString(((ItemResourceInfo)mnuSetSelectionEnd.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetSelectionEnd.Click += new EventHandler(btnSetHandlerRight_Click);
			mnuSetSelectionEnd.Visible = false;
			
			// Lock
			mnuLockSelection = new ToolStripMenuItem();
			mnuLockSelection.Tag = new ItemResourceInfo(m_ResourceManager, "LockSelectionLock");
			mnuLockSelection.Text = ((ItemResourceInfo)mnuLockSelection.Tag).resManager.GetString(((ItemResourceInfo)mnuLockSelection.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuLockSelection.Click += new EventHandler(btn_HandlersLock_Click);
			mnuLockSelection.Visible = false;

			// Capture Speed (for high speed cameras)
			mnuSetCaptureSpeed = new ToolStripMenuItem();
			mnuSetCaptureSpeed.Tag = new ItemResourceInfo(m_ResourceManager, "mnuSetCaptureSpeed");
			mnuSetCaptureSpeed.Text = ((ItemResourceInfo)mnuSetCaptureSpeed.Tag).resManager.GetString(((ItemResourceInfo)mnuSetCaptureSpeed.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetCaptureSpeed.Click += new EventHandler(mnuSetCaptureSpeed_Click);

			// Enregistrer l'image
			mnuSavePic = new ToolStripMenuItem();
			mnuSavePic.Tag = new ItemResourceInfo(m_ResourceManager, "mnuSavePic");
			mnuSavePic.Text = ((ItemResourceInfo)mnuSavePic.Tag).resManager.GetString(((ItemResourceInfo)mnuSavePic.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSavePic.Click += new EventHandler(btnSnapShot_Click);

			// Fermer l'écran
			mnuCloseScreen = new ToolStripMenuItem();
			mnuCloseScreen.Tag = new ItemResourceInfo(m_ResourceManager, "mnuCloseScreen");
			mnuCloseScreen.Text = ((ItemResourceInfo)mnuCloseScreen.Tag).resManager.GetString(((ItemResourceInfo)mnuCloseScreen.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuCloseScreen.Click += new EventHandler(btnClose_Click);

			popMenu.Items.AddRange(new ToolStripItem[] { mnuPlayPause, /*mnuSep, mnuSetSelectionStart, mnuSetSelectionEnd, mnuLockSelection, mnuSep2,*/ mnuSetCaptureSpeed, mnuSavePic, mnuSep3, mnuCloseScreen });
			#endregion

			#region 2. Drawings pop menu (Configure, Delete, Track this)
			popMenuDrawings = new ContextMenuStrip();

			// Change color
			mnuConfigureDrawing = new ToolStripMenuItem();
			mnuConfigureDrawing.Tag = new ItemResourceInfo(m_ResourceManager, "mnuConfigureDrawing_ColorSize");
			mnuConfigureDrawing.Text = ((ItemResourceInfo)mnuConfigureDrawing.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureDrawing.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);

			// Change fading
			mnuConfigureFading = new ToolStripMenuItem();
			mnuConfigureFading.Tag = new ItemResourceInfo(m_ResourceManager, "mnuConfigureFading");
			mnuConfigureFading.Text = ((ItemResourceInfo)mnuConfigureFading.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureFading.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuConfigureFading.Click += new EventHandler(mnuConfigureFading_Click);

			// Track trajectory (cross specific)
			mnuTrackTrajectory = new ToolStripMenuItem();
			mnuTrackTrajectory.Tag = new ItemResourceInfo(m_ResourceManager, "mnuTrackTrajectory");
			mnuTrackTrajectory.Text = ((ItemResourceInfo)mnuTrackTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuTrackTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuTrackTrajectory.Click += new EventHandler(mnuTrackTrajectory_Click);

			// Goto Keyframe
			mnuGotoKeyframe = new ToolStripMenuItem();
			mnuGotoKeyframe.Tag = new ItemResourceInfo(m_ResourceManager, "mnuGotoKeyframe");
			mnuGotoKeyframe.Text = ((ItemResourceInfo)mnuGotoKeyframe.Tag).resManager.GetString(((ItemResourceInfo)mnuGotoKeyframe.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuGotoKeyframe.Click += new EventHandler(mnuGotoKeyframe_Click);

			// Delete drawing
			mnuDeleteDrawing = new ToolStripMenuItem();
			mnuDeleteDrawing.Tag = new ItemResourceInfo(m_ResourceManager, "mnuDeleteDrawing");
			mnuDeleteDrawing.Text = ((ItemResourceInfo)mnuDeleteDrawing.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteDrawing.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);

			// Show measure (Line specific)
			mnuShowMeasure = new ToolStripMenuItem();
			mnuShowMeasure.Tag = new ItemResourceInfo(m_ResourceManager, "mnuShowMeasure");
			mnuShowMeasure.Text = ((ItemResourceInfo)mnuShowMeasure.Tag).resManager.GetString(((ItemResourceInfo)mnuShowMeasure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuShowMeasure.Click += new EventHandler(mnuShowMeasure_Click);

			// Seal (establish) measure (Line specific)
			mnuSealMeasure = new ToolStripMenuItem();
			mnuSealMeasure.Tag = new ItemResourceInfo(m_ResourceManager, "mnuSealMeasure");
			mnuSealMeasure.Text = ((ItemResourceInfo)mnuSealMeasure.Tag).resManager.GetString(((ItemResourceInfo)mnuSealMeasure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSealMeasure.Click += new EventHandler(mnuSealMeasure_Click);

			mnuSepDrawing = new ToolStripSeparator();
			ToolStripSeparator mnuSepDrawing2 = new ToolStripSeparator();

			popMenuDrawings.Items.AddRange(new ToolStripItem[] { mnuConfigureDrawing, mnuConfigureFading, mnuSepDrawing2, mnuTrackTrajectory, mnuShowMeasure, mnuSealMeasure, mnuGotoKeyframe, mnuSepDrawing, mnuDeleteDrawing });
			#endregion

			#region 3. Tracking pop menu (Restart, Stop tracking)
			popMenuTrack = new ContextMenuStrip();

			// Stopper le Tracking
			mnuStopTracking = new ToolStripMenuItem();
			mnuStopTracking.Tag = new ItemResourceInfo(m_ResourceManager, "mnuStopTracking");
			mnuStopTracking.Text = ((ItemResourceInfo)mnuStopTracking.Tag).resManager.GetString(((ItemResourceInfo)mnuStopTracking.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuStopTracking.Click += new EventHandler(mnuStopTracking_Click);
			mnuStopTracking.Visible = false;

			// Reprendre le suivi
			mnuRestartTracking = new ToolStripMenuItem();
			mnuRestartTracking.Tag = new ItemResourceInfo(m_ResourceManager, "mnuRestartTracking");
			mnuRestartTracking.Text = ((ItemResourceInfo)mnuRestartTracking.Tag).resManager.GetString(((ItemResourceInfo)mnuRestartTracking.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuRestartTracking.Click += new EventHandler(mnuRestartTracking_Click);
			mnuRestartTracking.Visible = false;

			// Effacer la trajectoire
			mnuDeleteTrajectory = new ToolStripMenuItem();
			mnuDeleteTrajectory.Tag = new ItemResourceInfo(m_ResourceManager, "mnuDeleteTrajectory");
			mnuDeleteTrajectory.Text = ((ItemResourceInfo)mnuDeleteTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteTrajectory.Click += new EventHandler(mnuDeleteTrajectory_Click);

			// Effacer la fin de la trajectoire
			mnuDeleteEndOfTrajectory = new ToolStripMenuItem();
			mnuDeleteEndOfTrajectory.Tag = new ItemResourceInfo(m_ResourceManager, "mnuDeleteEndOfTrajectory");
			mnuDeleteEndOfTrajectory.Text = ((ItemResourceInfo)mnuDeleteEndOfTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteEndOfTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteEndOfTrajectory.Click += new EventHandler(mnuDeleteEndOfTrajectory_Click);

			// Configurer l'affichage de la trajectoire
			mnuConfigureTrajectory = new ToolStripMenuItem();
			mnuConfigureTrajectory.Tag = new ItemResourceInfo(m_ResourceManager, "Generic_Configuration");
			mnuConfigureTrajectory.Text = ((ItemResourceInfo)mnuConfigureTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuConfigureTrajectory.Click += new EventHandler(mnuConfigureTrajectory_Click);
			//mnuConfigureTrajectory.Visible = false;

			ToolStripSeparator mnuSepTraj = new ToolStripSeparator();
			ToolStripSeparator mnuSepTraj2 = new ToolStripSeparator();


			popMenuTrack.Items.AddRange(new ToolStripItem[] { mnuConfigureTrajectory, mnuSepTraj, mnuStopTracking, mnuRestartTracking, mnuSepTraj2, mnuDeleteEndOfTrajectory, mnuDeleteTrajectory });
			#endregion

			#region 4. Chrono pop menu (Start, Stop, Hide, etc.)
			popMenuChrono = new ContextMenuStrip();

			// Start
			mnuChronoStart = new ToolStripMenuItem();
			mnuChronoStart.Tag = new ItemResourceInfo(m_ResourceManager, "mnuChronoStart");
			mnuChronoStart.Text = ((ItemResourceInfo)mnuChronoStart.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoStart.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoStart.Click += new EventHandler(mnuChronoStart_Click);

			// Stop
			mnuChronoStop = new ToolStripMenuItem();
			mnuChronoStop.Tag = new ItemResourceInfo(m_ResourceManager, "mnuChronoStop");
			mnuChronoStop.Text = ((ItemResourceInfo)mnuChronoStop.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoStop.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoStop.Click += new EventHandler(mnuChronoStop_Click);

			// Sep
			ToolStripSeparator mnuSepChrono = new ToolStripSeparator();

			// Hide
			mnuChronoHide = new ToolStripMenuItem();
			mnuChronoHide.Tag = new ItemResourceInfo(m_ResourceManager, "mnuChronoHide");
			mnuChronoHide.Text = ((ItemResourceInfo)mnuChronoHide.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoHide.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoHide.Click += new EventHandler(mnuChronoHide_Click);

			// Countdown
			mnuChronoCountdown = new ToolStripMenuItem();
			mnuChronoCountdown.Tag = new ItemResourceInfo(m_ResourceManager, "mnuChronoCountdown");
			mnuChronoCountdown.Text = ((ItemResourceInfo)mnuChronoCountdown.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoCountdown.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoCountdown.Click += new EventHandler(mnuChronoCountdown_Click);
			mnuChronoCountdown.Checked = false;
			mnuChronoCountdown.Enabled = false;
			//mnuChronoCountdown.Visible = false;

			// Delete
			mnuChronoDelete = new ToolStripMenuItem();
			mnuChronoDelete.Tag = new ItemResourceInfo(m_ResourceManager, "mnuChronoDelete");
			mnuChronoDelete.Text = ((ItemResourceInfo)mnuChronoDelete.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoDelete.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoDelete.Click += new EventHandler(mnuChronoDelete_Click);

			ToolStripSeparator mnuSepChrono2 = new ToolStripSeparator();

			mnuChronoConfigure = new ToolStripMenuItem();
			mnuChronoConfigure.Tag = new ItemResourceInfo(m_ResourceManager, "Generic_Configuration");
			mnuChronoConfigure.Text = ((ItemResourceInfo)mnuChronoConfigure.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoConfigure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoConfigure.Click += new EventHandler(mnuChronoConfigure_Click);

			popMenuChrono.Items.AddRange(new ToolStripItem[] { mnuChronoConfigure, mnuSepChrono, mnuChronoStart, mnuChronoStop, mnuChronoCountdown, mnuSepChrono2, mnuChronoHide, mnuChronoDelete, });
			#endregion

			#region 5. Magnifier
			popMenuMagnifier = new ContextMenuStrip();

			// Zoom factors.
			mnuMagnifier150 = new ToolStripMenuItem();
			mnuMagnifier150.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifier150");
			mnuMagnifier150.Text = ((ItemResourceInfo)mnuMagnifier150.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier150.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier150.Click += new EventHandler(mnuMagnifier150_Click);

			mnuMagnifier175 = new ToolStripMenuItem();
			mnuMagnifier175.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifier175");
			mnuMagnifier175.Text = ((ItemResourceInfo)mnuMagnifier175.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier175.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier175.Click += new EventHandler(mnuMagnifier175_Click);
			mnuMagnifier175.Checked = true;

			mnuMagnifier200 = new ToolStripMenuItem();
			mnuMagnifier200.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifier200");
			mnuMagnifier200.Text = ((ItemResourceInfo)mnuMagnifier200.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier200.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier200.Click += new EventHandler(mnuMagnifier200_Click);

			mnuMagnifier225 = new ToolStripMenuItem();
			mnuMagnifier225.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifier225");
			mnuMagnifier225.Text = ((ItemResourceInfo)mnuMagnifier225.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier225.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier225.Click += new EventHandler(mnuMagnifier225_Click);

			mnuMagnifier250 = new ToolStripMenuItem();
			mnuMagnifier250.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifier250");
			mnuMagnifier250.Text = ((ItemResourceInfo)mnuMagnifier250.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier250.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier250.Click += new EventHandler(mnuMagnifier250_Click);

			// Sep
			ToolStripSeparator mnuSepMagnifier = new ToolStripSeparator();

			// Go to Direct Zoom Mode
			mnuMagnifierDirect = new ToolStripMenuItem();
			mnuMagnifierDirect.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifierDirect");
			mnuMagnifierDirect.Text = ((ItemResourceInfo)mnuMagnifierDirect.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifierDirect.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifierDirect.Click += new EventHandler(mnuMagnifierDirect_Click);

			// Close
			mnuMagnifierQuit = new ToolStripMenuItem();
			mnuMagnifierQuit.Tag = new ItemResourceInfo(m_ResourceManager, "mnuMagnifierQuit");
			mnuMagnifierQuit.Text = ((ItemResourceInfo)mnuMagnifierQuit.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifierQuit.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifierQuit.Click += new EventHandler(mnuMagnifierQuit_Click);

			popMenuMagnifier.Items.AddRange(new ToolStripItem[] { mnuMagnifier150, mnuMagnifier175, mnuMagnifier200, mnuMagnifier225, mnuMagnifier250, mnuSepMagnifier, mnuMagnifierDirect, mnuMagnifierQuit });
			
			#endregion

			#region 6. Grids
			popMenuGrids = new ContextMenuStrip();

			// Configure Grids
			mnuGridsConfigure = new ToolStripMenuItem();
			mnuGridsConfigure.Tag = new ItemResourceInfo(m_ResourceManager, "mnuConfigureDrawing_ColorSize");
			mnuGridsConfigure.Text = ((ItemResourceInfo)mnuGridsConfigure.Tag).resManager.GetString(((ItemResourceInfo)mnuGridsConfigure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuGridsConfigure.Click += new EventHandler(mnuGridsConfigure_Click);

			mnuGridsHide = new ToolStripMenuItem();
			mnuGridsHide.Tag = new ItemResourceInfo(m_ResourceManager, "mnuGridsHide");
			mnuGridsHide.Text = ((ItemResourceInfo)mnuGridsHide.Tag).resManager.GetString(((ItemResourceInfo)mnuGridsHide.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuGridsHide.Click += new EventHandler(mnuGridsHide_Click);

			popMenuGrids.Items.AddRange(new ToolStripItem[] { mnuGridsConfigure, mnuGridsHide });
			
			#endregion

			// Default :
			this.ContextMenuStrip = popMenu;
		}
		private void SetupDebugPanel(bool _bShowPanel)
		{
			if (_bShowPanel)
			{
				m_bShowInfos = true;
				panelDebug.Left = 0;
				panelDebug.Width = 180;
				panelDebug.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				panelDebug.BackColor = Color.Black;
			}
		}
		private void SetupKeyframesAndDrawingTools()
		{
			m_Metadata = new Metadata(new GetTimeCode(TimeStampsToTimecode), new ShowClosestFrame(OnShowClosestFrame));
			
			SetupKeyframeCommentsHub();
			m_bDocked = true;
			m_bTextEdit = false;

			m_DrawingTools = new AbstractDrawingTool[(int)DrawingToolType.NumberOfDrawingTools];
			m_DrawingTools[(int)DrawingToolType.Pointer] = new DrawingToolPointer();
			m_DrawingTools[(int)DrawingToolType.Line2D] = new DrawingToolLine2D();
			m_DrawingTools[(int)DrawingToolType.Cross2D] = new DrawingToolCross2D();
			m_DrawingTools[(int)DrawingToolType.Angle2D] = new DrawingToolAngle2D();
			m_DrawingTools[(int)DrawingToolType.Pencil] = new DrawingToolPencil();
			m_DrawingTools[(int)DrawingToolType.Text] = new DrawingToolText();
			m_DrawingTools[(int)DrawingToolType.Chrono] = new DrawingToolChrono();
			m_ActiveTool = DrawingToolType.Pointer;

			m_ColorProfile = new ColorProfile();
			m_ColorProfile.Load(PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("ColorProfilesFolder") + "\\current.xml");

			m_Magnifier = new Magnifier();
		}
		#endregion

		#region Closing / Unload
		public void UnloadMovie()
		{
			//Only called when destroying the whole screen.

			m_KeyframeCommentsHub.Hide();
			m_PlayerServer.UnloadMovie();
			
			m_Metadata.Plane.Visible = false;
			m_Metadata.Grid.Visible = false;
			
			m_bDrawtimeFiltered = false;
		}
		#endregion

		#region Debug
		public void UpdateDebugInfos()
		{
			panelDebug.Visible = true;

			dbgDurationTimeStamps.Text = String.Format("TotalDuration (ts): {0:0}", m_iTotalDuration);
			dbgFFps.Text = String.Format("Fps Avg (f): {0:0.00}", m_PlayerServer.m_InfosVideo.fFps);
			dbgSelectionStart.Text = String.Format("SelStart (ts): {0:0}", m_iSelStart);
			dbgSelectionEnd.Text = String.Format("SelEnd (ts): {0:0}", m_iSelEnd);
			dbgSelectionDuration.Text = String.Format("SelDuration (ts): {0:0}", m_iSelDuration);
			dbgCurrentPositionAbs.Text = String.Format("CurrentPosition (abs, ts): {0:0}", m_iCurrentPosition);
			dbgCurrentPositionRel.Text = String.Format("CurrentPosition (rel, ts): {0:0}", m_iCurrentPosition-m_iSelStart);
			dbgStartOffset.Text = String.Format("StartOffset (ts): {0:0}", m_PlayerServer.m_InfosVideo.iFirstTimeStamp);
			dbgDrops.Text = String.Format("Drops (f): {0:0}", m_iDroppedFrames);

			dbgCurrentFrame.Text = String.Format("CurrentFrame (f): {0}", m_PlayerServer.m_PrimarySelection.iCurrentFrame);
			dbgDurationFrames.Text = String.Format("Duration (f) : {0}", m_PlayerServer.m_PrimarySelection.iDurationFrame);

			//dbgAvailableRam.Text = String.Format("Avail. RAM (Mb) : {0}", m_RamCounter.NextValue());

			panelDebug.Invalidate();
		}
		private void panelDebug_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			UpdateDebugInfos();
			log.Debug("");
			log.Debug("Timestamps Full Dump");
			log.Debug("--------------------");
			double fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
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
		
		#region Conversions  / Rescalings
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

			if (m_PlayerServer.m_bIsMovieLoaded)
				iSeconds = (double)iTimeStamp / m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds;
			else
				iSeconds = 0;

			// m_fSlowFactor is different from 1.0f only when user specify that the capture fps
			// was different than the playing fps. We readjust time.
			double iMilliseconds = (iSeconds * 1000) / m_fSlowFactor;
			
			// If there are more than 100 frames per seconds, we display milliseconds.
			// This can happen when the user manually tune the input fps.
			bool bShowThousandth = (m_fSlowFactor *  m_PlayerServer.m_InfosVideo.fFps >= 100);
			
			string outputTimeCode;
			switch (tcf)
			{
				case TimeCodeFormat.ClassicTime:
					outputTimeCode = TimeHelper.MillisecondsToTimecode((long)iMilliseconds, bShowThousandth);
					break;
				case TimeCodeFormat.Frames:
					if (m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame != 0)
					{
						outputTimeCode = String.Format("{0}", (int)((double)iTimeStamp / m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame) + 1);
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
					if (m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame != 0)
					{
						frameString = String.Format("{0}", (int)((double)iTimeStamp / m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame) + 1);
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
			// Rescaling génériquel.
			return (int)((double)((double)_iValue * (double)_iNewMax) / (double)_iOldMax);
		}
		private Point DescaleCoordinates(Point _point)
		{
			// Takes in screen coordinates and gives back image coordinates.
			// Image has been stretched, zoomed and moved.

			// 1. Unstretch coords -> As if stretch factor was 1.0f.
			double fUnstretchedX = (double)_point.X / m_fStretchFactor;
			double fUnstretchedY = (double)_point.Y / m_fStretchFactor;

			// 2. Unzoom coords -> As if zoom factor was 1.0f.
			// Unmoved is m_DirectZoomWindow.Left * m_fDirectZoomFactor.
			// Unzoomed is Unmoved / m_fDirectZoomFactor.
			double fUnzoomedX = (double)m_DirectZoomWindow.Left + (fUnstretchedX / m_fDirectZoomFactor);
			double fUnzoomedY = (double)m_DirectZoomWindow.Top + (fUnstretchedY / m_fDirectZoomFactor);

			return new Point((int)fUnzoomedX, (int)fUnzoomedY);
		}
		#endregion

		#region Video Controls

		#region Playback Controls
		public void buttonGotoFirst_Click(object sender, EventArgs e)
		{
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
				StopPlaying();
				
				m_iFramesToDecode = 1;
				ShowNextFrame(m_iSelStart, true);
				
				UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition);

				//if (m_ReportReady != null) { m_ReportReady(true); }
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		public void buttonGotoPrevious_Click(object sender, EventArgs e)
		{
			// Faire un seek de l'équivalent d'une frame en arrière ?
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
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

					//double  fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
				OnButtonPlay();
			}
		}
		public void buttonGotoNext_Click(object sender, EventArgs e)
		{
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				if (m_bIsCurrentlyPlaying)
				{
					// Go into Pause mode.
					StopPlaying();
					buttonPlay.BackgroundImage = Videa.ScreenManager.Properties.Resources.liqplay17;
					m_bIsCurrentlyPlaying = false;
					ActivateKeyframe(m_iCurrentPosition);
				}
				else
				{
					// Go into Play mode
					buttonPlay.BackgroundImage = Videa.ScreenManager.Properties.Resources.liqpause6;
					Application.Idle += new EventHandler(this.IdleDetector);
					StartMultimediaTimer(GetFrameInterval());
					m_bIsCurrentlyPlaying = true;
				}
			}
		}
		public void Common_MouseWheel(object sender, MouseEventArgs e)
		{
			// MouseWheel was recorded on one of the controls.
			int iScrollOffset = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

			/*if(m_Mosaic.Enabled)
			{
				// We get two events, one for the picbox, the other for the panel.
				// We'll ignore one of them.
				
				if(!(sender is Panel) && m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1 && !m_Mosaic.KeyImagesOnly)
				{
					// Get the next square spot and load it.
					int iCurrentSpot = (int)Math.Sqrt(m_Mosaic.LastImagesCount);
					
					if (iScrollOffset > 0)
					{
						if(iCurrentSpot < 10)
						{
							int iFramesToExtract = (iCurrentSpot+1) * (iCurrentSpot+1);
							m_Mosaic.Load(m_PlayerServer.ExtractForMosaic(iFramesToExtract));
						}
					}
					else
					{
						if(iCurrentSpot > 2)
						{
							int iFramesToExtract = (iCurrentSpot-1) * (iCurrentSpot-1);
							m_Mosaic.Load(m_PlayerServer.ExtractForMosaic(iFramesToExtract));
						}
					}
					
					m_SurfaceScreen.Invalidate();
				}
			}
			else*/ if ((ModifierKeys & Keys.Control) == Keys.Control)
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				//SetAsActiveScreen();
				StopPlaying();

				// Mettre à jour les données et leur affichage, l'image n'est pas rafraichie ici.
				
				// Modifs des timestamps
				UpdatePrimarySelectionPanelData();
				UpdatePrimarySelectionPanelInfos();

				// Remapper le FrameTracker.
				trkFrame.Minimum = m_iSelStart;
				trkFrame.Maximum = m_iSelEnd;
				
				// Ne pas mettre à jour le curseur de navigation ici.
				// C'est le FrameTracker qui s'en est chargé quand on a modifié les bornes.
				trkSelection.SelPos = trkFrame.Position;

				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		private void trkSelection_SelectionChanged(object sender, EventArgs e)
		{
			// Mise à jour effective.
			if (m_PlayerServer.m_bIsMovieLoaded && !m_bResetingHandlers)
			{
				SwitchToAnalysisMode(false);
				m_Metadata.SelectionStart = m_iSelStart;
				UpdateKeyframesMarkers();

				SetAsActiveScreen();
				if (m_ReportSelectionChanged != null) { m_ReportSelectionChanged(false); }

				// Mise à jour de l'image affichée si besoin.
				UpdateFramePrimarySelection();
				
				EnableDisableKeyframes();
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void trkSelection_TargetAcquired(object sender, EventArgs e)
		{
			//--------------------------------------------------------------
			// Clic dans la sélection : déplacement.
			// Mets à jour la position courante dans la selection primaire
			//--------------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
				StopPlaying();

				trkSelection.SelPos = trkSelection.SelTarget + trkSelection.Minimum;
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
			//-----------------------------------------
			// Vérouiller les poignées de la selection.
			//-----------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				m_bHandlersLocked = !m_bHandlersLocked;

				if (m_bHandlersLocked)
				{
					btn_HandlersLock.Image = Videa.ScreenManager.Properties.Resources.primselec_locked3;
					toolTips.SetToolTip(btn_HandlersLock, m_ResourceManager.GetString("LockSelectionUnlock", Thread.CurrentThread.CurrentUICulture));
					
					// Chaînes du menu contextuel
					mnuLockSelection.Tag = new ItemResourceInfo(m_ResourceManager, "LockSelectionUnlock");
					mnuLockSelection.Text = ((ItemResourceInfo)mnuLockSelection.Tag).resManager.GetString(((ItemResourceInfo)mnuLockSelection.Tag).strText, Thread.CurrentThread.CurrentUICulture);
					
					trkSelection.SelLocked = true;
				}
				else
				{
					btn_HandlersLock.Image = Videa.ScreenManager.Properties.Resources.primselec_unlocked3;
					toolTips.SetToolTip(btn_HandlersLock, m_ResourceManager.GetString("LockSelectionLock", Thread.CurrentThread.CurrentUICulture));
					
					// Chaînes du menu contextuel
					mnuLockSelection.Tag = new ItemResourceInfo(m_ResourceManager, "LockSelectionLock");
					mnuLockSelection.Text = ((ItemResourceInfo)mnuLockSelection.Tag).resManager.GetString(((ItemResourceInfo)mnuLockSelection.Tag).strText, Thread.CurrentThread.CurrentUICulture);
					
					trkSelection.SelLocked = false;
				}
			}
		}
		private void btnSetHandlerLeft_Click(object sender, EventArgs e)
		{
			//------------------------------------------------------
			// Positionner le handler de gauche à la frame courante.
			//------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded && !m_bHandlersLocked)
			{
				//m_iSelStart = m_iCurrentPosition;
				trkSelection.SelStart = m_iCurrentPosition;
				
				//trkSelection_SelectionChanging(null, EventArgs.Empty);
				//trkSelection_SelectionChanged(null, EventArgs.Empty);
			}
		}
		private void btnSetHandlerRight_Click(object sender, EventArgs e)
		{
			//------------------------------------------------------
			// Positionner le handler de droite à la frame courante.
			//------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded && !m_bHandlersLocked)
			{
				//m_iSelEnd = m_iCurrentPosition;
				trkSelection.SelEnd = m_iCurrentPosition;
				
				//trkSelection_SelectionChanging(null, EventArgs.Empty);
				//trkSelection_SelectionChanged(null, EventArgs.Empty);
			}
		}
		private void btnHandlersReset_Click(object sender, EventArgs e)
		{
			//------------------------------------------------------
			// Replacer les Handlers  au maximums
			//------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded && !m_bHandlersLocked)
			{
				m_bResetingHandlers = true;
				trkSelection.SelStart = trkSelection.Minimum;
				trkSelection.SelEnd = trkSelection.Maximum;
				m_bResetingHandlers = false;
				
				// We need to force the reloading of all frames.
				SwitchToAnalysisMode(true);
				
				// Update everything as if we moved the handlers manually.
				m_Metadata.SelectionStart = m_iSelStart;
				UpdateKeyframesMarkers();
				SetAsActiveScreen();
				if (m_ReportSelectionChanged != null) { m_ReportSelectionChanged(false); }

				// Mise à jour de l'image affichée si besoin.
				UpdateFramePrimarySelection();
				
				EnableDisableKeyframes();
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void UpdateFramePrimarySelection()
		{
			//--------------------------------------------------------------
			// Mets à jour l'image pour refléter la nouvelle selection.
			// vérifie que la frame en cours est toujours dans la selection.
			// sinon affiche la borne la plus proche.
			//--------------------------------------------------------------
			
			if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1)
			{
				// En mode analyse on est toujours dans la zone, mais
				// On peut avoir besoin de faire un refresh de l'image.
				ShowNextFrame(m_PlayerServer.m_PrimarySelection.iCurrentFrame, true);
			}
			else
			{
				if ((m_iCurrentPosition >= m_iSelStart) && (m_iCurrentPosition <= m_iSelEnd))
				{
					// Ne rien faire.
				}
				else
				{
					m_iFramesToDecode = 1;

					// Si la frame affichée à l'écran n'est plus dans l'intervalle, forcer un refresh.
					if (m_iCurrentPosition < m_iSelStart)
					{
						// Problème, la frame affichée risque être quand même hors zone.
						// On va tomber sur la dernière I-Frame avant la SelStart...
						ShowNextFrame(m_iSelStart, true);
					}
					else
					{
						// Supérieure à la EndFrame : On se replace sur la EndFrame.
						ShowNextFrame(m_iSelEnd, true);
					}
				}
			}

			//Mettre à jour le curseur.
			UpdateNavigationCursor();

			if (m_bShowInfos) UpdateDebugInfos();
		}
		private void UpdatePrimarySelectionPanelInfos()
		{
			//-----------------------------------------------------------------
			// Format d'affichage : Standard TimeCode.
			// Heures:Minutes:Secondes.Frames
			//-----------------------------------------------------------------
			if (m_ResourceManager != null)
			{
				lblSelStartSelection.Text = m_ResourceManager.GetString("lblSelStartSelection_Text", Thread.CurrentThread.CurrentUICulture) + " : " + TimeStampsToTimecode(m_iSelStart - m_iStartingPosition, m_PrefManager.TimeCodeFormat, false);
				lblSelDuration.Text = m_ResourceManager.GetString("lblSelDuration_Text", Thread.CurrentThread.CurrentUICulture) + " : " + TimeStampsToTimecode(m_iSelDuration, m_PrefManager.TimeCodeFormat, false);
			}
		}
		private void UpdatePrimarySelectionPanelSelection()
		{
			//------------------------------------------------
			// Redessine la selection en fonction des données.
			// (Au chargement ou sur Resize)
			//------------------------------------------------
			/*
            PrimarySelection.Left = TimeStampsToPixels(m_iSelStart, m_iTotalDuration, panelSelection.Width);
            PrimarySelection.Width = TimeStampsToPixels(m_iSelDuration, m_iTotalDuration, panelSelection.Width);

            HandlerLeft.Left = PrimarySelection.Left - (HandlerLeft.Width / 2); // Par exemple -8 au début.
            HandlerRight.Left = PrimarySelection.Left + PrimarySelection.Width - (HandlerRight.Width / 2);

            // Déplacer les labels d'affichage
            UpdateSelectionInfosStart();
            UpdateSelectionInfosEnd();*/
		}
		private void UpdatePrimarySelectionPanelData()
		{
			//-------------------------------------------------------------
			// Update les données en fonction de la selection
			// (Après modif par l'utilisateur)
			//--------------------------------------------------------------
			if ((m_iSelStart != trkSelection.SelStart) || (m_iSelEnd != trkSelection.SelEnd))
			{
				m_iSelStart = trkSelection.SelStart;
				m_iSelEnd = trkSelection.SelEnd;
				double fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
				m_iSelDuration = m_iSelEnd - m_iSelStart + (long)fAverageTimeStampsPerFrame;
			}
		}
		#endregion
		
		#region Frame Tracker
		public void SetCurrentFrame(Int64 _iFrame)
		{
			// Called during static sync.
			// Common position changed, we get a new frame to jump to.
			// target frame may be over the total.

			if (m_PlayerServer.m_bIsMovieLoaded)
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
					m_iCurrentPosition = _iFrame * m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame;
					m_iCurrentPosition += m_PlayerServer.m_InfosVideo.iFirstTimeStamp;

					if (m_iCurrentPosition > m_iSelEnd)
					{
						m_iCurrentPosition = m_iSelEnd;
					}
					if (m_iCurrentPosition < m_iSelStart)
					{
						m_iCurrentPosition = m_iSelStart;
					}

					ShowNextFrame(m_iCurrentPosition, true);
				}

				UpdateNavigationCursor();
				UpdateCurrentPositionInfos();
				ActivateKeyframe(m_iCurrentPosition);

				trkSelection.SelPos = trkFrame.Position;

				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		private void trkFrame_PositionChanging(object sender, long _iPosition)
		{
			//---------------------------------------------------
			// Appelée lors de déplacement de type MouseMove
			// UNIQUEMENT si mode Analyse.
			//---------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				//SetAsActiveScreen();
				//StopPlaying();

				// Mettre à jour l'image, mais ne pas toucher au curseur.
				UpdateFrameCurrentPosition(false);
				UpdateCurrentPositionInfos();

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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();
				StopPlaying();

				// Mettre à jour l'image, ET le CURSEUR.
				UpdateFrameCurrentPosition(true);
				UpdateCurrentPositionInfos();
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

			if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 0)
			{
				this.Cursor = Cursors.WaitCursor;
			}

			m_iCurrentPosition = trkFrame.Position;
			m_iFramesToDecode = 1;
			ShowNextFrame(m_iCurrentPosition, true);

			if (_bUpdateNavCursor) { UpdateNavigationCursor();}
			if (m_bShowInfos) { UpdateDebugInfos(); }

			if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 0)
			{
				this.Cursor = Cursors.Default;
			}
			
		}
		private void UpdateCurrentPositionInfos()
		{
			//-----------------------------------------------------------------
			// Format d'affichage : Standard TimeCode.
			// Heures:Minutes:Secondes.Frames
			// Position relative à la Selection Primaire / Zone de travail
			//-----------------------------------------------------------------
			if (m_ResourceManager != null)
			{
				string timecode = "";

				timecode = TimeStampsToTimecode(m_iCurrentPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, m_bSynched);

				lblTimeCode.Text = m_ResourceManager.GetString("lblTimeCode_Text", Thread.CurrentThread.CurrentUICulture) + " : " + timecode;
				lblTimeCode.Invalidate();
			}
		}
		private void UpdateNavigationCursor()
		{
			//---------------------------------------------------------------------------
			// Met à jour la position du curseur.
			// sur chargement, Resize, ShowNextFrame, ou Changement de selection primaire.
			//----------------------------------------------------------------------------
			trkFrame.Position = m_iCurrentPosition;
			trkSelection.SelPos = trkFrame.Position;
			UpdateCurrentPositionInfos();
		}
		private void UpdateKeyframesMarkers()
		{
			long[] ts = new long[m_Metadata.Keyframes.Count];
			for (int i = 0; i < ts.Length; i++)
			{
				// Selection Start will be taken care of directly in the FrameTracker.
				ts[i] = m_Metadata.Keyframes[i].Position;
			}
			trkFrame.KeyframesTimestamps = ts;
			trkFrame.Invalidate();
		}
		#endregion

		#region Speed Slider
		private void sldrSpeed_ValueChanged(object sender, EventArgs e)
		{
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				//---------------------------------------------------------------------------
				// Désactivation de SetAsActiveScreen : update des menus prend des ressources
				// qui interfère avec l'affichage du slider...
				//---------------------------------------------------------------------------
				//SetAsActiveScreen();

				m_iSlowmotionPercentage = sldrSpeed.Value;

				// Relancer le timer avec la nouvelle valeur.
				if (m_bIsCurrentlyPlaying)
				{
					int iFrameInterval = GetFrameInterval();
					StopMultimediaTimer();
					StartMultimediaTimer(iFrameInterval);
				}

				// Impact sur la synchro.
				if (m_ReportReady != null) { m_ReportReady(true); }
			}

			//  Affichage de la valeur
			if (m_ResourceManager != null)
			{
				lblSpeedTuner.Text = m_ResourceManager.GetString("lblSpeedTuner_Text", Thread.CurrentThread.CurrentUICulture) + " " + sldrSpeed.Value + "%";
			}
		}
		public void sldrSpeed_KeyDown(object sender, KeyEventArgs e)
		{
			//--------------------------------------------------------
			// Diminuer ou augmenter la vitesse à l'appui des touches
			// HAUT / BAS du clavier.
			// Fonction publique car appellée depuis le ScreenManager.
			//--------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				// Passe le Handled à true pour éviter le traitement par windows.
				if (e.KeyCode == Keys.Down)
				{
					// If Control, jump to the next 25% spot.
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
					// If Control, jump to the next 25% spot.
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
		#endregion

		#region Loop Mode
		private void buttonPlayingMode_Click(object sender, EventArgs e)
		{
			//----------------------------------------
			// Mode de lecture (Une fois ou en boucle)
			//----------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				SetAsActiveScreen();

				if (m_ePlayingMode == PlayingMode.Once)
				{
					// On était en mode Once, on passe en mode Loop.
					m_ePlayingMode = PlayingMode.Loop;
					buttonPlayingMode.Image = Videa.ScreenManager.Properties.Resources.playmulti3;
					toolTips.SetToolTip(buttonPlayingMode, m_ResourceManager.GetString("ToolTip_PlayingMode_Loop", Thread.CurrentThread.CurrentUICulture));
				}
				else if (m_ePlayingMode == PlayingMode.Loop)
				{
					// On était en mode Loop, on passe en mode Once.
					m_ePlayingMode = PlayingMode.Once;
					buttonPlayingMode.Image = Videa.ScreenManager.Properties.Resources.playonce4;
					toolTips.SetToolTip(buttonPlayingMode, m_ResourceManager.GetString("ToolTip_PlayingMode_Once", Thread.CurrentThread.CurrentUICulture));
				}
				
				// TODO - Bounce mode ?
			}
		}
		#endregion

		#endregion

		#region Image Border
		private void ShowBorder()
		{
			m_bShowImageBorder = true;
			_surfaceScreen.Invalidate();
		}
		private void HideBorder()
		{
			m_bShowImageBorder = false;
			_surfaceScreen.Invalidate();
		}
		private void DrawImageBorder(Graphics _canvas)
		{
			// Draw the border around the screen to mark it as selected.
			// Called back from main drawing routine.
			_canvas.DrawRectangle(m_PenImageBorder, 0, 0, _surfaceScreen.Width - m_PenImageBorder.Width, _surfaceScreen.Height - m_PenImageBorder.Width);
			// Order : top, left, bottom, right.
			/*_canvas.DrawLine(m_PenImageBorder, 0, 0, _surfaceScreen.Width, 0);
			_canvas.DrawLine(m_PenImageBorder, 0, 0, 0, _surfaceScreen.Height);
			_canvas.DrawLine(m_PenImageBorder, 0, _surfaceScreen.Height - m_PenImageBorder.Width, _surfaceScreen.Width, _surfaceScreen.Height - m_PenImageBorder.Width);
			_canvas.DrawLine(m_PenImageBorder, _surfaceScreen.Width - m_PenImageBorder.Width, 0, _surfaceScreen.Width - m_PenImageBorder.Width, _surfaceScreen.Height);*/
		}
		#endregion

		#region General Handlers: Resize, Close, ActiveScreen
		private void SetAsActiveScreen()
		{
			//---------------------------------------------------------------------
			// Se signale au ScreenManager
			// Utilise un tunnel de delegates pour remonter jusqu'au ScreenManager.
			// We will use this function as a hub for all button press.
			//---------------------------------------------------------------------
			if (m_SetMeAsActiveScreenUI != null) { m_SetMeAsActiveScreenUI(); }
			
			// 1. Ensure no DrawingText is in edit mode.
			m_Metadata.AllDrawingTextToNormalMode();

			// 2. Return to the pointer tool, except if Pencil
			if (m_ActiveTool != DrawingToolType.Pencil)
			{
				m_ActiveTool = DrawingToolType.Pointer;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, -1));
			}

			// 3. Dock Keyf panel if nothing to see.
			if (m_Metadata.Count < 1)
			{
				DockKeyframePanel();
			}
		}
		public void DisplayAsActiveScreen()
		{
			// Actually called from ScreenManager.
			ShowBorder();
		}
		public void DisplayAsInactiveScreen()
		{
			// Actually called from ScreenManager.
			HideBorder();
		}
		public void OnUndrawn()
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
		private void btnClose_Click(object sender, EventArgs e)
		{
			// Propagate to PlayerScreen which will report to ScreenManager.
			if (m_CloseMeUI != null) { m_CloseMeUI(); }
		}
		#endregion

		#region Auto Stretch & Manual Resize
		private void StretchSqueezeSurface()
		{
			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded)
				{
					// Check if the image was loaded squeezed.
					// (happen when screen control isn't being fully expanded at video load time.)
					if(m_SurfaceScreen.Height < _panelCenter.Height && m_fStretchFactor < 1.0)
					{
						m_fStretchFactor = 1.0;
					}
					
					//---------------------------------------------------------------
					// Check if the stretch factor is not going to outsize the panel.
					// If so, force maximized, unless screen is smaller than video.
					//---------------------------------------------------------------
					int iTargetHeight = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingHeight * m_fStretchFactor);
					int iTargetWidth = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingWidth * m_fStretchFactor);
					
					if (iTargetHeight > _panelCenter.Height || iTargetWidth > _panelCenter.Width)
					{
						if (m_fStretchFactor > 1.0)
						{
							m_bStretchModeOn = true;
						}
					}
					
					if ((m_bStretchModeOn) || (m_PlayerServer.m_InfosVideo.iDecodingWidth > _panelCenter.Width) || (m_PlayerServer.m_InfosVideo.iDecodingHeight > _panelCenter.Height))
					{
						//-------------------------------------------------------------------------------
						// Maximiser :
						//Redimensionner l'image selon la dimension la plus proche de la taille du panel.
						//-------------------------------------------------------------------------------
						float WidthRatio = (float)m_PlayerServer.m_InfosVideo.iDecodingWidth / _panelCenter.Width;
						float HeightRatio = (float)m_PlayerServer.m_InfosVideo.iDecodingHeight / _panelCenter.Height;
						
						if (WidthRatio > HeightRatio)
						{
							m_SurfaceScreen.Width = _panelCenter.Width;
							m_SurfaceScreen.Height = (int)((float)m_PlayerServer.m_InfosVideo.iDecodingHeight / WidthRatio);
							
							m_fStretchFactor = (1 / WidthRatio);
						}
						else
						{
							m_SurfaceScreen.Width = (int)((float)m_PlayerServer.m_InfosVideo.iDecodingWidth / HeightRatio);
							m_SurfaceScreen.Height = _panelCenter.Height;
							
							m_fStretchFactor = (1 / HeightRatio);
						}
					}
					else
					{
						
						m_SurfaceScreen.Width = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingWidth * m_fStretchFactor);
						m_SurfaceScreen.Height = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingHeight * m_fStretchFactor);
					}
					
					//recentrer
					m_SurfaceScreen.Left = (_panelCenter.Width / 2) - (m_SurfaceScreen.Width / 2);
					m_SurfaceScreen.Top = (_panelCenter.Height / 2) - (m_SurfaceScreen.Height / 2);
					
					// Repositionement des Resizers.
					ReplaceResizers();
					
					// Redéfinir les plans & grilles 3D
					Size imageSize = new Size(m_PlayerServer.m_InfosVideo.iDecodingWidth, m_PlayerServer.m_InfosVideo.iDecodingHeight);
					m_Metadata.Plane.SetLocations(imageSize, m_fStretchFactor, m_DirectZoomWindow.Location);
					m_Metadata.Grid.SetLocations(imageSize, m_fStretchFactor, m_DirectZoomWindow.Location);
				}
			}
		}
		private void ReplaceResizers()
		{
			ImageResizerSE.Left = m_SurfaceScreen.Left + m_SurfaceScreen.Width - (ImageResizerSE.Width / 2);
			ImageResizerSE.Top = m_SurfaceScreen.Top + m_SurfaceScreen.Height - (ImageResizerSE.Height / 2);

			ImageResizerSW.Left = m_SurfaceScreen.Left - (ImageResizerSW.Width / 2);
			ImageResizerSW.Top = m_SurfaceScreen.Top + m_SurfaceScreen.Height - (ImageResizerSW.Height / 2);

			ImageResizerNE.Left = m_SurfaceScreen.Left + m_SurfaceScreen.Width - (ImageResizerNE.Width / 2);
			ImageResizerNE.Top = m_SurfaceScreen.Top - (ImageResizerNE.Height / 2);

			ImageResizerNW.Left = m_SurfaceScreen.Left - (ImageResizerNW.Width / 2);
			ImageResizerNW.Top = m_SurfaceScreen.Top - (ImageResizerNW.Height / 2);
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
				if (m_fStretchFactor >= 1)
				{
					m_fStretchFactor = 1;
					m_bStretchModeOn = false;
				}
			}
			StretchSqueezeSurface();
			m_SurfaceScreen.Invalidate();
		}
		private void ImageResizerSE_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight, iTargetWidth;
				double fHeightFactor, fWidthFactor;

				iTargetHeight = (ImageResizerSE.Top - m_SurfaceScreen.Top + e.Y);
				iTargetWidth = (ImageResizerSE.Left - m_SurfaceScreen.Left + e.X);

				//-------------------------------------------------------------------
				// On resize à condition que l'image soit:
				// Supérieure à la taille originale, inférieure à la taille du panel.
				//-------------------------------------------------------------------
				if (iTargetHeight > m_PlayerServer.m_InfosVideo.iDecodingHeight &&
				    iTargetHeight < _panelCenter.Height &&
				    iTargetWidth > m_PlayerServer.m_InfosVideo.iDecodingWidth &&
				    iTargetWidth < _panelCenter.Width)
				{
					fHeightFactor = ((iTargetHeight) / (double)m_PlayerServer.m_InfosVideo.iDecodingHeight);
					fWidthFactor = ((iTargetWidth) / (double)m_PlayerServer.m_InfosVideo.iDecodingWidth);

					m_fStretchFactor = (fWidthFactor + fHeightFactor) / 2;
					m_bStretchModeOn = false;
					StretchSqueezeSurface();
					//rafraîchir
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void ImageResizerSW_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight, iTargetWidth;
				double fHeightFactor, fWidthFactor;

				iTargetHeight = (ImageResizerSW.Top - m_SurfaceScreen.Top + e.Y);
				iTargetWidth = m_SurfaceScreen.Width + (m_SurfaceScreen.Left - (ImageResizerSW.Left + e.X));

				//-------------------------------------------------------------------
				// On resize à condition que l'image soit:
				// Supérieure à la taille originale, inférieure à la taille du panel.
				//-------------------------------------------------------------------
				if (iTargetHeight > m_PlayerServer.m_InfosVideo.iDecodingHeight &&
				    iTargetHeight < _panelCenter.Height &&
				    iTargetWidth > m_PlayerServer.m_InfosVideo.iDecodingWidth &&
				    iTargetWidth < _panelCenter.Width)
				{
					fHeightFactor = ((iTargetHeight) / (double)m_PlayerServer.m_InfosVideo.iDecodingHeight);
					fWidthFactor = ((iTargetWidth) / (double)m_PlayerServer.m_InfosVideo.iDecodingWidth);

					m_fStretchFactor = (fWidthFactor + fHeightFactor) / 2;
					m_bStretchModeOn = false;
					StretchSqueezeSurface();
					//rafraîchir
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void ImageResizerNW_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight, iTargetWidth;
				double fHeightFactor, fWidthFactor;

				iTargetHeight = m_SurfaceScreen.Height + (m_SurfaceScreen.Top - (ImageResizerNW.Top + e.Y));
				iTargetWidth = m_SurfaceScreen.Width + (m_SurfaceScreen.Left - (ImageResizerNW.Left + e.X));

				//-------------------------------------------------------------------
				// On resize à condition que l'image soit:
				// Supérieure à la taille originale, inférieure à la taille du panel.
				//-------------------------------------------------------------------
				if (iTargetHeight > m_PlayerServer.m_InfosVideo.iDecodingHeight &&
				    iTargetHeight < _panelCenter.Height &&
				    iTargetWidth > m_PlayerServer.m_InfosVideo.iDecodingWidth &&
				    iTargetWidth < _panelCenter.Width)
				{
					fHeightFactor = ((iTargetHeight) / (double)m_PlayerServer.m_InfosVideo.iDecodingHeight);
					fWidthFactor = ((iTargetWidth) / (double)m_PlayerServer.m_InfosVideo.iDecodingWidth);

					m_fStretchFactor = (fWidthFactor + fHeightFactor) / 2;
					m_bStretchModeOn = false;
					StretchSqueezeSurface();
					//rafraîchir
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void ImageResizerNE_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int iTargetHeight, iTargetWidth;
				double fHeightFactor, fWidthFactor;

				iTargetHeight = m_SurfaceScreen.Height + (m_SurfaceScreen.Top - (ImageResizerNE.Top + e.Y));
				iTargetWidth = (ImageResizerNE.Left - m_SurfaceScreen.Left + e.X);

				//-------------------------------------------------------------------
				// On resize à condition que l'image soit:
				// Supérieure à la taille originale, inférieure à la taille du panel.
				//-------------------------------------------------------------------
				if (iTargetHeight > m_PlayerServer.m_InfosVideo.iDecodingHeight &&
				    iTargetHeight < _panelCenter.Height &&
				    iTargetWidth > m_PlayerServer.m_InfosVideo.iDecodingWidth &&
				    iTargetWidth < _panelCenter.Width)
				{
					fHeightFactor = ((iTargetHeight) / (double)m_PlayerServer.m_InfosVideo.iDecodingHeight);
					fWidthFactor = ((iTargetWidth) / (double)m_PlayerServer.m_InfosVideo.iDecodingWidth);

					m_fStretchFactor = (fWidthFactor + fHeightFactor) / 2;
					m_bStretchModeOn = false;
					StretchSqueezeSurface();
					//rafraîchir
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void ImageResizerNW_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Resizers_MouseDoubleClick();
		}
		private void ImageResizerNE_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Resizers_MouseDoubleClick();
		}
		private void ImageResizerSE_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Resizers_MouseDoubleClick();
		}
		private void ImageResizerSW_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Resizers_MouseDoubleClick();
		}
		private void Resizers_MouseDoubleClick()
		{
			// Maximiser l'écran ou repasser à la taille originale.
			if (!m_bStretchModeOn)
			{
				m_bStretchModeOn = true;
			}
			else
			{
				m_fStretchFactor = 1;
				m_bStretchModeOn = false;
			}
			StretchSqueezeSurface();
			m_SurfaceScreen.Invalidate();
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				//if (!m_bIsInPlayLoop)
				//{
				//m_bIsInPlayLoop = true;
				//Console.WriteLine("                             Total (tick):{0}", m_NotIdleWatch.ElapsedMilliseconds);
				BeginInvoke(m_CallbackPlayLoop);
				//}
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
			//
			// D'autre part il faut empécher la ReEntry dans cette fonction. (m_bIsInPlayLoop)
			//-----------------------------------------------------------------------------

			bool bStopAtEnd = false;

			
			//if (!m_bIsInPlayLoop)
			{
				//Console.WriteLine("                             Total (playloop):{0}", m_NotIdleWatch.ElapsedMilliseconds);
				//----------------------------------------------------------------------------
				// En prévision de l'appel à ShowNextFrame, on vérifie qu'on ne va pas sortir.
				// Si c'est le cas, on stoppe la lecture pour rewind.
				// m_iFramesToDecode est toujours strictement positif. (Car on est en Play)
				//----------------------------------------------------------------------------
				long    TargetPosition = m_iCurrentPosition + (m_iFramesToDecode * m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame);

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
					m_Metadata.StopAllTracking();
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
							StartMultimediaTimer(GetFrameInterval());
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

					#if TRACE
					//Console.WriteLine("                             Total (4):{0}", m_NotIdleWatch.ElapsedMilliseconds);
					UpdateNavigationCursor();
					//Console.WriteLine("                             Total (4b):{0}", m_NotIdleWatch.ElapsedMilliseconds);
					#else
					UpdateNavigationCursor();
					#endif
					
					if (m_bShowInfos) { UpdateDebugInfos(); }

					// Empty frame queue.
					m_iFramesToDecode = 1;
					//m_bIsInPlayLoop = false;
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
					if (m_Metadata.SelectedTrack >= 0)
					{
						Track trk = m_Metadata.Tracks[m_Metadata.SelectedTrack];
						if (trk.EditMode && m_iCurrentPosition >= trk.BeginTimeStamp && m_iCurrentPosition <= trk.EndTimeStamp )
						{
							bTracking = true;
						}
					}

					if (!bTracking)
					{
						m_iFramesToDecode++;
						m_iDroppedFrames++;
					}
					
					#if TRACE
					//m_iFramesToDecode--;          // Uncomment this to go as fast as possible.
					//m_DropsPerSecond.Increment();
					#endif
					

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
					//m_bIsInPlayLoop = false;
				}
				
			}
		}
		private void IdleDetector(object sender, EventArgs e)
		{
			m_bIsIdle = true;
			//#if TRACE
			////Console.WriteLine("                             Total (5):{0}", m_NotIdleWatch.ElapsedMilliseconds);
			//#endif
		}
		private int ShowNextFrame(Int64 _iSeekTarget, bool _bAllowUIUpdate)
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

			#if TRACE
			//m_NotIdleWatch.Reset();
			//m_NotIdleWatch.Start();
			//m_DecodeWatch.Reset();
			//m_DecodeWatch.Start();
			int res = m_PlayerServer.GetNextFrame((long)_iSeekTarget, m_iFramesToDecode);
			//Console.WriteLine("decoding:{0}", m_DecodeWatch.ElapsedMilliseconds);
			#else
			int res = m_PlayerServer.GetNextFrame((long)_iSeekTarget, m_iFramesToDecode);
			#endif

			if (res == 0)
			{
				//#if TRACE
				//m_DecodedPerSecond.IncrementBy(m_iFramesToDecode);
				//#endif
				
				m_iDecodedFrames++;
				m_iCurrentPosition = m_PlayerServer.m_PrimarySelection.iCurrentTimeStamp;

				// Compute or stop tracking
				
				if (m_Metadata.Tracks.Count > 0)
				{
					if (_iSeekTarget >= 0 || m_iFramesToDecode > 1)
					{
						m_Metadata.StopAllTracking();
					}
					else
					{
						foreach (Track t in m_Metadata.Tracks)
						{
							if (t.EditMode)
							{
								t.TrackCurrentPosition(m_iCurrentPosition, m_PlayerServer.m_BmpImage);
							}
						}
					}
				}

				// Rendu de l'image à l'écran
				if(_bAllowUIUpdate) m_SurfaceScreen.Invalidate();
				// 7 ms disparaissent ici, entre la fin du _paint et le retour...
				//Console.WriteLine("                             Total (4a):{0}", m_NotIdleWatch.ElapsedMilliseconds);
				
			}
			else
			{
				switch (res)
				{
					case 2:
						{
							// SHOW_NEXT_FRAME_ALLOC_ERROR
							StopPlaying(_bAllowUIUpdate);
							
							// This will be a silent error.
							// It is very low level and seem to always come in pair with another error
							// for which we'll show the dialog.
							break;
						}
					case 3:
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
							m_Metadata.StopAllTracking();
							
							break;
						}
					case 4:
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
		public void StopPlaying()
		{
			StopPlaying(true);
		}
		private void StopPlaying(bool _bAllowUIUpdate)
		{
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				if (m_bIsCurrentlyPlaying)
				{
					StopMultimediaTimer();
					m_bIsCurrentlyPlaying = false;
					Application.Idle -= new EventHandler(this.IdleDetector);
					m_iFramesToDecode = 0;

					if (_bAllowUIUpdate)
					{
						buttonPlay.BackgroundImage = Videa.ScreenManager.Properties.Resources.liqplay17;
						_surfaceScreen.Invalidate();
					}
				}
			}
		}
		public void RefreshImage()
		{
			// For cases where m_SurfaceScreen.Invalidate() is not enough.
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				ShowNextFrame(m_iCurrentPosition, true);
			}
		}
		public int GetFrameInterval()
		{
			// Returns the interval between frames in Milliseconds.

			int iFrameInterval = 40;

			if (m_PlayerServer.m_bIsMovieLoaded && (m_PlayerServer.m_InfosVideo.iFrameInterval > 0))
			{
				iFrameInterval = (int)((double)m_PlayerServer.m_InfosVideo.iFrameInterval / ((double)m_iSlowmotionPercentage / 100));
			}
			return iFrameInterval;
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}

				formConfigureSpeed fcs = new formConfigureSpeed(m_PlayerServer.m_InfosVideo.fFps, this);
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

				fcs.ShowDialog();
				fcs.Dispose();

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}

				// Update times.
				UpdatePrimarySelectionPanelInfos();
				UpdateCurrentPositionInfos();
				m_SurfaceScreen.Invalidate();
			}
		}
		#endregion
		
		#region Culture
		public void RefreshUICulture(ResourceManager _ResourceManager)
		{
			m_ResourceManager = _ResourceManager;

			//--------------------------
			// 1. Labels
			//--------------------------
			lblSelStartSelection.AutoSize = true;
			lblSelDuration.AutoSize = true;
				
			lblSpeedTuner.Text = m_ResourceManager.GetString("lblSpeedTuner_Text", Thread.CurrentThread.CurrentUICulture) + " " + m_iSlowmotionPercentage + "%";
			lblWorkingZone.Text = m_ResourceManager.GetString("lblWorkingZone_Text", Thread.CurrentThread.CurrentUICulture);
			lblSelStartSelection.Text = m_ResourceManager.GetString("lblSelStartSelection_Text", Thread.CurrentThread.CurrentUICulture) + " : " + TimeStampsToTimecode(m_iSelStart, m_PrefManager.TimeCodeFormat, false);
			lblSelDuration.Text = m_ResourceManager.GetString("lblSelDuration_Text", Thread.CurrentThread.CurrentUICulture) + " : " + TimeStampsToTimecode(m_iSelDuration, m_PrefManager.TimeCodeFormat, false);
			lblTimeCode.Text = m_ResourceManager.GetString("lblTimeCode_Text", Thread.CurrentThread.CurrentUICulture) + " : " + TimeStampsToTimecode(m_iCurrentPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, m_bSynched);
			
			//lblSelDuration.Left = lblSelStartSelection.Left + lblSelStartSelection.Width + 8;
			lblSpeedTuner.Left = lblTimeCode.Left + lblTimeCode.Width + 8;
			sldrSpeed.Left = lblSpeedTuner.Left + lblSpeedTuner.Width + 8;
			
			//---------------------------
			// 2. ToolTips
			//---------------------------
			if (m_ePlayingMode == PlayingMode.Once)
			{
				toolTips.SetToolTip(buttonPlayingMode, m_ResourceManager.GetString("ToolTip_PlayingMode_Once", Thread.CurrentThread.CurrentUICulture));
			}
			else
			{
				toolTips.SetToolTip(buttonPlayingMode, m_ResourceManager.GetString("ToolTip_PlayingMode_Loop", Thread.CurrentThread.CurrentUICulture));
			}
			toolTips.SetToolTip(btnSnapShot, m_ResourceManager.GetString("ToolTip_Snapshot", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnRafale, m_ResourceManager.GetString("ToolTip_Rafale", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDiaporama, m_ResourceManager.GetString("dlgDiapoExport_Title", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnPdf, m_ResourceManager.GetString("dlgExportToPDF_Title", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(buttonPlay, m_ResourceManager.GetString("ToolTip_Play", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(buttonGotoPrevious, m_ResourceManager.GetString("ToolTip_Back", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(buttonGotoNext, m_ResourceManager.GetString("ToolTip_Next", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(buttonGotoFirst, m_ResourceManager.GetString("ToolTip_First", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(buttonGotoLast, m_ResourceManager.GetString("ToolTip_Last", Thread.CurrentThread.CurrentUICulture));
			if (m_bHandlersLocked)
			{
				toolTips.SetToolTip(btn_HandlersLock, m_ResourceManager.GetString("LockSelectionUnlock", Thread.CurrentThread.CurrentUICulture));
				mnuLockSelection.Tag = new ItemResourceInfo(m_ResourceManager, "LockSelectionUnlock");
				mnuLockSelection.Text = ((ItemResourceInfo)mnuLockSelection.Tag).resManager.GetString(((ItemResourceInfo)mnuLockSelection.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			}
			else
			{
				toolTips.SetToolTip(btn_HandlersLock, m_ResourceManager.GetString("LockSelectionLock", Thread.CurrentThread.CurrentUICulture));
				mnuLockSelection.Tag = new ItemResourceInfo(m_ResourceManager, "LockSelectionLock");
				mnuLockSelection.Text = ((ItemResourceInfo)mnuLockSelection.Tag).resManager.GetString(((ItemResourceInfo)mnuLockSelection.Tag).strText, Thread.CurrentThread.CurrentUICulture);
				
			}
			toolTips.SetToolTip(btnSetHandlerLeft, m_ResourceManager.GetString("ToolTip_SetHandlerLeft", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnSetHandlerRight, m_ResourceManager.GetString("ToolTip_SetHandlerRight", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnHandlersReset, m_ResourceManager.GetString("ToolTip_ResetWorkingZone", Thread.CurrentThread.CurrentUICulture));



			trkSelection.ToolTip = m_ResourceManager.GetString("ToolTip_trkSelection", Thread.CurrentThread.CurrentUICulture);
			sldrSpeed.ToolTip = m_ResourceManager.GetString("ToolTip_sldrSpeed", Thread.CurrentThread.CurrentUICulture);

			//-----------------------------------
			// 3. Contextual Menu
			//-----------------------------------
			// On nothing.
			mnuPlayPause.Text = ((ItemResourceInfo)mnuPlayPause.Tag).resManager.GetString(((ItemResourceInfo)mnuPlayPause.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetSelectionStart.Text = ((ItemResourceInfo)mnuSetSelectionStart.Tag).resManager.GetString(((ItemResourceInfo)mnuSetSelectionStart.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetSelectionEnd.Text = ((ItemResourceInfo)mnuSetSelectionEnd.Tag).resManager.GetString(((ItemResourceInfo)mnuSetSelectionEnd.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSetCaptureSpeed.Text = ((ItemResourceInfo)mnuSetCaptureSpeed.Tag).resManager.GetString(((ItemResourceInfo)mnuSetCaptureSpeed.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			//mnuLockSelection déjà fait plus haut...
			mnuSavePic.Text = ((ItemResourceInfo)mnuSavePic.Tag).resManager.GetString(((ItemResourceInfo)mnuSavePic.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuCloseScreen.Text = ((ItemResourceInfo)mnuCloseScreen.Tag).resManager.GetString(((ItemResourceInfo)mnuCloseScreen.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			
			// On drawing
			mnuDeleteDrawing.Text = ((ItemResourceInfo)mnuDeleteDrawing.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteDrawing.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuTrackTrajectory.Text = ((ItemResourceInfo)mnuTrackTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuTrackTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuConfigureFading.Text = ((ItemResourceInfo)mnuConfigureFading.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureFading.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuGotoKeyframe.Text = ((ItemResourceInfo)mnuGotoKeyframe.Tag).resManager.GetString(((ItemResourceInfo)mnuGotoKeyframe.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			// TODO "Color & Size" or "Color" depending on drawing type.
			mnuConfigureDrawing.Text = ((ItemResourceInfo)mnuConfigureDrawing.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureDrawing.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuShowMeasure.Text = ((ItemResourceInfo)mnuShowMeasure.Tag).resManager.GetString(((ItemResourceInfo)mnuShowMeasure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuSealMeasure.Text = ((ItemResourceInfo)mnuSealMeasure.Tag).resManager.GetString(((ItemResourceInfo)mnuSealMeasure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			
			// On chrono
			mnuChronoStart.Text = ((ItemResourceInfo)mnuChronoStart.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoStart.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoStop.Text = ((ItemResourceInfo)mnuChronoStop.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoStop.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoHide.Text = ((ItemResourceInfo)mnuChronoHide.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoHide.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoCountdown.Text = ((ItemResourceInfo)mnuChronoCountdown.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoCountdown.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoDelete.Text = ((ItemResourceInfo)mnuChronoDelete.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoDelete.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuChronoConfigure.Text = ((ItemResourceInfo)mnuChronoConfigure.Tag).resManager.GetString(((ItemResourceInfo)mnuChronoConfigure.Tag).strText, Thread.CurrentThread.CurrentUICulture);

			// On trajectory
			mnuStopTracking.Text = ((ItemResourceInfo)mnuStopTracking.Tag).resManager.GetString(((ItemResourceInfo)mnuStopTracking.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuRestartTracking.Text = ((ItemResourceInfo)mnuRestartTracking.Tag).resManager.GetString(((ItemResourceInfo)mnuRestartTracking.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteTrajectory.Text = ((ItemResourceInfo)mnuDeleteTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteEndOfTrajectory.Text = ((ItemResourceInfo)mnuDeleteEndOfTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteEndOfTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuConfigureTrajectory.Text = ((ItemResourceInfo)mnuConfigureTrajectory.Tag).resManager.GetString(((ItemResourceInfo)mnuConfigureTrajectory.Tag).strText, Thread.CurrentThread.CurrentUICulture);

			// On magnifier
			mnuMagnifier150.Text = ((ItemResourceInfo)mnuMagnifier150.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier150.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier175.Text = ((ItemResourceInfo)mnuMagnifier175.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier175.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier200.Text = ((ItemResourceInfo)mnuMagnifier200.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier200.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier225.Text = ((ItemResourceInfo)mnuMagnifier225.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier225.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifier250.Text = ((ItemResourceInfo)mnuMagnifier250.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifier250.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifierDirect.Text = ((ItemResourceInfo)mnuMagnifierDirect.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifierDirect.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuMagnifierQuit.Text = ((ItemResourceInfo)mnuMagnifierQuit.Tag).resManager.GetString(((ItemResourceInfo)mnuMagnifierQuit.Tag).strText, Thread.CurrentThread.CurrentUICulture);

			// On Grid
			mnuGridsConfigure.Text = ((ItemResourceInfo)mnuGridsConfigure.Tag).resManager.GetString(((ItemResourceInfo)mnuGridsConfigure.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuGridsHide.Text = ((ItemResourceInfo)mnuGridsHide.Tag).resManager.GetString(((ItemResourceInfo)mnuGridsHide.Tag).strText, Thread.CurrentThread.CurrentUICulture);


			//-----------------------------------
			// 4. Drawing Tools
			//-----------------------------------
			toolTips.SetToolTip(btnAddKeyframe, m_ResourceManager.GetString("ToolTip_AddKeyframe", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolPointer, m_ResourceManager.GetString("ToolTip_DrawingToolPointer", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolText, m_ResourceManager.GetString("ToolTip_DrawingToolText", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolPencil, m_ResourceManager.GetString("ToolTip_DrawingToolPencil", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolLine2D, m_ResourceManager.GetString("ToolTip_DrawingToolLine2D", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolCross2D, m_ResourceManager.GetString("ToolTip_DrawingToolCross2D", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolAngle2D, m_ResourceManager.GetString("ToolTip_DrawingToolAngle2D", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnShowComments, m_ResourceManager.GetString("ToolTip_ShowComments", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnCopyDrawings, m_ResourceManager.GetString("ToolTip_CopyDrawings", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnColorProfile, m_ResourceManager.GetString("ToolTip_ColorProfile", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnDrawingToolChrono, m_ResourceManager.GetString("ToolTip_DrawingToolChrono", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btnMagnifier, m_ResourceManager.GetString("ToolTip_Magnifier", Thread.CurrentThread.CurrentUICulture));
			toolTips.SetToolTip(btn3dplane, m_ResourceManager.GetString("mnu3DPlane", Thread.CurrentThread.CurrentUICulture));

			//-----------------------------------
			// 5. keyframes comment box
			//-----------------------------------
			m_KeyframeCommentsHub.RefreshUICulture();

			// 6. Keyframes positions (May result from a change in preferences)
			if (m_Metadata.Count > 0)
			{
				EnableDisableKeyframes();
			}

			// 6. Drawings.
			// Because this method is called when we change the general preferences (to update language)
			// We can use it to update colors and timecode format for chrono too.
			m_SurfaceScreen.Invalidate();
		}
		private void SetPopupConfigureParams(AbstractDrawing _drawing)
		{
			// choose between "Color" and "Color & Size" popup menu.

			if (_drawing is DrawingAngle2D || _drawing is DrawingCross2D)
			{
				mnuConfigureDrawing.Text = m_ResourceManager.GetString("mnuConfigureDrawing_Color", Thread.CurrentThread.CurrentUICulture);
			}
			else
			{
				mnuConfigureDrawing.Text = m_ResourceManager.GetString("mnuConfigureDrawing_ColorSize", Thread.CurrentThread.CurrentUICulture);
			}
			
			// Check Show Measure menu
			if(_drawing is DrawingLine2D)
			{
				mnuShowMeasure.Checked = ((DrawingLine2D)_drawing).ShowMeasure;
			}
		}
		#endregion

		#region SurfaceScreen Events
		private void _surfaceScreen_MouseDown(object sender, MouseEventArgs e)
		{
			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded)
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
							
							Point descaledMouse = DescaleCoordinates(e.Location);
							
							// 1. Pass all DrawingText to normal mode
							m_Metadata.AllDrawingTextToNormalMode();
							
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
									bDrawingHit = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseDown(m_Metadata, m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition, m_PrefManager.DefaultFading.Enabled);
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
								m_Metadata.SelectedTrack = -1;
								
								// Add a Chrono.
								DrawingToolChrono dtc = (DrawingToolChrono)m_DrawingTools[(int)m_ActiveTool];
								m_Metadata.Chronos.Insert(0, (DrawingChrono)dtc.GetNewDrawing(descaledMouse, m_iCurrentPosition, m_Metadata.AverageTimeStampsPerFrame));
								m_Metadata.SelectedChrono = 0;
								
								// Complete Setup
								m_Metadata.Chronos[0].ParentMetadata = m_Metadata;
								m_ColorProfile.SetupDrawing(m_Metadata.Chronos[0], DrawingToolType.Chrono);
							}
							else
							{
								//-----------------------
								// Creating a new Drawing
								//-----------------------
								m_Metadata.SelectedTrack = -1;
								m_Metadata.SelectedChrono = -1;
								
								// Add a KeyFrame here if it doesn't exist.
								AddKeyframe();
								
								if (m_ActiveTool != DrawingToolType.Text)
								{
									// Add an instance of a drawing from the active tool to the current keyframe.
									// The drawing is initialized with the current mouse coordinates.
									AbstractDrawing ad = m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, m_iCurrentPosition, m_Metadata.AverageTimeStampsPerFrame);
									
									m_Metadata[m_iActiveKeyFrameIndex].AddDrawing(ad);
									m_Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
									m_Metadata.SelectedDrawing = 0;
									
									// Color
									m_ColorProfile.SetupDrawing(ad, m_ActiveTool);
									
									DrawingLine2D line = ad as DrawingLine2D;
									if(line != null)
									{
										line.ParentMetadata = m_Metadata;
										line.ShowMeasure = m_bMeasuring;
									}

								}
								else
								{
									// We are using the Text Tool. This is a special case because
									// if we are on an existing Textbox, we just go into edit mode
									// otherwise, we add and setup a new textbox.
									bool bEdit = false;
									foreach (AbstractDrawing ad in m_Metadata[m_iActiveKeyFrameIndex].Drawings)
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
										m_Metadata[m_iActiveKeyFrameIndex].AddDrawing(m_DrawingTools[(int)m_ActiveTool].GetNewDrawing(descaledMouse, m_iCurrentPosition, m_Metadata.AverageTimeStampsPerFrame));
										m_Metadata.SelectedDrawingFrame = m_iActiveKeyFrameIndex;
										m_Metadata.SelectedDrawing = 0;
										
										DrawingText dt = (DrawingText)m_Metadata[m_iActiveKeyFrameIndex].Drawings[0];
										
										dt.ContainerScreen = m_SurfaceScreen;
										dt.RelocateEditbox(m_fStretchFactor*m_fDirectZoomFactor, m_DirectZoomWindow.Location);
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
						
						Point descaledMouse = DescaleCoordinates(e.Location);
						
						if (!m_bIsCurrentlyPlaying)
						{
							m_Metadata.UnselectAll();
							
							//m_bWasRightClicking = true;
							
							if (m_Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition))
							{
								// If we are on a Cross2D, we activate the menu to let the user Track it.
								AbstractDrawing ad = m_Metadata.Keyframes[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing];
								
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
							else if (m_Metadata.IsOnChronometer(descaledMouse, m_iCurrentPosition))
							{
								// We can only toggle to countdown if we already have a stop.
								mnuChronoCountdown.Enabled = m_Metadata.Chronos[m_Metadata.SelectedChrono].HasTimeStop;
								mnuChronoCountdown.Checked = m_Metadata.Chronos[m_Metadata.SelectedChrono].CountDown;
								this.ContextMenuStrip = popMenuChrono;
							}
							else if (m_Metadata.IsOnTrack(descaledMouse, m_iCurrentPosition))
							{
								// Configure the "Tracking" Pop Menu
								if (m_Metadata.Tracks[m_Metadata.SelectedTrack].EditMode)
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
							else if (m_Metadata.IsOnGrid(descaledMouse))
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
					
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void SurfaceScreen_MouseMove(object sender, MouseEventArgs e)
		{
			// We must keep the same Z order.
			// 1:Magnifier, 2:Drawings, 3:Chronos/Tracks, 4:Grids.
			// When creating a drawing, the active tool will stay on this drawing until its setup is over.
			// After the drawing is created, we fall back to Pointer tool.
			// (except for pencil/cross)

			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded)
				{
					if (e.Button == MouseButtons.None && m_Magnifier.Mode == MagnifierMode.Direct)
					{
						m_Magnifier.MouseX = e.X;
						m_Magnifier.MouseY = e.Y;
						
						if (!m_bIsCurrentlyPlaying)
						{
							m_SurfaceScreen.Invalidate();
						}
					}
					else if (e.Button == MouseButtons.Left)
					{
						if (m_ActiveTool != DrawingToolType.Pointer && m_ActiveTool != DrawingToolType.Chrono)
						{
							if (m_iActiveKeyFrameIndex >= 0 && !m_bIsCurrentlyPlaying)
							{
								// Currently setting the second point of a Drawing.
								m_DrawingTools[(int)m_ActiveTool].OnMouseMove(m_Metadata[m_iActiveKeyFrameIndex], DescaleCoordinates(new Point(e.X, e.Y)));
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
									Point descaledMouse = DescaleCoordinates(e.Location);
									
									// Magnifier is not being moved or is invisible, try drawings through pointer tool.
									// (including chronos, tracks and grids)
									bool bMovingObject = ((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).OnMouseMove(m_Metadata, m_iActiveKeyFrameIndex, descaledMouse, m_DirectZoomWindow.Location, ModifierKeys);
									
									if (!bMovingObject && m_fDirectZoomFactor > 1.0f)
									{
										// Move the whole image in direct zoom mode.
										
										// Get mouse deltas (descaled=in image coords).
										double fDeltaX = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.X;
										double fDeltaY = (double)((DrawingToolPointer)m_DrawingTools[(int)m_ActiveTool]).MouseDelta.Y;
										
										if(Mirrored)
										{
											fDeltaX = -fDeltaX;
										}
										
										// Block at borders.
										int iNewLeft = (int)((double)m_DirectZoomWindow.Left - fDeltaX);
										int iNewTop = (int)((double)m_DirectZoomWindow.Top - fDeltaY);
										
										if (iNewLeft < 0) iNewLeft = 0;
										if (iNewLeft + m_DirectZoomWindow.Width >= m_PlayerServer.m_InfosVideo.iDecodingWidth)
											iNewLeft = m_PlayerServer.m_InfosVideo.iDecodingWidth - m_DirectZoomWindow.Width;
										
										if (iNewTop < 0) iNewTop = 0;
										if (iNewTop + m_DirectZoomWindow.Height >= m_PlayerServer.m_InfosVideo.iDecodingHeight)
											iNewTop = m_PlayerServer.m_InfosVideo.iDecodingHeight - m_DirectZoomWindow.Height;
										
										// Reposition.
										m_DirectZoomWindow = new Rectangle(iNewLeft, iNewTop, m_DirectZoomWindow.Width, m_DirectZoomWindow.Height);
									
										log.Debug(String.Format("Zoom Window : Location:{0}, Size:{1}", m_DirectZoomWindow.Location, m_DirectZoomWindow.Size));
									}
								}
							}
						}
						
						if (!m_bIsCurrentlyPlaying)
						{
							m_SurfaceScreen.Invalidate();
						}
					}
				}
			}
		}
		private void SurfaceScreen_MouseUp(object sender, MouseEventArgs e)
		{
			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded)
				{
					if (e.Button == MouseButtons.Left)
					{
						if (m_ActiveTool == DrawingToolType.Pointer)
						{
							SetAsActiveScreen();
							SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
							
							// Update tracks with current image and pos.
							if (m_Metadata.SelectedTrack >= 0)
							{
								if (m_Metadata.Tracks[m_Metadata.SelectedTrack].EditMode)
								{
									m_Metadata.Tracks[m_Metadata.SelectedTrack].UpdateCurrentPos(m_PlayerServer.m_BmpImage);
								}
							}
						}
						
						m_Magnifier.OnMouseUp(e);
						
						// memorize the previous action to enable undo
						if(m_ActiveTool == DrawingToolType.Chrono)
						{
							IUndoableCommand cac = new CommandAddChrono(this, m_Metadata);
							CommandManager cm = CommandManager.Instance();
							cm.LaunchUndoableCommand(cac);
						}
						else if (m_ActiveTool != DrawingToolType.Pointer && m_iActiveKeyFrameIndex >= 0)
						{
							// Record the adding unless we are editing a text box.
							if (!m_bTextEdit)
							{
								IUndoableCommand cad = new CommandAddDrawing(this, m_Metadata, m_Metadata[m_iActiveKeyFrameIndex].Position);
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
							SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
							((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).OnMouseUp();
						}
						
						if (m_iActiveKeyFrameIndex >= 0)
						{
							m_Metadata.SelectedDrawingFrame = -1;
							m_Metadata.SelectedDrawing = -1;
						}
						
						m_SurfaceScreen.Invalidate();
					}
				}
			}
		}
		private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded && e.Button == MouseButtons.Left)
				{
					SetAsActiveScreen();
					
					Point descaledMouse = DescaleCoordinates(e.Location);
					m_Metadata.AllDrawingTextToNormalMode();
					m_Metadata.UnselectAll();
					
					//------------------------------------------------------------------------------------
					// - If on text, switch to edit mode.
					// - If on other drawing, including chronos and grid, launch the configuration dialog.
					// - Otherwise -> Maximize/Reduce image.
					//------------------------------------------------------------------------------------
					
					if (m_Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, descaledMouse, m_iCurrentPosition))
					{
						AbstractDrawing ad = m_Metadata.Keyframes[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing];
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
					else if (m_Metadata.IsOnChronometer(descaledMouse, m_iCurrentPosition))
					{
						mnuChronoConfigure_Click(null, EventArgs.Empty);
					}
					else if (m_Metadata.IsOnTrack(descaledMouse, m_iCurrentPosition))
					{
						mnuConfigureTrajectory_Click(null, EventArgs.Empty);
					}
					else if (m_Metadata.IsOnGrid(descaledMouse))
					{
						mnuGridsConfigure_Click(null, EventArgs.Empty);
					}
					else
					{
						ToggleStretchMode();
					}
				}
			}
		}
		private void SurfaceScreen_Paint(object sender, PaintEventArgs e)
		{
			//Console.WriteLine("                             Total (1a):{0}", m_NotIdleWatch.ElapsedMilliseconds);
			//-------------------------------------------------------------------
			// On dessine toujours à la taille du SurfaceScreen.
			// C'est le SurfaceScreen qui change de taille sur resize ou stretch.
			//-------------------------------------------------------------------
			if(m_PlayerServer != null)
			{
				if (m_PlayerServer.m_bIsMovieLoaded)
				{
					if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
					{
						m_DrawingFilterOutput.Draw(e.Graphics, m_SurfaceScreen.Size, m_DrawingFilterOutput.InputFrames, m_DrawingFilterOutput.PrivateData);
					}
					else if(m_PlayerServer.m_BmpImage != null)
					{
						// ArgumentNullException
						// AccessViolationException
						// InvalidOperationException
						
						try
						{
							// If we are on a keyframe, see if it has any drawing.
							int iKeyFrameIndex = -1;
							if (m_iActiveKeyFrameIndex >= 0)
							{
								if (m_Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
								{
									iKeyFrameIndex = m_iActiveKeyFrameIndex;
								}
							}
							
							FlushOnGraphics(m_PlayerServer.m_BmpImage, e.Graphics, m_SurfaceScreen.Size, iKeyFrameIndex, m_iCurrentPosition);
							
							#if TRACE
							//m_RenderedPerSecond.Increment();
							#endif
						}
						catch (System.InvalidOperationException)
						{
							log.Error("Error while painting image. Object is currently in use elsewhere... ATI Drivers ?");
							//Console.WriteLine(exp.StackTrace);
						}
						//catch(System.AccessViolationException exception)
						//{
						//}
						//catch(System.ArgumentNullException exception)
						//{
						//}
						catch (Exception exp)
						{
							log.Error("Unknown error while painting image.");
							log.Error(exp.StackTrace);
						}
						finally
						{
							// Rien de particulier à faire.
						}
					}
					
					// Draw Selection Border if needed.
					if (m_bShowImageBorder)
					{
						DrawImageBorder(e.Graphics);
					}
				}
			}
			//Console.WriteLine("                             Total (3c):{0}", m_NotIdleWatch.ElapsedMilliseconds);
		}
		private void SurfaceScreen_MouseEnter(object sender, EventArgs e)
		{
			// Set focus to surfacescreen to enable mouse scroll
			
			// But only if there is no Text edition going on.
			bool bEditing = false;
			if(m_Metadata.Count > m_iActiveKeyFrameIndex && m_iActiveKeyFrameIndex >= 0)
			{
				foreach (AbstractDrawing ad in m_Metadata[m_iActiveKeyFrameIndex].Drawings)
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
				_surfaceScreen.Focus();
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
			if(m_Metadata.Mirrored)
			{
				rDst = new Rectangle(_iNewSize.Width, 0, -_iNewSize.Width, _iNewSize.Height);
			}
			else
			{
				rDst = new Rectangle(0, 0, _iNewSize.Width, _iNewSize.Height);
			}
			
			Rectangle rSrc;
			if (m_fDirectZoomFactor > 1.0f)
			{
				rSrc = m_DirectZoomWindow;
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
			//Matrix mx = new Matrix((float)m_fStretchFactor, 0, 0, (float)m_fStretchFactor, 0, 0);
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

			FlushDrawingsOnGraphics(g, _iKeyFrameIndex, _iPosition, m_fStretchFactor, m_fDirectZoomFactor, m_DirectZoomWindow.Location);

			// .Magnifier
			if (m_Magnifier.Mode != MagnifierMode.NotVisible)
			{
				// Mirrored ?

				m_Magnifier.Draw(_sourceImage, g, m_fStretchFactor, m_ColorProfile);
			}
		}
		private void FlushDrawingsOnGraphics(Graphics _canvas, int _iKeyFrameIndex, long _iPosition, double _fStretchFactor, double _fDirectZoomFactor, Point _DirectZoomTopLeft)
		{
			// Prepare for drawings
			_canvas.SmoothingMode = SmoothingMode.AntiAlias;

			// 1. 2D Grid
			if (m_Metadata.Grid.Visible)
			{
				m_Metadata.Grid.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _DirectZoomTopLeft);
			}

			// 2. 3D Plane
			if (m_Metadata.Plane.Visible)
			{
				m_Metadata.Plane.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _DirectZoomTopLeft);
			}

			// 3. Regular drawings.
			if (m_PrefManager.DefaultFading.Enabled)
			{
				if ((m_bIsCurrentlyPlaying && m_PrefManager.DrawOnPlay) || !m_bIsCurrentlyPlaying)
				{
					// If fading is on, we ask all drawings to draw themselves with their respective
					// fading factor for this position.

					int[] zOrder = m_Metadata.GetKeyframesZOrder(_iPosition);

					// Draw in reverse keyframes z order so the closest next keyframe gets drawn on top (last).
					for (int ikf = zOrder.Length-1; ikf >= 0 ; ikf--)
					{
						Keyframe kf = m_Metadata.Keyframes[zOrder[ikf]];
						for (int idr = kf.Drawings.Count - 1; idr >= 0; idr--)
						{
							bool bSelected = (zOrder[ikf] == m_Metadata.SelectedDrawingFrame && idr == m_Metadata.SelectedDrawing);
							kf.Drawings[idr].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, bSelected, _iPosition, _DirectZoomTopLeft);
						}
					}
				}
			}
			else if (_iKeyFrameIndex >= 0)
			{
				// if fading is off, only draw the current keyframe.
				// Draw all drawings in reverse order to get first object on the top of Z-order.
				for (int i = m_Metadata[_iKeyFrameIndex].Drawings.Count - 1; i >= 0; i--)
				{
					bool bSelected = (_iKeyFrameIndex == m_Metadata.SelectedDrawingFrame && i == m_Metadata.SelectedDrawing);
					m_Metadata[_iKeyFrameIndex].Drawings[i].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, bSelected, _iPosition, _DirectZoomTopLeft);
				}
			}
			else
			{
				// This is not a Keyframe, and fading is off.
				// Hence, there is no drawings to draw here.
			}

			// 4. Tracks
			if (m_Metadata.Tracks.Count > 0)
			{
				foreach (Track t in m_Metadata.Tracks)
				{
					t.Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, _iPosition, _DirectZoomTopLeft);
				}
			}

			// 5. Chronos
			if (m_Metadata.Chronos.Count > 0)
			{
				for (int i = 0; i < m_Metadata.Chronos.Count; i++)
				{
					m_Metadata.Chronos[i].Draw(_canvas, _fStretchFactor * _fDirectZoomFactor, (i == m_Metadata.SelectedChrono), _iPosition, _DirectZoomTopLeft);
				}
			}
		}
		#endregion

		#region PanelCenter Events
		private void PanelCenter_MouseEnter(object sender, EventArgs e)
		{
			// Give focus to enable mouse scroll.
			_panelCenter.Focus();
		}
		private void PanelCenter_MouseClick(object sender, MouseEventArgs e)
		{
			SetAsActiveScreen();
		}
		private void PanelCenter_Resize(object sender, EventArgs e)
		{
			StretchSqueezeSurface();
			m_SurfaceScreen.Invalidate();
		}
		#endregion
		
		private void PanelVideoControls_MouseEnter(object sender, EventArgs e)
		{
			// Set focus to enable mouse scroll
			panelVideoControls.Focus();	
		}
		
		#region Keyframes Panel
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
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				AddKeyframe();

				// Set as active screen is done after so the export as pdf menu is activated
				// even if we had no keyframes yet.
				SetAsActiveScreen();
			}
		}
		private void OrganizeKeyframes()
		{
			// Should only be called when adding/removing a Thumbnail
			
			pnlThumbnails.Controls.Clear();

			if (m_Metadata.Count > 0)
			{
				int iKeyframeIndex = 0;
				int iPixelsOffset = 0;
				int iPixelsSpacing = 20;

				foreach (Keyframe kf in m_Metadata.Keyframes)
				{
					ThumbBox tBox = new ThumbBox();
					SetupDefaultThumbBox(tBox);
					
					// Finish the setup
					tBox.Left = iPixelsOffset + iPixelsSpacing;

					tBox.pbThumbnail.Image = kf.Thumbnail;
					tBox.Title = kf.Title;
					//tBox.TimeCode = 
					
					
					tBox.Tag = iKeyframeIndex;
					tBox.pbThumbnail.SizeMode = PictureBoxSizeMode.CenterImage;
					
					tBox.CloseThumb += new ThumbBox.CloseThumbHandler(ThumbBoxClose);
					tBox.ClickThumb += new ThumbBox.ClickThumbHandler(ThumbBoxClick);
					tBox.ClickInfos += new ThumbBox.ClickInfosHandler(ThumbBoxInfosClick);
					
					// TODO - Titre de la Keyframe en ToolTip.
					iPixelsOffset += (iPixelsSpacing + tBox.Width);

					pnlThumbnails.Controls.Add(tBox);

					iKeyframeIndex++;
				}
				EnableDisableKeyframes();
				pnlThumbnails.Refresh();
				m_SurfaceScreen.Invalidate(); // Because of trajectories.
			}
			else
			{
				DockKeyframePanel();
				m_iActiveKeyFrameIndex = -1;
				
			}
		}
		private void SetupDefaultThumbBox(ThumbBox _ThumbBox)
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
				if (m_Metadata[i].Position == _iPosition)
				{
					m_iActiveKeyFrameIndex = i;
					if(_bAllowUIUpdate)
						((ThumbBox)pnlThumbnails.Controls[i]).Selected = true;

					// Make sure the thumbnail is always in the visible area by auto scrolling.
					if(_bAllowUIUpdate) pnlThumbnails.ScrollControlIntoView(pnlThumbnails.Controls[i]);
				}
				else
				{
					if(_bAllowUIUpdate)
						((ThumbBox)pnlThumbnails.Controls[i]).Selected = false;
				}
			}

			if (_bAllowUIUpdate && m_KeyframeCommentsHub.UserActivated && m_iActiveKeyFrameIndex >= 0)
			{
				m_KeyframeCommentsHub.UpdateContent(m_Metadata[m_iActiveKeyFrameIndex]);
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
				ThumbBox tb = pnlThumbnails.Controls[i] as ThumbBox;
				if(tb != null)
				{
					m_Metadata[i].TimeCode = TimeStampsToTimecode(m_Metadata[i].Position - m_iSelStart, m_PrefManager.TimeCodeFormat, false);
					
					// Enable thumbs that are within Working Zone, grey out others.
					if (m_Metadata[i].Position >= m_iSelStart && m_Metadata[i].Position <= m_iSelEnd)
					{
						m_Metadata[i].Disabled = false;
						
						tb.Enabled = true;
						tb.pbThumbnail.Image = m_Metadata[i].Thumbnail;						
						tb.Title = m_Metadata[i].Title;						
					}
					else
					{
						m_Metadata[i].Disabled = true;
						
						tb.Enabled = false;
						tb.pbThumbnail.Image = m_Metadata[i].DisabledThumbnail;
						tb.Title = m_Metadata[i].Title;						
					}
				}
			}
		}
		public void OnKeyframesTitleChanged()
		{
			// Called when title changed.

			// Update trajectories.
			m_Metadata.UpdateTrajectoriesForKeyframes();

			// Update thumb boxes.
			EnableDisableKeyframes();

			m_SurfaceScreen.Invalidate();

		}
		private void GotoNextKeyframe()
		{
			if (m_Metadata.Count > 1)
			{
				int iNextKeyframe = -1;
				for (int i = 0; i < m_Metadata.Count; i++)
				{
					if (m_iCurrentPosition < m_Metadata[i].Position)
					{
						iNextKeyframe = i;
						break;
					}
				}

				if (iNextKeyframe >= 0 && m_Metadata[iNextKeyframe].Position <= m_iSelEnd)
				{
					ThumbBoxClick(pnlThumbnails.Controls[iNextKeyframe], EventArgs.Empty);
				}
				
			}
		}
		private void GotoPreviousKeyframe()
		{
			if (m_Metadata.Count > 0)
			{
				int iPrevKeyframe = -1;
				for (int i = m_Metadata.Count - 1; i >= 0; i--)
				{
					if (m_iCurrentPosition > m_Metadata[i].Position)
					{
						iPrevKeyframe = i;
						break;
					}
				}

				if (iPrevKeyframe >= 0 && m_Metadata[iPrevKeyframe].Position >= m_iSelStart)
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
			for (i = 0; i < m_Metadata.Count; i++)
			{
				if (m_Metadata[i].Position == m_iCurrentPosition)
				{
					bAlreadyKeyFrame = true;
					m_iActiveKeyFrameIndex = i;
				}
			}

			// Add it to the list.
			if (!bAlreadyKeyFrame)
			{
				IUndoableCommand cak = new CommandAddKeyframe(this, m_Metadata, m_iCurrentPosition);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cak);
				
				// If it is the very first key frame, we raise the KF panel.
				// Otherwise we keep whatever choice the user made.
				if(m_Metadata.Count == 1)
				{
					UndockKeyframePanel();
				}
			}
		}
		public void OnAddKeyframe(long _iPosition)
		{
			// Public because called from CommandAddKeyframe.Execute()
			// Title becomes the current timecode. (relative to sel start or sel minimum ?)
			
			Keyframe kf = new Keyframe(_iPosition, TimeStampsToTimecode(_iPosition - m_iSelStart, m_PrefManager.TimeCodeFormat, m_bSynched), m_PlayerServer.m_BmpImage);
			
			if (_iPosition != m_iCurrentPosition)
			{
				// Move to the required Keyframe.
				// Should only happen when Undoing a DeleteKeyframe.
				m_iFramesToDecode = 1;
				ShowNextFrame(_iPosition, true);
				UpdateNavigationCursor();
				UpdateCurrentPositionInfos();
				trkSelection.SelPos = trkFrame.Position;

				// Readjust and complete the Keyframe
				kf.ImportImage(m_PlayerServer.m_BmpImage);
			}

			m_Metadata.Add(kf);

			// Keep the list sorted
			m_Metadata.Sort();

			m_Metadata.UpdateTrajectoriesForKeyframes();
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
			IUndoableCommand cdk = new CommandDeleteKeyframe(this, m_Metadata, m_Metadata[_iKeyframeIndex].Position);
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

			m_Metadata.RemoveAt(_iKeyframeIndex);
			m_Metadata.UpdateTrajectoriesForKeyframes();
			UpdateKeyframesMarkers();
			OrganizeKeyframes();
			m_SurfaceScreen.Invalidate();
		}
		public void UpdateKeyframes()
		{
			// Primary selection has been image-adjusted,
			// some keyframes may have been impacted.

			bool bAtLeastOne = false;

			foreach (Keyframe kf in m_Metadata.Keyframes)
			{
				if (kf.Position >= m_iSelStart && kf.Position <= m_iSelEnd)
				{
					kf.ImportImage(m_PlayerServer.m_FrameList[(int)m_PlayerServer.GetFrameNumber(kf.Position)].BmpImage);
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
			SetAsActiveScreen();
		}

		#region ThumbBox event Handlers
		private void ThumbBoxClose(object sender, EventArgs e)
		{
			RemoveKeyframe((int)((ThumbBox)sender).Tag);

			// Set as active screen is done after in case we don't have any keyframes left.
			SetAsActiveScreen();
		}
		private void ThumbBoxClick(object sender, EventArgs e)
		{
			// Move to the right spot.
			SetAsActiveScreen();
			StopPlaying();

			long iTargetPosition = m_Metadata[(int)((ThumbBox)sender).Tag].Position;

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
			btnDockBottom.BackgroundImage = Videa.ScreenManager.Properties.Resources.undock16x16;

			// If there is 0 images, the arrow isn't visible.
			if (m_Metadata.Count == 0)
				btnDockBottom.Visible = false;

			// change status
			m_bDocked = true;
			
		}
		private void UndockKeyframePanel()
		{
			// show the keyframes
			splitKeyframes.SplitterDistance = splitKeyframes.Height - 140;

			// change image
			btnDockBottom.BackgroundImage = Videa.ScreenManager.Properties.Resources.dock16x16;
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
			if (m_Metadata.Count < 1)
			{
				UndockKeyframePanel();
			}
		}
		#endregion

		#endregion

		#region Drawings Toolbar Event Handlers
		private void btnDrawingToolLine2D_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Line2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorLine2D, 0));
				
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolPointer_Click(object sender, EventArgs e)
		{
			SetAsActiveScreen();
			m_ActiveTool = DrawingToolType.Pointer;
			SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
			if (m_Metadata.Count < 1)
			{
				DockKeyframePanel();
			}
		}
		private void btnDrawingToolCross2D_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Cross2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorCross2D, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolAngle2D_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Angle2D;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorAngle2D, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolPencil_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Pencil;
				UpdateCursor();
				PrepareKeyframesDock();
			}
		}
		private void btnDrawingToolChrono_Click(object sender, EventArgs e)
		{
			if (m_Magnifier.Mode != MagnifierMode.Direct)
			{
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Chrono;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(m_ColorProfile.ColorChrono, 0));
			}
		}
		private void btnMagnifier_Click(object sender, EventArgs e)
		{
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				m_ActiveTool = DrawingToolType.Pointer;

				// Magnifier is half way between a persisting tool (like trackers and chronometers).
				// and a mode like grid and 3dplane.
				if (m_Magnifier.Mode == MagnifierMode.NotVisible)
				{
					UnzoomDirectZoom();
					m_Magnifier.Mode = MagnifierMode.Direct;
					btnMagnifier.BackgroundImage = Videa.ScreenManager.Properties.Resources.magnifierActive2;
					SetCursor(Cursors.Cross);
				}
				else if (m_Magnifier.Mode == MagnifierMode.Direct)
				{
					// Revert to no magnification.
					UnzoomDirectZoom();
					m_Magnifier.Mode = MagnifierMode.NotVisible;
					btnMagnifier.BackgroundImage = Videa.ScreenManager.Properties.Resources.magnifier2;
					SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
					m_SurfaceScreen.Invalidate();
				}
				else
				{
					DisableMagnifier();
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void DisableMagnifier()
		{
			// Revert to no magnification.
			m_Magnifier.Mode = MagnifierMode.NotVisible;
			btnMagnifier.BackgroundImage = Videa.ScreenManager.Properties.Resources.magnifier2;
			SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
		}
		private void btn3dplane_Click(object sender, EventArgs e)
		{
			m_Metadata.Plane.Visible = !m_Metadata.Plane.Visible;
			m_ActiveTool = DrawingToolType.Pointer;
			SetAsActiveScreen();
			m_SurfaceScreen.Invalidate();
		}
		
		private void UpdateCursor()
		{
			// Ther current cursor must be updated.

			// Get the cursor and use it.
			if (m_ActiveTool == DrawingToolType.Pencil)
			{
				int iCircleSize = (int)((double)m_ColorProfile.StylePencil.Size * m_fStretchFactor);
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
				SetAsActiveScreen();
				m_ActiveTool = DrawingToolType.Text;
				SetCursor(m_DrawingTools[(int)m_ActiveTool].GetCursor(Color.Empty, 0));
				PrepareKeyframesDock();
			}
		}
		private void btnCopyDrawings_Click(object sender, EventArgs e)
		{
			// Unused. Not reliable...

			/*
            // Import drawings from previous Keyframe into this one.
            // don't remove the existing ones (?)

            SetAsActiveScreen();
            
            if (m_PlayerServer.m_bIsMovieLoaded && m_Metadata.Count > 0)
            {
                // Get previous Keyframe.
                int iPrevKeyframe = -1;
                for (int i = m_Metadata.Count - 1; i >= 0; i--)
                {
                    if (m_iCurrentPosition > m_Metadata[i].Position)
                    {
                        iPrevKeyframe = i;
                        break;
                    }
                }

                // Create and Import
                if (iPrevKeyframe >= 0)
                {
                    AddKeyframe();

                    CommandManager cm = CommandManager.Instance();

                    // Clone all Drawings (in the original order...)
                    for (int iDrawing = m_Metadata[iPrevKeyframe].Drawings.Count - 1; iDrawing >= 0; iDrawing--)
                    {
                        // Always insert at 0.
                        m_Metadata[m_iActiveKeyFrameIndex].Drawings.Insert(0, m_Metadata[iPrevKeyframe].Drawings[iDrawing].Clone());

                        IUndoableCommand cad = new CommandAddDrawing(this, m_Metadata, m_Metadata[m_iActiveKeyFrameIndex].Position);
                        cm.LaunchUndoableCommand(cad);
                    }
                    m_SurfaceScreen.Invalidate();
                }
            }*/
		}
		private void btnShowComments_Click(object sender, EventArgs e)
		{
			SetAsActiveScreen();

			if (m_PlayerServer.m_bIsMovieLoaded)
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

				m_KeyframeCommentsHub.UpdateContent(m_Metadata[m_iActiveKeyFrameIndex]);
				m_KeyframeCommentsHub.Visible = true;
			}
		}
		private void btnColorProfile_Click(object sender, EventArgs e)
		{
			SetAsActiveScreen();

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

			m_SurfaceScreen.Cursor = _cur;
		}
		private void LocateForm(Form _form)
		{
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

		#region Drawings Menus
		private void mnuConfigureDrawing_Click(object sender, EventArgs e)
		{
			if(m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				formConfigureDrawing fcd = new formConfigureDrawing(m_Metadata[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing], m_SurfaceScreen);
				LocateForm(fcd);
				fcd.ShowDialog();
				fcd.Dispose();
				m_SurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		private void mnuConfigureFading_Click(object sender, EventArgs e)
		{
			if(m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				formConfigureFading fcf = new formConfigureFading(m_Metadata[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing], m_SurfaceScreen);
				LocateForm(fcf);
				fcf.ShowDialog();
				fcf.Dispose();
				m_SurfaceScreen.Invalidate();
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
			if (m_iActiveKeyFrameIndex >= 0 && m_iActiveKeyFrameIndex == m_Metadata.SelectedDrawingFrame)
			{
				int iSelectedDrawing = m_Metadata.SelectedDrawing;

				if (iSelectedDrawing >= 0)
				{
					// TODO - link to CommandAddTrajectory.
					// TODO - always insert at 0.

					// Ajouter un Track sur ce point.
					DrawingCross2D dc = (DrawingCross2D)m_Metadata[m_iActiveKeyFrameIndex].Drawings[iSelectedDrawing];
					m_Metadata.Tracks.Add(new Track(dc.CenterPoint.X, dc.CenterPoint.Y, m_iCurrentPosition, m_PlayerServer.m_BmpImage));

					// Complete Setup
					m_Metadata.Tracks[m_Metadata.Tracks.Count - 1].m_ShowClosestFrame = new ShowClosestFrame(OnShowClosestFrame);
					m_Metadata.Tracks[m_Metadata.Tracks.Count - 1].ParentMetadata = m_Metadata;
					m_Metadata.Tracks[m_Metadata.Tracks.Count - 1].MainColor = dc.PenColor;

					m_Metadata.Tracks[m_Metadata.Tracks.Count - 1].EditMode = true;
					m_Metadata.SelectedTrack = m_Metadata.Tracks.Count - 1;

					// Supprimer le point en tant que Drawing ?
					m_Metadata[m_iActiveKeyFrameIndex].Drawings.RemoveAt(iSelectedDrawing);
					
					m_Metadata.SelectedDrawingFrame = -1;
					m_Metadata.SelectedDrawing = -1;

					// Return to the pointer tool.
					m_ActiveTool = DrawingToolType.Pointer;
					SetCursor(m_DrawingTools[(int)DrawingToolType.Pointer].GetCursor(Color.Empty, 0));
				}
			}
			m_SurfaceScreen.Invalidate();
		}
		private void mnuGotoKeyframe_Click(object sender, EventArgs e)
		{
			if (m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				long iPosition = m_Metadata[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing].infosFading.ReferenceTimestamp;

				m_iFramesToDecode = 1;
				ShowNextFrame(iPosition, true);
				UpdateNavigationCursor();
				UpdateCurrentPositionInfos();
				trkSelection.SelPos = trkFrame.Position;
				ActivateKeyframe(m_iCurrentPosition);
			}
		}
		private void mnuShowMeasure_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the measure for this line.
			if(m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_Metadata[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing] as DrawingLine2D;
				if(line!= null)
				{
					mnuShowMeasure.Checked = !mnuShowMeasure.Checked;
					line.ShowMeasure = mnuShowMeasure.Checked;
					m_bMeasuring = mnuShowMeasure.Checked;
					m_SurfaceScreen.Invalidate();
				}
			}
		}
		private void mnuSealMeasure_Click(object sender, EventArgs e)
		{
			// display a dialog that let the user specify how many real-world-units long is this line.
		
			if(m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				DrawingLine2D line = m_Metadata[m_Metadata.SelectedDrawingFrame].Drawings[m_Metadata.SelectedDrawing] as DrawingLine2D;
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

						formConfigureMeasure fcm = new formConfigureMeasure(m_Metadata, line);
						LocateForm(fcm);
						fcm.ShowDialog();
						fcm.Dispose();
						
						m_SurfaceScreen.Invalidate();
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
			if (m_Metadata.SelectedDrawingFrame >= 0 && m_Metadata.SelectedDrawing >= 0)
			{
				IUndoableCommand cdd = new CommandDeleteDrawing(this, m_Metadata, m_Metadata[m_Metadata.SelectedDrawingFrame].Position, m_Metadata.SelectedDrawing);
				CommandManager cm = CommandManager.Instance();
				cm.LaunchUndoableCommand(cdd);
				m_SurfaceScreen.Invalidate();
				this.ContextMenuStrip = popMenu;
			}
		}
		#endregion
		
		#region Tracking Menus
		private void mnuStopTracking_Click(object sender, EventArgs e)
		{
			m_Metadata.Tracks[m_Metadata.SelectedTrack].StopTracking();
			m_SurfaceScreen.Invalidate();
		}
		private void mnuDeleteEndOfTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdeot = new CommandDeleteEndOfTrack(this, m_Metadata, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cdeot);

			//m_Metadata.Tracks[m_Metadata.SelectedTrack].ChopTrajectory(m_iCurrentPosition);
			m_SurfaceScreen.Invalidate();
		}
		private void mnuRestartTracking_Click(object sender, EventArgs e)
		{
			m_Metadata.Tracks[m_Metadata.SelectedTrack].RestartTracking();
			m_SurfaceScreen.Invalidate();
		}
		private void mnuDeleteTrajectory_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteTrack(this, m_Metadata);
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

			formConfigureTrajectoryDisplay fctd = new formConfigureTrajectoryDisplay(m_Metadata.Tracks[m_Metadata.SelectedTrack], m_SurfaceScreen);
			LocateForm(fctd);
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
			UpdateCurrentPositionInfos();
			trkSelection.SelPos = trkFrame.Position;
		}
		#endregion

		#region Chronometers Menus
		private void mnuChronoStart_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_Metadata, DrawingChrono.ChronoModificationType.TimeStart, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoStop_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_Metadata, DrawingChrono.ChronoModificationType.TimeStop, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoHide_Click(object sender, EventArgs e)
		{
			IUndoableCommand cmc = new CommandModifyChrono(this, m_Metadata, DrawingChrono.ChronoModificationType.TimeHide, m_iCurrentPosition);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
		}
		private void mnuChronoCountdown_Click(object sender, EventArgs e)
		{
			// This menu should only be accessible if we have a "Stop" value.
			mnuChronoCountdown.Checked = !mnuChronoCountdown.Checked;
			
			IUndoableCommand cmc = new CommandModifyChrono(this, m_Metadata, DrawingChrono.ChronoModificationType.Countdown, (mnuChronoCountdown.Checked == true)?1:0);
			CommandManager cm = CommandManager.Instance();
			cm.LaunchUndoableCommand(cmc);
			
			m_SurfaceScreen.Invalidate();
		}
		private void mnuChronoDelete_Click(object sender, EventArgs e)
		{
			IUndoableCommand cdc = new CommandDeleteChrono(this, m_Metadata);
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
			formConfigureChrono fcc = new formConfigureChrono(m_Metadata.Chronos[m_Metadata.SelectedChrono], m_SurfaceScreen);
			LocateForm(fcc);
			fcc.ShowDialog();
			fcc.Dispose();
			m_SurfaceScreen.Invalidate();

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
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifierDirect_Click(object sender, EventArgs e)
		{
			// Use position and magnification to Direct Zoom.
			// Go to direct zoom, at magnifier zoom factor, centered on same point as magnifier.
			RelocateDirectZoom(m_Magnifier.MagnifiedCenter, m_Magnifier.ZoomFactor);
			DisableMagnifier();
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifier150_Click(object sender, EventArgs e)
		{
			m_Magnifier.ZoomFactor = 1.5;
			UncheckMagnifierMenus();
			mnuMagnifier150.Checked = true;
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifier175_Click(object sender, EventArgs e)
		{
			m_Magnifier.ZoomFactor = 1.75;
			UncheckMagnifierMenus();
			mnuMagnifier175.Checked = true;
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifier200_Click(object sender, EventArgs e)
		{
			m_Magnifier.ZoomFactor = 2.0;
			UncheckMagnifierMenus();
			mnuMagnifier200.Checked = true;
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifier225_Click(object sender, EventArgs e)
		{
			m_Magnifier.ZoomFactor = 2.25;
			UncheckMagnifierMenus();
			mnuMagnifier225.Checked = true;
			m_SurfaceScreen.Invalidate();
		}
		private void mnuMagnifier250_Click(object sender, EventArgs e)
		{
			m_Magnifier.ZoomFactor = 2.5;
			UncheckMagnifierMenus();
			mnuMagnifier250.Checked = true;
			m_SurfaceScreen.Invalidate();
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

			if (m_Metadata.Plane.Selected)
			{
				m_Metadata.Plane.Selected = false;
				fcg = new formConfigureGrids(m_Metadata.Plane, m_SurfaceScreen);
				LocateForm(fcg);
				fcg.ShowDialog();
				fcg.Dispose();
			}
			else if (m_Metadata.Grid.Selected)
			{
				m_Metadata.Grid.Selected = false;
				fcg = new formConfigureGrids(m_Metadata.Grid, m_SurfaceScreen);
				LocateForm(fcg);
				fcg.ShowDialog();
				fcg.Dispose();
			}

			m_SurfaceScreen.Invalidate();
			
		}
		private void mnuGridsHide_Click(object sender, EventArgs e)
		{
			if (m_Metadata.Plane.Selected)
			{
				m_Metadata.Plane.Selected = false;
				m_Metadata.Plane.Visible = false;
			}
			else if (m_Metadata.Grid.Selected)
			{
				m_Metadata.Grid.Selected = false;
				m_Metadata.Grid.Visible = false;
			}

			m_SurfaceScreen.Invalidate();

			// Triggers an update to the menu.
			SetAsActiveScreen();
		}
		#endregion

		#region DirectZoom
		private void UnzoomDirectZoom()
		{
			SetupDirectZoom();
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).DirectZoomTopLeft = m_DirectZoomWindow.Location;
		}
		private void SetupDirectZoom()
		{
			m_fDirectZoomFactor = 1.0f;
			m_DirectZoomWindow = new Rectangle(0, 0, m_PlayerServer.m_InfosVideo.iDecodingWidth, m_PlayerServer.m_InfosVideo.iDecodingHeight);
		}
		private void IncreaseDirectZoom()
		{
			if (m_Magnifier.Mode != MagnifierMode.NotVisible)
			{
				DisableMagnifier();
			}

			// Max zoom : 600%
			if (m_fDirectZoomFactor < 6.0f)
			{
				m_fDirectZoomFactor += 0.20f;
				RelocateDirectZoom();
				_surfaceScreen.Invalidate();
			}
		}
		private void DecreaseDirectZoom()
		{
			if (m_fDirectZoomFactor > 1.0f)
			{
				m_fDirectZoomFactor -= 0.20f;
				RelocateDirectZoom();
				_surfaceScreen.Invalidate();
			}
		}
		private void RelocateDirectZoom()
		{
			RelocateDirectZoom(new Point(m_DirectZoomWindow.Left + (m_DirectZoomWindow.Width/2), m_DirectZoomWindow.Top + (m_DirectZoomWindow.Height/2)), m_fDirectZoomFactor);
		}
		private void RelocateDirectZoom(Point _Center, double _fZoomFactor)
		{
			m_fDirectZoomFactor = _fZoomFactor;

			int iNewWidth = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingWidth / m_fDirectZoomFactor);
			int iNewHeight = (int)((double)m_PlayerServer.m_InfosVideo.iDecodingHeight / m_fDirectZoomFactor);

			int iNewLeft = _Center.X - (iNewWidth / 2);
			int iNewTop = _Center.Y - (iNewHeight / 2);

			if (iNewLeft < 0) iNewLeft = 0;
			if (iNewLeft + iNewWidth >= m_PlayerServer.m_InfosVideo.iDecodingWidth) iNewLeft = m_PlayerServer.m_InfosVideo.iDecodingWidth - iNewWidth;

			if (iNewTop < 0) iNewTop = 0;
			if (iNewTop + iNewHeight >= m_PlayerServer.m_InfosVideo.iDecodingHeight) iNewTop = m_PlayerServer.m_InfosVideo.iDecodingHeight - iNewHeight;

			m_DirectZoomWindow = new Rectangle(iNewLeft, iNewTop, iNewWidth, iNewHeight);
			((DrawingToolPointer)m_DrawingTools[(int)DrawingToolType.Pointer]).DirectZoomTopLeft = m_DirectZoomWindow.Location;
		}
		#endregion

		#region VideoFilters Management
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
			buttonGotoFirst.Enabled = _bEnable;
			buttonGotoLast.Enabled = _bEnable;
			buttonGotoNext.Enabled = _bEnable;
			buttonGotoPrevious.Enabled = _bEnable;
			buttonPlay.Enabled = _bEnable;
			
			btnSetHandlerLeft.Enabled = _bEnable;
			btnSetHandlerRight.Enabled = _bEnable;
			btnHandlersReset.Enabled = _bEnable;
			btn_HandlersLock.Enabled = _bEnable;
			
			btnPdf.Enabled = _bEnable;
			btnRafale.Enabled = _bEnable;
			
			lblSpeedTuner.Enabled = _bEnable;
			trkFrame.Enabled = _bEnable;
			trkSelection.Enabled = _bEnable;
			sldrSpeed.Enabled = _bEnable;
			buttonPlayingMode.Enabled = _bEnable;
			btnDiaporama.Enabled = _bEnable;
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
		
		#region Keyboard Handling
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
						_surfaceScreen.Invalidate();
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
		#endregion

		#region Export video and frames
		private void btnSnapShot_Click(object sender, EventArgs e)
		{
			// Exporter l'image en cours.
			if ((m_PlayerServer.m_bIsMovieLoaded) && (m_PlayerServer.m_BmpImage != null))
			{
				StopPlaying();
				try
				{
					SaveFileDialog dlgSave = new SaveFileDialog();
					dlgSave.Title = m_ResourceManager.GetString("dlgSaveTitle", Thread.CurrentThread.CurrentUICulture);
					dlgSave.RestoreDirectory = true;
					dlgSave.Filter = m_ResourceManager.GetString("dlgSaveFilter", Thread.CurrentThread.CurrentUICulture);
					dlgSave.FilterIndex = 1;
					
					if(m_bDrawtimeFiltered && m_DrawingFilterOutput != null)
					{
						dlgSave.FileName = Path.GetFileNameWithoutExtension(m_FullPath);
					}
					else
					{
						dlgSave.FileName = BuildFilename(m_FullPath, m_iCurrentPosition, m_PrefManager.TimeCodeFormat);
					}
					
					if (dlgSave.ShowDialog() == DialogResult.OK)
					{

						// Reconstruct the extension.
						// If the user let "file.00.00" as a filename, the extension is not appended automatically.
						string strImgNameLower = dlgSave.FileName.ToLower();
						string extension;
						string strImgName;
						if (strImgNameLower.EndsWith("jpg") || strImgNameLower.EndsWith("jpeg") || strImgNameLower.EndsWith("bmp") || strImgNameLower.EndsWith("png"))
						{
							// Ok, the user added the extension himself or he did not use the preformatting.
							strImgName = dlgSave.FileName;
						}
						else
						{
							// Get the extension
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


						Bitmap outputImage = GetFlushedImage();
						
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
						else
						{
							// hum ?
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
			// 3. formFrameExport   : Progress holder and updater, calls:
			// 4. SaveImageSequence (below) to perform the real work. (saving the pics)
			//---------------------------------------------------------------------------------

			if ((m_PlayerServer.m_bIsMovieLoaded) && (m_PlayerServer.m_BmpImage != null))
			{
				StopPlaying();

				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}

				//throw new TypeLoadException();
				
				// Testing mosaic mode ! removeme.
				/*
				if(!m_Mosaic.Enabled)
				{
					m_Mosaic.Disable();
					m_Mosaic.MemoStretchMode = m_bStretchModeOn;
					bool bAnalysis = m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1;
					formConfigureMosaic fcm = new formConfigureMosaic(this, m_Mosaic, m_Metadata, bAnalysis, m_iSelDuration, m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds, m_PlayerServer.m_InfosVideo.fFps);
					if (fcm.ShowDialog() == DialogResult.OK)
					{
						if(m_Mosaic.Enabled)
						{
							DockKeyframePanel();
							m_bStretchModeOn = true;
							StretchSqueezeSurface();
							m_SurfaceScreen.Invalidate();
						}
					}
					fcm.Dispose();
				}
				else		
				{
					m_bStretchModeOn = m_Mosaic.MemoStretchMode;
					m_Mosaic.Disable();
					StretchSqueezeSurface();
					m_SurfaceScreen.Invalidate();
				}*/
				
				
				// Launch sequence saving configuration dialog
				formRafaleExport fre = new formRafaleExport(this, m_FullPath, m_iSelDuration, m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds, m_PlayerServer.m_InfosVideo.fFps);
				fre.ShowDialog();
				fre.Dispose();

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}
			}
		}
		private void btnDiaporama_Click(object sender, EventArgs e)
		{
			//-----------------------------------------------------------
			// Workflow:
			// 1. formDiapoExport   : configure the export parameters (interval), calls:
			// 2. FileSaveDialog    : chooses the filename, then
			// 3. SaveDiaporama (below), calls:
			// 4. formFileSave      : Progress bar and updater, calls:
			// 5. SaveMovie (PlayerServer) to perform the real work.
			//-----------------------------------------------------------
			if(m_Metadata.Keyframes.Count < 1)
			{
				MessageBox.Show(m_ResourceManager.GetString("Error_SaveDiaporama_NoKeyframes", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n"),
				                m_ResourceManager.GetString("Error_SaveDiaporama", Thread.CurrentThread.CurrentUICulture),
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);
			}
			else if ((m_PlayerServer.m_bIsMovieLoaded) && (m_PlayerServer.m_BmpImage != null))
			{
				StopPlaying();

				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
				{
					dp.DeactivateKeyboardHandler();
				}

				// Launch sequence saving configuration dialog
				formDiapoExport fde = new formDiapoExport(this, m_FullPath);
				fde.ShowDialog();
				fde.Dispose();

				if (dp.ActivateKeyboardHandler != null)
				{
					dp.ActivateKeyboardHandler();
				}
			}
		}
		private void btnPdf_Click(object sender, EventArgs e)
		{
			if (m_Metadata.Keyframes.Count < 1)
			{
				MessageBox.Show(m_ResourceManager.GetString("Error_SavePdf_NoKeyFrames", Thread.CurrentThread.CurrentUICulture).Replace("\\n", "\n"),
				                m_ResourceManager.GetString("Error_SavePdf", Thread.CurrentThread.CurrentUICulture),
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Exclamation);
			}
			else if ((m_PlayerServer.m_bIsMovieLoaded) && (m_PlayerServer.m_BmpImage != null))
			{
				StopPlaying();

				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Title = m_ResourceManager.GetString("dlgExportToPDF_Title", Thread.CurrentThread.CurrentUICulture);
				saveFileDialog.RestoreDirectory = true;
				saveFileDialog.Filter = m_ResourceManager.GetString("dlgExportToPDF_Filter", Thread.CurrentThread.CurrentUICulture);
				saveFileDialog.FilterIndex = 1;
				saveFileDialog.FileName = Path.GetFileNameWithoutExtension(m_Metadata.FullPath);

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					string filePath = saveFileDialog.FileName;
					if (filePath.Length > 0)
					{
						if (!filePath.ToLower().EndsWith(".pdf"))
						{
							filePath = filePath + ".pdf";
						}
						AnalysisExporterPDF aepdf = new AnalysisExporterPDF();
						aepdf.Export(filePath, m_Metadata);
					}
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
				int iTotal = m_Metadata.Keyframes.Count;
				foreach(Keyframe kf in m_Metadata.Keyframes)
				{
					if (kf.Position >= m_iSelStart && kf.Position <= m_iSelEnd)
					{
						// Build the file name
						string fileName = Path.GetDirectoryName(_FilePath) + "\\" + BuildFilename(_FilePath, kf.Position, m_PrefManager.TimeCodeFormat) + Path.GetExtension(_FilePath);

						// Get the image
						Size iNewSize = new Size((int)((double)kf.FullFrame.Width * m_fStretchFactor), (int)((double)kf.FullFrame.Height * m_fStretchFactor));
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

					Size iNewSize = new Size((int)((double)m_PlayerServer.m_BmpImage.Width * m_fStretchFactor), (int)((double)m_PlayerServer.m_BmpImage.Height * m_fStretchFactor));
					Bitmap outputImage = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
					outputImage.SetResolution(m_PlayerServer.m_BmpImage.HorizontalResolution, m_PlayerServer.m_BmpImage.VerticalResolution);
					Graphics g = Graphics.FromImage(outputImage);

					if (_bBlendDrawings)
					{
						int iKeyFrameIndex = -1;
						if (m_iActiveKeyFrameIndex >= 0 && m_Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
						{
							iKeyFrameIndex = m_iActiveKeyFrameIndex;
						}

						FlushOnGraphics(m_PlayerServer.m_BmpImage, g, iNewSize, iKeyFrameIndex, m_iCurrentPosition);
					}
					else
					{
						// image only.
						g.DrawImage(m_PlayerServer.m_BmpImage, 0, 0, iNewSize.Width, iNewSize.Height);
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
				//UpdateNavigationCursor();
				ActivateKeyframe(m_iCurrentPosition, false);
			}

			m_SurfaceScreen.Invalidate();
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
		public void SaveMovie(String _filePath, bool _bVideoAlone, bool _bChangeFramerate, bool _bFlushDrawings)
		{
			// Called from the dialog box "FormVideoExport".

			formFileSave ffs;

			// framerate
			int iFrameInterval;
			if(_bChangeFramerate)
			{
				iFrameInterval = GetFrameInterval();
			}
			else
			{
				iFrameInterval = m_PlayerServer.m_InfosVideo.iFrameInterval;
			}

			// On lui passe un pointeur de fonction
			// variable sur delegate dont le type est déclaré dans m_playerServer
			//DelegateFlushDrawings dfd = FlushDrawings;
			DelegateGetOutputBitmap dgob = GetOutputBitmap;
			
			// video alone or metadata muxed along
			if (_bVideoAlone)
			{
				ffs = new formFileSave(m_PlayerServer, _filePath, iFrameInterval, m_iSelStart, m_iSelEnd, null, _bFlushDrawings, false, dgob);
			}
			else
			{
				ffs = new formFileSave(m_PlayerServer, _filePath, iFrameInterval, m_iSelStart, m_iSelEnd, m_Metadata, _bFlushDrawings, false, dgob);
			}

			// Launch transcoding by showing the progress bar dialog.
			ffs.ShowDialog();
			ffs.Dispose();
		}
		public void SaveDiaporama(String _filePath, int _iFrameInterval)
		{
			// Called from the dialog box "FormDiapoExport".
			
			//DelegateFlushDrawings dfd = FlushDrawings;
			DelegateGetOutputBitmap dgob = GetOutputBitmap;
			
			formFileSave ffs = new formFileSave(m_PlayerServer, _filePath, _iFrameInterval, m_iSelStart, m_iSelEnd, null, true, true, dgob);
			ffs.ShowDialog();
			ffs.Dispose();
		}
		private bool GetOutputBitmap(Graphics _canvas, long _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly)
		{
			// Used by the PlayerServer for SaveMovie.
			// The image to save was already retrieved (from stream or analysis array)
			// This image is already drawn on _canvas.

			bool bShouldEncode = false;

			int iKeyFrameIndex = -1;
			int iCurrentKeyframe = 0;
			bool bFound = false;
			while (!bFound && iCurrentKeyframe < m_Metadata.Count)
			{
				if (m_Metadata[iCurrentKeyframe].Position == _iTimestamp)
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
				bShouldEncode = true;
				
				if (_bFlushDrawings)
				{
					FlushDrawingsOnGraphics(_canvas, iKeyFrameIndex, _iTimestamp, 1.0f, 1.0f, new Point(0,0));
				}
			}

			return bShouldEncode;
		}
		private Bitmap GetFlushedImage()
		{
			// Returns an image with all drawings flushed, including
			// grids, chronos, magnifier, etc.
			// image should be at same strech factor than the one visible on screen.

			Size iNewSize = new Size((int)((double)m_PlayerServer.m_BmpImage.Width * m_fStretchFactor), (int)((double)m_PlayerServer.m_BmpImage.Height * m_fStretchFactor));
			Bitmap output = new Bitmap(iNewSize.Width, iNewSize.Height, PixelFormat.Format24bppRgb);
			output.SetResolution(m_PlayerServer.m_BmpImage.HorizontalResolution, m_PlayerServer.m_BmpImage.VerticalResolution);

			if(m_bDrawtimeFiltered && m_DrawingFilterOutput.Draw != null)
			{
				m_DrawingFilterOutput.Draw(Graphics.FromImage(output), iNewSize, m_DrawingFilterOutput.InputFrames, m_DrawingFilterOutput.PrivateData);
			}
			else
			{	
				int iKeyFrameIndex = -1;
				if (m_iActiveKeyFrameIndex >= 0 && m_Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
				{
					iKeyFrameIndex = m_iActiveKeyFrameIndex;
				}
	
				FlushOnGraphics(m_PlayerServer.m_BmpImage, Graphics.FromImage(output), iNewSize, iKeyFrameIndex, m_iCurrentPosition);
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

				//We'll store images at 90% quality as compared with the original
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
		private Bitmap ConvertToPNG(Bitmap _image)
		{
			//---------------------------------------------
			// NOT USED.
			// Doesn't seem to have any effect anyway.
			// TODO: find a way to make png files smaller ?
			//---------------------------------------------

			// Intermediate MemoryStream for the conversion.
			MemoryStream memStr = new MemoryStream();

			//Get the list of available encoders
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			//find the encoder with the image/jpeg mime-type
			ImageCodecInfo ici = null;
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.MimeType == "image/png")
				{
					ici = codec;
				}
			}

			if (ici != null)
			{
				//Create a collection of encoder parameters (we only need one in the collection)
				EncoderParameters ep = new EncoderParameters();

				//We'll store images at 90% quality as compared with the original
				ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)75);

				_image.Save(memStr, ici, ep);
			}
			else
			{
				// No encoder found (is that common ?) Use default system.
				_image.Save(memStr, ImageFormat.Png);
			}

			return new Bitmap(memStr);
		}
		#endregion

		#region Analysis mode
		public void SwitchToAnalysisMode(bool _bForceReload)
		{
			//------------------------------------------------------------------------
			// Switcher la selection courante si possible.
			// Appelé au chargement, une fois que tout est ok et la première frame ok.
			// Appelé sur modification de la selection par l'utilisateur.
			//------------------------------------------------------------------------
			if (m_PlayerServer.m_bIsMovieLoaded)
			{
				if (IsSelectionAnalyzable())
				{
					formFramesImport ffi = new formFramesImport(m_ResourceManager, m_PlayerServer, trkSelection.SelStart, trkSelection.SelEnd, _bForceReload);
					ffi.ShowDialog();
					ffi.Dispose();
				}
				else if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1)
				{
					// Exiting Analysis mode.
					// TODO - free memory for images now ?
					m_PlayerServer.m_PrimarySelection.iAnalysisMode = 0;
				}

				// Ici on a éventuellement changé de mode.
				if (m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1)
				{
					// We now have solid facts. Update all variables with them.
					m_iSelStart = m_PlayerServer.GetTimeStamp(0);
					m_iSelEnd = m_PlayerServer.GetTimeStamp(m_PlayerServer.m_PrimarySelection.iDurationFrame - 1);
					double fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
					m_iSelDuration = (long)((double)(m_iSelEnd - m_iSelStart) + fAverageTimeStampsPerFrame);

					// Remapper le frame tracker - Utilisation des données réelles.
					trkFrame.Minimum = m_iSelStart;
					trkFrame.Maximum = m_iSelEnd;
					trkFrame.ReportOnMouseMove = true;

					// Afficher la première image.
					m_iFramesToDecode = 1;
					ShowNextFrame(m_iSelStart, true);
					UpdateNavigationCursor();
				}
				else
				{
					m_iSelStart = trkSelection.SelStart;
					// Hack : If we changed the trkSelection.SelEnd before the trkSelection.SelStart
					// (As we do when we first load the video), the selstart will not take into account
					// a possible shift of unreadable first frames.
					// We make the ad-hoc modif here.
					if (m_iSelStart < m_iStartingPosition) m_iSelStart = m_iStartingPosition;

					m_iSelEnd = trkSelection.SelEnd;

					double fAverageTimeStampsPerFrame = m_PlayerServer.m_InfosVideo.fAverageTimeStampsPerSeconds / m_PlayerServer.m_InfosVideo.fFps;
					m_iSelDuration = (long)((double)(m_iSelEnd - m_iSelStart) + fAverageTimeStampsPerFrame);

					// Remapper le FrameTracker
					trkFrame.Minimum = m_iSelStart;
					trkFrame.Maximum = m_iSelEnd;
					trkFrame.ReportOnMouseMove = false;
				}

				UpdatePrimarySelectionPanelInfos();
				SetAsActiveScreen();

				if (m_ReportSelectionChanged != null) { m_ReportSelectionChanged(true); }
				if (m_bShowInfos) { UpdateDebugInfos(); }
			}
		}
		private bool IsSelectionAnalyzable()
		{
			// Check the current selection against the preferences
			return m_PlayerServer.IsSelectionAnalyzable(trkSelection.SelStart, trkSelection.SelEnd, m_PrefManager.WorkingZoneSeconds, m_PrefManager.WorkingZoneMemory);
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
			SwitchToAnalysisMode(true);
			UpdateKeyframes();

			// Reset to the current selection.
			m_iSelStart = mps.SelStart;
			m_iSelEnd = mps.SelEnd;
		}
		#endregion

	}
}
