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


using Kinovea.Root.Languages;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.FileBrowser;
using Kinovea.ScreenManager;
using Kinovea.Services;
using Kinovea.Updater;

namespace Kinovea.Root
{

    //public delegate void DelegateClosingRequest();


    public class RootKernel : IKernel 
    {
        #region Members
        private KinoveaMainWindow MainWindow;
        private ResourceManager RootResourceManager;

        // Sub Modules
        public FileBrowserKernel FileBrowser;
        public UpdaterKernel Updater;
        public ScreenManagerKernel ScreenManager;


        // Menus
        private ToolStripMenuItem mnuFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHistory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuQuit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuEdit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUndo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRedo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuView = new ToolStripMenuItem();
        public ToolStripMenuItem mnuToggleFileExplorer = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMotion = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLanguages = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSpanish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuEnglish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFrench = new ToolStripMenuItem();
        private ToolStripMenuItem mnuGerman = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPolish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDutch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuItalian = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPortuguese = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRomanian = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFinnish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuNorwegian = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTurkish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChinese = new ToolStripMenuItem();
        private ToolStripMenuItem mnuGreek = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPreferences = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecode = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeClassic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeHoM = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeTToH = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeTimeAndFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHelp = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHelpContents = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTutorialVideos = new ToolStripMenuItem();
        private ToolStripMenuItem mnuApplicationFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAbout = new ToolStripMenuItem();

        // Status
        private ToolStripStatusLabel stLabel = new ToolStripStatusLabel();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public RootKernel()
        {
            // Store Kinovea's version from the assembly.
            Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            PreferencesManager.ReleaseVersion = String.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
            
            // Set type of release (Experimental vs Production) 
            PreferencesManager.ExperimentalRelease = false; 
            
            // Display some system infos in the log.
            log.Info(String.Format("Kinovea version : {0}, ({1})", PreferencesManager.ReleaseVersion, PreferencesManager.ExperimentalRelease?"Experimental":"Production"));
            log.Info(".NET Framework Version : " + Environment.Version.ToString());
            log.Info("OS Version : " + System.Environment.OSVersion.ToString());
            log.Info("Primary Screen : " + SystemInformation.PrimaryMonitorSize.ToString());
            log.Info("Virtual Screen : " + SystemInformation.VirtualScreen.ToString());
            
            // Since it is the very first call, it will both instanciate and import.
            // Previous calls were done on static prioperties, no instanciation. 
            PreferencesManager pm = PreferencesManager.Instance();
            
            // Get Manager for i18n
            RootResourceManager = new ResourceManager("Kinovea.Root.Languages.RootLang", Assembly.GetExecutingAssembly());
            
            // Initialise command line parser and get the arguments.
            CommandLineArgumentManager am = CommandLineArgumentManager.Instance();
            am.InitializeComandLineParser();
            string[] args = Environment.GetCommandLineArgs();
            am.ParseArguments(args);
            
            log.Debug("Build the modules tree.");
            BuildSubTree();
            
            log.Debug("Create main UI window.");
            MainWindow = new KinoveaMainWindow(this);
            
            log.Debug("Plug sub modules at UI extension points (Menus, ToolBars, StatusBAr, Windows).");
            ExtendMenu(MainWindow.menuStrip);
            ExtendToolBar(MainWindow.toolStrip);
            ExtendStatusBar(MainWindow.statusStrip);
            ExtendUI();

            log.Debug("Register global services offered at Root level.");
            DelegatesPool dp = DelegatesPool.Instance();
            dp.UpdateStatusBar = DoUpdateStatusBar;
            dp.MakeTopMost = DoMakeTopMost;
        }
        #endregion

        #region Prepare & Launch
        public void Prepare()
        {
            // Prepare the right strings before we open the curtains.
            log.Debug("Setting current ui culture.");
            Thread.CurrentThread.CurrentUICulture = PreferencesManager.Instance().GetSupportedCulture();
            RefreshUICulture();
            CheckLanguageMenu();
            CheckTimecodeMenu();
            
            ScreenManager.Prepare();
            PrintInitialConf();
            if(CommandLineArgumentManager.Instance().InputFile != null)
            {
            	ScreenManager.PrepareScreen();
            }
        }
        public void Launch()
        {            
            log.Debug("Calling Application.Run() to boot up the UI.");
            Application.Run(MainWindow);
        }
        #endregion

