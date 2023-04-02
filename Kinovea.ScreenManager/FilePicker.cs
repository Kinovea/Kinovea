using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    ///  Helper function to launch open file dialogs for video, replay watchers, annotations.
    /// </summary>
    public static class FilePicker
    {
        public static string OpenVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.mnuOpenVideo;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = ScreenManagerLang.FileFilter_All + "|*.*";
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

            path = Path.Combine(path, "*");
            return path;
        }

        public static string OpenAnnotations()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgLoadAnalysis_Title;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = FilesystemHelper.OpenKVAFilter(ScreenManagerLang.FileFilter_AllSupported);
            openFileDialog.FilterIndex = 1;

            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK || string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }
    }
}
