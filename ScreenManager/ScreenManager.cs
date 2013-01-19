#region License
/*
Copyright © Joan Charmant 2008.
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
    public class ScreenManagerKernel : IKernel, IScreenHandler, IScreenManagerUIContainer, IMessageFilter
    {
        #region Properties
        public UserControl UI
        {
            get { return (UserControl)view; }
        }
        public ScreenManagerUserInterface View
        {
            get { return view;}
        }
        public ResourceManager resManager
        {
            get { return new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly()); }
        }
        public bool CancelLastCommand
        {
            get { return m_bCancelLastCommand; } // Unused.
            set { m_bCancelLastCommand = value; }
        }
        #endregion

        #region Members
        private ScreenManagerUserInterface view;
        private bool m_bCancelLastCommand;			// true when a RemoveScreen command was canceled by user.

        //List of screens ( 0..n )
        public List<AbstractScreen> screenList = new List<AbstractScreen>();
        public AbstractScreen m_ActiveScreen = null;
        private bool canShowCommonControls;
        
        // Dual saving
        private string m_DualSaveFileName;
        private bool m_bDualSaveCancelled;
        private bool m_bDualSaveInProgress;
        private VideoFileWriter m_VideoFileWriter = new VideoFileWriter();
        private BackgroundWorker m_bgWorkerDualSave;
        private formProgressBar m_DualSaveProgressBar;

        // Video Filters
        private bool m_bHasSvgFiles;
        private string m_SvgPath;
        private FileSystemWatcher m_SVGFilesWatcher = new FileSystemWatcher();
        private bool m_BuildingSVGMenu;
        private List<ToolStripMenuItem> m_filterMenus = new List<ToolStripMenuItem>();
        
        #region Menus
        private ToolStripMenuItem mnuCloseFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseFile2 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSave = new ToolStripMenuItem();
 
        private ToolStripMenuItem mnuExportSpreadsheet = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportODF = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportMSXML = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportXHTML = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportTEXT = new ToolStripMenuItem();
		private ToolStripMenuItem mnuLoadAnalysis = new ToolStripMenuItem();

	    private ToolStripMenuItem mnuOnePlayer = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoPlayers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOneCapture = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoCaptures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoMixed = new ToolStripMenuItem();
        
		private ToolStripMenuItem mnuSwapScreens = new ToolStripMenuItem();
        private ToolStripMenuItem mnuToggleCommonCtrls = new ToolStripMenuItem();

        private ToolStripMenuItem mnuDeinterlace = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormat = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatAuto = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatForce43 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatForce169 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMirror = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSVGTools = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCoordinateAxis = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuHighspeedCamera = new ToolStripMenuItem();
        #endregion

        #region Toolbar
        private ToolStripButton toolHome = new ToolStripButton();
        private ToolStripButton toolSave = new ToolStripButton();
        private ToolStripButton toolOnePlayer = new ToolStripButton();
        private ToolStripButton toolTwoPlayers = new ToolStripButton();
        private ToolStripButton toolOneCapture = new ToolStripButton();
        private ToolStripButton toolTwoCaptures = new ToolStripButton();
        private ToolStripButton toolTwoMixed = new ToolStripButton();
        #endregion
        
        #region Synchronization
        private bool    m_bSynching;
        private bool 	m_bSyncMerging;				// true if blending each other videos. 
        private long    m_iSyncLag; 	            // Sync Lag in Frames, for static sync.
        private long     m_iSyncLagMilliseconds;		// Sync lag in Milliseconds, for dynamic sync.
        private bool 	m_bDynamicSynching;			// replace the common timer.
        
        // Static Sync Positions
        private long m_iCurrentFrame = 0;            // Current frame in trkFrame...
        private long m_iLeftSyncFrame = 0;           // Sync reference in the left video
        private long m_iRightSyncFrame = 0;          // Sync reference in the right video
        private long m_iMaxFrame = 0;                // Max du trkFrame

        // Dynamic Sync Flags.
        private bool m_bRightIsStarting = false;    // true when the video is between [0] and [1] frames.
        private bool m_bLeftIsStarting = false;
        private bool m_bLeftIsCatchingUp = false;   // CatchingUp is when the video is the only one left running,
        private bool m_bRightIsCatchingUp = false;  // heading towards end, the other video is waiting the lag.

        #endregion

        private bool m_bAllowKeyboardHandler;

        private List<ScreenManagerState> m_StoredStates  = new List<ScreenManagerState>();
        private const int WM_KEYDOWN = 0x0100;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & initialization
        public ScreenManagerKernel()
        {
            log.Debug("Module Construction : ScreenManager.");

            m_bAllowKeyboardHandler = true;

            view = new ScreenManagerUserInterface(this);
            view.LoadAsked += View_LoadAsked;
            
            view.thumbnailsViewer.LoadAsked += View_LoadAsked;
            
            InitializeVideoFilters();
            
            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();

            dp.LoadMovieInScreen = DoLoadMovieInScreen;
            dp.StopPlaying = DoStopPlaying;
            dp.DeactivateKeyboardHandler = DoDeactivateKeyboardHandler;
            dp.ActivateKeyboardHandler = DoActivateKeyboardHandler;
            
            // Watch for changes in the guides directory.
            m_SvgPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\guides\\";
            m_SVGFilesWatcher.Path = m_SvgPath;
            m_SVGFilesWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
        	m_SVGFilesWatcher.Filter = "*.svg";
        	m_SVGFilesWatcher.IncludeSubdirectories = true;
        	m_SVGFilesWatcher.EnableRaisingEvents = true;
        	
        	m_SVGFilesWatcher.Changed += OnSVGFilesChanged;
        	m_SVGFilesWatcher.Created += OnSVGFilesChanged;
        	m_SVGFilesWatcher.Deleted += OnSVGFilesChanged;
        	m_SVGFilesWatcher.Renamed += OnSVGFilesChanged;
        	
        }
        private void InitializeVideoFilters()
        {
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterAutoLevels()));
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterContrast()));
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterSharpen()));
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterEdgesOnly()));
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterMosaic()));
            m_filterMenus.Add(CreateFilterMenu(new VideoFilterReverse()));
            //m_filterMenus.Add(CreateFilterMenu(new VideoFilterSandbox()));
        }
        private ToolStripMenuItem CreateFilterMenu(AbstractVideoFilter _filter)
        {
            // TODO: test if we can directly use a copy of the argument in the closure.
            // would avoid passing through .Tag and multiple casts.
            ToolStripMenuItem menu = new ToolStripMenuItem(_filter.Name, _filter.Icon);
            menu.MergeAction = MergeAction.Append;
            menu.Tag = _filter;
            menu.Click += (s,e) => {
                PlayerScreen screen = m_ActiveScreen as PlayerScreen;
                if(screen == null || !screen.IsCaching)
                    return;
                AbstractVideoFilter filter = (AbstractVideoFilter)((ToolStripMenuItem)s).Tag;
                filter.Activate(screen.FrameServer.VideoReader.WorkingZoneFrames, SetInteractiveEffect);
                screen.RefreshImage();
            };
            return menu;
        }
        public void SetInteractiveEffect(InteractiveEffect _effect)
        {
            PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(player != null)
        		player.SetInteractiveEffect(_effect);
        }
        public void PrepareScreen()
        {
        	// Prepare a screen to hold the command line argument file.
        	IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
            CommandManager.Instance().LaunchUndoableCommand(caps);
            
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            OrganizeCommonControls();
            OrganizeMenus();
        }
        public void Prepare()
        {
        	Application.AddMessageFilter(this);
        }
        #endregion

        #region IKernel Implementation
        public void BuildSubTree()
        {
            // No sub modules.
        }
        public void ExtendMenu(ToolStrip _menu)
        {
            #region File
            ToolStripMenuItem mnuCatchFile = new ToolStripMenuItem();
            mnuCatchFile.MergeIndex = 0; // (File)
            mnuCatchFile.MergeAction = MergeAction.MatchOnly;

            mnuCloseFile.Image = Properties.Resources.film_close3;
            mnuCloseFile.Enabled = false;
            mnuCloseFile.Click += new EventHandler(mnuCloseFileOnClick);
            mnuCloseFile.MergeIndex = 2;
            mnuCloseFile.MergeAction = MergeAction.Insert;

            mnuCloseFile2.Image = Properties.Resources.film_close3;
            mnuCloseFile2.Enabled = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Click += new EventHandler(mnuCloseFile2OnClick);
            mnuCloseFile2.MergeIndex = 3;
            mnuCloseFile2.MergeAction = MergeAction.Insert;

            mnuSave.Image = Properties.Resources.filesave;
            mnuSave.Click += new EventHandler(mnuSaveOnClick);
            mnuSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuSave.MergeIndex = 5;
            mnuSave.MergeAction = MergeAction.Insert;

            mnuExportSpreadsheet.Image = Properties.Resources.table;
            mnuExportSpreadsheet.MergeIndex = 6;
            mnuExportSpreadsheet.MergeAction = MergeAction.Insert;
            mnuExportODF.Image = Properties.Resources.file_ods;
            mnuExportODF.Click += new EventHandler(mnuExportODF_OnClick);
            mnuExportMSXML.Image = Properties.Resources.file_xls;
            mnuExportMSXML.Click += new EventHandler(mnuExportMSXML_OnClick);
            mnuExportXHTML.Image = Properties.Resources.file_html;
            mnuExportXHTML.Click += new EventHandler(mnuExportXHTML_OnClick);
            mnuExportTEXT.Image = Properties.Resources.file_txt;
            mnuExportTEXT.Click += new EventHandler(mnuExportTEXT_OnClick);
            
            mnuExportSpreadsheet.DropDownItems.AddRange(new ToolStripItem[] { mnuExportODF, mnuExportMSXML, mnuExportXHTML, mnuExportTEXT });
            
            // Load Analysis
            mnuLoadAnalysis.Image = Properties.Resources.file_kva2;
            mnuLoadAnalysis.Click += new EventHandler(mnuLoadAnalysisOnClick);
            mnuLoadAnalysis.MergeIndex = 7;
            mnuLoadAnalysis.MergeAction = MergeAction.Insert;

            ToolStripItem[] subFile = new ToolStripItem[] { mnuCloseFile, mnuCloseFile2, mnuSave, mnuExportSpreadsheet, mnuLoadAnalysis };
            mnuCatchFile.DropDownItems.AddRange(subFile);
            #endregion

            #region View
            ToolStripMenuItem mnuCatchScreens = new ToolStripMenuItem();
            mnuCatchScreens.MergeIndex = 2; // (Screens)
            mnuCatchScreens.MergeAction = MergeAction.MatchOnly;

            mnuOnePlayer.Image = Properties.Resources.television;
            mnuOnePlayer.Click += new EventHandler(mnuOnePlayerOnClick);
            mnuOnePlayer.MergeAction = MergeAction.Append;
            mnuTwoPlayers.Image = Properties.Resources.dualplayback;
            mnuTwoPlayers.Click += new EventHandler(mnuTwoPlayersOnClick);
            mnuTwoPlayers.MergeAction = MergeAction.Append;
            mnuOneCapture.Image = Properties.Resources.camera_video;
            mnuOneCapture.Click += new EventHandler(mnuOneCaptureOnClick);
            mnuOneCapture.MergeAction = MergeAction.Append;
            mnuTwoCaptures.Image = Properties.Resources.dualcapture2;
            mnuTwoCaptures.Click += new EventHandler(mnuTwoCapturesOnClick);
            mnuTwoCaptures.MergeAction = MergeAction.Append;
			mnuTwoMixed.Image = Properties.Resources.dualmixed3;
            mnuTwoMixed.Click += new EventHandler(mnuTwoMixedOnClick);
            mnuTwoMixed.MergeAction = MergeAction.Append;
                        
            mnuSwapScreens.Image = Properties.Resources.arrow_swap;
            mnuSwapScreens.Enabled = false;
            mnuSwapScreens.Click += new EventHandler(mnuSwapScreensOnClick);
            mnuSwapScreens.MergeAction = MergeAction.Append;
            
            mnuToggleCommonCtrls.Image = Properties.Resources.common_controls;
            mnuToggleCommonCtrls.Enabled = false;
            mnuToggleCommonCtrls.ShortcutKeys = Keys.F5;
            mnuToggleCommonCtrls.Click += new EventHandler(mnuToggleCommonCtrlsOnClick);
            mnuToggleCommonCtrls.MergeAction = MergeAction.Append;
            
            ToolStripItem[] subScreens = new ToolStripItem[] { 		mnuOnePlayer,
            														mnuTwoPlayers,
            														new ToolStripSeparator(),
            														mnuOneCapture, 
            														mnuTwoCaptures,
            														new ToolStripSeparator(),
            														mnuTwoMixed, 
            														new ToolStripSeparator(), 
            														mnuSwapScreens, 
            														mnuToggleCommonCtrls };
            mnuCatchScreens.DropDownItems.AddRange(subScreens);
            #endregion

            #region Image
            ToolStripMenuItem mnuCatchImage = new ToolStripMenuItem();
            mnuCatchImage.MergeIndex = 3; // (Image)
            mnuCatchImage.MergeAction = MergeAction.MatchOnly;
            
            mnuDeinterlace.Image = Properties.Resources.deinterlace;
            mnuDeinterlace.Checked = false;
            mnuDeinterlace.ShortcutKeys = Keys.Control | Keys.D;
            mnuDeinterlace.Click += new EventHandler(mnuDeinterlaceOnClick);
            mnuDeinterlace.MergeAction = MergeAction.Append;
            
            mnuFormatAuto.Checked = true;
            mnuFormatAuto.Click += new EventHandler(mnuFormatAutoOnClick);
            mnuFormatAuto.MergeAction = MergeAction.Append;
            mnuFormatForce43.Image = Properties.Resources.format43;
            mnuFormatForce43.Click += new EventHandler(mnuFormatForce43OnClick);
            mnuFormatForce43.MergeAction = MergeAction.Append;
            mnuFormatForce169.Image = Properties.Resources.format169;
            mnuFormatForce169.Click += new EventHandler(mnuFormatForce169OnClick);
            mnuFormatForce169.MergeAction = MergeAction.Append;
            mnuFormat.Image = Properties.Resources.shape_formats;
            mnuFormat.MergeAction = MergeAction.Append;
            mnuFormat.DropDownItems.AddRange(new ToolStripItem[] { mnuFormatAuto, new ToolStripSeparator(), mnuFormatForce43, mnuFormatForce169});
                        
            mnuMirror.Image = Properties.Resources.shape_mirror;
            mnuMirror.Checked = false;
            mnuMirror.ShortcutKeys = Keys.Control | Keys.M;
            mnuMirror.Click += new EventHandler(mnuMirrorOnClick);
            mnuMirror.MergeAction = MergeAction.Append;

            BuildSvgMenu();
            
            mnuCoordinateAxis.Image = Properties.Resources.coordinate_axis;
            mnuCoordinateAxis.Click += new EventHandler(mnuCoordinateAxis_OnClick);
            mnuCoordinateAxis.MergeAction = MergeAction.Append;

            ConfigureVideoFilterMenus(null);

            mnuCatchImage.DropDownItems.Add(mnuDeinterlace);
            mnuCatchImage.DropDownItems.Add(mnuFormat);
            mnuCatchImage.DropDownItems.Add(mnuMirror);
            mnuCatchImage.DropDownItems.Add(new ToolStripSeparator());
            
            // Temporary hack for including filters sub menus until a full plugin system is in place.
            // We just check on their type. Ultimately each plugin will have a category or a submenu property.
            foreach(ToolStripMenuItem menu in m_filterMenus)
            {
                if(menu.Tag is AdjustmentFilter)
                    mnuCatchImage.DropDownItems.Add(menu);
            }
            
            mnuCatchImage.DropDownItems.Add(new ToolStripSeparator());
            mnuCatchImage.DropDownItems.Add(mnuSVGTools);
            mnuCatchImage.DropDownItems.Add(mnuCoordinateAxis);
            #endregion

            #region Motion
            ToolStripMenuItem mnuCatchMotion = new ToolStripMenuItem();
            mnuCatchMotion.MergeIndex = 4;
            mnuCatchMotion.MergeAction = MergeAction.MatchOnly;

            mnuHighspeedCamera.Image = Properties.Resources.camera_speed;
            mnuHighspeedCamera.Click += new EventHandler(mnuHighspeedCamera_OnClick);
            mnuHighspeedCamera.MergeAction = MergeAction.Append;
            
            mnuCatchMotion.DropDownItems.Add(mnuHighspeedCamera);
            mnuCatchMotion.DropDownItems.Add(new ToolStripSeparator());
            foreach(ToolStripMenuItem menu in m_filterMenus)
            {
                if(!(menu.Tag is AdjustmentFilter))
                    mnuCatchMotion.DropDownItems.Add(menu);
            }
            #endregion
            
            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchFile, mnuCatchScreens, mnuCatchImage, mnuCatchMotion });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, _menu);

            RefreshCultureMenu();
        }
        
        public void ExtendToolBar(ToolStrip _toolbar)
        {
        	// Save
			toolSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolSave.Image = Properties.Resources.filesave;
            toolSave.Click += new EventHandler(mnuSaveOnClick);
        	
        	// Workspace presets.
        	
        	toolHome.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolHome.Image = Properties.Resources.home3;
            toolHome.Click += new EventHandler(mnuHome_OnClick);
        	
            toolOnePlayer.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOnePlayer.Image = Properties.Resources.television;
            toolOnePlayer.Click += new EventHandler(mnuOnePlayerOnClick);
            
            toolTwoPlayers.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoPlayers.Image = Properties.Resources.dualplayback;
            toolTwoPlayers.Click += new EventHandler(mnuTwoPlayersOnClick);
            
            toolOneCapture.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOneCapture.Image = Properties.Resources.camera_video;
            toolOneCapture.Click += new EventHandler(mnuOneCaptureOnClick);
            
            toolTwoCaptures.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoCaptures.Image = Properties.Resources.dualcapture2;
            toolTwoCaptures.Click += new EventHandler(mnuTwoCapturesOnClick);
            
            toolTwoMixed.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoMixed.Image = Properties.Resources.dualmixed3;
            toolTwoMixed.Click += new EventHandler(mnuTwoMixedOnClick);
            
            ToolStrip ts = new ToolStrip(new ToolStripItem[] { 
                                         	toolSave,
                                         	new ToolStripSeparator(),
                                         	toolHome,
                                         	new ToolStripSeparator(),
                                         	toolOnePlayer,
                                         	toolTwoPlayers,
                                         	new ToolStripSeparator(),
                                         	toolOneCapture, 
                                         	toolTwoCaptures, 
                                         	new ToolStripSeparator(),
                                         	toolTwoMixed });
            
            ToolStripManager.Merge(ts, _toolbar);
            
        }
        public void ExtendStatusBar(ToolStrip _statusbar)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendUI()
        {
            // No sub modules.
        }
        public void RefreshUICulture()
        {
            RefreshCultureMenu();
            OrganizeMenus();
            RefreshCultureToolbar();
            UpdateStatusBar();
            view.RefreshUICulture();

            foreach (AbstractScreen screen in screenList)
                screen.refreshUICulture();
        }
        public bool CloseSubModules()
        {
            for(int i = screenList.Count - 1; i >= 0; i--)
            {
                screenList[i].BeforeClose();
                CloseFile(i);
                UpdateCaptureBuffers();
                PrepareSync(false);
            }
            
            return screenList.Count != 0;
        }
        #endregion
        
        #region IScreenHandler Implementation
        public void Screen_SetActiveScreen(AbstractScreen _ActiveScreen)
        {
            //-------------------------------------------------------------
        	// /!\ Calls in OrganizeMenu which is a bit heavy on the UI.
            // Screen_SetActiveScreen should only be called when necessary.
			//-------------------------------------------------------------
            
			if (m_ActiveScreen != _ActiveScreen )
            {
                m_ActiveScreen = _ActiveScreen;
                
                if (screenList.Count > 1)
                {
                    m_ActiveScreen.DisplayAsActiveScreen(true);

                    // Make other screens inactive.
                    foreach (AbstractScreen screen in screenList)
                    {
                        if (screen != _ActiveScreen)
                        {
                            screen.DisplayAsActiveScreen(false);
                        }
                    }
                }
            }

            OrganizeMenus();
        }
        public void Screen_CloseAsked(AbstractScreen _sender)
        {
        	// If the screen is in Drawtime filter (e.g: Mosaic), we just go back to normal play.
        	if(_sender is PlayerScreen && ((PlayerScreen)_sender).InteractiveFiltering)
        	{
        	    Screen_SetActiveScreen(_sender);
        	    ((PlayerScreen)_sender).DeactivateInteractiveEffect();
        	    return;
        	}
        	
            _sender.BeforeClose();
            
            // Reorganise screens.
            // We leverage the fact that screens are always well ordered relative to menus.
            if (screenList.Count > 0 && _sender == screenList[0])
                CloseFile(0);
            else
                CloseFile(1);
            
            UpdateCaptureBuffers();
            PrepareSync(false);
        }
        public void Screen_UpdateStatusBarAsked(AbstractScreen _SenderScreen)
        {
        	UpdateStatusBar();
        }
        public void Player_SpeedChanged(PlayerScreen _screen, bool _bInitialisation)
        {
            if (!m_bSynching || screenList.Count != 2)
                return;
            
            if(PreferencesManager.PlayerPreferences.SyncLockSpeed)
            {
                int otherScreen = _screen == screenList[0] ? 1 : 0;
                ((PlayerScreen)screenList[otherScreen]).RealtimePercentage = ((PlayerScreen)_screen).RealtimePercentage;
            }
            
            SetSyncPoint(true);
        }
        public void Player_PauseAsked(PlayerScreen _screen)
        {
        	// An individual player asks for a global pause.
        	if (m_bSynching && view.CommonPlaying)
            {
        		view.CommonPlaying = false;
        		CommonCtrl_Play();
        	}
        }
        public void Player_SelectionChanged(PlayerScreen _screen, bool _bInitialization)
        {
        	PrepareSync(_bInitialization);
        }
        public void Player_ImageChanged(PlayerScreen _screen, Bitmap _image)
        {
            if (!m_bSynching)
                return;

            if(m_bDynamicSynching)
                DynamicSync();
            
            // Transfer the caller's image to the other screen.
            // The image has been cloned and transformed in the caller screen.
            if(m_bSyncMerging && _image != null)
            {
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen != _screen && screen is PlayerScreen)
                        ((PlayerScreen)screen).SetSyncMergeImage(_image, !m_bDualSaveInProgress);
                }
            }
        }
        public void Player_SendImage(PlayerScreen _screen, Bitmap _image)
        {
        	// An image was sent from a screen to be added as an observational reference in the other screen.
        	for(int i=0;i<screenList.Count;i++)
            {
        		if (screenList[i] != _screen && screenList[i] is PlayerScreen)
                {
                	// The image has been cloned and transformed in the caller screen.
                	screenList[i].AddImageDrawing(_image);
                }
            }			
        }
        public void Player_Reset(PlayerScreen _screen)
        {
        	// A screen was reset. (ex: a video was reloded in place).
        	// We need to also reset all the sync states.
        	PrepareSync(true);
        }
        public void Capture_FileSaved(CaptureScreen _screen)
        {
        	// A file was saved in one screen, we need to update the text on the other.
        	for(int i=0;i<screenList.Count;i++)
            {
        		if (screenList[i] != _screen && screenList[i] is CaptureScreen)
                {
                	screenList[i].refreshUICulture();
                }
            }
        }
        public void Capture_LoadVideo(CaptureScreen _screen, string _filepath)
        {
        	// Launch a video in the other screen.
        	
        	if(screenList.Count == 1)
        	{
                // Create the screen if necessary.
                // The buffer of the capture screen will be reset during the operation.
        		DoLoadMovieInScreen(_filepath, -1, true);
        	}
        	else if(screenList.Count == 2)
        	{
        		// Identify the other screen.
        		AbstractScreen otherScreen = null;
        		int iOtherScreenIndex = 0;
        		for(int i=0;i<screenList.Count;i++)
            	{
        			if (screenList[i] != _screen)
                	{
        				otherScreen = screenList[i];
        				iOtherScreenIndex = i+1;
                	}
            	}
        		
        		if(otherScreen is CaptureScreen)
        		{
        			// Unload capture screen to play the video ?
        		}
        		else if(otherScreen is PlayerScreen)
        		{
        			// Replace the video.
        			DoLoadMovieInScreen(_filepath, iOtherScreenIndex, true);
        		}
        	}
        }
        #endregion
        
        #region ICommonControlsHandler Implementation
        public void View_LoadAsked(object source, LoadAskedEventArgs e)
        {
            DoLoadMovieInScreen(e.Source, e.Target, true);
        }
        public void CommonCtrl_GotoFirst()
        {
        	DoStopPlaying();
        	
        	if (m_bSynching)
            {
                m_iCurrentFrame = 0;
                OnCommonPositionChanged(m_iCurrentFrame, true);
                view.UpdateTrkFrame(m_iCurrentFrame);
                
            }
            else
            {
                // Ask global GotoFirst.
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen is PlayerScreen)
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoFirst_Click(null, EventArgs.Empty);
                    }
                }
            }	
        }
        public void CommonCtrl_GotoPrev()
        {
        	DoStopPlaying();
        	
        	if (m_bSynching)
            {
                if (m_iCurrentFrame > 0)
                {
                    m_iCurrentFrame--;
                    OnCommonPositionChanged(m_iCurrentFrame, true);
                    view.UpdateTrkFrame(m_iCurrentFrame);
                }
            }
            else
            {
                // Ask global GotoPrev.
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen.GetType().FullName.Equals("Kinovea.ScreenManager.PlayerScreen"))
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoPrevious_Click(null, EventArgs.Empty);
                    }
                }
            }	
        }
		public void CommonCtrl_GotoNext()
        {
			DoStopPlaying();
			
        	if (m_bSynching)
            {
                if (m_iCurrentFrame < m_iMaxFrame)
                {
                    m_iCurrentFrame++;
                    OnCommonPositionChanged(-1, true);
                    view.UpdateTrkFrame(m_iCurrentFrame);
                }
            }
            else
            {
                // Ask global GotoNext.
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen.GetType().FullName.Equals("Kinovea.ScreenManager.PlayerScreen"))
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoNext_Click(null, EventArgs.Empty);
                    }
                }
            }	
        }
		public void CommonCtrl_GotoLast()
        {
			DoStopPlaying();
        	
			if (m_bSynching)
            {
                m_iCurrentFrame = m_iMaxFrame;
                OnCommonPositionChanged(m_iCurrentFrame, true);
                view.UpdateTrkFrame(m_iCurrentFrame);
                
            }
            else
            {
                // Demander un GotoLast à tout le monde
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen is PlayerScreen)
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoLast_Click(null, EventArgs.Empty);
                    }
                }
            }	
        }
		public void CommonCtrl_Play()
        {
        	if (m_bSynching)
            {
                if (view.CommonPlaying)
                {
					// On play, simply launch the dynamic sync.
					// It will handle which video can start right away.
					StartDynamicSync();
                }
                else
                {
                    StopDynamicSync();
                    m_bLeftIsStarting = false;
                    m_bRightIsStarting = false;
                }
            }

        	// On stop, propagate the call to screens.
        	if(!view.CommonPlaying)
        	{	
        		if(screenList[0] is PlayerScreen)
	        		EnsurePause(0);
        		
        		if(screenList[1] is PlayerScreen)
        			EnsurePause(1);
        	}
        }
		public void CommonCtrl_Swap()
        {
        	mnuSwapScreensOnClick(null, EventArgs.Empty);	
        }
		public void CommonCtrl_Sync()
        {
			if (m_bSynching && screenList.Count == 2)
            {
                log.Debug("Sync point change.");
                SetSyncPoint(false);
                SetSyncLimits();
                OnCommonPositionChanged(m_iCurrentFrame, true);
            }
        }
		public void CommonCtrl_Merge()
        {
        	if (m_bSynching && screenList.Count == 2)
            {
        		m_bSyncMerging = view.Merging;
        		log.Debug(String.Format("SyncMerge videos is now {0}", m_bSyncMerging.ToString()));
        		
        		// This will also do a full refresh, and triggers Player_ImageChanged().
        		((PlayerScreen)screenList[0]).SyncMerge = m_bSyncMerging;
        		((PlayerScreen)screenList[1]).SyncMerge = m_bSyncMerging;
        	}
        }
   	public void CommonCtrl_PositionChanged(long _iPosition)
   	{
            // Manual static sync.
            if (m_bSynching)
            {
                StopDynamicSync();
                
                EnsurePause(0);
                EnsurePause(1);

                view.DisplayAsPaused();

                m_iCurrentFrame = _iPosition;
                OnCommonPositionChanged(m_iCurrentFrame, true);
            }	
        }
   	public void CommonCtrl_Snapshot()
   	{
   		// Retrieve current images and create a composite out of them.
   		if (m_bSynching && screenList.Count == 2)
        {
   			PlayerScreen ps1 = screenList[0] as PlayerScreen;
   			PlayerScreen ps2 = screenList[1] as PlayerScreen;
   			if(ps1 != null && ps2 != null)
   			{
   				DoStopPlaying();
   				
   				// get a copy of the images with drawings flushed on.
   				Bitmap leftImage = ps1.GetFlushedImage();
   				Bitmap rightImage = ps2.GetFlushedImage();
   				Bitmap composite = ImageHelper.GetSideBySideComposite(leftImage, rightImage, false, true);
   				
   				// Configure Save dialog.
   				SaveFileDialog dlgSave = new SaveFileDialog();
				dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
				dlgSave.RestoreDirectory = true;
				dlgSave.Filter = ScreenManagerLang.dlgSaveFilter;
				dlgSave.FilterIndex = 1;
				dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(ps1.FilePath), Path.GetFileNameWithoutExtension(ps2.FilePath));
				
				// Launch the dialog and save image.
				if (dlgSave.ShowDialog() == DialogResult.OK)
				{
					ImageHelper.Save(dlgSave.FileName, composite);
				}

				composite.Dispose();
				leftImage.Dispose();
				rightImage.Dispose();
				
				DelegatesPool dp = DelegatesPool.Instance();
        		if (dp.RefreshFileExplorer != null) dp.RefreshFileExplorer(false);
   			}
   		}
   	}
        public void CommonCtrl_DualVideo()
        {
            // Create and save a composite video with side by side synchronized images.
            // If merge is active, just save one video.
            
            if (!m_bSynching || screenList.Count != 2)
                return;
            
            PlayerScreen ps1 = screenList[0] as PlayerScreen;
            PlayerScreen ps2 = screenList[1] as PlayerScreen;
            if(ps1 == null || ps2 == null)
                return;
            
            DoStopPlaying();
            
            // Get file name from user.
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.dlgSaveVideoTitle;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
            dlgSave.FilterIndex = 1;
            dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(ps1.FilePath), Path.GetFileNameWithoutExtension(ps2.FilePath));
            
            if (dlgSave.ShowDialog() != DialogResult.OK)
                return;
            
            long iCurrentFrame = m_iCurrentFrame;
            m_bDualSaveCancelled = false;
            m_DualSaveFileName = dlgSave.FileName;
            
            // Instanciate and configure the bgWorker.
            m_bgWorkerDualSave = new BackgroundWorker();
            m_bgWorkerDualSave.WorkerReportsProgress = true;
            m_bgWorkerDualSave.WorkerSupportsCancellation = true;
            m_bgWorkerDualSave.DoWork += bgWorkerDualSave_DoWork;
            m_bgWorkerDualSave.ProgressChanged += bgWorkerDualSave_ProgressChanged;
            m_bgWorkerDualSave.RunWorkerCompleted += bgWorkerDualSave_RunWorkerCompleted;
            
            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            m_bDualSaveInProgress = true;
            ps1.DualSaveInProgress = true;
            ps2.DualSaveInProgress = true;
            
            // Create the progress bar and launch the worker.
            m_DualSaveProgressBar = new formProgressBar(true);
            m_DualSaveProgressBar.Cancel = dualSave_CancelAsked;
            m_bgWorkerDualSave.RunWorkerAsync();
            m_DualSaveProgressBar.ShowDialog();
            
            // If cancelled, delete temporary file.
            if(m_bDualSaveCancelled)
                DeleteTemporaryFile(m_DualSaveFileName);
            
            // Reset to where we were.
            m_bDualSaveInProgress = false;
            ps1.DualSaveInProgress = false;
            ps2.DualSaveInProgress = false;
            m_iCurrentFrame = iCurrentFrame;
            OnCommonPositionChanged(m_iCurrentFrame, true);
        }
        #endregion
        
        #region IMessageFilter Implementation
        public bool PreFilterMessage(ref Message m)
        {
            //----------------------------------------------------------------------------
            // Main keyboard handler.
            //
            // We must be careful with performances with this function.
            // As it will intercept every WM_XXX Windows message, 
            // incuding WM_PAINT, WM_MOUSEMOVE, etc. from each control.
            // 
            // If the function interfere with other parts of the application (because it
            // handles Return, Space, etc.) Use the DeactivateKeyboardHandler and 
            // ActivateKeyboardHandler delegates from the delegate pool, to temporarily 
            // bypass this handler.
            //----------------------------------------------------------------------------
            
            if (m.Msg != WM_KEYDOWN || !m_bAllowKeyboardHandler || view == null)
                return false;

            if ((screenList.Count == 0 || m_ActiveScreen == null) && !view.ThumbnailsViewerVisible)
                return false;

            bool bWasHandled = false;
            Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;

            switch (keyCode)
            {
            	case Keys.Delete:
            	case Keys.Add:
            	case Keys.Subtract:
                case Keys.NumPad0:
            	case Keys.F2:
            	case Keys.F7:
                    {
            			//------------------------------------------------
            			// These keystrokes impact only the active screen.
            			//------------------------------------------------
            			if(!view.ThumbnailsViewerVisible)
							bWasHandled = m_ActiveScreen.OnKeyPress(keyCode);
						else
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);

						break;
                    }
            	case Keys.Escape:
            	case Keys.F6:
                    {
            			//---------------------------------------------------
            			// These keystrokes impact each screen independently.
            			//---------------------------------------------------
            			if(!view.ThumbnailsViewerVisible)
            			{
                            foreach (AbstractScreen screen in screenList)
                                bWasHandled = screen.OnKeyPress(keyCode);
            			}
            			else
            			{
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);
            			}
                        break;
                    }
            	case Keys.Down:
            	case Keys.Up:
            		{
            			//-----------------------------------------------------------------------
            			// These keystrokes impact only one screen, because it will automatically 
            			// trigger the same change in the other screen.
            			//------------------------------------------------------------------------
            			if(!view.ThumbnailsViewerVisible)
            			{
            				if(screenList.Count > 0)
            					bWasHandled = screenList[0].OnKeyPress(keyCode);
            			}
            			else
            			{
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);
            			}
                        break;
                    }
                case Keys.Space:
            	case Keys.Return:
            	case Keys.Left:
            	case Keys.Right:
            	case Keys.End:
            	case Keys.Home:
                    {
                        //---------------------------------------------------
            			// These keystrokes impact both screens as a whole.
            			//---------------------------------------------------
                       	if(!view.ThumbnailsViewerVisible)
            			{
                       		if (screenList.Count == 2)
                            {
                       			if(view.CommonControlsVisible)
                       				bWasHandled = view.OnKeyPress(keyCode);
                       			else
                       				bWasHandled = m_ActiveScreen.OnKeyPress(keyCode);	
                            }
                            else if(screenList.Count == 1)
                            {
                            	bWasHandled = screenList[0].OnKeyPress(keyCode);
                            }	
                       	}
            			else
            			{
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);	
            			}
                        break;
                    }
            	//-------------------------------------------------
            	// All the remaining keystrokes impact both screen, 
            	// even if the common controls aren't visible.
            	//-------------------------------------------------
            	case Keys.Tab:
            	    {
            			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
						{
                			// Change active screen.
                			if(!view.ThumbnailsViewerVisible)
                			{
                				if(screenList.Count == 2)
                           		{
                					ActivateOtherScreen();
                					bWasHandled = true;
                				}
                			}
                			else
                			{
                				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);	
                			}
            			}
            			break;
            		}
                case Keys.F8:
                	{
                        // Go to sync frame. 
                        if(!view.ThumbnailsViewerVisible)
                		{
                        	if(m_bSynching)
                       		{
                                m_iCurrentFrame = m_iSyncLag > 0 ? m_iRightSyncFrame : m_iLeftSyncFrame;
                                
                                // Update
                                OnCommonPositionChanged(m_iCurrentFrame, true);
                                view.UpdateTrkFrame(m_iCurrentFrame);
                                bWasHandled = true;
                        	}
                        }
                        else
            			{
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);	
            			}
                        break;
                	}
                case Keys.F9:
                    {
                        //---------------------------------------
                        // Fonctions associées : 
                        // Resynchroniser après déplacement individuel
                        //---------------------------------------
                       	if(!view.ThumbnailsViewerVisible)
                        {
                       		if(m_bSynching)
                       		{
                       			SyncCatch();
                       			bWasHandled = true;
                       		}
                        }
                       	else
            			{
            				bWasHandled = view.thumbnailsViewer.OnKeyPress(keyCode);	
            			}
                        break;
                    }
                default:
                    break;
            }

            return bWasHandled;
        }
        #endregion
        
        #region Public Methods
        public void UpdateStatusBar()
        {
            //------------------------------------------------------------------
            // Function called on RefreshUiCulture, CommandShowScreen...
            // and calling upper module (supervisor).
            //------------------------------------------------------------------

            String StatusString = "";

            switch(screenList.Count)
            {
                case 1:
            		StatusString = screenList[0].Status;
                    break;
                case 2:
            		StatusString = screenList[0].Status + " | " + screenList[1].Status;
                    break;
                default:
                    break;
            }

            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.UpdateStatusBar != null)
            {
                dp.UpdateStatusBar(StatusString);
            }
        }
        public void OrganizeCommonControls()
        {
            bool show = screenList.Count == 2 && screenList[0] is PlayerScreen && screenList[1] is PlayerScreen;
            view.ShowCommonControls(show);
            canShowCommonControls = show;
        }
        public void UpdateCaptureBuffers()
        {
        	// The screen list has changed and involve capture screens.
        	// Update their shared state to trigger a memory buffer reset.
        	bool shared = screenList.Count == 2;
        	foreach(AbstractScreen screen in screenList)
        	{
        		CaptureScreen capScreen = screen as CaptureScreen;
        		if(capScreen != null)
        		{
        			capScreen.Shared = shared;
        		}
        	}
        }
        public void FullScreen(bool _bFullScreen)
        {
            // Propagate the new mode to screens.
            foreach (AbstractScreen screen in screenList)
            {
                screen.FullScreen(_bFullScreen);
            }
        }
        public static void AlertInvalidFileName()
        {
        	string msgTitle = ScreenManagerLang.Error_Capture_InvalidFile_Title;
        	string msgText = ScreenManagerLang.Error_Capture_InvalidFile_Text.Replace("\\n", "\n");
        		
			MessageBox.Show(msgText, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        
        #endregion
        
        #region Menu organization
        public void OrganizeMenus()
        {
            DoOrganizeMenu();
        }
        private void BuildSvgMenu()
        {
        	mnuSVGTools.Image = Properties.Resources.images;
            mnuSVGTools.MergeAction = MergeAction.Append;
            mnuImportImage.Image = Properties.Resources.image;
            mnuImportImage.Click += new EventHandler(mnuImportImage_OnClick);
			mnuImportImage.MergeAction = MergeAction.Append;
			AddImportImageMenu(mnuSVGTools);
            
        	AddSvgSubMenus(m_SvgPath, mnuSVGTools);
        }
        private void AddImportImageMenu(ToolStripMenuItem _menu)
        {
        	_menu.DropDownItems.Add(mnuImportImage);
            _menu.DropDownItems.Add(new ToolStripSeparator());
        }
        private void AddSvgSubMenus(string _dir, ToolStripMenuItem _menu)
        {
        	// This is a recursive function that browses a directory and its sub directories,
        	// each directory is made into a menu tree, each svg file is added as a menu leaf.
        	m_BuildingSVGMenu = true;
        	
        	if(Directory.Exists(_dir))
            {
            	// Loop sub directories.
            	string[] subDirs = Directory.GetDirectories (_dir);
				foreach (string subDir in subDirs)
				{
					// Create a menu
					ToolStripMenuItem mnuSubDir = new ToolStripMenuItem();
					mnuSubDir.Text = Path.GetFileName(subDir);
					mnuSubDir.Image = Properties.Resources.folder;
					mnuSubDir.MergeAction = MergeAction.Append;
					
					// Build sub tree.
					AddSvgSubMenus(subDir, mnuSubDir);
					
					// Add to parent if non-empty.
					if(mnuSubDir.HasDropDownItems)
					{
						_menu.DropDownItems.Add(mnuSubDir);
					}
				}

            	// Then loop files within the sub directory.
	            foreach (string file in Directory.GetFiles(_dir))
	            {
	            	if (Path.GetExtension(file).ToLower().Equals(".svg"))
	                {
	                	m_bHasSvgFiles = true;
	                	
	                	// Create a menu. 
	                	ToolStripMenuItem mnuSVGDrawing = new ToolStripMenuItem();
		            	mnuSVGDrawing.Text = Path.GetFileNameWithoutExtension(file);
		            	mnuSVGDrawing.Tag = file;
			            mnuSVGDrawing.Image = Properties.Resources.vector;
		            	mnuSVGDrawing.Click += new EventHandler(mnuSVGDrawing_OnClick);
			            mnuSVGDrawing.MergeAction = MergeAction.Append;
			            
			            // Add to parent.
			            _menu.DropDownItems.Add(mnuSVGDrawing);
	                }
	            }
            }
        	
        	m_BuildingSVGMenu = false;
        }
        private void DoOrganizeMenu()
        {
        	// Enable / disable menus depending on state of active screen
        	// and global screen configuration.
        	
            #region Menus depending only on the state of the active screen
            bool bActiveScreenIsEmpty = false;
            if (m_ActiveScreen != null && screenList.Count > 0)
            {
            	if(!m_ActiveScreen.Full)
            	{
            		bActiveScreenIsEmpty = true;	
            	}
            	else if (m_ActiveScreen is PlayerScreen)
                {
                	PlayerScreen player = m_ActiveScreen as PlayerScreen;
                	
                    // 1. Video is loaded : save-able and analysis is loadable.
                    
                	// File
                    mnuSave.Enabled = true;
                    toolSave.Enabled = true;
                    mnuExportSpreadsheet.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportODF.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportMSXML.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportXHTML.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportTEXT.Enabled = player.FrameServer.Metadata.HasTrack;
                    mnuLoadAnalysis.Enabled = true;
                    
                    // Image
                    mnuDeinterlace.Enabled = player.FrameServer.VideoReader.CanChangeDeinterlacing;
                    mnuMirror.Enabled = true;
                    mnuSVGTools.Enabled = m_bHasSvgFiles;
                    mnuCoordinateAxis.Enabled = true;
                  	
                    mnuDeinterlace.Checked = player.Deinterlaced;
                    mnuMirror.Checked = player.Mirrored;
                    
                    if(!player.IsSingleFrame)
                    {
                    	ConfigureImageFormatMenus(player);
                    }
                    else
                    {
                    	// Prevent usage of format menu for image files
                    	ConfigureImageFormatMenus(null);
                    }
                    
                    // Motion
                    mnuHighspeedCamera.Enabled = true;
                    ConfigureVideoFilterMenus(player);
                }
                else if(m_ActiveScreen is CaptureScreen)
                {
                 	CaptureScreen cs = m_ActiveScreen as CaptureScreen;   
                    
             		// File
                    mnuSave.Enabled = false;
                    toolSave.Enabled = false;
                   	mnuExportSpreadsheet.Enabled = false;
                    mnuExportODF.Enabled = false;
                    mnuExportMSXML.Enabled = false;
                    mnuExportXHTML.Enabled = false;
                    mnuExportTEXT.Enabled = false;
                    mnuLoadAnalysis.Enabled = false;
                    
                    // Image
                    mnuDeinterlace.Enabled = false;
                    mnuMirror.Enabled = false;
                    mnuSVGTools.Enabled = m_bHasSvgFiles;
                    mnuCoordinateAxis.Enabled = false;
                  	
                    mnuDeinterlace.Checked = false;
                    mnuMirror.Checked = false;
                   
                    ConfigureImageFormatMenus(cs);
                    
                    // Motion
                    mnuHighspeedCamera.Enabled = false;
                    ConfigureVideoFilterMenus(null);
                }
                else
                {
                	// KO ?
                	bActiveScreenIsEmpty = true;
                }
            }
            else
            {
                // No active screen. ( = no screens)
                bActiveScreenIsEmpty = true;
            }

            if (bActiveScreenIsEmpty)
            {
            	// File
                mnuSave.Enabled = false;
                toolSave.Enabled = false;
            	mnuLoadAnalysis.Enabled = false;
				mnuExportSpreadsheet.Enabled = false;
                mnuExportODF.Enabled = false;
                mnuExportMSXML.Enabled = false;
                mnuExportXHTML.Enabled = false;
                mnuExportTEXT.Enabled = false;

                // Image
                mnuDeinterlace.Enabled = false;
				mnuMirror.Enabled = false;
				mnuSVGTools.Enabled = false;
				mnuCoordinateAxis.Enabled = false;
				mnuDeinterlace.Checked = false;
				mnuMirror.Checked = false;
                
				ConfigureImageFormatMenus(null);
				
				// Motion
				mnuHighspeedCamera.Enabled = false;
				ConfigureVideoFilterMenus(null);
            }
            #endregion

            #region Menus depending on the specifc screen configuration
            // File
            mnuCloseFile.Visible  = false;
            mnuCloseFile.Enabled  = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Enabled = false;
            string strClosingText = ScreenManagerLang.Generic_Close;
            
            bool bAllScreensEmpty = false;
            switch (screenList.Count)
            {
                case 0:

                    // No screens at all.
                    mnuSwapScreens.Enabled        = false;
                    mnuToggleCommonCtrls.Enabled  = false;
                    bAllScreensEmpty = true;
                    break;

                case 1:
                    
                    // Only one screen
                    mnuSwapScreens.Enabled        = false;
                    mnuToggleCommonCtrls.Enabled  = false;

                    if(!screenList[0].Full)
                    {
                    	bAllScreensEmpty = true;	
                    }
                    else if(screenList[0] is PlayerScreen)
                    {
                    	// Only screen is an full PlayerScreen.
                        mnuCloseFile.Text = strClosingText;
                        mnuCloseFile.Enabled = true;
                        mnuCloseFile.Visible = true;

                        mnuCloseFile2.Visible = false;
                        mnuCloseFile2.Enabled = false;
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                    	bAllScreensEmpty = true;	
                    }
                    break;

                case 2:

                    // Two screens
                    mnuSwapScreens.Enabled = true;
                    mnuToggleCommonCtrls.Enabled = canShowCommonControls;
                    
                    // Left Screen
                    if (screenList[0] is PlayerScreen)
                    {
                        if (screenList[0].Full)
                        {
                            bAllScreensEmpty = false;
                            
                            string strCompleteClosingText = strClosingText + " - " + ((PlayerScreen)screenList[0]).FileName;
                            mnuCloseFile.Text = strCompleteClosingText;
                            mnuCloseFile.Enabled = true;
                            mnuCloseFile.Visible = true;
                        }
                        else
                        {
                            // Left screen is an empty PlayerScreen.
                            // Global emptiness might be changed below.
                            bAllScreensEmpty = true;
                        }
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                        // Global emptiness might be changed below.
                        bAllScreensEmpty = true;
                    }

                    // Right Screen.
                    if (screenList[1] is PlayerScreen)
                    {
                    	if (screenList[1].Full)
                        {
                            bAllScreensEmpty = false;
                            
                            string strCompleteClosingText = strClosingText + " - " + ((PlayerScreen)screenList[1]).FileName;
                            mnuCloseFile2.Text = strCompleteClosingText;
                            mnuCloseFile2.Enabled = true;
                            mnuCloseFile2.Visible = true;
                        }
                        else
                        {
                            // Ecran de droite en lecture, avec rien dedans.
                            // Si l'écran de gauche était également vide, bEmpty reste à true.
                            // Si l'écran de gauche était plein, bEmpty reste à false.
                        }
                    }
                    else if (screenList[1] is CaptureScreen)
                    {
                        // Ecran de droite en capture.
                        // Si l'écran de gauche était également vide, bEmpty reste à true.
                        // Si l'écran de gauche était plein, bEmpty reste à false.
                    }
                    break;

                default:
                    // KO.
                    mnuSwapScreens.Enabled       = false;
                    mnuToggleCommonCtrls.Enabled = false;
                    bAllScreensEmpty = true;
                    break;
            }

            if (bAllScreensEmpty)
            {
                // No screens at all, or all screens empty => 1 menu visible but disabled.

                mnuCloseFile.Text = strClosingText;
                mnuCloseFile.Visible = true;
                mnuCloseFile.Enabled = false;

                mnuCloseFile2.Visible = false;
            }
            #endregion

        }
        private void ConfigureVideoFilterMenus(PlayerScreen _player)
        {
            bool hasVideo = _player != null && _player.Full;
            foreach(ToolStripMenuItem menu in m_filterMenus)
			{
			    AbstractVideoFilter filter = menu.Tag as AbstractVideoFilter;
			    if(filter == null)
			        continue;
			    
			    menu.Visible = filter.Experimental ? Software.Experimental : true;
			    menu.Enabled = hasVideo ? _player.IsCaching : false;
			}
        }
        private void ConfigureImageFormatMenus(AbstractScreen _screen)
        {
			// Set the enable and check prop of the image formats menu according of current screen state.
			if(_screen == null || 
			   !_screen.Full ||
			  (_screen is PlayerScreen && !((PlayerScreen)_screen).FrameServer.VideoReader.CanChangeAspectRatio))
			{
			    mnuFormat.Enabled = false;
			    return;
			}
			
        	mnuFormat.Enabled = true;
			mnuFormatAuto.Enabled = true;
			mnuFormatForce43.Enabled = true;
			mnuFormatForce169.Enabled = true;
			
			// Reset all checks before setting the right one.
    		mnuFormatAuto.Checked = false;
        	mnuFormatForce43.Checked = false;
        	mnuFormatForce169.Checked = false;
    	
        	switch(_screen.AspectRatio)
        	{
        		case ImageAspectRatio.Force43:
        			mnuFormatForce43.Checked = true;
        			break;
        		case ImageAspectRatio.Force169:
        			mnuFormatForce169.Checked = true;
        			break;
        		case ImageAspectRatio.Auto:
        		default:
        			mnuFormatAuto.Checked = true;
        			break;
        	}
        }
        private void OnSVGFilesChanged(object source, FileSystemEventArgs e)
	    {
        	// We are in the file watcher thread. NO direct UI Calls from here.
        	log.Debug(String.Format("Action recorded in the guides directory: {0}", e.ChangeType));
        	if(!m_BuildingSVGMenu)
        	{
				m_BuildingSVGMenu = true;
				// Use "view" object just to merge back into the UI thread.
				view.BeginInvoke((MethodInvoker) delegate {DoSVGFilesChanged();});
        	}
	    }
        public void DoSVGFilesChanged()
        {
        	mnuSVGTools.DropDownItems.Clear();
        	AddImportImageMenu(mnuSVGTools);
        	AddSvgSubMenus(m_SvgPath, mnuSVGTools);
        }
        #endregion

        #region Culture
        private void RefreshCultureToolbar()
        {
        	toolSave.ToolTipText = ScreenManagerLang.mnuSave;
        	toolHome.ToolTipText = ScreenManagerLang.mnuHome;
        	toolOnePlayer.ToolTipText = ScreenManagerLang.mnuOnePlayer;
            toolTwoPlayers.ToolTipText = ScreenManagerLang.mnuTwoPlayers;
            toolOneCapture.ToolTipText = ScreenManagerLang.mnuOneCapture;
            toolTwoCaptures.ToolTipText = ScreenManagerLang.mnuTwoCaptures;
            toolTwoMixed.ToolTipText = ScreenManagerLang.mnuTwoMixed;	
        }
        private void RefreshCultureMenu()
        {
            mnuCloseFile.Text = ScreenManagerLang.Generic_Close;
            mnuCloseFile2.Text = ScreenManagerLang.Generic_Close;
            mnuSave.Text = ScreenManagerLang.mnuSave;
            mnuExportSpreadsheet.Text = ScreenManagerLang.mnuExportSpreadsheet;
            mnuExportODF.Text = ScreenManagerLang.mnuExportODF;
            mnuExportMSXML.Text = ScreenManagerLang.mnuExportMSXML;
            mnuExportXHTML.Text = ScreenManagerLang.mnuExportXHTML;
            mnuExportTEXT.Text = ScreenManagerLang.mnuExportTEXT;
            mnuLoadAnalysis.Text = ScreenManagerLang.mnuLoadAnalysis;
            
            mnuOnePlayer.Text = ScreenManagerLang.mnuOnePlayer;
            mnuTwoPlayers.Text = ScreenManagerLang.mnuTwoPlayers;
            mnuOneCapture.Text = ScreenManagerLang.mnuOneCapture;
            mnuTwoCaptures.Text = ScreenManagerLang.mnuTwoCaptures;
            mnuTwoMixed.Text = ScreenManagerLang.mnuTwoMixed;
            mnuSwapScreens.Text = ScreenManagerLang.mnuSwapScreens;
            mnuToggleCommonCtrls.Text = ScreenManagerLang.mnuToggleCommonCtrls;
            
            mnuDeinterlace.Text = ScreenManagerLang.mnuDeinterlace;
            mnuFormatAuto.Text = ScreenManagerLang.mnuFormatAuto;
            mnuFormatForce43.Text = ScreenManagerLang.mnuFormatForce43;
            mnuFormatForce169.Text = ScreenManagerLang.mnuFormatForce169;
            mnuFormat.Text = ScreenManagerLang.mnuFormat;
            mnuMirror.Text = ScreenManagerLang.mnuMirror;
            mnuCoordinateAxis.Text = ScreenManagerLang.mnuCoordinateSystem;
            
            mnuSVGTools.Text = ScreenManagerLang.mnuSVGTools;
            mnuImportImage.Text = ScreenManagerLang.mnuImportImage;
            RefreshCultureMenuFilters();
            mnuHighspeedCamera.Text = ScreenManagerLang.mnuSetCaptureSpeed;
        }
            
        private void RefreshCultureMenuFilters()
        {
            foreach(ToolStripMenuItem menu in m_filterMenus)
            {
                AbstractVideoFilter filter = menu.Tag as AbstractVideoFilter;
			    if(filter != null)
                    menu.Text = filter.Name;
            }
        }
                
        #endregion
        
        #region Side by side saving
        private void bgWorkerDualSave_DoWork(object sender, DoWorkEventArgs e)
        {
        	// This is executed in Worker Thread space. (Do not call any UI methods)
        	
        	// For each position: get both images, compute the composite, save it to the file.
        	// If blending is activated, only get the image from left screen, since it already contains both images.
        	log.Debug("Saving side by side video.");
        	
            if (!m_bSynching || screenList.Count != 2)
                return;
            
            PlayerScreen ps1 = screenList[0] as PlayerScreen;
			PlayerScreen ps2 = screenList[1] as PlayerScreen;
			if(ps1 == null && ps2 == null)
			    return;
			
			// Todo: get frame interval from one of the videos.
				
			// Get first frame outside the loop, to be able to set video size.
			m_iCurrentFrame = 0;
			OnCommonPositionChanged(m_iCurrentFrame, false);
			
			Bitmap img1 = ps1.GetFlushedImage();
			Bitmap img2 = null;
			Bitmap composite;
			if(!m_bSyncMerging)
			{
				img2 = ps2.GetFlushedImage();
				composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
			}
			else
			{
				composite = img1;
			}
				
			log.Debug(String.Format("Composite size: {0}.", composite.Size));
			
			// Configure a fake InfoVideo to setup image size.
			VideoInfo vi = new VideoInfo { OriginalSize = composite.Size };
			SaveResult result = m_VideoFileWriter.OpenSavingContext(m_DualSaveFileName, vi, -1, false);
	
			if(result != SaveResult.Success)
			{
			    e.Result = 2;
			    return;
			}
			
			m_VideoFileWriter.SaveFrame(composite);
			
			img1.Dispose();
			if(!m_bSyncMerging)
			{
                img2.Dispose();
	       	composite.Dispose();
			}

            // Loop all remaining frames in static sync mode, but without refreshing the UI.
			while(m_iCurrentFrame < m_iMaxFrame && !m_bDualSaveCancelled)
			{
				m_iCurrentFrame++;
				
				if(m_bgWorkerDualSave.CancellationPending)
				{
					e.Result = 1;
				    m_bDualSaveCancelled = true;
					break;
				}
				
				// Move both playheads and get the composite image.
				OnCommonPositionChanged(-1, false);
				img1 = ps1.GetFlushedImage();
                composite = img1;				
				if(!m_bSyncMerging)
				{
					img2 = ps2.GetFlushedImage();
					composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
				}
				
   			// Save to file.
   			m_VideoFileWriter.SaveFrame(composite);
   			
   			// Clean up and report progress.
   			img1.Dispose();
   			if(!m_bSyncMerging)
				{
   				img2.Dispose();
   				composite.Dispose();
   			}
   			
   			int percent = (int)(((double)(m_iCurrentFrame+1)/m_iMaxFrame) * 100);
   			m_bgWorkerDualSave.ReportProgress(percent);
			}
			
			if(!m_bDualSaveCancelled)
				e.Result = 0;
        }
        private void bgWorkerDualSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(m_bgWorkerDualSave.CancellationPending)
                return;

            m_DualSaveProgressBar.Update(Math.Min(e.ProgressPercentage, 100), 100, true);
        }
        private void bgWorkerDualSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	m_DualSaveProgressBar.Close();
    		m_DualSaveProgressBar.Dispose();
    		
    		if(!m_bDualSaveCancelled && (int)e.Result != 1)
    		{
    			m_VideoFileWriter.CloseSavingContext((int)e.Result == 0);
    		}
    		
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.RefreshFileExplorer != null) dp.RefreshFileExplorer(false);
        }
        private void dualSave_CancelAsked(object sender, EventArgs e)
		{
			// This will simply set BgWorker.CancellationPending to true,
			// which we check periodically in the saving loop.
	        // This will also end the bgWorker immediately,
	        // maybe before we check for the cancellation in the other thread. 
	        m_VideoFileWriter.CloseSavingContext(false);
	        m_bDualSaveCancelled = true;
	        m_bgWorkerDualSave.CancelAsync();
		}
        private void DeleteTemporaryFile(string _filename)
        {
        	log.Debug("Side by side video saving cancelled. Deleting temporary file.");
			if(File.Exists(_filename))
			{
				try
				{
					File.Delete(_filename);
				}
				catch (Exception exp)
				{
					log.Error("Error while deleting temporary file.");
					log.Error(exp.Message);
					log.Error(exp.StackTrace);
				}
			}
        }
        #endregion
             
        #region Menus events handlers

        #region File
        private void mnuCloseFileOnClick(object sender, EventArgs e)
        {
            CloseFile(0);
        }
        private void mnuCloseFile2OnClick(object sender, EventArgs e)
        {
            CloseFile(1);
        }
        private void CloseFile(int screenIndex)
        {
            RemoveScreen(screenIndex, true);
            
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);

            OrganizeCommonControls();
            OrganizeMenus();
        }
        public void mnuSaveOnClick(object sender, EventArgs e)
        {
        	// Public because accessed from the closing command when we realize there are 
            // unsaved modified data.
            PlayerScreen ps = m_ActiveScreen as PlayerScreen;
            if (ps == null)
                return;
            
            DoStopPlaying();
            DoDeactivateKeyboardHandler();
            ps.Save();
            DoActivateKeyboardHandler();
        }
        private void mnuLoadAnalysisOnClick(object sender, EventArgs e)
        {
            if (m_ActiveScreen != null && m_ActiveScreen is PlayerScreen)
                LoadAnalysis();
        }
        private void LoadAnalysis()
        {
            DoStopPlaying();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgLoadAnalysis_Title;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = ScreenManagerLang.dlgLoadAnalysis_Filter;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath.Length > 0)
                {
                   ((PlayerScreen)m_ActiveScreen).FrameServer.Metadata.Load(filePath, true);
                   ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.PostImportMetadata();
                }
            }
        }
        private void mnuExportODF_OnClick(object sender, EventArgs e)
        {
            ExportSpreadsheet(MetadataExportFormat.ODF);
        }
        private void mnuExportMSXML_OnClick(object sender, EventArgs e)
        {
        	ExportSpreadsheet(MetadataExportFormat.MSXML);	
        }
        private void mnuExportXHTML_OnClick(object sender, EventArgs e)
        {
        	ExportSpreadsheet(MetadataExportFormat.XHTML);
        }
        private void mnuExportTEXT_OnClick(object sender, EventArgs e)
        {
        	ExportSpreadsheet(MetadataExportFormat.TrajectoryText);
        }
        private void ExportSpreadsheet(MetadataExportFormat _format)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if (player != null)
            {
	            if (player.FrameServer.Metadata.HasData)
                {
                    DoStopPlaying();    

	            	SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Title = ScreenManagerLang.dlgExportSpreadsheet_Title;
                    saveFileDialog.RestoreDirectory = true;
                    saveFileDialog.Filter = ScreenManagerLang.dlgExportSpreadsheet_Filter;
                    
                    saveFileDialog.FilterIndex = ((int)_format) + 1;
                        
                    saveFileDialog.FileName = Path.GetFileNameWithoutExtension(player.FrameServer.Metadata.FullPath);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        if (filePath.Length > 0)
                        {
                        	player.FrameServer.Metadata.Export(filePath, _format);  
                        }
                    }
	            }
        	}
        }
        #endregion

        #region View
        private void mnuHome_OnClick(object sender, EventArgs e)
        {
        	// Remove all screens.
        	if(screenList.Count <= 0)
        	    return;
        	
        	if(RemoveScreen(0, true))
	        {
	        	m_bSynching = false;
	        	
	        	if(screenList.Count > 0)
	        	{
	        		// Second screen is now in [0] spot.
	        		RemoveScreen(0, true);
	        	}
	        }
	          
	        // Display the new list.
	        CommandManager cm = CommandManager.Instance();
	        ICommand css = new CommandShowScreens(this);
	        CommandManager.LaunchCommand(css);
	        
	        OrganizeCommonControls();
	        OrganizeMenus();
        }
        private void mnuOnePlayerOnClick(object sender, EventArgs e)
        {
        	//------------------------------------------------------------
        	// - Reorganize the list so it conforms to the asked combination.
        	// - Display the new list.
        	// 
        	// Here : One player screen.
        	//------------------------------------------------------------
            
            m_bSynching = false;
            CommandManager cm = CommandManager.Instance();

            switch (screenList.Count)
            {
                case 0:
            		{
	                    // Currently : 0 screens. -> add a player.
	                    IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    cm.LaunchUndoableCommand(caps);
	                    break;
            		}
                case 1:
            		{
	            		if(screenList[0] is CaptureScreen)
	                    {
	                    	// Currently : 1 capture. -> remove and add a player.
	                    	IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    	cm.LaunchUndoableCommand(caps);
	                    }
	                    else
	                    {
	                    	// Currently : 1 player. -> do nothing.
	                    }
	                    break;
            		}
                case 2:
            		{
	                    // We need to decide which screen(s) to remove.
						// Possible cases :
						// [capture][capture] -> remove both and add player.
						// [capture][player] -> remove capture.
						// [player][capture] -> remove capture.	
						// [player][player] -> depends on emptiness.
						
						if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
						{
							// [capture][capture] -> remove both and add player.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand crs2 = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs2);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    	cm.LaunchUndoableCommand(caps);
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove capture.	
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove capture.	
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
						}
						else
						{
							//---------------------------------------------
							// [player][player] -> depends on emptiness :
							// 
							// [empty][full] -> remove empty. 
							// [full][full] -> remove second one (right).
							// [full][empty] -> remove empty (right).
							// [empty][empty] -> remove second one (right).
							//---------------------------------------------
							
							if(!screenList[0].Full && screenList[1].Full)
							{
								RemoveScreen(0, true);
							}
							else
							{
								RemoveScreen(1, true);
							}
						}
	                    break;
            		}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuTwoPlayersOnClick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
        	// - Reorganize the list so it conforms to the asked combination.
        	// - Display the new list.
        	// 
        	// Here : Two player screens.
        	//------------------------------------------------------------
            m_bSynching = false;
            CommandManager cm = CommandManager.Instance();
            
            switch (screenList.Count)
            {
                case 0:
                    {
            			// Currently : 0 screens. -> add two players.
                        // We use two different commands to keep the undo history working.
            			IUndoableCommand caps1 = new CommandAddPlayerScreen(this, true);
                        cm.LaunchUndoableCommand(caps1);
                        IUndoableCommand caps2 = new CommandAddPlayerScreen(this, true);
                        cm.LaunchUndoableCommand(caps2);
                        break;
                    }
                case 1:
                    {
            			if(screenList[0] is CaptureScreen)
	                    {
	                    	// Currently : 1 capture. -> remove and add 2 players.
	                    	IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps1 = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps1);
                       		IUndoableCommand caps2 = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps2);
	                    }
	                    else
	                    {
	                    	// Currently : 1 player. -> add another.
	                    	IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps);
	                    }                    
                        break;
                    }
                case 2:
            		{
            			// We need to decide which screen(s) to remove.
						// Possible cases :
						// [capture][capture] -> remove both and add two players.
						// [capture][player] -> remove capture and add player.
						// [player][capture] -> remove capture and add player.	
						// [player][player] -> do nothing.
						
            			if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
						{
							// [capture][capture] -> remove both and add two players.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand crs2 = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs2);
							IUndoableCommand caps1 = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps1);
                       		IUndoableCommand caps2 = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps2);
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove capture and add player.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove capture and add player.
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps);
						}
						else
						{
            				// [player][player] -> do nothing.
						}
						
                    	break;
            		}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuOneCaptureOnClick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
        	// - Reorganize the list so it conforms to the asked combination.
        	// - Display the new list.
        	// 
        	// Here : One capture screens.
        	//------------------------------------------------------------
            m_bSynching = false;
            CommandManager cm = CommandManager.Instance();
           
           	switch (screenList.Count)
            {
                case 0:
           			{
	                    // Currently : 0 screens. -> add a capture.
	                    IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    cm.LaunchUndoableCommand(cacs);
	                    break;
           			}
                case 1:
                    {
	                    if(screenList[0] is PlayerScreen)
	                    {
	                    	// Currently : 1 player. -> remove and add a capture.
	                    	if(RemoveScreen(0, true))
	                    	{
	                    		IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    		cm.LaunchUndoableCommand(cacs);	
	                    	}
	                    }
	                    else
	                    {
	                    	// Currently : 1 capture. -> do nothing.
	                    }
	                    break;
                    }
                case 2:
           			{
	                    // We need to decide which screen(s) to remove.
						// Possible cases :
						// [capture][capture] -> depends on emptiness.
						// [capture][player] -> remove player.
						// [player][capture] -> remove player.	
						// [player][player] -> remove both and add capture.
						
						if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
						{
							//---------------------------------------------
							// [capture][capture] -> depends on emptiness.
							// 
							// [empty][full] -> remove empty.
							// [full][full] -> remove second one (right).
							// [full][empty] -> remove empty (right).
							// [empty][empty] -> remove second one (right).
							//---------------------------------------------
							
							if(!screenList[0].Full && screenList[1].Full)
							{
								RemoveScreen(0, true);
							}
							else
							{
								RemoveScreen(1, true);
							}
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove player.	
							RemoveScreen(1, true);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove player.
							RemoveScreen(0, true);
						}
						else
						{
							// remove both and add one capture.
							if(RemoveScreen(0, true))
							{
								// remaining player has moved in [0] spot.
								if(RemoveScreen(0, true))
								{
									IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    			cm.LaunchUndoableCommand(cacs);
								}
							}
						}
	                    break;
           			}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            UpdateCaptureBuffers();
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuTwoCapturesOnClick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
        	// - Reorganize the list so it conforms to the asked combination.
        	// - Display the new list.
        	// 
        	// Here : Two capture screens.
        	//------------------------------------------------------------
            m_bSynching = false;
            CommandManager cm = CommandManager.Instance();
            
            switch (screenList.Count)
            {
                case 0:
                    {
            			// Currently : 0 screens. -> add two capture.
                        // We use two different commands to keep the undo history working.
            			IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        cm.LaunchUndoableCommand(cacs);
                        IUndoableCommand cacs2 = new CommandAddCaptureScreen(this, true);
                        cm.LaunchUndoableCommand(cacs2);
                        break;
                    }
                case 1:
                    {
            			if(screenList[0] is CaptureScreen)
	                    {
	                    	// Currently : 1 capture. -> add another.
	                    	IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs);
	                    }
	                    else
	                    {
	                    	// Currently : 1 player. -> remove and add 2 capture.
	                    	if(RemoveScreen(0, true))
	                    	{
	                    		IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                        	cm.LaunchUndoableCommand(cacs);
	                        	IUndoableCommand cacs2 = new CommandAddCaptureScreen(this, true);
	                        	cm.LaunchUndoableCommand(cacs2);	
	                    	}
	                    }                   
                        break;
                    }
                case 2:
            		{
            			// We need to decide which screen(s) to remove.
						// Possible cases :
						// [capture][capture] -> do nothing.
						// [capture][player] -> remove player and add capture.
						// [player][capture] -> remove player and add capture.	
						// [player][player] -> remove both and add 2 capture.
						
            			if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
						{
							// [capture][capture] -> do nothing.
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove player and add capture.
							if(RemoveScreen(1, true))
							{
								IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        		cm.LaunchUndoableCommand(cacs);
							}
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove player and add capture.
							if(RemoveScreen(0, true))
							{
								IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        		cm.LaunchUndoableCommand(cacs);	
							}
						}
						else
						{
            				// [player][player] -> remove both and add 2 capture.
            				if(RemoveScreen(0, true))
							{
								// remaining player has moved in [0] spot.
								if(RemoveScreen(0, true))
								{
									IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
		                        	cm.LaunchUndoableCommand(cacs);
		                        	IUndoableCommand cacs2 = new CommandAddCaptureScreen(this, true);
		                        	cm.LaunchUndoableCommand(cacs2);
								}
							}
						}
						
                    	break;
            		}
                default:
                    break;
            }
            
            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            UpdateCaptureBuffers();
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuTwoMixedOnClick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
        	// - Reorganize the list so it conforms to the asked combination.
        	// - Display the new list.
        	// 
        	// Here : Mixed screen. The workspace preset is : [capture][player]
        	//------------------------------------------------------------
            m_bSynching = false;
            CommandManager cm = CommandManager.Instance();
            
            switch (screenList.Count)
            {
                case 0:
                    {
            			// Currently : 0 screens. -> add a capture and a player.
                        IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    cm.LaunchUndoableCommand(cacs);
            			IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
                        cm.LaunchUndoableCommand(caps);
                        break;
                    }
                case 1:
                    {
            			if(screenList[0] is CaptureScreen)
	                    {
	                    	// Currently : 1 capture. -> add a player.
	                    	IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps);
	                    }
	                    else
	                    {
	                    	// Currently : 1 player. -> add a capture.
	                    	IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    	cm.LaunchUndoableCommand(cacs);
	                    }                    
                        break;
                    }
                case 2:
            		{
            			// We need to decide which screen(s) to remove/replace.
						
            			if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
						{
							// [capture][capture] -> remove right and add player.
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps1 = new CommandAddPlayerScreen(this, true);
                        	cm.LaunchUndoableCommand(caps1);
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> do nothing.
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> do nothing.
						}
						else
						{
            				// [player][player] -> remove right and add capture.
            				if(RemoveScreen(1, true))
            				{
            					IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    		cm.LaunchUndoableCommand(cacs);
            				}
						}
						
                    	break;
            		}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
            UpdateCaptureBuffers();
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuSwapScreensOnClick(object sender, EventArgs e)
        {
            if (screenList.Count == 2)
            {
                IUndoableCommand command = new CommandSwapScreens(this);
                CommandManager cm = CommandManager.Instance();
                cm.LaunchUndoableCommand(command);
            }
        }
        private void mnuToggleCommonCtrlsOnClick(object sender, EventArgs e)
        {
            view.ToggleCommonControls();
        }
        #endregion

        #region Image
        private void mnuDeinterlaceOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(player != null)
        	{
        		mnuDeinterlace.Checked = !mnuDeinterlace.Checked;
        		player.Deinterlaced = mnuDeinterlace.Checked;	
        	}
        }
        private void mnuFormatAutoOnClick(object sender, EventArgs e)
        {
        	ChangeAspectRatio(ImageAspectRatio.Auto);
        }
        private void mnuFormatForce43OnClick(object sender, EventArgs e)
        {
        	ChangeAspectRatio(ImageAspectRatio.Force43);
        }
        private void mnuFormatForce169OnClick(object sender, EventArgs e)
        {
        	ChangeAspectRatio(ImageAspectRatio.Force169);
        }      
        private void ChangeAspectRatio(ImageAspectRatio _aspectRatio)
        {
        	if(m_ActiveScreen == null)
        	    return;
		
    		if(m_ActiveScreen.AspectRatio != _aspectRatio)
    			m_ActiveScreen.AspectRatio = _aspectRatio;
    		
    		mnuFormatForce43.Checked = _aspectRatio == ImageAspectRatio.Force43;
    		mnuFormatForce169.Checked = _aspectRatio == ImageAspectRatio.Force169;
    		mnuFormatAuto.Checked = _aspectRatio == ImageAspectRatio.Auto;
        }
        private void mnuMirrorOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(player != null)
        	{
        		mnuMirror.Checked = !mnuMirror.Checked;
        		player.Mirrored = mnuMirror.Checked;
        	}
        }
        private void mnuImportImage_OnClick(object sender, EventArgs e)
        {
            if(m_ActiveScreen == null || !m_ActiveScreen.CapabilityDrawings)
                return;
            
        	// Display file open dialog and launch the drawing.
        	OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgImportReference_Title;
            openFileDialog.Filter = ScreenManagerLang.dlgImportReference_Filter;
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                bool svg = Path.GetExtension(openFileDialog.FileName).ToLower() == ".svg";
                LoadDrawing(openFileDialog.FileName, svg);
            }
        }
        private void mnuSVGDrawing_OnClick(object sender, EventArgs e)
        {
        	// One of the dynamically added SVG tools menu has been clicked.
        	// Add a drawing of the right type to the active screen.
        	ToolStripMenuItem menu = sender as ToolStripMenuItem;
        	if(menu != null)
        	{
        		string svgFile = menu.Tag as string;
        		LoadDrawing(svgFile, true);
        	}
        }
        private void LoadDrawing(string _filePath, bool _bIsSVG)
        {
        	if(_filePath != null && _filePath.Length > 0 && m_ActiveScreen != null && m_ActiveScreen.CapabilityDrawings)
    		{
    			m_ActiveScreen.AddImageDrawing(_filePath, _bIsSVG);
    		}	
        }
        private void mnuCoordinateAxis_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = m_ActiveScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowCoordinateSystem();
        }
        #endregion

        #region Motion
        private void mnuHighspeedCamera_OnClick(object sender, EventArgs e)
        {
        	PlayerScreen ps = m_ActiveScreen as PlayerScreen;
        	if (ps != null)
        		ps.ConfigureHighSpeedCamera();
        }
        #endregion
        #endregion

        #region Services
        public void DoLoadMovieInScreen(string _filePath, int _iForceScreen, bool _bStoreState)
        {
        	if(File.Exists(_filePath))
            {
            	IUndoableCommand clmis = new CommandLoadMovieInScreen(this, _filePath, _iForceScreen, _bStoreState);
            	CommandManager cm = CommandManager.Instance();
            	cm.LaunchUndoableCommand(clmis);
            	
            	// No need to call PrepareSync here because it will be called when the working zone is set anyway.
        	}
        }
        public void DoStopPlaying()
        {
            // Called from Supervisor, when user launch open dialog box.
            
            // 1. Stop each screen.
            foreach (AbstractScreen screen in screenList)
            {
                if (screen is PlayerScreen)
                    ((PlayerScreen)screen).StopPlaying();
            }

            // 2. Stop the common timer.
            StopDynamicSync();
            view.DisplayAsPaused();
        }
        public void DoDeactivateKeyboardHandler()
        {
            m_bAllowKeyboardHandler = false;
        }
        public void DoActivateKeyboardHandler()
        {
            m_bAllowKeyboardHandler = true;
        }
        #endregion

        #region Keyboard Handling
        private void ActivateOtherScreen()
        {
        	if (screenList.Count != 2)
        	    return;
            
            if (m_ActiveScreen == screenList[0])
                Screen_SetActiveScreen(screenList[1]);
            else
                Screen_SetActiveScreen(screenList[0]);
        }
        #endregion

        #region Synchronisation
        private void PrepareSync(bool _bInitialization)
        {
        	// Called each time the screen list change 
        	// or when a screen changed selection.
        	
        	// We don't care which video was updated.
            // Set sync mode and reset sync.
            m_bSynching = false;

            if ( (screenList.Count == 2))
            {
                if ((screenList[0] is PlayerScreen) && (screenList[1] is PlayerScreen))
                {
                    if (((PlayerScreen)screenList[0]).Full && ((PlayerScreen)screenList[1]).Full)
                    {
                        m_bSynching = true;
                        ((PlayerScreen)screenList[0]).Synched = true;
                        ((PlayerScreen)screenList[1]).Synched = true;

                        if (_bInitialization)
                        {
                        	log.Debug("PrepareSync() - Initialization (reset of sync point).");
                            // Static Sync
                            m_iRightSyncFrame = 0;
                            m_iLeftSyncFrame = 0;
                            m_iSyncLag = 0;
                            m_iCurrentFrame = 0;
                            
                            ((PlayerScreen)screenList[0]).SyncPosition = 0;
	                		((PlayerScreen)screenList[1]).SyncPosition = 0;
	                		view.UpdateSyncPosition(m_iCurrentFrame);

                            // Dynamic Sync
                            ResetDynamicSyncFlags();
                            
                            // Sync Merging
                            ((PlayerScreen)screenList[0]).SyncMerge = false;
	                		((PlayerScreen)screenList[1]).SyncMerge = false;
	                		view.Merging = false;
                        }

                        // Mise à jour trkFrame
                        SetSyncLimits();

                        // Mise à jour Players
                        OnCommonPositionChanged(m_iCurrentFrame, true);
                    }
                    else
                    {
                        // Not all screens are loaded with videos.
                        ((PlayerScreen)screenList[0]).Synched = false;
                        ((PlayerScreen)screenList[1]).Synched = false;
                    }
                }
            }
            else
            {
                // Only one screen, or not all screens are PlayerScreens.
                switch (screenList.Count)
                {
                    case 1:
                        if (screenList[0] is PlayerScreen)
                        {
                            ((PlayerScreen)screenList[0]).Synched = false;
                        }
                        break;
                    case 2:
                        if (screenList[0] is PlayerScreen)
                        {
                            ((PlayerScreen)screenList[0]).Synched = false;
                        }
                        if (screenList[1] is PlayerScreen)
                        {
                            ((PlayerScreen)screenList[1]).Synched = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (!m_bSynching) 
            { 
                StopDynamicSync();
                view.DisplayAsPaused();
            }
        }
        public void SetSyncPoint(bool _bIntervalOnly)
        {
            //--------------------------------------------------------------------------------------------------
            // Registers the current position of each video as its sync frame. (Optional)
            // Computes the lag in common timestamps between positions.
            // Computes the lag in milliseconds between positions. (using current framerate of each video)
            // Update current common position.
            // (public only because accessed from the Swap command.)
            //--------------------------------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            // Par défaut les deux vidéos sont synchronisées sur {0}.
            // Le paramètre de synchro se lit comme suit : 
            // {+2} : La vidéo de droite à 2 frames d'avance sur celle de gauche.
            // {-4} : La vidéo de droite à 4 frames de retard.
            //
            // Si le décalage est positif, la vidéo de droite doit partir en premier.
            // La pause de terminaison dépend à la fois du paramètre de synchro et 
            // des durées (en frames) respectives des deux vidéos.
            //
            // Si _bIntervalOnly == true, on ne veut pas changer les frames de référence
            // (Généralement après une modification du framerate de l'une des vidéos ou swap)
            //----------------------------------------------------------------------------
            if (m_bSynching && screenList.Count == 2)
            {
	            // Registers current positions.
	            if (!_bIntervalOnly)
	            {
	                // For timing label only
	                ((PlayerScreen)screenList[0]).SyncPosition = ((PlayerScreen)screenList[0]).Position;
	                ((PlayerScreen)screenList[1]).SyncPosition = ((PlayerScreen)screenList[1]).Position;
	
	                m_iLeftSyncFrame = ((PlayerScreen)screenList[0]).CurrentFrame;
	                m_iRightSyncFrame = ((PlayerScreen)screenList[1]).CurrentFrame;
	                
	                log.Debug(String.Format("New Sync Points:[{0}][{1}], Sync lag:{2}",m_iLeftSyncFrame, m_iRightSyncFrame, m_iRightSyncFrame - m_iLeftSyncFrame));
	            }
	
	
	            // Sync Lag is expressed in frames.
	            m_iSyncLag = m_iRightSyncFrame - m_iLeftSyncFrame;
	
	            // We need to recompute the lag in milliseconds because it can change even when 
	            // the references positions don't change. For exemple when varying framerate (speed).
	            long iLeftSyncMilliseconds = (long)(((PlayerScreen)screenList[0]).FrameInterval * m_iLeftSyncFrame);
	            long iRightSyncMilliseconds = (long)(((PlayerScreen)screenList[1]).FrameInterval * m_iRightSyncFrame);
	            m_iSyncLagMilliseconds = iRightSyncMilliseconds - iLeftSyncMilliseconds;
	
	            // Update common position (sign of m_iSyncLag might have changed.)
	            m_iCurrentFrame = m_iSyncLag > 0 ? m_iRightSyncFrame : m_iLeftSyncFrame;
	            
	            view.UpdateSyncPosition(m_iCurrentFrame);  // <-- expects timestamp ?
            }
        }
        private void SetSyncLimits()
        {
            //-----------------------------------------------------------------------------------
            // Computes the real max of the trkFrame, considering the lag and original durations.
            // Updates trkFrame bounds, expressed in *Frames*.
            // impact : m_iMaxFrame.
            //-----------------------------------------------------------------------------------
			log.Debug("SetSyncLimits() called.");
            long leftEstimatedFrames = ((PlayerScreen)screenList[0]).EstimatedFrames;
            long rightEstimatedFrames = ((PlayerScreen)screenList[1]).EstimatedFrames;

            if (m_iSyncLag > 0)
            {
                // Lag is positive. Right video starts first and its duration stay the same as original.
                // Left video has to wait for an ammount of time.

                // Check if lag is still valid. (?) Why is this needed ?
                if (m_iSyncLag > rightEstimatedFrames)
                    m_iSyncLag = 0; 

                leftEstimatedFrames += m_iSyncLag;
            }
            else
            {
                // Lag is negative. Left video starts first and its duration stay the same as original.
                // Right video has to wait for an ammount of time.
                
                // Get Lag in frames of right video
                //int iSyncLagFrames = ((PlayerScreen)screenList[1]).NormalizedToFrame(m_iSyncLag);

                // Check if lag is still valid.(?)
                if (-m_iSyncLag > leftEstimatedFrames)
                    m_iSyncLag = 0;
                
                rightEstimatedFrames += (-m_iSyncLag);
            }

            m_iMaxFrame = (int)Math.Max(leftEstimatedFrames, rightEstimatedFrames);
            view.SetupTrkFrame(0, m_iMaxFrame, m_iCurrentFrame);

            log.DebugFormat("m_iSyncLag:{0}, m_iSyncLagMilliseconds:{1}, MaxFrames:{2}", m_iSyncLag, m_iSyncLagMilliseconds, m_iMaxFrame);
        }
        private void OnCommonPositionChanged(long _iFrame, bool _bAllowUIUpdate)
        {
            //------------------------------------------------------------------------------
            // This is where the "static sync" is done.
            // Updates each video to reflect current common position.
            // Used to handle GotoNext, GotoPrev, trkFrame, etc.
            // 
            // note: m_iSyncLag and _iFrame are expressed in frames.
            //------------------------------------------------------------------------------

            //log.Debug(String.Format("Static Sync, common position changed to {0}",_iFrame));
            
            // Get corresponding position in each video, in frames
            long iLeftFrame = 0;
            long iRightFrame = 0;

            if (_iFrame >= 0)
            {
                if (m_iSyncLag > 0)
                {
                    // Right video must go ahead.

                    iRightFrame = _iFrame;
                    iLeftFrame = _iFrame - m_iSyncLag;
                    if (iLeftFrame < 0)
                    {
                        iLeftFrame = 0;
                    }
                }
                else
                {
                    // Left video must go ahead.

                    iLeftFrame = _iFrame;
                    iRightFrame = _iFrame - (-m_iSyncLag);
                    if (iRightFrame < 0)
                    {
                        iRightFrame = 0;
                    }
                }

                // Force positions.
                ((PlayerScreen)screenList[0]).GotoFrame(iLeftFrame, _bAllowUIUpdate);
                ((PlayerScreen)screenList[1]).GotoFrame(iRightFrame, _bAllowUIUpdate);
            }
            else
            {
                // Special case for ++.
                if (m_iSyncLag > 0)
                {
                    // Right video must go ahead.
                    ((PlayerScreen)screenList[1]).GotoNextFrame(_bAllowUIUpdate);

                    if (m_iCurrentFrame > m_iSyncLag)
                    {
                        ((PlayerScreen)screenList[0]).GotoNextFrame(_bAllowUIUpdate);
                    }
                }
                else
                {
                    // Left video must go ahead.
                    ((PlayerScreen)screenList[0]).GotoNextFrame(_bAllowUIUpdate);

                    if (m_iCurrentFrame > -m_iSyncLag)
                    {
                        ((PlayerScreen)screenList[1]).GotoNextFrame(_bAllowUIUpdate);
                    }
                }
            }
        }
        public void SwapSync()
        {
            if (!m_bSynching || screenList.Count != 2)
        	    return;
            
            long temp = m_iLeftSyncFrame;
            m_iLeftSyncFrame = m_iRightSyncFrame;
            m_iRightSyncFrame = temp;

            ResetDynamicSyncFlags();
        }
        private void StartDynamicSync()
        {
        	m_bDynamicSynching = true;
        	DynamicSync();
        }
        private void StopDynamicSync()
        {
        	m_bDynamicSynching = false;
        }
        private void DynamicSync()
        {
        	// This is where the dynamic sync is done.
        	// It was used in timer loop at some point but now it's called directly.
        	// When a screen finishes decoding its image, we call in here to verify if the other screen
        	// needs to be started, paused, or something else.
        	
            // Get each video positions in common timebase and milliseconds.
            // Figure if a restart or pause is needed, considering current positions.
            
            // When the user press the common play button, we just propagate the play to the screens.
            // The common timer is just set to try to be notified of each frame change.
            // It is not used to provoke frame change itself.
            // We just start and stop the players timers when we detect one of the video has reached the end,
            // to prevent it from auto restarting.

            // Glossary:
            // XIsStarting 	: currently on [0] but a Play was asked.
            // XIsCatchingUp 	: video is between [0] and the point where both video will be running. 
            
            
            if (m_bSynching && screenList.Count == 2)
            {
                // L'ensemble de la supervision est réalisée en TimeStamps.
                // Seul les décision de lancer / arrêter sont établies par rapport
                // au temps auquel on est.

                long iLeftPosition = ((PlayerScreen)screenList[0]).CurrentFrame;
                long iRightPosition = ((PlayerScreen)screenList[1]).CurrentFrame;
                long iLeftMilliseconds = (long)(iLeftPosition * ((PlayerScreen)screenList[0]).FrameInterval);
                long iRightMilliseconds = (long)(iRightPosition * ((PlayerScreen)screenList[1]).FrameInterval);

                //-----------------------------------------------------------------------
                // Dans cette fonction, on part du principe que les deux vidéos tournent.
                // Et on fait des 'Ensure Pause' quand nécessaire.
                // On évite les Ensure Play' car l'utilisateur a pu 
                // manuellement pauser une vidéo.
                //-----------------------------------------------------------------------
                #region [i][0]
                if (iLeftPosition > 0 && iRightPosition == 0)
                {
                	EnsurePlay(0);
                	
                    // Etat 4. [i][0]
                    m_bLeftIsStarting = false;

                    if (m_iSyncLag == 0)
                    {
                        //-----------------------------------------------------
                        // La vidéo de droite 
                        // - vient de boucler et on doit attendre l'autre
                        // - est en train de repartir.
                        //-----------------------------------------------------
                        if (!m_bRightIsStarting)
                        {
                            // Stop pour bouclage
                            EnsurePause(1);
                        }

                        m_iCurrentFrame = iLeftPosition;
                    }
                    else if (m_iSyncLagMilliseconds > 0)
                    {
                        // La vidéo de droite est sur 0 et doit partir en premier.
                        // Si elle n'est pas en train de repartir, c'est qu'on 
                        // doit attendre que la vidéo de gauche ait finit son tour.
                        if (!m_bRightIsStarting)
                        {
                            EnsurePause(1);
                            m_iCurrentFrame = iLeftPosition + m_iSyncLag;
                        }
                        else
                        {
                            m_iCurrentFrame = iLeftPosition;
                        }
                    }
                    else if (m_iSyncLagMilliseconds < 0)
                    {
                        // La vidéo de droite est sur 0, en train de prendre son retard.
                        // On la relance si celle de gauche a fait son décalage.

                        // Attention, ne pas relancer si celle de gauche est en fait en train de terminer son tour
                        if (!m_bLeftIsCatchingUp && !m_bRightIsStarting)
                        {
                            EnsurePause(1);
                            m_iCurrentFrame = iLeftPosition;
                        }
                        else if (iLeftMilliseconds > (-m_iSyncLagMilliseconds) - 24)
                        {
                            // La vidéo de gauche est sur le point de franchir le sync point.
                            // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                            // la vidéo qui est partie en premier...
                            EnsurePlay(1);
                            m_bRightIsStarting = true;
                            m_bLeftIsCatchingUp = false;
                            m_iCurrentFrame = iLeftPosition;
                        }
                        else
                        {
                            // La vidéo de gauche n'a pas encore fait son décalage.
                            // On ne force pas sa lecture. (Pause manuelle possible).
                            m_bLeftIsCatchingUp = true;
                            m_iCurrentFrame = iLeftPosition;
                        }
                    }
                }
                #endregion
                #region [0][0]
                else if (iLeftPosition == 0 && iRightPosition == 0)
                {
                    // Etat 1. [0][0]
                    m_iCurrentFrame = 0;

                    // Les deux vidéos viennent de boucler ou sont en train de repartir.
                    if (m_iSyncLag == 0)
                    {
                        //---------------------
                        // Redemmarrage commun.
                        //---------------------
                        if (!m_bLeftIsStarting && !m_bRightIsStarting)
                        {
                            EnsurePlay(0);
                            EnsurePlay(1);

                            m_bRightIsStarting = true;
                            m_bLeftIsStarting = true;
                        }
                    }
                    else if (m_iSyncLagMilliseconds > 0)
                    {
                        // Redemarrage uniquement de la vidéo de droite, 
                        // qui doit faire son décalage

                        EnsurePause(0);
                        EnsurePlay(1);
                        m_bRightIsStarting = true;
                        m_bRightIsCatchingUp = true;
                    }
                    else if (m_iSyncLagMilliseconds < 0)
                    {
                        // Redemarrage uniquement de la vidéo de gauche, 
                        // qui doit faire son décalage

                        EnsurePlay(0);
                        EnsurePause(1);
                        m_bLeftIsStarting = true;
                        m_bLeftIsCatchingUp = true;
                    }
                }
                #endregion
                #region [0][i]
                else if (iLeftPosition == 0 && iRightPosition > 0)
                {
                    // Etat [0][i]
                    EnsurePlay(1);
                    
                    m_bRightIsStarting = false;

                    if (m_iSyncLag == 0)
                    {
                        m_iCurrentFrame = iRightPosition;

                        //--------------------------------------------------------------------
                        // Configuration possible : la vidéo de gauche vient de boucler.
                        // On la stoppe en attendant le redemmarrage commun.
                        //--------------------------------------------------------------------
                        if (!m_bLeftIsStarting)
                        {
                            EnsurePause(0);
                        }
                    }
                    else if (m_iSyncLagMilliseconds > 0)
                    {
                        // La vidéo de gauche est sur 0, en train de prendre son retard.
                        // On la relance si celle de droite a fait son décalage.

                        // Attention ne pas relancer si la vidéo de droite est en train de finir son tour
                        if (!m_bRightIsCatchingUp && !m_bLeftIsStarting)
                        {
                            // La vidéo de droite est en train de finir son tour tandisque celle de gauche a déjà bouclé.
                            EnsurePause(0);
                            m_iCurrentFrame = iRightPosition;
                        }
                        else if (iRightMilliseconds > m_iSyncLagMilliseconds - 24)
                        {
                            // La vidéo de droite est sur le point de franchir le sync point.
                            // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                            // la vidéo qui est partie en premier...
                            EnsurePlay(0);
                            m_bLeftIsStarting = true;
                            m_bRightIsCatchingUp = false;
                            m_iCurrentFrame = iRightPosition;
                        }
                        else
                        {
                            // La vidéo de droite n'a pas encore fait son décalage.
                            // On ne force pas sa lecture. (Pause manuelle possible).
                            m_bRightIsCatchingUp = true;
                            m_iCurrentFrame = iRightPosition;
                        }
                    }
                    else if (m_iSyncLagMilliseconds < 0)
                    {
                        // La vidéo de gauche est sur 0 et doit partir en premier.
                        // Si elle n'est pas en train de repartir, c'est qu'on 
                        // doit attendre que la vidéo de droite ait finit son tour.
                        if (!m_bLeftIsStarting)
                        {
                            EnsurePause(0);
                            m_iCurrentFrame = iRightPosition + m_iSyncLag;
                        }
                        else
                        {
                            // Rare, les deux première frames de chaque vidéo n'arrivent pas en même temps
                            m_iCurrentFrame = iRightPosition;
                        }
                    }
                }
                #endregion
                #region [i][i]
                else
                {
                    // Etat [i][i]
                 	EnsurePlay(0);
                 	EnsurePlay(1);
                    
                    m_bLeftIsStarting = false;
                    m_bRightIsStarting = false;

                    m_iCurrentFrame = Math.Max(iLeftPosition, iRightPosition);
                }
                #endregion

                // Update position for trkFrame.
                object[] parameters = new object[] { m_iCurrentFrame };
                
                // Note: do we need to begin invoke here ?
                view.BeginInvoke(view.delegateUpdateTrackerFrame, parameters);

                //log.Debug(String.Format("Tick:[{0}][{1}], Starting:[{2}][{3}], Catching up:[{4}][{5}]", iLeftPosition, iRightPosition, m_bLeftIsStarting, m_bRightIsStarting, m_bLeftIsCatchingUp, m_bRightIsCatchingUp));
            }
            else
            {
                // This can happen when a screen is closed on the fly while synching.
                StopDynamicSync();
                m_bSynching = false;
                view.DisplayAsPaused();
            }
        }
        private void EnsurePause(int _iScreen)
        {
        	//log.Debug(String.Format("Ensuring pause of screen [{0}]", _iScreen));
            if (_iScreen < screenList.Count)
            {
                if (((PlayerScreen)screenList[_iScreen]).IsPlaying)
                {
                    ((PlayerScreen)screenList[_iScreen]).m_PlayerScreenUI.OnButtonPlay();
                }
            }
            else
            {
                m_bSynching = false;
                view.DisplayAsPaused();
            }
        }
        private void EnsurePlay(int _iScreen)
        {
        	//log.Debug(String.Format("Ensuring play of screen [{0}]", _iScreen));
            if (_iScreen < screenList.Count)
            {
                if (!((PlayerScreen)screenList[_iScreen]).IsPlaying)
                {
                    ((PlayerScreen)screenList[_iScreen]).m_PlayerScreenUI.OnButtonPlay();
                }
            }
            else
            {
                m_bSynching = false;
                view.DisplayAsPaused();
            }
        }
        private void ResetDynamicSyncFlags()
        {
            m_bRightIsStarting = false;
            m_bLeftIsStarting = false;
            m_bLeftIsCatchingUp = false;
            m_bRightIsCatchingUp = false;
        }
        private void SyncCatch()
        {
            // We sync back the videos.
            // Used when one video has been moved individually.
			log.Debug("SyncCatch() called.");
			long iLeftFrame = ((PlayerScreen)screenList[0]).CurrentFrame;
			long iRightFrame = ((PlayerScreen)screenList[1]).CurrentFrame;

            if (m_iSyncLag > 0)
            {
                // Right video goes ahead.
                if (iLeftFrame + m_iSyncLag == m_iCurrentFrame || (m_iCurrentFrame < m_iSyncLag && iLeftFrame == 0))
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    m_iCurrentFrame = iRightFrame;
                }
                else if (iRightFrame == m_iCurrentFrame)
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    m_iCurrentFrame = iLeftFrame + m_iSyncLag;
                }
                else
                {
                    // Both videos were moved.
                    m_iCurrentFrame = iLeftFrame + m_iSyncLag;
                }
            }
            else
            {
                // Left video goes ahead.
                if (iRightFrame - m_iSyncLag == m_iCurrentFrame || (m_iCurrentFrame < -m_iSyncLag && iRightFrame == 0))
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    m_iCurrentFrame = iLeftFrame;
                }
                else if (iLeftFrame == m_iCurrentFrame)
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    m_iCurrentFrame = iRightFrame - m_iSyncLag;
                }
                else
                {
                    // Both videos were moved.
                    m_iCurrentFrame = iLeftFrame;
                }
            }

            OnCommonPositionChanged(m_iCurrentFrame, true);
            view.UpdateTrkFrame(m_iCurrentFrame);

        }
        #endregion

        #region Screens State Recalling
        public void StoreCurrentState()
        {
            //------------------------------------------------------------------------------
            // Before we start anything messy, let's store the current state of the ViewPort
            // So we can reinstate it later in case the user change his mind.
            //-------------------------------------------------------------------------------
            m_StoredStates.Add(GetCurrentState());
        }
        public ScreenManagerState GetCurrentState()
        {
            ScreenManagerState mState = new ScreenManagerState();

            foreach (AbstractScreen screen in screenList)
            {
                ScreenState state = new ScreenState();

                state.UniqueId = screen.UniqueId;

                if (screen is PlayerScreen && screen.Full)
                {
                    state.Loaded = true;
                    state.FilePath = ((PlayerScreen)screen).FilePath;
                    state.MetadataString = ((PlayerScreen)screen).FrameServer.Metadata.ToXmlString(1);
                }
                else
                {
                    state.Loaded = false;
                    state.FilePath = "";
                    state.MetadataString = "";
                }
                mState.ScreenList.Add(state);
            }

            return mState;
        }
        public void RecallState()
        {
            //-------------------------------------------------
            // Reconfigure the ViewPort to match the old state.
            // Reload the right movie with its meta data.
            //-------------------------------------------------
         
            if (m_StoredStates.Count > 0)
            {
                int iLastState = m_StoredStates.Count -1;
                CommandManager cm = CommandManager.Instance();
                ICommand css = new CommandShowScreens(this);

                ScreenManagerState CurrentState = GetCurrentState();

                switch (CurrentState.ScreenList.Count)
                {
                    case 0:
                        //-----------------------------
                        // Il y a actuellement 0 écran.
                        //-----------------------------
                        switch (m_StoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                // Il n'y en avait aucun : Ne rien faire.
                                break;
                            case 1:
                                {
                                    // Il y en avait un : Ajouter l'écran.
                                    ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState); 
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 2:
                                {
                                    // Ajouter les deux écrans, on ne se préoccupe pas trop de l'ordre
                                    ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                    ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    case 1:
                        //-----------------------------
                        // Il y a actuellement 1 écran.
                        //-----------------------------
                        switch (m_StoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                {
                                    // Il n'y en avait aucun : Supprimer l'écran.
                                    RemoveScreen(0, false);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 1:
                                {
                                    // Il y en avait un : Remplacer si besoin.
                                    ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 2:
                                {
                                    // Il y avait deux écran : Comparer chaque ancien écran avec le restant.
                                    int iMatchingScreen = -1;
                                    int i=0;
                                    while ((iMatchingScreen == -1) && (i < m_StoredStates[iLastState].ScreenList.Count))
                                    {
                                        if (m_StoredStates[iLastState].ScreenList[i].UniqueId == CurrentState.ScreenList[0].UniqueId)
                                        {
                                            iMatchingScreen = i;
                                        }
                                        else
                                        {
                                            i++;
                                        }
                                    }

                                    switch (iMatchingScreen)
                                    {
                                        case -1:
                                            {
                                                // No matching screen found
                                                ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                                ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                break;
                                            }
                                        case 0:
                                            {
                                                // the old 0 is the new 0, the old 1 doesn't exist yet.
                                                ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                break;
                                            }
                                        case 1:
                                            {
                                                // the old 1 is the new 0, the old 0 doesn't exist yet.
                                                ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 1, CurrentState);
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    case 2:
                        // Il y a actuellement deux écrans.
                        switch (m_StoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                {
                                    // Il n'yen avait aucun : supprimer les deux.
                                    RemoveScreen(1, false);
                                    RemoveScreen(0, false);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 1:
                                {
                                    // Il y en avait un : le rechercher parmi les nouveaux.
                                    int iMatchingScreen = -1;
                                    int i = 0;
                                    while ((iMatchingScreen == -1) && (i < CurrentState.ScreenList.Count))
                                    {
                                        if (m_StoredStates[iLastState].ScreenList[0].UniqueId == CurrentState.ScreenList[i].UniqueId)
                                        {
                                            iMatchingScreen = i;
                                        }
                                        
                                        i++;
                                    }

                                    switch (iMatchingScreen)
                                    {
                                        case -1:
                                            // L'ancien écran n'a pas été retrouvé.
                                            // On supprime tout et on le rajoute.
                                            RemoveScreen(1, false);
                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                            break;
                                        case 0:
                                            // L'ancien écran a été retrouvé dans l'écran [0]
                                            // On supprime le second.
                                            RemoveScreen(1, false);
                                            break;
                                        case 1:
                                            // L'ancien écran a été retrouvé dans l'écran [1]
                                            // On supprime le premier.
                                            RemoveScreen(0, false);
                                            break;
                                        default:
                                            break;
                                    }
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 2:
                                {
                                    // Il y avait deux écrans également : Rechercher chacun parmi les nouveaux.
                                    int[] iMatchingScreen = new int[2];
                                    iMatchingScreen[0] = -1;
                                    iMatchingScreen[1] = -1;
                                    int i = 0;
                                    while (i < CurrentState.ScreenList.Count)
                                    {
                                        if (m_StoredStates[iLastState].ScreenList[0].UniqueId == CurrentState.ScreenList[i].UniqueId)
                                        {
                                            iMatchingScreen[0] = i;
                                        }
                                        else if (m_StoredStates[iLastState].ScreenList[1].UniqueId == CurrentState.ScreenList[i].UniqueId)
                                        {
                                            iMatchingScreen[1] = i;
                                        }

                                        i++;
                                    }

                                    switch (iMatchingScreen[0])
                                    {
                                        case -1:
                                            {
                                                // => L'ancien écran [0] n'a pas été retrouvé.
                                                switch (iMatchingScreen[1])
                                                {
                                                    case -1:
                                                        {
                                                            // Aucun écran n'a été retrouvé.
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                            break;
                                                        }
                                                    case 0:
                                                        {
                                                            // Ecran 0 non retrouvé, écran 1 retrouvé dans le 0.
                                                            // Remplacer l'écran 1 par l'ancien 0.
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 1, CurrentState);
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            // Ecran 0 non retrouvé, écran 1 retrouvé dans le 1.
                                                            // Remplacer l'écran 0.
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                                            break;
                                                        }
                                                    default:
                                                        break;
                                                }
                                                break;
                                            }
                                        case 0:
                                            {
                                                // L'ancien écran [0] a été retrouvé dans l'écran [0]
                                                switch (iMatchingScreen[1])
                                                {
                                                    case -1:
                                                        {
                                                            // Ecran 0 retrouvé dans le [0], écran 1 non retrouvé. 
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                            break;
                                                        }
                                                    case 0:
                                                        {
                                                            // Ecran 0 retrouvé dans le [0], écran 1 retrouvé dans le [0].
                                                            // Impossible.
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            // Ecran 0 retrouvé dans le [0], écran 1 retrouvé dans le [1].
                                                            // rien à faire.
                                                            break;
                                                        }
                                                    default:
                                                        break;
                                                }
                                                break;
                                            }
                                        case 1:
                                            {
                                                // L'ancien écran [0] a été retrouvé dans l'écran [1]
                                                switch (iMatchingScreen[1])
                                                {
                                                    case -1:
                                                        {
                                                            // Ecran 0 retrouvé dans le [1], écran 1 non retrouvé. 
                                                            ReinstateScreen(m_StoredStates[iLastState].ScreenList[1], 0, CurrentState);
                                                            break;
                                                        }
                                                    case 0:
                                                        {
                                                            // Ecran 0 retrouvé dans le [1], écran 1 retrouvé dans le [0].
                                                            // rien à faire (?)
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            // Ecran 0 retrouvé dans le [1], écran 1 retrouvé dans le [1].
                                                            // Impossible
                                                            break;
                                                        }
                                                    default:
                                                        break;
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                // Once we have made such a recall, the Redo menu must be disabled...
                cm.BlockRedo();

                UpdateCaptureBuffers();
                
                // Mettre à jour menus et Status bar
                UpdateStatusBar();
                OrganizeCommonControls();
                OrganizeMenus();

                m_StoredStates.RemoveAt(iLastState);
            }
        }
        private void ReinstateScreen(ScreenState _OldScreen, int _iNewPosition, ScreenManagerState _CurrentState)
        {
            CommandManager cm = CommandManager.Instance();

            if (_iNewPosition > _CurrentState.ScreenList.Count - 1)
            {
                // We need a new screen.
                ICommand caps = new CommandAddPlayerScreen(this, false);
                CommandManager.LaunchCommand(caps);

                if (_OldScreen.Loaded)
                {
                    ReloadScreen(_OldScreen, _iNewPosition + 1);
                }
            }
            else
            {
                if (_OldScreen.Loaded)
                {
                    ReloadScreen(_OldScreen, _iNewPosition + 1);
                }
                else if (_CurrentState.ScreenList[_iNewPosition].Loaded)
                {
                    // L'ancien n'est pas chargé mais le nouveau l'est.
                    // => unload movie.
                    RemoveScreen(_iNewPosition, false);

                    ICommand caps = new CommandAddPlayerScreen(this, false);
                    CommandManager.LaunchCommand(caps);
                }
                else
                {
                    // L'ancien n'est pas chargé, le nouveau non plus.
                    // vérifier que les deux sont bien des players...
                }
            }
        }
        private bool RemoveScreen(int _iPosition, bool _bStoreState)
        {
            ICommand crs = new CommandRemoveScreen(this, _iPosition, _bStoreState);
            CommandManager.LaunchCommand(crs);

            bool cancelled = m_bCancelLastCommand;
            if (cancelled)
            {
            	CommandManager cm = CommandManager.Instance();
                cm.UnstackLastCommand();
                m_bCancelLastCommand = false;
            }
            
            return !cancelled;
        }
        private void ReloadScreen(ScreenState _OldScreen, int _iNewPosition)
        {
        	if(!File.Exists(_OldScreen.FilePath))
        	    return;
            
        	// We instantiate and launch it like a simple command (not undoable).
            ICommand clmis = new CommandLoadMovieInScreen(this, _OldScreen.FilePath, _iNewPosition, false);
            CommandManager.LaunchCommand(clmis);
            
            // Check that everything went well
            // Potential problem : the video was deleted between do and undo.
            // _iNewPosition should always point to a valid position here.
            if (screenList[_iNewPosition-1].Full)
            {
            	PlayerScreen ps = m_ActiveScreen as PlayerScreen;
            	if(ps != null)
            	{
                	ps.FrameServer.Metadata.Load(_OldScreen.MetadataString, false);
                	ps.m_PlayerScreenUI.PostImportMetadata();
            	}
            }
        }
        #endregion
    }
}
