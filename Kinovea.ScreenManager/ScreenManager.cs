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
using System.Linq;

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
    public class ScreenManagerKernel : IKernel
    {
        #region Properties
        public UserControl UI
        {
            get { return view; }
        }
        public ScreenManagerUserInterface View
        {
            get { return view;}
        }
        public ResourceManager resManager
        {
            get { return new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly()); }
        }
        public int ScreenCount
        {
            get { return screenList.Count;}
        }
        #endregion

        #region Members
        private ScreenManagerUserInterface view;
        private DualPlayerController dualPlayer = new DualPlayerController();
        private DualCaptureController dualCapture = new DualCaptureController();
        private List<AbstractScreen> screenList = new List<AbstractScreen>();
        private IEnumerable<PlayerScreen> playerScreens;
        private IEnumerable<CaptureScreen> captureScreens;
        private AbstractScreen activeScreen = null;
        private bool canShowCommonControls;
        
        // Dual saving
        private string dualSaveFileName;
        private bool dualSaveCancelled;
        private bool dualSaveInProgress;
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private BackgroundWorker bgWorkerDualSave;
        private formProgressBar dualSaveProgressBar;

        // Video Filters
        private bool hasSvgFiles;
        private string svgPath;
        private FileSystemWatcher svgFilesWatcher = new FileSystemWatcher();
        private bool buildingSVGMenu;
        private List<ToolStripMenuItem> filterMenus = new List<ToolStripMenuItem>();
        
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
        private long syncLag; 	            // Sync Lag in Frames, for static sync.
        private long syncLagMilliseconds;		// Sync lag in Milliseconds, for dynamic sync.
        private bool dynamicSynching;			// replace the common timer.
        
        // Static Sync Positions
        private long currentFrame = 0;            // Current frame in trkFrame...
        private long leftSyncFrame = 0;           // Sync reference in the left video
        private long rightSyncFrame = 0;          // Sync reference in the right video
        private long maxFrame = 0;                // Max du trkFrame

        // Dynamic Sync Flags.
        private bool rightIsStarting = false;    // true when the video is between [0] and [1] frames.
        private bool leftIsStarting = false;
        private bool leftIsCatchingUp = false;   // CatchingUp is when the video is the only one left running,
        private bool rightIsCatchingUp = false;  // heading towards end, the other video is waiting the lag.

        #endregion

        private const int WM_KEYDOWN = 0x0100;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & initialization
        public ScreenManagerKernel()
        {
            log.Debug("Module Construction : ScreenManager.");

            view = new ScreenManagerUserInterface();
            view.FileLoadAsked += View_FileLoadAsked;
            view.AutoLaunchAsked += View_AutoLaunchAsked;
            AddCommonControlsPlayersEventHandlers();
            AddCommonControlsCaptureEventHandlers();

            CameraTypeManager.CameraLoadAsked += CameraTypeManager_CameraLoadAsked;
            VideoTypeManager.VideoLoadAsked += VideoTypeManager_VideoLoadAsked;
            
            InitializeVideoFilters();

            NotificationCenter.StopPlayback += (s, e) => DoStopPlaying();
            
            // Watch for changes in the guides directory.
            svgPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\guides\\";
            svgFilesWatcher.Path = svgPath;
            svgFilesWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            svgFilesWatcher.Filter = "*.svg";
            svgFilesWatcher.IncludeSubdirectories = true;
            svgFilesWatcher.EnableRaisingEvents = true;
            
            svgFilesWatcher.Changed += OnSVGFilesChanged;
            svgFilesWatcher.Created += OnSVGFilesChanged;
            svgFilesWatcher.Deleted += OnSVGFilesChanged;
            svgFilesWatcher.Renamed += OnSVGFilesChanged;

            playerScreens = screenList.Where(s => s is PlayerScreen).Select(s => s as PlayerScreen);
            captureScreens = screenList.Where(s => s is CaptureScreen).Select(s => s as CaptureScreen);
        }

        private void InitializeVideoFilters()
        {
            filterMenus.Add(CreateFilterMenu(new VideoFilterAutoLevels()));
            filterMenus.Add(CreateFilterMenu(new VideoFilterContrast()));
            filterMenus.Add(CreateFilterMenu(new VideoFilterSharpen()));
            filterMenus.Add(CreateFilterMenu(new VideoFilterEdgesOnly()));
            filterMenus.Add(CreateFilterMenu(new VideoFilterMosaic()));
            filterMenus.Add(CreateFilterMenu(new VideoFilterReverse()));
            //filterMenus.Add(CreateFilterMenu(new VideoFilterSandbox()));
        }

        private ToolStripMenuItem CreateFilterMenu(AbstractVideoFilter _filter)
        {
            // TODO: test if we can directly use a copy of the argument in the closure.
            // would avoid passing through .Tag and multiple casts.
            ToolStripMenuItem menu = new ToolStripMenuItem(_filter.Name, _filter.Icon);
            menu.MergeAction = MergeAction.Append;
            menu.Tag = _filter;
            menu.Click += (s,e) => {
                PlayerScreen screen = activeScreen as PlayerScreen;
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
            PlayerScreen player = activeScreen as PlayerScreen;
            if(player != null)
                player.SetInteractiveEffect(_effect);
        }
        
        public void RecoverCrash()
        {
            try
            {
                List<ScreenDescriptionPlayback> recoverables = RecoveryManager.GetRecoverables();
                if (recoverables != null && recoverables.Count > 0)
                {
                    FormCrashRecovery fcr = new FormCrashRecovery(recoverables);
                    FormsHelper.Locate(fcr);
                    if (fcr.ShowDialog() != DialogResult.OK)
                    {
                        log.DebugFormat("Recovery procedure cancelled. Deleting files.");
                        FilesystemHelper.DeleteOrphanFiles();
                    }
                }
            }
            catch (Exception)
            {
                log.Error("An error happened while running crash detection and recovery routine.");
                FilesystemHelper.DeleteOrphanFiles();
            }
        }
        #endregion

        #region IKernel Implementation
        public void BuildSubTree()
        {
            // No sub modules.
        }
        public void ExtendMenu(ToolStrip menu)
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
                        
            mnuSwapScreens.Image = Properties.Resources.flatswap3d;
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
            foreach(ToolStripMenuItem m in filterMenus)
            {
                if(m.Tag is AdjustmentFilter)
                    mnuCatchImage.DropDownItems.Add(m);
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
            foreach(ToolStripMenuItem m in filterMenus)
            {
                if(!(m.Tag is AdjustmentFilter))
                    mnuCatchMotion.DropDownItems.Add(m);
            }
            #endregion
            
            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchFile, mnuCatchScreens, mnuCatchImage, mnuCatchMotion });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, menu);

            RefreshCultureMenu();
        }
        
        public void ExtendToolBar(ToolStrip toolbar)
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
            
            ToolStripManager.Merge(ts, toolbar);
            
        }
        public void ExtendStatusBar(ToolStrip statusbar)
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
            dualPlayer.RefreshUICulture();
            dualCapture.RefreshUICulture();
            view.RefreshUICulture();

            foreach (AbstractScreen screen in screenList)
                screen.RefreshUICulture();
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
        public void PreferencesUpdated()
        {
            foreach (AbstractScreen screen in screenList)
                screen.PreferencesUpdated();
                
            RefreshUICulture();
        }
        #endregion
        
        #region Event handlers for screens
        private void Screen_CloseAsked(object sender, EventArgs e)
        {
            AbstractScreen screen = sender as AbstractScreen;
            if (screen == null)
                return;

            // If the screen is in Drawtime filter (e.g: Mosaic), we just go back to normal play.
            if (screen is PlayerScreen && ((PlayerScreen)screen).InteractiveFiltering)
            {
                SetActiveScreen(screen);
                ((PlayerScreen)screen).DeactivateInteractiveEffect();
                return;
            }

            screen.BeforeClose();

            // Reorganise screens.
            // We leverage the fact that screens are always well ordered relative to menus.
            if (screenList.Count > 0 && screen == screenList[0])
                CloseFile(0);
            else
                CloseFile(1);

            UpdateCaptureBuffers();
            PrepareSync(false);
        }
        private void Screen_Activated(object sender, EventArgs e)
        {
            AbstractScreen screen = sender as AbstractScreen;
            SetActiveScreen(screen);
        }
        private void Screen_CommandProcessed(object sender, CommandProcessedEventArgs e)
        {
            // Propagate the command to the other screen.
            AbstractScreen screen = sender as AbstractScreen;

            if (screenList.Count != 2 || screen == null)
                return;

            int otherScreen = sender == screenList[0] ? 1 : 0;
            
            if (screenList[0].GetType() == screenList[1].GetType())
                screenList[otherScreen].ExecuteCommand(e.Command);
        }

        private void Player_SpeedChanged(object sender, EventArgs e)
        {
            PlayerScreen screen = sender as PlayerScreen;
            if (screen == null)
                return;

            if (!dualPlayer.Synching || screenList.Count != 2)
                return;
            
            if(PreferencesManager.PlayerPreferences.SyncLockSpeed)
            {
                int otherScreen = screen == screenList[0] ? 1 : 0;
                ((PlayerScreen)screenList[otherScreen]).RealtimePercentage = screen.RealtimePercentage;
            }
            
            SetSyncPoint(true);
        }
        private void Player_PauseAsked(object sender, EventArgs e)
        {
            // An individual player asks for a global pause.
            if (dualPlayer.Synching && dualPlayer.Playing)
            {
                dualPlayer.DisplayAsPaused();
                CCtrl_PlayToggled(this, EventArgs.Empty);
            }
        }
        private void Player_SelectionChanged(object sender, EventArgs<bool> e)
        {
            PrepareSync(e.Value);
        }
        private void Player_ImageChanged(object sender, EventArgs<Bitmap> e)
        {
            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            if (!dualPlayer.Synching)
                return;

            if(dynamicSynching)
                DynamicSync();
            
            // Transfer the caller's image to the other screen.
            // The image has been cloned and transformed in the caller screen.
            if (!dualPlayer.Merging || e.Value == null)
                return;

            foreach (PlayerScreen p in playerScreens)
            {
                if (p != player)
                    p.SetSyncMergeImage(e.Value, !dualSaveInProgress);
            }
        }
        private void Player_SendImage(object sender, EventArgs<Bitmap> e)
        {
            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            // An image was sent from a screen to be added as an observational reference in the other screen.
            // The image has been cloned and transformed in the caller screen.
            foreach (PlayerScreen p in playerScreens)
            {
                if (p != player)
                    p.AddImageDrawing(e.Value);
            }
        }
        private void Player_ResetAsked(object sender, EventArgs e)
        {
            // A screen was reset. (ex: a video was reloded in place).
            // We need to also reset all the sync states.
            PrepareSync(true);
        }
        #endregion

        #region Common controls event handlers
        private void AddCommonControlsPlayersEventHandlers()
        {
            dualPlayer.View.GotoFirst += CCtrl_GotoFirst;
            dualPlayer.View.PlayToggled += CCtrl_PlayToggled;
            dualPlayer.View.GotoFirst += CCtrl_GotoFirst;
            dualPlayer.View.GotoPrev += CCtrl_GotoPrev;
            dualPlayer.View.GotoNext += CCtrl_GotoNext;
            dualPlayer.View.GotoLast += CCtrl_GotoLast;
            dualPlayer.View.SwapAsked += CCtrl_SwapAsked;
            dualPlayer.View.SyncAsked += CCtrl_SyncAsked;
            dualPlayer.View.MergeAsked += CCtrl_MergeAsked;
            dualPlayer.View.PositionChanged += CCtrl_PositionChanged;
            dualPlayer.View.DualSaveAsked += CCtrl_DualSaveAsked;
            dualPlayer.View.DualSnapshotAsked += CCtrl_DualSnapshotAsked;
        }
        private void AddCommonControlsCaptureEventHandlers()
        {
            dualCapture.View.SwapAsked += CCtrl_SwapAsked;
            dualCapture.View.GrabbingChanged += CCtrl_GrabbingChanged;
            dualCapture.View.SnapshotAsked += CCtrl_SnapshotAsked;
            dualCapture.View.RecordingChanged += CCtrl_RecordingChanged;
        }
        
        private void CCtrl_SwapAsked(object sender, EventArgs e)
        {
            mnuSwapScreensOnClick(null, EventArgs.Empty);	
        }

        private void CCtrl_PlayToggled(object sender, EventArgs e)
        {
            if (dualPlayer.Synching)
            {
                if (dualPlayer.Playing)
                {
                    // On play, simply launch the dynamic sync.
                    // It will handle which video can start right away.
                    StartDynamicSync();
                }
                else
                {
                    StopDynamicSync();
                    leftIsStarting = false;
                    rightIsStarting = false;
                }
            }

            // On stop, propagate the call to screens.
            if(!dualPlayer.Playing)
            {	
                if(screenList[0] is PlayerScreen)
                    EnsurePause(0);
                
                if(screenList[1] is PlayerScreen)
                    EnsurePause(1);
            }
        }
        private void CCtrl_GotoFirst(object sender, EventArgs e)
        {
            DoStopPlaying();
            
            if (dualPlayer.Synching)
            {
                currentFrame = 0;
                OnCommonPositionChanged(currentFrame, true);
                dualPlayer.UpdateTrkFrame(currentFrame);
            }
            else
            {
                foreach (PlayerScreen screen in playerScreens)
                    screen.view.buttonGotoFirst_Click(this, EventArgs.Empty);
            }	
        }
        private void CCtrl_GotoPrev(object sender, EventArgs e)
        {
            DoStopPlaying();
            
            if (dualPlayer.Synching)
            {
                if (currentFrame > 0)
                {
                    currentFrame--;
                    OnCommonPositionChanged(currentFrame, true);
                    dualPlayer.UpdateTrkFrame(currentFrame);
                }
            }
            else
            {
                foreach (PlayerScreen screen in playerScreens)
                    screen.view.buttonGotoPrevious_Click(this, EventArgs.Empty);
            }	
        }
        private void CCtrl_GotoNext(object sender, EventArgs e)
        {
            DoStopPlaying();
            
            if (dualPlayer.Synching)
            {
                if (currentFrame < maxFrame)
                {
                    currentFrame++;
                    OnCommonPositionChanged(-1, true);
                    dualPlayer.UpdateTrkFrame(currentFrame);
                }
            }
            else
            {
                foreach (PlayerScreen player in playerScreens)
                    player.view.buttonGotoNext_Click(this, EventArgs.Empty);
            }	
        }
        private void CCtrl_GotoLast(object sender, EventArgs e)
        {
            DoStopPlaying();
            
            if (dualPlayer.Synching)
            {
                currentFrame = maxFrame;
                OnCommonPositionChanged(currentFrame, true);
                dualPlayer.UpdateTrkFrame(currentFrame);
                
            }
            else
            {
                foreach (PlayerScreen player in playerScreens)
                    player.view.buttonGotoLast_Click(this, EventArgs.Empty);
            }	
        }
        private void CCtrl_SyncAsked(object sender, EventArgs e)
        {
            if (!dualPlayer.Synching || screenList.Count != 2)
                return;
            
            log.Debug("Sync point change.");
            SetSyncPoint(false);
            SetSyncLimits();
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_MergeAsked(object sender, EventArgs e)
        {
            if (!dualPlayer.Synching || screenList.Count != 2)
                return;

            log.Debug(String.Format("SyncMerge videos is now {0}", dualPlayer.Merging.ToString()));
                
            // This will also do a full refresh, and triggers Player_ImageChanged().
            ((PlayerScreen)screenList[0]).SyncMerge = dualPlayer.Merging;
            ((PlayerScreen)screenList[1]).SyncMerge = dualPlayer.Merging;
        }
        private void CCtrl_PositionChanged(object sender, TimeEventArgs e)
        {
            // Manual static sync.
            if (!dualPlayer.Synching)
                return;
                
            StopDynamicSync();
                
            EnsurePause(0);
            EnsurePause(1);

            dualPlayer.DisplayAsPaused();

            currentFrame = e.Time;
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_DualSaveAsked(object sender, EventArgs e)
        {
            // Create and save a composite video with side by side synchronized images.
            // If merge is active, just save one video.
            if (!dualPlayer.Synching || screenList.Count != 2)
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
            
            long iCurrentFrame = currentFrame;
            dualSaveCancelled = false;
            dualSaveFileName = dlgSave.FileName;
            
            // Instanciate and configure the bgWorker.
            bgWorkerDualSave = new BackgroundWorker();
            bgWorkerDualSave.WorkerReportsProgress = true;
            bgWorkerDualSave.WorkerSupportsCancellation = true;
            bgWorkerDualSave.DoWork += bgWorkerDualSave_DoWork;
            bgWorkerDualSave.ProgressChanged += bgWorkerDualSave_ProgressChanged;
            bgWorkerDualSave.RunWorkerCompleted += bgWorkerDualSave_RunWorkerCompleted;
            
            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            dualSaveInProgress = true;
            ps1.DualSaveInProgress = true;
            ps2.DualSaveInProgress = true;
            
            // Create the progress bar and launch the worker.
            dualSaveProgressBar = new formProgressBar(true);
            dualSaveProgressBar.Cancel = dualSave_CancelAsked;
            bgWorkerDualSave.RunWorkerAsync();
            dualSaveProgressBar.ShowDialog();
            
            // If cancelled, delete temporary file.
            if(dualSaveCancelled)
                DeleteTemporaryFile(dualSaveFileName);
            
            // Reset to where we were.
            dualSaveInProgress = false;
            ps1.DualSaveInProgress = false;
            ps2.DualSaveInProgress = false;
            currentFrame = iCurrentFrame;
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_DualSnapshotAsked(object sender, EventArgs e)
        {
            // Retrieve current images and create a composite out of them.
            if (!dualPlayer.Synching || screenList.Count != 2)
                return;
        
            PlayerScreen ps1 = screenList[0] as PlayerScreen;
            PlayerScreen ps2 = screenList[1] as PlayerScreen;
            if (ps1 == null || ps2 == null)
                return;
            
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
                ImageHelper.Save(dlgSave.FileName, composite);

            composite.Dispose();
            leftImage.Dispose();
            rightImage.Dispose();
                
            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }

        private void CCtrl_GrabbingChanged(object sender, EventArgs<bool> e)
        {
            foreach (CaptureScreen screen in captureScreens)
                screen.ForceGrabbingStatus(e.Value);
        }
        private void CCtrl_SnapshotAsked(object sender, EventArgs e)
        {
            foreach (CaptureScreen screen in captureScreens)
                screen.PerformSnapshot();
        }
        private void CCtrl_RecordingChanged(object sender, EventArgs<bool> e)
        {
            foreach (CaptureScreen screen in captureScreens)
                screen.ForceRecordingStatus(e.Value);
        }
        #endregion
        
        #region Public Methods
        public void SetActiveScreen(AbstractScreen screen)
        {
            if(screen == null)
                return;

            if (screenList.Count == 1 || screen == activeScreen)
            {
                activeScreen = screen;
                OrganizeMenus();
                return;
            }

            foreach (AbstractScreen s in screenList)
                s.DisplayAsActiveScreen(s == screen);
                
            activeScreen = screen;
            OrganizeMenus();
        }
        public void SetAllToInactive()
        {
            foreach (AbstractScreen screen in screenList)
                screen.DisplayAsActiveScreen(false);
        }
        public AbstractScreen GetScreenAt(int index)
        {
            return (index >= 0 && index < screenList.Count) ? screenList[index] : null;
        }
        
        public void RemoveFirstEmpty()
        {
            foreach (AbstractScreen screen in screenList)
            {
                if (screen.Full)
                    continue;

                RemoveScreen(screen);
                break;
            }
            
            AfterRemoveScreen();
        }
        public void RemoveScreen(AbstractScreen screen)
        {
            RemoveScreenEventHandlers(screen);
            
            screen.BeforeClose();
            screenList.Remove(screen);
            screen.AfterClose();
            
            AfterRemoveScreen();
        }
        private void AfterRemoveScreen()
        {
            if (screenList.Count > 0)
                SetActiveScreen(screenList[0]);
        }
        
        public void SwapScreens()
        {
            if (screenList.Count != 2)
                return;
            
            AbstractScreen temp = screenList[0];
            screenList[0] = screenList[1];
            screenList[1] = temp;
        }

        public void OrganizeScreens()
        {
            view.OrganizeScreens(screenList);
            UpdateStatusBar();
        }

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

            NotificationCenter.RaiseStatusUpdated(this, StatusString);
        }
        public void OrganizeCommonControls()
        {
            dualPlayer.ScreenListChanged(screenList);
            dualCapture.ScreenListChanged(screenList);
            
            if (screenList.Count == 2)
            {
                Pair<Type, Type> types = new Pair<Type, Type>(screenList[0].GetType(), screenList[1].GetType());
                bool show = types.First == types.Second;
                view.ShowCommonControls(show, types, dualPlayer.View, dualCapture.View);
                canShowCommonControls = show;
            }
            else
            {
                view.ShowCommonControls(false, null, null, null);
                canShowCommonControls = false;
            }
        }
        public void UpdateCaptureBuffers()
        {
            // The screen list has changed and involve capture screens.
            // Update their shared state to trigger a memory buffer reset.
            foreach (CaptureScreen screen in captureScreens)
                screen.SetShared(screenList.Count == 2);
        }
        public void FullScreen(bool fullScreen)
        {
            foreach (AbstractScreen screen in screenList)
                screen.FullScreen(fullScreen);
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
            
            AddSvgSubMenus(svgPath, mnuSVGTools);
        }
        private void AddImportImageMenu(ToolStripMenuItem menu)
        {
            menu.DropDownItems.Add(mnuImportImage);
            menu.DropDownItems.Add(new ToolStripSeparator());
        }
        private void AddSvgSubMenus(string dir, ToolStripMenuItem menu)
        {
            // This is a recursive function that browses a directory and its sub directories,
            // each directory is made into a menu tree, each svg file is added as a menu leaf.
            if (!Directory.Exists(dir))
                return;
            
            buildingSVGMenu = true;

            // Loop sub directories.
            string[] subDirs = Directory.GetDirectories (dir);
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
                    menu.DropDownItems.Add(mnuSubDir);
            }

            // Then loop files within the sub directory.
            foreach (string file in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(file).ToLower().Equals(".svg"))
                    continue;
                
                hasSvgFiles = true;
                        
                // Create a menu. 
                ToolStripMenuItem mnuSVGDrawing = new ToolStripMenuItem();
                mnuSVGDrawing.Text = Path.GetFileNameWithoutExtension(file);
                mnuSVGDrawing.Tag = file;
                mnuSVGDrawing.Image = Properties.Resources.vector;
                mnuSVGDrawing.Click += new EventHandler(mnuSVGDrawing_OnClick);
                mnuSVGDrawing.MergeAction = MergeAction.Append;
                        
                // Add to parent.
                menu.DropDownItems.Add(mnuSVGDrawing);
            }
                    
            buildingSVGMenu = false;
        }
        private void DoOrganizeMenu()
        {
            // Enable / disable menus depending on state of active screen
            // and global screen configuration.
            
            // TODO: fix selection of history stack context.


            #region Menus depending only on the state of the active screen
            bool activeScreenIsEmpty = false;
            if (activeScreen != null && screenList.Count > 0)
            {
                if(!activeScreen.Full)
                {
                    activeScreenIsEmpty = true;
                    HistoryMenuManager.SwitchContext(null);
                }
                else if (activeScreen is PlayerScreen)
                {
                    PlayerScreen player = activeScreen as PlayerScreen;
                    
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
                    mnuSVGTools.Enabled = hasSvgFiles;
                    mnuCoordinateAxis.Enabled = true;
                    
                    mnuDeinterlace.Checked = player.Deinterlaced;
                    mnuMirror.Checked = player.Mirrored;
                    
                    if(!player.IsSingleFrame)
                        ConfigureImageFormatMenus(player);
                    else
                        ConfigureImageFormatMenus(null);
                    
                    // Motion
                    mnuHighspeedCamera.Enabled = true;
                    ConfigureVideoFilterMenus(player);

                    HistoryMenuManager.SwitchContext(player.HistoryStack);
                }
                else if(activeScreen is CaptureScreen)
                {
                    CaptureScreen captureScreen = activeScreen as CaptureScreen;   
                    
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
                    mnuSVGTools.Enabled = hasSvgFiles;
                    mnuCoordinateAxis.Enabled = false;
                    
                    mnuDeinterlace.Checked = false;
                    mnuMirror.Checked = false;
                   
                    ConfigureImageFormatMenus(captureScreen);
                    
                    // Motion
                    mnuHighspeedCamera.Enabled = false;
                    ConfigureVideoFilterMenus(null);

                    HistoryMenuManager.SwitchContext(captureScreen.HistoryStack);
                }
                else
                {
                    // KO ?
                    activeScreenIsEmpty = true;
                    HistoryMenuManager.SwitchContext(null);
                }
            }
            else
            {
                // No active screen. ( = no screens)
                activeScreenIsEmpty = true;
                HistoryMenuManager.SwitchContext(null);
            }

            if (activeScreenIsEmpty)
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
            
            bool allScreensAreEmpty = false;
            switch (screenList.Count)
            {
                case 0:
                    mnuSwapScreens.Enabled = false;
                    mnuToggleCommonCtrls.Enabled = false;
                    allScreensAreEmpty = true;
                    break;

                case 1:
                    mnuSwapScreens.Enabled = false;
                    mnuToggleCommonCtrls.Enabled = false;

                    if(!screenList[0].Full)
                    {
                        allScreensAreEmpty = true;	
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
                        allScreensAreEmpty = true;	
                    }
                    break;

                case 2:
                    mnuSwapScreens.Enabled = true;
                    mnuToggleCommonCtrls.Enabled = canShowCommonControls;
                    
                    // Left Screen
                    if (screenList[0] is PlayerScreen)
                    {
                        if (screenList[0].Full)
                        {
                            allScreensAreEmpty = false;
                            
                            string strCompleteClosingText = strClosingText + " - " + ((PlayerScreen)screenList[0]).FileName;
                            mnuCloseFile.Text = strCompleteClosingText;
                            mnuCloseFile.Enabled = true;
                            mnuCloseFile.Visible = true;
                        }
                        else
                        {
                            // Left screen is an empty PlayerScreen.
                            // Global emptiness might be changed below.
                            allScreensAreEmpty = true;
                        }
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                        // Global emptiness might be changed below.
                        allScreensAreEmpty = true;
                    }

                    // Right Screen.
                    if (screenList[1] is PlayerScreen)
                    {
                        if (screenList[1].Full)
                        {
                            allScreensAreEmpty = false;
                            
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
                    allScreensAreEmpty = true;
                    break;
            }

            if (allScreensAreEmpty)
            {
                // No screens at all, or all screens empty => 1 menu visible but disabled.

                mnuCloseFile.Text = strClosingText;
                mnuCloseFile.Visible = true;
                mnuCloseFile.Enabled = false;

                mnuCloseFile2.Visible = false;
            }
            #endregion
        }
        private void ConfigureVideoFilterMenus(PlayerScreen player)
        {
            bool hasVideo = player != null && player.Full;
            foreach(ToolStripMenuItem menu in filterMenus)
            {
                AbstractVideoFilter filter = menu.Tag as AbstractVideoFilter;
                if(filter == null)
                    continue;
                
                menu.Visible = filter.Experimental ? Software.Experimental : true;
                menu.Enabled = hasVideo ? player.IsCaching : false;
            }
        }
        private void ConfigureImageFormatMenus(AbstractScreen screen)
        {
            // Set the enable and check prop of the image formats menu according of current screen state.
            if(screen == null || 
               !screen.Full ||
              (screen is PlayerScreen && !((PlayerScreen)screen).FrameServer.VideoReader.CanChangeAspectRatio))
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
        
            switch(screen.AspectRatio)
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
            if(!buildingSVGMenu)
            {
                buildingSVGMenu = true;
                // Use "view" object just to merge back into the UI thread.
                view.BeginInvoke((MethodInvoker) delegate {DoSVGFilesChanged();});
            }
        }
        public void DoSVGFilesChanged()
        {
            mnuSVGTools.DropDownItems.Clear();
            AddImportImageMenu(mnuSVGTools);
            AddSvgSubMenus(svgPath, mnuSVGTools);
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
            foreach(ToolStripMenuItem menu in filterMenus)
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
            
            if (!dualPlayer.Synching || screenList.Count != 2)
                return;
            
            PlayerScreen ps1 = screenList[0] as PlayerScreen;
            PlayerScreen ps2 = screenList[1] as PlayerScreen;
            if(ps1 == null && ps2 == null)
                return;
            
            // Todo: get frame interval from one of the videos.
                
            // Get first frame outside the loop, to be able to set video size.
            currentFrame = 0;
            OnCommonPositionChanged(currentFrame, false);
            
            Bitmap img1 = ps1.GetFlushedImage();
            Bitmap img2 = null;
            Bitmap composite;
            if (!dualPlayer.Merging)
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
            SaveResult result = videoFileWriter.OpenSavingContext(dualSaveFileName, vi, -1, false);
    
            if(result != SaveResult.Success)
            {
                e.Result = 2;
                return;
            }
            
            videoFileWriter.SaveFrame(composite);
            
            img1.Dispose();
            if (!dualPlayer.Merging)
            {
                img2.Dispose();
                composite.Dispose();
            }

            // Loop all remaining frames in static sync mode, but without refreshing the UI.
            while(currentFrame < maxFrame && !dualSaveCancelled)
            {
                currentFrame++;
                
                if(bgWorkerDualSave.CancellationPending)
                {
                    e.Result = 1;
                    dualSaveCancelled = true;
                    break;
                }
                
                // Move both playheads and get the composite image.
                OnCommonPositionChanged(-1, false);
                img1 = ps1.GetFlushedImage();
                composite = img1;				
                if(!dualPlayer.Merging)
                {
                    img2 = ps2.GetFlushedImage();
                    composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
                }
                
                videoFileWriter.SaveFrame(composite);
            
                img1.Dispose();
                if (!dualPlayer.Merging)
                {
                    img2.Dispose();
                    composite.Dispose();
                }
            
                int percent = (int)(((double)(currentFrame+1)/maxFrame) * 100);
                bgWorkerDualSave.ReportProgress(percent);
            }
            
            if(!dualSaveCancelled)
                e.Result = 0;
        }
        private void bgWorkerDualSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(bgWorkerDualSave.CancellationPending)
                return;

            dualSaveProgressBar.Update(Math.Min(e.ProgressPercentage, 100), 100, true);
        }
        private void bgWorkerDualSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dualSaveProgressBar.Close();
            dualSaveProgressBar.Dispose();
            
            if(!dualSaveCancelled && (int)e.Result != 1)
                videoFileWriter.CloseSavingContext((int)e.Result == 0);
            
            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }
        private void dualSave_CancelAsked(object sender, EventArgs e)
        {
            // This will simply set BgWorker.CancellationPending to true,
            // which we check periodically in the saving loop.
            // This will also end the bgWorker immediately,
            // maybe before we check for the cancellation in the other thread. 
            videoFileWriter.CloseSavingContext(false);
            dualSaveCancelled = true;
            bgWorkerDualSave.CancelAsync();
        }
        private void DeleteTemporaryFile(string filename)
        {
            log.Debug("Side by side video saving cancelled. Deleting temporary file.");
            if (!File.Exists(filename))
                return;
            
            try
            {
                File.Delete(filename);
            }
            catch (Exception exp)
            {
                log.Error("Error while deleting temporary file.");
                log.Error(exp.Message);
                log.Error(exp.StackTrace);
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
            ScreenRemover.RemoveScreen(this, screenIndex);
            OrganizeScreens();
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuSaveOnClick(object sender, EventArgs e)
        {
            SaveData();
        }
        
        public void SaveData()
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;
            
            DoStopPlaying();
            player.Save();
        }
        private void mnuLoadAnalysisOnClick(object sender, EventArgs e)
        {
            if (activeScreen != null && activeScreen is PlayerScreen)
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
            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            MetadataSerializer s = new MetadataSerializer();
            s.Load(((PlayerScreen)activeScreen).FrameServer.Metadata, openFileDialog.FileName, true);
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
        private void ExportSpreadsheet(MetadataExportFormat format)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null || !player.FrameServer.Metadata.HasData)
                return;
            
            DoStopPlaying();    

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgExportSpreadsheet_Title;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = ScreenManagerLang.dlgExportSpreadsheet_Filter;
                    
            saveFileDialog.FilterIndex = ((int)format) + 1;
                        
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(player.FrameServer.Metadata.FullPath);

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            MetadataExporter.Export(player.FrameServer.Metadata, saveFileDialog.FileName, format);
        }
        #endregion

        #region View
        private void mnuHome_OnClick(object sender, EventArgs e)
        {
            // Remove all screens.
            if(screenList.Count <= 0)
                return;
            
            if(ScreenRemover.RemoveScreen(this, 0))
            {
                dualPlayer.Synching = false;
                
                // Second screen is now in [0] spot.
                if(screenList.Count > 0)
                    ScreenRemover.RemoveScreen(this, 0);
            }

            OrganizeScreens();
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
            
            dualPlayer.Synching = false;

            switch (screenList.Count)
            {
                case 0:
                    {
                        AddPlayerScreen();
                        break;
                    }
                case 1:
                    {
                        if(screenList[0] is CaptureScreen)
                        {
                            // Currently : 1 capture. -> remove and add a player.
                            ScreenRemover.RemoveScreen(this, 0);
                            AddPlayerScreen();
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
                            ScreenRemover.RemoveScreen(this, 0);
                            ScreenRemover.RemoveScreen(this, 0);
                            AddPlayerScreen();
                        }
                        else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
                        {
                            // [capture][player] -> remove capture.	
                            ScreenRemover.RemoveScreen(this, 0);
                        }
                        else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
                        {
                            // [player][capture] -> remove capture.	
                            ScreenRemover.RemoveScreen(this, 1);
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
                                ScreenRemover.RemoveScreen(this, 0);
                            else
                                ScreenRemover.RemoveScreen(this, 1);
                        }
                        break;
                    }
                default:
                    break;
            }

            OrganizeScreens();
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
            dualPlayer.Synching = false;
            
            switch (screenList.Count)
            {
                case 0:
                    {
                        // Currently : 0 screens. -> add two players.
                        // We use two different commands to keep the undo history working.
                        AddPlayerScreen();
                        AddPlayerScreen();
                        break;
                    }
                case 1:
                    {
                        if(screenList[0] is CaptureScreen)
                        {
                            // Currently : 1 capture. -> remove and add 2 players.
                            ScreenRemover.RemoveScreen(this, 0);
                            AddPlayerScreen();
                            AddPlayerScreen();
                        }
                        else
                        {
                            // Currently : 1 player. -> add another.
                            AddPlayerScreen();
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
                            ScreenRemover.RemoveScreen(this, 0);
                            ScreenRemover.RemoveScreen(this, 0);
                            AddPlayerScreen();
                            AddPlayerScreen();
                        }
                        else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
                        {
                            // [capture][player] -> remove capture and add player.
                            ScreenRemover.RemoveScreen(this, 0);
                            AddPlayerScreen();
                        }
                        else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
                        {
                            // [player][capture] -> remove capture and add player.
                            ScreenRemover.RemoveScreen(this, 1);
                            AddPlayerScreen();
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

            OrganizeScreens();
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
            dualPlayer.Synching = false;
            
            switch (screenList.Count)
            {
                case 0:
                    {
                        // Currently : 0 screens. -> add a capture.
                        AddCaptureScreen();
                        break;
                    }
                case 1:
                    {
                        if(screenList[0] is PlayerScreen)
                        {
                            // Currently : 1 player. -> remove and add a capture.
                            if(ScreenRemover.RemoveScreen(this, 0))
                                AddCaptureScreen();
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
                                ScreenRemover.RemoveScreen(this, 0);
                            else
                                ScreenRemover.RemoveScreen(this, 1);
                        }
                        else if(screenList[0] is CaptureScreen && screenList[1] is PlayerScreen)
                        {
                            // [capture][player] -> remove player.	
                            ScreenRemover.RemoveScreen(this, 1);
                        }
                        else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
                        {
                            // [player][capture] -> remove player.
                            ScreenRemover.RemoveScreen(this, 0);
                        }
                        else
                        {
                            // remove both and add one capture.
                            if(ScreenRemover.RemoveScreen(this, 0))
                            {
                                // remaining player has moved in [0] spot.
                                if(ScreenRemover.RemoveScreen(this, 0))
                                    AddCaptureScreen();
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            UpdateCaptureBuffers();
            
            OrganizeScreens();
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
            dualPlayer.Synching = false;
            
            switch (screenList.Count)
            {
                case 0:
                    {
                        // Currently : 0 screens. -> add two capture.
                        AddCaptureScreen();
                        AddCaptureScreen();
                        break;
                    }
                case 1:
                    {
                        if(screenList[0] is CaptureScreen)
                        {
                            // Currently : 1 capture. -> add another.
                            AddCaptureScreen();
                        }
                        else
                        {
                            // Currently : 1 player. -> remove and add 2 capture.
                            if(ScreenRemover.RemoveScreen(this, 0))
                            {
                                AddCaptureScreen();
                                AddCaptureScreen();
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
                            if(ScreenRemover.RemoveScreen(this, 1))
                                AddCaptureScreen();
                        }
                        else if(screenList[0] is PlayerScreen && screenList[1] is CaptureScreen)
                        {
                            // [player][capture] -> remove player and add capture.
                            if(ScreenRemover.RemoveScreen(this, 0))
                                AddCaptureScreen();
                        }
                        else
                        {
                            // [player][player] -> remove both and add 2 capture.
                            if(ScreenRemover.RemoveScreen(this, 0))
                            {
                                // remaining player has moved in [0] spot.
                                if(ScreenRemover.RemoveScreen(this, 0))
                                {
                                    AddCaptureScreen();
                                    AddCaptureScreen();
                                }
                            }
                        }
                        
                        break;
                    }
                default:
                    break;
            }
            
            UpdateCaptureBuffers();
            
            OrganizeScreens();
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
            dualPlayer.Synching = false;
            
            switch (screenList.Count)
            {
                case 0:
                    {
                        // Currently : 0 screens. -> add a capture and a player.
                        AddCaptureScreen();
                        AddPlayerScreen();
                        break;
                    }
                case 1:
                    {
                        if(screenList[0] is CaptureScreen)
                        {
                            // Currently : 1 capture. -> add a player.
                            AddPlayerScreen();
                        }
                        else
                        {
                            // Currently : 1 player. -> add a capture.
                            AddCaptureScreen();
                        }
                        break;
                    }
                case 2:
                    {
                        // We need to decide which screen(s) to remove/replace.
                        
                        if(screenList[0] is CaptureScreen && screenList[1] is CaptureScreen)
                        {
                            // [capture][capture] -> remove right and add player.
                            ScreenRemover.RemoveScreen(this, 1);
                            AddPlayerScreen();
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
                            if(ScreenRemover.RemoveScreen(this, 1))
                                AddCaptureScreen();
                        }
                        
                        break;
                    }
                default:
                    break;
            }

            UpdateCaptureBuffers();
            
            OrganizeScreens();
            OrganizeCommonControls();
            OrganizeMenus();
        }
        private void mnuSwapScreensOnClick(object sender, EventArgs e)
        {
            if (screenList.Count != 2)
                return;

            SwapScreens();
            OrganizeScreens();
            OrganizeMenus();
            UpdateStatusBar();
            SwapSync();
            SetSyncPoint(true);
        }
        private void mnuToggleCommonCtrlsOnClick(object sender, EventArgs e)
        {
            view.ToggleCommonControls();
        }
        #endregion

        #region Image
        private void mnuDeinterlaceOnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
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
        private void ChangeAspectRatio(Video.ImageAspectRatio _aspectRatio)
        {
            if(activeScreen == null)
                return;
        
            if(activeScreen.AspectRatio != _aspectRatio)
                activeScreen.AspectRatio = _aspectRatio;
            
            mnuFormatForce43.Checked = _aspectRatio == ImageAspectRatio.Force43;
            mnuFormatForce169.Checked = _aspectRatio == ImageAspectRatio.Force169;
            mnuFormatAuto.Checked = _aspectRatio == ImageAspectRatio.Auto;
        }
        private void mnuMirrorOnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if(player != null)
            {
                mnuMirror.Checked = !mnuMirror.Checked;
                player.Mirrored = mnuMirror.Checked;
            }
        }
        private void mnuImportImage_OnClick(object sender, EventArgs e)
        {
            if(activeScreen == null || !activeScreen.CapabilityDrawings)
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
        private void LoadDrawing(string path, bool isSVG)
        {
            if(path != null && path.Length > 0 && activeScreen != null && activeScreen.CapabilityDrawings)
            {
                activeScreen.AddImageDrawing(path, isSVG);
            }	
        }
        private void mnuCoordinateAxis_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowCoordinateSystem();
        }
        #endregion

        #region Motion
        private void mnuHighspeedCamera_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps != null)
                ps.ConfigureHighSpeedCamera();
        }
        #endregion
        #endregion

        #region Services
        private void VideoTypeManager_VideoLoadAsked(object sender, VideoLoadAskedEventArgs e)
        {
            DoLoadMovieInScreen(e.Path, e.Target);
        }
        
        private void DoLoadMovieInScreen(string path, int targetScreen)
        {
            if(!File.Exists(path))
                return;

            if (Path.GetExtension(path).ToLower() == ".kva" && targetScreen >= 0)
            {
                // Special case of loading a KVA file on top of a loaded video.
                AbstractScreen screen = GetScreenAt(targetScreen);
                if (screen == null || !screen.Full)
                    return;

                screen.LoadKVA(path);
            }
            else
            {
                LoaderVideo.LoadVideoInScreen(this, path, targetScreen);
            }
            
            // No need to call PrepareSync here because it will be called when the working zone is set anyway.
        }
        
        public void DoLoadCameraInScreen(CameraSummary summary, int targetScreen)
        {
            if(summary == null)
                return;

            LoaderCamera.LoadCameraInScreen(this, summary, targetScreen);
        }
        
        public void DoStopPlaying()
        {
            foreach (PlayerScreen player in playerScreens)
                player.StopPlaying();
            
            StopDynamicSync();
            dualPlayer.DisplayAsPaused();
        }

        private void View_FileLoadAsked(object source, FileLoadAskedEventArgs e)
        {
            DoLoadMovieInScreen(e.Source, e.Target);
        }

        private void CameraTypeManager_CameraLoadAsked(object source, CameraLoadAskedEventArgs e)
        {
            CameraTypeManager.StopDiscoveringCameras();
            DoLoadCameraInScreen(e.Source, e.Target);
        }

        private void View_AutoLaunchAsked(object source, EventArgs e)
        {
            int reloaded = 0;
            foreach (IScreenDescription screenDescription in LaunchSettingsManager.ScreenDescriptions)
            {
                if (screenDescription is ScreenDescriptionPlayback)
                {
                    AddPlayerScreen();

                    ScreenDescriptionPlayback sdp = screenDescription as ScreenDescriptionPlayback;
                    LoaderVideo.LoadVideoInScreen(this, sdp.FullPath, sdp);
                    reloaded++;
                }

                if (reloaded == 2)
                    break;
            }

            if (reloaded > 0)
            {
                OrganizeScreens();
                OrganizeCommonControls();
                OrganizeMenus();
            }
        }

        #endregion

        #region Keyboard Handling
        private void ActivateOtherScreen()
        {
            if (screenList.Count != 2)
                return;
            
            if (activeScreen == screenList[0])
                SetActiveScreen(screenList[1]);
            else
                SetActiveScreen(screenList[0]);
        }
        #endregion

        #region Synchronisation
        private void PrepareSync(bool initialization)
        {
            // Called each time the screen list change 
            // or when a screen changed selection.
            
            // We don't care which video was updated.
            // Set sync mode and reset sync.
            dualPlayer.Synching = false;

            if ( (screenList.Count == 2))
            {
                if ((screenList[0] is PlayerScreen) && (screenList[1] is PlayerScreen))
                {
                    if (((PlayerScreen)screenList[0]).Full && ((PlayerScreen)screenList[1]).Full)
                    {
                        dualPlayer.Synching = true;
                        ((PlayerScreen)screenList[0]).Synched = true;
                        ((PlayerScreen)screenList[1]).Synched = true;

                        if (initialization)
                        {
                            log.Debug("PrepareSync() - Initialization (reset of sync point).");
                            // Static Sync
                            rightSyncFrame = 0;
                            leftSyncFrame = 0;
                            syncLag = 0;
                            currentFrame = 0;
                            
                            ((PlayerScreen)screenList[0]).SyncPosition = 0;
                            ((PlayerScreen)screenList[1]).SyncPosition = 0;
                            dualPlayer.UpdateSyncPosition(currentFrame);

                            // Dynamic Sync
                            ResetDynamicSyncFlags();
                            
                            // Sync Merging
                            ((PlayerScreen)screenList[0]).SyncMerge = false;
                            ((PlayerScreen)screenList[1]).SyncMerge = false;
                            dualPlayer.Merging = false;
                        }

                        // Mise à jour trkFrame
                        SetSyncLimits();

                        // Mise à jour Players
                        OnCommonPositionChanged(currentFrame, true);
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
                foreach (PlayerScreen p in playerScreens)
                {
                    p.Synched = false;
                }

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

            if (!dualPlayer.Synching) 
            { 
                StopDynamicSync();
                dualPlayer.DisplayAsPaused();
            }
        }
        public void SetSyncPoint(bool intervalOnly)
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
            // Si _bIntervalOnly, on ne veut pas changer les frames de référence
            // (Généralement après une modification du framerate de l'une des vidéos ou swap)
            //----------------------------------------------------------------------------
            if (dualPlayer.Synching && screenList.Count == 2)
            {
                // Registers current positions.
                if (!intervalOnly)
                {
                    // For timing label only
                    ((PlayerScreen)screenList[0]).SyncPosition = ((PlayerScreen)screenList[0]).Position;
                    ((PlayerScreen)screenList[1]).SyncPosition = ((PlayerScreen)screenList[1]).Position;
    
                    leftSyncFrame = ((PlayerScreen)screenList[0]).CurrentFrame;
                    rightSyncFrame = ((PlayerScreen)screenList[1]).CurrentFrame;
                    
                    log.Debug(String.Format("New Sync Points:[{0}][{1}], Sync lag:{2}", leftSyncFrame, rightSyncFrame, rightSyncFrame - leftSyncFrame));
                }
    
    
                // Sync Lag is expressed in frames.
                syncLag = rightSyncFrame - leftSyncFrame;
    
                // We need to recompute the lag in milliseconds because it can change even when 
                // the references positions don't change. For exemple when varying framerate (speed).
                long iLeftSyncMilliseconds = (long)(((PlayerScreen)screenList[0]).FrameInterval * leftSyncFrame);
                long iRightSyncMilliseconds = (long)(((PlayerScreen)screenList[1]).FrameInterval * rightSyncFrame);
                syncLagMilliseconds = iRightSyncMilliseconds - iLeftSyncMilliseconds;
    
                // Update common position (sign of m_iSyncLag might have changed.)
                currentFrame = syncLag > 0 ? rightSyncFrame : leftSyncFrame;

                dualPlayer.UpdateSyncPosition(currentFrame);  // <-- expects timestamp ?
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

            if (syncLag > 0)
            {
                // Lag is positive. Right video starts first and its duration stay the same as original.
                // Left video has to wait for an ammount of time.

                // Check if lag is still valid. (?) Why is this needed ?
                if (syncLag > rightEstimatedFrames)
                    syncLag = 0; 

                leftEstimatedFrames += syncLag;
            }
            else
            {
                // Lag is negative. Left video starts first and its duration stay the same as original.
                // Right video has to wait for an ammount of time.
                
                // Get Lag in frames of right video
                //int iSyncLagFrames = ((PlayerScreen)screenList[1]).NormalizedToFrame(m_iSyncLag);

                // Check if lag is still valid.(?)
                if (-syncLag > leftEstimatedFrames)
                    syncLag = 0;
                
                rightEstimatedFrames += (-syncLag);
            }

            maxFrame = (int)Math.Max(leftEstimatedFrames, rightEstimatedFrames);
            dualPlayer.SetupTrkFrame(0, maxFrame, currentFrame);

            log.DebugFormat("m_iSyncLag:{0}, m_iSyncLagMilliseconds:{1}, MaxFrames:{2}", syncLag, syncLagMilliseconds, maxFrame);
        }
        private void OnCommonPositionChanged(long frame, bool allowUIUpdate)
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
            long leftFrame = 0;
            long rightFrame = 0;

            if (frame >= 0)
            {
                if (syncLag > 0)
                {
                    // Right video must go ahead.

                    rightFrame = frame;
                    leftFrame = frame - syncLag;
                    if (leftFrame < 0)
                        leftFrame = 0;
                }
                else
                {
                    // Left video must go ahead.

                    leftFrame = frame;
                    rightFrame = frame - (-syncLag);
                    if (rightFrame < 0)
                        rightFrame = 0;
                }

                // Force positions.
                ((PlayerScreen)screenList[0]).GotoFrame(leftFrame, allowUIUpdate);
                ((PlayerScreen)screenList[1]).GotoFrame(rightFrame, allowUIUpdate);
            }
            else
            {
                // Special case for ++.
                if (syncLag > 0)
                {
                    // Right video must go ahead.
                    ((PlayerScreen)screenList[1]).GotoNextFrame(allowUIUpdate);

                    if (currentFrame > syncLag)
                        ((PlayerScreen)screenList[0]).GotoNextFrame(allowUIUpdate);
                }
                else
                {
                    // Left video must go ahead.
                    ((PlayerScreen)screenList[0]).GotoNextFrame(allowUIUpdate);

                    if (currentFrame > -syncLag)
                        ((PlayerScreen)screenList[1]).GotoNextFrame(allowUIUpdate);
                }
            }
        }
        public void SwapSync()
        {
            if (!dualPlayer.Synching || screenList.Count != 2)
                return;
            
            long temp = leftSyncFrame;
            leftSyncFrame = rightSyncFrame;
            rightSyncFrame = temp;

            ResetDynamicSyncFlags();
        }
        private void StartDynamicSync()
        {
            dynamicSynching = true;
            DynamicSync();
        }
        private void StopDynamicSync()
        {
            dynamicSynching = false;
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
            
            if (!dualPlayer.Synching || screenList.Count != 2)
            {
                // This can happen when a screen is closed on the fly while dualPlayer.Synching.
                StopDynamicSync();
                dualPlayer.Synching = false;
                dualPlayer.DisplayAsPaused();
                return;
            }

            // L'ensemble de la supervision est réalisée en TimeStamps.
            // Seul les décision de lancer / arrêter sont établies par rapport
            // au temps auquel on est.

            long leftPosition = ((PlayerScreen)screenList[0]).CurrentFrame;
            long rightPosition = ((PlayerScreen)screenList[1]).CurrentFrame;
            long leftMilliseconds = (long)(leftPosition * ((PlayerScreen)screenList[0]).FrameInterval);
            long rightMilliseconds = (long)(rightPosition * ((PlayerScreen)screenList[1]).FrameInterval);

            //-----------------------------------------------------------------------
            // Dans cette fonction, on part du principe que les deux vidéos tournent.
            // Et on fait des 'Ensure Pause' quand nécessaire.
            // On évite les Ensure Play' car l'utilisateur a pu 
            // manuellement pauser une vidéo.
            //-----------------------------------------------------------------------
            #region [i][0]
            if (leftPosition > 0 && rightPosition == 0)
            {
                EnsurePlay(0);
                    
                // Etat 4. [i][0]
                leftIsStarting = false;

                if (syncLag == 0)
                {
                    //-----------------------------------------------------
                    // La vidéo de droite 
                    // - vient de boucler et on doit attendre l'autre
                    // - est en train de repartir.
                    //-----------------------------------------------------
                    if (!rightIsStarting)
                    {
                        // Stop pour bouclage
                        EnsurePause(1);
                    }

                    currentFrame = leftPosition;
                }
                else if (syncLagMilliseconds > 0)
                {
                    // La vidéo de droite est sur 0 et doit partir en premier.
                    // Si elle n'est pas en train de repartir, c'est qu'on 
                    // doit attendre que la vidéo de gauche ait finit son tour.
                    if (!rightIsStarting)
                    {
                        EnsurePause(1);
                        currentFrame = leftPosition + syncLag;
                    }
                    else
                    {
                        currentFrame = leftPosition;
                    }
                }
                else if (syncLagMilliseconds < 0)
                {
                    // La vidéo de droite est sur 0, en train de prendre son retard.
                    // On la relance si celle de gauche a fait son décalage.

                    // Attention, ne pas relancer si celle de gauche est en fait en train de terminer son tour
                    if (!leftIsCatchingUp && !rightIsStarting)
                    {
                        EnsurePause(1);
                        currentFrame = leftPosition;
                    }
                    else if (leftMilliseconds > (-syncLagMilliseconds) - 24)
                    {
                        // La vidéo de gauche est sur le point de franchir le sync point.
                        // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                        // la vidéo qui est partie en premier...
                        EnsurePlay(1);
                        rightIsStarting = true;
                        leftIsCatchingUp = false;
                        currentFrame = leftPosition;
                    }
                    else
                    {
                        // La vidéo de gauche n'a pas encore fait son décalage.
                        // On ne force pas sa lecture. (Pause manuelle possible).
                        leftIsCatchingUp = true;
                        currentFrame = leftPosition;
                    }
                }
            }
            #endregion
            #region [0][0]
            else if (leftPosition == 0 && rightPosition == 0)
            {
                // Etat 1. [0][0]
                currentFrame = 0;

                // Les deux vidéos viennent de boucler ou sont en train de repartir.
                if (syncLag == 0)
                {
                    //---------------------
                    // Redemmarrage commun.
                    //---------------------
                    if (!leftIsStarting && !rightIsStarting)
                    {
                        EnsurePlay(0);
                        EnsurePlay(1);

                        rightIsStarting = true;
                        leftIsStarting = true;
                    }
                }
                else if (syncLagMilliseconds > 0)
                {
                    // Redemarrage uniquement de la vidéo de droite, 
                    // qui doit faire son décalage

                    EnsurePause(0);
                    EnsurePlay(1);
                    rightIsStarting = true;
                    rightIsCatchingUp = true;
                }
                else if (syncLagMilliseconds < 0)
                {
                    // Redemarrage uniquement de la vidéo de gauche, 
                    // qui doit faire son décalage

                    EnsurePlay(0);
                    EnsurePause(1);
                    leftIsStarting = true;
                    leftIsCatchingUp = true;
                }
            }
            #endregion
            #region [0][i]
            else if (leftPosition == 0 && rightPosition > 0)
            {
                // Etat [0][i]
                EnsurePlay(1);
                    
                rightIsStarting = false;

                if (syncLag == 0)
                {
                    currentFrame = rightPosition;

                    //--------------------------------------------------------------------
                    // Configuration possible : la vidéo de gauche vient de boucler.
                    // On la stoppe en attendant le redemmarrage commun.
                    //--------------------------------------------------------------------
                    if (!leftIsStarting)
                    {
                        EnsurePause(0);
                    }
                }
                else if (syncLagMilliseconds > 0)
                {
                    // La vidéo de gauche est sur 0, en train de prendre son retard.
                    // On la relance si celle de droite a fait son décalage.

                    // Attention ne pas relancer si la vidéo de droite est en train de finir son tour
                    if (!rightIsCatchingUp && !leftIsStarting)
                    {
                        // La vidéo de droite est en train de finir son tour tandisque celle de gauche a déjà bouclé.
                        EnsurePause(0);
                        currentFrame = rightPosition;
                    }
                    else if (rightMilliseconds > syncLagMilliseconds - 24)
                    {
                        // La vidéo de droite est sur le point de franchir le sync point.
                        // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                        // la vidéo qui est partie en premier...
                        EnsurePlay(0);
                        leftIsStarting = true;
                        rightIsCatchingUp = false;
                        currentFrame = rightPosition;
                    }
                    else
                    {
                        // La vidéo de droite n'a pas encore fait son décalage.
                        // On ne force pas sa lecture. (Pause manuelle possible).
                        rightIsCatchingUp = true;
                        currentFrame = rightPosition;
                    }
                }
                else if (syncLagMilliseconds < 0)
                {
                    // La vidéo de gauche est sur 0 et doit partir en premier.
                    // Si elle n'est pas en train de repartir, c'est qu'on 
                    // doit attendre que la vidéo de droite ait finit son tour.
                    if (!leftIsStarting)
                    {
                        EnsurePause(0);
                        currentFrame = rightPosition + syncLag;
                    }
                    else
                    {
                        // Rare, les deux première frames de chaque vidéo n'arrivent pas en même temps
                        currentFrame = rightPosition;
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
                    
                leftIsStarting = false;
                rightIsStarting = false;

                currentFrame = Math.Max(leftPosition, rightPosition);
            }
            #endregion

            // Update position for trkFrame.
            object[] parameters = new object[] { currentFrame };
                
            // Note: do we need to begin invoke here ?
            //view.BeginInvoke(view.delegateUpdateTrackerFrame, parameters);
            dualPlayer.UpdateTrkFrame(currentFrame);

            //log.Debug(String.Format("Tick:[{0}][{1}], Starting:[{2}][{3}], Catching up:[{4}][{5}]", iLeftPosition, iRightPosition, m_bLeftIsStarting, m_bRightIsStarting, m_bLeftIsCatchingUp, m_bRightIsCatchingUp));
        }
        private void EnsurePause(int screenIndex)
        {
            //log.Debug(String.Format("Ensuring pause of screen [{0}]", _iScreen));
            if (screenIndex < screenList.Count)
            {
                if (((PlayerScreen)screenList[screenIndex]).IsPlaying)
                    ((PlayerScreen)screenList[screenIndex]).view.OnButtonPlay();
            }
            else
            {
                dualPlayer.Synching = false;
                dualPlayer.DisplayAsPaused();
            }
        }
        private void EnsurePlay(int screenIndex)
        {
            //log.Debug(String.Format("Ensuring play of screen [{0}]", _iScreen));
            if (screenIndex < screenList.Count)
            {
                if (!((PlayerScreen)screenList[screenIndex]).IsPlaying)
                    ((PlayerScreen)screenList[screenIndex]).view.OnButtonPlay();
            }
            else
            {
                dualPlayer.Synching = false;
                dualPlayer.DisplayAsPaused();
            }
        }
        private void ResetDynamicSyncFlags()
        {
            rightIsStarting = false;
            leftIsStarting = false;
            leftIsCatchingUp = false;
            rightIsCatchingUp = false;
        }
        private void SyncCatch()
        {
            // We sync back the videos.
            // Used when one video has been moved individually.
            log.Debug("SyncCatch() called.");
            long leftFrame = ((PlayerScreen)screenList[0]).CurrentFrame;
            long rightFrame = ((PlayerScreen)screenList[1]).CurrentFrame;

            if (syncLag > 0)
            {
                // Right video goes ahead.
                if (leftFrame + syncLag == currentFrame || (currentFrame < syncLag && leftFrame == 0))
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    currentFrame = rightFrame;
                }
                else if (rightFrame == currentFrame)
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    currentFrame = leftFrame + syncLag;
                }
                else
                {
                    // Both videos were moved.
                    currentFrame = leftFrame + syncLag;
                }
            }
            else
            {
                // Left video goes ahead.
                if (rightFrame - syncLag == currentFrame || (currentFrame < -syncLag && rightFrame == 0))
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    currentFrame = leftFrame;
                }
                else if (leftFrame == currentFrame)
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    currentFrame = rightFrame - syncLag;
                }
                else
                {
                    // Both videos were moved.
                    currentFrame = leftFrame;
                }
            }

            OnCommonPositionChanged(currentFrame, true);
            dualPlayer.UpdateTrkFrame(currentFrame);

        }
        #endregion

        #region Screen organization
        public void AddPlayerScreen()
        {
            PlayerScreen screen = new PlayerScreen();
            screen.RefreshUICulture();
            AddScreen(screen);
        }
        public void AddCaptureScreen()
        {
            CaptureScreen screen = new CaptureScreen();
            if (screenList.Count > 1)
                screen.SetShared(true);

            screen.RefreshUICulture();
            AddScreen(screen);
        }

        /// <summary>
        /// Looks for a camera screen or a non-loaded player screen.
        /// </summary>
        public int FindEmptyScreen()
        {
            AbstractScreen screen0 = GetScreenAt(0);
            if (!screen0.Full)
                return 0;

            AbstractScreen screen1 = GetScreenAt(1);
            if (!screen1.Full)
                return 1;

            return -1;
        }
        /// <summary>
        /// Asks the user for confirmation on replacing the current content.
        /// Check if we are overloading on a non-empty screen and propose to save data.
        /// </summary>
        /// <returns>true if the loading can go on</returns>
        public bool BeforeReplacingPlayerContent(int targetScreen)
        {
            PlayerScreen player = GetScreenAt(targetScreen) as PlayerScreen;
            if (player == null || !player.FrameServer.Metadata.IsDirty)
                return true;

            DialogResult save = ShowConfirmDirtyDialog();
            if (save == DialogResult.No)
            {
                return true;
            }
            else if (save == DialogResult.Cancel)
            {
                return false;
            }
            else
            {
                // TODO: shouldn't we save the correct screen instead of just the active one ?
                SaveData();
                return true;
            }
        }
        private DialogResult ShowConfirmDirtyDialog()
        {
            return MessageBox.Show(
                ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n"),
                ScreenManagerLang.InfoBox_MetadataIsDirty_Title,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }
        private void AddScreen(AbstractScreen screen)
        {
            AddScreenEventHandlers(screen);
            screenList.Add(screen);
        }
        private void AddScreenEventHandlers(AbstractScreen screen)
        {
            screen.CloseAsked += Screen_CloseAsked;
            screen.Activated += Screen_Activated;
            screen.CommandProcessed += Screen_CommandProcessed;

            if (screen is PlayerScreen)
                AddPlayerScreenEventHandlers(screen as PlayerScreen);
            
        }
        private void AddPlayerScreenEventHandlers(PlayerScreen player)
        {
            player.SpeedChanged += Player_SpeedChanged;
            player.PauseAsked += Player_PauseAsked;
            player.SelectionChanged += Player_SelectionChanged;
            player.ImageChanged += Player_ImageChanged;
            player.SendImage += Player_SendImage;
            player.ResetAsked += Player_ResetAsked;
        }
        private void RemoveScreenEventHandlers(AbstractScreen screen)
        {
            screen.CloseAsked -= Screen_CloseAsked;
            screen.Activated -= Screen_Activated;
            screen.CommandProcessed -= Screen_CommandProcessed;

            if (screen is PlayerScreen)
                RemovePlayerScreenEventHandlers(screen as PlayerScreen);
        }
        private void RemovePlayerScreenEventHandlers(PlayerScreen player)
        {
            player.SpeedChanged -= Player_SpeedChanged;
            player.PauseAsked -= Player_PauseAsked;
            player.SelectionChanged -= Player_SelectionChanged;
            player.ImageChanged -= Player_ImageChanged;
            player.SendImage -= Player_SendImage;
            player.ResetAsked -= Player_ResetAsked;
        }
        
        #endregion
    }
}
