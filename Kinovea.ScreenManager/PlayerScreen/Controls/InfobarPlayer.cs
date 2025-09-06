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
        private string watchedFolder;
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

            SetContextMenu();
        }

        public void UpdateReplayWatcher(string watchedFolderPath)
        {
            this.watchedFolder = watchedFolderPath;
            SetContextMenu();
        }

        private void SetContextMenu()
        { 
            // We come here whenever the current file changes or a switch between replay watcher or normal video.
            popMenu.Items.Clear();

            isReplayWatcher = screenDescriptor.IsReplayWatcher;
            
            btnVideoType.BackgroundImage = isReplayWatcher ? Properties.Resources.replaywatcher : Properties.Resources.film_small;

            if (isReplayWatcher)
            {
                string toolTipText = string.Format(ScreenManagerLang.Infobar_Player_Observing, watchedFolder);
                toolTips.SetToolTip(btnVideoType, toolTipText);
                toolTips.SetToolTip(lblFilename, toolTipText);

                mnuStopWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StopWatcher, watchedFolder);
                popMenu.Items.Add(mnuStopWatcher);
                
                if (!string.IsNullOrEmpty(parentFolder) && parentFolder != watchedFolder)
                {
                    mnuStartWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, parentFolder);
                    popMenu.Items.Add(mnuStartWatcher);
                }
            }
            else
            {
                toolTips.SetToolTip(btnVideoType, null);
                toolTips.SetToolTip(lblFilename, null);

                if (parentFolder != null)
                {
                    mnuStartWatcher.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, parentFolder);
                    popMenu.Items.Add(mnuStartWatcher);
                }
            }

            // Add menus for the capture folders.
            List<CaptureFolder> ccff = PreferencesManager.CapturePreferences.CapturePathConfiguration.CaptureFolders;
            if (ccff.Count == 0)
                return;

            popMenu.Items.Add(new ToolStripSeparator());
            
            foreach (var cf in ccff)
            {
                CaptureFolder captureFolder = cf;
                ToolStripMenuItem mnuCaptureFolder = new ToolStripMenuItem();
                mnuCaptureFolder.Image = Properties.Resources.camera_video;
                
                // Note: instead of hiding the corresponding menu we just check it.
                // This provides feedback of which capture folder is being watched.
                if (screenDescriptor != null && screenDescriptor.IsReplayWatcher && screenDescriptor.FullPath == captureFolder.Id.ToString())
                {
                    mnuCaptureFolder.Text = string.Format("Observing folder: {0}", captureFolder.FriendlyName);
                    mnuCaptureFolder.Checked = true;
                }
                else
                {
                    mnuCaptureFolder.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, captureFolder.FriendlyName);
                    mnuCaptureFolder.Click += (s, e) => StartWatcherAsked?.Invoke(s, new EventArgs<CaptureFolder>(captureFolder));
                }

                popMenu.Items.Add(mnuCaptureFolder);
            }
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
