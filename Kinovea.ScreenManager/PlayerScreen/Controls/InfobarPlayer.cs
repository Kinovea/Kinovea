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

        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuStopWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStartWatcher = new ToolStripMenuItem();
        private bool replayWatcher;
        private string watchedFolder;
        private string parentFolder;

        public InfobarPlayer()
        {
            InitializeComponent();
            BuildContextMenus();
        }

        public void UpdateValues(string path, string size, string fps, IVideoFilter videoFilter)
        {
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

            SetContextMenu();
        }

        public void UpdateReplayWatcher(bool replayWatcher, string path)
        {
            this.replayWatcher = replayWatcher;
            this.watchedFolder = path;
            btnVideoType.BackgroundImage = replayWatcher ? Properties.Resources.replaywatcher : Properties.Resources.film_small;
            
            SetContextMenu();
        }

        private void SetContextMenu()
        { 
            // We come here whenever the current file changes or a switch between replay watcher or normal video.
            popMenu.Items.Clear();
            if (replayWatcher)
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
                mnuCaptureFolder.Text = string.Format(ScreenManagerLang.Infobar_Player_StartWatcher, captureFolder.FriendlyName); 
                mnuCaptureFolder.Image = Properties.Resources.camera_video;
                mnuCaptureFolder.Click += (s, e) => StartWatcherAsked?.Invoke(s, new EventArgs<CaptureFolder>(captureFolder));
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
