using Kinovea.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace Kinovea.Services
{
    /// <summary>
    ///  Helper function to launch open file dialogs for video, replay watchers, annotations.
    /// </summary>
    public static class FilePicker
    {
        public static string OpenVideo(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title; 
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;
            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }

        public static string OpenReplayWatcher()
        {
            string initialDirectory = null;
            string lastReplayFolder = PreferencesManager.FileExplorerPreferences.LastReplayFolder;
            if (!string.IsNullOrEmpty(lastReplayFolder))
            {
                lastReplayFolder = Path.GetDirectoryName(lastReplayFolder);
                if (Directory.Exists(lastReplayFolder))
                    initialDirectory = lastReplayFolder;
            }
            else
            {
                initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            string path = FilesystemHelper.OpenFolderBrowserDialog(initialDirectory);
            if (path == null || !Directory.Exists(path))
                return null;

            return path;
        }

        public static string OpenAnnotations(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;

            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }
    }
}
