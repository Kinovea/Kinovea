#region License
/*
Copyright ｩ Joan Charmant 2008.
jcharmant@gmail.com 
 
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
        private int dualLaunchSettingsPendingCountdown;
        private List<string> camerasToDiscover = new List<string>();
        private AudioInputLevelMonitor audioInputLevelMonitor = new AudioInputLevelMonitor();
        
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
        private ToolStripMenuItem mnuSaveAs = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportSpreadsheet = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportODS = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportXLSX = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportJSON = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportCSV = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLoadAnalysis = new ToolStripMenuItem();

        private ToolStripMenuItem mnuCutDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopyDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPasteDrawing = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuOnePlayer = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoPlayers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOneCapture = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoCaptures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoMixed = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSwapScreens = new ToolStripMenuItem();
        private ToolStripMenuItem mnuToggleCommonCtrls = new ToolStripMenuItem();

        private ToolStripMenuItem mnuDeinterlace = new ToolStripMenuItem();

        private ToolStripMenuItem mnuDemosaic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicNone = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicRGGB = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicBGGR = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicGRBG = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicGBRG = new ToolStripMenuItem();

        private ToolStripMenuItem mnuFormat = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatAuto = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatForce43 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFormatForce169 = new ToolStripMenuItem();

        private ToolStripMenuItem mnuRotation = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation0 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation90 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation180 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation270 = new ToolStripMenuItem();

        private ToolStripMenuItem mnuMirror = new ToolStripMenuItem();

        private ToolStripMenuItem mnuTimebase = new ToolStripMenuItem();

        private ToolStripMenuItem mnuSVGTools = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTestGrid = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCoordinateAxis = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCameraCalibration = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTrajectoryAnalysis = new ToolStripMenuItem();
        private ToolStripMenuItem mnuScatterDiagram = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAngularAnalysis = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAngleAngleAnalysis = new ToolStripMenuItem();

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
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & initialization
        public ScreenManagerKernel()
        {
            log.Debug("Module Construction: ScreenManager.");

            view = new ScreenManagerUserInterface();
            view.FileLoadAsked += View_FileLoadAsked;
            view.AutoLaunchAsked += View_AutoLaunchAsked;
            AddCommonControlsEventHandlers();

            CameraTypeManager.CameraLoadAsked += CameraTypeManager_CameraLoadAsked;
            VideoTypeManager.VideoLoadAsked += VideoTypeManager_VideoLoadAsked;

            audioInputLevelMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputLevelMonitor.Threshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;
            audioInputLevelMonitor.ThresholdPassed += (s, e) => TriggerCapture();
            audioInputLevelMonitor.DeviceLost += (s, e) => AudioDeviceLost();

            InitializeVideoFilters();
            InitializeGuideWatcher();

            NotificationCenter.StopPlayback += (s, e) => DoStopPlaying();
            NotificationCenter.PreferencesOpened += NotificationCenter_PreferencesOpened;
            NotificationCenter.ExternalCommand += NotificationCenter_ExternalCommand;

            playerScreens = screenList.Where(s => s is PlayerScreen).Select(s => s as PlayerScreen);
            captureScreens = screenList.Where(s => s is CaptureScreen).Select(s => s as CaptureScreen);
        }

        private void InitializeVideoFilters()
        {
            filterMenus.Add(CreateFilterMenu(VideoFilterType.Kinogram));
        }

        private ToolStripMenuItem CreateFilterMenu(VideoFilterType type)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(VideoFilterFactory.GetFriendlyName(type), VideoFilterFactory.GetIcon(type));
            menu.MergeAction = MergeAction.Append;
            menu.Tag = type;
            menu.Click += (s, e) =>
            {
                PlayerScreen screen = activeScreen as PlayerScreen;
                if(screen == null)
                    return;

                VideoFilterType filterType = (VideoFilterType)((ToolStripMenuItem)s).Tag;
                if (filterType == screen.ActiveVideoFilterType)
                    screen.DeactivateVideoFilter();
                else
                    screen.ActivateVideoFilter(filterType);

                OrganizeMenus();
            };

            return menu;
        }

        private void InitializeGuideWatcher()
        {
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
        }

        public void RecoverCrash()
        {
            // Import recovered screens into launch settings.
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

        public void LoadDefaultWorkspace()
        {
            if (LaunchSettingsManager.ScreenDescriptions.Count > 0)
                return;

            Workspace workspace = PreferencesManager.GeneralPreferences.Workspace;
            if (workspace.Screens == null || workspace.Screens.Count == 0)
                return;

            foreach (var sd in workspace.Screens)
                LaunchSettingsManager.AddScreenDescription(sd);
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

            // Load Analysis
            mnuLoadAnalysis.Image = Properties.Resources.file_kva2;
            mnuLoadAnalysis.Click += mnuLoadAnalysisOnClick;
            mnuLoadAnalysis.MergeIndex = 2;
            mnuLoadAnalysis.MergeAction = MergeAction.Insert;

            //----

            mnuSave.Image = Properties.Resources.filesave;
            mnuSave.Click += new EventHandler(mnuSaveOnClick);
            mnuSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuSave.MergeIndex = 5;
            mnuSave.MergeAction = MergeAction.Insert;

            mnuSaveAs.Image = Properties.Resources.filesave;
            mnuSaveAs.Click += new EventHandler(mnuSaveAsOnClick);
            mnuSaveAs.MergeIndex = 6;
            mnuSaveAs.MergeAction = MergeAction.Insert;

            mnuExportVideo.Image = Properties.Resources.film_save;
            mnuExportVideo.Click += new EventHandler(mnuExportVideoOnClick);
            mnuExportVideo.MergeIndex = 7;
            mnuExportVideo.MergeAction = MergeAction.Insert;

            mnuExportSpreadsheet.Image = Properties.Resources.table;
            mnuExportSpreadsheet.MergeIndex = 8;
            mnuExportSpreadsheet.MergeAction = MergeAction.Insert;
            mnuExportODS.Image = Properties.Resources.file_ods;
            mnuExportODS.Click += new EventHandler(mnuExportODS_OnClick);
            mnuExportXLSX.Image = Properties.Resources.file_xls;
            mnuExportXLSX.Click += new EventHandler(mnuExportXLSX_OnClick);
            mnuExportJSON.Image = Properties.Resources.table;
            mnuExportJSON.Click += new EventHandler(mnuExportJSON_OnClick);
            mnuExportCSV.Image = Properties.Resources.table;
            mnuExportCSV.Click += new EventHandler(mnuExportCSV_OnClick);
            mnuExportSpreadsheet.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuExportODS, 
                mnuExportXLSX, 
                mnuExportJSON,
                mnuExportCSV,
            });

            //------------------------

            mnuCloseFile.Image = Properties.Resources.film_close3;
            mnuCloseFile.Enabled = false;
            mnuCloseFile.Click += new EventHandler(mnuCloseFileOnClick);
            mnuCloseFile.MergeIndex = 10;
            mnuCloseFile.MergeAction = MergeAction.Insert;

            mnuCloseFile2.Image = Properties.Resources.film_close3;
            mnuCloseFile2.Enabled = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Click += new EventHandler(mnuCloseFile2OnClick);
            mnuCloseFile2.MergeIndex = 11;
            mnuCloseFile2.MergeAction = MergeAction.Insert;

            //--------------------

            ToolStripItem[] subFile = new ToolStripItem[] {
                // Open file
                // Open replay observer,
                // Recent,
                mnuLoadAnalysis,
                // ----
                mnuSave,
                mnuSaveAs,
                mnuExportVideo,
                mnuExportSpreadsheet,
                //----
                mnuCloseFile,
                mnuCloseFile2,
                //----
                // Quit.
                };

            mnuCatchFile.DropDownItems.AddRange(subFile);
            #endregion

            #region Edit
            ToolStripMenuItem mnuCatchEdit = new ToolStripMenuItem();
            mnuCatchEdit.MergeIndex = 1; // (Edit)
            mnuCatchEdit.MergeAction = MergeAction.MatchOnly;

            mnuCutDrawing.Image = Properties.Drawings.cut;
            mnuCutDrawing.Click += new EventHandler(mnuCutDrawing_OnClick);
            mnuCutDrawing.MergeAction = MergeAction.Append;
            mnuCopyDrawing.Image = Properties.Drawings.copy;
            mnuCopyDrawing.Click += new EventHandler(mnuCopyDrawing_OnClick);
            mnuCopyDrawing.MergeAction = MergeAction.Append;
            mnuPasteDrawing.Image = Properties.Drawings.paste;
            mnuPasteDrawing.Click += new EventHandler(mnuPasteDrawing_OnClick);
            mnuPasteDrawing.MergeAction = MergeAction.Append;
            
            ToolStripItem[] subEdit = new ToolStripItem[] { new ToolStripSeparator(), mnuCutDrawing, mnuCopyDrawing, mnuPasteDrawing };
            mnuCatchEdit.DropDownItems.AddRange(subEdit);
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

            mnuDemosaicNone.Click += mnuDemosaicNone_Click;
            mnuDemosaicRGGB.Click += mnuDemosaicRGGB_Click;
            mnuDemosaicBGGR.Click += mnuDemosaicBGGR_Click;
            mnuDemosaicGRBG.Click += mnuDemosaicGRBG_Click;
            mnuDemosaicGBRG.Click += mnuDemosaicGBRG_Click;
            mnuDemosaicRGGB.Image = Properties.Resources.rggb;
            mnuDemosaicBGGR.Image = Properties.Resources.bggr;
            mnuDemosaicGRBG.Image = Properties.Resources.grbg;
            mnuDemosaicGBRG.Image = Properties.Resources.gbrg;
            mnuDemosaic.Image = Properties.Resources.rggb;
            mnuDemosaic.MergeAction = MergeAction.Append;
            mnuDemosaic.DropDownItems.AddRange(new ToolStripItem[] { mnuDemosaicNone, new ToolStripSeparator(), mnuDemosaicRGGB, mnuDemosaicBGGR, mnuDemosaicGRBG, mnuDemosaicGBRG });
            
            mnuFormatAuto.Checked = true;
            mnuFormatAuto.Click += mnuFormatAutoOnClick;
            mnuFormatAuto.MergeAction = MergeAction.Append;
            mnuFormatForce43.Image = Properties.Resources.format43;
            mnuFormatForce43.Click += mnuFormatForce43OnClick;
            mnuFormatForce43.MergeAction = MergeAction.Append;
            mnuFormatForce169.Image = Properties.Resources.format169;
            mnuFormatForce169.Click += mnuFormatForce169OnClick;
            mnuFormatForce169.MergeAction = MergeAction.Append;
            mnuFormat.Image = Properties.Resources.shape_formats;
            mnuFormat.MergeAction = MergeAction.Append;
            mnuFormat.DropDownItems.AddRange(new ToolStripItem[] { mnuFormatAuto, new ToolStripSeparator(), mnuFormatForce43, mnuFormatForce169});

            mnuRotation0.Click += mnuRotation0_Click;
            mnuRotation90.Image = Properties.Resources.rotate90;
            mnuRotation90.Click += mnuRotation90_Click;
            mnuRotation180.Image = Properties.Resources.rotate180;
            mnuRotation180.Click += mnuRotation180_Click;
            mnuRotation270.Image = Properties.Resources.rotate270;
            mnuRotation270.Click += mnuRotation270_Click;
            mnuRotation.Image = Properties.Resources.imagerotate;
            mnuRotation.MergeAction = MergeAction.Append;
            mnuRotation.DropDownItems.AddRange(new ToolStripItem[] { mnuRotation0, mnuRotation90, mnuRotation270, mnuRotation180 });

            mnuMirror.Image = Properties.Resources.shape_mirror;
            mnuMirror.Checked = false;
            mnuMirror.ShortcutKeys = Keys.Control | Keys.M;
            mnuMirror.Click += new EventHandler(mnuMirrorOnClick);
            mnuMirror.MergeAction = MergeAction.Append;

            ConfigureVideoFilterMenus(null);

            mnuCatchImage.DropDownItems.Add(mnuFormat);
            mnuCatchImage.DropDownItems.Add(mnuRotation);
            mnuCatchImage.DropDownItems.Add(mnuMirror);
            mnuCatchImage.DropDownItems.Add(mnuDemosaic);
            mnuCatchImage.DropDownItems.Add(mnuDeinterlace);
            //mnuCatchImage.DropDownItems.Add(new ToolStripSeparator());
            
            // Temporary hack for including filters sub menus until a full plugin system is in place.
            // We just check on their type. Ultimately each plugin will have a category or a submenu property.
            //foreach(ToolStripMenuItem m in filterMenus)
            //{
            //    if (m.Tag is AdjustmentFilter)
            //        mnuCatchImage.DropDownItems.Add(m);
            //}
            
            #endregion

            #region Video
            ToolStripMenuItem mnuCatchVideo = new ToolStripMenuItem();
            mnuCatchVideo.MergeIndex = 4;
            mnuCatchVideo.MergeAction = MergeAction.MatchOnly;

            mnuTimebase.Image = Properties.Resources.camera_speed;
            mnuTimebase.Click += new EventHandler(mnuTimebase_OnClick);
            mnuTimebase.MergeAction = MergeAction.Append;
            
            mnuCatchVideo.DropDownItems.Add(mnuTimebase);
            mnuCatchVideo.DropDownItems.Add(new ToolStripSeparator());
            foreach(ToolStripMenuItem m in filterMenus)
            {
                mnuCatchVideo.DropDownItems.Add(m);
            }
            #endregion

            #region Tools
            ToolStripMenuItem mnuCatchTools = new ToolStripMenuItem();
            mnuCatchTools.MergeIndex = 5;
            mnuCatchTools.MergeAction = MergeAction.MatchOnly;

            BuildSvgMenu();

            mnuTestGrid.Image = Properties.Resources.grid2;
            mnuTestGrid.Click += mnuTestGrid_OnClick;
            mnuTestGrid.MergeAction = MergeAction.Append;

            mnuCoordinateAxis.Image = Properties.Resources.coordinate_axis;
            mnuCoordinateAxis.Click += mnuCoordinateAxis_OnClick;
            mnuCoordinateAxis.MergeAction = MergeAction.Append;

            mnuCameraCalibration.Image = Properties.Resources.checkerboard;
            mnuCameraCalibration.Click += mnuCameraCalibration_OnClick;
            mnuCameraCalibration.MergeAction = MergeAction.Append;

            mnuTrajectoryAnalysis.Image = Properties.Resources.function;
            mnuTrajectoryAnalysis.Click += mnuTrajectoryAnalysis_OnClick;
            mnuTrajectoryAnalysis.MergeAction = MergeAction.Append;

            mnuScatterDiagram.Image = Properties.Resources.function;
            mnuScatterDiagram.Click += mnuScatterDiagram_OnClick;
            mnuScatterDiagram.MergeAction = MergeAction.Append;

            mnuAngularAnalysis.Image = Properties.Resources.function;
            mnuAngularAnalysis.Click += mnuAngularAnalysis_OnClick;
            mnuAngularAnalysis.MergeAction = MergeAction.Append;

            mnuAngleAngleAnalysis.Image = Properties.Resources.function;
            mnuAngleAngleAnalysis.Click += mnuAngleAngleAnalysis_OnClick;
            mnuAngleAngleAnalysis.MergeAction = MergeAction.Append;

            mnuCatchTools.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuSVGTools, 
                mnuTestGrid, 
                mnuCoordinateAxis, 
                mnuCameraCalibration, 
                new ToolStripSeparator(),
                mnuScatterDiagram,
                mnuTrajectoryAnalysis,
                mnuAngularAnalysis,
                mnuAngleAngleAnalysis
            });

            #endregion

            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { mnuCatchFile, mnuCatchEdit, mnuCatchScreens, mnuCatchImage, mnuCatchVideo, mnuCatchTools });
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

        /// <summary>
        /// Close the screen manager and its components.
        /// Returns true if the closing was cancelled. This happens when there are unsaved changes and the user cancelled.
        /// </summary>
        public bool CloseSubModules()
        {
            view.Closing = true;
            for(int i = screenList.Count - 1; i >= 0; i--)
            {
                screenList[i].BeforeClose();
                CloseFile(i);
                AfterSharedBufferChange();
            }

            bool cancelled = screenList.Count > 0;
            if (cancelled)
                view.Closing = false;

            return cancelled;
        }
        public void PreferencesUpdated()
        {
            foreach (AbstractScreen screen in screenList)
                screen.PreferencesUpdated();

            audioInputLevelMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputLevelMonitor.Threshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;

            // We may have changed the preferred audio input device.
            if (captureScreens.Count() > 0 && audioInputLevelMonitor.Enabled)
            {
                string id = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
                audioInputLevelMonitor.Start(id);
            }

            RefreshUICulture();
        }
        #endregion
        
        #region Event handlers for screens
        private void Screen_CloseAsked(object sender, EventArgs e)
        {
            AbstractScreen screen = sender as AbstractScreen;
            if (screen == null)
                return;

            // Reorganise screens.
            // We leverage the fact that screens are always well ordered relative to menus.
            if (screenList.Count > 0 && screen == screenList[0])
                CloseFile(0);
            else
                CloseFile(1);

            AfterSharedBufferChange();
        }
        private void Screen_Activated(object sender, EventArgs e)
        {
            AbstractScreen screen = sender as AbstractScreen;
            SetActiveScreen(screen);
        }
        private void Screen_DualCommandReceived(object sender, EventArgs<HotkeyCommand> e)
        {
            // A screen has received a hotkey that must be handled at manager level.
            if (dualPlayer.Active)
                dualPlayer.ExecuteDualCommand(e.Value);
            else if (dualCapture.Active)
                dualCapture.ExecuteDualCommand(e.Value);
        }

        private void Player_OpenVideoAsked(object sender, EventArgs e)
        {
            string filename = FilePicker.OpenVideo();
            if (string.IsNullOrEmpty(filename))
                return;

            int index = sender == screenList[0] ? 0 : 1;
            VideoTypeManager.LoadVideo(filename, index);
        }
        private void Player_OpenReplayWatcherAsked(object sender, EventArgs e)
        {
            string path = FilePicker.OpenReplayWatcher();
            if (string.IsNullOrEmpty(path))
                return;

            int index = sender == screenList[0] ? 0 : 1;

            ScreenDescriptionPlayback screenDescription = new ScreenDescriptionPlayback();
            screenDescription.FullPath = path;
            screenDescription.IsReplayWatcher = true;
            screenDescription.Autoplay = true;
            screenDescription.Stretch = true;
            screenDescription.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
            LoaderVideo.LoadVideoInScreen(this, path, index, screenDescription);
        }
        private void Player_OpenAnnotationsAsked(object sender, EventArgs e)
        {
            int index = sender == screenList[0] ? 0 : 1;
            LoadAnalysis(index);
        }
        private void Player_SelectionChanged(object sender, EventArgs<bool> e)
        {
            ResetSync();

            dualLaunchSettingsPendingCountdown--;

            if (dualLaunchSettingsPendingCountdown == 0)
                dualPlayer.CommitLaunchSettings();

            OrganizeMenus();
        }
        private void Player_KVAImported(object sender, EventArgs e)
        {
            OrganizeMenus();
        }

        private void Player_FilterExited(object sender, EventArgs e)
        {
            OrganizeMenus();
        }

        private void Player_ResetAsked(object sender, EventArgs e)
        {
            // A screen was reset. (ex: a video was reloded in place).
            // We need to also reset all the sync states.
            ResetSync();
        }
        private void Capture_CameraDiscoveryComplete(object sender, EventArgs<string> e)
        {
            // A capture screen has just completed its camera discovery,
            // either by finding and loading the camera or by timeout.
            // Tick off that camera from the list and stop the whole discovery process if we are done.
            if (camerasToDiscover.Contains(e.Value))
                camerasToDiscover.Remove(e.Value);

            if (camerasToDiscover.Count == 0)
                CameraTypeManager.StopDiscoveringCameras();
        }
        #endregion

        #region Common controls event handlers
        private void AddCommonControlsEventHandlers()
        {
            dualPlayer.View.SwapAsked += CCtrl_SwapAsked;
            dualCapture.View.SwapAsked += CCtrl_SwapAsked;
        }
        private void CCtrl_SwapAsked(object sender, EventArgs e)
        {
            mnuSwapScreensOnClick(null, EventArgs.Empty);	
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
            else
                activeScreen = null;

            foreach (PlayerScreen p in playerScreens)
                p.Synched = false;
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

            for (int i = 0; i < screenList.Count; i++)
                screenList[i].Identify(i);

            if (captureScreens.Count() > 0 && audioInputLevelMonitor.Enabled)
            {
                string id = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
                audioInputLevelMonitor.Start(id);
            }
            else
            {
                audioInputLevelMonitor.Stop();
            }
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
        public void AfterSharedBufferChange()
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
        public static void AlertDirectoryNotCreated()
        {
            string msgTitle = ScreenManagerLang.Error_Capture_DirectoryNotCreated_Title;
            string msgText = ScreenManagerLang.Error_Capture_DirectoryNotCreated_Text.Replace("\\n", "\n");

            MessageBox.Show(msgText, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        
        public Workspace ExtractWorkspace()
        {
            Workspace workspace = new Workspace();
            foreach (var screen in screenList)
                workspace.Screens.Add(screen.GetScreenDescription());

            return workspace;
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
            
            #region Menus depending only on the state of the active screen
            bool activeScreenIsEmpty = false;
            if (activeScreen != null && screenList.Count > 0)
            {
                if(!activeScreen.Full)
                {
                    activeScreenIsEmpty = true;
                }
                else if (activeScreen is PlayerScreen)
                {
                    PlayerScreen player = activeScreen as PlayerScreen;
                    
                    // 1. Video is loaded : save-able and analysis is loadable.
                    
                    // File
                    mnuSave.Enabled = true;
                    mnuSaveAs.Enabled = true;
                    mnuExportVideo.Enabled = true;
                    toolSave.Enabled = true;
                    mnuExportSpreadsheet.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportODS.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportXLSX.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportJSON.Enabled = player.FrameServer.Metadata.HasData;
                    mnuExportCSV.Enabled = player.FrameServer.Metadata.HasData;
                    mnuLoadAnalysis.Enabled = true;
                    
                    // Edit
                    HistoryMenuManager.SwitchContext(player.HistoryStack);
                    ConfigureClipboardMenus(player);

                    // Image
                    mnuDeinterlace.Enabled = player.FrameServer.VideoReader.CanChangeDeinterlacing;
                    mnuMirror.Enabled = true;
                    mnuDeinterlace.Checked = player.Deinterlaced;
                    mnuMirror.Checked = player.Mirrored;
                    if (!player.IsSingleFrame)
                    {
                        ConfigureImageFormatMenus(player);
                        ConfigureImageRotationMenus(player);
                        ConfigureImageDemosaicingMenus(player);
                    }
                    else
                    {
                        ConfigureImageFormatMenus(null);
                        ConfigureImageRotationMenus(null);
                        ConfigureImageDemosaicingMenus(null);
                    }

                    // Video
                    mnuTimebase.Enabled = true;
                    ConfigureVideoFilterMenus(player);

                    // Tools
                    mnuSVGTools.Enabled = hasSvgFiles;
                    mnuTestGrid.Enabled = false;
                    mnuCoordinateAxis.Enabled = true;
                    mnuCoordinateAxis.Checked = player.FrameServer.Metadata.DrawingCoordinateSystem.Visible;
                    mnuCameraCalibration.Enabled = true;
                    mnuTrajectoryAnalysis.Enabled = true;
                    mnuScatterDiagram.Enabled = true;
                    mnuAngularAnalysis.Enabled = true;
                    mnuAngleAngleAnalysis.Enabled = true;
                    
                }
                else if(activeScreen is CaptureScreen)
                {
                    CaptureScreen captureScreen = activeScreen as CaptureScreen;   
                    
                    // File
                    mnuSave.Enabled = false;
                    mnuSaveAs.Enabled = false;
                    mnuExportVideo.Enabled = false;
                    toolSave.Enabled = false;
                    mnuExportSpreadsheet.Enabled = false;
                    mnuExportODS.Enabled = false;
                    mnuExportXLSX.Enabled = false;
                    mnuExportJSON.Enabled = false;
                    mnuExportCSV.Enabled = false;
                    mnuLoadAnalysis.Enabled = true;

                    // Edit
                    HistoryMenuManager.SwitchContext(captureScreen.HistoryStack);
                    ConfigureClipboardMenus(activeScreen);

                    // Image
                    mnuDeinterlace.Enabled = false;
                    mnuMirror.Enabled = true;
                    mnuDeinterlace.Checked = false;
                    mnuMirror.Checked = captureScreen.Mirrored;
                    ConfigureImageFormatMenus(captureScreen);
                    ConfigureImageRotationMenus(captureScreen);
                    ConfigureImageDemosaicingMenus(captureScreen);

                    // Video
                    mnuTimebase.Enabled = false;
                    ConfigureVideoFilterMenus(null);

                    // Tools
                    mnuSVGTools.Enabled = false;
                    mnuTestGrid.Enabled = true;
                    mnuTestGrid.Checked = captureScreen.TestGridVisible;
                    mnuCoordinateAxis.Enabled = false;
                    mnuCameraCalibration.Enabled = false;
                    mnuTrajectoryAnalysis.Enabled = false;
                    mnuScatterDiagram.Enabled = false;
                    mnuAngularAnalysis.Enabled = false;
                    mnuAngleAngleAnalysis.Enabled = false;
                }
                else
                {
                    // KO ?
                    activeScreenIsEmpty = true;
                }
            }
            else
            {
                // No active screen. ( = no screens)
                activeScreenIsEmpty = true;
            }

            if (activeScreenIsEmpty)
            {
                // File
                mnuSave.Enabled = false;
                mnuSaveAs.Enabled = false;
                mnuExportVideo.Enabled = false;
                toolSave.Enabled = false;
                mnuLoadAnalysis.Enabled = false;
                mnuExportSpreadsheet.Enabled = false;
                mnuExportODS.Enabled = false;
                mnuExportXLSX.Enabled = false;
                mnuExportJSON.Enabled = false;
                mnuExportCSV.Enabled = false;

                // Edit
                HistoryMenuManager.SwitchContext(null);
                ConfigureClipboardMenus(null);

                // Image
                mnuDeinterlace.Enabled = false;
                mnuMirror.Enabled = false;
                mnuDeinterlace.Checked = false;
                mnuMirror.Checked = false;
                ConfigureImageFormatMenus(null);
                ConfigureImageRotationMenus(null);
                ConfigureImageDemosaicingMenus(null);
                
                // Video
                mnuTimebase.Enabled = false;
                ConfigureVideoFilterMenus(null);

                // Tools
                mnuSVGTools.Enabled = false;
                mnuTestGrid.Enabled = false;
                mnuTestGrid.Checked = false;
                mnuCoordinateAxis.Enabled = false;
                mnuCoordinateAxis.Checked = false;
                mnuCameraCalibration.Enabled = false;
                mnuTrajectoryAnalysis.Enabled = false;
                mnuScatterDiagram.Enabled = false;
                mnuAngularAnalysis.Enabled = false;
                mnuAngleAngleAnalysis.Enabled = false;
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
                            // Si l'馗ran de gauche 騁ait 馮alement vide, bEmpty reste ・true.
                            // Si l'馗ran de gauche 騁ait plein, bEmpty reste ・false.
                        }
                    }
                    else if (screenList[1] is CaptureScreen)
                    {
                        // Ecran de droite en capture.
                        // Si l'馗ran de gauche 騁ait 馮alement vide, bEmpty reste ・true.
                        // Si l'馗ran de gauche 騁ait plein, bEmpty reste ・false.
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
                VideoFilterType filterType = (VideoFilterType)menu.Tag;
                menu.Visible = VideoFilterFactory.GetExperimental(filterType) ? Software.Experimental : true;
                menu.Enabled = hasVideo && player.IsCaching;
                menu.Checked = hasVideo && player.ActiveVideoFilterType == filterType;
            }
        }
        private void ConfigureImageFormatMenus(AbstractScreen screen)
        {
            // Set the enable and check prop of the image formats menu according of current screen state.
            bool canChangeAspectRatio = screen != null && screen.Full && screen is PlayerScreen && ((PlayerScreen)screen).FrameServer.VideoReader.CanChangeAspectRatio;
            mnuFormat.Enabled = canChangeAspectRatio;
            mnuFormatAuto.Enabled = canChangeAspectRatio;
            mnuFormatForce43.Enabled = canChangeAspectRatio;
            mnuFormatForce169.Enabled = canChangeAspectRatio;

            if (!canChangeAspectRatio)
                return;

            mnuFormatAuto.Checked = screen.AspectRatio == ImageAspectRatio.Auto;
            mnuFormatForce43.Checked = screen.AspectRatio == ImageAspectRatio.Force43;
            mnuFormatForce169.Checked = screen.AspectRatio == ImageAspectRatio.Force169;
        }
        private void ConfigureImageDemosaicingMenus(AbstractScreen screen)
        {
            bool canChangeDemosaicing = screen != null && screen.Full && screen is PlayerScreen && ((PlayerScreen)screen).FrameServer.VideoReader.CanChangeDemosaicing;
            mnuDemosaic.Enabled = canChangeDemosaicing;
            mnuDemosaicNone.Enabled = canChangeDemosaicing;
            mnuDemosaicRGGB.Enabled = canChangeDemosaicing;
            mnuDemosaicBGGR.Enabled = canChangeDemosaicing;
            mnuDemosaicGRBG.Enabled = canChangeDemosaicing;
            mnuDemosaicGBRG.Enabled = canChangeDemosaicing;

            if (!canChangeDemosaicing)
                return;
            
            mnuDemosaicNone.Checked = screen.Demosaicing == Demosaicing.None;
            mnuDemosaicRGGB.Checked = screen.Demosaicing == Demosaicing.RGGB;
            mnuDemosaicBGGR.Checked = screen.Demosaicing == Demosaicing.BGGR;
            mnuDemosaicGRBG.Checked = screen.Demosaicing == Demosaicing.GRBG;
            mnuDemosaicGBRG.Checked = screen.Demosaicing == Demosaicing.GBRG;
        }
        private void ConfigureImageRotationMenus(AbstractScreen screen)
        {
            bool screenIsFull = screen != null && screen.Full;
            bool playerCanChangeRotation = screenIsFull && screen is PlayerScreen && ((PlayerScreen)screen).FrameServer.VideoReader.CanChangeImageRotation;
            bool canChangeImageRotation = screenIsFull && (screen is CaptureScreen || playerCanChangeRotation);
            mnuRotation.Enabled = canChangeImageRotation;
            mnuRotation0.Enabled = canChangeImageRotation;
            mnuRotation90.Enabled = canChangeImageRotation;
            mnuRotation180.Enabled = canChangeImageRotation;
            mnuRotation270.Enabled = canChangeImageRotation;

            if (!canChangeImageRotation)
                return;

            mnuRotation0.Checked = screen.ImageRotation == ImageRotation.Rotate0;
            mnuRotation90.Checked = screen.ImageRotation == ImageRotation.Rotate90;
            mnuRotation180.Checked = screen.ImageRotation == ImageRotation.Rotate180;
            mnuRotation270.Checked = screen.ImageRotation == ImageRotation.Rotate270;
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
        private void ConfigureClipboardMenus(AbstractScreen screen)
        {
            if (screen is PlayerScreen)
            {
                PlayerScreen player = screen as PlayerScreen;
                bool canCutOrCopy = player.FrameServer.Metadata.HitDrawing != null && player.FrameServer.Metadata.HitDrawing.IsCopyPasteable;
                mnuCutDrawing.Enabled = canCutOrCopy;
                mnuCopyDrawing.Enabled = canCutOrCopy;
                if (!canCutOrCopy)
                {
                    mnuCutDrawing.Text = ScreenManagerLang.mnuCutDrawing;
                    mnuCopyDrawing.Text = ScreenManagerLang.mnuCopyDrawing;
                }
                else
                {
                    mnuCutDrawing.Text = string.Format("{0} ({1})", ScreenManagerLang.mnuCutDrawing, player.FrameServer.Metadata.HitDrawing.Name);
                    mnuCopyDrawing.Text = string.Format("{0} ({1})", ScreenManagerLang.mnuCopyDrawing, player.FrameServer.Metadata.HitDrawing.Name);
                }
                
                mnuPasteDrawing.Enabled = DrawingClipboard.HasContent;
                if (DrawingClipboard.HasContent)
                {
                    mnuPasteDrawing.Text = string.Format("{0} ({1})", ScreenManagerLang.mnuPasteDrawing, DrawingClipboard.Name);
                }
            }
            else
            {
                mnuCutDrawing.Enabled = false;
                mnuCopyDrawing.Enabled = false;
                mnuPasteDrawing.Enabled = false;
            }
        }
        #endregion

        #region Culture
        private void RefreshCultureToolbar()
        {
            toolSave.ToolTipText = ScreenManagerLang.Generic_SaveKVA;
            toolHome.ToolTipText = ScreenManagerLang.mnuHome;
            toolOnePlayer.ToolTipText = ScreenManagerLang.mnuOnePlayer;
            toolTwoPlayers.ToolTipText = ScreenManagerLang.mnuTwoPlayers;
            toolOneCapture.ToolTipText = ScreenManagerLang.mnuOneCapture;
            toolTwoCaptures.ToolTipText = ScreenManagerLang.mnuTwoCaptures;
            toolTwoMixed.ToolTipText = ScreenManagerLang.mnuTwoMixed;	
        }
        private void RefreshCultureMenu()
        {
            // File
            mnuCloseFile.Text = ScreenManagerLang.Generic_Close;
            mnuCloseFile2.Text = ScreenManagerLang.Generic_Close;
            mnuSave.Text = ScreenManagerLang.Generic_SaveKVA;
            mnuSaveAs.Text = ScreenManagerLang.Generic_SaveKVAAs;
            mnuExportVideo.Text = ScreenManagerLang.Generic_ExportVideo;
            //mnuExportSpreadsheet.Text = ScreenManagerLang.mnuExportSpreadsheet;
            mnuExportSpreadsheet.Text = "Export data";
            mnuExportODS.Text = "LibreOffice Calc (.ods)";
            mnuExportXLSX.Text = "Microsoft Excel (.xlsx)";
            mnuExportJSON.Text = "Raw JSON (.json)";
            mnuExportCSV.Text = "Raw CSV (.csv)";
            mnuLoadAnalysis.Text = ScreenManagerLang.mnuLoadAnalysis;

            // Edit
            mnuCutDrawing.Text = ScreenManagerLang.mnuCutDrawing;
            mnuCopyDrawing.Text = ScreenManagerLang.mnuCopyDrawing;
            mnuPasteDrawing.Text = ScreenManagerLang.mnuPasteDrawing;
            mnuCutDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CutDrawing);
            mnuCopyDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CopyDrawing);
            mnuPasteDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.PasteDrawing);
            
            // View
            mnuOnePlayer.Text = ScreenManagerLang.mnuOnePlayer;
            mnuTwoPlayers.Text = ScreenManagerLang.mnuTwoPlayers;
            mnuOneCapture.Text = ScreenManagerLang.mnuOneCapture;
            mnuTwoCaptures.Text = ScreenManagerLang.mnuTwoCaptures;
            mnuTwoMixed.Text = ScreenManagerLang.mnuTwoMixed;
            mnuSwapScreens.Text = ScreenManagerLang.mnuSwapScreens;
            mnuToggleCommonCtrls.Text = ScreenManagerLang.mnuToggleCommonCtrls;
            
            // Image
            mnuDeinterlace.Text = ScreenManagerLang.mnuDeinterlace;
            mnuFormatAuto.Text = ScreenManagerLang.mnuFormatAuto;
            mnuFormatForce43.Text = ScreenManagerLang.mnuFormatForce43;
            mnuFormatForce169.Text = ScreenManagerLang.mnuFormatForce169;
            mnuFormat.Text = ScreenManagerLang.mnuFormat;
            mnuDemosaicNone.Text = "None";
            mnuDemosaicRGGB.Text = "RGGB";
            mnuDemosaicBGGR.Text = "BGGR";
            mnuDemosaicGRBG.Text = "GRBG";
            mnuDemosaicGBRG.Text = "GBRG";
            mnuDemosaic.Text = "Demosaicing";
            mnuRotation0.Text = ScreenManagerLang.mnuRotation0;
            mnuRotation90.Text = ScreenManagerLang.mnuRotation90;
            mnuRotation180.Text = ScreenManagerLang.mnuRotation180;
            mnuRotation270.Text = ScreenManagerLang.mnuRotation270;
            mnuRotation.Text = ScreenManagerLang.mnuRotation;
            mnuMirror.Text = ScreenManagerLang.mnuMirror;
            RefreshCultureMenuFilters();

            // Video
            mnuTimebase.Text = ScreenManagerLang.mnuTimebase;

            // Tools
            mnuSVGTools.Text = ScreenManagerLang.mnuSVGTools;
            mnuImportImage.Text = ScreenManagerLang.mnuImportImage;
            mnuTestGrid.Text = ScreenManagerLang.DrawingName_TestGrid;
            mnuCoordinateAxis.Text = ScreenManagerLang.mnuCoordinateSystem;
            mnuCameraCalibration.Text = ScreenManagerLang.dlgCameraCalibration_Title + "...";
            mnuScatterDiagram.Text = ScreenManagerLang.DataAnalysis_ScatterDiagram + "...";
            mnuTrajectoryAnalysis.Text = ScreenManagerLang.DataAnalysis_LinearKinematics + "...";
            mnuAngularAnalysis.Text = ScreenManagerLang.DataAnalysis_AngularKinematics + "...";
            mnuAngleAngleAnalysis.Text = ScreenManagerLang.DataAnalysis_AngleAngleDiagrams + "...";
        }
            
        private void RefreshCultureMenuFilters()
        {
            foreach(ToolStripMenuItem menu in filterMenus)
            {
                VideoFilterType filter = (VideoFilterType)menu.Tag;
                menu.Text = VideoFilterFactory.GetFriendlyName(filter);
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
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;

            DoStopPlaying();
            player.Save();
        }

        private void mnuSaveAsOnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;

            DoStopPlaying();
            player.SaveAs();
        }

        private void mnuExportVideoOnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;

            DoStopPlaying();
            player.ExportVideo();
        }

        private void mnuLoadAnalysisOnClick(object sender, EventArgs e)
        {
            if (activeScreen != null)
            {
                int index = activeScreen == screenList[0] ? 0 : 1;
                LoadAnalysis(index);
            }
        }
        private void LoadAnalysis(int targetScreen)
        {
            if (screenList[targetScreen] == null)
                return;

            if (screenList[targetScreen] is PlayerScreen)
                DoStopPlaying();
             
            string filename = FilePicker.OpenAnnotations();
            if (filename == null)
                return;

            screenList[targetScreen].LoadKVA(filename);
        }
        private void mnuExportODS_OnClick(object sender, EventArgs e)
        {
            ExportSpreadsheet(MetadataExportFormat.ODS);
        }
        private void mnuExportXLSX_OnClick(object sender, EventArgs e)
        {
            ExportSpreadsheet(MetadataExportFormat.XLSX);
        }
        private void mnuExportJSON_OnClick(object sender, EventArgs e)
        {
            ExportSpreadsheet(MetadataExportFormat.JSON);
        }
        private void mnuExportCSV_OnClick(object sender, EventArgs e)
        {
            ExportSpreadsheet(MetadataExportFormat.CSV);
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
            saveFileDialog.Filter = "LibreOffice calc (*.ods)|*.ods|Microsoft Excel (*.xlsx)|*.xlsx|JSON (*.json)|*.json|CSV (*.csv)|*.csv";
            int filterIndex;
            switch (format)
            {
                case MetadataExportFormat.ODS:
                    filterIndex = 1;
                    break;
                case MetadataExportFormat.XLSX:
                    filterIndex = 2;
                    break;
                case MetadataExportFormat.CSV:
                    filterIndex = 4;
                    break;
                case MetadataExportFormat.JSON:
                default:
                    filterIndex = 3;
                    break;
            }

            saveFileDialog.FilterIndex = filterIndex;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(player.FrameServer.Metadata.VideoPath);

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            try
            { 
                MetadataExporter.Export(player.FrameServer.Metadata, saveFileDialog.FileName, format);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception encountered while exporting to spreadsheet.", e);
            }
        }
        #endregion

        #region Edit
        private void mnuCutDrawing_OnClick(object sender, EventArgs e)
        {
            if (activeScreen is PlayerScreen)
            {
                PlayerScreen player = activeScreen as PlayerScreen;
                player.ExecuteScreenCommand((int)PlayerScreenCommands.CutDrawing);
            }
            else if (activeScreen is CaptureScreen)
            {
                //CaptureScreen captureScreen = activeScreen as CaptureScreen;
                //captureScreen.ExecuteScreenCommand((int)CaptureScreenCommands.CutDrawing);
            }
        }
        private void mnuCopyDrawing_OnClick(object sender, EventArgs e)
        {
            if (activeScreen is PlayerScreen)
            {
                PlayerScreen player = activeScreen as PlayerScreen;
                player.ExecuteScreenCommand((int)PlayerScreenCommands.CopyDrawing);
            }
        }
        private void mnuPasteDrawing_OnClick(object sender, EventArgs e)
        {
            if (activeScreen is PlayerScreen)
            {
                PlayerScreen player = activeScreen as PlayerScreen;
                player.ExecuteScreenCommand((int)PlayerScreenCommands.PasteInPlaceDrawing);
            }
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

            AfterSharedBufferChange();
            
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
            
            AfterSharedBufferChange();
            
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

            AfterSharedBufferChange();
            
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
            
            dualPlayer.SwapSync();
        }
        private void mnuToggleCommonCtrlsOnClick(object sender, EventArgs e)
        {
            view.ToggleCommonControls();

            // Reset synchronization. 
            // This will allow the shortcuts to only be routed to the active screen if the dual controls aren't visible.
            ResetSync();
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
        private void ChangeAspectRatio(ImageAspectRatio aspect)
        {
            if(activeScreen == null)
                return;
        
            if(activeScreen.AspectRatio != aspect)
                activeScreen.AspectRatio = aspect;
            
            mnuFormatForce43.Checked = aspect == ImageAspectRatio.Force43;
            mnuFormatForce169.Checked = aspect == ImageAspectRatio.Force169;
            mnuFormatAuto.Checked = aspect == ImageAspectRatio.Auto;
        }
        private void mnuDemosaicNone_Click(object sender, EventArgs e)
        {
            ChangeDemosaic(Demosaicing.None);
        }
        private void mnuDemosaicRGGB_Click(object sender, EventArgs e)
        {
            ChangeDemosaic(Demosaicing.RGGB);
        }
        private void mnuDemosaicBGGR_Click(object sender, EventArgs e)
        {
            ChangeDemosaic(Demosaicing.BGGR);
        }
        private void mnuDemosaicGRBG_Click(object sender, EventArgs e)
        {
            ChangeDemosaic(Demosaicing.GRBG);
        }
        private void mnuDemosaicGBRG_Click(object sender, EventArgs e)
        {
            ChangeDemosaic(Demosaicing.GBRG);
        }
        private void ChangeDemosaic(Demosaicing demosaic)
        {
            if (activeScreen == null)
                return;

            if (activeScreen.Demosaicing != demosaic)
                activeScreen.Demosaicing = demosaic;
            
            mnuDemosaicNone.Checked = activeScreen.Demosaicing == Demosaicing.None;
            mnuDemosaicRGGB.Checked = activeScreen.Demosaicing == Demosaicing.RGGB;
            mnuDemosaicBGGR.Checked = activeScreen.Demosaicing == Demosaicing.BGGR;
            mnuDemosaicGRBG.Checked = activeScreen.Demosaicing == Demosaicing.GRBG;
            mnuDemosaicGBRG.Checked = activeScreen.Demosaicing == Demosaicing.GBRG;
        }
        private void mnuRotation0_Click(object sender, EventArgs e)
        {
            ChangeImageRotation(ImageRotation.Rotate0);
        }
        private void mnuRotation90_Click(object sender, EventArgs e)
        {
            ChangeImageRotation(ImageRotation.Rotate90);
        }
        private void mnuRotation180_Click(object sender, EventArgs e)
        {
            ChangeImageRotation(ImageRotation.Rotate180);
        }
        private void mnuRotation270_Click(object sender, EventArgs e)
        {
            ChangeImageRotation(ImageRotation.Rotate270);
        }
        private void ChangeImageRotation(ImageRotation rot)
        {
            if (activeScreen == null)
                return;

            if (activeScreen.ImageRotation != rot)
                activeScreen.ImageRotation = rot;

            mnuRotation0.Checked = rot == ImageRotation.Rotate0;
            mnuRotation90.Checked = rot == ImageRotation.Rotate90;
            mnuRotation180.Checked = rot == ImageRotation.Rotate180;
            mnuRotation270.Checked = rot == ImageRotation.Rotate270;
        }
        private void mnuMirrorOnClick(object sender, EventArgs e)
        {
            if (activeScreen == null)
                return;

            mnuMirror.Checked = !mnuMirror.Checked;
            activeScreen.Mirrored = mnuMirror.Checked;
        }
        private void mnuImportImage_OnClick(object sender, EventArgs e)
        {
            if(activeScreen == null || !activeScreen.CapabilityDrawings)
                return;
            
            // Display file open dialog and launch the drawing.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgImportReference_Title;
            openFileDialog.Filter = FilesystemHelper.OpenImageFilter(ScreenManagerLang.FileFilter_AllSupported);
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

            mnuCoordinateAxis.Checked = !mnuCoordinateAxis.Checked;
            ps.FrameServer.Metadata.DrawingCoordinateSystem.Visible = mnuCoordinateAxis.Checked;
            ps.RefreshImage();
        }

        private void mnuTestGrid_OnClick(object sender, EventArgs e)
        {
            CaptureScreen cs = activeScreen as CaptureScreen;
            if (cs == null)
                return;

            mnuTestGrid.Checked = !mnuTestGrid.Checked;
            cs.TestGridVisible = mnuTestGrid.Checked;
        }

        private void mnuCameraCalibration_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowCameraCalibration();
        }

        private void mnuTrajectoryAnalysis_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowTrajectoryAnalysis();
        }
        private void mnuScatterDiagram_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowScatterDiagram();
        }
        private void mnuAngularAnalysis_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowAngularAnalysis();
        }
        private void mnuAngleAngleAnalysis_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowAngleAngleAnalysis();
        }
        #endregion

        #region Motion
        private void mnuTimebase_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps != null)
                ps.ConfigureTimebase();
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
            if (FilesystemHelper.IsReplayWatcher(path))
            {
                ScreenDescriptionPlayback screenDescription = new ScreenDescriptionPlayback();
                screenDescription.FullPath = path;
                screenDescription.IsReplayWatcher = true;
                screenDescription.Autoplay = true;
                screenDescription.Stretch = true;
                screenDescription.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
                LoaderVideo.LoadVideoInScreen(this, path, screenDescription);
            }
            else
            {
                if (!File.Exists(path))
                    return;

                if (MetadataSerializer.IsMetadataFile(path) && targetScreen >= 0)
                {
                    // Special case of loading a KVA file on top of a loaded video.
                    AbstractScreen screen = GetScreenAt(targetScreen);
                    if (screen == null || !screen.Full)
                        return;

                    screen.LoadKVA(path);
                    screen.RefreshImage();
                }
                else
                {
                    LoaderVideo.LoadVideoInScreen(this, path, targetScreen);
                }
            }
        }
        
        private void DoLoadCameraInScreen(CameraSummary summary, int targetScreen)
        {
            if(summary == null)
                return;

            LoaderCamera.LoadCameraInScreen(this, summary, targetScreen);
        }
        
        private void DoStopPlaying()
        {
            foreach (PlayerScreen player in playerScreens)
                player.StopPlaying();

            dualPlayer.Pause();
        }

        private void View_FileLoadAsked(object source, FileLoadAskedEventArgs e)
        {
            DoLoadMovieInScreen(e.Source, e.Target);
        }

        private void CameraTypeManager_CameraLoadAsked(object source, CameraLoadAskedEventArgs e)
        {
            DoLoadCameraInScreen(e.Source, e.Target);
        }

        private void View_AutoLaunchAsked(object source, EventArgs e)
        {
            int reloaded = 0;

            int count = LaunchSettingsManager.ScreenDescriptions.Count;
            if (count > 2)
                LaunchSettingsManager.ScreenDescriptions.RemoveRange(2, count - 2);

            // Start by collecting the list of cameras to be found. 
            // We will keep the camera discovery system active until we have found all of them or time out.
            camerasToDiscover.Clear();
            foreach (IScreenDescription screenDescription in LaunchSettingsManager.ScreenDescriptions)
            {
                if (screenDescription is ScreenDescriptionCapture)
                    camerasToDiscover.Add(((ScreenDescriptionCapture)screenDescription).CameraName);
            }

            if (camerasToDiscover.Count == 0)
                CameraTypeManager.StopDiscoveringCameras();

            foreach (IScreenDescription screenDescription in LaunchSettingsManager.ScreenDescriptions)
            {
                if (screenDescription is ScreenDescriptionCapture)
                {
                    AddCaptureScreen();
                    ScreenDescriptionCapture sdc = screenDescription as ScreenDescriptionCapture;
                    CameraSummary summary = new CameraSummary(sdc.CameraName);

                    int targetScreen = reloaded == 1 ? 1 : 0;
                    LoaderCamera.LoadCameraInScreen(this, summary, targetScreen, sdc);
                    reloaded++;
                }
                else if (screenDescription is ScreenDescriptionPlayback)
                {
                    AddPlayerScreen();
                    ScreenDescriptionPlayback sdp = screenDescription as ScreenDescriptionPlayback;
                    LoaderVideo.LoadVideoInScreen(this, sdp.FullPath, sdp);
                    reloaded++;
                }
            }

            dualLaunchSettingsPendingCountdown = reloaded;

            if (reloaded > 0)
            {
                OrganizeScreens();
                OrganizeCommonControls();
                OrganizeMenus();
            }
        }

        private void TriggerCapture()
        {
            foreach (CaptureScreen screen in captureScreens)
                screen.TriggerCapture();
        }

        private void AudioDeviceLost()
        {
            foreach (CaptureScreen screen in captureScreens)
                screen.AudioDeviceLost();
        }

        private void NotificationCenter_PreferencesOpened(object source, EventArgs e)
        {
            audioInputLevelMonitor.Enabled = false;
        }

        private void NotificationCenter_ExternalCommand(object source, ExternalCommandEventArgs e)
        {
            // Parses the payload of the external command string and send it to the correct handler.
            // The payload is in the form "<Handler>.<Command>", for example "CaptureScreen.ToggleRecording".

            string[] tokens = e.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
            {
                log.ErrorFormat("Malformed external command. \"{0\"}", e.Name);
                return;
            }

            switch (tokens[0])
            {
                case "CaptureScreen":
                    {
                        CaptureScreenCommands command;
                        bool parsed = Enum.TryParse(tokens[1], out command);
                        if (!parsed)
                        {
                            log.ErrorFormat("Unsupported capture screen command \"{0}\".", tokens[1]);
                            return;
                        }

                        foreach (CaptureScreen screen in captureScreens)
                            screen.ExecuteScreenCommand((int)command);

                        break;
                    }
                case "PlayerScreen":
                    {
                        PlayerScreenCommands command;
                        bool parsed = Enum.TryParse(tokens[1], out command);
                        if (!parsed)
                        {
                            log.ErrorFormat("Unsupported player screen command \"{0}\".", tokens[1]);
                            return;
                        }

                        foreach (PlayerScreen screen in playerScreens)
                            screen.ExecuteScreenCommand((int)command);

                        break;
                    }
                default:
                    log.ErrorFormat("Unsupported handler in external command: \"{0}\"", tokens[0]);
                    break;
            }
        }
        #endregion

        #region Screen organization
        /// <summary>
        /// Disable synchronization or reset it to the screens' time origins.
        /// This should be called any time the screen list change, working zones change, dual controls visiblity changes.
        /// </summary>
        private void ResetSync()
        {
            foreach (PlayerScreen p in playerScreens)
                p.Synched = false;

            if (view.CommonControlsVisible)
                dualPlayer.ResetSync();
        }
        public void AddPlayerScreen()
        {
            PlayerScreen screen = new PlayerScreen();
            screen.RefreshUICulture();
            AddScreen(screen);
        }
        public void AddCaptureScreen()
        {
            
            CaptureScreen screen = new CaptureScreen();
            if (screenList.Count > 0)
                screen.SetShared(true);

            screen.RefreshUICulture();
            AddScreen(screen);
        }

        /// <summary>
        /// Find the most appropriate screen to load into.
        /// Must be of the same type, and empty if possible.
        /// </summary>
        public int FindTargetScreen(Type type)
        {
            AbstractScreen screen0 = GetScreenAt(0);
            AbstractScreen screen1 = GetScreenAt(1);
            if (screen0 != null && !screen0.Full && screen0.GetType() == type)
                return 0;

            if (screen1 != null && !screen1.Full && screen1.GetType() == type)
                return 1;

            // If no empty screen was found, overload, but start on the right.
            if (screen1 != null && screen1.GetType() == type)
                return 1;

            if (screen0 != null && screen0.GetType() == type)
                return 0;

            // We do not replace capture screens with videos or vice-versa.
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
                // No: don't save.
                return true;
            }
            else if (save == DialogResult.Cancel)
            {
                // Cancel: stop loading the new video.
                return false;
            }
            else
            {
                // Yes: save, then load the new video.
                DoStopPlaying();
                player.Save();
                return true;
            }
        }
        private DialogResult ShowConfirmDirtyDialog()
        {
            string caption = ScreenManagerLang.InfoBox_MetadataIsDirty_Title;
            string text = ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n");
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }
        
        private void AddScreen(AbstractScreen screen)
        {
            // We are about to add a new screen, signal it to a potential existing capture screen for buffer memory management.
            foreach (CaptureScreen captureScreen in captureScreens)
                captureScreen.SetShared(true);

            AddScreenEventHandlers(screen);
            screenList.Add(screen);
        }
        private void AddScreenEventHandlers(AbstractScreen screen)
        {
            screen.CloseAsked += Screen_CloseAsked;
            screen.Activated += Screen_Activated;
            screen.DualCommandReceived += Screen_DualCommandReceived;

            if (screen is PlayerScreen)
                AddPlayerScreenEventHandlers(screen as PlayerScreen);
            else if (screen is CaptureScreen)
                AddCaptureScreenEventHandlers(screen as CaptureScreen);
        }
        private void AddPlayerScreenEventHandlers(PlayerScreen screen)
        {
            screen.OpenVideoAsked += Player_OpenVideoAsked;
            screen.OpenReplayWatcherAsked += Player_OpenReplayWatcherAsked;
            screen.OpenAnnotationsAsked += Player_OpenAnnotationsAsked;
            screen.SelectionChanged += Player_SelectionChanged;
            screen.KVAImported += Player_KVAImported;
            screen.FilterExited += Player_FilterExited;
            screen.ResetAsked += Player_ResetAsked;
        }
        private void AddCaptureScreenEventHandlers(CaptureScreen screen)
        {
            screen.CameraDiscoveryComplete += Capture_CameraDiscoveryComplete;
        }
        private void RemoveScreenEventHandlers(AbstractScreen screen)
        {
            screen.CloseAsked -= Screen_CloseAsked;
            screen.Activated -= Screen_Activated;
            screen.DualCommandReceived -= Screen_DualCommandReceived;

            if (screen is PlayerScreen)
                RemovePlayerScreenEventHandlers(screen as PlayerScreen);
            else if (screen is CaptureScreen)
                RemoveCaptureScreenEventHandlers(screen as CaptureScreen);

        }
        private void RemovePlayerScreenEventHandlers(PlayerScreen screen)
        {
            screen.OpenVideoAsked -= Player_OpenVideoAsked;
            screen.OpenReplayWatcherAsked -= Player_OpenReplayWatcherAsked;
            screen.OpenAnnotationsAsked -= Player_OpenAnnotationsAsked;
            screen.SelectionChanged -= Player_SelectionChanged;
            screen.KVAImported -= Player_KVAImported;
            screen.FilterExited -= Player_FilterExited;
            screen.ResetAsked -= Player_ResetAsked;
        }

        private void RemoveCaptureScreenEventHandlers(CaptureScreen screen)
        {
            screen.CameraDiscoveryComplete -= Capture_CameraDiscoveryComplete;
        }

        #endregion
    }
}
