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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using Videa.Services;
using VideaPlayerServer;
using System.IO;

namespace Videa.ScreenManager
{
	
    public class ScreenManagerKernel : IKernel , IMessageFilter
    {
        #region Imports Win32
        
        const int WM_KEYDOWN = 0x100;
        const int TIME_PERIODIC = 0x01;
        const int TIME_KILL_SYNCHRONOUS = 0x0100;

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution, MMTimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeKillEvent(uint timerEventId);
        #endregion

        #region enums
        public enum SyncStep
        {
            Initial,
            StartingWait,
            BothPlaying,
            EndingWait
        }
        #endregion

        #region Delegates

        public delegate void OrganizeMenuProxy(Delegate _method);
        private OrganizeMenuProxy m_OrganizeMenuProxy;
        private delegate void DelegateOrganizeMenu();
        
        //private delegate void DelegateUpdateTrakerFrame(int _iFrame);
        //private DelegateUpdateTrakerFrame m_DelegateUpdateTrakerFrame;

        // Internes
        public delegate void MMTimerEventHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2);
        private MMTimerEventHandler m_DelegateMMTimerEventHandler;
        
        #endregion

        #region Properties
        public UserControl UI
        {
            get { return _UI; }
            set { _UI = value; }
        }
        public ResourceManager resManager
        {
            get { return m_resManager; }
            set { m_resManager = value; }
        }
        public bool CancelLastCommand
        {
            get { return m_bCancelLastCommand; } // Unused.
            set { m_bCancelLastCommand = value; }
        }
        #endregion

        #region Members

        private UserControl _UI;
        private ResourceManager m_resManager;
        private bool m_bCancelLastCommand = false;

        //List of screens ( 0..n )
        public List<AbstractScreen> screenList;
        
        private bool m_bAdjustingImage = false;
        public AbstractScreen m_ActiveScreen = null;
        private bool m_bCommonControlsVisible = false;

        // Video Filters
        private AbstractVideoFilter[] m_VideoFilters;
        
        //Menus
        public ToolStripMenuItem    m_mnuCloseFile;
        public ToolStripMenuItem    m_mnuCloseFile2;
        private ToolStripMenuItem   mnuSave;
        private ToolStripMenuItem   mnuLoadAnalysis;

        public ToolStripMenuItem    mnuSwapScreens;
        public ToolStripMenuItem    mnuToggleCommonCtrls;

        public ToolStripMenuItem    m_mnuCatchImage;
        public ToolStripMenuItem    m_mnuDeinterlace;
        public ToolStripMenuItem    m_mnuMirror;
        public ToolStripMenuItem    m_mnuGrid;
        public ToolStripMenuItem    m_mnu3DPlane;

        #region Synchronization
        
        private uint    m_IdMultimediaTimer = 0; // Timer servant à contrôler l'état d'avancement de chaque vidéo pour prise de décision d'arrêt/relance.          
        
        private bool    m_bSynching = false;
        private int     m_iSyncLag = 0;              // Sync Lag in Frames, for static sync.
        private int     m_iSyncLagMilliseconds = 0;  // Sync lag in Milliseconds, for dynamic sync.
        
        // Static Sync Positions
        private int m_iCurrentFrame = 0;            // Current frame in trkFrame...
        private int m_iLeftSyncFrame = 0;           // Sync reference in the left video
        private int m_iRightSyncFrame = 0;          // Sync reference in the right video
        private int m_iMaxFrame = 0;                // Max du trkFrame

        // Dynamic Sync Flags.
        private bool m_bRightIsStarting = false;    // true when the video is between [0] and [1] frames.
        private bool m_bLeftIsStarting = false;
        private bool m_bLeftIsCatchingUp = false;   // true when the video is the only one left running, heading towards end.
        private bool m_bRightIsCatchingUp = false;  // Or when the other video is waiting the lag.

        #endregion

        private bool m_bAllowKeyboardHandler;

