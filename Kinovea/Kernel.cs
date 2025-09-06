#region Licence
/*
Copyright � Joan Charmant 2008-2009.
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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Kinovea.FileBrowser;
using Kinovea.Root.Languages;
using Kinovea.ScreenManager;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Updater;
using Kinovea.Video;
using Kinovea.Camera;
using System.Linq;

namespace Kinovea.Root
{
    public class RootKernel : IKernel 
    {
        #region Properties
        public ScreenManagerKernel ScreenManager
        {
            get { return screenManager; }
        }
        #endregion
        
        #region Members
        private KinoveaMainWindow mainWindow;
        private FileBrowserKernel navigationPanel;
        private UpdaterKernel updater;
        private ScreenManagerKernel screenManager;
        private Stopwatch stopwatch = new Stopwatch();
        
        #region Menus

        // File
        private ToolStripMenuItem mnuFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenReplayWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHistory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHistoryReset = new ToolStripMenuItem();
        private ToolStripMenuItem mnuQuit = new ToolStripMenuItem();

        // Edit
        private ToolStripMenuItem mnuEdit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUndo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRedo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuView = new ToolStripMenuItem();
        public ToolStripMenuItem mnuToggleNavigationPanel = new ToolStripMenuItem();
        public ToolStripMenuItem mnuFullScreen = new ToolStripMenuItem();

        // Image
        private ToolStripMenuItem mnuImage = new ToolStripMenuItem();

        // Video
        private ToolStripMenuItem mnuVideo = new ToolStripMenuItem();

        // Tools
        private ToolStripMenuItem mnuTools = new ToolStripMenuItem();

        // Window
        private ToolStripMenuItem mnuWindow = new ToolStripMenuItem();
        private ToolStripMenuItem mnuWindowProperties = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenNewWindow = new ToolStripMenuItem();
        private ToolStripMenuItem mnuReopenWindow = new ToolStripMenuItem();
        private ToolStripMenuItem mnuManageWindows = new ToolStripMenuItem();

        // Options
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLanguages = new ToolStripMenuItem();
        private Dictionary<string, ToolStripMenuItem> languageMenus = new Dictionary<string, ToolStripMenuItem>();
        private ToolStripMenuItem mnuTranslate1 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTranslate2 = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPreferences = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecode = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeClassic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeMilliseconds = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeMicroseconds = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeTimeAndFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeNormalized = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPointer = new ToolStripMenuItem();

        // Help
        private ToolStripMenuItem mnuHelp = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHelpContents = new ToolStripMenuItem();
        private ToolStripMenuItem mnuApplicationFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuEnableDebugLogs = new ToolStripMenuItem();
        private ToolStripMenuItem mnuWebsite = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAbout = new ToolStripMenuItem();
        #endregion
        
        private ToolStripButton toolOpenFile = new ToolStripButton();
        private ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public RootKernel()
        {
            bool enableVideoReaders = true;
            bool enableCameraManagers = true;
            bool enableTools = true;
            bool enableCursors = true;

            if (enableVideoReaders)
            {
                List<Type> videoReaders = new List<Type>();
                log.Debug("Loading video readers.");
                videoReaders.Add(typeof(Video.Bitmap.VideoReaderBitmap));
                videoReaders.Add(typeof(Video.FFMpeg.VideoReaderFFMpeg));
                videoReaders.Add(typeof(Video.GIF.VideoReaderGIF));
                videoReaders.Add(typeof(Video.SVG.VideoReaderSVG));
                videoReaders.Add(typeof(Video.Synthetic.VideoReaderSynthetic));
                VideoTypeManager.LoadVideoReaders(videoReaders);
            }

            if (enableCameraManagers)
            {
                log.Debug("Loading built-in camera managers.");
                CameraTypeManager.LoadCameraManager(typeof(Camera.DirectShow.CameraManagerDirectShow));
                CameraTypeManager.LoadCameraManager(typeof(Camera.HTTP.CameraManagerHTTP));
                CameraTypeManager.LoadCameraManager(typeof(Camera.FrameGenerator.CameraManagerFrameGenerator));

                log.Debug("Loading camera managers plugins.");
                CameraTypeManager.LoadCameraManagersPlugins();
            }

            if (enableTools)
            {
                log.Debug("Loading tools.");
                ToolManager.LoadTools();
            }
            
            if (enableCursors)
            {
                log.Debug("Loading cursors.");
                PointerManager.LoadPointers();
            }

            BuildSubTree();
            mainWindow = new KinoveaMainWindow(this);
            NotificationCenter.RecentFilesChanged += NotificationCenter_RecentFilesChanged;
            NotificationCenter.FullScreenToggle += NotificationCenter_FullscreenToggle;
            NotificationCenter.StatusUpdated += (s, e) => statusLabel.Text = e.Status;
            NotificationCenter.PreferenceTabAsked += NotificationCenter_PreferenceTabAsked;
            NotificationCenter.WakeUpAsked += NotificationCenter_WakeUpAsked;
            NotificationCenter.ExternalCommand += NotificationCenter_ExternalCommand;

            log.Debug("Plug sub modules at UI extension points (Menus, Toolbars, Statusbar, Windows).");
            ExtendMenu(mainWindow.menuStrip);
            ExtendToolBar(mainWindow.toolStrip);
            ExtendStatusBar(mainWindow.statusStrip);
            ExtendUI();

            log.Debug("Register global services offered at Root level.");
            
            FormsHelper.SetMainForm(mainWindow);
        }
        #endregion

        #region Prepare & Launch
        public void Prepare()
        {
            log.Debug("Setting current ui culture.");
            Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
            RefreshUICulture();
            CheckLanguageMenu();
            CheckTimecodeMenu();
        }
        public void Launch()
        {
            screenManager.RecoverCrash();
            
            log.Debug("Calling Application.Run() to boot up the UI.");
            Application.Run(mainWindow);
        }
        #endregion
        
        #region IKernel Implementation
        public void BuildSubTree()
        {   
            stopwatch.Restart();
            log.Debug("Building the modules tree.");
            navigationPanel = new FileBrowserKernel();
            updater = new UpdaterKernel();
            screenManager = new ScreenManagerKernel();
            log.DebugFormat("Modules tree built in {0} ms.", stopwatch.ElapsedMilliseconds);
        }
        public void ExtendMenu(ToolStrip menu)
        {
            menu.AllowMerge = true;
            GetModuleMenus(menu);
            GetSubModulesMenus(menu);
        }
        public void ExtendToolBar(ToolStrip toolbar)
        {
            toolbar.AllowMerge = true;
            GetModuleToolBar(toolbar);
            GetSubModulesToolBars(toolbar);
            toolbar.Visible = true;
        }
        public void ExtendStatusBar(ToolStrip statusbar)
        {
            if(statusbar != null)
            {
                // This level
                statusLabel = new ToolStripStatusLabel();
                statusLabel.ForeColor = Color.White;
                statusbar.Items.AddRange(new ToolStripItem[] { statusLabel });
            }
        }
        public void ExtendUI()
        {
            navigationPanel.ExtendUI();
            updater.ExtendUI();
            screenManager.ExtendUI();

            mainWindow.PlugUI(navigationPanel.UI, screenManager.UI);
            mainWindow.SupervisorControl.buttonCloseExplo.BringToFront();
        }
        public void RefreshUICulture()
        {
            log.Debug("RefreshUICulture - Reload localized strings for the whole tree.");
            RefreshCultureMenu();
            CheckLanguageMenu();
            CheckTimecodeMenu();
            
            toolOpenFile.ToolTipText = ScreenManagerLang.mnuOpenVideo;
            
            navigationPanel.RefreshUICulture();
            updater.RefreshUICulture();
            screenManager.RefreshUICulture();
            
            log.Debug("RefreshUICulture - Whole tree culture reloaded.");
        }

        public void PreferencesUpdated()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// After the preferences are updated asks all modules to refresh their UI.
        /// Optionally send the alert to all other windows as well.
        /// </summary>
        private void PreferencesUpdated(bool sendMessage)
        {
            RefreshUICulture();
            
            navigationPanel.PreferencesUpdated();
            updater.PreferencesUpdated();
            screenManager.PreferencesUpdated();

            if (sendMessage)
            {
                WindowManager.SendMessage("Kinovea:Window.PreferencesUpdated");
            }
        }
        public bool CloseSubModules()
        {
            log.Debug("Root is closing. Call close on all sub modules.");
            bool cancel = screenManager.CloseSubModules();
            if(!cancel)
            {
                navigationPanel.CloseSubModules();
                updater.CloseSubModules();
            }

            return cancel;
        }
        #endregion

        #region Extension point helpers
        private void GetModuleMenus(ToolStrip menu)
        {
            // Affectation of .Text property happens in RefreshCultureMenu
            
            #region File
            mnuFile.MergeAction = MergeAction.Append;
            mnuOpenFile.Image = Properties.Resources.folder;
            mnuOpenFile.ShortcutKeys = Keys.Control | Keys.O;
            mnuOpenFile.Click += mnuOpenFileOnClick;

            mnuOpenReplayWatcher.Image = Properties.Resources.user_detective;
            mnuOpenReplayWatcher.Click += mnuOpenReplayWatcherOnClick;

            mnuHistory.Image = Properties.Resources.time;
            
            NotificationCenter.RaiseRecentFilesChanged(this);
            mnuHistoryReset.Image = Properties.Resources.bin_empty;
            mnuHistoryReset.Click += mnuHistoryResetOnClick;
            
            mnuQuit.Image = Properties.Resources.close2_16;
            mnuQuit.Click += new EventHandler(menuQuitOnClick);

            // The indices are used by the other modules to insert their menus.
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] {
                mnuOpenFile,                    // 0
                mnuOpenReplayWatcher,           // 1
                mnuHistory,                     // 2
                new ToolStripSeparator(),       // 3
                // Load annotations,            // 4
                // Save annotations,            // 5
                // Save annotations as,         // 6
                // Save as default annotations, // 7
                // Unload annotations,          // 8
                new ToolStripSeparator(),       // 9
                // Export video,                // 10
                // Export image,                // 11
                // Export spreadsheet,          // 12
                // Export document,             // 13
                new ToolStripSeparator(),       // 14
                // Close A,                     // 15
                // Close B,                     // 16
                new ToolStripSeparator(),       // 17
                mnuQuit                         // 18
                });
            
            #endregion

            #region Edit
            mnuEdit.MergeAction = MergeAction.Append;
            
            mnuUndo.Tag = RootLang.ResourceManager;
            mnuUndo.Image = Properties.Resources.arrow_undo;
            mnuUndo.ShortcutKeys = Keys.Control | Keys.Z;
            mnuUndo.Enabled = false;

            mnuRedo.Tag = RootLang.ResourceManager;
            mnuRedo.Image = Properties.Resources.arrow_redo;
            mnuRedo.ShortcutKeys = Keys.Control | Keys.Y;
            mnuRedo.Enabled = false;

            HistoryMenuManager.RegisterMenus(mnuUndo, mnuRedo);
            
            mnuEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuUndo, mnuRedo });
            #endregion

            #region View
            mnuToggleNavigationPanel.Image = Properties.Resources.explorer;
            mnuToggleNavigationPanel.Checked = true;
            mnuToggleNavigationPanel.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuToggleNavigationPanel.ShortcutKeys = Keys.F4;
            mnuToggleNavigationPanel.Click += mnuToggleNavigationPanel_Click;
            mnuFullScreen.Image = Properties.Resources.fullscreen;
            mnuFullScreen.ShortcutKeys = Keys.F11;
            mnuFullScreen.Click += new EventHandler(mnuFullScreenOnClick);
            
            mnuView.DropDownItems.AddRange(new ToolStripItem[] { mnuToggleNavigationPanel, mnuFullScreen, new ToolStripSeparator() });
            #endregion

            #region Window
            mnuWindowProperties.Image = Properties.Resources.application_form;
            mnuOpenNewWindow.Image = Properties.Resources.application_add;
            mnuReopenWindow.Image = Properties.Resources.application_cascade;
            mnuManageWindows.Image = Properties.Resources.application_edit;
            BuildReopenWindowsMenus();

            mnuWindowProperties.Click += mnuWindowProperties_Click;
            mnuOpenNewWindow.Click += mnuOpenNewWindow_Click;
            mnuManageWindows.Click += mnuManageWindows_Click;

            mnuWindow.DropDownItems.AddRange(new ToolStripItem[] {
                mnuWindowProperties,
                new ToolStripSeparator(),
                mnuOpenNewWindow,
                mnuReopenWindow,
                mnuManageWindows});

            #endregion

            #region Options
            mnuLanguages.Image = Properties.Resources.international;
            foreach(KeyValuePair<string, string> lang in LanguageManager.Languages)
            {
                ToolStripMenuItem mnuLang = new ToolStripMenuItem(lang.Value);
                mnuLang.Tag = lang.Key;
                mnuLang.Click += mnuLanguage_OnClick;
                languageMenus.Add(lang.Key, mnuLang);
                mnuLanguages.DropDownItems.Add(mnuLang);
            }
            mnuTranslate1.Image = Properties.Resources.international;
            mnuTranslate1.Click += (s, e) => Process.Start("https://hosted.weblate.org/engage/kinovea/");
            mnuLanguages.DropDownItems.Add(new ToolStripSeparator());
            mnuLanguages.DropDownItems.Add(mnuTranslate1);

            mnuPreferences.Image = Properties.Resources.wrench;
            mnuPreferences.Click += new EventHandler(mnuPreferencesOnClick);
            
            mnuTimecode.Image = Properties.Resources.time_edit;
            mnuTimecodeClassic.Click += new EventHandler(mnuTimecodeClassic_OnClick);
            mnuTimecodeFrames.Click += new EventHandler(mnuTimecodeFrames_OnClick);
            mnuTimecodeMilliseconds.Click += new EventHandler(mnuTimecodeMilliseconds_OnClick);
            mnuTimecodeMicroseconds.Click += new EventHandler(mnuTimecodeMicroseconds_OnClick);
            mnuTimecodeTimeAndFrames.Click += new EventHandler(mnuTimecodeTimeAndFrames_OnClick);
            mnuTimecodeNormalized.Click += new EventHandler(mnuTimecodeNormalized_OnClick);
            mnuTimecode.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuTimecodeClassic, 
                mnuTimecodeFrames, 
                mnuTimecodeMilliseconds, 
                mnuTimecodeMicroseconds, 
                mnuTimecodeTimeAndFrames});

            mnuPointer.Image = Properties.Resources.handopen24c;
            BuildPointerMenus();
            
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuLanguages, 
                mnuTimecode, 
                mnuPointer,
                new ToolStripSeparator(),
                new ToolStripSeparator(), 
                mnuPreferences});
            #endregion

            #region Help
            mnuHelpContents.Image = Properties.Resources.book_open;
            mnuHelpContents.ShortcutKeys = Keys.F1;
            mnuTranslate2.Image = Properties.Resources.international;
            mnuApplicationFolder.Image = Properties.Resources.folder;
            mnuEnableDebugLogs.Image = Properties.Resources.bug_16;
            mnuWebsite.Image = Properties.Resources.website;
            mnuAbout.Image = Properties.Resources.information;

            mnuHelpContents.Click += mnuHelpContents_OnClick;
            mnuTranslate2.Click += (s, e) => Process.Start("https://hosted.weblate.org/engage/kinovea/");
            mnuApplicationFolder.Click += (s, e) =>
            {
                FilesystemHelper.LocateDirectory(Software.SettingsDirectory);
            };
            mnuEnableDebugLogs.Click += (s, e) => ToggleDebugLogs();
            mnuWebsite.Click += (s,e) => Process.Start("https://www.kinovea.org");
            mnuAbout.Click += new EventHandler(mnuAbout_OnClick);

            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuHelpContents,
                mnuTranslate2,
                new ToolStripSeparator(), 
                mnuApplicationFolder, 
                mnuEnableDebugLogs,
                new ToolStripSeparator(),
                mnuWebsite,
                mnuAbout });
            #endregion

            // Top level merge.
            MenuStrip thisMenuStrip = new MenuStrip();
            thisMenuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuEdit, mnuView, mnuImage, mnuVideo, mnuTools, mnuWindow, mnuOptions, mnuHelp });
            thisMenuStrip.AllowMerge = true;

            ToolStripManager.Merge(thisMenuStrip, menu);
            
            // We need to affect the Text properties before merging with submenus.
            RefreshCultureMenu();
        }

        private void GetSubModulesMenus(ToolStrip menu)
        {
            navigationPanel.ExtendMenu(menu);
            updater.ExtendMenu(menu);
            screenManager.ExtendMenu(menu);
        }
        private void GetModuleToolBar(ToolStrip toolbar)
        {
            // Open.
            toolOpenFile.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOpenFile.Image = Properties.Resources.folder;
            toolOpenFile.ToolTipText = ScreenManagerLang.mnuOpenVideo;
            toolOpenFile.Click += new EventHandler(mnuOpenFileOnClick);
            
            toolbar.Items.Add(toolOpenFile);
        }
        private void GetSubModulesToolBars(ToolStrip toolbar)
        {
            navigationPanel.ExtendToolBar(toolbar);
            updater.ExtendToolBar(toolbar);
            screenManager.ExtendToolBar(toolbar);
        }
        private void RefreshCultureMenu()
        {
            mnuFile.Text = RootLang.mnuFile;
            mnuOpenFile.Text = ScreenManagerLang.mnuOpenVideo;
            mnuOpenReplayWatcher.Text = ScreenManagerLang.mnuOpenReplayWatcher;
            mnuHistory.Text = RootLang.mnuHistory;
            mnuHistoryReset.Text = RootLang.mnuHistoryReset;
            mnuQuit.Text = RootLang.Generic_Quit;
            
            mnuEdit.Text = RootLang.mnuEdit;
            mnuUndo.Text = RootLang.mnuUndo;
            mnuRedo.Text = RootLang.mnuRedo;
            
            mnuView.Text = RootLang.mnuScreens;
            mnuToggleNavigationPanel.Text = "Navigation panel";
            mnuFullScreen.Text = RootLang.mnuFullScreen;
            
            mnuImage.Text = RootLang.mnuImage;
            mnuVideo.Text = RootLang.mnuVideo;
            mnuTools.Text = RootLang.mnuTools;
            
            mnuOptions.Text = RootLang.mnuOptions;
            mnuLanguages.Text = RootLang.mnuLanguages;
            mnuTranslate1.Text = RootLang.mnuTranslate;
            mnuTranslate2.Text = RootLang.mnuTranslate;
            mnuPreferences.Text = RootLang.mnuPreferences;
            mnuTimecode.Text = RootLang.mnuTimeFormat;

            mnuTimecodeClassic.Text = "[h:][mm:]ss.xx[x]";
            mnuTimecodeClassic.Image = Properties.Resources.timecode;
            mnuTimecodeFrames.Text = RootLang.TimeCodeFormat_Frames;
            mnuTimecodeFrames.Image = Properties.Resources.framenumber;
            mnuTimecodeMilliseconds.Text = RootLang.TimeCodeFormat_Milliseconds;
            mnuTimecodeMilliseconds.Image = Properties.Resources.milliseconds;
            mnuTimecodeMicroseconds.Text = RootLang.TimeCodeFormat_Microseconds;
            mnuTimecodeMicroseconds.Image = Properties.Resources.microseconds;
            mnuTimecodeTimeAndFrames.Text = mnuTimecodeClassic.Text + " + " + RootLang.TimeCodeFormat_Frames;

            mnuPointer.Text = RootLang.mnuPointer;
            // Rebuild the whole pointer menu to get the correct text.
            BuildPointerMenus();

            mnuWindow.Text = "Window";
            mnuWindowProperties.Text = "Window properties…";
            mnuOpenNewWindow.Text = "Open new window";
            mnuReopenWindow.Text = "Reopen window";
            mnuManageWindows.Text = "Manage windows…";
            // Rebuild the reopen window menu to get the correct list.
            BuildReopenWindowsMenus();

            mnuHelp.Text = RootLang.mnuHelp;
            mnuHelpContents.Text = RootLang.mnuHelpContents;
            mnuApplicationFolder.Text = "Open application data folder…";
            mnuEnableDebugLogs.Text = PreferencesManager.GeneralPreferences.EnableDebugLog ? "Disable debug logs" : "Enable debug logs";
            mnuWebsite.Text = "www.kinovea.org";
            mnuAbout.Text = RootLang.mnuAbout;
            mnuHelp.Text = RootLang.mnuHelp;
        }
        #endregion

        #region Menus Event Handlers

        #region File
        private void mnuOpenFileOnClick(object sender, EventArgs e)
        {
            NotificationCenter.RaiseStopPlayback(this);

            string title = ScreenManagerLang.mnuOpenVideo;
            string filter = ScreenManagerLang.FileFilter_All + "|*.*";
            string filename = FilePicker.OpenVideo(title, filter);
            if (!string.IsNullOrEmpty(filename))
                OpenFromPath(filename);
        }
        private void mnuOpenReplayWatcherOnClick(object sender, EventArgs e)
        {
            NotificationCenter.RaiseStopPlayback(this);

            string path = FilePicker.OpenReplayWatcher();
            if (path == null || !Directory.Exists(Path.GetDirectoryName(path)))
                return;

            OpenFromPath(path);
        }

        private void mnuHistoryResetOnClick(object sender, EventArgs e)
        {
            PreferencesManager.FileExplorerPreferences.ResetRecentFiles();
        }
        private void menuQuitOnClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region View
        private void mnuToggleNavigationPanel_Click(object sender, EventArgs e)
        {
            bool show = !mnuToggleNavigationPanel.Checked;
            mainWindow.SupervisorControl.ToggleNavigationPanel(show, true);
            mnuToggleNavigationPanel.Checked = show;
        }
        private void mnuFullScreenOnClick(object sender, EventArgs e)
        {
            ToggleFullScreen();
        }
        #endregion

        #region Window
        private void mnuOpenNewWindow_Click(object sender, EventArgs e)
        {
            WindowManager.OpenNewWindow();

            // Note: we can't really trigger a refresh of the "Reopen" menu here.
            // The new window hasn't had time to create the descriptor file yet.
        }

        private void mnuWindowProperties_Click(object sender, EventArgs e)
        {
            string memoName = WindowManager.TitleName;

            FormWindowProperties fwp = new FormWindowProperties(this);
            fwp.StartPosition = FormStartPosition.CenterScreen;
            fwp.ShowDialog();
            fwp.Dispose();

            if (fwp.DialogResult == DialogResult.OK)
            {
                WindowManager.SaveActiveWindow();
                if (memoName != WindowManager.TitleName)
                    mainWindow.UpdateTitle();

                // The only thing we may have changed here is our own instance
                // so we don't need to refresh the "Reopen" menu.
            }
        }

        private void BuildReopenWindowsMenus()
        {
            mnuReopenWindow.DropDownItems.Clear();

            ToolStripMenuItem mnuRefresh = new ToolStripMenuItem();
            mnuRefresh.Text = "Refresh";
            mnuRefresh.Image = Properties.Resources.arrow_refresh;
            mnuRefresh.Click += (s, e) =>
            {
                WindowManager.ReadAllDescriptors();

                // Trigger a rebuild of this whole drop down.
                // In theory when we exit this closure the original mnuRefresh can be garbage collected.
                BuildReopenWindowsMenus();
            };

            mnuReopenWindow.DropDownItems.Add(mnuRefresh);
            mnuReopenWindow.DropDownItems.Add(new ToolStripSeparator());

            List<WindowDescriptor> sorted = WindowManager.WindowDescriptors.ToList();
            sorted.Sort((x, y) => y.LastSave.CompareTo(x.LastSave));

            foreach (var d in sorted)
            {
                // Filter out our own instance.
                if (d.Id == WindowManager.ActiveWindow.Id)
                    continue;

                ToolStripMenuItem mnuWindowDescriptor = new ToolStripMenuItem();
                string name = d.Name;
                if (string.IsNullOrEmpty(name))
                    name = string.Format("[{0}]", WindowManager.GetIdName(d));

                mnuWindowDescriptor.Text = name;

                // Use a custom icon based on the last known screen list.
                if (d.ScreenList.Count == 0)
                {
                    mnuWindowDescriptor.Image = Properties.Resources.home3;
                }
                else if (d.ScreenList.Count == 1)
                {
                    if (d.ScreenList[0].ScreenType == ScreenType.Playback)
                    {
                        mnuWindowDescriptor.Image = Properties.Resources.television;
                    }
                    else
                    {
                        mnuWindowDescriptor.Image = Properties.Resources.camera_video;
                    }
                }
                else if (d.ScreenList.Count == 2)
                {
                    if (d.ScreenList[0].ScreenType == ScreenType.Playback && d.ScreenList[1].ScreenType == ScreenType.Playback)
                    {
                        mnuWindowDescriptor.Image = Properties.Resources.dualplayback;
                    }
                    else if (d.ScreenList[0].ScreenType == ScreenType.Capture && d.ScreenList[1].ScreenType == ScreenType.Capture)
                    {
                        mnuWindowDescriptor.Image = Properties.Resources.dualcapture2;
                    }
                    else
                    {
                        // Mixed.
                        mnuWindowDescriptor.Image = Properties.Resources.dualmixed3;
                    }
                }

                WindowDescriptor desc = d;
                mnuWindowDescriptor.Click += (s, e) =>
                {
                    if (desc != null)
                        WindowManager.ReopenWindow(d);
                };

                mnuReopenWindow.DropDownItems.Add(mnuWindowDescriptor);
            }
        }

        private void mnuManageWindows_Click(object sender, EventArgs e)
        {
            FormWindowManager fwm = new FormWindowManager(this);
            fwm.StartPosition = FormStartPosition.CenterScreen;
            fwm.ShowDialog();
            fwm.Dispose();

            RefreshCultureMenu();
        }
        #endregion

        #region Options
        private void mnuLanguage_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if(menu != null && menu.Tag is string)
                SwitchCulture((string)menu.Tag);
        }
        private void SwitchCulture(string name)
        {
            try
            {
                // Make sure we have the latest core prefs before saving this.
                PreferencesManager.BeforeRead();

                CultureInfo oldCulture = Thread.CurrentThread.CurrentUICulture;
                CultureInfo newCulture = new CultureInfo(name);

                log.Debug(String.Format("Changing culture from [{0}] to [{1}].", oldCulture.Name, newCulture.Name));

                PreferencesManager.GeneralPreferences.SetCulture(newCulture.Name);
                Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();

                RefreshUICulture();

                // Make sure all the other windows are updated immediately.
                WindowManager.SendMessage("Kinovea:Window.PreferencesUpdated");
            }
            catch (ArgumentException)
            {
                log.ErrorFormat("Could not switch from culture {0} to {1}.", Thread.CurrentThread.CurrentUICulture.Name, name);
            }
        }
        private void CheckLanguageMenu()
        {
            foreach(ToolStripMenuItem mnuLang in languageMenus.Values)
                mnuLang.Checked = false;

            string cultureName = LanguageManager.GetCurrentCultureName();
            
            try
            {
                languageMenus[cultureName].Checked = true;    
            }
            catch(KeyNotFoundException)
            {
                languageMenus["en"].Checked = true;            
            }
        }
        private void mnuPreferencesOnClick(object sender, EventArgs e)
        {
            FormPreferences2 fp = new FormPreferences2();
            fp.ShowDialog();
            fp.Dispose();

            if (fp.DialogResult == DialogResult.OK)
            {
                // Refresh the UI.
                log.Debug("Setting current UI culture.");
                Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
                PreferencesUpdated(true);
            }
        }

        private void CheckTimecodeMenu()
        {
            mnuTimecodeClassic.Checked = false;
            mnuTimecodeFrames.Checked = false;
            mnuTimecodeMilliseconds.Checked = false;
            mnuTimecodeMicroseconds.Checked = false;
            mnuTimecodeTimeAndFrames.Checked = false;
            mnuTimecodeNormalized.Checked = false;
            
            TimecodeFormat tf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            
            switch (tf)
            {
                case TimecodeFormat.ClassicTime:
                    mnuTimecodeClassic.Checked = true;
                    break;
                case TimecodeFormat.Frames:
                    mnuTimecodeFrames.Checked = true;
                    break;
                case TimecodeFormat.Milliseconds:
                    mnuTimecodeMilliseconds.Checked = true;
                    break;
                case TimecodeFormat.Microseconds:
                    mnuTimecodeMicroseconds.Checked = true;
                    break;
                case TimecodeFormat.TimeAndFrames:
                    mnuTimecodeTimeAndFrames.Checked = true;
                    break;
                case TimecodeFormat.Normalized:
                    mnuTimecodeNormalized.Checked = true;
                    break; 
                default:
                    break;
            }
        }
        private void mnuTimecodeClassic_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.ClassicTime);
        }
        private void mnuTimecodeFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Frames);
        }
        private void mnuTimecodeMilliseconds_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Milliseconds);
        }
        private void mnuTimecodeMicroseconds_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Microseconds);
        }
        private void mnuTimecodeTimeAndFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.TimeAndFrames);
        }
        private void mnuTimecodeNormalized_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Normalized);
        }
        private void SwitchTimecode(TimecodeFormat _timecode)
        {
            // Make sure we have the latest core prefs before saving this.
            PreferencesManager.BeforeRead();

            PreferencesManager.PlayerPreferences.TimecodeFormat = _timecode;
            RefreshUICulture();

            // Make sure all the other windows are updated immediately.
            WindowManager.SendMessage("Kinovea:Window.PreferencesUpdated");
        }
        #endregion

        #region Help
        private void mnuHelpContents_OnClick(object sender, EventArgs e)
        {
            // Open online help.
            // Currently only English is supported.
            Process.Start("https://www.kinovea.org/help/en/");
        }

        private void ToggleDebugLogs()
        {
            // Make sure we have the latest core prefs before saving this.
            PreferencesManager.BeforeRead();

            bool enabled = !PreferencesManager.GeneralPreferences.EnableDebugLog;
            Software.UpdateLogLevel(enabled);
            PreferencesManager.GeneralPreferences.EnableDebugLog = enabled;
            mnuEnableDebugLogs.Text = enabled ? "Disable debug logs" : "Enable debug logs";
            if (enabled)
            {
                log.Debug("Debug logs enabled.");
            }
        }

        private void mnuAbout_OnClick(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
            fa.Dispose();
        }
        #endregion

        #endregion        
        
        private void NotificationCenter_RecentFilesChanged(object sender, EventArgs e)
        {
            mnuHistory.DropDownItems.Clear();

            int maxRecentFiles = PreferencesManager.FileExplorerPreferences.MaxRecentFiles;
            List<string> recentFiles = PreferencesManager.FileExplorerPreferences.RecentFiles;
            List<string> recentWatchers = PreferencesManager.FileExplorerPreferences.RecentWatchers;
            if ((recentFiles == null || recentFiles.Count == 0) && 
                (recentWatchers == null || recentFiles.Count == 0))
            {
                mnuHistory.Enabled = false;
                return;
            }

            int addedFiles = FillHistoryDropDown(maxRecentFiles, recentFiles, true);
            if (addedFiles > 0)
                mnuHistory.DropDownItems.Add(new ToolStripSeparator());

            int addedWatchers = FillHistoryDropDown(maxRecentFiles, recentWatchers, false);
            if (addedWatchers > 0)
                mnuHistory.DropDownItems.Add(new ToolStripSeparator());

            bool added = addedFiles + addedWatchers > 0;
            if (added)
                mnuHistory.DropDownItems.Add(mnuHistoryReset);

            mnuHistory.Enabled = added;
        }

        private int FillHistoryDropDown(int maxRecentFiles, List<string> recentFiles, bool isFile)
        {
            if (maxRecentFiles == 0 || recentFiles == null || recentFiles.Count == 0)
                return 0;

            int added = 0;
            foreach (string file in recentFiles)
            {
                if (added >= maxRecentFiles)
                    break;

                if (string.IsNullOrEmpty(file))
                    continue;

                if ((isFile && !File.Exists(file)) ||
                    (!isFile && !Directory.Exists(Path.GetDirectoryName(file))))
                {
                        continue;
                }

                ToolStripMenuItem menu = new ToolStripMenuItem();
                menu.Image = isFile ? Properties.Resources.film_small : Properties.Resources.user_detective;
                menu.Text = file;
                menu.Click += (s, evt) => OpenFromPath(file);

                mnuHistory.DropDownItems.Add(menu);
                added++;
            }

            return added;
        }

        private void NotificationCenter_FullscreenToggle(object sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        private void NotificationCenter_PreferenceTabAsked(object sender, PreferenceTabEventArgs e)
        {
            FormPreferences2 fp = new FormPreferences2(e.Tab);
            fp.ShowDialog();
            fp.Dispose();
            
            if (fp.DialogResult == DialogResult.OK)
            {
                Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
                PreferencesUpdated(true);
            }
        }

        private void NotificationCenter_WakeUpAsked(object sender, EventArgs e)
        {
            WindowManager.WakeUp(mainWindow.Handle);
        }

        private void NotificationCenter_ExternalCommand(object sender, ExternalCommandEventArgs e)
        {
            string[] tokens = e.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
            {
                log.ErrorFormat("Malformed external command. \"{0\"}", e.Name);
                return;
            }

            switch (tokens[0])
            {
                case "Window":
                    {
                        WindowCommand command;
                        bool parsed = Enum.TryParse(tokens[1], out command);
                        if (!parsed)
                        {
                            log.ErrorFormat("Unsupported window command \"{0}\".", tokens[1]);
                            return;
                        }

                        ExecuteWindowCommand(command);
                        break;
                    }
                default:
                    break;
            }
        }
       
        #region Lower Level Methods
        private void ExecuteWindowCommand(WindowCommand cmd)
        {
            switch (cmd)
            {
                case WindowCommand.PreferencesUpdated:

                    // An external window has updated the shared core preferences.
                    // At this point we should already have detected the date change on the file.
                    // Make sure we reload the file and import the new preferences.
                    log.DebugFormat("Preferences updated in an other window.");
                    PreferencesManager.BeforeRead();

                    // Special handling if the culture changed.
                    CultureInfo oldCulture = Thread.CurrentThread.CurrentUICulture;
                    CultureInfo newCulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
                    if (oldCulture.Name != newCulture.Name)
                    {
                        Thread.CurrentThread.CurrentUICulture = newCulture;
                    }

                    // Trigger a local update in this instance to make sure the UI is up to date.
                    PreferencesUpdated(false);
                    break;
                default:
                    break;
            }
        }
        private void OpenFromPath(string path)
        {
            if (Path.GetFileName(path).Contains("*"))
            {
                // Replay watcher.
                ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                sdp.FullPath = path;
                sdp.IsReplayWatcher = true;
                sdp.Stretch = true;
                sdp.Autoplay = true;
                sdp.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
                LoaderVideo.LoadVideoInScreen(screenManager, path, sdp);

                screenManager.OrganizeScreens();
            }
            else
            {
                // Normal file.
                if (File.Exists(path))
                {
                    ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                    sdp.FullPath = path;
                    sdp.IsReplayWatcher = false;
                    sdp.Stretch = false;
                    sdp.Autoplay = false;
                    sdp.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;

                    LoaderVideo.LoadVideoInScreen(screenManager, path, -1, sdp);
                    screenManager.OrganizeScreens();
                }
                else
                {
                    MessageBox.Show(ScreenManagerLang.LoadMovie_FileNotOpened, ScreenManagerLang.LoadMovie_Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        private void ToggleFullScreen()
        {
            mainWindow.ToggleFullScreen();

            if (mainWindow.FullScreen)
            {
                // Entering full screen, force hide explorer, don't save prefs.
                mainWindow.SupervisorControl.ToggleNavigationPanel(false, false);
            }
            else
            {
                // Exiting full screen, restore from preferences.
                bool show = WindowManager.ActiveWindow.ExplorerVisible;
                mainWindow.SupervisorControl.ToggleNavigationPanel(show, false);
            }
            
            // Propagates the call to screens.
            screenManager.FullScreen(mainWindow.FullScreen);
        }
        
        private void BuildPointerMenus()
        {
            mnuPointer.DropDownItems.Clear();

            // Standard menus.
            ToolStripMenuItem mnuDefaultPointer = new ToolStripMenuItem();
            mnuDefaultPointer.Text = RootLang.mnuDefaultPointer;
            mnuDefaultPointer.Image = Properties.Resources.handopen24c;
            mnuDefaultPointer.Tag = "::default";
            mnuDefaultPointer.Click += mnuPointer_OnClick;

            ToolStripMenuItem mnuBigHand = new ToolStripMenuItem();
            mnuBigHand.Text = RootLang.mnuBigHand;
            mnuBigHand.Image = Properties.Resources.pointer_pointing_hand;
            mnuBigHand.Tag = "::bigHand";
            mnuBigHand.Click += mnuPointer_OnClick;

            ToolStripMenuItem mnuBigArrow = new ToolStripMenuItem();
            mnuBigArrow.Text = RootLang.mnuBigArrow;
            mnuBigArrow.Image = Properties.Resources.pointer_arrow;
            mnuBigArrow.Tag = "::bigArrow";
            mnuBigArrow.Click += mnuPointer_OnClick;

            mnuPointer.DropDownItems.Add(mnuDefaultPointer);
            mnuPointer.DropDownItems.Add(new ToolStripSeparator());
            mnuPointer.DropDownItems.Add(mnuBigHand);
            mnuPointer.DropDownItems.Add(mnuBigArrow);

            // Custom menus from special directory.
            if (!Directory.Exists(Software.PointersDirectory))
                return;

            bool addedSeparator = false;
            foreach (string file in Directory.GetFiles(Software.PointersDirectory))
            {
                if (!addedSeparator)
                {
                    mnuPointer.DropDownItems.Add(new ToolStripSeparator());
                    addedSeparator = true;
                }
                
                ToolStripMenuItem mnuCustomPointer = new ToolStripMenuItem();
                mnuCustomPointer.Text = Path.GetFileNameWithoutExtension(file);
                mnuCustomPointer.Tag = Path.GetFileNameWithoutExtension(file);
                mnuCustomPointer.Image = Properties.Resources.image;
                mnuCustomPointer.Click += mnuPointer_OnClick;

                mnuPointer.DropDownItems.Add(mnuCustomPointer);
            }
        }

        private void mnuPointer_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if (menu == null)
                return;

            string tag = menu.Tag as string;
            if (string.IsNullOrEmpty(tag))
                return;

            // Make sure we have the latest core prefs before saving this.
            PreferencesManager.BeforeRead();

            PointerManager.SetCursor(tag);
        }
        
        #endregion
    }
}
