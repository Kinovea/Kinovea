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
        private string currentFile;
        private FileSystemWatcher watcher;
        private Control dummy = new Control();
        private int overwriteEventCount;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public void Start(ScreenDescriptionPlayback sdp, string currentFile)
        {
            // We'll pass here upon initialization and also everytime the player is reloaded with a video.
            // Verifiy the file watcher is on the right directory.
            double oldSpeed = screenDescription == null ? 0 : screenDescription.SpeedPercentage;
            this.screenDescription = sdp;
            this.currentFile = currentFile;

            string watchedDir = Path.GetDirectoryName(sdp.FullPath);

            if (watcher != null)
            {
                if (watcher.Path == watchedDir && oldSpeed == screenDescription.SpeedPercentage)
                    return;

                Close();
            }

            if (!Directory.Exists(watchedDir))
                return;

            watcher = new FileSystemWatcher(watchedDir);

            overwriteEventCount = 0;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
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

            if (e.FullPath == currentFile)
            {
                // In this case we can't use the normal heuristic of trying to get exclusive access on the file,
                // because the player screen itself already has the file opened.

                // First of all we need to stop the player from playing the file as it's reading frames from disk (no caching).
                dummy.BeginInvoke((Action)delegate { player.StopPlaying(); });
                
                // We normally receive only two events. One at start and one on close.
                overwriteEventCount++;
                if (overwriteEventCount >= 2)
                {
                    overwriteEventCount = 0;
                    dummy.BeginInvoke((Action)delegate { LoadVideo(e.FullPath); });
                }
            }
            else
            {
                if (FilesystemHelper.CanRead(e.FullPath))
                    dummy.BeginInvoke((Action)delegate { LoadVideo(e.FullPath); });
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
