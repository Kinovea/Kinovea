#region License
/*
Copyright © Joan Charmant 2008.
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
using Kinovea.ScreenManager;

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
        private List<string> camerasToDiscover = new List<string>();
        private bool autoLaunchInProgress;
        private AudioInputLevelMonitor audioInputLevelMonitor = new AudioInputLevelMonitor();
        private UDPMonitor udpMonitor = new UDPMonitor();

        #region Menus

        // File
        private ToolStripMenuItem mnuLoadAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSave = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveAs = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUnloadAnnotations = new ToolStripMenuItem();

        private ToolStripMenuItem mnuExportVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideoVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideoSlideshow = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideoWithPauses = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideoSideBySide = new ToolStripMenuItem();

        private ToolStripMenuItem mnuExportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportImageImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportImageKeys = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportImageSequence = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportImageSideBySide = new ToolStripMenuItem();

        private ToolStripMenuItem mnuExportSpreadsheet = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportODS = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportXLSX = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportCSVTrajectory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportCSVChronometer = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportTXTTrajectory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportJSON = new ToolStripMenuItem();


        private ToolStripMenuItem mnuExportDocument = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportODT = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportDOCX = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportMarkdown = new ToolStripMenuItem();

        private ToolStripMenuItem mnuCloseFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseFile2 = new ToolStripMenuItem();

        private ToolStripMenuItem mnuCutDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopyDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPasteDrawing = new ToolStripMenuItem();

        // View
        private ToolStripMenuItem mnuOnePlayer = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoPlayers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOneCapture = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoCaptures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTwoMixed = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVerticalLayout = new ToolStripMenuItem();
        private ToolStripMenuItem mnuToggleCommonCtrls = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSwapScreens = new ToolStripMenuItem();

        // Image
        private ToolStripMenuItem mnuDeinterlace = new ToolStripMenuItem();

        private ToolStripMenuItem mnuDemosaic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicNone = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicRGGB = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicBGGR = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicGRBG = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDemosaicGBRG = new ToolStripMenuItem();

        private ToolStripMenuItem mnuAspectRatio = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAspectRatioAuto = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAspectRatioForce43 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAspectRatioForce169 = new ToolStripMenuItem();

        private ToolStripMenuItem mnuRotation = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation0 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation90 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation180 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRotation270 = new ToolStripMenuItem();

        private ToolStripMenuItem mnuMirror = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStabilization = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStabilizationTrackNone = new ToolStripMenuItem();

        // Video
        private List<ToolStripMenuItem> filterMenus = new List<ToolStripMenuItem>();

        // Tools
        private ToolStripMenuItem mnuImportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuBackground = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTestGrid = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimeCalibration = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCoordinateSystem = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLensCalibration = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLensCalibrationOpen = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLensCalibrationMode = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLensCalibrationManual = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLensCalibrationNone = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCalibrationValidation = new ToolStripMenuItem();
        private ToolStripMenuItem mnuScatterDiagram = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLinearKinematics = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAngularKinematics = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAngleAngleDiagram = new ToolStripMenuItem();

        // Options
        private ToolStripMenuItem mnuVariables = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportVariables = new ToolStripMenuItem();

        #endregion

        #region Toolbar
        private ToolStripButton toolSave = new ToolStripButton();
        private ToolStripButton toolToggleNavigationPane = new ToolStripButton();
        private ToolStripButton toolExplorer = new ToolStripButton();
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

            dualPlayer.ExportImageAsked += (s, e) => ExportImages(ImageExportFormat.SideBySide);
            dualPlayer.ExportVideoAsked += (s, e) => ExportVideo(VideoExportFormat.SideBySide);

            audioInputLevelMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputLevelMonitor.Threshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;
            audioInputLevelMonitor.Triggered += (s, e) => TriggerCapture();
            audioInputLevelMonitor.DeviceLost += (s, e) => AudioDeviceLost();

            udpMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableUDPTrigger;
            udpMonitor.Port = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort;
            udpMonitor.Triggered += (s, e) => TriggerCapture();

            InitializeVideoFilters();

            CameraTypeManager.CameraLoadAsked += CameraTypeManager_CameraLoadAsked;
            NotificationCenter.LoadVideoAsked += NotificationCenter_LoadVideoAsked;
            NotificationCenter.StopPlaybackAsked += (s, e) => DoStopPlaying();
            NotificationCenter.PreferencesOpened += NotificationCenter_PreferencesOpened;
            NotificationCenter.ReceivedExternalCommand += NotificationCenter_ReceivedExternalCommand;

            playerScreens = screenList.Where(s => s is PlayerScreen).Select(s => s as PlayerScreen);
            captureScreens = screenList.Where(s => s is CaptureScreen).Select(s => s as CaptureScreen);
        }

        private void InitializeVideoFilters()
        {
            filterMenus.Add(CreateFilterMenu(VideoFilterType.None));
            filterMenus.Add(CreateFilterMenu(VideoFilterType.Kinogram));
            filterMenus.Add(CreateFilterMenu(VideoFilterType.CameraMotion));
            filterMenus.Add(CreateFilterMenu(VideoFilterType.LensCalibration));
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
                if (filterType == VideoFilterType.None)
                {
                    screen.DeactivateVideoFilter();
                }
                else
                {
                    if (filterType != screen.ActiveVideoFilterType)
                        screen.ActivateVideoFilter(filterType);
                }

                OrganizeMenus();
            };

            return menu;
        }

        public void RecoverCrash()
        {
            // Import recovered screens into launch settings.
            log.DebugFormat("Running crash recovery.");
            try
            {
                List<ScreenDescriptorPlayback> recoverables = RecoveryManager.GetRecoverables();
                if (recoverables != null && recoverables.Count > 0)
                {
                    FormCrashRecovery fcr = new FormCrashRecovery(recoverables);
                    fcr.StartPosition = FormStartPosition.CenterScreen;
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

            int index = 0;  // Open video
            index++;        // Open replay observer = 1
            index++;        // History = 2

            // ----
            index++;        // Separator = 3

            index++;
            mnuLoadAnnotations.Image = Properties.Resources.notes2_16;
            mnuLoadAnnotations.Click += mnuLoadAnnotationsOnClick;
            mnuLoadAnnotations.MergeIndex = index;
            mnuLoadAnnotations.MergeAction = MergeAction.Insert;

            index++;
            mnuSave.Image = Properties.Resources.save_annotations;
            mnuSave.Click += mnuSaveOnClick;
            mnuSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuSave.MergeIndex = index;
            mnuSave.MergeAction = MergeAction.Insert;

            index++;
            mnuSaveAs.Image = Properties.Resources.save_annotations;
            mnuSaveAs.Click += mnuSaveAsOnClick;
            mnuSaveAs.MergeIndex = index;
            mnuSaveAs.MergeAction = MergeAction.Insert;

            index++;
            mnuUnloadAnnotations.Image = Properties.Resources.delete_notes;
            mnuUnloadAnnotations.Click += mnuUnloadAnnotationsOnClick;
            mnuUnloadAnnotations.MergeIndex = index;
            mnuUnloadAnnotations.MergeAction = MergeAction.Insert;

            //----
            index++;    // Separator

            index++;
            mnuExportVideo.Image = Properties.Resources.film_save;
            mnuExportVideo.MergeIndex = index;
            mnuExportVideo.MergeAction = MergeAction.Insert;
            mnuExportVideoVideo.Image = Properties.Resources.export_video_video;
            mnuExportVideoSlideshow.Image = Properties.Resources.export_video_slideshow;
            mnuExportVideoWithPauses.Image = Properties.Resources.export_video_with_pauses;
            mnuExportVideoSideBySide.Image = Properties.Resources.export_video_sidebyside;
            mnuExportVideoVideo.Click += (s, e) => ExportVideo(VideoExportFormat.Video);
            mnuExportVideoSlideshow.Click += (s, e) => ExportVideo(VideoExportFormat.VideoSlideShow);
            mnuExportVideoWithPauses.Click += (s, e) => ExportVideo(VideoExportFormat.VideoWithPauses);
            mnuExportVideoSideBySide.Click += (s, e) => ExportVideo(VideoExportFormat.SideBySide);
            mnuExportVideo.DropDownItems.AddRange(new ToolStripItem[] {
                mnuExportVideoVideo,
                mnuExportVideoSlideshow,
                mnuExportVideoWithPauses,
                mnuExportVideoSideBySide,
            });

            index++;
            mnuExportImage.Image = Properties.Resources.picture_save;
            mnuExportImage.MergeIndex = index;
            mnuExportImage.MergeAction = MergeAction.Insert;
            mnuExportImageImage.Image = Properties.Resources.image;
            mnuExportImageSequence.Image = Properties.Resources.images;
            mnuExportImageKeys.Image = Properties.Resources.export_image_keys;
            mnuExportImageSideBySide.Image = Properties.Resources.export_image_sidebyside;
            mnuExportImageImage.Click += (s, e) => ExportImages(ImageExportFormat.Image);
            mnuExportImageSequence.Click += (s, e) => ExportImages(ImageExportFormat.ImageSequence);
            mnuExportImageKeys.Click += (s, e) => ExportImages(ImageExportFormat.KeyImages);
            mnuExportImageSideBySide.Click += (s, e) => ExportImages(ImageExportFormat.SideBySide);
            mnuExportImage.DropDownItems.AddRange(new ToolStripItem[] {
                mnuExportImageImage,
                mnuExportImageSequence,
                mnuExportImageKeys,
                mnuExportImageSideBySide,
            });

            index++;
            mnuExportDocument.Image = Properties.Resources.export_document;
            mnuExportDocument.MergeIndex = index;
            mnuExportDocument.MergeAction = MergeAction.Insert;
            mnuExportODT.Image = Properties.Resources.file_odt;
            mnuExportDOCX.Image = Properties.Resources.file_doc;
            mnuExportMarkdown.Image = Properties.Resources.file_markdown;
            mnuExportODT.Click += (s, e) => ExportDocument(DocumentExportFormat.ODT);
            mnuExportDOCX.Click += (s, e) => ExportDocument(DocumentExportFormat.DOCX);
            mnuExportMarkdown.Click += (s, e) => ExportDocument(DocumentExportFormat.Mardown);
            mnuExportDocument.DropDownItems.AddRange(new ToolStripItem[] {
                mnuExportODT,
                mnuExportDOCX,
                new ToolStripSeparator(),
                mnuExportMarkdown,
            });

            index++;
            mnuExportSpreadsheet.Image = Properties.Resources.export_spreadsheet;
            mnuExportSpreadsheet.MergeIndex = index;
            mnuExportSpreadsheet.MergeAction = MergeAction.Insert;
            mnuExportODS.Image = Properties.Resources.file_ods;
            mnuExportXLSX.Image = Properties.Resources.file_xls;
            mnuExportCSVTrajectory.Image = Properties.Resources.file_csv;
            mnuExportCSVChronometer.Image = Properties.Resources.file_csv;
            mnuExportTXTTrajectory.Image = Properties.Resources.file_txt;
            mnuExportJSON.Image = Properties.Resources.json;
            mnuExportODS.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.ODS);
            mnuExportXLSX.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.XLSX);
            mnuExportCSVTrajectory.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.CSVTrajectory);
            mnuExportCSVChronometer.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.CSVChronometer);
            mnuExportTXTTrajectory.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.TXTTrajectory);
            mnuExportJSON.Click += (s, e) => ExportSpreadsheet(SpreadsheetExportFormat.JSON);
            mnuExportSpreadsheet.DropDownItems.AddRange(new ToolStripItem[] {
                mnuExportODS,
                mnuExportXLSX,
                mnuExportCSVTrajectory,
                mnuExportTXTTrajectory,
                mnuExportCSVChronometer,
                new ToolStripSeparator(),
                mnuExportJSON,
            });

            //------------------------
            index++; // Separator

            index++;
            mnuCloseFile.Image = Properties.Resources.close_player;
            mnuCloseFile.Enabled = false;
            mnuCloseFile.Click += mnuCloseFileOnClick;
            mnuCloseFile.MergeIndex = index;
            mnuCloseFile.MergeAction = MergeAction.Insert;

            index++;
            mnuCloseFile2.Image = Properties.Resources.close_player;
            mnuCloseFile2.Enabled = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Click += mnuCloseFile2OnClick;
            mnuCloseFile2.MergeIndex = index;
            mnuCloseFile2.MergeAction = MergeAction.Insert;

            //----
            index++;    // Separator
            index++;    // Quit

            ToolStripItem[] subFile = new ToolStripItem[] {
                // Open file
                // Open replay observer,
                // Recent,
                // ----
                mnuLoadAnnotations,
                mnuSave,
                mnuSaveAs,
                mnuUnloadAnnotations,
                // ----
                mnuExportVideo,
                mnuExportImage,
                mnuExportDocument,
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
            mnuCutDrawing.Click += mnuCutDrawing_OnClick;
            mnuCutDrawing.MergeAction = MergeAction.Append;
            mnuCopyDrawing.Image = Properties.Drawings.copy;
            mnuCopyDrawing.Click += mnuCopyDrawing_OnClick;
            mnuCopyDrawing.MergeAction = MergeAction.Append;
            mnuPasteDrawing.Image = Properties.Drawings.paste;
            mnuPasteDrawing.Click += mnuPasteDrawing_OnClick;
            mnuPasteDrawing.MergeAction = MergeAction.Append;

            ToolStripItem[] subEdit = new ToolStripItem[] {
                new ToolStripSeparator(),
                mnuCutDrawing,
                mnuCopyDrawing,
                mnuPasteDrawing
            };
            mnuCatchEdit.DropDownItems.AddRange(subEdit);
            #endregion

            #region View
            ToolStripMenuItem mnuCatchScreens = new ToolStripMenuItem();
            mnuCatchScreens.MergeIndex = 2; // (Screens)
            mnuCatchScreens.MergeAction = MergeAction.MatchOnly;

            mnuOnePlayer.Image = Properties.Resources.television;
            mnuOnePlayer.Click += mnuOnePlayerOnClick;
            mnuOnePlayer.MergeAction = MergeAction.Append;
            mnuTwoPlayers.Image = Properties.Resources.dualplayback;
            mnuTwoPlayers.Click += mnuTwoPlayersOnClick;
            mnuTwoPlayers.MergeAction = MergeAction.Append;
            mnuOneCapture.Image = Properties.Resources.camera_video;
            mnuOneCapture.Click += mnuOneCaptureOnClick;
            mnuOneCapture.MergeAction = MergeAction.Append;
            mnuTwoCaptures.Image = Properties.Resources.dualcapture2;
            mnuTwoCaptures.Click += mnuTwoCapturesOnClick;
            mnuTwoCaptures.MergeAction = MergeAction.Append;
            mnuTwoMixed.Image = Properties.Resources.dualmixed3;
            mnuTwoMixed.Click += mnuTwoMixedOnClick;
            mnuTwoMixed.MergeAction = MergeAction.Append;

            mnuSwapScreens.Image = Properties.Resources.flatswap3d;
            mnuSwapScreens.Enabled = false;
            mnuSwapScreens.Click += mnuSwapScreensOnClick;
            mnuSwapScreens.MergeAction = MergeAction.Append;

            mnuVerticalLayout.Image = Properties.Resources.application_split;
            mnuVerticalLayout.Enabled = false;
            mnuVerticalLayout.Click += mnuVerticalLayout_Click;
            mnuVerticalLayout.MergeAction = MergeAction.Append;

            mnuToggleCommonCtrls.Image = Properties.Resources.application_common_controls2;
            mnuToggleCommonCtrls.Enabled = false;
            mnuToggleCommonCtrls.ShortcutKeys = Keys.F5;
            mnuToggleCommonCtrls.Click += mnuToggleCommonCtrlsOnClick;
            mnuToggleCommonCtrls.MergeAction = MergeAction.Append;

            ToolStripItem[] subScreens = new ToolStripItem[] { 		
                mnuOnePlayer,
                mnuTwoPlayers,
                mnuOneCapture,
                mnuTwoCaptures,
                mnuTwoMixed,
                new ToolStripSeparator(),
                mnuToggleCommonCtrls,
                mnuVerticalLayout,
                mnuSwapScreens
            };
            
            mnuCatchScreens.DropDownItems.AddRange(subScreens);
            #endregion

            #region Image
            ToolStripMenuItem mnuCatchImage = new ToolStripMenuItem();
            mnuCatchImage.MergeIndex = 3; // (Image)
            mnuCatchImage.MergeAction = MergeAction.MatchOnly;

            mnuDeinterlace.Image = Properties.Resources.deinterlace;
            mnuDeinterlace.Checked = false;
            mnuDeinterlace.ShortcutKeys = Keys.Control | Keys.D;
            mnuDeinterlace.Click += mnuDeinterlaceOnClick;
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

            mnuAspectRatioAuto.Checked = true;
            mnuAspectRatioAuto.Click += mnuFormatAutoOnClick;
            mnuAspectRatioAuto.MergeAction = MergeAction.Append;
            mnuAspectRatioForce43.Image = Properties.Resources.format43;
            mnuAspectRatioForce43.Click += mnuFormatForce43OnClick;
            mnuAspectRatioForce43.MergeAction = MergeAction.Append;
            mnuAspectRatioForce169.Image = Properties.Resources.format169;
            mnuAspectRatioForce169.Click += mnuFormatForce169OnClick;
            mnuAspectRatioForce169.MergeAction = MergeAction.Append;
            mnuAspectRatio.Image = Properties.Resources.picture_in_picture_16;
            mnuAspectRatio.MergeAction = MergeAction.Append;
            mnuAspectRatio.DropDownItems.AddRange(new ToolStripItem[] { mnuAspectRatioAuto, new ToolStripSeparator(), mnuAspectRatioForce43, mnuAspectRatioForce169});

            mnuRotation0.Click += mnuRotation0_Click;
            mnuRotation90.Image = Properties.Resources.rotate90;
            mnuRotation90.Click += mnuRotation90_Click;
            mnuRotation180.Image = Properties.Resources.rotate180;
            mnuRotation180.Click += mnuRotation180_Click;
            mnuRotation270.Image = Properties.Resources.rotate270;
            mnuRotation270.Click += mnuRotation270_Click;
            mnuRotation.Image = Properties.Resources.tilt_16;
            mnuRotation.MergeAction = MergeAction.Append;
            mnuRotation.DropDownItems.AddRange(new ToolStripItem[] { mnuRotation0, mnuRotation90, mnuRotation270, mnuRotation180 });

            mnuMirror.Image = Properties.Resources.shape_mirror;
            mnuMirror.Checked = false;
            mnuMirror.ShortcutKeys = Keys.Control | Keys.M;
            mnuMirror.Click += mnuMirror_Click;
            mnuMirror.MergeAction = MergeAction.Append;

            mnuStabilization.Image = Properties.Resources.pin;
            mnuStabilization.MergeAction = MergeAction.Append;
            mnuStabilizationTrackNone.Image = Properties.Resources.null_symbol_16;
            mnuStabilizationTrackNone.Tag = Guid.Empty;
            mnuStabilizationTrackNone.Checked = true;
            mnuStabilizationTrackNone.Click += mnuStabilizationTrack_OnClick;
            // The drop down items are rebuilt on the fly.
            mnuStabilization.DropDownItems.Clear();
            mnuStabilization.DropDownItems.Add(mnuStabilizationTrackNone);

            mnuCatchImage.DropDownItems.Add(mnuAspectRatio);
            mnuCatchImage.DropDownItems.Add(mnuRotation);
            mnuCatchImage.DropDownItems.Add(mnuMirror);
            mnuCatchImage.DropDownItems.Add(mnuDeinterlace);
            mnuCatchImage.DropDownItems.Add(mnuDemosaic);
            mnuCatchImage.DropDownItems.Add(mnuStabilization);

            #endregion

            #region Video
            ToolStripMenuItem mnuCatchVideo = new ToolStripMenuItem();
            mnuCatchVideo.MergeIndex = 4;
            mnuCatchVideo.MergeAction = MergeAction.MatchOnly;

            ConfigureVideoFilterMenus(null);
            mnuCatchVideo.DropDownItems.Add(filterMenus[0]);
            mnuCatchVideo.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 1; i < filterMenus.Count; i++)
                mnuCatchVideo.DropDownItems.Add(filterMenus[i]);

            #endregion

            #region Tools
            ToolStripMenuItem mnuCatchTools = new ToolStripMenuItem();
            mnuCatchTools.MergeIndex = 5;
            mnuCatchTools.MergeAction = MergeAction.MatchOnly;

            mnuImportImage.Image = Properties.Resources.image;
            mnuImportImage.Click += mnuImportImage_OnClick;
            mnuImportImage.MergeAction = MergeAction.Append;

            mnuBackground.Image = Properties.Resources.shading;
            mnuBackground.Click += mnuBackgroundColor_Click;
            mnuBackground.MergeAction = MergeAction.Append;

            mnuTestGrid.Image = Properties.Resources.grid2;
            mnuTestGrid.Click += mnuTestGrid_OnClick;
            mnuTestGrid.MergeAction = MergeAction.Append;

            mnuTimeCalibration.Image = Properties.Drawings.clock_frame;
            mnuTimeCalibration.Click += mnuTimebase_OnClick;
            mnuTimeCalibration.MergeAction = MergeAction.Append;

            mnuLensCalibration.Image = Properties.Resources.checkerboard;
            mnuLensCalibration.MergeAction = MergeAction.Append;
            mnuLensCalibrationOpen.Image = Properties.Resources.folder;
            mnuLensCalibrationMode.Image = Properties.Resources.checkerboard;
            mnuLensCalibrationManual.Image = Properties.Resources.border_all;
            mnuLensCalibrationNone.Image = Properties.Resources.null_symbol_16;
            mnuLensCalibrationOpen.Click += mnuLensCalibrationOpen_OnClick;
            mnuLensCalibrationMode.Click += mnuLensCalibrationMode_OnClick;
            mnuLensCalibrationManual.Click += mnuLensCalibrationManual_OnClick;
            mnuLensCalibrationNone.Click += mnuLensCalibrationNone_OnClick;

            BuildLensCalibrationMenu();

            mnuCoordinateSystem.Image = Properties.Resources.coordinate_axis;
            mnuCoordinateSystem.Click += mnuCoordinateSystem_OnClick;
            mnuCoordinateSystem.MergeAction = MergeAction.Append;

            mnuCalibrationValidation.Image = Properties.Resources.ruler_triangle;
            mnuCalibrationValidation.Click += mnuCalibrationValidation_OnClick;
            mnuCalibrationValidation.MergeAction = MergeAction.Append;

            mnuScatterDiagram.Image = Properties.Resources.scatter_plot_16;
            mnuScatterDiagram.Click += mnuScatterDiagram_OnClick;
            mnuScatterDiagram.MergeAction = MergeAction.Append;

            mnuLinearKinematics.Image = Properties.Resources.plot_16;
            mnuLinearKinematics.Click += mnuLinearKinematics_OnClick;
            mnuLinearKinematics.MergeAction = MergeAction.Append;

            mnuAngularKinematics.Image = Properties.Resources.sine_16;
            mnuAngularKinematics.Click += mnuAngularKinematics_OnClick;
            mnuAngularKinematics.MergeAction = MergeAction.Append;

            mnuAngleAngleDiagram.Image = Properties.Resources.plot_16;
            mnuAngleAngleDiagram.Click += mnuAngleAngleDiagram_OnClick;
            mnuAngleAngleDiagram.MergeAction = MergeAction.Append;

            mnuCatchTools.DropDownItems.AddRange(new ToolStripItem[] {
                mnuImportImage,
                mnuBackground,
                mnuTestGrid,
                new ToolStripSeparator(),
                mnuTimeCalibration,
                mnuCoordinateSystem,
                mnuLensCalibration,
                mnuCalibrationValidation,
                new ToolStripSeparator(),
                mnuScatterDiagram,
                mnuLinearKinematics,
                mnuAngularKinematics,
                mnuAngleAngleDiagram
            });

            #endregion

            #region Options
            ToolStripMenuItem mnuCatchOptions = new ToolStripMenuItem();
            mnuCatchOptions.MergeIndex = 7;
            mnuCatchOptions.MergeAction = MergeAction.MatchOnly;

            // Language     = 0
            // Time         = 1
            // Pointer      = 2
            // ----         = 3
            // Variables    = 4
            // ----         = 5
            // Preferences  = 6

            mnuVariables.Image = Properties.Capture.id3_16;
            mnuVariables.MergeIndex = 4;
            mnuVariables.MergeAction = MergeAction.Insert;
            mnuImportVariables.Image = Properties.Resources.folder;
            mnuImportVariables.Click += mnuImportVariables_OnClick;

            // It's possible that by this point the menu was already built,
            // if we imported a profile from the command line or preferences.
            // In that case the import menu is already there, if not we must add it.
            if (mnuVariables.DropDownItems.Count == 0)
            {
                mnuVariables.DropDownItems.Add(mnuImportVariables);
            }

            ToolStripItem[] subOptions = new ToolStripItem[] {
                mnuVariables,
            };
            
            mnuCatchOptions.DropDownItems.AddRange(subOptions);

            #endregion

            MenuStrip ThisMenu = new MenuStrip();
            ThisMenu.Items.AddRange(new ToolStripItem[] { 
                mnuCatchFile, mnuCatchEdit, mnuCatchScreens, mnuCatchImage, mnuCatchVideo, mnuCatchTools, mnuCatchOptions });
            ThisMenu.AllowMerge = true;

            ToolStripManager.Merge(ThisMenu, menu);

            RefreshCultureMenu();
        }

        public void ExtendToolBar(ToolStrip toolbar)
        {
            // Save
            toolSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolSave.Image = Properties.Resources.save_annotations;
            toolSave.Click += mnuSaveOnClick;

            toolToggleNavigationPane.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolToggleNavigationPane.Image = Properties.Resources.navigation_pane;
            toolToggleNavigationPane.Click += toolToggleNavigationPanel_Click;

            toolExplorer.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolExplorer.Image = Properties.Resources.file_browser;
            toolExplorer.Click += mnuExplorer_OnClick;

            toolOnePlayer.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOnePlayer.Image = Properties.Resources.television;
            toolOnePlayer.Click += mnuOnePlayerOnClick;

            toolTwoPlayers.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoPlayers.Image = Properties.Resources.dualplayback;
            toolTwoPlayers.Click += mnuTwoPlayersOnClick;

            toolOneCapture.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOneCapture.Image = Properties.Resources.camera_video;
            toolOneCapture.Click += mnuOneCaptureOnClick;

            toolTwoCaptures.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoCaptures.Image = Properties.Resources.dualcapture2;
            toolTwoCaptures.Click += mnuTwoCapturesOnClick;

            toolTwoMixed.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolTwoMixed.Image = Properties.Resources.dualmixed3;
            toolTwoMixed.Click += mnuTwoMixedOnClick;

            ToolStrip ts = new ToolStrip(new ToolStripItem[] {
                                            toolSave,
                                            new ToolStripSeparator(),
                                            toolToggleNavigationPane,
                                            new ToolStripSeparator(),
                                            toolExplorer,
                                            toolOnePlayer,
                                            toolTwoPlayers,
                                            toolOneCapture,
                                            toolTwoCaptures,
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

        /// <summary>
        /// Called after a change in preferences.
        /// </summary>
        public void RefreshUICulture()
        {
            // We may have changed the trigger source/parameters.
            audioInputLevelMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputLevelMonitor.Threshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;
            udpMonitor.Enabled = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableUDPTrigger;
            udpMonitor.Port = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort;
            if (captureScreens.Count() > 0)
            {
                if (audioInputLevelMonitor.Enabled)
                {
                    string id = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
                    audioInputLevelMonitor.Start(id);
                }

                if (udpMonitor.Enabled)
                {
                    int port = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort;
                    udpMonitor.Start(port);
                }
            }

            // Local
            RefreshCultureMenu();
            OrganizeMenus();
            RefreshCultureToolbar();
            NotificationCenter.RaiseUpdateStatus();

            dualPlayer.RefreshUICulture();
            dualCapture.RefreshUICulture();
            view.RefreshUICulture();

            // Screens
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
            RefreshUICulture();
        }
        #endregion

        #region Event handlers for screens
        private void Screen_Activated(object sender, EventArgs e)
        {
            AbstractScreen screen = sender as AbstractScreen;
            SetActiveScreen(screen);
        }
        private void Screen_LoadAnnotationsAsked(object sender, EventArgs e)
        {
            int index = sender == screenList[0] ? 0 : 1;
            LoadAnnotations(index);
        }
        private void Screen_DualCommandReceived(object sender, EventArgs<HotkeyCommand> e)
        {
            // A screen has received a hotkey that must be handled at manager level.
            if (dualPlayer.Active)
                dualPlayer.ExecuteDualCommand(e.Value);
            else if (dualCapture.Active)
                dualCapture.ExecuteDualCommand(e.Value);
        }
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
        private void Player_OpenVideoAsked(object sender, EventArgs e)
        {
            string title = ScreenManagerLang.mnuOpenVideo;
            string filter = ScreenManagerLang.FileFilter_All + "|*.*";
            string filename = FilePicker.OpenVideo(title, filter);
            if (string.IsNullOrEmpty(filename))
                return;

            int index = sender == screenList[0] ? 0 : 1;
            NotificationCenter.RaiseLoadVideoAsked(filename, index);
        }
        private void Player_OpenReplayWatcherAsked(object sender, EventArgs<CaptureFolder> e)
        {
            // Replay watcher asked from the context menu in the player screen.
            // This ends up in FrameServerPlayer.Load(path) which can handle
            // capture folder id or normal paths.

            int index = sender == screenList[0] ? 0 : 1;
            
            // Start from the current screen descriptor if any. To get UI state.
            ScreenDescriptorPlayback sdp;
            if (sender is PlayerScreen ps)
            {
                sdp = (ScreenDescriptorPlayback)ps.GetScreenDescriptor();
            }
            else
            {
                sdp = new ScreenDescriptorPlayback();
            }

            string path = "";
            if (e.Value == null)
            {
                // Open a random folder on the file system.
                path = FilePicker.OpenReplayWatcher();
                if (string.IsNullOrEmpty(path))
                    return;

                // Add it to capture folders.
                CaptureFolder cf = PreferencesManager.CapturePreferences.AddCaptureFolder(path);
                sdp.FullPath = cf.Id.ToString();
                path = sdp.FullPath;

                // TODO: trigger preferences updated.
            }
            else
            {
                // Open a known capture folder.
                string id = e.Value.Id.ToString();
                CaptureFolder cf = FilesystemHelper.GetCaptureFolder(id);
                if (cf == null)
                {
                    log.ErrorFormat("Capture folder not found: \"{0}\"", id);
                    return;
                }

                sdp.FullPath = id;
                var context = DynamicPathResolver.BuildDateContext();
                path = DynamicPathResolver.Resolve(cf.Path, context);

                if (!FilesystemHelper.IsValidPath(path))
                {
                    log.ErrorFormat("Invalid capture folder path: \"{0}\"", path);
                    return;
                }

                // If the watched folder doesn't exist we create it.
                // This may happen when we have a dynamic path with a date and it's the first session of the day.
                // The capture screen would only create the folder on the first recording, it would be too late.
                // The watcher needs an actual folder to watch before the capture puts the recording in it.
                if (!Directory.Exists(path))
                {
                    log.DebugFormat("Replay watcher asked for a non-existent capture folder. Creating the folder on the file system.");
                    Directory.CreateDirectory(path);
                }

                path = sdp.FullPath;
            }
                
            sdp.IsReplayWatcher = true;
            sdp.Autoplay = true;
            sdp.Stretch = true;
            sdp.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
            LoaderVideo.LoadVideoInScreen(this, path, index, sdp);
        }
        private void Player_Loaded(object sender, EventArgs e)
        {
            // We just received an event that one player screen finished loading.
            // For dual replay we can never really tell if this is the first or the last event of a dual recording,
            // or an isolated event.
            // Instead of trying to keep track of that we will check the creation dates of the files for 
            // auto-play of dual recordings. The other contexts don't need to auto-start.
            // The screens themselves know they are in dual replay context and won't auto-play.
            ResetSync();
            dualPlayer.CommitLaunchSettings();
            OrganizeMenus();
            NotificationCenter.RaiseUpdateStatus();
            return;
        }
        private void Player_SelectionChanged(object sender, EventArgs<bool> e)
        {
            ResetSync();
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

        private void Player_DrawingAdded(object sender, EventArgs e)
        {
            // Make sure the export options are correctly enabled.
            OrganizeMenus();
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
            if(screen == null )
                return;

            if (screen == activeScreen)
                return;

            if (screenList.Count == 1)
            {
                activeScreen = screen;
                OrganizeMenus();
            }
            else
            {
                activeScreen = screen;

                foreach (AbstractScreen s in screenList)
                    s.DisplayAsActiveScreen(s == screen);
                
                OrganizeMenus();
            }

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

            IdentifyScreens();
        }

        public void SwapScreens()
        {
            if (screenList.Count != 2)
                return;

            AbstractScreen temp = screenList[0];
            screenList[0] = screenList[1];
            screenList[1] = temp;

            IdentifyScreens();
        }

        /// <summary>
        /// Make sure the screens know their index in the list.
        /// Should be called after any change to the screen list.
        /// </summary>
        private void IdentifyScreens()
        {
            for (int i = 0; i < screenList.Count; i++)
                screenList[i].Identify(i);
        }

        /// <summary>
        /// This is called after the screen list changed.
        /// </summary>
        public void OrganizeScreens()
        {
            IdentifyScreens();

            view.OrganizeScreens(screenList);
            NotificationCenter.RaiseUpdateStatus();

            if (captureScreens.Count() == 0)
            {
                audioInputLevelMonitor.Stop();
                udpMonitor.Stop();
            }
            else
            {
                if (audioInputLevelMonitor.Enabled)
                {
                    string id = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
                    audioInputLevelMonitor.Start(id);
                }
                else
                {
                   audioInputLevelMonitor.Stop();
                }

                if (udpMonitor.Enabled)
                {
                    int port = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort;
                    udpMonitor.Start(port);
                }
                else
                {
                    udpMonitor.Stop();
                }
            }

            // If we are in "Continue where you left off" mode, save immediately.
            // This is not strictly necessary as we will save on close but it helps 
            // the other windows get a more up to date state of this window.
            // We must only do this if we are not in the process of closing though
            // otherwise we always save an empty state as this also runs *after* the screens are closed.
            if (WindowManager.ActiveWindow.StartupMode == WindowStartupMode.Continue &&
                !autoLaunchInProgress && 
                !view.Closing)
            {
                var descriptors = GetScreenDescriptors();
                WindowManager.ActiveWindow.ReplaceScreens(descriptors);
                WindowManager.SaveActiveWindow();
            }
        }

        /// <summary>
        /// Get the current status string for the status bar.
        /// </summary>
        public string GetStatus()
        {
            String statusString = "";
            switch(screenList.Count)
            {
                case 0:
                    statusString = view.GetBrowserStatusString();
                    break;
                case 1:
                    statusString = screenList[0].Status;
                    break;
                case 2:
                    statusString = screenList[0].Status + " | " + screenList[1].Status;
                    break;
                default:
                    break;
            }

            return statusString;
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
            view.SetFullScreen(fullScreen);

            foreach (AbstractScreen screen in screenList)
                screen.FullScreen(fullScreen);
        }
        public static void AlertInvalidFileName()
        {
            string msgTitle = ScreenManagerLang.Error_Capture_InvalidFile_Title;
            string msgText = ScreenManagerLang.Error_Capture_InvalidFile_Text.Replace("\\n", "\n");

            msgText += "\n";
            msgText += "\\ / : * ? ' < > %";

            MessageBox.Show(msgText, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void AlertDirectoryNotCreated()
        {
            string msgTitle = ScreenManagerLang.Error_Capture_DirectoryNotCreated_Title;
            string msgText = ScreenManagerLang.Error_Capture_DirectoryNotCreated_Text.Replace("\\n", "\n");

            MessageBox.Show(msgText, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void AlertCaptureFolderNotDefined()
        {
            string msgTitle = "Capture folder not defined";
            string msgText = "Select a capture folder from the drop down list in the lower left";
            MessageBox.Show(msgText, msgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Ask all screens for an up to date screen descriptor.
        /// </summary>
        public List<IScreenDescriptor> GetScreenDescriptors()
        {
            List<IScreenDescriptor> screenDescriptors = new List<IScreenDescriptor>();
            foreach (var screen in screenList)
                screenDescriptors.Add(screen.GetScreenDescriptor());

            return screenDescriptors;
        }
        #endregion

        #region Menu organization
        public void OrganizeMenus()
        {
            DoOrganizeMenu();
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
                    mnuLoadAnnotations.Enabled = true;
                    mnuSave.Enabled = true;
                    mnuSaveAs.Enabled = true;
                    mnuUnloadAnnotations.Enabled = true;
                    mnuExportVideo.Enabled = true;
                    mnuExportImage.Enabled = true;
                    mnuExportSpreadsheet.Enabled = player.FrameServer.Metadata.HasVisibleData;
                    mnuExportDocument.Enabled = true;

                    ConfigureSaveMenu(activeScreen);
                        
                    // Edit
                    HistoryMenuManager.SwitchContext(activeScreen.HistoryStack);
                    ConfigureClipboardMenus(player);

                    // Image
                    mnuDeinterlace.Enabled = player.FrameServer.VideoReader.CanChangeDeinterlacing;
                    mnuMirror.Enabled = true;
                    mnuStabilization.Enabled = true;
                    mnuDeinterlace.Checked = player.Deinterlaced;
                    mnuMirror.Checked = activeScreen.Mirrored;

                    if (!player.IsSingleFrame)
                    {
                        ConfigureImageFormatMenus(activeScreen);
                        ConfigureImageRotationMenus(activeScreen);
                        ConfigureImageDemosaicingMenus(activeScreen);
                        ConfigureImageStabilizationMenus(activeScreen);
                    }
                    else
                    {
                        ConfigureImageFormatMenus(null);
                        ConfigureImageRotationMenus(null);
                        ConfigureImageDemosaicingMenus(null);
                        ConfigureImageStabilizationMenus(null);
                    }

                    // Video
                    ConfigureVideoFilterMenus(player);

                    // Tools
                    mnuImportImage.Enabled = true;
                    mnuBackground.Enabled = true;
                    mnuTestGrid.Enabled = true;
                    mnuTimeCalibration.Enabled = true;
                    mnuCoordinateSystem.Enabled = true;
                    mnuLensCalibration.Enabled = true;
                    ConfigureLensCalibrationMenus(player);
                    mnuCalibrationValidation.Enabled = true;
                    mnuScatterDiagram.Enabled = true;
                    mnuLinearKinematics.Enabled = true;
                    mnuAngularKinematics.Enabled = true;
                    mnuAngleAngleDiagram.Enabled = true;

                    mnuCoordinateSystem.Checked = activeScreen.CoordinateSystemVisible;
                    mnuTestGrid.Checked = activeScreen.TestGridVisible;

                    // Toolbar
                    toolSave.Enabled = true;
                }
                else if(activeScreen is CaptureScreen)
                {
                    CaptureScreen captureScreen = activeScreen as CaptureScreen;

                    // File
                    mnuLoadAnnotations.Enabled = true;
                    mnuSave.Enabled = true;
                    mnuSaveAs.Enabled = true;
                    mnuUnloadAnnotations.Enabled = true;

                    ConfigureSaveMenu(activeScreen);

                    mnuExportVideo.Enabled = false;
                    mnuExportImage.Enabled = false;
                    mnuExportSpreadsheet.Enabled = false;
                    mnuExportDocument.Enabled = false;

                    // Edit
                    HistoryMenuManager.SwitchContext(activeScreen.HistoryStack);
                    ConfigureClipboardMenus(activeScreen);

                    // Image
                    mnuDeinterlace.Enabled = false;
                    mnuMirror.Enabled = true;
                    mnuDeinterlace.Checked = false;
                    mnuMirror.Checked = activeScreen.Mirrored;

                    ConfigureImageFormatMenus(activeScreen);
                    ConfigureImageRotationMenus(activeScreen);
                    ConfigureImageDemosaicingMenus(activeScreen);
                    ConfigureImageStabilizationMenus(activeScreen);

                    // Video
                    ConfigureVideoFilterMenus(null);

                    // Tools
                    mnuImportImage.Enabled = false;
                    mnuBackground.Enabled = false;
                    mnuTestGrid.Enabled = true;
                    mnuTimeCalibration.Enabled = false;
                    mnuCoordinateSystem.Enabled = true;
                    mnuLensCalibration.Enabled = false;
                    mnuCalibrationValidation.Enabled = true;
                    mnuScatterDiagram.Enabled = false;
                    mnuLinearKinematics.Enabled = false;
                    mnuAngularKinematics.Enabled = false;
                    mnuAngleAngleDiagram.Enabled = false;

                    mnuCoordinateSystem.Checked = activeScreen.CoordinateSystemVisible;
                    mnuTestGrid.Checked = activeScreen.TestGridVisible;

                    // Toolbar
                    toolSave.Enabled = true;
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
                mnuLoadAnnotations.Enabled = false;
                mnuSave.Enabled = false;
                mnuSaveAs.Enabled = false;
                mnuUnloadAnnotations.Enabled = false;
                mnuExportVideo.Enabled = false;
                mnuExportImage.Enabled = false;
                mnuExportSpreadsheet.Enabled = false;
                mnuExportDocument.Enabled = false;
                toolSave.Enabled = false;

                ConfigureSaveMenu(activeScreen);

                // Edit
                HistoryMenuManager.SwitchContext(null);
                ConfigureClipboardMenus(null);

                // Image
                mnuDeinterlace.Enabled = false;
                mnuDeinterlace.Checked = false;
                mnuMirror.Enabled = false;
                mnuMirror.Checked = false;
                mnuBackground.Enabled = false;
                ConfigureImageFormatMenus(null);
                ConfigureImageRotationMenus(null);
                ConfigureImageDemosaicingMenus(null);
                ConfigureImageStabilizationMenus(null);

                // Video
                ConfigureVideoFilterMenus(null);

                // Tools
                mnuTimeCalibration.Enabled = false;
                mnuImportImage.Enabled = false;
                mnuTestGrid.Enabled = false;
                mnuCoordinateSystem.Enabled = false;
                mnuLensCalibration.Enabled = false;
                mnuCalibrationValidation.Enabled = false;
                mnuScatterDiagram.Enabled = false;
                mnuLinearKinematics.Enabled = false;
                mnuAngularKinematics.Enabled = false;
                mnuAngleAngleDiagram.Enabled = false;

                mnuCoordinateSystem.Checked = false;
                mnuTestGrid.Checked = false;
            }
            #endregion

            #region Menus depending on the specifc screen configuration
            // File
            mnuCloseFile.Visible  = false;
            mnuCloseFile.Enabled  = false;
            mnuCloseFile2.Visible = false;
            mnuCloseFile2.Enabled = false;
            string strClosingText = ScreenManagerLang.Generic_Close;

            bool hasNothingToClose = false;
            bool canSaveSideBySide = true;
            switch (screenList.Count)
            {
                case 0:
                    mnuToggleCommonCtrls.Enabled = false;
                    mnuVerticalLayout.Enabled = false;
                    mnuSwapScreens.Enabled = false;
                    hasNothingToClose = true;
                    canSaveSideBySide = false;
                    break;

                case 1:
                    mnuToggleCommonCtrls.Enabled = false;
                    mnuVerticalLayout.Enabled = false;
                    mnuSwapScreens.Enabled = false;
                    canSaveSideBySide = false;

                    if(!screenList[0].Full)
                    {
                        hasNothingToClose = true;
                    }
                    else if(screenList[0] is PlayerScreen)
                    {
                        // The only screen is a loaded PlayerScreen.
                        mnuCloseFile.Text = string.Format("{0} ({1})", strClosingText, ((PlayerScreen)screenList[0]).FileName);
                        mnuCloseFile.Enabled = true;
                        mnuCloseFile.Visible = true;

                        mnuCloseFile2.Visible = false;
                        mnuCloseFile2.Enabled = false;
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                        hasNothingToClose = true;
                    }
                    break;

                case 2:
                    mnuToggleCommonCtrls.Enabled = canShowCommonControls;
                    mnuVerticalLayout.Enabled = true;
                    mnuVerticalLayout.Checked = view.IsTopBottomLayout;
                    mnuSwapScreens.Enabled = true;

                    // Left Screen
                    if (screenList[0] is PlayerScreen)
                    {
                        if (screenList[0].Full)
                        {
                            hasNothingToClose = false;

                            mnuCloseFile.Text = string.Format("{0} ({1})", strClosingText, ((PlayerScreen)screenList[0]).FileName);
                            mnuCloseFile.Enabled = true;
                            mnuCloseFile.Visible = true;
                        }
                        else
                        {
                            // Left screen is an empty PlayerScreen.
                            // Global emptiness might be changed below.
                            hasNothingToClose = true;
                            canSaveSideBySide = false;
                        }
                    }
                    else if(screenList[0] is CaptureScreen)
                    {
                        // Global emptiness might be changed below.
                        hasNothingToClose = true;
                        canSaveSideBySide = false;
                    }

                    // Right Screen.
                    if (screenList[1] is PlayerScreen)
                    {
                        if (screenList[1].Full)
                        {
                            hasNothingToClose = false;
                            
                            mnuCloseFile2.Text = string.Format("{0} ({1})", strClosingText, ((PlayerScreen)screenList[1]).FileName);
                            mnuCloseFile2.Enabled = true;
                            mnuCloseFile2.Visible = true;
                        }
                        else
                        {
                            // Right screen is an empty player screen, nothing to do.
                            // The final value of hasNothingToClose stays at whatever the value was for the left screen.
                            canSaveSideBySide = false;
                        }
                    }
                    else if (screenList[1] is CaptureScreen)
                    {
                        // Right screen is a capture screen, nothing to do.
                        // The final value of hasNothingToClose stays at whatever the value was for the left screen.
                        canSaveSideBySide = false;
                    }
                    break;

                default:
                    // KO.
                    mnuToggleCommonCtrls.Enabled = false;
                    mnuVerticalLayout.Enabled = false;
                    mnuSwapScreens.Enabled = false;
                    hasNothingToClose = true;
                    break;
            }

            if (hasNothingToClose)
            {
                // No screens or all screens are either capture or empty players.
                // Single menu visible but disabled.
                mnuCloseFile.Text = strClosingText;
                mnuCloseFile.Visible = true;
                mnuCloseFile.Enabled = false;
                mnuCloseFile2.Visible = false;
            }

            mnuExportImageSideBySide.Enabled = canSaveSideBySide;
            mnuExportVideoSideBySide.Enabled = canSaveSideBySide;

            #endregion

            BuildVariablesMenu();
        }

        /// <summary>
        /// Inject the name of the working KVA file in the Save menu.
        /// </summary>
        private void ConfigureSaveMenu(AbstractScreen screen)
        {
            if (screen == null || screen.Metadata == null || string.IsNullOrEmpty(screen.Metadata.LastKVAPath))
            {
                mnuSave.Text = ScreenManagerLang.Generic_SaveKVA;
                return;
            }

            mnuSave.Text = string.Format("{0} ({1})", 
                ScreenManagerLang.Generic_SaveKVA, 
                Path.GetFileName(screen.Metadata.LastKVAPath));
        }

        private void ConfigureVideoFilterMenus(PlayerScreen player)
        {
            bool hasVideo = player != null && player.Full;
            foreach(ToolStripMenuItem menu in filterMenus)
            {
                VideoFilterType filterType = (VideoFilterType)menu.Tag;
                menu.Visible = VideoFilterFactory.GetExperimental(filterType) ? Software.Experimental : true;
                menu.Enabled = hasVideo && (filterType == VideoFilterType.None || player.IsCaching);
                menu.Checked = hasVideo && player.ActiveVideoFilterType == filterType;
            }
        }
        
        private void ConfigureImageFormatMenus(AbstractScreen screen)
        {
            // Set the enable and check prop of the image formats menu according of current screen state.
            bool canChangeAspectRatio = screen != null && screen.Full && screen is PlayerScreen && ((PlayerScreen)screen).FrameServer.VideoReader.CanChangeAspectRatio;
            mnuAspectRatio.Enabled = canChangeAspectRatio;
            mnuAspectRatioAuto.Enabled = canChangeAspectRatio;
            mnuAspectRatioForce43.Enabled = canChangeAspectRatio;
            mnuAspectRatioForce169.Enabled = canChangeAspectRatio;

            if (!canChangeAspectRatio)
                return;

            mnuAspectRatioAuto.Checked = screen.AspectRatio == ImageAspectRatio.Auto;
            mnuAspectRatioForce43.Checked = screen.AspectRatio == ImageAspectRatio.Force43;
            mnuAspectRatioForce169.Checked = screen.AspectRatio == ImageAspectRatio.Force169;
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
        
        private void ConfigureImageStabilizationMenus(AbstractScreen screen)
        {

            bool canStabilize = screen != null && screen.Full && screen is PlayerScreen && ((PlayerScreen)screen).FrameServer.VideoReader.CanStabilize;
            mnuStabilization.Enabled = canStabilize;
            if (!canStabilize)
                return;

            // Rebuild the menu on the fly since it's dependent on the metadata content.
            mnuStabilization.DropDownItems.Clear();

            // Add available tracks to the sub-menu.
            var metadata = ((PlayerScreen)screen).FrameServer.Metadata;
            bool found = false;
            foreach (var track in metadata.Tracks())
            {
                // Add one track menu.
                ToolStripMenuItem mnuStabilizationTrack = new ToolStripMenuItem();
                mnuStabilizationTrack.Text = track.Name;
                mnuStabilizationTrack.Tag = track.Id;
                mnuStabilizationTrack.Click += mnuStabilizationTrack_OnClick;
                mnuStabilizationTrack.MergeAction = MergeAction.Append;

                if (metadata.StabilizationTrack == track.Id)
                {
                    mnuStabilizationTrack.Checked = true;
                    found = true;
                }

                // Add to parent.
                mnuStabilization.DropDownItems.Add(mnuStabilizationTrack);
            }

            if (mnuStabilization.DropDownItems.Count > 0)
            {
                // Add a separator and the menu entry to forget stabilization.
                mnuStabilization.DropDownItems.Add(new ToolStripSeparator());
                mnuStabilization.DropDownItems.Add(mnuStabilizationTrackNone);
                mnuStabilizationTrackNone.Checked = !found;
            }
            else
            {
                mnuStabilization.Enabled = false;
            }
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

        private void ConfigureLensCalibrationMenus(PlayerScreen player)
        {
            // Note: the menu contains entries that are not calibration files,
            // like the menu for manual estimation, the menu for "None" or even separators.
            // We still treat them collectively and check for the "Tag" property
            // containing the filename if any. If other menus in this drop down ever
            // need to have a check or have a Tag containing a file name, we'll need
            // to identify them and ignore them.

            // Default state: all unchecked except "None".
            foreach (object m in mnuLensCalibration.DropDownItems)
            {
                ToolStripMenuItem item = m as ToolStripMenuItem;
                if (item != null)
                    item.Checked = false;
            }

            // Bail out if we don't have a lens calibration.
            DistortionParameters parameters = player?.FrameServer?.Metadata?.CalibrationHelper?.DistortionHelper?.Parameters;
            if (parameters == null)
            {
                mnuLensCalibrationNone.Checked = true;
                return;
            }

            // The parameters themselves may or may not come from a specific file.
            if (string.IsNullOrEmpty(parameters.Path))
            {
                mnuLensCalibrationNone.Checked = true;
                return;
            }

            // Special case for manual calibration. This is when we use the Manual estimation and 
            // the file isn't saved yet.
            if (parameters.Path == "::Manual")
            {
                mnuLensCalibrationManual.Checked = true;
                return;
            }
            
            // Otherwise see if there is a lens calibration menu corresponding to the currently loaded file.
            foreach (object m in mnuLensCalibration.DropDownItems)
            {
                ToolStripMenuItem item = m as ToolStripMenuItem;
                if (item == null)
                    continue;

                if ((string)item.Tag == parameters.Path)
                {
                    item.Checked = true;
                    break;
                }
            }
        }

        private void BuildLensCalibrationMenu()
        {
            mnuLensCalibration.DropDownItems.Clear();
            mnuLensCalibration.DropDownItems.Add(mnuLensCalibrationOpen);
            mnuLensCalibration.DropDownItems.Add(mnuLensCalibrationMode);
            mnuLensCalibration.DropDownItems.Add(mnuLensCalibrationManual);
            mnuLensCalibration.DropDownItems.Add(new ToolStripSeparator());
            int count = AddLensCalibrationMenus(Software.CameraCalibrationDirectory, mnuLensCalibration);
            if (count > 0)
                mnuLensCalibration.DropDownItems.Add(new ToolStripSeparator());

            mnuLensCalibration.DropDownItems.Add(mnuLensCalibrationNone);
        }

        /// <summary>
        /// Recursively browse the passed directory for lens calibration xml files.
        /// Adds sub-directories as drop-down menus and files as leaf menus.
        /// </summary>
        private int AddLensCalibrationMenus(string dir, ToolStripMenuItem menu)
        {
            if (!Directory.Exists(dir))
                return 0;

            int count = 0;
            
            // Loop sub directories.
            string[] subDirs = Directory.GetDirectories(dir);
            foreach (string subDir in subDirs)
            {
                // Create a menu
                ToolStripMenuItem mnuSubDir = new ToolStripMenuItem();
                mnuSubDir.Text = Path.GetFileName(subDir);
                mnuSubDir.Image = Properties.Resources.folder;
                mnuSubDir.MergeAction = MergeAction.Append;

                // Build sub tree.
                AddLensCalibrationMenus(subDir, mnuSubDir);

                // Add to parent if non-empty.
                if (mnuSubDir.HasDropDownItems)
                {
                    menu.DropDownItems.Add(mnuSubDir);
                    count += mnuSubDir.DropDownItems.Count;
                }
            }

            // Loop files within the sub directory.
            foreach (string file in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(file).ToLower().Equals(".xml"))
                    continue;

                // Create a menu. 
                ToolStripMenuItem mnuLensCalibrationFile = new ToolStripMenuItem();
                mnuLensCalibrationFile.Text = Path.GetFileNameWithoutExtension(file);
                mnuLensCalibrationFile.Tag = file;
                mnuLensCalibrationFile.Image = Properties.Resources.vector;
                mnuLensCalibrationFile.Click += mnuLensCalibrationFile_OnClick;
                mnuLensCalibrationFile.MergeAction = MergeAction.Append;

                // Add to parent.
                menu.DropDownItems.Add(mnuLensCalibrationFile);
                count++;
            }

            return count;
        }

        private bool ImportLensCalibration(string xmlFile)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null || !player.FrameServer.Loaded)
                return false;

            var calibHelper = player.FrameServer.Metadata.CalibrationHelper;
            DistortionParameters dp = DistortionImporterKinovea.Import(xmlFile, calibHelper.ImageSize);
            bool loaded = dp != null;
            if (loaded)
            {
                calibHelper.DistortionHelper.Initialize(dp, calibHelper.ImageSize);
                calibHelper.AfterDistortionUpdated();
                player.RefreshImage();
            }

            ConfigureLensCalibrationMenus(player);
            return loaded;
        }

        /// <summary>
        /// Rebuild the whole variables menu. 
        /// This should be done after files are added or changed externally.
        /// Does not reset the current active profile.
        /// </summary>
        private void BuildVariablesMenu()
        {
            mnuVariables.DropDownItems.Clear();
            mnuVariables.DropDownItems.Add(mnuImportVariables);

            // Bail out if there are no variables.
            if (VariablesRepository.VariableTables.Count == 0)
            {
                return;
            }

            mnuVariables.DropDownItems.Add(new ToolStripSeparator());

            foreach (var pair in VariablesRepository.VariableTables)
            {
                // Add a menu for the table.
                var mnuTable = new ToolStripMenuItem();
                mnuTable.Text = pair.Key;
                mnuTable.Tag = pair.Value;

                mnuVariables.DropDownItems.Add(mnuTable);
                VariableTable table = pair.Value;

                // TODO: it's not clear if the next/previous menus are useful or risky.
                // It's probably always better to let the user explicitly select the profile they want,
                // rather than rely on prev/next.
                //AddNextPrevMenus(mnuTable);

                // Add menus for profiles.
                foreach (var key in table.Keys)
                {
                    ToolStripMenuItem mnuProfile = new ToolStripMenuItem();
                    mnuProfile.Text = key;
                    mnuProfile.Tag = key;
                    mnuProfile.Checked = (key == table.CurrentKey);
                    mnuProfile.Click += (s, e) => {
                        table.CurrentKey = key;
                        CheckCurrentProfileKey(mnuTable);
                        VariablesRepository.SaveContext(null);
                    };

                    mnuTable.DropDownItems.Add(mnuProfile);
                }
            }
        }

        private void AddNextPrevMenus(ToolStripMenuItem mnuTable)
        {
            // Menus to move to next/previous profile.
            ToolStripMenuItem mnuNext = new ToolStripMenuItem();
            mnuNext.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuNext;
            mnuNext.Image = Properties.Resources.arrow_down2_16;
            mnuNext.Click += (s, e) => {
                MoveToNextProfile(mnuTable, true);
            };

            ToolStripMenuItem mnuPrev = new ToolStripMenuItem();
            mnuPrev.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuPrevious;
            mnuPrev.Image = Properties.Resources.arrow_up2_16;
            mnuPrev.Click += (s, e) => {
                MoveToNextProfile(mnuTable, false);
            };

            mnuTable.DropDownItems.Add(mnuPrev);
            mnuTable.DropDownItems.Add(mnuNext);
            mnuTable.DropDownItems.Add(new ToolStripSeparator());
        }

        private void MoveToNextProfile(ToolStripMenuItem mnuTable, bool down)
        {
            VariableTable table = mnuTable.Tag as VariableTable;
            if (table == null)
                return;

            // This is implemented at the menu level because the 
            // table uses a dictionary and isn't particularly sorted.
            for (int i = 0; i < mnuTable.DropDownItems.Count; i++)
            {
                ToolStripMenuItem item = mnuTable.DropDownItems[i] as ToolStripMenuItem;
                if (item == null || !(item.Tag is string) || (string)item.Tag != table.CurrentKey)
                    continue;
                
                // Found the current key, move to the next one.
                // First 3 entries are Next, Previous and separator.
                ToolStripMenuItem nextItem = null;
                if (down && ((i + 1) < mnuTable.DropDownItems.Count))
                {
                    nextItem = mnuTable.DropDownItems[i + 1] as ToolStripMenuItem;
                }
                else if (!down && ((i - 1) > 2))
                {
                    nextItem = mnuTable.DropDownItems[i - 1] as ToolStripMenuItem;
                }

                if (nextItem == null || !(nextItem.Tag is string))
                    return;

                table.CurrentKey = (string)nextItem.Tag;
                item.Checked = false;
                nextItem.Checked = true;
                break;
            }
        }

        /// <summary>
        /// Check the profile key menu corresponding to the current key.
        /// </summary>
        private void CheckCurrentProfileKey(ToolStripMenuItem mnuTable)
        {
            VariableTable table = mnuTable.Tag as VariableTable;
            if (table == null)
                return;

            // Note: the menu contains entries that are not profile entries
            // like the menu for Next, Previous, separators, etc.
            foreach (object m in mnuTable.DropDownItems)
            {
                ToolStripMenuItem item = m as ToolStripMenuItem;
                if (item != null)
                {
                    item.Checked = (item.Tag is string && (string)item.Tag == table.CurrentKey);
                }
            }
        }

        #endregion

        #region Culture
        private void RefreshCultureToolbar()
        {
            toolSave.ToolTipText = ScreenManagerLang.Generic_SaveKVA;
            toolToggleNavigationPane.ToolTipText = "Navigation pane";
            toolExplorer.ToolTipText = "File browser";
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

            mnuLoadAnnotations.Text = ScreenManagerLang.mnuLoadAnalysis;
            mnuSave.Text = ScreenManagerLang.Generic_SaveKVA;
            mnuSaveAs.Text = ScreenManagerLang.Generic_SaveKVAAs;
            mnuUnloadAnnotations.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuUnloadAnnotations;

            mnuExportVideo.Text = ScreenManagerLang.mnuExport_Video;
            mnuExportVideoVideo.Text = ScreenManagerLang.mnuExport_Video_Video;
            mnuExportVideoSlideshow.Text = ScreenManagerLang.mnuExport_Video_Slideshow;
            mnuExportVideoWithPauses.Text = ScreenManagerLang.mnuExport_Video_WithPauses;
            mnuExportVideoSideBySide.Text = ScreenManagerLang.mnuExport_Video_SideBySide;

            mnuExportImage.Text = ScreenManagerLang.mnuExport_Image;
            mnuExportImageImage.Text = ScreenManagerLang.mnuExport_Images_Image;
            mnuExportImageKeys.Text = ScreenManagerLang.mnuExport_Images_Keys;
            mnuExportImageSequence.Text = ScreenManagerLang.mnuExport_Images_Sequence;
            mnuExportImageSideBySide.Text = ScreenManagerLang.mnuExport_Images_SideBySide;

            mnuExportSpreadsheet.Text = ScreenManagerLang.mnuExport_Spreadsheet;
            mnuExportODS.Text = "LibreOffice Calc…";
            mnuExportXLSX.Text = "Microsoft Excel…";
            mnuExportCSVTrajectory.Text = ScreenManagerLang.mnuExport_Spreadsheet_TrajectoryCSV;
            mnuExportCSVChronometer.Text = ScreenManagerLang.mnuExport_Spreadsheet_ChronoCSV;
            mnuExportTXTTrajectory.Text = "Trajectory text…";
            mnuExportJSON.Text = "JSON…";

            mnuExportDocument.Text = ScreenManagerLang.mnuExport_Document;
            mnuExportODT.Text = "LibreOffice Writer…";
            mnuExportDOCX.Text = "Microsoft Word…";
            mnuExportMarkdown.Text = "Markdown…";

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
            mnuToggleCommonCtrls.Text = ScreenManagerLang.mnuToggleCommonCtrls;
            mnuVerticalLayout.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuVerticalLayout;
            mnuSwapScreens.Text = ScreenManagerLang.mnuSwapScreens;

            // Image
            mnuAspectRatio.Text = ScreenManagerLang.mnuFormat;
            mnuAspectRatioAuto.Text = ScreenManagerLang.mnuFormatAuto;
            mnuAspectRatioForce43.Text = ScreenManagerLang.mnuFormatForce43;
            mnuAspectRatioForce169.Text = ScreenManagerLang.mnuFormatForce169;

            mnuRotation.Text = ScreenManagerLang.mnuRotation;
            mnuRotation0.Text = ScreenManagerLang.mnuRotation0;
            mnuRotation90.Text = ScreenManagerLang.mnuRotation90;
            mnuRotation180.Text = ScreenManagerLang.mnuRotation180;
            mnuRotation270.Text = ScreenManagerLang.mnuRotation270;

            mnuMirror.Text = ScreenManagerLang.mnuMirror;
            mnuDeinterlace.Text = ScreenManagerLang.mnuDeinterlace;

            mnuDemosaic.Text = ScreenManagerLang.mnuDemosaic;
            mnuDemosaicNone.Text = ScreenManagerLang.mnuDemosaicNone;
            mnuDemosaicRGGB.Text = "RGGB";
            mnuDemosaicBGGR.Text = "BGGR";
            mnuDemosaicGRBG.Text = "GRBG";
            mnuDemosaicGBRG.Text = "GBRG";

            mnuStabilization.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuStabilization;
            mnuStabilizationTrackNone.Text = ScreenManagerLang.ScreenManagerKernel_LensCalibration_None;

            RefreshCultureMenuFilters();

            // Tools
            mnuImportImage.Text = ScreenManagerLang.mnuImportImage;
            mnuBackground.Text = ScreenManagerLang.PlayerScreenUserInterface_Background;
            mnuTestGrid.Text = ScreenManagerLang.DrawingName_TestGrid;
            mnuTimeCalibration.Text = ScreenManagerLang.mnuTimeCalibration;
            mnuLensCalibration.Text = ScreenManagerLang.mnuLensCalibration;
            mnuLensCalibrationOpen.Text = ScreenManagerLang.Generic_Import;
            mnuLensCalibrationMode.Text = ScreenManagerLang.ScreenManagerKernel_LensCalibration_LensCalibrationMode;
            mnuLensCalibrationManual.Text = ScreenManagerLang.ScreenManagerKernel_LensCalibration_ManualEstimation;
            mnuLensCalibrationNone.Text = ScreenManagerLang.ScreenManagerKernel_LensCalibration_None;
            mnuCoordinateSystem.Text = ScreenManagerLang.mnuCoordinateSystem;
            mnuCalibrationValidation.Text = ScreenManagerLang.ScreenManagerKernel_LensCalibration_CalibrationValidation;
            mnuScatterDiagram.Text = ScreenManagerLang.DataAnalysis_ScatterDiagram + "…";
            mnuLinearKinematics.Text = ScreenManagerLang.DataAnalysis_LinearKinematics + "…";
            mnuAngularKinematics.Text = ScreenManagerLang.DataAnalysis_AngularKinematics + "…";
            mnuAngleAngleDiagram.Text = ScreenManagerLang.DataAnalysis_AngleAngleDiagrams + "…";

            // Options
            mnuVariables.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.mnuContext;
            mnuImportVariables.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.Generic_Import;
        }

        private void RefreshCultureMenuFilters()
        {
            foreach(ToolStripMenuItem menu in filterMenus)
            {
                VideoFilterType filterType = (VideoFilterType)menu.Tag;
                menu.Text = VideoFilterFactory.GetFriendlyName(filterType);
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
        private void mnuLoadAnnotationsOnClick(object sender, EventArgs e)
        {
            if (activeScreen != null)
            {
                int index = activeScreen == screenList[0] ? 0 : 1;
                LoadAnnotations(index);
            }
        }

        private void mnuUnloadAnnotationsOnClick(object sender, EventArgs e)
        {
            if (activeScreen == null)
                return;
            
            int index = activeScreen == screenList[0] ? 0 : 1;
            if (screenList[index] is PlayerScreen)
                DoStopPlaying();

            screenList[index].UnloadAnnotations();
        }

        /// <summary>
        /// Open a dialog box to select an annotation file.
        /// </summary>
        private void LoadAnnotations(int targetScreen)
        {
            if (screenList[targetScreen] == null)
                return;

            if (screenList[targetScreen] is PlayerScreen)
                DoStopPlaying();

            string title = ScreenManagerLang.dlgLoadAnalysis_Title;
            string filter = FilesystemHelper.OpenKVAFilter(ScreenManagerLang.FileFilter_AllSupported);
            string filename = FilePicker.OpenAnnotations(title, filter);
            if (filename == null)
                return;

            screenList[targetScreen].LoadKVA(filename);
        }
        private void mnuSaveOnClick(object sender, EventArgs e)
        {
            if (activeScreen == null)
                return;

            DoStopPlaying();
            activeScreen.SaveAnnotations();
        }

        private void mnuSaveAsOnClick(object sender, EventArgs e)
        {
            if (activeScreen == null)
                return;

            DoStopPlaying();
            activeScreen.SaveAnnotationsAs();
        }

        // TODO: save as default.
        // TODO: unload.

        private void ExportVideo(VideoExportFormat format)
        {
            DoStopPlaying();
            if (format == VideoExportFormat.SideBySide)
            {
                // For side-by-side we don't care about the active screen but the order is important.
                AbstractScreen screen0 = GetScreenAt(0);
                AbstractScreen screen1 = GetScreenAt(1);
                PlayerScreen player1 = screen0 as PlayerScreen;
                PlayerScreen player2 = screen1 as PlayerScreen;
                if (player1 == null || player2 == null)
                    return;

                if (!player1.Full || !player2.Full)
                    return;

                VideoExporter exporter = new VideoExporter();
                exporter.Export(format, player1, player2, dualPlayer);
            }
            else
            {
                PlayerScreen player = activeScreen as PlayerScreen;
                if (player == null)
                    return;

                VideoExporter exporter = new VideoExporter();
                exporter.Export(format, player, null, null);
            }
        }

        private void ExportImages(ImageExportFormat format)
        {
            DoStopPlaying();
            if (format == ImageExportFormat.SideBySide)
            {
                // For side-by-side we don't care about the active screen but the order is important.
                AbstractScreen screen0 = GetScreenAt(0);
                AbstractScreen screen1 = GetScreenAt(1);
                PlayerScreen player1 = screen0 as PlayerScreen;
                PlayerScreen player2 = screen1 as PlayerScreen;
                if (player1 == null || player2 == null)
                    return;

                if (!player1.Full || !player2.Full)
                    return;

                ImageExporter exporter = new ImageExporter();
                exporter.Export(format, player1, player2);
            }
            else
            {
                PlayerScreen player = activeScreen as PlayerScreen;
                if (player == null)
                    return;

                ImageExporter exporter = new ImageExporter();
                exporter.Export(format, player, null);
            }
        }

        private void ExportDocument(DocumentExportFormat format)
        {
            DoStopPlaying();
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;

            DocumentExporter exporter = new DocumentExporter();
            exporter.Export(format, player);
        }

        private void ExportSpreadsheet(SpreadsheetExportFormat format)
        {
            DoStopPlaying();
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null || !player.FrameServer.Metadata.HasVisibleData)
                return;

            SpreadsheetExporter exporter = new SpreadsheetExporter();
            exporter.Export(format, player);
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
        private void toolToggleNavigationPanel_Click(object sender, EventArgs e)
        {
            NotificationCenter.RaiseToggleNavigationPane();
        }

        private void mnuExplorer_OnClick(object sender, EventArgs e)
        {
            // Remove all screens.
            if (screenList.Count <= 0)
                return;

            if (ScreenRemover.RemoveScreen(this, 0))
            {
                // Second screen is now in [0] spot.
                if (screenList.Count > 0)
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

        private void mnuVerticalLayout_Click(object sender, EventArgs e)
        {
            view.ToggleDualScreenOrientation();
            mnuVerticalLayout.Checked = view.IsTopBottomLayout;
        }


        private void mnuSwapScreensOnClick(object sender, EventArgs e)
        {
            if (screenList.Count != 2)
                return;

            SwapScreens();
            OrganizeScreens();
            OrganizeMenus();
            NotificationCenter.RaiseUpdateStatus();

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

            mnuAspectRatioForce43.Checked = aspect == ImageAspectRatio.Force43;
            mnuAspectRatioForce169.Checked = aspect == ImageAspectRatio.Force169;
            mnuAspectRatioAuto.Checked = aspect == ImageAspectRatio.Auto;
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
        private void mnuMirror_Click(object sender, EventArgs e)
        {
            if (activeScreen == null)
                return;

            mnuMirror.Checked = !mnuMirror.Checked;
            activeScreen.Mirrored = mnuMirror.Checked;
        }

        private void mnuStabilizationTrack_OnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null)
                return;

            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu == null)
                return;

            Guid id = (Guid)menu.Tag;
            player.StabilizationTrack = id;
        }

        private void mnuBackgroundColor_Click(object sender, EventArgs e)
        {
            // TODO: implement for both Playback and Capture screen.
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player != null)
            {
                // Launch Background color/opacity dialog.
                Color memo = player.BackgroundColor;
                FormBackgroundColor fbc = new FormBackgroundColor(player.FrameServer.Metadata, player.view);
                fbc.StartPosition = FormStartPosition.CenterScreen;
                fbc.ShowDialog();
                if (fbc.DialogResult != DialogResult.OK)
                {
                    player.BackgroundColor = memo;
                }

                fbc.Dispose();
            }
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
                bool isSVG = Path.GetExtension(openFileDialog.FileName).ToLower() == ".svg";
                LoadDrawing(openFileDialog.FileName, isSVG);
            }
        }
        private void LoadDrawing(string path, bool isSVG)
        {
            if(path != null && path.Length > 0 && activeScreen != null && activeScreen.CapabilityDrawings)
            {
                activeScreen.AddImageDrawing(path, isSVG);
            }
        }
        private void mnuCoordinateSystem_OnClick(object sender, EventArgs e)
        {
            mnuCoordinateSystem.Checked = !mnuCoordinateSystem.Checked;
            activeScreen.CoordinateSystemVisible = mnuCoordinateSystem.Checked;
            activeScreen.RefreshImage();
        }

        private void mnuTestGrid_OnClick(object sender, EventArgs e)
        {
            mnuTestGrid.Checked = !mnuTestGrid.Checked;
            activeScreen.TestGridVisible = mnuTestGrid.Checked;
            activeScreen.RefreshImage();
        }

        private void mnuCalibrationValidation_OnClick(object sender, EventArgs e)
        {
            // Collect the metadata of both screens for 3D validation when available.
            PlayerScreen thisScreen = null;
            PlayerScreen otherScreen = null;
            foreach (AbstractScreen screen in screenList)
            {
                var ps = screen as PlayerScreen;
                if (ps == null)
                    continue;

                if (ps == activeScreen)
                {
                    thisScreen = ps;
                }
                else
                {
                    otherScreen = ps;
                }
            }

            if (thisScreen == null)
                return;

            thisScreen.ShowCalibrationValidation(otherScreen);
        }

        private void mnuScatterDiagram_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowScatterDiagram();
        }
        private void mnuLinearKinematics_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowLinearKinematics();
        }
        private void mnuAngularKinematics_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowAngularKinematics();
        }
        private void mnuAngleAngleDiagram_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowAngleAngleDiagram();
        }
        #endregion

        #region Tools
        private void mnuTimebase_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps != null)
                ps.ConfigureTimebase();
        }

        private void mnuLensCalibrationFile_OnClick(object sender, EventArgs e)
        {
            // One of the dynamically added lens calibration file menu has been clicked.
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu != null)
            {
                string xmlFile = menu.Tag as string;

                bool loaded = ImportLensCalibration(xmlFile);
                // TODO: Alert if couldn't be loaded.
                // TODO: Toast if successfuly loaded.
            }
        }

        private void mnuLensCalibrationOpen_OnClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_OpenDialogTitle;
            openFileDialog.Filter = FilesystemHelper.OpenXMLFilter();
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            string path = openFileDialog.FileName;
            bool loaded = ImportLensCalibration(path);
            // TODO: Alert if couldn't be loaded.
            // TODO: Toast if successfuly loaded.

            // Copy this file to the special folder if it doesn't exist yet,
            // this way it will be available directly from the menu from now on.
            string target = Path.Combine(Software.CameraCalibrationDirectory, Path.GetFileName(path));
            if (loaded && !File.Exists(target))
            {
                File.Copy(path, target);
                BuildLensCalibrationMenu();
                OrganizeMenus();
            }
        }

        private void mnuLensCalibrationManual_OnClick(object sender, EventArgs e)
        {
            PlayerScreen ps = activeScreen as PlayerScreen;
            if (ps == null)
                return;

            ps.ShowCameraCalibration();
            ConfigureLensCalibrationMenus(ps);
        }

        private void mnuLensCalibrationMode_OnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null || !player.FrameServer.Loaded)
                return;

            player.ActivateVideoFilter(VideoFilterType.LensCalibration);
        }

        private void mnuLensCalibrationNone_OnClick(object sender, EventArgs e)
        {
            PlayerScreen player = activeScreen as PlayerScreen;
            if (player == null || !player.FrameServer.Loaded)
                return;

            var calibHelper = player.FrameServer.Metadata.CalibrationHelper;
            calibHelper.DistortionHelper.Uninitialize();
            calibHelper.AfterDistortionUpdated();
            player.RefreshImage();
            ConfigureLensCalibrationMenus(player);
        }
        #endregion

        #region Options
        private void mnuImportVariables_OnClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Import variables";
            openFileDialog.Filter = FilesystemHelper.OpenVariableTableFilter(ScreenManagerLang.FileFilter_AllSupported);
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.VariablesDirectory;

            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            // "Importing" a variable table means we copy the file to the variables directory and reload.
            // Copy the file to the variables directory.
            string filename = Path.GetFileName(openFileDialog.FileName);
            string target = Path.Combine(Software.VariablesDirectory, filename);
            bool canCopy = true;
            bool inPlace = target == openFileDialog.FileName;
            if (File.Exists(target) && !inPlace)
            {
                // Ask for confirmation.
                //string msgTitle = "File already exists";
                string msgTitle = ScreenManagerLang.Error_Capture_FileExists_Title;
                string msgText = String.Format(ScreenManagerLang.Error_Capture_FileExists_Text, filename).Replace("\\n", "\n");

                DialogResult result = MessageBox.Show(msgText, msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result != DialogResult.Yes)
                    canCopy = false;
            }
            
            if (!canCopy)
            {
                log.Warn("Import of Variable table cancelled.");
                return;
            }

            if (!inPlace)
            {
                File.Copy(openFileDialog.FileName, target, true);
            }
            
            // Load the variable table.
            VariablesRepository.LoadFile(target);
            OrganizeMenus();

            // Alert the other instances that they need to reload the variables from the file system.
            NotificationCenter.RaiseTriggerPreferencesUpdated(false);
            WindowManager.SendMessage("Kinovea:Window.VariableTableImported");
        }
        #endregion
        #endregion

        #region Services
        private void NotificationCenter_LoadVideoAsked(object sender, VideoLoadAskedEventArgs e)
        {
            DoLoadVideoInScreen(e.Path, e.Target);
        }

        private void DoLoadVideoInScreen(string path, int targetScreen)
        {
            if (FilesystemHelper.IsReplayWatcher(path))
            {
                ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                sdp.FullPath = path;
                sdp.IsReplayWatcher = true;
                sdp.Autoplay = true;
                sdp.Stretch = true;
                sdp.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
                LoaderVideo.LoadVideoInScreen(this, path, sdp);
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
                    log.DebugFormat("Loading a new video in an existing screen.");

                    // Assume empty screen for starters.
                    ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                    sdp.FullPath = path;
                    sdp.IsReplayWatcher = false;
                    sdp.Autoplay = false;
                    sdp.Stretch = false;
                    sdp.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;

                    // Here we detect the case where we load a video in an existing player screen.
                    // We keep the existing screen descriptor as much as possible.
                    // This includes keeping a watcher watching the original folder, even if we just 
                    // loaded a video from a completely different place. 
                    // This is to make it possible to quickly review older videos without stopping the watcher.
                    // This should also generally handle the case of carrying over the screen state into the 
                    // next video, like the speed slider.
                    AbstractScreen screen = GetScreenAt(targetScreen);
                    if (screen != null && screen is PlayerScreen && screen.Full)
                    {
                        var view = ((PlayerScreen)screen).view;
                        if (view.ScreenDescriptor != null)
                        {
                            sdp = view.ScreenDescriptor;
                        
                            // If we are NOT a watcher though, we must update to reflect the actual file.
                            if (!sdp.IsReplayWatcher)
                                sdp.FullPath = path;

                            // Update to latest state.
                            sdp.Stretch = view.ImageFill;
                            sdp.SpeedPercentage = view.SpeedPercentage;
                        }
                    }

                    LoaderVideo.LoadVideoInScreen(this, path, targetScreen, sdp);
                }
            }
        }

        private void DoLoadCameraInScreen(CameraSummary summary, int targetScreen)
        {
            if(summary == null)
                return;

            // Manually load a camera in a screen.
            ScreenDescriptorCapture sdc;
            
            // Initialize with the backup configuration if possible.
            // If we are loading on top of a full screen we will swap for that later.
            var wd = WindowManager.ActiveWindow;
            if (wd.ScreenDescriptorCaptureBackup != null)
            {
                sdc = (ScreenDescriptorCapture)wd.ScreenDescriptorCaptureBackup.Clone();
            }
            else
            {
                // Very first load of a camera in this screen.
                sdc = new ScreenDescriptorCapture();
                sdc.FileName = PreferencesManager.CapturePreferences.CapturePathConfiguration.DefaultFileName;
            }

            LoaderCamera.LoadCameraInScreen(this, summary, targetScreen, sdc);
        }

        private void DoStopPlaying()
        {
            foreach (PlayerScreen player in playerScreens)
                player.StopPlaying();

            dualPlayer.Pause();
        }

        private void View_FileLoadAsked(object source, FileLoadAskedEventArgs e)
        {
            DoLoadVideoInScreen(e.Source, e.Target);
        }

        private void CameraTypeManager_CameraLoadAsked(object source, CameraLoadAskedEventArgs e)
        {
            DoLoadCameraInScreen(e.Source, e.Target);
        }

        private void View_AutoLaunchAsked(object source, EventArgs e)
        {
            var screenDescriptors = WindowManager.ActiveWindow.ScreenList;
            int count = screenDescriptors.Count;
            if (count > 2)
            {
                log.ErrorFormat("More than two screen descriptors.");
                screenDescriptors.RemoveRange(2, count - 2);
                count = 2;
            }

            // Start by collecting the list of cameras to be found.
            // We will keep the camera discovery system active until we have found all of them or time out.
            camerasToDiscover.Clear();
            foreach (IScreenDescriptor sd in screenDescriptors)
            {
                if (sd is ScreenDescriptorCapture)
                    camerasToDiscover.Add(((ScreenDescriptorCapture)sd).CameraName);
            }

            if (camerasToDiscover.Count == 0)
                CameraTypeManager.StopDiscoveringCameras();

            int added = 0;
            autoLaunchInProgress = true;
            foreach (IScreenDescriptor sd in screenDescriptors)
            {
                // Note: the screens should work with copies of the screen descriptors, 
                // to avoid saving the screen descriptor when they shouldn't. 
                // If the window is in "specific screens" startup mode we should not 
                // save descriptors except from within the Window properties dialog.
                // Plus an exception for the post-recording command.
                
                if (sd is ScreenDescriptorCapture)
                {
                    AddCaptureScreen();
                    ScreenDescriptorCapture sdc = sd.Clone() as ScreenDescriptorCapture;
                    CameraSummary summary = new CameraSummary(sdc.CameraName);

                    int targetScreen = added == 0 ? 0 : 1;
                    LoaderCamera.LoadCameraInScreen(this, summary, targetScreen, sdc);
                    added++;
                }
                else if (sd is ScreenDescriptorPlayback)
                {
                    AddPlayerScreen();
                    ScreenDescriptorPlayback sdp = sd.Clone() as ScreenDescriptorPlayback;
                    LoaderVideo.LoadVideoInScreen(this, sdp.FullPath, sdp);
                    added++;
                }
            }

            autoLaunchInProgress = false;

            if (added > 0)
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

        private void NotificationCenter_PreferencesOpened(object sender, EventArgs e)
        {
            audioInputLevelMonitor.Enabled = false;
            udpMonitor.Enabled = false;
        }

        private void NotificationCenter_ReceivedExternalCommand(object source, EventArgs<string> e)
        {
            // Parses the payload of the external command string and send it to the correct handler.
            // The payload is in the form "<Handler>.<Command>", for example "CaptureScreen.ToggleRecording".

            string[] tokens = e.Value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
            {
                log.ErrorFormat("Malformed external command. \"{0\"}", e.Value);
                return;
            }

            switch (tokens[0])
            {
                case "Window":
                    // Handled in Kernel.cs.
                    break;
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

        private void AddScreen(AbstractScreen screen)
        {
            // We are about to add a new screen, signal it to a potential existing capture screen for buffer memory management.
            foreach (CaptureScreen captureScreen in captureScreens)
                captureScreen.SetShared(true);

            AddScreenEventHandlers(screen);
            screenList.Add(screen);
            IdentifyScreens();
        }
        private void AddScreenEventHandlers(AbstractScreen screen)
        {
            screen.Activated += Screen_Activated;
            screen.LoadAnnotationsAsked += Screen_LoadAnnotationsAsked;
            screen.DualCommandReceived += Screen_DualCommandReceived;
            screen.CloseAsked += Screen_CloseAsked;

            if (screen is PlayerScreen)
                AddPlayerScreenEventHandlers(screen as PlayerScreen);
            else if (screen is CaptureScreen)
                AddCaptureScreenEventHandlers(screen as CaptureScreen);
        }
        private void AddPlayerScreenEventHandlers(PlayerScreen screen)
        {
            screen.OpenVideoAsked += Player_OpenVideoAsked;
            screen.OpenReplayWatcherAsked += Player_OpenReplayWatcherAsked;
            screen.Loaded += Player_Loaded;
            screen.SelectionChanged += Player_SelectionChanged;
            screen.KVAImported += Player_KVAImported;
            screen.FilterExited += Player_FilterExited;
            screen.ResetAsked += Player_ResetAsked;
            screen.DrawingAdded += Player_DrawingAdded;
        }
        private void AddCaptureScreenEventHandlers(CaptureScreen screen)
        {
            screen.CameraDiscoveryComplete += Capture_CameraDiscoveryComplete;
        }
        private void RemoveScreenEventHandlers(AbstractScreen screen)
        {
            screen.Activated -= Screen_Activated;
            screen.LoadAnnotationsAsked -= Screen_LoadAnnotationsAsked;
            screen.DualCommandReceived -= Screen_DualCommandReceived;
            screen.CloseAsked -= Screen_CloseAsked;

            if (screen is PlayerScreen)
                RemovePlayerScreenEventHandlers(screen as PlayerScreen);
            else if (screen is CaptureScreen)
                RemoveCaptureScreenEventHandlers(screen as CaptureScreen);

        }
        private void RemovePlayerScreenEventHandlers(PlayerScreen screen)
        {
            screen.OpenVideoAsked -= Player_OpenVideoAsked;
            screen.OpenReplayWatcherAsked -= Player_OpenReplayWatcherAsked;
            screen.SelectionChanged -= Player_SelectionChanged;
            screen.KVAImported -= Player_KVAImported;
            screen.FilterExited -= Player_FilterExited;
            screen.ResetAsked -= Player_ResetAsked;
            screen.DrawingAdded -= Player_DrawingAdded;
        }

        private void RemoveCaptureScreenEventHandlers(CaptureScreen screen)
        {
            screen.CameraDiscoveryComplete -= Capture_CameraDiscoveryComplete;
        }

        #endregion
    }
}