        #region IKernel Implementation
        public void BuildSubTree()
        {        	
        	FileBrowser     = new FileBrowserKernel();
        	Updater         = new UpdaterKernel();
            ScreenManager   = new ScreenManagerKernel();
            log.Debug("Modules tree built.");
        }
        public void ExtendMenu(ToolStrip _menu)
        {
            // Each module must merge its menu into the root one.

            _menu.AllowMerge = true;

            GetModuleMenus(_menu);
            GetSubModulesMenus(_menu);

        }
        public void ExtendToolBar(ToolStrip _toolbar)
        {
            // Nothing at this level.
            GetSubModulesToolBars(_toolbar);

			// TODO: add tool buttons for screens dispositions.
            _toolbar.Visible = false;
        }
        public void ExtendStatusBar(ToolStrip _statusbar)
        {
            if(_statusbar != null)
            {
                // This level
                stLabel = new ToolStripStatusLabel();
                _statusbar.Items.AddRange(new ToolStripItem[] { stLabel });
            
                // TODO: get statusbar from other modules when they have some.
            }
        }
        public void ExtendUI()
        {
            // Sub Modules
            FileBrowser.ExtendUI();
            Updater.ExtendUI();
            ScreenManager.ExtendUI();

            // Integrate the sub modules UI into the main kernel UI.
            MainWindow.SupervisorControl.splitWorkSpace.Panel1.Controls.Add(FileBrowser.UI);
            MainWindow.SupervisorControl.splitWorkSpace.Panel2.Controls.Add(ScreenManager.UI);
            
			MainWindow.SupervisorControl.buttonCloseExplo.BringToFront();
        }
        public void RefreshUICulture()
        {
            log.Debug("RefreshUICulture - Reload localized strings for the whole tree.");
         
            // Menu
            foreach (ToolStripItem item in MainWindow.menuStrip.Items)
            {
                RefreshSubMenu(item);
            }

            CheckLanguageMenu();
            CheckTimecodeMenu();
            
            PreferencesManager pm = PreferencesManager.Instance();
            pm.OrganizeHistoryMenu();
                        
            CommandManager cm = CommandManager.Instance();
            cm.UpdateMenus();

            // ToolBar
            foreach (ToolStripItem item in MainWindow.toolStrip.Items)
            {
                // If tag is null, non translatable texts.
                if (item.Tag != null)
                {
                    item.Text = ((ItemResourceInfo)item.Tag).resManager.GetString(((ItemResourceInfo)item.Tag).strText, Thread.CurrentThread.CurrentUICulture);
                    item.ToolTipText = ((ItemResourceInfo)item.Tag).resManager.GetString(((ItemResourceInfo)item.Tag).strToolTipText, Thread.CurrentThread.CurrentUICulture);
                }
                // TODO: SubItems (when any)
            }

            // Sub Modules
            FileBrowser.RefreshUICulture();
            Updater.RefreshUICulture();
            ScreenManager.RefreshUICulture();
            
            log.Debug("RefreshUICulture - Whole tree culture reloaded.");
        }
        public void CloseSubModules()
        {
            log.Debug("Root is closing. Call close on all sub modules.");
            FileBrowser.CloseSubModules();
            Updater.CloseSubModules();
            ScreenManager.CloseSubModules();    
        }
        #endregion

