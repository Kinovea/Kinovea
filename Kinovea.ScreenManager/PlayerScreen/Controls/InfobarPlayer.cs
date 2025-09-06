using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class InfobarPlayer : UserControl
    {
        public event EventHandler StopWatcherAsked;
        public event EventHandler<EventArgs<CaptureFolder>> StartWatcherAsked;

        public ScreenDescriptorPlayback ScreenDescriptor
        {
            get { return screenDescriptor; }
            set 
            { 
                screenDescriptor = value; 
            }
        }

        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuStopWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStartWatcher = new ToolStripMenuItem();
        private bool isReplayWatcher;
        private string parentFolder;
        private ScreenDescriptorPlayback screenDescriptor = null;

        public InfobarPlayer()
        {
            InitializeComponent();
            BuildContextMenus();
        }

        /// <summary>
        /// Update the infobar with info about the actual video being played.
        /// For replay watcher this is more specific than what's in the screen descriptor.
        /// </summary>
        public void UpdateValues(string path, string size, string fps, IVideoFilter videoFilter)
        {
            if (screenDescriptor == null)
                return;
            
            lblFilename.Text = Path.GetFileNameWithoutExtension(path);
            lblSize.Text = size;
            lblFps.Text = fps;

            VideoFilterType filterType = videoFilter == null ? VideoFilterType.None : videoFilter.Type;
            btnMode.Image = VideoFilterFactory.GetIcon(filterType);
            lblMode.Text = VideoFilterFactory.GetFriendlyName(filterType);

            if (!string.IsNullOrEmpty(path))
                parentFolder = Path.GetDirectoryName(path);
            else
                parentFolder = null;

            isReplayWatcher = screenDescriptor.IsReplayWatcher;

            SetContextMenu(path);
        }

        public void UpdateReplayWatcher(string watchedFolderPath)
        {
            SetContextMenu(watchedFolderPath);
        }

        /// <summary>
        /// Update the tooltip and the context menu.
        /// `path` is the real file system path of the file or watched folder.
        /// </summary>
        private void SetContextMenu(string path)
        { 
            // We come here whenever the current file changes or a switch between replay watcher or normal video.
            popMenu.Items.Clear();

            isReplayWatcher = screenDescriptor.IsReplayWatcher;
            
            btnVideoType.BackgroundImage = isReplayWatcher ? Properties.Resources.replaywatcher : Properties.Resources.film_small;

            if (isReplayWatcher)
            {
                string shortName = "";
                var cf = FilesystemHelper.GetCaptureFolder(screenDescriptor.FullPath);
                if (cf != null)
                    shortName = cf.ShortName;

                string infoName = path;
                if (!string.IsNullOrEmpty(shortName))
                {
                    infoName = string.Format("{0} ({1})", shortName, path);
                }

                string toolTipText = string.Format(ScreenManagerLang.Infobar_Player_Observing, infoName);
                toolTips.SetToolTip(btnVideoType, toolTipText);
                toolTips.SetToolTip(lblFilename, toolTipText);
                mnuStopWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StopWatcher, infoName);
                popMenu.Items.Add(mnuStopWatcher);

                // Note: we no longer allow starting a watcher on the current folder from here.
                // This was super confusing because it wouldn't reverse-resolve to the capture folder.
                // Instead it would start observing on the static path of the capture folder, not the 
                // dynamic path, and worse, it would add the static path as a new capture folder.
                // It's still possible to start a watcher on a random folder but only from the "Open folder"
                // dialog or from the navigation pane file lister.
                //if (!string.IsNullOrEmpty(parentFolder) && parentFolder != watchedFolder)
                //{
                //    mnuStartWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, parentFolder);
                //    popMenu.Items.Add(mnuStartWatcher);
                //}
            }
            else
            {
                toolTips.SetToolTip(btnVideoType, path);
                toolTips.SetToolTip(lblFilename, path);

                // Note: we no longer allow starting a watcher on the current folder from here.
                // See note above.
                //if (parentFolder != null)
                //{
                //    mnuStartWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, parentFolder);
                //    popMenu.Items.Add(mnuStartWatcher);
                //}
            }

            List<CaptureFolder> ccff = PreferencesManager.CapturePreferences.CapturePathConfiguration.CaptureFolders;
            if (ccff.Count == 0)
            {
                AddConfigureCaptureFoldersMenu();
                return;
            }

            if (popMenu.Items.Count > 0)
            {
                popMenu.Items.Add(new ToolStripSeparator());
            }

            // Add entries for the capture folders.
            foreach (var cf in ccff)
            {
                CaptureFolder captureFolder = cf;
                ToolStripMenuItem mnuCaptureFolder = new ToolStripMenuItem();
                mnuCaptureFolder.Image = Properties.Resources.camera_video;
                
                // Hide the current capture folder, we already added its short name to the "stop watching" menu above,
                // this gives feedback about the current watched folder.
                if (screenDescriptor != null && screenDescriptor.IsReplayWatcher && 
                    screenDescriptor.FullPath == captureFolder.Id.ToString())
                {
                    continue;
                }
                
                mnuCaptureFolder.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, captureFolder.FriendlyName);
                mnuCaptureFolder.Click += (s, e) => StartWatcherAsked?.Invoke(s, new EventArgs<CaptureFolder>(captureFolder));
                
                popMenu.Items.Add(mnuCaptureFolder);
            }

            AddConfigureCaptureFoldersMenu();
        }

        private void AddConfigureCaptureFoldersMenu()
        {
            ToolStripMenuItem mnuConfigureCaptureFolders = new ToolStripMenuItem();
            mnuConfigureCaptureFolders.Image = Properties.Capture.explorer_video;
            mnuConfigureCaptureFolders.Text = "Configure capture folders";

            mnuConfigureCaptureFolders.Click += (s, e) => {
                NotificationCenter.RaisePreferenceTabAsked(this, PreferenceTab.Capture_Paths);
            };

            popMenu.Items.Add(new ToolStripSeparator());
            popMenu.Items.Add(mnuConfigureCaptureFolders);
        }

        private void BuildContextMenus()
        {
            mnuStopWatcher.Image = Properties.Resources.film_small;
            mnuStopWatcher.Click += (s, e) => StopWatcherAsked?.Invoke(s, e);

            mnuStartWatcher.Image = Properties.Resources.replaywatcher;
            mnuStartWatcher.Click += (s, e) => StartWatcherAsked?.Invoke(s, new EventArgs<CaptureFolder>(null));
        }

        private void fileinfo_MouseDown(object sender, MouseEventArgs e)
        {
            ShowContextMenu();
        }

        private void ShowContextMenu()
        {
            popMenu.Show(btnVideoType, new Point(0, btnVideoType.Height));
        }
    }
}
