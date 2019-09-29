using Kinovea.Services;
using Kinovea.Video;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Watches a directory for new files and load them in the parent player.
    /// </summary>
    public class ReplayWatcher : IDisposable
    {
        private PlayerScreen player;
        private ScreenDescriptionPlayback screenDescription;
        private FileSystemWatcher watcher;
        private Control dummy = new Control();

        public ReplayWatcher(PlayerScreen player)
        {
            this.player = player;
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ReplayWatcher()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                dummy.Dispose();
            }
        }

        public void Start(ScreenDescriptionPlayback sdp)
        {
            // We'll pass here upon initialization and also everytime the player is reloaded with a video.
            // Verifiy the file watcher is on the right directory.

            double oldSpeed = screenDescription == null ? 0 : screenDescription.SpeedPercentage;
            this.screenDescription = sdp;

            string path = Path.GetDirectoryName(sdp.FullPath);

            if (watcher != null)
            {
                if (watcher.Path == path && oldSpeed == screenDescription.SpeedPercentage)
                    return;

                Close();
            }

            if (!Directory.Exists(path))
                return;

            watcher = new FileSystemWatcher(path);

            watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = false;
            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!VideoTypeManager.IsSupported(Path.GetExtension(e.FullPath)))
                return;
            
            if (FilesystemHelper.CanRead(e.FullPath))
            {
                if (dummy.InvokeRequired)
                    dummy.BeginInvoke((Action)delegate { LoadVideo(e.FullPath); });
                else
                    LoadVideo(e.FullPath);
            }
        }

        public void Close()
        {
            if (watcher == null)
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= watcher_Changed;
            watcher.Created -= watcher_Changed;
            watcher.Dispose();
            watcher = null;
        }

        private void LoadVideo(string path)
        {
            // Update the descriptor with the speed from the UI.
            screenDescription.SpeedPercentage = player.view.RealtimePercentage;

            // Load the video in the player.
            // We send the actual file name in path, at this point the player doesn't need to know this is coming from a watched directory.
            LoaderVideo.LoadVideo(player, path, screenDescription);
            
            if (player.FrameServer.Loaded)
            {
                NotificationCenter.RaiseFileOpened(null, path);
                PreferencesManager.FileExplorerPreferences.AddRecentFile(path);
                PreferencesManager.Save();
            }
        }
    }
}