        #region Extension point helpers
        private void GetModuleMenus(ToolStrip _menu)
        {
            //----------------------------------------------------------------------------------------------
            // Note on menus localization:
            // The refreshing of the UI after a change of language will happen from here, but the localized 
            // text for a given menu is stored in the resource of the module it comes from.
            // Thus, each menu must know its own Resource Manager, even if it's in a sub module.
            // To that end, we store an ItemResourceInfo object directly into the menu.
            // It contains the string to look up and the ResourceManager to use.
            //----------------------------------------------------------------------------------------------

            #region File

            // 1.File Menu
            mnuFile.Tag = new ItemResourceInfo(RootResourceManager, "mnuFile");
            mnuFile.Text = ((ItemResourceInfo)mnuFile.Tag).resManager.GetString(((ItemResourceInfo)mnuFile.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuFile.MergeAction = MergeAction.Append;

            // OpenFile
            mnuOpenFile.Tag = new ItemResourceInfo(RootResourceManager, "mnuOpenFile");
            mnuOpenFile.Text = ((ItemResourceInfo)mnuOpenFile.Tag).resManager.GetString(((ItemResourceInfo)mnuOpenFile.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuOpenFile.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            mnuOpenFile.Click += new EventHandler(mnuOpenFileOnClick);

            
            // History
           	mnuHistory.Tag = new ItemResourceInfo(RootResourceManager, "mnuHistory");
            mnuHistory.Text = ((ItemResourceInfo)mnuHistory.Tag).resManager.GetString(((ItemResourceInfo)mnuHistory.Tag).strText, Thread.CurrentThread.CurrentUICulture);

            #region History Items
            ToolStripMenuItem mnuHistoryVideo1 = new ToolStripMenuItem();
            mnuHistoryVideo1.MergeAction = MergeAction.Append;
            mnuHistoryVideo1.Visible = false;
            mnuHistoryVideo1.Click += new EventHandler(mnuHistoryVideo1OnClick);

            ToolStripMenuItem mnuHistoryVideo2 = new ToolStripMenuItem();
            mnuHistoryVideo2.MergeAction = MergeAction.Append;
            mnuHistoryVideo2.Visible = false;
            mnuHistoryVideo2.Click += new EventHandler(mnuHistoryVideo2OnClick);

            ToolStripMenuItem mnuHistoryVideo3 = new ToolStripMenuItem();
            mnuHistoryVideo3.MergeAction = MergeAction.Append;
            mnuHistoryVideo3.Visible = false;
            mnuHistoryVideo3.Click += new EventHandler(mnuHistoryVideo3OnClick);

            ToolStripMenuItem mnuHistoryVideo4 = new ToolStripMenuItem();
            mnuHistoryVideo4.MergeAction = MergeAction.Append;
            mnuHistoryVideo4.Visible = false;
            mnuHistoryVideo4.Click += new EventHandler(mnuHistoryVideo4OnClick);

            ToolStripMenuItem mnuHistoryVideo5 = new ToolStripMenuItem();
            mnuHistoryVideo5.MergeAction = MergeAction.Append;
            mnuHistoryVideo5.Visible = false;
            mnuHistoryVideo5.Click += new EventHandler(mnuHistoryVideo5OnClick);

            ToolStripMenuItem mnuHistoryVideo6 = new ToolStripMenuItem();
            mnuHistoryVideo6.MergeAction = MergeAction.Append;
            mnuHistoryVideo6.Visible = false;
            mnuHistoryVideo6.Click += new EventHandler(mnuHistoryVideo6OnClick);

            ToolStripMenuItem mnuHistoryVideo7 = new ToolStripMenuItem();
            mnuHistoryVideo7.MergeAction = MergeAction.Append;
            mnuHistoryVideo7.Visible = false;
            mnuHistoryVideo7.Click += new EventHandler(mnuHistoryVideo7OnClick);

            ToolStripMenuItem mnuHistoryVideo8 = new ToolStripMenuItem();
            mnuHistoryVideo8.MergeAction = MergeAction.Append;
            mnuHistoryVideo8.Visible = false;
            mnuHistoryVideo8.Click += new EventHandler(mnuHistoryVideo8OnClick);

            ToolStripMenuItem mnuHistoryVideo9 = new ToolStripMenuItem();
            mnuHistoryVideo9.MergeAction = MergeAction.Append;
            mnuHistoryVideo9.Visible = false;
            mnuHistoryVideo9.Click += new EventHandler(mnuHistoryVideo9OnClick);

            ToolStripMenuItem mnuHistoryVideo10 = new ToolStripMenuItem();
            mnuHistoryVideo10.MergeAction = MergeAction.Append;
            mnuHistoryVideo10.Visible = false;
            mnuHistoryVideo10.Click += new EventHandler(mnuHistoryVideo10OnClick);

            #endregion

            ToolStripSeparator mnuSepHistory = new ToolStripSeparator();
            mnuSepHistory.Visible = false;

            ToolStripMenuItem mnuHistoryReset = new ToolStripMenuItem();
            mnuHistoryReset.Tag = new ItemResourceInfo(RootResourceManager, "mnuHistoryReset");
            mnuHistoryReset.Text = ((ItemResourceInfo)mnuHistoryReset.Tag).resManager.GetString(((ItemResourceInfo)mnuHistoryReset.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuHistoryReset.MergeAction = MergeAction.Append;
            mnuHistoryReset.Visible = false;
            mnuHistoryReset.Click += new EventHandler(mnuHistoryResetOnClick);

            mnuHistory.DropDownItems.AddRange(new ToolStripItem[] { mnuHistoryVideo1, mnuHistoryVideo2, mnuHistoryVideo3, mnuHistoryVideo4, mnuHistoryVideo5, mnuHistoryVideo6, mnuHistoryVideo7, mnuHistoryVideo8, mnuHistoryVideo9, mnuHistoryVideo10, mnuSepHistory, mnuHistoryReset });

            PreferencesManager pm = PreferencesManager.Instance();
            pm.RegisterHistoryMenu(mnuHistory);

            // Quit
            mnuQuit.Tag = new ItemResourceInfo(RootResourceManager, "Generic_Quit");
            mnuQuit.Text = ((ItemResourceInfo)mnuQuit.Tag).resManager.GetString(((ItemResourceInfo)mnuQuit.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuQuit.Click += new EventHandler(menuQuitOnClick);

            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { 	mnuOpenFile, 
                                           							mnuHistory, 
                                           							new ToolStripSeparator(),
                                           							// -> Here will be plugged the other file menus (save, export)
                                           							new ToolStripSeparator(), 
                                           							mnuQuit });
            
            #endregion

            #region Edit
            // 2.Edit Menu
            mnuEdit.Tag = new ItemResourceInfo(RootResourceManager, "mnuEdit");
            mnuEdit.Text = ((ItemResourceInfo)mnuEdit.Tag).resManager.GetString(((ItemResourceInfo)mnuEdit.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuEdit.MergeAction = MergeAction.Append;

            //Undo
            mnuUndo.Tag = new ItemResourceInfo(RootResourceManager, "mnuUndo");
            mnuUndo.Text = ((ItemResourceInfo)mnuUndo.Tag).resManager.GetString(((ItemResourceInfo)mnuUndo.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            mnuUndo.Click += new EventHandler(menuUndoOnClick);
            mnuUndo.Enabled = false;

            CommandManager cm = CommandManager.Instance();
            cm.RegisterUndoMenu(mnuUndo);

            //Redo
            mnuRedo.Tag = new ItemResourceInfo(RootResourceManager, "mnuRedo");
            mnuRedo.Text = ((ItemResourceInfo)mnuRedo.Tag).resManager.GetString(((ItemResourceInfo)mnuRedo.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            mnuRedo.Click += new EventHandler(menuRedoOnClick);
            mnuRedo.Enabled = false;

            cm.RegisterRedoMenu(mnuRedo);

            mnuEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuUndo, mnuRedo });
            #endregion

            #region View
            // 3.View
            mnuView.Tag = new ItemResourceInfo(RootResourceManager, "mnuScreens");
            mnuView.Text = ((ItemResourceInfo)mnuView.Tag).resManager.GetString(((ItemResourceInfo)mnuView.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            
            // Toggle File Explorer
            mnuToggleFileExplorer.Tag = new ItemResourceInfo(RootResourceManager, "mnuToggleFileExplorer");
            mnuToggleFileExplorer.Text = ((ItemResourceInfo)mnuToggleFileExplorer.Tag).resManager.GetString(((ItemResourceInfo)mnuToggleFileExplorer.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuToggleFileExplorer.Checked = true;
            mnuToggleFileExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuToggleFileExplorer.ShortcutKeys = System.Windows.Forms.Keys.F4;
            mnuToggleFileExplorer.Click += new EventHandler(mnuToggleFileExplorerOnClick);

            ToolStripSeparator mnuSepView = new ToolStripSeparator();

            mnuView.DropDownItems.AddRange(new ToolStripItem[] { mnuToggleFileExplorer, mnuSepView });
            #endregion

            #region Image
            // 4.Image
            mnuImage.Tag = new ItemResourceInfo(RootResourceManager, "mnuImage");
            mnuImage.Text = ((ItemResourceInfo)mnuImage.Tag).resManager.GetString(((ItemResourceInfo)mnuImage.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            #endregion

            #region Motion
            // 5.Motion
            mnuMotion.Tag = new ItemResourceInfo(RootResourceManager, "mnuMotion");
            mnuMotion.Text = ((ItemResourceInfo)mnuMotion.Tag).resManager.GetString(((ItemResourceInfo)mnuMotion.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            #endregion

            #region Options
            // 6.Options
            mnuOptions.Tag = new ItemResourceInfo(RootResourceManager, "mnuOptions");
            mnuOptions.Text = ((ItemResourceInfo)mnuOptions.Tag).resManager.GetString(((ItemResourceInfo)mnuOptions.Tag).strText, Thread.CurrentThread.CurrentUICulture);

            // Languages
            mnuLanguages.Tag = new ItemResourceInfo(RootResourceManager, "mnuLanguages");
            mnuLanguages.Text = ((ItemResourceInfo)mnuLanguages.Tag).resManager.GetString(((ItemResourceInfo)mnuLanguages.Tag).strText, Thread.CurrentThread.CurrentUICulture);

            #region Languages menus 
            
            // [Untranslatables]
            
            // German
            mnuGerman = new ToolStripMenuItem(PreferencesManager.LanguageGerman);
            mnuGerman.Click += new EventHandler(menuGermanOnClick);

            //English
            mnuEnglish = new ToolStripMenuItem(PreferencesManager.LanguageEnglish);
            mnuEnglish.Click += new EventHandler(menuEnglishOnClick);

            // Spanish
            mnuSpanish = new ToolStripMenuItem(PreferencesManager.LanguageSpanish);
            mnuSpanish.Click += new EventHandler(menuSpanishOnClick);

            //French
            mnuFrench = new ToolStripMenuItem(PreferencesManager.LanguageFrench);
            mnuFrench.Click += new EventHandler(menuFrenchOnClick);

            // Italian
            mnuItalian = new ToolStripMenuItem(PreferencesManager.LanguageItalian);
            mnuItalian.Click += new EventHandler(menuItalianOnClick);

            // Dutch
            mnuDutch = new ToolStripMenuItem(PreferencesManager.LanguageDutch);
            mnuDutch.Click += new EventHandler(menuDutchOnClick);

            // Polish
            mnuPolish = new ToolStripMenuItem(PreferencesManager.LanguagePolish);
            mnuPolish.Click += new EventHandler(menuPolishOnClick);

            // Portuguese
            mnuPortuguese = new ToolStripMenuItem(PreferencesManager.LanguagePortuguese);
            mnuPortuguese.Click += new EventHandler(menuPortugueseOnClick);

            // Romanian
            mnuRomanian = new ToolStripMenuItem(PreferencesManager.LanguageRomanian);
            mnuRomanian.Click += new EventHandler(menuRomanianOnClick);

            // Finnish
            mnuFinnish = new ToolStripMenuItem(PreferencesManager.LanguageFinnish);
            mnuFinnish.Click += new EventHandler(menuFinnishOnClick);
            
            // Norwegian
            mnuNorwegian = new ToolStripMenuItem(PreferencesManager.LanguageNorwegian);
            mnuNorwegian.Click += new EventHandler(menuNorwegianOnClick);
            
            // Turkish
            mnuTurkish = new ToolStripMenuItem(PreferencesManager.LanguageTurkish);
            mnuTurkish.Click += new EventHandler(menuTurkishOnClick);
            
            // Chinese
            mnuChinese = new ToolStripMenuItem(PreferencesManager.LanguageChinese);
            mnuChinese.Click += new EventHandler(menuChineseOnClick);
            
            // Greek
            mnuGreek = new ToolStripMenuItem(PreferencesManager.LanguageGreek);
            mnuGreek.Click += new EventHandler(menuGreekOnClick);
            
            // Re-Order alphabetically by localized name.
            mnuLanguages.DropDownItems.AddRange(new ToolStripItem[] { 
                                                						mnuGerman,
                                                						mnuGreek,
                                                						mnuEnglish, 
                                                						mnuSpanish, 
                                                						mnuFrench, 
                                                						mnuItalian, 
                                                						mnuDutch, 
                                                						mnuNorwegian,
                                                						mnuPolish, 
                                                						mnuPortuguese, 
                                                						mnuRomanian, 
                                                						mnuFinnish,
                                                						mnuTurkish,
                                                						mnuChinese });
            #endregion

            // Preferences
            mnuPreferences.Tag = new ItemResourceInfo(RootResourceManager, "mnuPreferences");
            mnuPreferences.Text = ((ItemResourceInfo)mnuPreferences.Tag).resManager.GetString(((ItemResourceInfo)mnuPreferences.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuPreferences.Click += new EventHandler(mnuPreferencesOnClick);

            // Time codes.
            mnuTimecode.Tag = new ItemResourceInfo(RootResourceManager, "dlgPreferences_LabelTimeFormat");
            mnuTimecode.Text = ((ItemResourceInfo)mnuTimecode.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecode.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            
            #region Timecode menus.
            mnuTimecodeClassic.Tag = new ItemResourceInfo(RootResourceManager, "TimeCodeFormat_Classic");
            mnuTimecodeClassic.Text = ((ItemResourceInfo)mnuTimecodeClassic.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecodeClassic.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTimecodeClassic.Click += new EventHandler(mnuTimecodeClassic_OnClick);
            
            mnuTimecodeFrames.Tag = new ItemResourceInfo(RootResourceManager, "TimeCodeFormat_Frames");
            mnuTimecodeFrames.Text = ((ItemResourceInfo)mnuTimecodeFrames.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecodeFrames.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTimecodeFrames.Click += new EventHandler(mnuTimecodeFrames_OnClick);
            
            mnuTimecodeHoM.Tag = new ItemResourceInfo(RootResourceManager, "TimeCodeFormat_HundredthOfMinutes");
            mnuTimecodeHoM.Text = ((ItemResourceInfo)mnuTimecodeHoM.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecodeHoM.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTimecodeHoM.Click += new EventHandler(mnuTimecodeHoM_OnClick);
            mnuTimecodeHoM.Visible = false;
            
            mnuTimecodeTToH.Tag = new ItemResourceInfo(RootResourceManager, "TimeCodeFormat_TenThousandthOfHours");
            mnuTimecodeTToH.Text = ((ItemResourceInfo)mnuTimecodeTToH.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecodeTToH.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTimecodeTToH.Click += new EventHandler(mnuTimecodeTToH_OnClick);
            mnuTimecodeTToH.Visible = false;
            
            mnuTimecodeTimeAndFrames.Tag = new ItemResourceInfo(RootResourceManager, "TimeCodeFormat_TimeAndFrames");
            mnuTimecodeTimeAndFrames.Text = ((ItemResourceInfo)mnuTimecodeTimeAndFrames.Tag).resManager.GetString(((ItemResourceInfo)mnuTimecodeTimeAndFrames.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTimecodeTimeAndFrames.Click += new EventHandler(mnuTimecodeTimeAndFrames_OnClick);
            
            mnuTimecode.DropDownItems.AddRange(new ToolStripItem[] { mnuTimecodeClassic, mnuTimecodeFrames, mnuTimecodeHoM, mnuTimecodeTToH, mnuTimecodeTimeAndFrames});
            #endregion
            
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] { mnuLanguages, 
                                              						mnuTimecode, 
                                              						new ToolStripSeparator(), 
                                              						mnuPreferences});
                                              						
            #endregion

            #region Help
            // 7.Help
            mnuHelp.Tag = new ItemResourceInfo(RootResourceManager, "mnuHelp");
            mnuHelp.Text = ((ItemResourceInfo)mnuHelp.Tag).resManager.GetString(((ItemResourceInfo)mnuHelp.Tag).strText, Thread.CurrentThread.CurrentUICulture);

            // Manual
            mnuHelpContents.Tag = new ItemResourceInfo(RootResourceManager, "mnuHelpContents");
            mnuHelpContents.Text = ((ItemResourceInfo)mnuHelpContents.Tag).resManager.GetString(((ItemResourceInfo)mnuHelpContents.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuHelpContents.ShortcutKeys = System.Windows.Forms.Keys.F1;
            mnuHelpContents.Click += new EventHandler(mnuHelpContents_OnClick);

            // Video Tutorial
            mnuTutorialVideos.Tag = new ItemResourceInfo(RootResourceManager, "mnuTutorialVideos");
            mnuTutorialVideos.Text = ((ItemResourceInfo)mnuTutorialVideos.Tag).resManager.GetString(((ItemResourceInfo)mnuTutorialVideos.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuTutorialVideos.Click += new EventHandler(mnuTutorialVideos_OnClick);

            // Logs Folder
            mnuApplicationFolder.Tag = new ItemResourceInfo(RootResourceManager, "mnuApplicationFolder");
            mnuApplicationFolder.Text = ((ItemResourceInfo)mnuApplicationFolder.Tag).resManager.GetString(((ItemResourceInfo)mnuApplicationFolder.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuApplicationFolder.Click += new EventHandler(mnuApplicationFolder_OnClick);

            // About
            mnuAbout.Tag = new ItemResourceInfo(RootResourceManager, "mnuAbout");
            mnuAbout.Text = ((ItemResourceInfo)mnuAbout.Tag).resManager.GetString(((ItemResourceInfo)mnuAbout.Tag).strText, Thread.CurrentThread.CurrentUICulture);
            mnuAbout.Click += new EventHandler(mnuAbout_OnClick);

            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { 
												mnuHelpContents, 
												mnuTutorialVideos, 
												new ToolStripSeparator(), 
												mnuApplicationFolder, 
												new ToolStripSeparator(),
												mnuAbout });
            #endregion

            // Top level merge.
            MenuStrip ThisMenuStrip = new MenuStrip();
            ThisMenuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuEdit, mnuView, mnuImage, mnuMotion, mnuOptions, mnuHelp });
            ThisMenuStrip.AllowMerge = true;

            ToolStripManager.Merge(ThisMenuStrip, _menu);
        }
        private void GetSubModulesMenus(ToolStrip _menu)
        {
            FileBrowser.ExtendMenu(_menu);
            Updater.ExtendMenu(_menu);
            ScreenManager.ExtendMenu(_menu);
        }
        private void GetSubModulesToolBars(ToolStrip _toolbar)
        {
            FileBrowser.ExtendToolBar(_toolbar);
            Updater.ExtendToolBar(_toolbar);
            ScreenManager.ExtendToolBar(_toolbar);
        }
        private void RefreshSubMenu(ToolStripItem item)
        {
            //---------------------------------------------------
            // [Recursive] - Refresh this menu and all sub menus.
            //---------------------------------------------------

            // Sub items
            if (item is ToolStripMenuItem)
            {
                foreach (ToolStripItem subItem in ((ToolStripMenuItem)item).DropDownItems)
                {
                    RefreshSubMenu(subItem);
                }
            }
            
            // This item
            ItemResourceInfo info = item.Tag as ItemResourceInfo;
            if (info != null)
            {
                item.Text = info.resManager.GetString(info.strText, Thread.CurrentThread.CurrentUICulture);
            }
        }
        #endregion

        #region Menus Event Handlers

        #region File
        private void mnuOpenFileOnClick(object sender, EventArgs e)
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
            {
                dp.StopPlaying();
            }

            string filePath = LaunchOpenFileDialog();
            if (filePath.Length > 0)
            {
                OpenFileFromPath(filePath);
            }
        }
        #region History sub menus
        private void mnuHistoryVideo1OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(0));
        }
        private void mnuHistoryVideo2OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(1));
        }
        private void mnuHistoryVideo3OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(2));
        }
        private void mnuHistoryVideo4OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(3));
        }
        private void mnuHistoryVideo5OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(4));
        }
        private void mnuHistoryVideo6OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(5));
        }
        private void mnuHistoryVideo7OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(6));
        }
        private void mnuHistoryVideo8OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(7));
        }
        private void mnuHistoryVideo9OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(8));
        }
        private void mnuHistoryVideo10OnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            OpenFileFromPath(pm.GetFilePathAtIndex(9));
        }
        #endregion
        private void mnuHistoryResetOnClick(object sender, EventArgs e)
        {
            PreferencesManager pm = PreferencesManager.Instance();
            pm.HistoryReset();
            pm.OrganizeHistoryMenu();
        }
        private void menuQuitOnClick(object sender, EventArgs e)
        {
            //Environment.Exit(1);
            Application.Exit();
        }
        #endregion

        #region Edit
        private void menuUndoOnClick(object sender, EventArgs e)
        {
            CommandManager cm = CommandManager.Instance();
            cm.Undo();
        }
        private void menuRedoOnClick(object sender, EventArgs e)
        {
            CommandManager cm = CommandManager.Instance();
            cm.Redo();
        }
        #endregion

        #region View
        private void mnuToggleFileExplorerOnClick(object sender, EventArgs e)
        {
            if (MainWindow.SupervisorControl.IsExplorerCollapsed)
            {
                MainWindow.SupervisorControl.ExpandExplorer(true);
            }
            else
            {
                MainWindow.SupervisorControl.CollapseExplorer();
            }
        }
        #endregion

        #region Options
        private void menuSpanishOnClick(object sender, EventArgs e)
        {
            SwitchCulture("es");
        }
        private void menuEnglishOnClick(object sender, EventArgs e)
        {
            SwitchCulture("en");
        }
        private void menuFrenchOnClick(object sender, EventArgs e)
        {
            SwitchCulture("fr");
        }
        private void menuDutchOnClick(object sender, EventArgs e)
        {
            SwitchCulture("nl");
        }
        private void menuGermanOnClick(object sender, EventArgs e)
        {
            SwitchCulture("de");
        }
        private void menuItalianOnClick(object sender, EventArgs e)
        {
            SwitchCulture("it");
        }
        private void menuPortugueseOnClick(object sender, EventArgs e)
        {
            SwitchCulture("pt");
        }
        private void menuRomanianOnClick(object sender, EventArgs e)
        {
            SwitchCulture("ro");
        }
        private void menuPolishOnClick(object sender, EventArgs e)
        {
            SwitchCulture("pl");
        }
        private void menuFinnishOnClick(object sender, EventArgs e)
        {
            SwitchCulture("fi");
        }
        private void menuNorwegianOnClick(object sender, EventArgs e)
        {
            SwitchCulture("no");
        }
        private void menuTurkishOnClick(object sender, EventArgs e)
        {
            SwitchCulture("tr");
        }
        private void menuChineseOnClick(object sender, EventArgs e)
        {
            SwitchCulture("zh-CHS");
        }
        private void menuGreekOnClick(object sender, EventArgs e)
        {
            SwitchCulture("el");
        }
        private void SwitchCulture(string name)
        {
            IUndoableCommand command = new CommandSwitchUICulture(this, Thread.CurrentThread, new CultureInfo(name), Thread.CurrentThread.CurrentUICulture);
            CommandManager cm = CommandManager.Instance();
            cm.LaunchUndoableCommand(command);
        }
        private void CheckLanguageMenu()
        {
            mnuDutch.Checked = false;
            mnuGreek.Checked = false;
            mnuEnglish.Checked = false;
            mnuFrench.Checked = false;
            mnuGerman.Checked = false;
            mnuItalian.Checked = false;
            mnuPolish.Checked = false;
            mnuSpanish.Checked = false;
            mnuPortuguese.Checked = false;
            mnuRomanian.Checked = false;
            mnuFinnish.Checked = false;
            mnuNorwegian.Checked = false;
			mnuTurkish.Checked = false;
            mnuChinese.Checked = false;
			
            CultureInfo ci = PreferencesManager.Instance().GetSupportedCulture();
            
            string cultureName = ci.Name;
            if(!ci.IsNeutralCulture)
        	{
        		cultureName = ci.Parent.Name;
        	}
            
            switch (cultureName)
            {
                case "es":
                    mnuSpanish.Checked = true;
                    break;
                case "de":
                    mnuGerman.Checked = true;
                    break;
                case "fr":
                    mnuFrench.Checked = true;
                    break;
                case "nl":
                    mnuDutch.Checked = true;
                    break;
                case "pt":
                    mnuPortuguese.Checked = true;
                    break;
                case "pl":
                    mnuPolish.Checked = true;
                    break;
                case "it":
                    mnuItalian.Checked = true;
                    break;
                case "ro":
                    mnuRomanian.Checked = true;
                    break;
                case "fi":
                    mnuFinnish.Checked = true;
                    break;
                case "no":
                    mnuNorwegian.Checked = true;
                    break;
				case "tr":
                    mnuTurkish.Checked = true;
                    break;
                case "zh-CHS":
                    mnuChinese.Checked = true;
                    break;
                case "el":
                    mnuGreek.Checked = true;
                    break;
                case "en":
                default:
                    mnuEnglish.Checked = true;
                    break;
            }
        }
        private void mnuPreferencesOnClick(object sender, EventArgs e)
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
            {
                dp.StopPlaying();
            }

            formPreferences fp = new formPreferences();
            fp.ShowDialog();
            fp.Dispose();

            // Refresh Preferences
            PreferencesManager pm = PreferencesManager.Instance();
            log.Debug("Setting current ui culture.");
            Thread.CurrentThread.CurrentUICulture = pm.GetSupportedCulture();
            RefreshUICulture();
        }
        private void CheckTimecodeMenu()
        {
        	mnuTimecodeClassic.Checked = false;
        	mnuTimecodeFrames.Checked = false;
        	mnuTimecodeHoM.Checked = false;
        	mnuTimecodeTToH.Checked = false;
        	mnuTimecodeTimeAndFrames.Checked = false;
        	
            TimeCodeFormat tf = PreferencesManager.Instance().TimeCodeFormat;
            
            switch (tf)
            {
                case TimeCodeFormat.Frames:
                    mnuTimecodeFrames.Checked = true;
                    break;
                case TimeCodeFormat.HundredthOfMinutes:
                    mnuTimecodeHoM.Checked = true;
                    break;
                case TimeCodeFormat.TenThousandthOfHours:
                    mnuTimecodeTToH.Checked = true;
                    break;
                case TimeCodeFormat.TimeAndFrames:
                    mnuTimecodeTimeAndFrames.Checked = true;
                    break; 
                case TimeCodeFormat.ClassicTime:
                default:
                    mnuTimecodeClassic.Checked = true;
                    break;
            }
        }
        private void mnuTimecodeClassic_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimeCodeFormat.ClassicTime);
        }
        private void mnuTimecodeFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimeCodeFormat.Frames);
        }
        private void mnuTimecodeHoM_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimeCodeFormat.HundredthOfMinutes);
        }
        private void mnuTimecodeTToH_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimeCodeFormat.TenThousandthOfHours);
        }
        private void mnuTimecodeTimeAndFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimeCodeFormat.TimeAndFrames);
        }
        private void SwitchTimecode(TimeCodeFormat _timecode)
        {
        	// Todo: turn this into a command ?
        	PreferencesManager pm = PreferencesManager.Instance();
            pm.TimeCodeFormat = _timecode;
            RefreshUICulture();
            pm.Export();	
        }
        #endregion

        #region Help
        private void mnuHelpContents_OnClick(object sender, EventArgs e)
        {
            // Launch Help file from current UI language.

            ResourceManager SharedResources = PreferencesManager.ResourceManager;
            HelpIndex hiLocal = new HelpIndex(Application.StartupPath + "\\" + SharedResources.GetString("URILocalHelpIndex"));

            if (hiLocal.LoadSuccess)
            {
                string LocaleHelpUri = "";
                string EnglishHelpUri = "";
                bool bLocaleFound = false;
                bool bEnglishFound = false;
                int i = 0;

                // Look for a matching locale.
                while (!bLocaleFound && i < hiLocal.UserGuides.Count)
                {
                	if (hiLocal.UserGuides[i].Language == PreferencesManager.Instance().GetSupportedCulture().Name)
                    {
                        bLocaleFound = true;
                        LocaleHelpUri = hiLocal.UserGuides[i].FileLocation;
                    }

                    if (hiLocal.UserGuides[i].Language == "en")
                    {
                        bEnglishFound = true;
                        EnglishHelpUri = hiLocal.UserGuides[i].FileLocation;
                    }

                    i++;
                }

                if (bLocaleFound)
                {
                    Help.ShowHelp(MainWindow, LocaleHelpUri);
                }
                else if (bEnglishFound)
                {
                    Help.ShowHelp(MainWindow, EnglishHelpUri);
                }
                else
                {
                    log.Error("Cannot find any help file.");
                }
            }
            else
            {
                log.Error("Cannot find the xml help index.");
            }
        }
        private void mnuTutorialVideos_OnClick(object sender, EventArgs e)
        {
            ResourceManager SharedResources = PreferencesManager.ResourceManager;
            HelpIndex hiLocal = new HelpIndex(Application.StartupPath + "\\" + SharedResources.GetString("URILocalHelpIndex"));

            if (hiLocal.LoadSuccess)
            {
                HelpVideosDialog hvd = new HelpVideosDialog(RootResourceManager, hiLocal, ScreenManager);
                hvd.ShowDialog();
                hvd.Dispose();
            }
            else
            {
                log.Error("Cannot find the xml help index.");
            }

        }
        private void mnuApplicationFolder_OnClick(object sender, EventArgs e)
        {
            // Launch Windows Explorer on App folder.
			Process.Start(  "explorer.exe", 
                          	 Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Kinovea\\");
        }
        private void mnuAbout_OnClick(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
            fa.Dispose();
        }
        #endregion

        #endregion        
        
        #region Lower Level Methods
        private void OpenFileFromPath(string _FilePath)
        {
        	if (File.Exists(_FilePath))
            {
                //--------------------------------------------------------------------------
                // CommandLoadMovieInScreen est une commande du ScreenManager.
                // elle gère la création du screen si besoin, et demande 
                // si on veut charger surplace ou dans un nouveau en fonction de l'existant.
                //--------------------------------------------------------------------------
                IUndoableCommand clmis = new CommandLoadMovieInScreen(ScreenManager, _FilePath, -1, true);
                CommandManager cm = CommandManager.Instance();
                cm.LaunchUndoableCommand(clmis);

                //-------------------------------------------------------------
                // Get the video ready to play (normalement inutile ici, car on
                // l'a déjà fait dans le LoadMovieInScreen.
                //-------------------------------------------------------------
                ICommand css = new CommandShowScreens(ScreenManager);
                CommandManager.LaunchCommand(css);
            }
            else
            {
        		MessageBox.Show(ScreenManager.resManager.GetString("LoadMovie_FileNotOpened", Thread.CurrentThread.CurrentUICulture),
                                    ScreenManager.resManager.GetString("LoadMovie_Error", Thread.CurrentThread.CurrentUICulture),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
            
            }
        }
        public void DoUpdateStatusBar(string _status)
        {
            //------------------------------------------------------------------------
            // Mettre à jour la status bar avec les données du screenmanager
            // Fonction appelée via la mécanique des delegates depuis le screenmanager 
            //------------------------------------------------------------------------
            stLabel.Text = _status;
        }
        public string LaunchOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = RootResourceManager.GetString("dlgOpenFile_Title", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = RootResourceManager.GetString("dlgOpenFile_Filter", Thread.CurrentThread.CurrentUICulture);
            openFileDialog.FilterIndex = 1;
            string filePath = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            return filePath;
        }
        public void DoMakeTopMost(Form _form)
        {
            // Fonction utilisée pour les minibox de commentaires.
            // Pour qu'elles ne soient pas modales, quand même globales, etc.
            _form.Owner = MainWindow;
        }
        private void PrintInitialConf()
        {
        	CommandLineArgumentManager am = CommandLineArgumentManager.Instance();
        	
            log.Debug("Initial configuration:");
            log.Debug("InputFile : " + am.InputFile);
            log.Debug("SpeedPercentage : " + am.SpeedPercentage.ToString());
            log.Debug("StretchImage : " + am.StretchImage.ToString());
            log.Debug("HideExplorer : " + am.HideExplorer.ToString());
        }
        #endregion

    }
}