        private List<ScreenManagerState> mStoredStates;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & initialization
        public ScreenManagerKernel()
        {
            log.Debug("Module Construction : ScreenManager.");

            Application.AddMessageFilter(this);
            
            //Gestion i18n
            resManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            screenList = new List<AbstractScreen>();
            mStoredStates = new List<ScreenManagerState>();
            
            // Callbacks du MultimediaTimer.
            m_DelegateMMTimerEventHandler = new MMTimerEventHandler(MultimediaTimer_Tick);
            m_bAllowKeyboardHandler = true;

            UI = new ScreenManagerUserInterface();

            PlugDelegates();
            InitializeVideoFilters();
            
            // Registers our exposed functions to the DelegatePool.
            DelegatesPool dp = DelegatesPool.Instance();

            dp.LoadMovieInScreen = DoLoadMovieInScreen;
            dp.StopPlaying = DoStopPlaying;
            dp.DeactivateKeyboardHandler = DoDeactivateKeyboardHandler;
            dp.ActivateKeyboardHandler = DoActivateKeyboardHandler;
            dp.VideoProcessingDone = DoVideoProcessingDone;
        }
        private void PlugDelegates()
        {

            m_OrganizeMenuProxy = new OrganizeMenuProxy(((ScreenManagerUserInterface)this.UI).OrganizeMenuProxy);

            ((ScreenManagerUserInterface)this.UI).m_CallbackDropLoadMovie += new ScreenManagerUserInterface.CallbackDropLoadMovie(DropLoadMovie);
            ((ScreenManagerUserInterface)this.UI).GotoFirst += new ScreenManagerUserInterface.GotoFirstHandler(CommonCtrlsGotoFirst);
            ((ScreenManagerUserInterface)this.UI).GotoLast += new ScreenManagerUserInterface.GotoLastHandler(CommonCtrlsGotoLast);
            ((ScreenManagerUserInterface)this.UI).GotoPrev += new ScreenManagerUserInterface.GotoPrevHandler(CommonCtrlsGotoPrev);
            ((ScreenManagerUserInterface)this.UI).GotoNext += new ScreenManagerUserInterface.GotoNextHandler(CommonCtrlsGotoNext);
            ((ScreenManagerUserInterface)this.UI).Play += new ScreenManagerUserInterface.PlayHandler(CommonCtrlsPlay);
            ((ScreenManagerUserInterface)this.UI).Swap += new ScreenManagerUserInterface.SwapHandler(CommonCtrlsSwap);
            ((ScreenManagerUserInterface)this.UI).Sync += new ScreenManagerUserInterface.SyncHandler(CommonCtrlsSync);
            ((ScreenManagerUserInterface)this.UI).PositionChanged += new ScreenManagerUserInterface.PositionChangedHandler(CommonCtrlsPositionChanged);

            ((ScreenManagerUserInterface)this.UI).m_ThumbsViewer.m_CallBackLoadMovie += new ScreenManagerUserInterface.CallbackDropLoadMovie(DropLoadMovie);

        }
        private void InitializeVideoFilters()
        {
        	// Creates Video Filters
        	m_VideoFilters = new AbstractVideoFilter[(int)VideoFilterType.NumberOfVideoFilters];
        	
        	m_VideoFilters[(int)VideoFilterType.AutoLevels] = new VideoFilterAutoLevels();
        	m_VideoFilters[(int)VideoFilterType.AutoContrast] = new VideoFilterContrast();
        	m_VideoFilters[(int)VideoFilterType.Sharpen] = new VideoFilterSharpen();
        	m_VideoFilters[(int)VideoFilterType.EdgesOnly] = new VideoFilterEdgesOnly();
			m_VideoFilters[(int)VideoFilterType.Mosaic] = new VideoFilterMosaic();
        	m_VideoFilters[(int)VideoFilterType.Reverse] = new VideoFilterReverse();
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

            //Close File (1)
            ToolStripMenuItem mnuCloseFile = new ToolStripMenuItem();
            mnuCloseFile.Tag = new ItemResourceInfo(resManager, "Generic_Close");
            mnuCloseFile.Text = ((ItemResourceInfo)mnuCloseFile.Tag).resManager.GetString(((ItemResourceInfo)mnuCloseFile.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuCloseFile.Enabled = false;
            mnuCloseFile.Click += new EventHandler(mnuCloseFileOnClick);

            mnuCloseFile.MergeIndex = 2;
            mnuCloseFile.MergeAction = MergeAction.Insert;

            m_mnuCloseFile = mnuCloseFile;

            //Close File (2)
            ToolStripMenuItem mnuCloseFile2 = new ToolStripMenuItem();
            mnuCloseFile2.Tag = new ItemResourceInfo(resManager, "Generic_Close");
            mnuCloseFile2.Text = ((ItemResourceInfo)mnuCloseFile2.Tag).resManager.GetString(((ItemResourceInfo)mnuCloseFile2.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuCloseFile2.Enabled = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Click += new EventHandler(mnuCloseFile2OnClick);

            mnuCloseFile2.MergeIndex = 3;
            mnuCloseFile2.MergeAction = MergeAction.Insert;

            m_mnuCloseFile2 = mnuCloseFile2;


            // Save Analysis or Video
            mnuSave = new ToolStripMenuItem();
            mnuSave.Tag = new ItemResourceInfo(resManager, "mnuSave");
            mnuSave.Text = ((ItemResourceInfo)mnuSave.Tag).resManager.GetString(((ItemResourceInfo)mnuSave.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuSave.Click += new EventHandler(mnuSaveOnClick);
            mnuSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
          
            mnuSave.MergeIndex = 5;
            mnuSave.MergeAction = MergeAction.Insert;

            // Export to PDF
            /*mnuExportToPDF = new ToolStripMenuItem();
            mnuExportToPDF.Tag = new ItemResourceInfo(resManager, "mnuExportToPDF");
            mnuExportToPDF.Text = ((ItemResourceInfo)mnuExportToPDF.Tag).resManager.GetString(((ItemResourceInfo)mnuExportToPDF.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuExportToPDF.Click += new EventHandler(mnuExportToPDFOnClick);
            mnuExportToPDF.MergeIndex = 6;

            mnuExportToPDF.MergeAction = MergeAction.Insert;

            ToolStripSeparator mnuSepFile = new ToolStripSeparator();
            mnuSepFile.MergeIndex = 7;
            mnuSepFile.MergeAction = MergeAction.Insert;
            */

            // Load Analysis
            mnuLoadAnalysis = new ToolStripMenuItem();
            mnuLoadAnalysis.Tag = new ItemResourceInfo(resManager, "mnuLoadAnalysis");
            mnuLoadAnalysis.Text = ((ItemResourceInfo)mnuLoadAnalysis.Tag).resManager.GetString(((ItemResourceInfo)mnuLoadAnalysis.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuLoadAnalysis.Click += new EventHandler(mnuLoadAnalysisOnClick);
            
            mnuLoadAnalysis.MergeIndex = 6;
            mnuLoadAnalysis.MergeAction = MergeAction.Insert;


            //---------------------------------
            //Organisation du sous menu File
            //---------------------------------
            ToolStripItem[] subFile = new ToolStripItem[] { mnuCloseFile, mnuCloseFile2, mnuSave, /*mnuExportToPDF, mnuSepFile, */mnuLoadAnalysis };
            mnuCatchFile.DropDownItems.AddRange(subFile);
            #endregion

            #region View
            ToolStripMenuItem mnuCatchScreens = new ToolStripMenuItem();
            mnuCatchScreens.MergeIndex = 2; // (Screens)
            mnuCatchScreens.MergeAction = MergeAction.MatchOnly;

            // One player
            ToolStripMenuItem mnuOnePlayer = new ToolStripMenuItem();
            mnuOnePlayer.Tag = new ItemResourceInfo(resManager, "mnuOnePlayer");
            mnuOnePlayer.Text = ((ItemResourceInfo)mnuOnePlayer.Tag).resManager.GetString(((ItemResourceInfo)mnuOnePlayer.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuOnePlayer.Click += new EventHandler(mnuOnePlayerOnClick);
            mnuOnePlayer.MergeAction = MergeAction.Append;

            // Two players
            ToolStripMenuItem mnuTwoPlayers = new ToolStripMenuItem();
            mnuTwoPlayers.Tag = new ItemResourceInfo(resManager, "mnuTwoPlayers");
            mnuTwoPlayers.Text = ((ItemResourceInfo)mnuTwoPlayers.Tag).resManager.GetString(((ItemResourceInfo)mnuTwoPlayers.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTwoPlayers.Click += new EventHandler(mnuTwoPlayersOnClick);
            mnuTwoPlayers.MergeAction = MergeAction.Append;

			// One capture
            ToolStripMenuItem mnuOneCapture = new ToolStripMenuItem();
            mnuOneCapture.Tag = new ItemResourceInfo(resManager, "mnuOneCapture");
            mnuOneCapture.Text = ((ItemResourceInfo)mnuOneCapture.Tag).resManager.GetString(((ItemResourceInfo)mnuOneCapture.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuOneCapture.Click += new EventHandler(mnuOneCaptureOnClick);
            mnuOneCapture.MergeAction = MergeAction.Append;
            mnuOneCapture.Enabled = false;

            // Two captures
            ToolStripMenuItem mnuTwoCaptures = new ToolStripMenuItem();
            mnuTwoCaptures.Tag = new ItemResourceInfo(resManager, "mnuTwoCaptures");
            mnuTwoCaptures.Text = ((ItemResourceInfo)mnuTwoCaptures.Tag).resManager.GetString(((ItemResourceInfo)mnuTwoCaptures.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTwoCaptures.Click += new EventHandler(mnuTwoCapturesOnClick);
            mnuTwoCaptures.MergeAction = MergeAction.Append;
			mnuTwoCaptures.Enabled = false;
            
            // Two mixed
            ToolStripMenuItem mnuTwoMixed = new ToolStripMenuItem();
            mnuTwoMixed.Tag = new ItemResourceInfo(resManager, "mnuTwoMixed");
            mnuTwoMixed.Text = ((ItemResourceInfo)mnuTwoMixed.Tag).resManager.GetString(((ItemResourceInfo)mnuTwoMixed.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTwoMixed.Click += new EventHandler(mnuTwoMixedOnClick);
            mnuTwoMixed.MergeAction = MergeAction.Append;
            mnuTwoMixed.Enabled = false;
            
            //Swap - activé seulement si DualFull ?
            mnuSwapScreens = new ToolStripMenuItem();
            mnuSwapScreens.Tag = new ItemResourceInfo(resManager, "mnuSwapScreens");
            mnuSwapScreens.Text = ((ItemResourceInfo)mnuSwapScreens.Tag).resManager.GetString(((ItemResourceInfo)mnuSwapScreens.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuSwapScreens.Enabled = false;
            mnuSwapScreens.Click += new EventHandler(mnuSwapScreensOnClick);
            mnuSwapScreens.MergeAction = MergeAction.Append;

            //Toggle Common Controls - activé seulement si DualFull.
            mnuToggleCommonCtrls = new ToolStripMenuItem();
            mnuToggleCommonCtrls.Tag = new ItemResourceInfo(resManager, "mnuToggleCommonCtrls");
            mnuToggleCommonCtrls.Text = ((ItemResourceInfo)mnuToggleCommonCtrls.Tag).resManager.GetString(((ItemResourceInfo)mnuToggleCommonCtrls.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuToggleCommonCtrls.Enabled = false;
            mnuToggleCommonCtrls.ShortcutKeys = Keys.F5;
            mnuToggleCommonCtrls.Click += new EventHandler(mnuToggleCommonCtrlsOnClick);
            mnuToggleCommonCtrls.MergeAction = MergeAction.Append;

            ToolStripSeparator mnuSepView = new ToolStripSeparator();
            
            //---------------------------------
            //Organisation du sous menu Screens
            //---------------------------------
            ToolStripItem[] subScreens = new ToolStripItem[] { mnuOnePlayer, mnuTwoPlayers, mnuOneCapture, mnuTwoCaptures, mnuTwoMixed, mnuSepView, mnuSwapScreens, mnuToggleCommonCtrls };
            mnuCatchScreens.DropDownItems.AddRange(subScreens);
            #endregion

            #region Image
            m_mnuCatchImage = new ToolStripMenuItem();
            m_mnuCatchImage.MergeIndex = 3; // (Image)
            m_mnuCatchImage.MergeAction = MergeAction.MatchOnly;

            // Deinterlace
            m_mnuDeinterlace = new ToolStripMenuItem();
            m_mnuDeinterlace.Tag = new ItemResourceInfo(resManager, "mnuDeinterlace");
            m_mnuDeinterlace.Text = ((ItemResourceInfo)m_mnuDeinterlace.Tag).resManager.GetString(((ItemResourceInfo)m_mnuDeinterlace.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_mnuDeinterlace.Checked = false;
            m_mnuDeinterlace.ShortcutKeys = Keys.Control | Keys.D;
            m_mnuDeinterlace.Click += new EventHandler(mnuDeinterlaceOnClick);
            m_mnuDeinterlace.MergeAction = MergeAction.Append;

            // Brightness
            /*m_mnuBrightness = new ToolStripMenuItem();
            m_mnuBrightness.Tag = new ItemResourceInfo(resManager, "mnuBrightness");
            m_mnuBrightness.Text = ((ItemResourceInfo)m_mnuBrightness.Tag).resManager.GetString(((ItemResourceInfo)m_mnuBrightness.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_mnuBrightness.Click += new EventHandler(mnuBrightnessOnClick);
            m_mnuBrightness.MergeAction = MergeAction.Append;*/

            // Mirror
            m_mnuMirror = new ToolStripMenuItem();
            m_mnuMirror.Tag = new ItemResourceInfo(resManager, "mnuMirror");
            m_mnuMirror.Text = ((ItemResourceInfo)m_mnuMirror.Tag).resManager.GetString(((ItemResourceInfo)m_mnuMirror.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_mnuMirror.Checked = false;
            m_mnuMirror.ShortcutKeys = Keys.Control | Keys.M;
            m_mnuMirror.Click += new EventHandler(mnuMirrorOnClick);
            m_mnuMirror.MergeAction = MergeAction.Append;

            ToolStripSeparator mnuSep = new ToolStripSeparator();
            ToolStripSeparator mnuSep2 = new ToolStripSeparator();
            ToolStripSeparator mnuSep3 = new ToolStripSeparator();

            // Grid
            m_mnuGrid = new ToolStripMenuItem();
            m_mnuGrid.Tag = new ItemResourceInfo(resManager, "mnuGrid");
            m_mnuGrid.Text = ((ItemResourceInfo)m_mnuGrid.Tag).resManager.GetString(((ItemResourceInfo)m_mnuGrid.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_mnuGrid.Checked = false;
            m_mnuGrid.ShortcutKeys = Keys.Control | Keys.G;
            m_mnuGrid.Click += new EventHandler(mnuGridOnClick);
            m_mnuGrid.MergeAction = MergeAction.Append;

            // 3D Plane
            m_mnu3DPlane = new ToolStripMenuItem();
            m_mnu3DPlane.Tag = new ItemResourceInfo(resManager, "mnu3DPlane");
            m_mnu3DPlane.Text = ((ItemResourceInfo)m_mnu3DPlane.Tag).resManager.GetString(((ItemResourceInfo)m_mnu3DPlane.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            m_mnu3DPlane.Checked = false;
            m_mnu3DPlane.ShortcutKeys = Keys.Control | Keys.P;
            m_mnu3DPlane.Click += new EventHandler(mnu3DPlaneOnClick);
            m_mnu3DPlane.MergeAction = MergeAction.Append;

            ConfigureVideoFilterMenus(null, true);

            //---------------------------------
            //Organisation du sous menu Image
            //---------------------------------
            m_mnuCatchImage.DropDownItems.AddRange(new ToolStripItem[] 
													{ 
                                                   		m_mnuDeinterlace, 
                                                   		mnuSep, 
                                                   		m_VideoFilters[(int)VideoFilterType.AutoLevels].Menu,  
                                                   		m_VideoFilters[(int)VideoFilterType.AutoContrast].Menu,  
                                                   		m_VideoFilters[(int)VideoFilterType.Sharpen].Menu, 
                                                   		mnuSep2, 
                                                   		m_mnuMirror, 
                                                   		m_VideoFilters[(int)VideoFilterType.EdgesOnly].Menu, 
                                                   		mnuSep3, 
                                                   		m_mnuGrid, 
                                                   		m_mnu3DPlane});
            #endregion

            #region Motion

            ToolStripMenuItem mnuCatchMotion = new ToolStripMenuItem();
            mnuCatchMotion.MergeIndex = 4;
            mnuCatchMotion.MergeAction = MergeAction.MatchOnly;

            mnuCatchMotion.DropDownItems.AddRange(new ToolStripItem[] 
                                                  {  
                                                  		m_VideoFilters[(int)VideoFilterType.Mosaic].Menu,
                                                  		m_VideoFilters[(int)VideoFilterType.Reverse].Menu});
            
            #endregion
            
            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchFile, mnuCatchScreens, m_mnuCatchImage, mnuCatchMotion });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, _menu);

            // No sub modules.
        }
        public void ExtendToolBar(ToolStrip _toolbar)
        {
            // TODO: Expose workspaces presets as buttons.

            //ToolStrip toolbar = new ToolStrip();

            /* Déroulement:
             * 1. Instanciation de l'item de menu.
             * 2. Association du texte du menu avec une resource.
             * 3. Affectation du contenu de la resource dans le texte.
             * 4. Affectation d'un Event Handler pour gérer l'action à lancer.
             * 5. Définition du mode d'insertion dans le menu général.
             * 6. Détermination de l'index d'insertion dans le menu général.
             * 7. Organisation des sous menus entre eux et avec le parent direct.
             * Note : les menus intraduisibles doivent avoir un .Tag == null.
            */


            /*
            ToolStripButton toolOnePlayer = new ToolStripButton();
            toolOnePlayer.Tag = new ItemResourceInfo(resManager, "toolOnePlayer", "mnuMonoPlayer");
            toolOnePlayer.Name                  = "toolOnePlayer";
            toolOnePlayer.DisplayStyle          = ToolStripItemDisplayStyle.Image;
            toolOnePlayer.Image                 = (System.Drawing.Image)(Videa.ScreenManager.Properties.Resources.MonoPlayer3);
            toolOnePlayer.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolOnePlayer.AutoSize              = true;
            toolOnePlayer.ToolTipText           = ((ItemResourceInfo)toolOnePlayer.Tag).resManager.GetString(((ItemResourceInfo)toolOnePlayer.Tag).strToolTipText, Thread.CurrentThread.CurrentUICulture); ;
            toolOnePlayer.Click += new EventHandler(mnuMonoPlayerOnClick);

            
            ToolStripButton toolTwoPlayers = new ToolStripButton();
            toolTwoPlayers.Tag = new ItemResourceInfo(resManager, "toolTwoPlayers", "mnuDoublePlayer");
            toolTwoPlayers.Name = "toolTwoPlayers";
            //toolTwoPlayers.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText; 
            toolTwoPlayers.DisplayStyle = ToolStripItemDisplayStyle.Image;
            //toolTwoPlayers.Text = ((ItemResourceInfo)toolTwoPlayers.Tag).resManager.GetString(((ItemResourceInfo)toolTwoPlayers.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            //toolTwoPlayers.TextAlign = ContentAlignment.BottomCenter;
            //toolTwoPlayers.TextImageRelation = TextImageRelation.ImageAboveText;
            toolTwoPlayers.Image = (System.Drawing.Image)(Videa.ScreenManager.Properties.Resources.DualPlayer2);
            toolTwoPlayers.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolTwoPlayers.AutoSize = true;
            toolTwoPlayers.ToolTipText = ((ItemResourceInfo)toolTwoPlayers.Tag).resManager.GetString(((ItemResourceInfo)toolTwoPlayers.Tag).strToolTipText, Thread.CurrentThread.CurrentUICulture); ;
            toolTwoPlayers.Click += new EventHandler(mnuDoublePlayerOnClick);
            



            //Organisation de la Toolbar
            ToolStripItem[] allButtons = new ToolStripItem[] { toolOnePlayer, toolTwoPlayers };
            toolbar.Items.AddRange(allButtons);
            
            toolbar.AllowMerge = true;

            toolStrips.Add(toolbar);
            */



            // No sub modules.


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
            log.Debug("Refresh UI Culture at ScreenManager level.");
            OrganizeMenus();
            UpdateStatusBar();

            ((ScreenManagerUserInterface)this.UI).RefreshUICulture(resManager);

            // Screens.
            foreach (AbstractScreen screen in screenList)
            {
                screen.refreshUICulture();
            }

            ((ScreenManagerUserInterface)UI).DisplaySyncLag(m_iSyncLag);

            // No submodules.
        }
        public void CloseSubModules()
        {
            // No sub modules to close.
            // Close this module
            foreach (AbstractScreen screen in screenList)
            {
                screen.CloseScreen();
            }
        }
        #endregion

        
        public void UpdateStatusBar()
        {
            //------------------------------------------------------------------
            // Fonction appellée sur RefreshUiCulture, CommandShowScreen (dans le ScreenManager)
            // 
            // et appelant le module supérieur (Supervisor)  
            //
            // Mettre à jour les infos de la status bar.
            // Fabriquer la chaîne qui ira dans l'espace dédié au ScreenManager.
            //------------------------------------------------------------------

            String StatusString = "";

            switch(screenList.Count)
            {
                case 1:
                    {
                        if (screenList[0].GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                        {
                            if(((PlayerScreen)screenList[0]).m_bIsMovieLoaded)
                            {
                                StatusString += ((PlayerScreen)screenList[0]).m_sFileName;
                            }
                            else
                            {
                                // Un seul écran de lecture, avec rien dedans.
                                StatusString += StatusString += resManager.GetString("statusEmptyScreen", Thread.CurrentThread.CurrentUICulture); 
                            }
                        }
                        else
                        {
                            // Un seul écran de capture
                        }

                        break;
                    }
                
                case 2:
                    {
                        //bool bLeftIsFull = false;

                        if (screenList[0].GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                        {
                            if(((PlayerScreen)screenList[0]).m_bIsMovieLoaded)
                            {
                                StatusString += ((PlayerScreen)screenList[0]).m_sFileName;
                                //bLeftIsFull = true;
                            }
                            else
                            {
                                // Ecran de gauche en lecture, avec rien dedans.
                                StatusString += StatusString += resManager.GetString("statusEmptyScreen", Thread.CurrentThread.CurrentUICulture); ;
                            }
                        }
                        else
                        {
                            // Ecran de gauche en capture.
                        }

                        if (screenList[1].GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                        {
                            StatusString += " | ";
                            
                            if (((PlayerScreen)screenList[1]).m_bIsMovieLoaded)
                            {
                                StatusString += ((PlayerScreen)screenList[1]).m_sFileName;
                            }
                            else
                            {
                                // Ecran de droite en lecture, avec rien dedans.
                                StatusString += resManager.GetString("statusEmptyScreen", Thread.CurrentThread.CurrentUICulture);
                            }
                        }
                        else
                        {
                            // Ecran de droite en capture.
                        }

                        break;
                    }
                default:
                    break;

            }

            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.UpdateStatusBar != null)
            {
                dp.UpdateStatusBar(StatusString);
            }
        }
        public void OrganizeMenus()
        {
            DoOrganizeMenu();
        }
        private void DoOrganizeMenu()
        {
        	// Show / hide menus depending on state of active screen
        	// and global screen configuration.
        	
            #region Menus depending only on the state of the active screen
            bool bActiveScreenIsEmpty = false;
            if (m_ActiveScreen != null && screenList.Count > 0)
            {
            	PlayerScreen player = m_ActiveScreen as PlayerScreen;
                if (player != null)
                {
                    // 1. Video is loaded : save-able and analysis is loadable.
                    if (player.m_bIsMovieLoaded)
                    {
                        mnuSave.Enabled = true;
                        mnuLoadAnalysis.Enabled = true;
                        m_mnuDeinterlace.Checked = player.Deinterlaced;
                        m_mnuMirror.Checked = player.Mirrored;
                        m_mnuGrid.Checked = player.ShowGrid;
                        m_mnu3DPlane.Checked = player.Show3DPlane;

                        // Video Filters menus
                        ConfigureVideoFilterMenus(player, false);
                    }
                    else
                    {
                        // Active screen is an empty player screen.
                        bActiveScreenIsEmpty = true;
                    }
                }
                else
                {
                    // Active screen is not a PlayerScreen.
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
                mnuLoadAnalysis.Enabled = false;
                mnuSave.Enabled = false;
				m_mnuDeinterlace.Checked = false;
				m_mnuMirror.Checked = false;
                m_mnuGrid.Checked = false;
                m_mnu3DPlane.Checked = false;
				
                // Video Filters menus
				ConfigureVideoFilterMenus(null, true);
            }
            #endregion

            #region Menus depending on the specifc screen configuration
            // File
            m_mnuCloseFile.Visible  = false;
            m_mnuCloseFile.Enabled  = false;
            m_mnuCloseFile2.Visible = false;
            m_mnuCloseFile2.Enabled = false;
            string strClosingText = ((ItemResourceInfo)m_mnuCloseFile.Tag).resManager.GetString(((ItemResourceInfo)m_mnuCloseFile.Tag).strText, Thread.CurrentThread.CurrentUICulture);

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
                        strClosingText = strClosingText + " - " + ((PlayerScreen)screenList[0]).m_sFileName;
                        m_mnuCloseFile.Text = strClosingText;
                        m_mnuCloseFile.Enabled = true;
                        m_mnuCloseFile.Visible = true;

                        m_mnuCloseFile2.Visible = false;
                        m_mnuCloseFile2.Enabled = false;
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                    	bAllScreensEmpty = true;	
                    }
                    break;

                case 2:

                    // Two screens

                    mnuSwapScreens.Enabled = true;
                    mnuToggleCommonCtrls.Enabled = true;
                    
                    m_bCommonControlsVisible = !((ScreenManagerUserInterface)UI).splitScreensPanel.Panel2Collapsed; 

                    // Left Screen
                    if (screenList[0] is PlayerScreen)
                    {
                        if (screenList[0].Full)
                        {
                            bAllScreensEmpty = false;
                            
                            string strCompleteClosingText = strClosingText + " - " + ((PlayerScreen)screenList[0]).m_sFileName;
                            m_mnuCloseFile.Text = strCompleteClosingText;
                            m_mnuCloseFile.Enabled = true;
                            m_mnuCloseFile.Visible = true;
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
                            
                            string strCompleteClosingText = strClosingText + " - " + ((PlayerScreen)screenList[1]).m_sFileName;
                            m_mnuCloseFile2.Text = strCompleteClosingText;
                            m_mnuCloseFile2.Enabled = true;
                            m_mnuCloseFile2.Visible = true;
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

                m_mnuCloseFile.Text = strClosingText;
                m_mnuCloseFile.Visible = true;
                m_mnuCloseFile.Enabled = false;

                m_mnuCloseFile2.Visible = false;
            }
            #endregion

        }
        private void ConfigureVideoFilterMenus(PlayerScreen _player, bool _bDisableAll)
        {
			// determines if any given video filter menu should be
			// visible, enabled, checked...
        	
        	//----------------------------------------------------------
        	// 1. Is a given menu enabled ? (analysis mode/regular mode)
        	//----------------------------------------------------------
        	bool bEnable = false;
        	
        	if(!_bDisableAll && _player != null)
        	{
        		bEnable = _player.IsInAnalysisMode;
        	}
        	        	
    		foreach(AbstractVideoFilter vf in m_VideoFilters)
        	{
        		vf.Menu.Enabled = bEnable;
        	}
            
            // Associate the input frames
            if(bEnable)
            {
            	List<DecompressedFrame> frameList = _player.m_PlayerScreenUI.m_PlayerServer.m_FrameList;
	            
            	foreach(AbstractVideoFilter vf in m_VideoFilters)
            	{
            		vf.FrameList = frameList;
            	}
            }

            //----------------------------------------------------------
            // 2. Is a given menu visible ?
            //----------------------------------------------------------
            foreach(AbstractVideoFilter vf in m_VideoFilters)
        	{
            	if(vf.Experimental)
            	{
            		// Experimental filters = depends on current release type.
            		vf.Menu.Visible = PreferencesManager.ExperimentalRelease;
            	}
            	else
            	{
            		// Production filters = always visible.
            		vf.Menu.Visible = true;
            	}
        	}
                      
            //----------------------------------------------------------
            // 3. Is a given boolean menu checked ?
        	//----------------------------------------------------------
            
        	// Uncheck all togglable menus
        	foreach(AbstractVideoFilter vf in m_VideoFilters)
        	{
        		vf.Menu.Checked = false;
        	}
        	
        	if(_player != null)
        	{
        		if(_player.DrawtimeFilterType > -1)
        		{
        			m_VideoFilters[_player.DrawtimeFilterType].Menu.Checked = true;
        		}
        	}
        }

        #region Menus events handlers

        #region File
        private void mnuCloseFileOnClick(object sender, EventArgs e)
        {
            //--------------------------------------------------------------------
            // Dans cet event Handler, on ferme toujours le premier écran.
            // Si on a pu cliquer, c'est qu'il y a forcément une vidéo de chargée 
            // dans le premier écran.
            //--------------------------------------------------------------------
            // Supprimer explicitement l'écran de la liste.
            IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
            CommandManager cm = CommandManager.Instance();
            cm.LaunchUndoableCommand(crs);

            //----------------------------------
            // doublon avec commandshowscreens ?
            //----------------------------------
            
            if (!m_bCancelLastCommand)
            {
                // Cleanup screen panel.
                ((ScreenManagerUserInterface)UI).splitScreens.Panel1.Controls.Clear();

                // ?
                
                switch (screenList.Count)
                {
                    case 1:
                        // Plus aucun écran.
                        ((ScreenManagerUserInterface)UI).pnlScreens.Visible = false;
                        ((ScreenManagerUserInterface)UI).AllowDrop = true;
                        ((ScreenManagerUserInterface)UI).splitScreens.Panel1.AllowDrop = false;
                        ((ScreenManagerUserInterface)UI).splitScreens.Panel2.AllowDrop = false;
                        m_ActiveScreen = null;
                        break;

                    case 2:
                        // Dupliquer le second écran dans le premier...
                        ((ScreenManagerUserInterface)UI).splitScreens.Panel1.Controls.Add(((ScreenManagerUserInterface)UI).splitScreens.Panel2.Controls[0]);

                        // Supprimer le second.
                        ((ScreenManagerUserInterface)UI).splitScreens.Panel2.Controls.Clear();

                        // Ne garder que le premier de visible.
                        ((ScreenManagerUserInterface)UI).splitScreens.Panel2Collapsed = true;
                        ((ScreenManagerUserInterface)UI).splitScreensPanel.Panel2Collapsed = true;

                        // TODO: First screen becomes the active screen...
                        break;
                    default:
                        break;
                }

                // Afficher les écrans.
                ICommand css = new CommandShowScreens(this);
                CommandManager.LaunchCommand(css);

                OrganizeMenus();
            }
            else
            {
                cm.UnstackLastCommand();
                m_bCancelLastCommand = false;
            }
        }
        private void mnuCloseFile2OnClick(object sender, EventArgs e)
        {
            //--------------------------------------------------------------------
            // Dans cet event Handler, on ferme toujours le second écran.
            // Si on a pu cliquer, c'est qu'il y a forcément une vidéo de chargée 
            // dans le second écran.
            // Donc il y a deux écrans, mais le premier n'est pas forcément plein
            //--------------------------------------------------------------------
            // Supprimer explicitement l'écran de la liste...
            IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
            CommandManager cm = CommandManager.Instance();
            cm.LaunchUndoableCommand(crs);

            if (!m_bCancelLastCommand)
            {

                // Supprimer l'écran de la liste des contrôles du panel
                ((ScreenManagerUserInterface)UI).splitScreens.Panel2.Controls.Clear();

                // Ne garder que le premier de visible.
                ((ScreenManagerUserInterface)UI).splitScreens.Panel2Collapsed = true;
                ((ScreenManagerUserInterface)UI).splitScreensPanel.Panel2Collapsed = true;

                // Afficher les écrans.
                ICommand css = new CommandShowScreens(this);
                CommandManager.LaunchCommand(css);

                // TODO : The other screen becomes the active screen.

                OrganizeMenus();
            }
            else
            {
                cm.UnstackLastCommand();
                m_bCancelLastCommand = false;
            }

        }
        public void mnuSaveOnClick(object sender, EventArgs e)
        {
            //---------------------------------------------------------------------------
            // Launch the dialog box where the user can choose to save the video,
            // the metadat or both.
            // Public because accessed from the closing command when we realize there are 
            // unsaved modified data.
            //---------------------------------------------------------------------------
            if (m_ActiveScreen != null)
            {
                if (m_ActiveScreen is PlayerScreen)
                {
                    DoStopPlaying();
                    DoDeactivateKeyboardHandler();
                    
                    formVideoExport fve = new formVideoExport((PlayerScreen)m_ActiveScreen);
                    fve.ShowDialog();
                    fve.Dispose();

                    DoActivateKeyboardHandler();
                }
            }
        }
        private void mnuExportToPDFOnClick(object sender, EventArgs e)
        {
            /*if (m_ActiveScreen != null)
            {
                if (m_ActiveScreen is PlayerScreen)
                {
                    if (((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata.HasData)
                    {
                        DoStopPlaying();

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Title = m_resManager.GetString("dlgExportToPDF_Title", Thread.CurrentThread.CurrentUICulture);
                        saveFileDialog.RestoreDirectory = true;
                        saveFileDialog.Filter = m_resManager.GetString("dlgExportToPDF_Filter", Thread.CurrentThread.CurrentUICulture);
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata.FullPath);

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string filePath = saveFileDialog.FileName;
                            if (filePath.Length > 0)
                            {
                                AnalysisExporterPDF aepdf = new AnalysisExporterPDF();
                                aepdf.Export(filePath, ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata);
                            }
                        }
                    }
                }
            }*/
        }
        private void mnuLoadAnalysisOnClick(object sender, EventArgs e)
        {
            if (m_ActiveScreen != null)
            {
                if (m_ActiveScreen is PlayerScreen)
                {
                    if (((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata.HasData)
                    {
                        // TODO : Merge mechanics.
                        LoadAnalysis();
                    }
                    else
                    {
                        LoadAnalysis();
                    }
                }
            }
        }
        private void LoadAnalysis()
        {
            DoStopPlaying();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = m_resManager.GetString("dlgLoadAnalysis_Title", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = m_resManager.GetString("dlgLoadAnalysis_Filter", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata.LoadFromFile(filePath);
                    ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.PostImportAnalysis();
                }
            }
        }
        #endregion

        #region View
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
							IUndoableCommand crs2 = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs2);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    	cm.LaunchUndoableCommand(caps);
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove capture.	
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    	cm.LaunchUndoableCommand(caps);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove capture.	
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand caps = new CommandAddPlayerScreen(this, true);
	                    	cm.LaunchUndoableCommand(caps);
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
								// remove [0].
								IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
								cm.LaunchUndoableCommand(crs);
	
								// todo : document me.
	                            if (!m_bCancelLastCommand)
	                            {
	                                // Hmmm. This should be under commandshowscreen responsibility ?
	                                ((ScreenManagerUserInterface)UI).splitScreens.Panel1.Controls.Add(((PlayerScreen)screenList[0]).m_PlayerScreenUI);
	                            }
	                            else
	                            {
	                                cm.UnstackLastCommand();
	                                m_bCancelLastCommand = false;
	                            }
							}
							else
							{
								// remove [1].
								IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
								cm.LaunchUndoableCommand(crs);
	
								// todo: doc.
	                            if(m_bCancelLastCommand)
	                            {
	                                cm.UnstackLastCommand();
	                                m_bCancelLastCommand = false;
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
            
            // Mettre à jour les menus
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
							IUndoableCommand crs2 = new CommandRemoveScreen(this, 1, true);
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
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    	cm.LaunchUndoableCommand(cacs);
	
							// todo : m_bCancelLastCommand ?
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
								// remove [0].
								IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
								cm.LaunchUndoableCommand(crs);
	
								// todo : m_bCancelLastCommand ?
							}
							else
							{
								// remove [1].
								IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
								cm.LaunchUndoableCommand(crs);
	
								// todo: doc.
	                            if(m_bCancelLastCommand)
	                            {
	                                cm.UnstackLastCommand();
	                                m_bCancelLastCommand = false;
	                            }	
							}
						}
						else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
						{
							// [capture][player] -> remove player.	
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove player.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
						}
						else
						{
							// remove both and add one capture.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand crs2 = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs2);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    	cm.LaunchUndoableCommand(cacs);
						}
	                    break;
           			}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
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
	                    	IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
	                    	IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs);
                        	IUndoableCommand cacs2 = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs2);
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
							IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs);
						}
						else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
						{
							// [player][capture] -> remove player and add capture.
							IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs);
						}
						else
						{
            				// [player][player] -> remove both and add 2 capture.
            				IUndoableCommand crs = new CommandRemoveScreen(this, 0, true);
							cm.LaunchUndoableCommand(crs);
            				IUndoableCommand crs2 = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs2);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs);
                        	IUndoableCommand cacs2 = new CommandAddCaptureScreen(this, true);
                        	cm.LaunchUndoableCommand(cacs2);
						}
						
                    	break;
            		}
                default:
                    break;
            }
            
            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
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
            				IUndoableCommand crs = new CommandRemoveScreen(this, 1, true);
							cm.LaunchUndoableCommand(crs);
							IUndoableCommand cacs = new CommandAddCaptureScreen(this, true);
	                    	cm.LaunchUndoableCommand(cacs);
						}
						
                    	break;
            		}
                default:
                    break;
            }

            // Display the new list.
            ICommand css = new CommandShowScreens(this);
            CommandManager.LaunchCommand(css);
            
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
            IUndoableCommand ctcc = new CommandToggleCommonControls(((ScreenManagerUserInterface)UI).splitScreensPanel);
            CommandManager cm = CommandManager.Instance();
            cm.LaunchUndoableCommand(ctcc);
            
            m_bCommonControlsVisible = !((ScreenManagerUserInterface)UI).splitScreensPanel.Panel2Collapsed;
        }
        #endregion

        #region Image
        private void mnuDeinterlaceOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(m_ActiveScreen != null)
        	{
        		m_mnuDeinterlace.Checked = !m_mnuDeinterlace.Checked;
        		player.Deinterlaced = m_mnuDeinterlace.Checked;	
        	}
        }
        private void mnuMirrorOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(m_ActiveScreen != null)
        	{
        		m_mnuMirror.Checked = !m_mnuMirror.Checked;
        		player.Mirrored = m_mnuMirror.Checked;
        	}
        }
        private void mnuGridOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(m_ActiveScreen != null)
        	{
        		m_mnuGrid.Checked = !m_mnuGrid.Checked;
        		player.ShowGrid = m_mnuGrid.Checked;
        	}
        }
        private void mnu3DPlaneOnClick(object sender, EventArgs e)
        {
        	PlayerScreen player = m_ActiveScreen as PlayerScreen;
        	if(m_ActiveScreen != null)
        	{
        		m_mnu3DPlane.Checked = !m_mnu3DPlane.Checked;
        		player.Show3DPlane = m_mnu3DPlane.Checked;
        	}
        }
        #endregion

        #endregion

        #region Déléguées appellées depuis l'UI
        public void DropLoadMovie(string _FilePath, int _iScreen)
        {
            //----------------------------------------------------------------
            // Fin du glisser-déposer entre le FileManager et le ScreenManager
            // Fonction appelée par déléguée depuis l'UI.
            //----------------------------------------------------------------
            if(File.Exists(_FilePath))
            {
            	CommandManager cm = CommandManager.Instance();
            	IUndoableCommand clmis = new CommandLoadMovieInScreen(this, _FilePath, _iScreen, true);
            	cm.LaunchUndoableCommand(clmis);
            }
        }
        public void CommonCtrlsGotoFirst(object sender, EventArgs e)
        {
            if (m_bSynching)
            {
                m_iCurrentFrame = 0;
                OnCommonPositionChanged(m_iCurrentFrame);
                ((ScreenManagerUserInterface)UI).UpdateTrkFrame(m_iCurrentFrame);
                DoStopPlaying();
            }
            else
            {
                // Demander un GotoFirst à tout le monde
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen is PlayerScreen)
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoFirst_Click(sender, e);
                    }
                }
            }
        }
        public void CommonCtrlsGotoLast(object sender, EventArgs e)
        {
            if (m_bSynching)
            {
                m_iCurrentFrame = m_iMaxFrame;
                OnCommonPositionChanged(m_iCurrentFrame);
                ((ScreenManagerUserInterface)UI).UpdateTrkFrame(m_iCurrentFrame);
                DoStopPlaying();
            }
            else
            {
                // Demander un GotoLast à tout le monde
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen is PlayerScreen)
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoLast_Click(sender, e);
                    }
                }
            }
        }
        public void CommonCtrlsGotoPrev(object sender, EventArgs e)
        {
            if (m_bSynching)
            {
                if (m_iCurrentFrame > 0)
                {
                    m_iCurrentFrame--;
                    OnCommonPositionChanged(m_iCurrentFrame);
                    ((ScreenManagerUserInterface)UI).UpdateTrkFrame(m_iCurrentFrame);
                }
            }
            else
            {
                // Demander un GotoPrev à tout le monde
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen.GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoPrevious_Click(sender, e);
                    }
                }
            }
        }
        public void CommonCtrlsGotoNext(object sender, EventArgs e)
        {
            if (m_bSynching)
            {
                if (m_iCurrentFrame < m_iMaxFrame)
                {
                    m_iCurrentFrame++;
                    OnCommonPositionChanged(-1);
                    ((ScreenManagerUserInterface)UI).UpdateTrkFrame(m_iCurrentFrame);
                }
            }
            else
            {
                // Demander un GotoNext à tout le monde
                foreach (AbstractScreen screen in screenList)
                {
                    if (screen.GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.buttonGotoNext_Click(sender, e);
                    }
                }
            }
        }
        public void CommonCtrlsPlay(object sender, EventArgs e)
        {
            if (m_bSynching)
            {
                if (((ScreenManagerUserInterface)UI).ComCtrls.Playing)
                {
                    //--------------------------------------------------------
                    // On lance le timer local avec un intervalle deux fois plus précis que le plus précis des deux vidéos.
                    // Ce timer ne sert pas à demander chaque frame mais juste à 
                    // contrôler l'état lancé/arrêté de chaque vidéo en fonction du décalage requis.
                    //--------------------------------------------------------
                
                    // Les vidéos ont pu être déplacées indépendamment pendant la pause...

                    
                    StartMultimediaTimer(Math.Min(((PlayerScreen)screenList[0]).FrameInterval/2, ((PlayerScreen)screenList[1]).FrameInterval/2));
                }
                else
                {
                    StopMultimediaTimer();
                }
            }

            //-------------------------------------------------------------
            // Propager la demande
            // Si un écran est déjà dans l'état demandé, ne pas le toucher.
            //-------------------------------------------------------------
            foreach (AbstractScreen screen in screenList)
            {
                if (screen is PlayerScreen)
                {
                    // lancer la vidéo si besoin.
                    if (((PlayerScreen)screen).IsPlaying != ((ScreenManagerUserInterface)this.UI).ComCtrls.Playing)
                    {
                        ((PlayerScreen)screen).m_PlayerScreenUI.OnButtonPlay();
                    }
                }
            }
        }
        public void CommonCtrlsSwap(object sender, EventArgs e)
        {
            mnuSwapScreensOnClick(sender, e);
        }
        public void CommonCtrlsSync(object sender, EventArgs e)
        {
            if (m_bSynching && screenList.Count == 2)
            {
                // Mise à jour : m_iLeftSyncFrame, m_iRightSyncFrame, m_iSyncLag, m_iCurrentFrame. m_iMaxFrame.
                SetSyncPoint(false);
                SetSyncLimits();

                // Mise à jour du trkFrame.
                ((ScreenManagerUserInterface)UI).SetupTrkFrame(0, m_iMaxFrame, m_iCurrentFrame);

                // Mise à jour des Players.
                OnCommonPositionChanged(m_iCurrentFrame);

                // debug
                ((ScreenManagerUserInterface)UI).DisplaySyncLag(m_iSyncLag);
            }
        }
        public void CommonCtrlsPositionChanged(object sender, long _iPosition)
        {
            if (m_bSynching)
            {
                StopMultimediaTimer();
                
                EnsurePause(0);
                EnsurePause(1);

                ((ScreenManagerUserInterface)UI).DisplayAsPaused();

                m_iCurrentFrame = (int)_iPosition;
                OnCommonPositionChanged(m_iCurrentFrame);
            }
        }
        #endregion

        #region Delguées appellées depuis les PlayerScreens
        public void Screen_SetActiveScreen(AbstractScreen _ActiveScreen)
        {
            //---------------------------------------------------------------------------------
            // Cette fonction doit pouvoir être accédée déclenchée depuis les Screens.
            // Les screens contiennent un delegate avec ce prototype, on injecte cette fonction 
            // dans le delegate.
            //---------------------------------------------------------------------------------
            
            // /!\ Eviter d'appeller SetAsActiveScreen à tout bout de champ
            // La fonction OrganizeMenu est assez lourde au niveau de l'UI et peut
            // monopoliser la pile de messages windows.

            if (m_ActiveScreen != _ActiveScreen )
            {
                m_ActiveScreen = _ActiveScreen;
                
                if (screenList.Count > 1)
                {
                    m_ActiveScreen.DisplayAsActiveScreen();

                    // Désactiver les autres
                    foreach (AbstractScreen screen in screenList)
                    {
                        if (screen != _ActiveScreen)
                        {
                            screen.DisplayAsInactiveScreen();
                        }
                    }
                }
            }

            OrganizeMenus();

        }
        public void Screen_CloseAsked(AbstractScreen _SenderScreen)
        {
            // Close the sender.
            // We leverage the fact that screens are always weel ordered relative to menus.
            if (_SenderScreen == screenList[0])
            {
                mnuCloseFileOnClick(null, EventArgs.Empty);
            }
            else
            {
                mnuCloseFile2OnClick(null, EventArgs.Empty);
            }
        }
        public void Player_IsReady(PlayerScreen _screen, bool _bInitialisation)
        {
            // Appelé lors de changement de framerate.
            if (m_bSynching)
            {
                SetSyncPoint(true);
            }
        }
        public void Player_SelectionChanged(PlayerScreen _screen, bool _bInitialization)
        {
            // We actually don't care which video was updated.
            // Set sync mode and reset sync.
			log.Debug("Player_SelectionChanged() called.");
            m_bSynching = false;

            if ( (screenList.Count == 2))
            {
                if ((screenList[0] is PlayerScreen) && (screenList[1] is PlayerScreen))
                {
                    if (((PlayerScreen)screenList[0]).HasMovie && ((PlayerScreen)screenList[1]).HasMovie)
                    {
                        m_bSynching = true;
                        ((PlayerScreen)screenList[0]).Synched = true;
                        ((PlayerScreen)screenList[1]).Synched = true;

                        if (_bInitialization)
                        {
                            // Static Sync
                            m_iRightSyncFrame = 0;
                            m_iLeftSyncFrame = 0;
                            m_iSyncLag = 0;
                            m_iCurrentFrame = 0;

                            // Dynamic Sync
                            ResetDynamicSyncFlags();
                        }

                        // Mise à jour trkFrame
                        SetSyncLimits();
                        ((ScreenManagerUserInterface)UI).SetupTrkFrame(0, m_iMaxFrame, m_iCurrentFrame);

                        // Mise à jour Players
                        OnCommonPositionChanged(m_iCurrentFrame);

                        // debug
                        ((ScreenManagerUserInterface)UI).DisplaySyncLag(m_iSyncLag);
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
                StopMultimediaTimer();
                ((ScreenManagerUserInterface)UI).DisplayAsPaused();
            }

        }
        #endregion

        #region Delegates called from anywhere, through Services
        public void DoLoadMovieInScreen(string _filePath, int _iForceScreen, bool _bStoreState)
        {
        	if(File.Exists(_filePath))
            {
            	IUndoableCommand clmis = new CommandLoadMovieInScreen(this, _filePath, _iForceScreen, _bStoreState);
            	CommandManager cm = CommandManager.Instance();
            	cm.LaunchUndoableCommand(clmis);
        	}
        }
        /*public void DoCompilePlayerScreen()
        {
            //----------------------------------------------------------------------------------------
            // This function is just to enhance user experience, 
            // It is called when the main window is idle just after first display
            // and should fasten the first screen showing, since the framework will have it in memory.
            //----------------------------------------------------------------------------------------
            //log.Debug("Creating a PlayerScreen just to get the class compiled beforehand.");
            //PlayerScreen ps = new PlayerScreen();
            //ps = null;
        }*/
        public void DoStopPlaying()
        {
            // Fonction appelée depuis le Supervisor, lorsque l'utilisateur lance la
            // boîte de dialogue Ouvrir.

            // Stopper chaque écran
            foreach (AbstractScreen screen in screenList)
            {
                if (screen.GetType().FullName.Equals("Videa.ScreenManager.PlayerScreen"))
                {
                    ((PlayerScreen)screen).StopPlaying();
                }
            }

            // stopper ici
            StopMultimediaTimer();
            ((ScreenManagerUserInterface)UI).DisplayAsPaused();
        }
        public void DoDeactivateKeyboardHandler()
        {
            m_bAllowKeyboardHandler = false;
        }
        public void DoActivateKeyboardHandler()
        {
            m_bAllowKeyboardHandler = true;
        }
        public void DoVideoProcessingDone(DrawtimeFilterOutput _dfo)
        {
        	// Todo, désactiver les filtres drawtime dans le player s'il y en a.
        	
        	if(_dfo != null)
        	{
    			m_VideoFilters[_dfo.VideoFilterType].Menu.Checked = _dfo.Active;
    			
        		PlayerScreen player = m_ActiveScreen as PlayerScreen;
	        	if(player != null)
	        	{
	        		player.SetDrawingtimeFilterOutput(_dfo);
	        	}
        	}
        	
        	m_ActiveScreen.RefreshImage();
        }
        #endregion

        #region Keyboard Handling
        public bool PreFilterMessage(ref Message m)
        {
            //----------------------------------------------------------------------------
            // Attention au niveau des performances avec cette fonction
            // car du coup tous les WM_XXX windows passent par là
            // WM_PAINT, WM_MOUSELEAVE de tous les contrôles, etc...
            // Plus on la place haut dans la hiérarchie, plus elle plombe les perfs.
            //
            // Les actions de ce KeyHandler n'affectent pour la plupart que l'ActiveScreen
            // (sauf en mode DualScreen)
            //
            // Si cette fonction interfère avec d'autres parties 
            // (car elle redéfinie return, space, etc.) utiliser le delegate pool avec 
            // DeactivateKeyboardHandler et ActivateKeyboardHandler
            //----------------------------------------------------------------------------

            bool bWasHandled = false;
			ScreenManagerUserInterface smui = UI as ScreenManagerUserInterface;
            	
			if (m_bAllowKeyboardHandler && smui != null)
            {
                m_bCommonControlsVisible = !smui.splitScreensPanel.Panel2Collapsed;
                bool bThumbnailsViewerVisible = smui.m_ThumbsViewer.Visible;

                if ( (m.Msg == WM_KEYDOWN)  && 
                     (!m_bAdjustingImage)   &&
                     ((screenList.Count > 0 && m_ActiveScreen != null) || (bThumbnailsViewerVisible)))
                {
                    Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;

                    switch (keyCode)
                    {
                    	case Keys.Delete:
                    	case Keys.Add:
                    	case Keys.Subtract:
                    	case Keys.F2:
                    	case Keys.F6:
                    	case Keys.F7:
                            {
                    			//------------------------------------------------
                    			// These keystrokes impact only the active screen.
                    			//------------------------------------------------
                    			if(!bThumbnailsViewerVisible)
                    			{       
									bWasHandled = m_ActiveScreen.OnKeyPress(keyCode);
                    			}
								else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);
                    			}
                    			break;
                            }
                    	case Keys.Escape:
                    	case Keys.F11:
                    	case Keys.Down:
                    	case Keys.Up:
                            {
                    			//---------------------------------------------------
                    			// These keystrokes impact each screen independently.
                    			//---------------------------------------------------
                    			if(!bThumbnailsViewerVisible)
                    			{
	                                foreach (AbstractScreen abScreen in screenList)
	                                {
	                                    bWasHandled = abScreen.OnKeyPress(keyCode);
	                                }
                    			}
                    			else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);
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
                               	if(!bThumbnailsViewerVisible)
                    			{
                               		if (screenList.Count == 2)
	                                {
                               			if(m_bCommonControlsVisible)
                               			{
                               				bWasHandled = OnKeyPress(keyCode);
                               			}
                               			else
                               			{
                               				bWasHandled = m_ActiveScreen.OnKeyPress(keyCode);	
                               			}
	                                }
	                                else if(screenList.Count == 1)
	                                {
	                                	bWasHandled = screenList[0].OnKeyPress(keyCode);
	                                }	
                               	}
                    			else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);	
                    			}
                                break;
                            }
                    	//-------------------------------------------------
                    	// All the remaining keystrokes impact both screen, 
                    	// even if the common controls aren't visible.
                    	//-------------------------------------------------
                        case Keys.PageUp:
                    	case Keys.PageDown:
                            {
                    			// Change active screen.
                    			if(!bThumbnailsViewerVisible)
                    			{
                    				if(m_bSynching)
                               		{
                    					ActivateOtherScreen();
                    					bWasHandled = true;
                    				}
                    			}
                    			else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);	
                    			}
                    			break;
                    		}
                        case Keys.F8:
                        	{
	                            // Go to sync frame. 
	                            if(!bThumbnailsViewerVisible)
	                    		{
	                            	if(m_bSynching)
                               		{
		                                if (m_iSyncLag > 0)
		                                {
		                                    m_iCurrentFrame = m_iRightSyncFrame;
		                                }
		                                else
		                                {
		                                    m_iCurrentFrame = m_iLeftSyncFrame;
		                                }
		
		                                // Update
		                                OnCommonPositionChanged(m_iCurrentFrame);
		                                smui.UpdateTrkFrame(m_iCurrentFrame);
		                                bWasHandled = true;
	                            	}
	                            }
	                            else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);	
                    			}
	                            break;
                        	}
                        case Keys.F9:
                            {
                                //---------------------------------------
                                // Fonctions associées : 
                                // Resynchroniser après déplacement individuel
                                //---------------------------------------
                               	if(!bThumbnailsViewerVisible)
                                {
                               		if(m_bSynching)
                               		{
                               			SyncCatch();
                               			bWasHandled = true;
                               		}
                                }
                               	else
                    			{
                    				bWasHandled = smui.m_ThumbsViewer.OnKeyPress(keyCode);	
                    			}
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            return bWasHandled;
        }
        private bool OnKeyPress(Keys _keycode)
        {
        	//---------------------------------------------------------
        	// Here are grouped the handling of the keystrokes that are 
        	// for of screen manager's responsibility.
        	// And only when the common controls are actually visible.
        	//---------------------------------------------------------
        	bool bWasHandled = false;
        	ScreenManagerUserInterface smui = UI as ScreenManagerUserInterface;
            	
			if (smui != null)
            {
	        	switch (_keycode)
				{
	        		case Keys.Space:
	        		case Keys.Return:
	        			{
	                       	smui.ComCtrls.buttonPlay_Click(null, EventArgs.Empty);
	                        bWasHandled = true;
	                    	break;
	        			}
	        		case Keys.Left:
	        			{
							smui.ComCtrls.buttonGotoPrevious_Click(null, EventArgs.Empty);
                        	bWasHandled = true;
	        				break;
	        			}
	        		case Keys.Right:
	        			{
                           	smui.ComCtrls.buttonGotoNext_Click(null, EventArgs.Empty);
                       		bWasHandled = true;
							break;
	        			}
	        		case Keys.End:
                        {
	        				smui.ComCtrls.buttonGotoLast_Click(null, EventArgs.Empty);
	        				bWasHandled = true;
							break;
	        			}
	        		case Keys.Home:
	        			{
	        				smui.ComCtrls.buttonGotoFirst_Click(null, EventArgs.Empty);
                            bWasHandled = true;
                            break;
	        			}
	        		default:
	        			break;
	        	}
			}
        	return bWasHandled;
        }
        private void ActivateOtherScreen()
        {
        	if (screenList.Count == 2)
            {
                if (m_ActiveScreen == screenList[0])
                {
                    Screen_SetActiveScreen(screenList[1]);
                }
                else
                {
                    Screen_SetActiveScreen(screenList[0]);
                }
            }	
        }
        #endregion

        #region Synchronisation

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

            log.Debug("SetSyncPoint() called.");

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
	            }
	
	
	            // Sync Lag is expressed in frames.
	            m_iSyncLag = m_iRightSyncFrame - m_iLeftSyncFrame;
	
	            // We need to recompute the lag in milliseconds because it can change even when 
	            // the references positions don't change. For exemple when varying framerate (speed).
	            int iLeftSyncMilliseconds = (int)(((PlayerScreen)screenList[0]).FrameInterval * m_iLeftSyncFrame);
	            int iRightSyncMilliseconds = (int)(((PlayerScreen)screenList[1]).FrameInterval * m_iRightSyncFrame);
	            m_iSyncLagMilliseconds = iRightSyncMilliseconds - iLeftSyncMilliseconds;
	
	            // Update common position (sign of m_iSyncLag might have changed.)
	            if (m_iSyncLag > 0)
	            {
	                m_iCurrentFrame = m_iRightSyncFrame;
	            }
	            else
	            {
	                m_iCurrentFrame = m_iLeftSyncFrame;
	            }
	
	            // debug
	            ((ScreenManagerUserInterface)UI).DisplaySyncLag(m_iSyncLag);
            }
        }
        private void SetSyncLimits()
        {
            //--------------------------------------------------------------------------------
            // Computes the real max of the trkFrame, considering the lag and original sizes.
            // Updates trkFrame bounds, expressed in *Frames*.
            // impact : m_iMaxFrame.
            //---------------------------------------------------------------------------------
			log.Debug("SetSyncLimits() called.");
            int iLeftMaxFrame = ((PlayerScreen)screenList[0]).LastFrame;
            int iRightMaxFrame = ((PlayerScreen)screenList[1]).LastFrame;

            if (m_iSyncLag > 0)
            {
                // Lag is positive. Right video starts first and its duration stay the same as original.
                // Left video has to wait for an ammount of time.

                // Get Lag in number of frames of left video.
                //int iSyncLagFrames = ((PlayerScreen)screenList[0]).NormalizedToFrame(m_iSyncLag);

                // Check if lag is still valid. (?)
                if (m_iSyncLag > iRightMaxFrame) 
                {
                    m_iSyncLag = 0; 
                }

                iLeftMaxFrame += m_iSyncLag;
            }
            else
            {
                // Lag is negative. Left video starts first and its duration stay the same as original.
                // Right video has to wait for an ammount of time.
                
                // Get Lag in frames of right video
                //int iSyncLagFrames = ((PlayerScreen)screenList[1]).NormalizedToFrame(m_iSyncLag);

                // Check if lag is still valid.(?)
                if (-m_iSyncLag > iLeftMaxFrame) { m_iSyncLag = 0; }
                iRightMaxFrame += (-m_iSyncLag);
            }

            m_iMaxFrame = Math.Max(iLeftMaxFrame, iRightMaxFrame);

            //Console.WriteLine("m_iSyncLag:{0}, m_iSyncLagMilliseconds:{1}, MaxFrames:{2}", m_iSyncLag, m_iSyncLagMilliseconds, m_iMaxFrame);
        }
        private void OnCommonPositionChanged(int _iFrame)
        {
            //------------------------------------------------------------------------------
            // Updates each video to reflect current common position.
            // Used to handle GotoNext, GotoPrev, trkFrame, etc.
            // this is where the "static sync" is done.
            // 
            // note: m_iSyncLag and _iFrame are expressed in frames.
            //------------------------------------------------------------------------------

            log.Debug("OnCommonPositionChanged() called. Static Sync.");
            
            // Get corresponding position in each video, in frames
            int iLeftFrame = 0;
            int iRightFrame = 0;

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
                ((PlayerScreen)screenList[0]).CurrentFrame = iLeftFrame;
                ((PlayerScreen)screenList[1]).CurrentFrame = iRightFrame;
            }
            else
            {
                // Special case for ++.
                if (m_iSyncLag > 0)
                {
                    // Right video must go ahead.
                    ((PlayerScreen)screenList[1]).GotoNextFrame();

                    if (m_iCurrentFrame > m_iSyncLag)
                    {
                        ((PlayerScreen)screenList[0]).GotoNextFrame();
                    }
                }
                else
                {
                    // Left video must go ahead.
                    ((PlayerScreen)screenList[0]).GotoNextFrame();

                    if (m_iCurrentFrame > -m_iSyncLag)
                    {
                        ((PlayerScreen)screenList[1]).GotoNextFrame();
                    }
                

                }
            }
        }
        public void SwapSync()
        {
        	if (m_bSynching && screenList.Count == 2)
        	{
	        	int iTemp = m_iLeftSyncFrame;
	            m_iLeftSyncFrame = m_iRightSyncFrame;
	            m_iRightSyncFrame = iTemp;
	
	            // Reset dynamic sync flags
	            ResetDynamicSyncFlags();
        	}
        }
        private void StartMultimediaTimer(int _interval)
        {
            
            if (m_DelegateMMTimerEventHandler != null)
            {
                int myData = 0;	// dummy data
                m_IdMultimediaTimer = timeSetEvent( _interval,                              // Délai en ms.
                                                    _interval,                              // Resolution en ms.
                                                    m_DelegateMMTimerEventHandler,          // event handler du tick.
                                                    ref myData,                             // ?
                                                    TIME_PERIODIC | TIME_KILL_SYNCHRONOUS); // Type d'event.
                log.Debug("Common multimedia timer started");
            }
        }
        private void StopMultimediaTimer()
        {
            if (m_IdMultimediaTimer != 0)
            {
                timeKillEvent(m_IdMultimediaTimer);
                log.Debug("Common multimedia timer stopped");
            }
        }
        private void MultimediaTimer_Tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            // Get each video positions in common timebase and milliseconds.
            // Figure if a restart or pause is needed, considering current positions.
            // This is where the dynamic sync is done.
            // 


            //-----------------------------------------------------------------------------
            // This function is executed in the WORKER THREAD.
            // nothing called from here should ultimately call in the UI thread.
            //
            // Except when using BeginInvoke.
            // But we can't use BeginInvoke here, because it's only available for Controls.
            // Calling the BeginInvoke of the PlayerScreenUI is useless because it's not the same 
            // UI thread as the one used to create the menus that we will update upon SetAsActiveScreen
            // 
            //-----------------------------------------------------------------------------

            if (m_bSynching && screenList.Count == 2)
            {
                //--------------------------------------------------------------
                // Fonction appellée par l'eventhandler du timer, 
                // à chaque tick, et de façon asynchrone.
                // TODO : si synching, mettre à jour la position.
                // de façon à ce que le prochain GotoNext soit à peu près bien géré.
                //--------------------------------------------------------------

                // L'ensemble de la supervision est réalisée en TimeStamps.
                // Seul les décision de lancer / arrêter sont établies par rapport
                // au temps auquel on est.

                int iLeftPosition = ((PlayerScreen)screenList[0]).CurrentFrame;
                int iRightPosition = ((PlayerScreen)screenList[1]).CurrentFrame;
                int iLeftMilliseconds = (int)(iLeftPosition * ((PlayerScreen)screenList[0]).FrameInterval);
                int iRightMilliseconds = (int)(iRightPosition * ((PlayerScreen)screenList[1]).FrameInterval);

                //-----------------------------------------------------------------------
                // Dans cette fonction, on part du principe que les deux vidéos tournent.
                // Et on fait des 'Ensure Pause' quand nécessaire.
                // On évite les Ensure Play' car l'utilisateur a pu 
                // manuellement pauser une vidéo.
                //-----------------------------------------------------------------------
                #region [i][0]
                if (iLeftPosition > 0 && iRightPosition == 0)
                {
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
                            // les 10 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
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
                    m_bLeftIsStarting = false;
                    m_bRightIsStarting = false;

                    m_iCurrentFrame = Math.Max(iLeftPosition, iRightPosition);
                }
                #endregion

                // Update position for trkFrame.
                object[] parameters = new object[] { m_iCurrentFrame };
                ((ScreenManagerUserInterface)UI).BeginInvoke(((ScreenManagerUserInterface)UI).m_DelegateUpdateTrkFrame, parameters);

                //Console.WriteLine("Tick:[{0}][{1}], Starting:[{2}][{3}]", iLeftPosition, iRightPosition, m_bLeftIsStarting, m_bRightIsStarting);
            }
            else
            {
                // This can happen when a screen is closed on the fly while synching.
                StopMultimediaTimer();
                m_bSynching = false;
                ((ScreenManagerUserInterface)UI).DisplayAsPaused();
            }
        }
        private void EnsurePause(int _iScreen)
        {
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
                ((ScreenManagerUserInterface)UI).DisplayAsPaused();
            }
        }
        private void EnsurePlay(int _iScreen)
        {
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
                ((ScreenManagerUserInterface)UI).DisplayAsPaused();
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
            int iLeftFrame = ((PlayerScreen)screenList[0]).CurrentFrame;
            int iRightFrame = ((PlayerScreen)screenList[1]).CurrentFrame;

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

            OnCommonPositionChanged(m_iCurrentFrame);
            ((ScreenManagerUserInterface)UI).UpdateTrkFrame(m_iCurrentFrame);

        }
        #endregion

        #region Screens State Recalling
        public void StoreCurrentState()
        {
            //------------------------------------------------------------------------------
            // Before we start anything messy, let's store the current state of the ViewPort
            // So we can reinstate it later in case the user change his mind.
            //-------------------------------------------------------------------------------
            mStoredStates.Add(GetCurrentState());
        }
        public ScreenManagerState GetCurrentState()
        {
            ScreenManagerState mState = new ScreenManagerState();

            foreach (AbstractScreen bs in screenList)
            {
                ScreenState state = new ScreenState();

                state.UniqueId = bs.UniqueId;

                if (bs is PlayerScreen)
                {
                    state.Loaded = ((PlayerScreen)bs).m_bIsMovieLoaded;
                    state.FilePath = ((PlayerScreen)bs).FilePath;
                    if (((PlayerScreen)bs).m_bIsMovieLoaded)
                    {
                        state.MetadataString = ((PlayerScreen)bs).m_PlayerScreenUI.Metadata.ToXmlString();
                    }
                    else
                    {
                        state.MetadataString = "";
                    }
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
         
            if (mStoredStates.Count > 0)
            {
                int iLastState = mStoredStates.Count -1;
                CommandManager cm = CommandManager.Instance();
                ICommand css = new CommandShowScreens(this);

                ScreenManagerState CurrentState = GetCurrentState();

                switch (CurrentState.ScreenList.Count)
                {
                    case 0:
                        //-----------------------------
                        // Il y a actuellement 0 écran.
                        //-----------------------------
                        switch (mStoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                // Il n'y en avait aucun : Ne rien faire.
                                break;
                            case 1:
                                {
                                    // Il y en avait un : Ajouter l'écran.
                                    ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState); 
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 2:
                                {
                                    // Ajouter les deux écrans, on ne se préoccupe pas trop de l'ordre
                                    ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                    ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 1, CurrentState);
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
                        switch (mStoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                {
                                    // Il n'y en avait aucun : Supprimer l'écran.
                                    RemoveScreen(0);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 1:
                                {
                                    // Il y en avait un : Remplacer si besoin.
                                    ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                    CommandManager.LaunchCommand(css);
                                    break;
                                }
                            case 2:
                                {
                                    // Il y avait deux écran : Comparer chaque ancien écran avec le restant.
                                    int iMatchingScreen = -1;
                                    int i=0;
                                    while ((iMatchingScreen == -1) && (i < mStoredStates[iLastState].ScreenList.Count))
                                    {
                                        if (mStoredStates[iLastState].ScreenList[i].UniqueId == CurrentState.ScreenList[0].UniqueId)
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
                                                ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                                ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                break;
                                            }
                                        case 0:
                                            {
                                                // the old 0 is the new 0, the old 1 doesn't exist yet.
                                                ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                break;
                                            }
                                        case 1:
                                            {
                                                // the old 1 is the new 0, the old 0 doesn't exist yet.
                                                ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 1, CurrentState);
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
                        switch (mStoredStates[iLastState].ScreenList.Count)
                        {
                            case 0:
                                {
                                    // Il n'yen avait aucun : supprimer les deux.
                                    RemoveScreen(1);
                                    RemoveScreen(0);
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
                                        if (mStoredStates[iLastState].ScreenList[0].UniqueId == CurrentState.ScreenList[i].UniqueId)
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
                                            RemoveScreen(1);
                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                            break;
                                        case 0:
                                            // L'ancien écran a été retrouvé dans l'écran [0]
                                            // On supprime le second.
                                            RemoveScreen(1);
                                            break;
                                        case 1:
                                            // L'ancien écran a été retrouvé dans l'écran [1]
                                            // On supprime le premier.
                                            RemoveScreen(0);
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
                                        if (mStoredStates[iLastState].ScreenList[0].UniqueId == CurrentState.ScreenList[i].UniqueId)
                                        {
                                            iMatchingScreen[0] = i;
                                        }
                                        else if (mStoredStates[iLastState].ScreenList[1].UniqueId == CurrentState.ScreenList[i].UniqueId)
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
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 1, CurrentState);
                                                            break;
                                                        }
                                                    case 0:
                                                        {
                                                            // Ecran 0 non retrouvé, écran 1 retrouvé dans le 0.
                                                            // Remplacer l'écran 1 par l'ancien 0.
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 1, CurrentState);
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            // Ecran 0 non retrouvé, écran 1 retrouvé dans le 1.
                                                            // Remplacer l'écran 0.
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[0], 0, CurrentState);
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
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 1, CurrentState);
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
                                                            ReinstateScreen(mStoredStates[iLastState].ScreenList[1], 0, CurrentState);
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

                // Mettre à jour menus et Status bar
                UpdateStatusBar();
                OrganizeMenus();

                mStoredStates.RemoveAt(iLastState);

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
                    RemoveScreen(_iNewPosition);

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
        private void RemoveScreen(int _iPosition)
        {
            CommandManager cm = CommandManager.Instance();

            ICommand crs = new CommandRemoveScreen(this, _iPosition, false);
            CommandManager.LaunchCommand(crs);

            if (m_bCancelLastCommand)
            {
                cm.UnstackLastCommand();
                m_bCancelLastCommand = false;
            }
        }
        private void ReloadScreen(ScreenState _OldScreen, int _iNewPosition)
        {
        	if(File.Exists(_OldScreen.FilePath))
            {
            
        		// We instantiate and launch it like a simple command (not undoable).
	            ICommand clmis = new CommandLoadMovieInScreen(this, _OldScreen.FilePath, _iNewPosition, false);
	            CommandManager.LaunchCommand(clmis);
	
	            // Check that everything went well 
	            // Potential problem : the video was deleted between do and undo.
	            // _iNewPosition should always point to a valid position here.
	            if (((PlayerScreen)screenList[_iNewPosition-1]).m_bIsMovieLoaded)
	            {
	                ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.Metadata.LoadFromString(_OldScreen.MetadataString);
	                ((PlayerScreen)m_ActiveScreen).m_PlayerScreenUI.PostImportAnalysis();
	            }
        	}
        }
        #endregion
    }

	#region Global enums
	public enum VideoFilterType
	{
		AutoLevels,
		AutoContrast,
		Sharpen,
		EdgesOnly,
		Mosaic,
		Reverse,
		NumberOfVideoFilters
	};
    #endregion

}
