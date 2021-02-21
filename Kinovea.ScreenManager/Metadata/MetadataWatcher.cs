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
    /// Watches a KVA file for updates from outside and raises an event.
    /// This is a thin wrapper around FileSystemWatcher and re-raise the FileSystemEvent.
    /// Important: the event handler will run in the FileSystemWatcher thread.
    /// </summary>
    public class MetadataWatcher : IDisposable
    {
        public bool IsEnabled { get; private set; } = false;
        public event FileSystemEventHandler Changed;

        private FileSystemWatcher watcher;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MetadataWatcher()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~MetadataWatcher()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Close();
        }

        public void Start(string path)
        {
            if (!File.Exists(path))
                return;
            
            string watchedDir = Path.GetDirectoryName(path);
            string watchedFile = Path.GetFileName(path);
            if (watcher != null)
            {
                if (watcher.Path == watchedDir && watcher.Filter == watchedFile)
                    return;

                Close();
            }

            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(path);
            watcher.Filter = Path.GetFileName(path);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = false;
            watcher.Changed += watcher_Changed;
            watcher.EnableRaisingEvents = true;

            IsEnabled = true;
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Changed != null)
                Changed(sender, e);
        }

        public void Close()
        {
            if (watcher == null)
                return;

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= watcher_Changed;
            watcher.Dispose();
            watcher = null;

            IsEnabled = false;
        }
    }
}
